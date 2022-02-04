using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Text;

namespace XmlComplex.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void GetNewLineCode_ReturnsExpectedValueForKnownKinds()
        {
            Assert.AreEqual("\r", InvokeGetNewLineCode("CR"));
            Assert.AreEqual("\n", InvokeGetNewLineCode("LF"));
            Assert.AreEqual("\r\n", InvokeGetNewLineCode("CRLF"));
        }

        [TestMethod]
        public void GetNewLineCode_ReturnsNullForUnknownKinds()
        {
            Assert.IsNull(InvokeGetNewLineCode("invalid"));
        }

        [TestMethod]
        public void GetEncoding_ParsesUtf8BomAndNoBom()
        {
            var withBom = InvokeGetEncoding("utf-8bom") as UTF8Encoding;
            var noBom = InvokeGetEncoding("utf8nobom") as UTF8Encoding;

            Assert.IsNotNull(withBom);
            Assert.IsNotNull(noBom);
            Assert.IsTrue(withBom.GetPreamble().Length > 0);
            Assert.AreEqual(0, noBom.GetPreamble().Length);
        }

        private static string InvokeGetNewLineCode(string value)
        {
            var method = typeof(Program).GetMethod("GetNewLineCode", BindingFlags.NonPublic | BindingFlags.Static);
            return (string)method.Invoke(null, new object[] { value });
        }

        private static Encoding InvokeGetEncoding(string value)
        {
            var method = typeof(Program).GetMethod("GetEncoding", BindingFlags.NonPublic | BindingFlags.Static);
            return (Encoding)method.Invoke(null, new object[] { value });
        }
    }
}
