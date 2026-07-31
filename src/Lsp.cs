#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
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

    // The stdio server logs every message to a temp file. In-process consumers
    // (the GUI) run the same code on every keystroke and turn this off.
    internal static bool LoggingEnabled { get; set; } = true;

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
            case "textDocument/definition":
                HandleDefinition(message, idToken);
                break;
            case "textDocument/references":
                HandleReferences(message, idToken);
                break;
            case "textDocument/completion":
                HandleCompletion(message, idToken);
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
                    Change = TextDocumentSyncKind.Incremental,
                    OpenClose = true
                },
                HoverProvider = true,
                DefinitionProvider = true,
                ReferencesProvider = true,
                CompletionProvider = new CompletionOptions
                {
                    ResolveProvider = false,
                    TriggerCharacters = new[] { "," }
                }
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

        if (_documents.TryGetValue(key, out DocumentState? document))
        {
            document.ApplyContentChanges(parameters.ContentChanges ?? Enumerable.Empty<TextDocumentContentChangeEvent>());
            return;
        }

        string? latest = null;
        if (parameters.ContentChanges is not null)
        {
            foreach (TextDocumentContentChangeEvent change in parameters.ContentChanges)
            {
                if (change.Range is null)
                {
                    latest = change.Text;
                }
            }
        }

        if (latest is not null)
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

        string? identifier = null;
        LspRange? range = null;
        VariableDefinition? variableDefinition = null;

        if (token.Type == NeobemLexer.IDENTIFIER)
        {
            identifier = token.Text;
            range = CreateTokenRange(token);
            variableDefinition = document.GetVariableDefinitionForUsage(token.TokenIndex);
        }
        else if (token.Type == NeobemLexer.FIELD)
        {
            // The cursor may sit on an identifier inside a <..> replacement, which is
            // opaque FIELD text to the main parse but indexed separately.
            ReplacementIdentifier? replacementIdentifier = document.FindReplacementIdentifierAt(
                parameters.Position.Line,
                parameters.Position.Character);
            if (replacementIdentifier is not null)
            {
                identifier = replacementIdentifier.Name;
                range = replacementIdentifier.Range;
                variableDefinition = replacementIdentifier.Definition;
            }
        }

        Log(identifier ?? string.Empty);

        string? hoverBody = null;
        if (!string.IsNullOrEmpty(identifier))
        {
            if (variableDefinition is not null)
            {
                hoverBody = BuildVariableHoverMarkdown(variableDefinition);
            }
            else if (BuiltInSymbols.TryGetValue(identifier, out BuiltInSymbol? info))
            {
                hoverBody = BuildHoverMarkdown(info);
            }
        }

        if (hoverBody is null || range is null)
        {
            SendResponse(idToken, null);
            return;
        }

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

    private void HandleDefinition(JObject message, JToken? idToken)
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

        IReadOnlyList<Location> definitions = document.FindDefinitions(
            parameters.TextDocument.Uri,
            parameters.Position.Line,
            parameters.Position.Character);

        SendResponse(idToken, definitions.Count == 0 ? null : definitions.ToArray());
    }

    private void HandleReferences(JObject message, JToken? idToken)
    {
        if (idToken is null)
        {
            return;
        }

        ReferenceParams? parameters = message["params"]?.ToObject<ReferenceParams>(_serializer);
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

        bool includeDeclaration = parameters.Context?.IncludeDeclaration ?? false;
        IReadOnlyList<Location> references = document.FindReferences(
            parameters.TextDocument.Uri,
            parameters.Position.Line,
            parameters.Position.Character,
            includeDeclaration);

        SendResponse(idToken, references.Count == 0 ? null : references.ToArray());
    }

    private void HandleCompletion(JObject message, JToken? idToken)
    {
        if (idToken is null)
        {
            return;
        }

        CompletionParams? parameters = message["params"]?.ToObject<CompletionParams>(_serializer);
        if (parameters?.TextDocument?.Uri is null)
        {
            SendResponse(idToken, Array.Empty<CompletionItem>());
            return;
        }

        string key = parameters.TextDocument.Uri.ToString();
        if (!_documents.TryGetValue(key, out DocumentState? document))
        {
            SendResponse(idToken, Array.Empty<CompletionItem>());
            return;
        }

        IReadOnlyList<CompletionItem> completions = document.FindCompletions(
            parameters.Position.Line,
            parameters.Position.Character);

        SendResponse(idToken, completions.ToArray());
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
        if (!LoggingEnabled)
        {
            return;
        }

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
        if (!LoggingEnabled)
        {
            return;
        }

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

    // Hover content for a built-in identifier, for callers that want the text
    // without speaking JSON-RPC. Null when the identifier is not a built-in.
    public static string? TryGetBuiltInHoverMarkdown(string? identifier) =>
        !string.IsNullOrEmpty(identifier) && BuiltInSymbols.TryGetValue(identifier, out BuiltInSymbol? symbol)
            ? BuildHoverMarkdown(symbol)
            : null;

    private static string BuildVariableHoverMarkdown(VariableDefinition definition)
    {
        string detail = string.IsNullOrEmpty(definition.Detail) ? definition.Name : definition.Detail;
        return $"```neobem\n{detail}\n```";
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

    internal sealed class DocumentState
    {
        private readonly SimpleAntlrErrorListener _lexerErrorListener = new();
        private readonly SimpleAntlrErrorListener _parserErrorListener = new();
        private readonly FileType _fileType;
        private AntlrInputStream _inputStream;
        private readonly NeobemLexer _lexer;
        private readonly CommonTokenStream _tokenStream;
        private readonly NeobemParser _parser;
        private readonly List<IToken> _tokenCache = new();
        private readonly Dictionary<string, List<ObjectDefinition>> _objectDefinitions = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<int> _referableNameTokenIndices = new();
        private readonly Dictionary<int, ObjectFieldCompletionTarget> _objectFieldCompletionTargetsByTokenIndex = new();
        private readonly Dictionary<int, VariableDefinition> _variableDefinitionsByUsageTokenIndex = new();
        private readonly List<ReplacementIdentifier> _replacementIdentifiers = new();

        public string Text { get; private set; }
        public NeobemParser.IdfContext? ParseTree { get; private set; }
        public IReadOnlyList<AntlrError> LexerErrors => _lexerErrorListener.Errors;
        public IReadOnlyList<AntlrError> ParserErrors => _parserErrorListener.Errors;
        public Exception? LastParseException { get; private set; }

        public DocumentState(string text, FileType fileType)
        {
            Text = text;
            _fileType = fileType;
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

        public void ApplyContentChanges(IEnumerable<TextDocumentContentChangeEvent> changes)
        {
            string updatedText = Text;

            foreach (TextDocumentContentChangeEvent change in changes)
            {
                updatedText = change.Range is null
                    ? change.Text ?? string.Empty
                    : ApplyIncrementalChange(updatedText, change.Range, change.Text ?? string.Empty);
            }

            UpdateText(updatedText);
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

        public IReadOnlyList<Location> FindDefinitions(Uri documentUri, int zeroBasedLine, int zeroBasedCharacter)
        {
            if (_fileType != FileType.Idf)
            {
                return Array.Empty<Location>();
            }

            IToken? token = FindToken(zeroBasedLine, zeroBasedCharacter);
            if (token is null)
            {
                return Array.Empty<Location>();
            }

            if (token.Type == NeobemLexer.IDENTIFIER &&
                _variableDefinitionsByUsageTokenIndex.TryGetValue(token.TokenIndex, out VariableDefinition? variableDefinition))
            {
                return new[] { variableDefinition.ToLocation(documentUri) };
            }

            if (token.Type != NeobemLexer.FIELD)
            {
                return Array.Empty<Location>();
            }

            // Identifiers inside <..> replacements live within FIELD tokens; check the
            // replacement index before falling back to the object-name lookup.
            ReplacementIdentifier? replacementIdentifier = FindReplacementIdentifierAt(zeroBasedLine, zeroBasedCharacter);
            if (replacementIdentifier?.Definition is not null)
            {
                return new[] { replacementIdentifier.Definition.ToLocation(documentUri) };
            }

            string lookupName = NormalizeFieldValue(token.Text);
            if (string.IsNullOrEmpty(lookupName) || !_objectDefinitions.TryGetValue(lookupName, out List<ObjectDefinition>? definitions))
            {
                return Array.Empty<Location>();
            }

            return definitions.Select(definition => definition.ToLocation(documentUri)).ToArray();
        }

        public IReadOnlyList<Location> FindReferences(
            Uri documentUri,
            int zeroBasedLine,
            int zeroBasedCharacter,
            bool includeDeclaration)
        {
            if (_fileType != FileType.Idf)
            {
                return Array.Empty<Location>();
            }

            IToken? token = FindToken(zeroBasedLine, zeroBasedCharacter);
            if (token is null || token.Type != NeobemLexer.FIELD)
            {
                return Array.Empty<Location>();
            }

            string targetName = NormalizeFieldValue(token.Text);
            if (string.IsNullOrEmpty(targetName))
            {
                return Array.Empty<Location>();
            }

            List<Location> locations = new();
            foreach (IToken candidate in _tokenCache)
            {
                if (candidate.Type != NeobemLexer.FIELD)
                {
                    continue;
                }

                string candidateName = NormalizeFieldValue(candidate.Text);
                if (!string.Equals(candidateName, targetName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool isDeclaration = _referableNameTokenIndices.Contains(candidate.TokenIndex);
                if (!includeDeclaration && isDeclaration)
                {
                    continue;
                }

                locations.Add(new Location
                {
                    Uri = documentUri,
                    Range = CreateTrimmedFieldRange(candidate)
                });
            }

            return locations;
        }

        public IReadOnlyList<CompletionItem> FindCompletions(int zeroBasedLine, int zeroBasedCharacter)
        {
            if (_fileType != FileType.Idf)
            {
                return Array.Empty<CompletionItem>();
            }

            if (!TryGetObjectFieldCompletionTarget(
                    zeroBasedLine,
                    zeroBasedCharacter,
                    out IToken? token,
                    out ObjectFieldCompletionTarget? completionTarget))
            {
                Log($"Completion miss at {zeroBasedLine}:{zeroBasedCharacter}");
                return Array.Empty<CompletionItem>();
            }

            ObjectFieldCompletionTarget resolvedTarget = completionTarget!;

            if (!EnergyPlusFieldKeyData.Objects.TryGetValue(resolvedTarget.ObjectType, out EnergyPlusObjectFieldKeyMap? objectFieldMap) ||
                objectFieldMap is null ||
                !objectFieldMap.FieldsByPosition.TryGetValue(resolvedTarget.FieldPosition, out EnergyPlusFieldKeyDefinition? fieldDefinition) ||
                fieldDefinition is null)
            {
                Log($"Completion map miss at {zeroBasedLine}:{zeroBasedCharacter} token={DescribeToken(token)} target={resolvedTarget.ObjectType}#{resolvedTarget.FieldPosition}");
                return Array.Empty<CompletionItem>();
            }

            Log($"Completion hit at {zeroBasedLine}:{zeroBasedCharacter} token={DescribeToken(token)} target={resolvedTarget.ObjectType}#{resolvedTarget.FieldPosition}");

            return fieldDefinition.Keys.Select(key => new CompletionItem
            {
                Label = key,
                Kind = CompletionItemKind.EnumMember,
                Detail = fieldDefinition.Label,
                FilterText = key,
                InsertText = key,
                TextEdit = new TextEdit
                {
                    NewText = key,
                    Range = resolvedTarget.ReplacementRange
                }
            }).ToArray();
        }

        private bool TryGetObjectFieldCompletionTarget(
            int zeroBasedLine,
            int zeroBasedCharacter,
            out IToken? token,
            out ObjectFieldCompletionTarget? completionTarget)
        {
            token = FindToken(zeroBasedLine, zeroBasedCharacter);
            completionTarget = null;

            if (token is null)
            {
                return false;
            }

            if (token.Type != NeobemLexer.FIELD &&
                token.Type != NeobemLexer.FIELD_SEP &&
                token.Type != NeobemLexer.OBJECT_TERMINATOR)
            {
                return false;
            }

            // A cursor on the separator's own line still belongs to the field that just ended.
            // Positions on later lines within the same separator belong to the upcoming field.
            if ((token.Type == NeobemLexer.FIELD_SEP || token.Type == NeobemLexer.OBJECT_TERMINATOR) &&
                zeroBasedLine == token.Line - 1)
            {
                IToken? previousToken = FindPreviousDefaultToken(token.TokenIndex);
                if (previousToken is not null &&
                    _objectFieldCompletionTargetsByTokenIndex.TryGetValue(previousToken.TokenIndex, out completionTarget))
                {
                    token = previousToken;
                    return true;
                }
            }

            return _objectFieldCompletionTargetsByTokenIndex.TryGetValue(token.TokenIndex, out completionTarget);
        }

        private IToken? FindPreviousDefaultToken(int tokenIndex)
        {
            for (int index = tokenIndex - 1; index >= 0; index--)
            {
                IToken token = _tokenStream.Get(index);
                if (token.Type == TokenConstants.EOF || token.Channel != TokenConstants.DefaultChannel)
                {
                    continue;
                }

                return token;
            }

            return null;
        }

        private static string DescribeToken(IToken? token)
        {
            if (token is null)
            {
                return "null";
            }

            string text = (token.Text ?? string.Empty)
                .Replace("\r", "\\r", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal);
            return $"{token.Type}:{text}";
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
            int tokenStartLine = Math.Max(token.Line - 1, 0);
            int tokenStartCharacter = Math.Max(token.Column, 0);
            (int tokenEndLine, int tokenEndCharacter) = ComputeTokenEndPosition(token, tokenStartLine, tokenStartCharacter);

            if (ComparePosition(zeroBasedStartLine, zeroBasedCharacter, tokenStartLine, tokenStartCharacter) < 0)
            {
                return -1;
            }

            if (ComparePosition(zeroBasedStartLine, zeroBasedCharacter, tokenEndLine, tokenEndCharacter) >= 0)
            {
                return 1;
            }

            return 0;
        }

        private static int ComparePosition(int leftLine, int leftCharacter, int rightLine, int rightCharacter)
        {
            int lineComparison = leftLine.CompareTo(rightLine);
            return lineComparison != 0 ? lineComparison : leftCharacter.CompareTo(rightCharacter);
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

            RebuildObjectDefinitions();
        }

        private void RebuildObjectDefinitions()
        {
            _objectDefinitions.Clear();
            _referableNameTokenIndices.Clear();
            _objectFieldCompletionTargetsByTokenIndex.Clear();
            _variableDefinitionsByUsageTokenIndex.Clear();
            _replacementIdentifiers.Clear();

            if (_fileType != FileType.Idf || ParseTree is null)
            {
                return;
            }

            ParseTreeWalker walker = new();
            walker.Walk(new ObjectDefinitionListener(_objectDefinitions, _referableNameTokenIndices), ParseTree);
            walker.Walk(new ObjectFieldCompletionListener(_tokenStream, _objectFieldCompletionTargetsByTokenIndex), ParseTree);

            RebuildVariableDefinitions();
        }

        private void RebuildVariableDefinitions()
        {
            if (ParseTree is null)
            {
                return;
            }

            new ScopedVariableIndexer(this).Index(ParseTree);
        }

        public VariableDefinition? GetVariableDefinitionForUsage(int tokenIndex) =>
            _variableDefinitionsByUsageTokenIndex.TryGetValue(tokenIndex, out VariableDefinition? definition)
                ? definition
                : null;

        public ReplacementIdentifier? FindReplacementIdentifierAt(int zeroBasedLine, int zeroBasedCharacter)
        {
            foreach (ReplacementIdentifier identifier in _replacementIdentifiers)
            {
                LspRange range = identifier.Range;
                if (ComparePosition(zeroBasedLine, zeroBasedCharacter, range.Start.Line, range.Start.Character) >= 0 &&
                    ComparePosition(zeroBasedLine, zeroBasedCharacter, range.End.Line, range.End.Character) < 0)
                {
                    return identifier;
                }
            }

            return null;
        }

        /// <summary>
        /// Walks the parse tree with lexical scoping (top level, lambda parameters and
        /// bodies, let bindings) and records where each identifier usage resolves. For
        /// objects it additionally sub-parses the &lt;..&gt; replacement expressions —
        /// which the main grammar leaves as opaque FIELD text — and records those
        /// identifiers with document-absolute positions.
        /// </summary>
        private sealed class ScopedVariableIndexer
        {
            private readonly DocumentState _document;
            private readonly List<Dictionary<string, VariableDefinition>> _scopes = new();
            private readonly string[] _lines;

            public ScopedVariableIndexer(DocumentState document)
            {
                _document = document;
                _lines = document.Text.Split('\n');
            }

            public void Index(NeobemParser.IdfContext tree)
            {
                _scopes.Add(new Dictionary<string, VariableDefinition>(StringComparer.Ordinal));

                foreach (NeobemParser.Base_idfContext statement in tree.base_idf())
                {
                    switch (statement)
                    {
                        case NeobemParser.VariableDeclarationContext variableDeclaration:
                            IndexVariableDeclaration(variableDeclaration.variable_declaration(), null);
                            break;
                        case NeobemParser.PrintStatmentContext printStatment:
                            IndexExpressionTree(printStatment.print_statment().expression(), null);
                            break;
                        case NeobemParser.LogStatementContext logStatement:
                            IndexExpressionTree(logStatement.log_statement().expression(), null);
                            break;
                        case NeobemParser.ObjectDeclarationContext objectDeclaration:
                            IndexObjectReplacements(objectDeclaration.@object());
                            break;
                        case NeobemParser.ImportStatementContext importStatement:
                            IndexImportStatement(importStatement.import_statement());
                            break;
                    }
                }
            }

            private void IndexVariableDeclaration(NeobemParser.Variable_declarationContext declaration, SnippetPositionMap? snippet)
            {
                // Index the right-hand side first so self and forward references stay unresolved.
                IndexExpressionTree(declaration.expression(), snippet);
                Define(declaration.IDENTIFIER().Symbol, snippet);
            }

            private void IndexImportStatement(NeobemParser.Import_statementContext importStatement)
            {
                IndexExpressionTree(importStatement.expression(), null);

                foreach (NeobemParser.Import_optionContext option in importStatement.import_option())
                {
                    switch (option)
                    {
                        case NeobemParser.AsOptionContext asOption:
                            Define(asOption.IDENTIFIER().Symbol, null);
                            break;
                        case NeobemParser.OnlyOptionContext onlyOption:
                            foreach (ITerminalNode name in onlyOption.IDENTIFIER())
                            {
                                Define(name.Symbol, null);
                            }
                            break;
                    }
                }
            }

            private void IndexExpressionTree(IParseTree? tree, SnippetPositionMap? snippet)
            {
                switch (tree)
                {
                    case null:
                        return;
                    case NeobemParser.Lambda_defContext lambda:
                        IndexLambda(lambda, snippet);
                        return;
                    case NeobemParser.Let_bindingContext letBinding:
                        IndexLetBinding(letBinding, snippet);
                        return;
                    case ITerminalNode terminal:
                        if (terminal.Symbol.Type == NeobemLexer.IDENTIFIER)
                        {
                            RecordUsage(terminal.Symbol, snippet);
                        }
                        return;
                }

                for (int i = 0; i < tree.ChildCount; i++)
                {
                    IndexExpressionTree(tree.GetChild(i), snippet);
                }
            }

            private void IndexLambda(NeobemParser.Lambda_defContext lambda, SnippetPositionMap? snippet)
            {
                _scopes.Add(new Dictionary<string, VariableDefinition>(StringComparer.Ordinal));

                foreach (ITerminalNode parameter in lambda.IDENTIFIER())
                {
                    Define(parameter.Symbol, snippet, $"(parameter) {parameter.Symbol.Text}");
                }

                IndexExpressionTree(lambda.expression(), snippet);

                foreach (NeobemParser.Function_statementContext statement in lambda.function_statement())
                {
                    IndexFunctionStatement(statement, snippet);
                }

                _scopes.RemoveAt(_scopes.Count - 1);
            }

            private void IndexFunctionStatement(NeobemParser.Function_statementContext statement, SnippetPositionMap? snippet)
            {
                switch (statement)
                {
                    case NeobemParser.FunctionVariableDeclarationContext variableDeclaration:
                        IndexVariableDeclaration(variableDeclaration.variable_declaration(), snippet);
                        break;
                    case NeobemParser.FunctionObjectDeclarationContext objectDeclaration:
                        // Objects parsed from a replacement snippet would need their own
                        // token bookkeeping and nested replacements are unsupported anyway.
                        if (snippet is null)
                        {
                            IndexObjectReplacements(objectDeclaration.@object());
                        }
                        break;
                    case NeobemParser.FunctionPrintStatementContext printStatement:
                        IndexExpressionTree(printStatement.print_statment().expression(), snippet);
                        break;
                    case NeobemParser.FunctionLogStatementContext logStatement:
                        IndexExpressionTree(logStatement.log_statement().expression(), snippet);
                        break;
                    case NeobemParser.ReturnStatementContext returnStatement:
                        IndexExpressionTree(returnStatement.return_statement().expression(), snippet);
                        break;
                }
            }

            private void IndexLetBinding(NeobemParser.Let_bindingContext letBinding, SnippetPositionMap? snippet)
            {
                _scopes.Add(new Dictionary<string, VariableDefinition>(StringComparer.Ordinal));

                ITerminalNode[] names = letBinding.IDENTIFIER();
                NeobemParser.ExpressionContext[] expressions = letBinding.expression();

                for (int i = 0; i < names.Length && i < expressions.Length; i++)
                {
                    IndexExpressionTree(expressions[i], snippet);
                    Define(names[i].Symbol, snippet);
                }

                IndexExpressionTree(letBinding.let_expression(), snippet);

                _scopes.RemoveAt(_scopes.Count - 1);
            }

            private void IndexObjectReplacements(NeobemParser.ObjectContext objectContext)
            {
                // Reconstruct the same text runtime evaluation sees (context.GetText():
                // default-channel token texts, skipped whitespace absent), remembering
                // which token owns each character offset.
                StringBuilder builder = new();
                List<(int Offset, IToken Token)> segments = new();

                for (int tokenIndex = objectContext.Start.TokenIndex; tokenIndex <= objectContext.Stop.TokenIndex; tokenIndex++)
                {
                    IToken token = _document._tokenStream.Get(tokenIndex);
                    if (token.Channel != TokenConstants.DefaultChannel)
                    {
                        continue;
                    }

                    segments.Add((builder.Length, token));
                    builder.Append(token.Text);
                }

                string objectText = builder.ToString();
                (List<ReplacementSpan> spans, _) = ReplacementScanner.Scan(objectText);

                foreach (ReplacementSpan span in spans)
                {
                    string expressionText = span.ExpressionText(objectText);
                    if (string.IsNullOrWhiteSpace(expressionText))
                    {
                        continue;
                    }

                    NeobemLexer lexer = new(new AntlrInputStream(expressionText)) { FileType = FileType.Idf };
                    lexer.RemoveErrorListeners();
                    NeobemParser parser = new(new CommonTokenStream(lexer));
                    parser.RemoveErrorListeners();

                    try
                    {
                        NeobemParser.ExpressionContext expressionTree = parser.expression();
                        IndexExpressionTree(expressionTree, new SnippetPositionMap(segments, span.ExpressionStart));
                    }
                    catch (Exception)
                    {
                        // A malformed replacement shouldn't take down indexing of the rest of the document.
                    }
                }
            }

            private void RecordUsage(IToken token, SnippetPositionMap? snippet)
            {
                VariableDefinition? definition = Resolve(token.Text);

                if (snippet is null)
                {
                    if (definition is not null)
                    {
                        _document._variableDefinitionsByUsageTokenIndex[token.TokenIndex] = definition;
                    }

                    return;
                }

                // Record snippet identifiers even when unresolved so hover can still
                // surface built-in function documentation.
                _document._replacementIdentifiers.Add(new ReplacementIdentifier(token.Text, snippet.RangeOf(token), definition));
            }

            private VariableDefinition? Resolve(string name)
            {
                for (int i = _scopes.Count - 1; i >= 0; i--)
                {
                    if (_scopes[i].TryGetValue(name, out VariableDefinition? definition))
                    {
                        return definition;
                    }
                }

                return null;
            }

            private void Define(IToken identifierToken, SnippetPositionMap? snippet, string? detailOverride = null)
            {
                LspRange range = snippet is null ? CreateTokenRange(identifierToken) : snippet.RangeOf(identifierToken);
                string detail = detailOverride ?? LineTextAt(range.Start.Line);
                _scopes[^1][identifierToken.Text] = new VariableDefinition(identifierToken.Text, range, detail);
            }

            private string LineTextAt(int line) =>
                line >= 0 && line < _lines.Length ? _lines[line].TrimEnd('\r').Trim() : string.Empty;
        }

        /// <summary>
        /// Maps token positions from a replacement expression sub-parse back to
        /// document-absolute positions, via the object-text offset each main-tree
        /// token contributed.
        /// </summary>
        private sealed class SnippetPositionMap
        {
            private readonly List<(int Offset, IToken Token)> _segments;
            private readonly int _expressionStart;

            public SnippetPositionMap(List<(int Offset, IToken Token)> segments, int expressionStart)
            {
                _segments = segments;
                _expressionStart = expressionStart;
            }

            public LspRange RangeOf(IToken snippetToken)
            {
                (int startLine, int startCharacter) = MapOffset(_expressionStart + snippetToken.StartIndex);
                (int endLine, int endCharacter) = MapOffset(_expressionStart + snippetToken.StopIndex + 1);

                return new LspRange
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
            }

            private (int Line, int Character) MapOffset(int objectTextOffset)
            {
                int index = 0;
                for (int i = 0; i < _segments.Count; i++)
                {
                    if (_segments[i].Offset <= objectTextOffset)
                    {
                        index = i;
                    }
                    else
                    {
                        break;
                    }
                }

                (int segmentOffset, IToken token) = _segments[index];
                return AdvanceWithinToken(token, objectTextOffset - segmentOffset);
            }

            private static (int Line, int Character) AdvanceWithinToken(IToken token, int characterOffset)
            {
                int line = Math.Max(token.Line - 1, 0);
                int character = Math.Max(token.Column, 0);
                string text = token.Text ?? string.Empty;
                int limit = Math.Min(characterOffset, text.Length);

                for (int i = 0; i < limit; i++)
                {
                    char current = text[i];
                    if (current == '\n')
                    {
                        line++;
                        character = 0;
                    }
                    else if (current == '\r')
                    {
                        // Counts as a character of offset but the line advance happens
                        // on the '\n' that follows, when present.
                        if (i + 1 >= text.Length || text[i + 1] != '\n')
                        {
                            line++;
                            character = 0;
                        }
                    }
                    else
                    {
                        character++;
                    }
                }

                return (line, character);
            }
        }

        private static string ApplyIncrementalChange(string currentText, LspRange range, string replacementText)
        {
            int startOffset = GetOffset(currentText, range.Start);
            int endOffset = GetOffset(currentText, range.End);

            if (startOffset > endOffset)
            {
                (startOffset, endOffset) = (endOffset, startOffset);
            }

            return currentText[..startOffset] + replacementText + currentText[endOffset..];
        }

        private static int GetOffset(string text, Position position)
        {
            int targetLine = Math.Max(position.Line, 0);
            int targetCharacter = Math.Max(position.Character, 0);
            int line = 0;
            int character = 0;
            int index = 0;

            while (index < text.Length)
            {
                if (line == targetLine && character == targetCharacter)
                {
                    return index;
                }

                char current = text[index];
                if (current == '\r')
                {
                    index++;
                    if (index < text.Length && text[index] == '\n')
                    {
                        index++;
                    }

                    line++;
                    character = 0;
                    continue;
                }

                if (current == '\n')
                {
                    index++;
                    line++;
                    character = 0;
                    continue;
                }

                index++;
                character++;
            }

            return text.Length;
        }

        internal static string NormalizeFieldValue(string? fieldValue) => (fieldValue ?? string.Empty).Trim();

    }

    private sealed class ObjectDefinitionListener : NeobemParserBaseListener
    {
        private readonly Dictionary<string, List<ObjectDefinition>> _objectDefinitions;
        private readonly HashSet<int> _nameTokenIndices;

        public ObjectDefinitionListener(
            Dictionary<string, List<ObjectDefinition>> objectDefinitions,
            HashSet<int> nameTokenIndices)
        {
            _objectDefinitions = objectDefinitions;
            _nameTokenIndices = nameTokenIndices;
        }

        public override void EnterObject(NeobemParser.ObjectContext context)
        {
            string objectType = context.OBJECT_TYPE().GetText().Trim();
            if (!LspReferableIdfObjectTypes.Values.Contains(objectType))
            {
                return;
            }

            ITerminalNode[] fields = context.FIELD();
            if (fields.Length < 1)
            {
                return;
            }

            IToken nameToken = fields[0].Symbol;
            string objectName = DocumentState.NormalizeFieldValue(nameToken.Text);
            if (string.IsNullOrEmpty(objectName))
            {
                return;
            }

            _nameTokenIndices.Add(nameToken.TokenIndex);

            int startLine = Math.Max(nameToken.Line - 1, 0);
            int startCharacter = Math.Max(nameToken.Column, 0);
            (int endLine, int endCharacter) = ComputeTokenEndPosition(nameToken, startLine, startCharacter);

            if (!_objectDefinitions.TryGetValue(objectName, out List<ObjectDefinition>? definitions))
            {
                definitions = new List<ObjectDefinition>();
                _objectDefinitions[objectName] = definitions;
            }

            definitions.Add(new ObjectDefinition(objectType, objectName, new LspRange
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
            }));
        }
    }

    private sealed class ObjectFieldCompletionListener : NeobemParserBaseListener
    {
        private readonly CommonTokenStream _tokenStream;
        private readonly Dictionary<int, ObjectFieldCompletionTarget> _completionTargetsByTokenIndex;

        public ObjectFieldCompletionListener(
            CommonTokenStream tokenStream,
            Dictionary<int, ObjectFieldCompletionTarget> completionTargetsByTokenIndex)
        {
            _tokenStream = tokenStream;
            _completionTargetsByTokenIndex = completionTargetsByTokenIndex;
        }

        public override void EnterObject(NeobemParser.ObjectContext context)
        {
            string objectType = context.OBJECT_TYPE().GetText().Trim();
            if (!EnergyPlusFieldKeyData.Objects.ContainsKey(objectType))
            {
                return;
            }

            int fieldPosition = 0;
            IToken? currentFieldSeparator = null;

            for (int tokenIndex = context.Start.TokenIndex + 1; tokenIndex <= context.Stop.TokenIndex; tokenIndex++)
            {
                IToken token = _tokenStream.Get(tokenIndex);
                if (token.Channel != TokenConstants.DefaultChannel)
                {
                    continue;
                }

                switch (token.Type)
                {
                    case NeobemLexer.FIELD_SEP:
                        AddEmptyFieldCompletionTargetIfNeeded(objectType, fieldPosition, currentFieldSeparator);
                        fieldPosition++;
                        currentFieldSeparator = token;
                        break;
                    case NeobemLexer.FIELD:
                        if (currentFieldSeparator is null)
                        {
                            break;
                        }

                        ObjectFieldCompletionTarget completionTarget = new(
                            objectType,
                            fieldPosition,
                            CreateTrimmedFieldRange(token));
                        _completionTargetsByTokenIndex[currentFieldSeparator.TokenIndex] = completionTarget;
                        _completionTargetsByTokenIndex[token.TokenIndex] = completionTarget;
                        currentFieldSeparator = null;
                        break;
                    case NeobemLexer.OBJECT_TERMINATOR:
                        AddEmptyFieldCompletionTargetIfNeeded(objectType, fieldPosition, currentFieldSeparator);
                        currentFieldSeparator = null;
                        break;
                }
            }
        }

        private void AddEmptyFieldCompletionTargetIfNeeded(string objectType, int fieldPosition, IToken? fieldSeparatorToken)
        {
            if (fieldSeparatorToken is null || fieldPosition <= 0)
            {
                return;
            }

            _completionTargetsByTokenIndex[fieldSeparatorToken.TokenIndex] = new ObjectFieldCompletionTarget(
                objectType,
                fieldPosition,
                CreateEmptyRangeAtTokenEnd(fieldSeparatorToken));
        }
    }

    internal sealed record ObjectDefinition(string ObjectType, string Name, LspRange Range)
    {
        public Location ToLocation(Uri documentUri) => new()
        {
            Uri = documentUri,
            Range = Range
        };
    }

    internal sealed record ObjectFieldCompletionTarget(string ObjectType, int FieldPosition, LspRange ReplacementRange);

    internal sealed record VariableDefinition(string Name, LspRange Range, string? Detail = null)
    {
        public Location ToLocation(Uri documentUri) => new()
        {
            Uri = documentUri,
            Range = Range
        };
    }

    // An identifier found by sub-parsing a <..> replacement expression inside an
    // object. Range is document-absolute; Definition is null when the name did not
    // resolve in the lexical scope at the object's position.
    internal sealed record ReplacementIdentifier(string Name, LspRange Range, VariableDefinition? Definition);

    private static LspRange CreateEmptyRangeAtTokenEnd(IToken token)
    {
        int startLine = Math.Max(token.Line - 1, 0);
        int startCharacter = Math.Max(token.Column, 0);
        (int endLine, int endCharacter) = ComputeTokenEndPosition(token, startLine, startCharacter);
        Position position = new()
        {
            Line = endLine,
            Character = endCharacter
        };

        return new LspRange
        {
            Start = position,
            End = position
        };
    }

    private static LspRange CreateTrimmedFieldRange(IToken token)
    {
        string text = token.Text ?? string.Empty;
        int leadingWhitespace = 0;
        while (leadingWhitespace < text.Length && char.IsWhiteSpace(text[leadingWhitespace]))
        {
            leadingWhitespace++;
        }

        int trailingWhitespace = 0;
        while (trailingWhitespace < text.Length - leadingWhitespace &&
               char.IsWhiteSpace(text[text.Length - 1 - trailingWhitespace]))
        {
            trailingWhitespace++;
        }

        int startLine = Math.Max(token.Line - 1, 0);
        int startCharacter = Math.Max(token.Column + leadingWhitespace, 0);
        int endCharacter = Math.Max(startCharacter, token.Column + text.Length - trailingWhitespace);

        Position start = new()
        {
            Line = startLine,
            Character = startCharacter
        };

        Position end = new()
        {
            Line = startLine,
            Character = endCharacter
        };

        return new LspRange
        {
            Start = start,
            End = end
        };
    }

    private static LspRange CreateTokenRange(IToken token)
    {
        int startLine = Math.Max(token.Line - 1, 0);
        int startCharacter = Math.Max(token.Column, 0);
        (int endLine, int endCharacter) = ComputeTokenEndPosition(token, startLine, startCharacter);

        return new LspRange
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
            ["exists"] = new("exists(path)", "Checks whether a definition exists. Most useful for checking for flags passed in on the CLI."),
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
