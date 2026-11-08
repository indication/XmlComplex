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
        public void Main_ReturnsSuccessAndWritesVersion_WhenVersionOptionSpecified()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var input = Path.Combine(tempDir, "base.xml");
                File.WriteAllText(input, "<?xml version='1.0' encoding='utf-8'?><root><a /></root>");

                var exitCode = InvokeMainWithOutput(new[] { input, "-o=out.xml", "--version" }, out var output);

                Assert.AreEqual(CommandArgumentSuccessException.ExitCodeValue, exitCode);
                Assert.IsFalse(string.IsNullOrEmpty(output));
                Assert.IsTrue(output.Contains(Path.GetFileName(typeof(Program).Assembly.Location)));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void Main_ReturnsInvalidArgumentsAndWritesHelp_WhenRequiredArgumentsMissing()
        {
            var exitCode = InvokeMainWithOutput(new string[0], out var output);

            Assert.AreEqual(InvalidCommandArgumentException.ExitCodeValue, exitCode);
            Assert.IsFalse(string.IsNullOrEmpty(output));
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

        private static int InvokeMainWithOutput(string[] args, out string output)
        {
            var originalOut = Console.Out;
            var writer = new StringWriter();
            try
            {
                Console.SetOut(writer);
                var exitCode = InvokeMain(args);
                output = writer.ToString();
                return exitCode;
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
