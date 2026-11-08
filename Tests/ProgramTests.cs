using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace XmlComplex.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void Main_ReturnsSuccess_WhenVersionOptionSpecified()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var input = Path.Combine(tempDir, "base.xml");
                File.WriteAllText(input, "<?xml version='1.0' encoding='utf-8'?><root><a /></root>");

                var exitCode = InvokeMain(new[] { input, "-o=out.xml", "--version" });

                Assert.AreEqual(CommandArgumentSuccessException.ExitCodeValue, exitCode);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void Main_ReturnsInvalidArguments_WhenRequiredArgumentsMissing()
        {
            var exitCode = InvokeMain(new string[0]);

            Assert.AreEqual(InvalidCommandArgumentException.ExitCodeValue, exitCode);
        }

        [TestMethod]
        public void Main_ReturnsMissingInputFile_WhenInputFileDoesNotExist()
        {
            var exitCode = InvokeMain(new[] { "not-found.xml", "-o=out.xml" });

            Assert.AreEqual(MissingInputFileCommandArgumentException.ExitCodeValue, exitCode);
        }

        [TestMethod]
        public void Main_ReturnsSuccess_WhenArgumentsAreValid()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var input = Path.Combine(tempDir, "base.xml");
                var output = Path.Combine(tempDir, "out.xml");
                File.WriteAllText(input, "<?xml version='1.0' encoding='utf-8'?><root><a /></root>");

                var exitCode = InvokeMain(new[] { input, "-o=" + output });

                Assert.AreEqual(CommandArgumentSuccessException.ExitCodeValue, exitCode);
                Assert.IsTrue(File.Exists(output));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        private static int InvokeMain(string[] args)
        {
            var method = typeof(Program).GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new InvalidOperationException("Program.Main was not found.");

            return (int)method.Invoke(null, new object[] { args });
        }
    }
}
