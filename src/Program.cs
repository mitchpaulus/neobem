using System;
using System.IO;
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
                if (args[i] == "-h" || args[i] == "--help")
                {
                    Console.WriteLine(Help.Text());
                    return 0;
                }
                if (args[i] == "-v" || args[i] == "--version")
                {
                    Console.WriteLine("0.1");
                    return 0;
                }

                if (args[i] == "-o" || args[i] == "--output")
                {
                    if (i + 1 < args.Length)
                    {
                        options.OutputFile = args[i + 1];
                    }
                    else
                    {
                        Console.WriteLine("No file path given for output option.");
                        return 1;
                    }
                    i++;
                }
                else
                {
                    options.InputFile = args[i];
                }
            }

            AntlrInputStream inputStream;

            FileInfo fileInfo;
            if (options.InputFile != "-")
            {
                fileInfo = new FileInfo(options.InputFile);
                inputStream = new AntlrFileStream(fileInfo.FullName);
            }
            else
            {
                throw new NotImplementedException("Input from standard input not implemented yet.");
            }

            NeobemLexer lexer = new NeobemLexer(inputStream);

            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);

            NeobemParser parser = new NeobemParser(commonTokenStream);

            IdfPlusVisitor visitor = new IdfPlusVisitor(fileInfo.DirectoryName);

            NeobemParser.IdfContext tree = parser.idf();

            string result;
            try
            {
                result = visitor.Visit(tree);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return 1;
            }
            if (string.IsNullOrWhiteSpace(options.OutputFile))
            {
                Console.WriteLine(result);
            }
            else
            {
                try
                {
                    File.WriteAllText(options.OutputFile, result);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Could not write output to {options.OutputFile}.");
                }
            }

            return 0;
        }

        public class BempOptions
        {
            public string OutputFile = "";
            public string InputFile = "in.nbem";
        }
    }
}
