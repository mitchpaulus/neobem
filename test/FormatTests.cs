using System;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class FormatTests
    {
        [Test]
        public void SimpleVariableTest()
        {
            string input = "      my_var   =  2\n";
            string actual = input.Format();
            Assert.AreEqual("my_var = 2\n", actual);
        }

        [Test]
        public void SimpleLambdaTest()
        {
            string input = "   my_var        = \\ name     another { name  + 1 }\n";
            string actual = input.Format();
            Assert.AreEqual("my_var = λ name another { name + 1 }\n", actual);
        }

        [Test]
        public void MultiLineLambda() => AssertMatch("multi_line_lambda");

        [Test]
        public void NestedLambdaTest() => AssertMatch("nested_lambda_test");

        [Test]
        public void SingleLineIfTest() => AssertMatch("single_line_if_test");

        [Test]
        public void BooleanLiteral() => AssertMatch("boolean_literal");

        [Test]
        public void MultiLineIf() => AssertMatch("multi_line_if");

        [Test]
        public void Structure() => AssertMatch("structure");

        [Test]
        public void ListOfStructures() => AssertMatch("list_of_structures");

        [Test]
        public void ListOfLists() => AssertMatch("list_of_lists");

        [Test]
        public void Return() => AssertMatch("return");

        [Test]
        public void Print() => AssertMatch("print");

        [Test]
        public void MemberAccess() => AssertMatch("member_access");

        [Test]
        public void Polynomial() => AssertMatch("polynomial");

        [Test]
        public void InlineDataTable() => AssertMatch("inline_data_table");

        [Test]
        public void NestedInlineDataTableMessy() => AssertMatch("nested_inline_data_table_messy");

        [Test]
        public void LetBindings() => AssertMatch("let_bindings");

        [Test]
        public void SingleImport() => AssertMatch("single_import");

        [Test]
        public void Export() => AssertMatch("export");

        [Test]
        public void Comment() => AssertMatch("comment");

        [Test]
        public void Log() => AssertMatch("log");

        [Test]
        public void IdfComment() => AssertMatch("idf_comment");

        [Test]
        public void MultipleIdfObjectsInFunction() => AssertMatch("multiple_idf_objects_in_function");

        [Test]
        public void MapPipe() => AssertMatch("map_pipe");

        [Test]
        public void RangeOperator() => AssertMatch("range_operator");

        public void AssertMatch(string fileName)
        {
            string inputPath = Path.Join(TestDir.Dir, "formatting_tests", $"{fileName}.nbem");
            string input = File.ReadAllText(inputPath);
            AntlrInputStream inputStream = new(input);
            NeobemLexer lexer = new(inputStream);
            CommonTokenStream tokens = new(lexer);
            NeobemParser parser = new(tokens);
            NeobemParser.IdfContext idfTree = parser.idf();
            FormatVisitor visitor = new(0, 0, tokens);
            string actual = visitor.Visit(idfTree);
            string expected =
                File.ReadAllText(Path.Join(TestDir.Dir, "formatting_tests", $"{fileName}_expected.nbem"));
            Assert.AreEqual(expected, actual);
        }
    }
}