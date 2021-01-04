using Antlr4.Runtime;

namespace src
{
    public static class Extensions
    {
        public static IdfplusParser ToParser(this string input)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);
            IdfplusLexer lexer = new IdfplusLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            return new IdfplusParser(tokens);
        }
    }
}