using System;
using System.IO;
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
            string file = File.ReadAllText("/home/mitch/repos/idf-plus/test/test_files/test_1.idfplus");
            var visitor = new IdfPlusVisitor();

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Assert.AreEqual("Version,9.2;\n", output);
        }

        [Test]
        public void TestRecursiveFunction()
        {
            string file = "factorial = \\ n { if n == 0 then 1 else n * factorial(n - 1)  }\nVersion,< factorial(4) >;\n";
            var visitor = new IdfPlusVisitor();

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Assert.AreEqual("Version,24;\n\n", output);
        }

        [Test]
        public void TestFileWithMathFunction()
        {
            string file = "myangle = ceiling(2.1)\nVersion,< myangle >;\n";
            var visitor = new IdfPlusVisitor();

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Console.WriteLine(output);
            Assert.AreEqual("Version,3;\n\n", output);
        }

        [Test]
        public void TestMapFunction()
        {
            string file = "list = ['9.1', '9.2']\nprint map(\\x {\nVersion,<x>;\n}\n, list) ";

            var visitor = new IdfPlusVisitor();

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Console.WriteLine(output);
            Assert.AreEqual("Version,9.1;\nVersion,9.2;\n", output);
        }

        [Test]
        public void TestMemberAccess()
        {
            var file = File.ReadAllText(Path.Combine(TestDir.Dir, "member_access.idfplus"));
            var visitor = new IdfPlusVisitor();

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Assert.AreEqual("Version,9.1;\nVersion,9.2;\n", output);
        }

        [Test]
        public void TestListOfListInReplacement()
        {
            var filepath = Path.Combine(TestDir.Dir, "test_nested_list_replacement.bemp");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor();
            var parser = file.ToParser();
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.AreEqual("Version,1,2,Hello,3,4,There;\n\n", output);
        }

        [Test]
        public void TestFilterFunction()
        {
             var filepath = Path.Combine(TestDir.Dir, "test_filter_function.bemp");
             var file = File.ReadAllText(filepath);
             var visitor = new IdfPlusVisitor();
             var parser = file.ToParser();
             var tree = parser.idf();
             string output = visitor.Visit(tree);

             Console.WriteLine(output);
             Assert.AreEqual("Version,1,2;\n\n", output);
        }

        [Test]
        public void TestQualifiedImport()
        {
              var filepath = Path.Combine(TestDir.Dir, "import_test/in2.bemp");
              var file = File.ReadAllText(filepath);
              var visitor = new IdfPlusVisitor(TestDir.Dir + "/import_test");
              var parser = file.ToParser();
              var tree = parser.idf();
              string output = visitor.Visit(tree);

              Console.WriteLine(output);
              Assert.AreEqual("Version,9.2;\n\nZoneAirHeatBalanceAlgorithm,EulerMethod;\n", output);
        }

        [Test]
        public void TestInlineDataTable()
        {
            var filepath = Path.Combine(TestDir.Dir, "inline_data_table_test.bemp");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor();
            var parser = file.ToParser();
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Console.WriteLine(output);
        }

        [Test]
        public void TestObjectFormatting()
        {
            var filepath = Path.Combine(TestDir.Dir, "object_format_test.bemp");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor();
            var parser = file.ToParser();
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Console.WriteLine(output);
        }
    }
}