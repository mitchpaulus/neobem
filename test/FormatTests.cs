using System;
using System.IO;
using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class FormatTests
    {
        public FormatVisitor FormatVisitor = new(0, 0);

        [Test]
        public void SimpleVariableTest()
        {
            string input = "      my_var   =  2\n";
            string actual = FormatVisitor.Visit(input.ToIdfTree());
            Assert.AreEqual("my_var = 2\n", actual);
        }

        [Test]
        public void SimpleLambdaTest()
        {
            string input = "   my_var        = \\ name     another { name  + 1 }\n";
            string actual = FormatVisitor.Visit(input.ToIdfTree());
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
        public void LetBindings() => AssertMatch("let_bindings");

        public void AssertMatch(string fileName)
        {
            string inputPath = Path.Join(TestDir.Dir, "formatting_tests", $"{fileName}.nbem");
            string input = File.ReadAllText(inputPath);
            string actual = FormatVisitor.Visit(input.ToIdfTree());
            string expected =
                File.ReadAllText(Path.Join(TestDir.Dir, "formatting_tests", $"{fileName}_expected.nbem"));
            Assert.AreEqual(expected, actual);
        }
    }
}