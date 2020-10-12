using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace src
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo file = null;
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-h" || args[i] == "--help")
                {
                    Console.WriteLine("idfplus\n\nA better way to EnergyPlus.\n");
                    return;
                }
                else
                {
                    file = new FileInfo(args[i]);
                }
            }

            if (file == null)
            {
                Console.WriteLine("No file specified.");
                return;
            }

            // string input = File.ReadAllText(file.FullName);

            AntlrInputStream inputStream = new AntlrFileStream(file.FullName);
            IdfplusLexer lexer = new IdfplusLexer(inputStream);

            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);

            IdfplusParser parser = new IdfplusParser(commonTokenStream);

            IdfPlusListener listener = new IdfPlusListener();

            ParseTreeWalker walker = new ParseTreeWalker();

            IdfplusParser.IdfContext tree = parser.idf();

            walker.Walk(listener, tree);

            Console.WriteLine(listener.Output);
        }
    }
}
