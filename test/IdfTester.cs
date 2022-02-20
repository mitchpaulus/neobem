using System.IO;
using NUnit.Framework;
using src;

namespace test;

public class IdfTester
{
    public static void TestIdfFile(string fullFilePath, string expectedIdfOutput)
    {
        string directory = new FileInfo(fullFilePath).DirectoryName;
        TestIdfFromContents(File.ReadAllText(fullFilePath), expectedIdfOutput, directory);
    }

    public static void TestIdfFromContents(string contents, string expectedIdfOutput, string dir = null)
    {
        IdfPlusVisitor visitor = new(dir, FileType.Idf);
        NeobemParser parser = contents.ToParser(FileType.Idf);
        NeobemParser.IdfContext tree = parser.idf();
        string output = visitor.Visit(tree);
        Assert.IsTrue(IdfObjectCompare.Equals(expectedIdfOutput, output));
    }
}