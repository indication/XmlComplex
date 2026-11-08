using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace XmlComplex.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void ResolveNewLine_ReturnsExpectedValueForKnownKinds()
        {
            Assert.AreEqual("\r", NewLineResolver.Resolve("CR"));
            Assert.AreEqual("\n", NewLineResolver.Resolve("LF"));
            Assert.AreEqual("\r\n", NewLineResolver.Resolve("CRLF"));
        }

        [TestMethod]
        public void ResolveNewLine_IsCaseInsensitive()
        {
            Assert.AreEqual("\r\n", NewLineResolver.Resolve("crlf"));
        }

        [TestMethod]
        public void ResolveNewLine_ReturnsNullForUnknownKinds()
        {
            Assert.IsNull(NewLineResolver.Resolve("invalid"));
        }

        [TestMethod]
        public void ResolveEncoding_ParsesUtf8BomAndNoBom()
        {
            var withBom = EncodingResolver.Resolve("utf-8bom") as UTF8Encoding;
            var noBom = EncodingResolver.Resolve("utf8nobom") as UTF8Encoding;

            Assert.IsNotNull(withBom);
            Assert.IsNotNull(noBom);
            Assert.IsTrue(withBom.GetPreamble().Length > 0);
            Assert.AreEqual(0, noBom.GetPreamble().Length);
        }

        [TestMethod]
        public void ResolveEncoding_ParsesUtf16BigEndianWithBom()
        {
            var utf16 = EncodingResolver.Resolve("utf16bebom") as UnicodeEncoding;

            Assert.IsNotNull(utf16);
            Assert.IsTrue(utf16.GetPreamble().Length > 0);
            Assert.AreEqual(1201, utf16.CodePage);
        }

        [TestMethod]
        public void ResolveEncoding_ParsesUtf32LittleEndianWithoutBom()
        {
            var utf32 = EncodingResolver.Resolve("utf-32") as UTF32Encoding;

            Assert.IsNotNull(utf32);
            Assert.AreEqual(12000, utf32.CodePage);
            Assert.AreEqual(0, utf32.GetPreamble().Length);
        }

        [TestMethod]
        public void ResolveEncoding_UsesFrameworkEncodingForCodePageName()
        {
            var encoding = EncodingResolver.Resolve("shift_jis");

            Assert.AreEqual(932, encoding.CodePage);
        }
    }
}
