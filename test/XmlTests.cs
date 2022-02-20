using System.IO;
using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class XmlTests
    {
        [Test]
        public void TestXml()
        {
            XmlDataLoader loader = new();
            string file = File.ReadAllText(Path.Combine(TestDir.Dir, "load_test_files", "gbxml.xml"));
            loader.Load(file);
        }

        [Test]
        public void TestGBXML()
        {
            var filepath = Path.Combine(TestDir.Dir, "load_test_files" , "load_xml.nbem");
            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(Path.Combine( TestDir.Dir, "load_test_files"), FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            Assert.IsTrue(IdfObjectCompare.Equals("Site:Location,Boston MA, 42.21, -71.03,0.0,0.0;", output));
        }

        [Test]
        public void SpacesMap()
        {
            var filepath = Path.Combine(TestDir.Dir, "load_test_files" , "map_spaces.nbem");

            var file = File.ReadAllText(filepath);
            var visitor = new IdfPlusVisitor(Path.Combine( TestDir.Dir, "load_test_files"), FileType.Idf);
            var parser = file.ToParser(FileType.Idf);
            var tree = parser.idf();
            string output = visitor.Visit(tree);

            string expectedOutput =
                File.ReadAllText(Path.Combine(TestDir.Dir, "load_test_files", "map_spaces_expected.idf"));

            Assert.IsTrue(IdfObjectCompare.Equals(expectedOutput, output));
        }

        [Test]
        public void TestXMLLoadBehavior()
        {
            XmlDataLoader loader = new();
            string xmlWithoutDeclaration = "<element attribute=\"attribute value\">inner value</element>";
            var dictionary = loader.Load(xmlWithoutDeclaration);

            Assert.AreEqual(2, dictionary.Members.Count);
            Assert.AreEqual(new StringExpression("attribute value"), dictionary.Members["attribute"]);
            Assert.AreEqual(new StringExpression("inner value"), dictionary.Members["value"]);
        }

        [Test]
        public void TestValueBehavior()
        {
             XmlDataLoader loader = new();
             string xmlWithoutDeclaration = "<element>   inner  <br></br>  value   </element>";
             var dictionary = loader.Load(xmlWithoutDeclaration);

             Assert.AreEqual(2, dictionary.Members.Count);
             Assert.AreEqual(new StringExpression("inner value"), dictionary.Members["value"]);
        }
    }
}
