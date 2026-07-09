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
              var visitor = new IdfPlusVisitor(Path.Combine(TestDir.Dir, "Excel"), FileType.Idf);
              var parser = file.ToParser(FileType.Idf);
              var tree = parser.idf();
              string output = visitor.Visit(tree);

              Console.WriteLine(output);
        }

        // End-to-end test through load + map for every branch a real .xlsx can produce:
        // first-worksheet default, named worksheet, full ('A1:F3') and start-cell ('A1')
        // ranges, the empty worksheet, and shared-string / number / boolean / string-formula
        // / date (numeric serial) / blank cells. See load_test_2.nbem for the layout.
        [Test]
        public void LoadExcel2GoldenTest()
        {
            string expectedOutput = File.ReadAllText(Path.Combine(TestDir.Dir, "Excel", "load_test_2_expected.idf"));
            IdfTester.TestIdfFile(Path.Combine(TestDir.Dir, "Excel", "load_test_2.nbem"), expectedOutput);
        }

        // Asserts the exact loaded types and values, including the error cell (whose '#DIV/0!'
        // text cannot round-trip through IDF because of the '!' comment character).
        [Test]
        public void LoadExcel2TypesAndErrorsTest()
        {
            string xlsx = Path.Combine(TestDir.Dir, "Excel", "load_test_2.xlsx");

            ListExpression data = ExcelDataLoader.Load(xlsx, null, new SheetDimensionRange());
            Assert.AreEqual(2, data.Expressions.Count);

            IdfPlusObjectExpression alpha = (IdfPlusObjectExpression) data.Expressions[0];
            Assert.AreEqual("Alpha", ((StringExpression) alpha.Members["name"]).Text);
            Assert.AreEqual(1.5, ((NumericExpression) alpha.Members["number"]).Value);
            Assert.IsTrue(((BooleanExpression) alpha.Members["flag"]).Value);
            Assert.AreEqual("xy", ((StringExpression) alpha.Members["calc"]).Text);
            // Error cells are sanitized of IDF-special characters when loaded, so the '!' is gone.
            Assert.AreEqual("#DIV/0", ((StringExpression) alpha.Members["err"]).Text);
            // Dates load as their underlying numeric (serial) value, not a formatted string.
            Assert.AreEqual(46037, ((NumericExpression) alpha.Members["when"]).Value);

            IdfPlusObjectExpression beta = (IdfPlusObjectExpression) data.Expressions[1];
            Assert.AreEqual(-2, ((NumericExpression) beta.Members["number"]).Value);
            Assert.IsFalse(((BooleanExpression) beta.Members["flag"]).Value);
            // D3 is blank in the sheet, so the cell comes through as an empty string.
            Assert.AreEqual("", ((StringExpression) beta.Members["calc"]).Text);
            Assert.AreEqual("#N/A", ((StringExpression) beta.Members["err"]).Text);
            Assert.AreEqual(46073, ((NumericExpression) beta.Members["when"]).Value);
        }

        [Test]
        public void LoadExcel2EmptySheetTest()
        {
            string xlsx = Path.Combine(TestDir.Dir, "Excel", "load_test_2.xlsx");
            ListExpression empty = ExcelDataLoader.Load(xlsx, "Empty", new SheetDimensionRange());
            Assert.IsEmpty(empty.Expressions);
        }

        [Test]
        public void LoadExcel2MissingSheetTest()
        {
            string xlsx = Path.Combine(TestDir.Dir, "Excel", "load_test_2.xlsx");
            ArgumentException ex = Assert.Throws<ArgumentException>(
                () => ExcelDataLoader.Load(xlsx, "Nope", new SheetDimensionRange()));
            Assert.That(ex.Message, Does.Contain("Types"));
            Assert.That(ex.Message, Does.Contain("Empty"));
        }

        [Test]
        public void LoadExcel2InvalidFileTest()
        {
            // A non-zip file (here, the .nbem itself) should surface a clear error rather than crash.
            string notXlsx = Path.Combine(TestDir.Dir, "Excel", "load_test_2.nbem");
            Assert.Throws<ArgumentException>(
                () => ExcelDataLoader.Load(notXlsx, null, new SheetDimensionRange()));
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
            string expectedOutputFilePath = Path.Combine(TestDir.LoadTestFiles, "load_json.expected");
            string expectedOutput = File.ReadAllText(expectedOutputFilePath);
            IdfTester.TestIdfFile(Path.Combine(TestDir.LoadTestFiles, "load_json.nbem"), expectedOutput);
        }

        [Test]
        public void TestNoHeaderOption()
        {
            string expectedOutputFilePath = Path.Combine(TestDir.Dir, "load_test_files", "no_header_data_expected.idf");
            string expectedOutput = File.ReadAllText(expectedOutputFilePath);
            IdfTester.TestIdfFile(Path.Combine(TestDir.Dir, "load_test_files", "no_header_data.nbem"), expectedOutput);
        }

        [Test]
        public void TestLoadingCSV()
        {
               string expectedOutputFilePath = Path.Combine(TestDir.Dir, "load_test_files", "csv_expected.idf");
               string expectedOutput = File.ReadAllText(expectedOutputFilePath);
               var filepath = Path.Combine(TestDir.Dir, "load_test_files", "csv.nbem");
               IdfTester.TestIdfFile(filepath, expectedOutput);
        }

        [Test]
        public void TestSkippingHeaderLines()
        {
            string expectedOutputFilePath = Path.Combine(TestDir.Dir, "load_test_files", "skip_test_data_expected.idf");
            string expectedOutput = File.ReadAllText(expectedOutputFilePath);
            string filepath = Path.Combine(TestDir.Dir, "load_test_files", "skip_test_data.nbem");
            IdfTester.TestIdfFile(filepath, expectedOutput);
        }
    }
}