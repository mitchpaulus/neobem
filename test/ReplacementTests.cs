using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using src;

namespace test
{
    [TestFixture]
    public class ReplacementTests
    {

        [Test]
        public void TestBasicExpressionReplacement()
        {
            string objectText = "Version\n  < versionNum >;\n";

            ObjectVariableReplacer replacer = new ObjectVariableReplacer(TestDir.Dir);

            List<Dictionary<string, Expression>> variables = new List<Dictionary<string, Expression>>();
            variables.Add(new Dictionary<string, Expression>
            {
                { "versionNum",  new StringExpression("9.4") }
            });

            var replaced = replacer.Replace(objectText, variables, FileType.Idf);

            Assert.AreEqual("Version\n  9.4;\n", replaced);

        }

        [Test]
        public void TestOnlyReplacement()
        {
            // This test used to fail for index out of bounds exception
            const string objectText = "<num>";
            ObjectVariableReplacer replacer = new(TestDir.Dir);
            List<Dictionary<string, Expression>> variables = new()
            {
                new Dictionary<string, Expression>
                {
                    { "num",  new StringExpression("1") }
                }
            };
            var (actual, _) = replacer.Replace(objectText, variables, FileType.Idf);
            Assert.AreEqual("1", actual);
        }
    }
}
