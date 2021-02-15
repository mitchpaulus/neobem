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
            NeobemLexer lexer = new NeobemLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            NeobemParser parser = new NeobemParser(tokens);

            NeobemParser.ExpressionContext tree =  parser.expression();
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(new List<Dictionary<string, Expression>> {variables}, null);
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
            NeobemLexer lexer = new NeobemLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            NeobemParser parser = new NeobemParser(tokens);

            NeobemParser.ExpressionContext tree =  parser.expression();
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(null);

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
            variables["tons"] = new NumericExpression(2);
            Expression output = Evaluate("2 * (3 + 4 / tons)^2", variables);
            Assert.AreEqual(50, ((NumericExpression)output).Value);
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
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(null);

            Expression expression = visitor.Visit(tree);

            Assert.IsTrue(expression is StringExpression);
            Assert.IsTrue(((StringExpression)expression).Text == "My string 1 is great");

        }

        [Test]
        public void TestVariablePrinting()
        {
            string file = "$myvariable = 12\nprint $myvariable / 6";

            AntlrInputStream inputStream = new AntlrInputStream(file);
            NeobemLexer lexer = new NeobemLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            NeobemParser parser = new NeobemParser(tokens);
            NeobemParser.IdfContext tree = parser.idf();
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
            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(null);

            Expression expression = visitor.Visit(tree);

            Assert.IsTrue(expression is NumericExpression);
            Assert.IsTrue(Math.Abs(((NumericExpression)expression).Value - 2) < 0.00001);
        }

        [Test]
        public void TestFunctionDefinition()
        {
            string test = "(\\x { x + 2 })(2)";
            var parser = test.ToParser();
            var tree = parser.expression();

            IdfPlusExpVisitor visitor = new IdfPlusExpVisitor(null);
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
            string test = "myadd = \\x { \\y { x + y } }\nversionnum = myadd(2)(3)\nVersion,<versionnum>;\n";
            var parser = test.ToParser();
            var tree = parser.idf();

            var visitor = new IdfPlusVisitor(null);

            string output = visitor.Visit(tree);

            Console.WriteLine(output);
            Assert.AreEqual("Version,5;\n\n", output);
        }

        [Test]
        public void TestBooleanExpressions()
        {
            bool GetBooleanExpression(string s)
            {
                var parser = s.ToParser();
                var expressionTree = parser.expression();
                IdfPlusExpVisitor expVisitor = new IdfPlusExpVisitor(null);
                BooleanExpression booleanExpression = (BooleanExpression) expVisitor.Visit(expressionTree);
                return booleanExpression.Value;
            }

            void True(string test) => Assert.IsTrue(GetBooleanExpression(test));
            void False(string test) => Assert.IsFalse(GetBooleanExpression(test));

            True("2 == 2");
            False("2 == 3");

            True("2 != 3");
            False("2 != 2");

            True("'Test1' == 'Test1'");
            False("'Test1' == 'Test2'");

            False("'Test1' != 'Test1'");
            True("'Test1' != 'Test2'");

            True("1 < 2");
            False("3 < 2");
            False("2 < 2");

            True("2 > 1");
            False("2 > 3");
            False("2 > 2");

            True("1 <= 2");
            False("3 <= 2");
            True("2 <= 2");

            False("1 >= 2");
            True("3 >= 2");
            True("2 >= 2");
        }

        [Test]
        public void TestBooleanLiteral()
        {
            string test_filepath = Path.Combine(TestDir.Dir, "boolean_literal_test.bemp");
            string file = File.ReadAllText(test_filepath);

            var visitor = new IdfPlusVisitor(null);

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Console.WriteLine(output);
        }

        [Test]
        public void TestLetBindings()
        {
             string test_filepath = Path.Combine(TestDir.Dir, "test_let_bindings.bemp");
             string file = File.ReadAllText(test_filepath);

             var visitor = new IdfPlusVisitor(TestDir.Dir);

             var parser = file.ToParser();

             var tree = parser.idf();

             string output = visitor.Visit(tree);

             Assert.AreEqual("Version,1;\n\n", output);
        }
    }
}