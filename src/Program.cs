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

            AntlrInputStream inputStream = new AntlrFileStream(file.FullName);
            IdfplusLexer lexer = new IdfplusLexer(inputStream);

            CommonTokenStream commonTokenStream = new CommonTokenStream(lexer);

            IdfplusParser parser = new IdfplusParser(commonTokenStream);

            IdfPlusVisitor visitor = new IdfPlusVisitor();

            IdfplusParser.IdfContext tree = parser.idf();

            string result = visitor.Visit(tree);

            Console.WriteLine(result);
        }
    }
}
