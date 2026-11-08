using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace XmlComplex.Tests
{
    [TestClass]
    public class CommandArgumentsTests
    {
        [TestMethod]
        public void Parse_SetsInputOutputAndFlags()
        {
            var args = CommandArguments.Parse(new[] { "base.xml", "-o=out.xml", "--no-indent", "--indent=\t" });

            Assert.AreEqual(1, args.InputFiles.Count);
            Assert.AreEqual("base.xml", args.InputFiles[0]);
            Assert.AreEqual("out.xml", args.OutputPath);
            Assert.IsFalse(args.Indent);
            Assert.AreEqual("\t", args.IndentChars);
        }

        [TestMethod]
        public void HasRequiredArguments_ReturnsTrueWhenInputAndOutputExist()
        {
            var args = CommandArguments.Parse(new[] { "base.xml", "-o=out.xml" });

            Assert.IsTrue(args.HasRequiredArguments());
        }

        [TestMethod]
        public void HasRequiredArguments_ReturnsFalseWhenHelpOptionExists()
        {
            var args = CommandArguments.Parse(new[] { "base.xml", "-o=out.xml", "--help" });

            Assert.IsFalse(args.HasRequiredArguments());
        }

        [TestMethod]
        public void TryGetNewLine_StoresResolvedNewLineChars()
        {
            var args = CommandArguments.Parse(new[] { "base.xml", "-o=out.xml", "--newline=CRLF" });

            var result = args.TryGetNewLine();

            Assert.IsTrue(result);
            Assert.AreEqual("\r\n", args.NewLineChars);
        }

        [TestMethod]
        public void TryGetEncoding_StoresResolvedEncoding()
        {
            var args = CommandArguments.Parse(new[] { "base.xml", "-o=out.xml", "--encoding=utf-8bom" });

            var result = args.TryGetEncoding(out var exitCode, out var showEncodings);

            Assert.IsTrue(result);
            Assert.AreEqual(0, exitCode);
            Assert.IsFalse(showEncodings);
            Assert.IsNotNull(args.Encoding);
            Assert.AreEqual(65001, args.Encoding.CodePage);
            Assert.IsTrue(args.HasEncodingOption);
        }

        [TestMethod]
        public void HasAllInputFiles_ReturnsFalseWhenFileDoesNotExist()
        {
            var args = CommandArguments.Parse(new[] { "not-found.xml", "-o=out.xml" });

            Assert.IsFalse(args.HasAllInputFiles());
        }

        [TestMethod]
        public void HasAllInputFiles_ReturnsTrueWhenAllFilesExist()
        {
            var temp = Path.GetTempFileName();
            try
            {
                var args = CommandArguments.Parse(new[] { temp, "-o=out.xml" });

                Assert.IsTrue(args.HasAllInputFiles());
            }
            finally
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
        }
    }
}
