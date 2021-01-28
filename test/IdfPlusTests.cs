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

    }
}