#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Antlr4.Runtime;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LspRange = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace src;

internal static class Lsp
{
    public static int ServerLoop()
    {
        LanguageServer server = new(Console.OpenStandardInput(), Console.OpenStandardOutput());
        return server.Run();
    }
}

internal sealed class LanguageServer
{
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly Dictionary<string, DocumentState> _documents = new(StringComparer.OrdinalIgnoreCase);
    private readonly JsonSerializer _serializer;
    private readonly object _writeLock = new();

    private bool _shutdownRequested;
    private bool _exitNotificationReceived;
    private int _exitCode;

    private static readonly Dictionary<string, BuiltInSymbol> BuiltInSymbols = CreateBuiltInSymbols();
    private static readonly object LogLock = new();
    private static readonly string LogFilePath = DetermineLogFilePath();

    private readonly List<byte> _byteBuffer = new();
    private byte[] _contentBuffer = new byte[1024*1024];
    private int _contentBufferSize = 1024*1024;

    public LanguageServer(Stream input, Stream output)
    {
        _input = input;
        _output = output;
        _serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include
        });
    }

    private string? ReadHeader()
    {
        _byteBuffer.Clear();
        while (true)
        {
            var b = _input.ReadByte();
            if (b < 0)
                return null;

            if (b == '\r')
            {
                var b2 = _input.ReadByte();
                if (b2 == '\n')
                {
                    return Encoding.ASCII.GetString(_byteBuffer.ToArray());
                }

                _byteBuffer.Add((byte)b);
            }
            else
            {
                _byteBuffer.Add((byte)b);
            }
        }
    }

    private string ReadContent(int contentLength)
    {
        _byteBuffer.Clear();
        if (contentLength > _contentBufferSize)
        {
            _contentBufferSize *= 2;
            _contentBuffer = new byte[_contentBufferSize];
        }

        int totalRead = 0;
        while (totalRead < contentLength)
        {
            int readCount = _input.Read(_contentBuffer, totalRead, contentLength - totalRead);
            totalRead += readCount;
        }
        string content = Encoding.UTF8.GetString(_contentBuffer,0, contentLength);
        return content;
    }

    public int Run()
    {
        using StreamReader reader = new(_input, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);

        while (true)
        {
            int contentLength = -1;
            while (true)
            {
                string? header = ReadHeader();
                if (header is null) return _exitCode;
                if (header == "") break;
                if (header.ToLower().StartsWith("content-length:"))
                {
                    string length = header["Content-Length:".Length..].Trim();
                    if (!int.TryParse(length, NumberStyles.Integer, CultureInfo.InvariantCulture, out contentLength))
                    {
                        contentLength = -1;
                    }
                }
            }

            if (contentLength < 0)
            {
                continue;
            }

            string payload = ReadContent(contentLength);
            // string? payload = ReadMessage(reader);
            Log(payload ?? "NULL", $"neobem_{DateTime.Now:yyyy-MM-ddThhmmss}_message.json");

            if (payload is null)
            {
                break;
            }

            JObject? message;
            try
            {
                message = JObject.Parse(payload);
            }
            catch (JsonException jsonException)
            {
                // Log(payload, $"neobem_{DateTime.Now:yyyy-mm-ddThhmmss}_message.json");
                SendLogMessage($"Failed to parse JSON RPC payload: {jsonException.Message}", MessageType.Error);
                continue;
            }

            HandleMessage(message);

            if (_exitNotificationReceived)
            {
                break;
            }
        }

        return _exitCode;
    }

    private void HandleMessage(JObject message)
    {
        message.TryGetValue("id", StringComparison.Ordinal, out JToken? idToken);
        message.TryGetValue("method", StringComparison.Ordinal, out JToken? methodToken);
        string? method = methodToken?.Value<string>();

        if (method is null)
        {
            // Response from client - nothing for us to do today.
            return;
        }

        switch (method)
        {
            case "initialize":
                HandleInitialize(message, idToken);
                break;
            case "initialized":
                break;
            case "$/cancelRequest":
                break;
            case "textDocument/didOpen":
                HandleDidOpen(message);
                break;
            case "textDocument/didChange":
                HandleDidChange(message);
                break;
            case "textDocument/didClose":
                HandleDidClose(message);
                break;
            case "textDocument/hover":
                HandleHover(message, idToken);
                break;
            case "shutdown":
                HandleShutdown(idToken);
                break;
            case "exit":
                HandleExit();
                break;
            default:
                if (idToken is not null)
                {
                    SendMethodNotFound(idToken, method);
                }
                break;
        }
    }

    private void HandleInitialize(JObject message, JToken? idToken)
    {
        InitializeParams? initializeParams = message["params"]?.ToObject<InitializeParams>(_serializer);

        InitializeResult result = new()
        {
            Capabilities = new ServerCapabilities
            {
                TextDocumentSync = new TextDocumentSyncOptions
                {
                    Change = TextDocumentSyncKind.Full,
                    OpenClose = true
                },
                HoverProvider = true
            }
        };

        SendResponse(idToken, result);

        if (initializeParams?.Trace is not null)
        {
            SendLogMessage($"Client trace preference: {initializeParams.Trace}", MessageType.Log);
        }
    }

    private void HandleDidOpen(JObject message)
    {
        DidOpenTextDocumentParams? parameters = message["params"]?.ToObject<DidOpenTextDocumentParams>(_serializer);
        if (parameters?.TextDocument?.Uri is null)
        {
            return;
        }

        string key = parameters.TextDocument.Uri.ToString();
        string initialText = parameters.TextDocument.Text ?? string.Empty;
        FileType fileType = DetermineFileType(parameters.TextDocument.Uri);

        if (_documents.TryGetValue(key, out DocumentState? existing))
        {
            existing.UpdateText(initialText);
        }
        else
        {
            _documents[key] = new DocumentState(initialText, fileType);
        }
    }

    private void HandleDidChange(JObject message)
    {
        DidChangeTextDocumentParams? parameters = message["params"]?.ToObject<DidChangeTextDocumentParams>(_serializer);
        if (parameters?.TextDocument?.Uri is null)
        {
            return;
        }

        string key = parameters.TextDocument.Uri.ToString();

        string? latest = null;
        if (parameters.ContentChanges is not null)
        {
            foreach (TextDocumentContentChangeEvent change in parameters.ContentChanges)
            {
                latest = change.Text;
            }
        }

        if (latest is null)
        {
            return;
        }

        if (_documents.TryGetValue(key, out DocumentState? document))
        {
            document.UpdateText(latest);
        }
        else
        {
            FileType fileType = DetermineFileType(parameters.TextDocument.Uri);
            _documents[key] = new DocumentState(latest, fileType);
        }
    }

    private void HandleDidClose(JObject message)
    {
        DidCloseTextDocumentParams? parameters = message["params"]?.ToObject<DidCloseTextDocumentParams>(_serializer);
        if (parameters?.TextDocument?.Uri is null)
        {
            return;
        }

        string key = parameters.TextDocument.Uri.ToString();
        _documents.Remove(key);
    }

    private void HandleHover(JObject message, JToken? idToken)
    {
        if (idToken is null)
        {
            return;
        }

        TextDocumentPositionParams? parameters = message["params"]?.ToObject<TextDocumentPositionParams>(_serializer);
        if (parameters?.TextDocument?.Uri is null)
        {
            SendResponse(idToken, null);
            return;
        }

        string key = parameters.TextDocument.Uri.ToString();
        if (!_documents.TryGetValue(key, out DocumentState? document))
        {
            SendResponse(idToken, null);
            return;
        }

        IToken? token = document.FindToken(parameters.Position.Line, parameters.Position.Character);
        if (token is null)
        {
            SendResponse(idToken, null);
            return;
        }

        string identifier = token.Text ?? string.Empty;
        Log(identifier);
        if (string.IsNullOrEmpty(identifier) || !BuiltInSymbols.TryGetValue(identifier, out var info))
        {
            SendResponse(idToken, null);
            return;
        }

        int startLine = Math.Max(token.Line - 1, 0);
        int startCharacter = Math.Max(token.Column, 0);
        (int endLine, int endCharacter) = ComputeTokenEndPosition(token, startLine, startCharacter);

        LspRange range = new()
        {
            Start = new Position
            {
                Line = startLine,
                Character = startCharacter
            },
            End = new Position
            {
                Line = endLine,
                Character = endCharacter
            }
        };

        string hoverBody = BuildHoverMarkdown(info);
        Hover hover = new()
        {
            Contents = new SumType<SumType<string, MarkedString>, SumType<string, MarkedString>[], MarkupContent>(
                new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = hoverBody
                }),
            Range = range
        };

        SendResponse(idToken, hover);
    }

    private void HandleShutdown(JToken? idToken)
    {
        _shutdownRequested = true;
        SendResponse(idToken, null);
    }

    private void HandleExit()
    {
        _exitNotificationReceived = true;
        _exitCode = _shutdownRequested ? 0 : 1;
    }

    private void SendResponse(JToken? idToken, object? result)
    {
        JObject response = new()
        {
            ["jsonrpc"] = "2.0",
            ["id"] = idToken?.DeepClone() ?? JValue.CreateNull()
        };

        response["result"] = result is null ? JValue.CreateNull() : JToken.FromObject(result, _serializer);

        WriteMessage(response);
    }

    private void SendMethodNotFound(JToken idToken, string method)
    {
        JObject response = new()
        {
            ["jsonrpc"] = "2.0",
            ["id"] = idToken.DeepClone(),
            ["error"] = new JObject
            {
                ["code"] = -32601,
                ["message"] = $"Method '{method}' is not implemented."
            }
        };

        WriteMessage(response);
    }

    private void SendLogMessage(string message, MessageType type)
    {
        Log($"{type}: {message}");

        LogMessageParams logMessage = new()
        {
            MessageType = type,
            Message = message
        };

        JObject notification = new()
        {
            ["jsonrpc"] = "2.0",
            ["method"] = "window/logMessage",
            ["params"] = JToken.FromObject(logMessage, _serializer)
        };

        WriteMessage(notification);
    }

    private void WriteMessage(JObject message)
    {
        string json = message.ToString(Formatting.None);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        byte[] headerBytes = Encoding.ASCII.GetBytes($"Content-Length: {jsonBytes.Length}\r\n\r\n");

        lock (_writeLock)
        {
            _output.Write(headerBytes, 0, headerBytes.Length);
            _output.Write(jsonBytes, 0, jsonBytes.Length);
            _output.Flush();
        }
    }

    private static void Log(string message)
    {
        try
        {
            string logEntry = $"{DateTime.UtcNow:O} {message}{Environment.NewLine}";
            lock (LogLock)
            {
                File.AppendAllText(LogFilePath, logEntry, Encoding.UTF8);
            }
        }
        catch (Exception)
        {
            // Swallow logging failures to avoid impacting server behavior.
        }
    }

    private static void Log(string message, string filename)
    {
        try
        {
            string logEntry = $"{DateTime.UtcNow:O} {message}{Environment.NewLine}";
            lock (LogLock)
            {
                File.AppendAllText(DetermineLogFilePath(filename), logEntry, Encoding.UTF8);
            }
        }
        catch (Exception)
        {
            // Swallow logging failures to avoid impacting server behavior.
        }
    }

    private static string DetermineLogFilePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string? tempDirectory = Environment.GetEnvironmentVariable("TMP");
            if (string.IsNullOrEmpty(tempDirectory))
            {
                tempDirectory = Path.GetTempPath();
            }

            return Path.Combine(tempDirectory, "neobem_lsp.log");
        }

        return Path.Combine("/tmp", "neobem_lsp.log");
    }

    private static string DetermineLogFilePath(string filename)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string? tempDirectory = Environment.GetEnvironmentVariable("TMP");
            if (string.IsNullOrEmpty(tempDirectory))
            {
                tempDirectory = Path.GetTempPath();
            }

            return Path.Combine(tempDirectory, filename);
        }

        return Path.Combine("/tmp", filename);
    }

    private static string? ReadMessage(StreamReader reader)
    {
        string? line;
        int contentLength = -1;

        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrEmpty(line))
            {
                break;
            }

            if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
            {
                string length = line.Substring("Content-Length:".Length).Trim();
                if (!int.TryParse(length, NumberStyles.Integer, CultureInfo.InvariantCulture, out contentLength))
                {
                    contentLength = -1;
                }
            }
        }

        if (contentLength < 0)
        {
            return null;
        }

        char[] buffer = new char[contentLength];
        int totalRead = 0;

        while (totalRead < contentLength)
        {
            int read = reader.Read(buffer, totalRead, contentLength - totalRead);
            if (read == 0)
            {
                return null;
            }
            totalRead += read;
        }

        return new string(buffer, 0, totalRead);
    }

    private static FileType DetermineFileType(Uri uri)
    {
        string path = uri.IsAbsoluteUri ? uri.LocalPath : uri.ToString();
        string extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".bdl" => FileType.Doe2,
            ".inp" => FileType.Doe2,
            _ => FileType.Idf
        };
    }

    private static string BuildHoverMarkdown(BuiltInSymbol symbol)
    {
        StringBuilder builder = new();
        builder.Append("`");
        builder.Append(symbol.Signature);
        builder.Append("`\n\n");
        builder.Append(symbol.Description);
        return builder.ToString();
    }

    private static (int Line, int Character) ComputeTokenEndPosition(IToken token, int startLine, int startCharacter)
    {
        string text = token.Text ?? string.Empty;
        int line = startLine;
        int character = startCharacter;

        for (int i = 0; i < text.Length; i++)
        {
            char current = text[i];
            if (current == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                line++;
                character = 0;
            }
            else if (current == '\n')
            {
                line++;
                character = 0;
            }
            else
            {
                character++;
            }
        }

        return (line, character);
    }

    private sealed class DocumentState
    {
        private readonly SimpleAntlrErrorListener _lexerErrorListener = new();
        private readonly SimpleAntlrErrorListener _parserErrorListener = new();
        private AntlrInputStream _inputStream;
        private readonly NeobemLexer _lexer;
        private readonly CommonTokenStream _tokenStream;
        private readonly NeobemParser _parser;
        private readonly List<IToken> _tokenCache = new();

        public string Text { get; private set; }
        public NeobemParser.IdfContext? ParseTree { get; private set; }
        public IReadOnlyList<AntlrError> LexerErrors => _lexerErrorListener.Errors;
        public IReadOnlyList<AntlrError> ParserErrors => _parserErrorListener.Errors;
        public Exception? LastParseException { get; private set; }

        public DocumentState(string text, FileType fileType)
        {
            Text = text;
            _inputStream = new AntlrInputStream(text);
            _lexer = new NeobemLexer(_inputStream)
            {
                FileType = fileType
            };
            _lexer.RemoveErrorListeners();
            _lexer.AddErrorListener(_lexerErrorListener);

            _tokenStream = new CommonTokenStream(_lexer);

            _parser = new NeobemParser(_tokenStream);
            _parser.RemoveErrorListeners();
            _parser.AddErrorListener(_parserErrorListener);

            Parse();
        }

        public void UpdateText(string text)
        {
            Log("Updating text");
            Text = text;
            _inputStream = new AntlrInputStream(text);
            _lexer.SetInputStream(_inputStream);
            _tokenStream.SetTokenSource(_lexer);
            _tokenStream.Reset();
            _parser.Reset();
            Parse();
        }

        public IToken? FindToken(int zeroBasedLine, int zeroBasedCharacter)
        {
            if (zeroBasedLine < 0 || zeroBasedCharacter < 0) return null;

            int low = 0;
            int high = _tokenCache.Count - 1;

            int loops = 0;
            while (loops < 1000)
            {
                if (low > high) return null;
                int mid = low + ((high - low) / 2);

                IToken token = _tokenCache[mid];
                int relativePosition = TokenRelativePosition(token, zeroBasedLine, zeroBasedCharacter);

                // Log($"Searching token: {low} {mid} {high}, {zeroBasedLine} {zeroBasedCharacter}, {token.Line} {token.Column} {token.StartIndex} {token.StopIndex} '{token.Text}' {relativePosition}");

                if (relativePosition == 0) return token;
                loops++;
                if (relativePosition < 0)
                {
                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns -1 if the position is before token, 0 in token, or 1 if after.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="zeroBasedStartLine"></param>
        /// <param name="zeroBasedCharacter"></param>
        /// <returns></returns>
        public static int TokenRelativePosition(IToken token, int zeroBasedStartLine, int zeroBasedCharacter)
        {
            if (zeroBasedStartLine < token.Line - 1) return -1;
            if (zeroBasedStartLine > token.Line - 1) return 1;
            if (zeroBasedCharacter < token.Column) return -1;
            if (zeroBasedCharacter > token.Column + (token.StopIndex - token.StartIndex) + 1) return 1;
            return 0;
        }

        private void Parse()
        {
            _lexerErrorListener.Errors.Clear();
            _parserErrorListener.Errors.Clear();
            _tokenStream.Seek(0);
            _parser.Reset();

            try
            {
                ParseTree = _parser.idf();
                LastParseException = null;
            }
            catch (Exception ex)
            {
                Log($"Exception in parsing: {ex.Message}");
                LastParseException = ex;
                ParseTree = null;
            }

            _tokenCache.Clear();
            _tokenStream.Fill();
            int tokenCount = _tokenStream.Size;
            for (int i = 0; i < tokenCount; i++)
            {
                IToken token = _tokenStream.Get(i);
                if (token is null)
                {
                    continue;
                }

                if (token.Type == TokenConstants.EOF)
                {
                    continue;
                }

                if (token.Channel != TokenConstants.DefaultChannel)
                {
                    continue;
                }

                if (token.StartIndex < 0 || token.StopIndex < token.StartIndex)
                {
                    continue;
                }

                _tokenCache.Add(token);
            }
        }

    }

    private static Dictionary<string, BuiltInSymbol> CreateBuiltInSymbols()
    {
        Dictionary<string, BuiltInSymbol> map = new(StringComparer.OrdinalIgnoreCase)
        {
            ["map"] = new("map(list, func)", "Applies `func` to every element in `list` and returns a new list."),
            ["filter"] = new("filter(list, func)", "Returns a list that only contains elements from `list` where `func` returns truthy."),
            ["fold"] = new("fold(list, func, initial)", "Reduces `list` with `func`, starting from `initial`."),
            ["keys"] = new("keys(object)", "Returns the keys of an object as a list of strings."),
            ["has"] = new("has(object, key)", "Returns true when `object` has the provided `key`."),
            ["load"] = new("load(path)", "Loads an external file and returns its deserialized contents."),
            ["head"] = new("head(list)", "Returns the first element of `list`."),
            ["tail"] = new("tail(list)", "Returns all but the first element of `list`."),
            ["init"] = new("init(list)", "Returns all but the last element of `list`."),
            ["last"] = new("last(list)", "Returns the final element of `list`."),
            ["index"] = new("index(list, position)", "Returns the element at `position` in `list`. Negative indices count from the end."),
            ["length"] = new("length(list)", "Returns the number of elements in `list`."),
            ["join"] = new("join(list, separator)", "Concatenates string elements in `list` separated by `separator`."),
            ["replace"] = new("replace(text, search, replacement)", "Replaces occurrences of `search` in `text` with `replacement`."),
            ["mod"] = new("mod(dividend, divisor)", "Returns the remainder of dividing `dividend` by `divisor`."),
            ["type"] = new("type(value)", "Returns the Neobem type name of `value`."),
            ["guid"] = new("guid()", "Generates a random GUID string."),
            ["exists"] = new("exists(path)", "Checks whether a file exists at `path`."),
            ["handle"] = new("handle(object, field)", "Returns a handle value for an object reference field."),
            ["contains"] = new("contains(listOrString, value)", "Returns true when `value` appears in the list or string."),
            ["lower"] = new("lower(text)", "Converts `text` to lowercase."),
            ["upper"] = new("upper(text)", "Converts `text` to uppercase."),
            ["ln"] = new("ln(number)", "Natural logarithm of `number`."),
            ["log10"] = new("log10(number)", "Base-10 logarithm of `number`."),
            ["log2"] = new("log2(number)", "Base-2 logarithm of `number`."),
            ["abs"] = new("abs(number)", "Absolute value of `number`."),
            ["acos"] = new("acos(number)", "Arc cosine of `number` in radians."),
            ["asin"] = new("asin(number)", "Arc sine of `number` in radians."),
            ["atan2"] = new("atan2(y, x)", "Arc tangent of `y / x` in radians."),
            ["ceiling"] = new("ceiling(number)", "Smallest integer greater than or equal to `number`."),
            ["cos"] = new("cos(number)", "Cosine of `number` in radians."),
            ["floor"] = new("floor(number)", "Largest integer less than or equal to `number`."),
            ["sin"] = new("sin(number)", "Sine of `number` in radians."),
            ["sqrt"] = new("sqrt(number)", "Square root of `number`."),
            ["tan"] = new("tan(number)", "Tangent of `number` in radians.")
        };

        return map;
    }

    private sealed record BuiltInSymbol(string Signature, string Description);
}
