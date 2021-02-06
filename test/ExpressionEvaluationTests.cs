using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using NUnit.Framework;
using src;

namespace test
{
    public class Tests
    {

        public Expression Evaluate(string expression, Dictionary<string, Expression> variables)
        {
            AntlrInputStream inputStream = new AntlrInputStream(expression);
            IdfplusLexer lexer = new IdfplusLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            IdfplusParser parser = new IdfplusParser(tokens);

            IdfplusParser.ExpressionContext tree =  parser.expression();
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(new List<Dictionary<string, Expression>> {variables});
            return  visitor.Visit(tree);
        }

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

        [Test]
        public void TestNumericExpression()
        {
            Expression output = Evaluate("2 * (3 + 4 / 2)^2", new Dictionary<string, Expression>());
            Console.WriteLine(output.ToString());
        }

        [Test]
        public void TestNumericExpressionWithVariable()
        {
            var variables = new Dictionary<string, Expression>();
            variables["$tons"] = new NumericExpression(2);
            Expression output = Evaluate("2 * (3 + 4 / $tons)^2", variables);
            Console.WriteLine(output.ToString());
        }

        [Test]
        public void TestListConcatenation()
        {
            string test = "$var1 = [2 3 4]\n$var2 = [5 6 7]\n$var3 = $var1 + $var2\nprint $var3\n";
            var parser = test.ToParser();

            var tree = parser.idf();

            ParseTreeWalker walker = new ParseTreeWalker();
            IdfPlusListener listener = new IdfPlusListener();
            walker.Walk(listener, tree);

            Console.WriteLine(listener.Output);
        }

        [Test]
        public void TestStringConcatenation()
        {
            string test = "'My string 1 ' + 'is great'";
            var parser = test.ToParser();
            var tree = parser.expression();
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor();

            Expression expression = visitor.Visit(tree);

            Assert.IsTrue(expression is StringExpression);
            Assert.IsTrue(((StringExpression)expression).Text == "My string 1 is great");

        }

        [Test]
        public void TestVariablePrinting()
        {
            string file = "$myvariable = 12\nprint $myvariable / 6";

            AntlrInputStream inputStream = new AntlrInputStream(file);
            IdfplusLexer lexer = new IdfplusLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            IdfplusParser parser = new IdfplusParser(tokens);
            IdfplusParser.IdfContext tree = parser.idf();
            ParseTreeWalker walker = new ParseTreeWalker();
            IdfPlusListener listener = new IdfPlusListener();
            walker.Walk(listener, tree);

            Console.WriteLine(listener.Output);
        }

        [Test]
        public void TestVariableReplacement()
        {
            string testFile = "$versionNum = 2\nVersion,\n  $versionNum;\n";
            // string testFile = "$versionNum = 2\n\nVersion, 2;\n";

            var parser = testFile.ToParser();
            var tree = parser.idf();

            ParseTreeWalker walker = new ParseTreeWalker();
            IdfPlusListener listener = new IdfPlusListener();
            walker.Walk(listener, tree);

            Console.WriteLine(listener.Output);
        }

        [Test]
        public void TestFunctionApplication()
        {
            string test = "ceiling(3.1415926 / 2)";
            var parser = test.ToParser();
            var tree = parser.expression();
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor();

            Expression expression = visitor.Visit(tree);

            Assert.IsTrue(expression is NumericExpression);
            Assert.IsTrue(Math.Abs(((NumericExpression)expression).Value - 1) < 0.00001);
        }

        [Test]
        public void TestFunctionDefinition()
        {
            string test = "(\\x { x + 2 })(2)";
            var parser = test.ToParser();
            var tree = parser.expression();

            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor();
            Expression expression = visitor.Visit(tree);

            Assert.IsTrue(expression is NumericExpression);
            Assert.IsTrue(Math.Abs(((NumericExpression)expression).Value - 4) < 0.00001);
        }

        [Test]
        public void TestBasicFunctionApplication()
        {
            string test = "myadd = \\x { x + 2 }\nversionnum = myadd(2)\nVersion, $versionnum;\n";
            var parser = test.ToParser();
            var tree = parser.idf();

            ParseTreeWalker walker = new ParseTreeWalker();
            IdfPlusListener listener = new IdfPlusListener();
            walker.Walk(listener, tree);

            Console.WriteLine(listener.Output);
        }

        [Test]
        public void TestNestedFunctionApplication()
        {
            string test = "myadd = \\x { \\y { x + y } }\nversionnum = myadd(2)(3)\nVersion, $versionnum;\n";
            var parser = test.ToParser();
            var tree = parser.idf();

            ParseTreeWalker walker = new ParseTreeWalker();
            IdfPlusListener listener = new IdfPlusListener();
            walker.Walk(listener, tree);

            Console.WriteLine(listener.Output);
            Assert.AreEqual("Version, 5;\n", listener.Output.ToString());
        }

        [Test]
        public void TestBooleanExpressions()
        {
            bool GetBooleanExpression(string s)
            {
                var parser = s.ToParser();
                var expressionTree = parser.expression();
                IdfPlusExpVisitor expVisitor = new IdfPlusExpVisitor();
                BooleanExpression booleanExpression = (BooleanExpression) expVisitor.Visit(expressionTree);
                return booleanExpression.Value;
            }

            string test = "2 == 2";
            Assert.IsTrue(GetBooleanExpression(test));

            test = "2 == 3";
            Assert.IsFalse(GetBooleanExpression(test));

            test = "'Test1' == 'Test1'";
            Assert.IsTrue(GetBooleanExpression(test));

            test = "'Test1' == 'Test2'";
            Assert.IsFalse(GetBooleanExpression(test));

        }
    }
}