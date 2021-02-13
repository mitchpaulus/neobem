using System;
using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class TestSanitizeIdentifier
    {

        [Test]
        public void TestIdentifierSanitation()
        {
            string test = "  spaces   ";

            Assert.AreEqual("spaces", test.SanitizeIdentifier());

            // Test that first character gets made into lowercase
            test = "   Spaces   ";
            Assert.AreEqual("spaces", test.SanitizeIdentifier());

            test = "Temp (°F)";
            Assert.AreEqual("temp_F", test.SanitizeIdentifier());

            test = "190";
            Assert.Throws<ArgumentException>(() => test.SanitizeIdentifier());
        }
    }
}