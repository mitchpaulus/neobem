using System;
using System.IO;
using Antlr4.Runtime;
using NUnit.Framework;
using src;


namespace test
{
    [TestFixture]
    public class IdfPlusTests
    {

        [Test]
        public void TestBasicTemplate()
        {
            string file = File.ReadAllText(Path.Combine(TestDir.Dir, "test_1.idfplus"));
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);

            var parser = file.ToIdfParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,9.2;", output));
        }

        [Test]
        public void TestRecursiveFunction()
        {
            string file = "factorial = \\ n { if n == 0 then 1 else n * factorial(n - 1)  }\nVersion,< factorial(4) >;\n";
            var visitor = new IdfPlusVisitor(null, FileType.Idf);

            var parser = file.ToIdfParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Assert.IsTrue( IdfObjectCompare.Equals("Version,24;\n\n", output));
        }

        [Test]
        public void TestFileWithMathFunction()
        {
            string file = "myangle = ceiling(2.1)\nVersion,< myangle >;\n";
            var visitor = new IdfPlusVisitor(null, FileType.Idf);

            var parser = file.ToIdfParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Console.WriteLine(output);
            Assert.IsTrue(IdfObjectCompare.Equals("Version,3;\n\n", output));
        }

        [Test]
        public void TestMapFunction()
        {
            string file = "list = ['9.1', '9.2']\nprint map(list, \\x {\nVersion,<x>;\n}\n) ";

            var visitor = new IdfPlusVisitor(null, FileType.Idf);

            var parser = file.ToIdfParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Console.WriteLine(output);
            Assert.IsTrue(IdfObjectCompare.Equals("Version,9.1;\nVersion,9.2;\n", output));
        }

        [Test]
        public void TestMemberAccess()
        {
            var file = File.ReadAllText(Path.Combine(TestDir.Dir, "member_access.idfplus"));
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);

            var parser = file.ToParser(FileType.Idf);

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,9.1;\nVersion,9.2;\n", output));
        }

        [Test]
        public void TestNestedMemberAccess()
        {
            var file = File.ReadAllText(Path.Combine(TestDir.Dir, "nested_member_access.nbem"));
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,9.4,9.3;", output));
        }

        [Test]
        public void TestNestedMemberAddField()
        {
            var file = File.ReadAllText(Path.Combine(TestDir.Dir, "nested_member_access_add_field.nbem"));
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,9.4,9.3,9.5;", output));
        }

        [Test]
        public void TestListOfListInReplacement()
        {
            var filepath = Path.Combine(TestDir.Dir, "test_nested_list_replacement.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,1,2,Hello,3,4,There;\n\n", output));
        }

        [Test]
        public void TestFilterFunction()
        {
             var filepath = Path.Combine(TestDir.Dir, "test_filter_function.nbem");
             var file = File.ReadAllText(filepath);
             var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
             var parser = file.ToParser(FileType.Idf);
             var tree = parser.idf();
             string output = visitor.Visit(tree);

             Console.WriteLine(output);
             Assert.IsTrue(IdfObjectCompare.Equals("Version,1,2;\n\n", output));
        }

        [Test]
        public void TestQualifiedImport()
        {
              var filepath = Path.Combine(TestDir.Dir, "import_test/in2.nbem");
              var file = File.ReadAllText(filepath);
              var visitor = new IdfPlusVisitor(Path.Combine(TestDir.Dir, "import_test"), FileType.Idf);
              var parser = file.ToParser(FileType.Idf);
              var tree = parser.idf();
              string output = visitor.Visit(tree);

              Console.WriteLine(output);
              Assert.IsTrue( IdfObjectCompare.Equals("Version,9.2;ZoneAirHeatBalanceAlgorithm,EulerMethod;\n", output));
        }

        [Test]
        public void TestImportFromExpression()
        {
              var filepath = Path.Combine(TestDir.Dir, "import_test/import_expression.nbem");
              var file = File.ReadAllText(filepath);
              var visitor = new IdfPlusVisitor(Path.Combine(TestDir.Dir, "import_test"), FileType.Idf);
              var parser = file.ToParser(FileType.Idf);
              var tree = parser.idf();
              string output = visitor.Visit(tree);

              Assert.IsTrue(IdfObjectCompare.Equals("Version,9.2;", output));
        }

        [Test]
        public void TestInlineDataTable()
        {
            var filepath = Path.Combine(TestDir.Dir, "inline_data_table_test.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Console.WriteLine(output);
        }

        [Test]
        public void TestInlineDataTableOverline()
        {
            var filepath = Path.Combine(TestDir.Dir, "inline_data_table_test.nbem");

            var file = File.ReadAllText(filepath);
            AntlrInputStream inputStream = new AntlrInputStream(file);
            NeobemLexer lexer = new NeobemLexer(inputStream);
            var errorListener = new SimpleAntlrErrorListener();
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(errorListener);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            var parser = new NeobemParser(tokens);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var tree = parser.idf();

            Assert.AreEqual(0, errorListener.Errors.Count, "There should be no errors in lexing or parsing.");

            string output = visitor.Visit(tree);
        }

        [Test]
        public void TestObjectFormatting()
        {
            var filepath = Path.Combine(TestDir.Dir, "object_format_test.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Console.WriteLine(output);
        }

        [Test]
        public void TestEmojiSupport()
        {
            var filepath = Path.Combine(TestDir.Dir, "emoji_key_support_test.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,Neutral Face;", output));
        }

        [Test]
        public void TestTrailingComma()
        {
            var filepath = Path.Combine(TestDir.Dir, "test_trailing_comma.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,1,2,3;", output));
        }

        [Test]
        public void TestAlternateTerminator()
        {
            var filepath = Path.Combine(TestDir.Dir, "alternate_terminator.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,1,2,3,4,5,6;", output));
        }

        [Test]
        public void TestExists()
        {
            var filepath = Path.Combine(TestDir.Dir, "exists.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,2;", output));
        }

        [Test]
        public void TestLogicAndShortCircuit()
        {
            var filepath = Path.Combine(TestDir.Dir, "logic.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir, FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Version,1,0,0,0,1,1,1,0;", output));
        }
    }
}
