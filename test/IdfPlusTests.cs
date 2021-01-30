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

            Assert.AreEqual("Version,\n    9.2;\n", output);
        }

        [Test]
        public void TestRecursiveFunction()
        {
            string file = "factorial = \\ n { if n == 0 then 1 else n * factorial(n - 1)  }\nVersion,{ factorial(4) };\n";
            var visitor = new IdfPlusVisitor();

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Assert.AreEqual("Version,24;", output);
        }

        [Test]
        public void TestFileWithMathFunction()
        {
            string file = "myangle = ceiling(2.1)\nVersion,{ myangle };\n";
            var visitor = new IdfPlusVisitor();

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            Console.WriteLine(output);
            Assert.AreEqual("Version,3;", output);
        }

        [Test]
        public void TestMapFunction()
        {
            string file = "list = ['9.1', '9.2']\nprint map(\\x {\nVersion,{x};\n}\n, list) ";

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
    }
}