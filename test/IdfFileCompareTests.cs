using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class IdfFileCompareTests
    {

        [Test]
        public void CompareTest()
        {
            string idf1 = "Version,9.1;";

            string idf2 = "  Version\n,\n    9.1;  ";

            Assert.IsTrue(IdfObjectCompare.Equals(idf1, idf2));
        }

        [Test]
        public void OrderingTest()
        {
            string idf1 = "Version,9.1;Other,10.1;";

            string idf2 = "Other,10.1;Version,9.1;";

            Assert.IsFalse(IdfObjectCompare.Equals(idf1, idf2));
        }

        [Test]
        public void DifferentLengthTest()
        {
            string idf1 = "Version,9.1;";
            string idf2 = "Version,9.1;Second,9.2;";

            Assert.IsFalse(IdfObjectCompare.Equals(idf1, idf2));
        }

    }
}