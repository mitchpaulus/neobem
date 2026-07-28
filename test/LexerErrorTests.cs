using System.IO;
using NUnit.Framework;

namespace test
{
    public class LexerErrorTests
    {
        [Test]
        public void TestLexerErrorReturnsNonZeroExitCode()
        {
            // The '_ ' after the lambda backslash is not a valid token.
            string path = Path.Combine(Path.GetTempPath(), "lexer_error_test.nbem");
            File.WriteAllText(path, "bad = \\ _ {\n    Version,1;\n}\n\nprint bad()\n");

            int exitCode = src.Program.Main(new[] { path });

            Assert.AreEqual(1, exitCode);
        }

        [Test]
        public void TestValidFileReturnsZeroExitCode()
        {
            string path = Path.Combine(Path.GetTempPath(), "lexer_valid_test.nbem");
            File.WriteAllText(path, "good = \\ x {\n    Version,1;\n}\n\nprint good(1)\n");

            int exitCode = src.Program.Main(new[] { path });

            Assert.AreEqual(0, exitCode);
        }
    }
}
