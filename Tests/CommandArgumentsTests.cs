using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace XmlComplex.Tests
{
    [TestClass]
    public class CommandArgumentsTests
    {
        [TestMethod]
        public void Parse_SetsInputOutputAndFlags()
        {
            var temp = Path.GetTempFileName();
            try
            {
                var args = CommandArguments.Parse(new[] { temp, "-o=out.xml", "--no-indent", "--indent=\t" });

                Assert.AreEqual(1, args.InputFiles.Count);
                Assert.AreEqual(temp, args.InputFiles[0]);
                Assert.AreEqual("out.xml", args.OutputPath);
                Assert.IsFalse(args.Indent);
                Assert.AreEqual("\t", args.IndentChars);
            }
            finally
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
        }

        [TestMethod]
        public void Parse_ResolvesNewLineAndEncoding()
        {
            var temp = Path.GetTempFileName();
            try
            {
                var args = CommandArguments.Parse(new[] { temp, "-o=out.xml", "--newline=CRLF", "--encoding=utf-8bom" });

                Assert.AreEqual("\r\n", args.NewLineChars);
                Assert.IsNotNull(args.Encoding);
                Assert.AreEqual(65001, args.Encoding.CodePage);
                Assert.IsTrue(args.Encoding.GetPreamble().Length > 0);
            }
            finally
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
        }

        [TestMethod]
        public void Parse_ThrowsSuccessExitCode_WhenVersionOptionSpecified()
        {
            var temp = Path.GetTempFileName();
            try
            {
                var ex = AssertThrowsCommandArgumentException(() => CommandArguments.Parse(new[] { temp, "-o=out.xml", "--version" }));

                Assert.AreEqual(CommandArgumentSuccessException.ExitCodeValue, ex.ExitCode);
                Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
                Assert.IsTrue(ex.Message.Contains(Path.GetFileName(typeof(CommandArguments).Assembly.Location)));
            }
            finally
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
        }

        [TestMethod]
        public void Parse_ThrowsSuccessExitCode_WhenHelpOptionSpecified()
        {
            var ex = AssertThrowsCommandArgumentException(() => CommandArguments.Parse(new[] { "--help" }));

            Assert.AreEqual(CommandArgumentSuccessException.ExitCodeValue, ex.ExitCode);
        }

        [TestMethod]
        public void Parse_ThrowsSuccessExitCode_WhenEncodingsOptionSpecified()
        {
            var ex = AssertThrowsCommandArgumentException(() => CommandArguments.Parse(new[] { "--encodings" }));

            Assert.AreEqual(CommandArgumentSuccessException.ExitCodeValue, ex.ExitCode);
        }

        [TestMethod]
        public void Parse_ThrowsInvalidArgumentsExitCode_WhenRequiredArgumentsMissing()
        {
            var temp = Path.GetTempFileName();
            try
            {
                var ex = AssertThrowsCommandArgumentException(() => CommandArguments.Parse(new[] { temp }));

                Assert.AreEqual(InvalidCommandArgumentException.ExitCodeValue, ex.ExitCode);
            }
            finally
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
        }

        [TestMethod]
        public void Parse_ThrowsInvalidArgumentsExitCode_WhenNewLineIsInvalid()
        {
            var temp = Path.GetTempFileName();
            try
            {
                var ex = AssertThrowsCommandArgumentException(() => CommandArguments.Parse(new[] { temp, "-o=out.xml", "--newline=invalid" }));

                Assert.AreEqual(InvalidCommandArgumentException.ExitCodeValue, ex.ExitCode);
            }
            finally
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
        }

        [TestMethod]
        public void Parse_ThrowsMissingInputFileExitCode_WhenInputFileDoesNotExist()
        {
            var ex = AssertThrowsCommandArgumentException(() => CommandArguments.Parse(new[] { "not-found.xml", "-o=out.xml" }));

            Assert.AreEqual(MissingInputFileCommandArgumentException.ExitCodeValue, ex.ExitCode);
        }

        private static CommandArgumentException AssertThrowsCommandArgumentException(Action action)
        {
            try
            {
                action();
            }
            catch (CommandArgumentException ex)
            {
                return ex;
            }

            Assert.Fail("Expected CommandArgumentException was not thrown.");
            return null;
        }
    }
}
