﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

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
                    case "--objects":
                        options.PrintObjects = true;
                        break;
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
                    case "--deps":
                         if (i + 1 < args.Length)
                         {
                             options.DependenciesFile = args[i + 1];
                         }
                         else
                         {
                             WriteLine("No file path given for --deps option.");
                             return 1;
                         }
                         i++;
                         break;
                    case "--flags":
                        if (i + 1 < args.Length)
                        {
                            // Split flags on comma and append if not already in list
                            var allFlags = args[i + 1].Split(",").Select(s => s.Trim());
                            foreach (var flag in allFlags)
                            {
                                if (!options.Flags.Contains(flag)) options.Flags.Add(flag);
                            }
                        }
                        else
                        {
                            WriteLine($"No flags given for {args[i]} option.");
                            return 1;
                        }
                        i++;
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

            if (options.PrintObjects)
            {
                EnergyPlusObjectListener listener = new();
                ParseTreeWalker w = new();
                w.Walk(listener, tree);
                return 0;
            }

            // Construct the main visitor for the initial file.
            IdfPlusVisitor visitor = new(baseDirectory, options.FileType, options.Flags);

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
                catch (Exception)
                {
                    Console.Error.WriteLine($"Could not write output to {options.OutputFile}.");
                    return 1;
                }
            }

            if (!string.IsNullOrEmpty(options.DependenciesFile))
            {
                StringBuilder b = new();
                foreach (var dep in Dependencies.Set.Order())
                {
                    b.Append(dep);
                    b.Append('\n');
                }

                try
                {
                    File.WriteAllText(options.DependenciesFile, b.ToString());
                }
                catch (Exception)
                {
                    Console.Error.WriteLine($"Could not write dependencies output to {options.DependenciesFile}.");
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
            public List<string> Flags = new();
            public bool PrintObjects = false;
            public string DependenciesFile = "";
        }
    }
}
