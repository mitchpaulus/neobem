using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class StringEscapeTests
    {
        [Test]
        public void TestBasicEscapes()
        {
            string input = "a\\tb";
            Assert.AreEqual("a\tb", StringEscape.Escape(input));
        }
    }
}