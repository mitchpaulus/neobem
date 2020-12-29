using System;
using System.IO;
using Antlr4.Runtime;
using NUnit.Framework;
using src;

namespace test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            bool success = VariableRegex.Regex.IsMatch("  $hello ");
        }

        [Test]
        public void TestExpressionEvaluation()
        {
            StringReader reader = new StringReader("5 ^ 3");

            AntlrInputStream inputStream = new AntlrInputStream("5 ^ 3");

            IdfplusLexer lexer = new IdfplusLexer(inputStream);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            IdfplusParser parser = new IdfplusParser(tokens);

            IdfplusParser.ExpressionContext tree =  parser.expression();

            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor();

            Expression expression = visitor.Visit(tree);

            Assert.IsTrue(expression is NumericExpression);
            Assert.IsTrue(Math.Abs((expression as NumericExpression).Value - 125) < 0.00000001);
        }
    }
}