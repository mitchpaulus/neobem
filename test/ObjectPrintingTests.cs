using System;
using System.IO;
using src;
using NUnit.Framework;

namespace test
{
    [TestFixture]
    public class ObjectPrintingTests
    {
        [Test]
        public void PrettyPrintTest()
        {
            var filepath = Path.Combine(TestDir.Dir, "object_pretty_print.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(TestDir.Dir);
            var parser = file.ToParser();
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Console.WriteLine(output);

        }

        [Test]
        public void SplittingTest()
        {

            string test = "  , ";

            var split = test.Split(',');

            test = ",";
            split = test.Split(',');

            test = "line 1\nline2\n";
            split = test.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            var newSplit = test.SplitLines();
        }
    }
}