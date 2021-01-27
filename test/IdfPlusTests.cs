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
    }
}