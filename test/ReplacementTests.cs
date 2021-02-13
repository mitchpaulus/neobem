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

            var replaced = replacer.Replace(objectText, variables);

            Assert.AreEqual("Version\n  9.4;\n", replaced);

        }

    }
}