using System.IO;
using System.Linq;
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
    }
}