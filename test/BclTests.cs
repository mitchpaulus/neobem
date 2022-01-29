using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class BclTests
    {

        [Test]
        public void TestBclRead()
        {
            string uuid = "908accf0-5ea7-0130-b19d-14109fdf0b37";
            Bcl bcl = new Bcl();
            var response = bcl.GetByUUID(uuid);
            Console.Write(response);
        }

        [Test]
        public void TestBclParsing()
        {
            string uuid = "908accf0-5ea7-0130-b19d-14109fdf0b37";
            Bcl bcl = new Bcl();
            var response = bcl.GetByUUID(uuid);

            var structure = bcl.ParseUUIDResponse(response);

            Console.WriteLine(structure.AsErrorString());
        }

        [Test]
        public void TestBclFile()
        {
            string file = File.ReadAllText(Path.Combine(TestDir.Dir, "import_test", "import_bcl.nbem" ));
            var visitor = new IdfPlusVisitor(TestDir.Dir);

            var parser = file.ToParser();

            var tree = parser.idf();

            string output = visitor.Visit(tree);

            string expected = File.ReadAllText(Path.Combine(TestDir.Dir, "import_test", "import_bcl_expected.nbem"));

            Assert.IsTrue(IdfObjectCompare.Equals(expected, output));
        }


        [Test]

        public void TestCoffeeMaker()
        {
            // This test will check that a component without files defined works. Also double checks the example in the doc.
             string file = File.ReadAllText(Path.Combine(TestDir.Dir, "coffee_maker.nbem"));
             var visitor = new IdfPlusVisitor(TestDir.Dir);
             var parser = file.ToParser();
             var tree = parser.idf();
             string output = visitor.Visit(tree);

             string expected = File.ReadAllText(Path.Combine(TestDir.Dir,  "coffee_maker_expected.idf"));

             Assert.IsTrue(IdfObjectCompare.Equals(expected, output));
        }
    }
}
