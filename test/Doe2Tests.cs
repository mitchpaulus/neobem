using System.IO;
using Antlr4.Runtime;
using NUnit.Framework;
using src;

namespace test;

[TestFixture]
public class Doe2Tests
{
    [Test]
    public void ParseTest()
    {
        // This is a sample from the documentation, make sure it parses fine as is.
        AntlrFileStream fileStream = new(Path.Combine(TestDir.Dir, "DOE2", "doe2sample.bdl"));
        NeobemLexer lexer = new(fileStream)
        {
            FileType = FileType.Doe2
        };

        CommonTokenStream commonTokenStream = new(lexer);
        NeobemParser parser = new(commonTokenStream);
        NeobemParser.IdfContext tree = parser.idf();
        IdfPlusVisitor visitor = new(null, FileType.Doe2);
        Assert.DoesNotThrow(() => { visitor.Visit(tree); });
    }

    [Test]
    public void TestDoe2TemplateExample()
    {
        string inputFilePath          = Path.Combine(TestDir.Dir, "DOE2", "doe2example.nbem");
        string inputContents          = File.ReadAllText(inputFilePath);

        string expectedOutputFilePath = Path.Combine(TestDir.Dir, "DOE2", "doe2example_expected.bdl");
        string expectedOutputContents = File.ReadAllText(expectedOutputFilePath);

        IdfPlusVisitor visitor = new(new FileInfo(inputFilePath).DirectoryName , FileType.Doe2);
        NeobemParser parser = inputContents.ToParser(FileType.Doe2);
        NeobemParser.IdfContext tree = parser.idf();
        string output = visitor.Visit(tree);

        Assert.AreEqual(expectedOutputContents, output);
    }

    [Test]
    public void TestDoe2Importing()
    {
        // This test is important to verify that when you import from a file being parsed
         string inputFilePath          = Path.Combine(TestDir.Dir, "DOE2", "import_doe2_test.nbem");
         string inputContents          = File.ReadAllText(inputFilePath);

         string expectedOutputFilePath = Path.Combine(TestDir.Dir, "DOE2", "import_doe2_test_expected.bdl");
         string expectedOutputContents = File.ReadAllText(expectedOutputFilePath);

         IdfPlusVisitor visitor = new(new FileInfo(inputFilePath).DirectoryName , FileType.Doe2);
         NeobemParser parser = inputContents.ToParser(FileType.Doe2);
         NeobemParser.IdfContext tree = parser.idf();
         string output = visitor.Visit(tree);

         Assert.AreEqual(expectedOutputContents, output);       // as DOE-2, that those subsequent files imported are also parsed as DOE-2.
    }
}