using NUnit.Framework;
using src;

namespace test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            bool success = VariableRegex.Regex.IsMatch("  $hello ");
        }
    }
}