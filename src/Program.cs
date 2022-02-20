using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;

namespace src
{
    class Program
    {
        static int Main(string[] args)
        {
            BempOptions options = new BempOptions();
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "--help":
                        WriteLine(Help.Text());
                        return 0;
                    case "-v":
                    case "--version":
                        WriteLine(Version.Num());
                        return 0;
                    case "-o":
                    case "--output":
                    {
                        if (i + 1 < args.Length)
                        {
                            options.OutputFile = args[i + 1];
                        }
                        else
                        {
                            WriteLine("No file path given for output option.");
                            return 1;
                        }
                        i++;
                        break;
                    }
                    case "-f":
                    case "--fmt":
                        options.FormatFile = true;
                        break;
                    // Tokens for debugging purposes
                    case "--tokens":
                        options.Tokens = true;
                        break;
                    case "--tree":
                        options.Tree = true;
                        break;
                    case "--doe2":
                        options.FileType = FileType.Doe2;
                        break;
                    default:
                        options.InputFile = args[i];
                        break;
                }
            }

            AntlrInputStream inputStream;

            FileInfo fileInfo;
            string baseDirectory;
            if (options.InputFile != "-")
            {
                fileInfo = new FileInfo(options.InputFile);
                if (!fileInfo.Exists)
                {
                    string errorMessage = $"The input file '{fileInfo.FullName}' does not exist.";
                    Console.Error.Write(errorMessage + "\n");
                    return 1;
                }
                inputStream = new AntlrFileStream(fileInfo.FullName);
                baseDirectory = fileInfo.DirectoryName;
            }
            else
            {
                inputStream = new AntlrInputStream(Console.In);
                baseDirectory = Environment.CurrentDirectory;
            }

            NeobemLexer lexer = new(inputStream);
            lexer.FileType = options.FileType;
            lexer.RemoveErrorListeners();
            SimpleAntlrErrorListener lexerErrorListener = new();
            lexer.AddErrorListener(lexerErrorListener);


            CommonTokenStream commonTokenStream = new(lexer);

            if (options.Tokens)
            {
                commonTokenStream.Fill();
                foreach (IToken token in commonTokenStream.GetTokens())
                {
                    Console.Error.Write($"{token.ToString(lexer)}\n");
                }
            }

            if (lexerErrorListener.Errors.Any())
            {
                foreach (AntlrError error in lexerErrorListener.Errors) Console.Error.Write(error.WriteError() + "\n");
                return 1;
            }

            NeobemParser parser = new(commonTokenStream);
            parser.RemoveErrorListeners();
            SimpleAntlrErrorListener parserErrorListener = new();
            parser.AddErrorListener(parserErrorListener);

            NeobemParser.IdfContext tree = parser.idf();

            if (options.Tree) Console.Error.Write(tree.ToStringTree(parser).AddNewLines(1));

            if (parserErrorListener.Errors.Any())
            {
                foreach (AntlrError error in parserErrorListener.Errors) Console.Error.Write(error.WriteError() + "\n");
                return 1;
            }

            if (options.FormatFile)
            {
                FormatVisitor formatVisitor = new(0, 0, commonTokenStream);
                // var listener = new FormatListener(commonTokenStream);

                // ParseTreeWalker walker = new();
                // walker.Walk(listener, tree);
                try
                {
                    // string output = listener.Rewriter.GetText();
                    string output = formatVisitor.Visit(tree);
                    Console.Write(output);
                    return 0;
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine(exception.Message);
                    return 1;
                }
            }

            // Construct the main visitor for the initial file.
            IdfPlusVisitor visitor = new(baseDirectory, options.FileType);

            string result;
            try
            {
                result = visitor.Visit(tree);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
                return 1;
            }
            // If no output file was specified in the command line options, then dump results to standard output.
            if (string.IsNullOrWhiteSpace(options.OutputFile))
            {
                Console.Write(result);
            }
            else
            {
                try
                {
                    File.WriteAllText(options.OutputFile, result);
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine($"Could not write output to {options.OutputFile}.");
                    return 1;
                }
            }

            return 0;
        }

        // Until I get complaints otherwise, going to print using Unix newlines.
        public static void WriteLine(string message) => Console.Write($"{message}\n");

        public class BempOptions
        {
            public string OutputFile = "";
            public string InputFile = "in.nbem";
            public bool FormatFile = false;
            public FileType FileType = FileType.Idf;
            public bool Tokens = false;
            public bool Tree = false;
        }
    }
}
