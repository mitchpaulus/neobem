using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class ExtensionTests
    {

        [Test]
        public void TestSigFigs()
        {
            Assert.AreEqual("0", ((double)0).ToSigFigs(4));
            Assert.AreEqual("100", ((double)100).ToSigFigs(4));
            Assert.AreEqual("0.1234", 0.12341234.ToSigFigs(4));
            Assert.AreEqual("0.0000000005", 0.0000000005.ToSigFigs(4));
        }
    }
}
