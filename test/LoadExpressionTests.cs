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
            Assert.IsTrue(((IdfPlusObjectExpression)output.Expressions[0]).Members["x_origin"] is NumericExpression);

            Assert.AreEqual(((StringExpression)((IdfPlusObjectExpression) output.Expressions[0]).Members["name"]).Text, "Zone 1");
            IdfPlusObjectExpression secondExpression = (IdfPlusObjectExpression) output.Expressions[1];
            Assert.AreEqual(((NumericExpression) secondExpression.Members["x_origin"]).Value,  56);
        }

        [Test]
        public void LoadExcelStartCellTest()
        {
              var filepath = Path.Combine(TestDir.Dir, "Excel", "load_excel_test_1.bemp");
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
    }
}