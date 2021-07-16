using System;
using System.IO;
using System.Linq;
using Antlr4.Runtime.Tree;
using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class LoadExpressionTests
    {

        [Test]
        public void TestDelimitedFileReader()
        {
            string testFilePath = Path.Combine(TestDir.Dir, "load_test_files/data.txt");

            string contents = File.ReadAllText(testFilePath);

            DelimitedFileReader reader = new DelimitedFileReader();
            var output = reader.ReadFile(contents);

            Assert.IsTrue(output.Expressions.Count == 2);

            Assert.IsTrue(output.Expressions[0] is IdfPlusObjectExpression);
            Assert.IsTrue(output.Expressions[1] is IdfPlusObjectExpression);

            Assert.IsTrue(((IdfPlusObjectExpression)output.Expressions[0]).Members["name"] is StringExpression);
            Assert.IsTrue(((IdfPlusObjectExpression)output.Expressions[0]).Members["x origin"] is NumericExpression);

            Assert.AreEqual(((StringExpression)((IdfPlusObjectExpression) output.Expressions[0]).Members["name"]).Text, "Zone 1");
            IdfPlusObjectExpression secondExpression = (IdfPlusObjectExpression) output.Expressions[1];
            Assert.AreEqual(((NumericExpression) secondExpression.Members["x origin"]).Value,  56);
        }

        [Test]
        public void LoadExcelStartCellTest()
        {
              var filepath = Path.Combine(TestDir.Dir, "Excel", "load_excel_test_1.nbem");
              var file = File.ReadAllText(filepath);
              var visitor = new IdfPlusVisitor(Path.Combine(TestDir.Dir, "Excel"));
              var parser = file.ToParser();
              var tree = parser.idf();
              string output = visitor.Visit(tree);

              Console.WriteLine(output);
        }

        [Test]
        public void TestRangeSyntax()
        {
            string test = "A1:B2";

            var parser = test.ToExcelRangeParser();

            ExcelRangeParser.RangeContext tree = parser.range();
            MyExcelRangeListener listener = new MyExcelRangeListener();
            ParseTreeWalker walker = new ParseTreeWalker();
            walker.Walk(listener, tree);
            var range = listener.ExcelRange;
        }

        [Test]
        public void LoadJSONTest()
        {
            string json = "{ \"integer\" : 10, \"list\": [ true, false, null, 10.121231 ], \"string\": \"2020-01-02\" }";
            var jsonLoader = new JsonDataLoader();
            var output = jsonLoader.Load(json);

            if (output is IdfPlusObjectExpression objectExpression)
            {
                Assert.IsTrue(objectExpression.Members["integer"] is NumericExpression);

                if (objectExpression.Members["list"] is ListExpression listExpression)
                {
                    Assert.IsTrue(listExpression.Expressions[0] is BooleanExpression);
                    Assert.IsTrue(listExpression.Expressions[1] is BooleanExpression);
                    Assert.IsTrue(listExpression.Expressions[2] is StringExpression);
                    Assert.IsTrue(listExpression.Expressions[3] is NumericExpression);
                }
                else
                {
                    // Yes, this is redundant, but want to fail unit test with assert.
                    Assert.IsTrue(objectExpression.Members["list"] is ListExpression);
                }

                Assert.IsTrue(objectExpression.Members["string"] is StringExpression);

            }
            else
            {
                // Yes, this is redundant, but want to fail unit test with assert.
                Assert.IsTrue(output is IdfPlusObjectExpression);
            }
        }

        [Test]
        public void LoadJSONNbemTest()
        {
              var filepath = Path.Combine(TestDir.LoadTestFiles, "load_json.nbem");
              var file = File.ReadAllText(filepath);
              var visitor = new IdfPlusVisitor(Path.Combine(TestDir.Dir, "load_test_files"));
              var parser = file.ToParser();
              var tree = parser.idf();
              string output = visitor.Visit(tree);

              string expectedOutputFilePath = Path.Combine(TestDir.LoadTestFiles, "load_json.expected");
              string expectedOutput = File.ReadAllText(expectedOutputFilePath);

              Assert.IsTrue(IdfObjectCompare.Equals(expectedOutput, output));
        }

        [Test]
        public void TestNoHeaderOption()
        {
               var filepath = Path.Combine(TestDir.Dir, "load_test_files", "no_header_data.nbem");
               var file = File.ReadAllText(filepath);
               var visitor = new IdfPlusVisitor(Path.Combine(TestDir.Dir, "load_test_files"));
               var parser = file.ToParser();
               var tree = parser.idf();
               string output = visitor.Visit(tree);

               string expectedOutputFilePath = Path.Combine(TestDir.Dir, "load_test_files", "no_header_data_expected.idf");
               string expectedOutput = File.ReadAllText(expectedOutputFilePath);

               Assert.IsTrue(IdfObjectCompare.Equals(expectedOutput, output));
        }

        [Test]
        public void TestLoadingCSV()
        {
               var filepath = Path.Combine(TestDir.Dir, "load_test_files", "csv.nbem");
               var file = File.ReadAllText(filepath);
               var visitor = new IdfPlusVisitor(Path.Combine(TestDir.Dir, "load_test_files"));
               var parser = file.ToParser();
               var tree = parser.idf();
               string output = visitor.Visit(tree);

               string expectedOutputFilePath = Path.Combine(TestDir.Dir, "load_test_files", "csv_expected.idf");
               string expectedOutput = File.ReadAllText(expectedOutputFilePath);

               Assert.IsTrue(IdfObjectCompare.Equals(expectedOutput, output));
        }

        [Test]
        public void TestSkippingHeaderLines()
        {
               var filepath = Path.Combine(TestDir.Dir, "load_test_files", "skip_test_data.nbem");
               var file = File.ReadAllText(filepath);
               var visitor = new IdfPlusVisitor(Path.Combine(TestDir.Dir, "load_test_files"));
               var parser = file.ToParser();
               var tree = parser.idf();
               string output = visitor.Visit(tree);

               string expectedOutputFilePath = Path.Combine(TestDir.Dir, "load_test_files", "skip_test_data_expected.idf");
               string expectedOutput = File.ReadAllText(expectedOutputFilePath);

               Assert.IsTrue(IdfObjectCompare.Equals(expectedOutput, output));
        }
    }
}