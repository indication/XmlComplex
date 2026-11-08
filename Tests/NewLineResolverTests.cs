using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmlComplex.Tests
{
    [TestClass]
    public class NewLineResolverTests
    {
        [TestMethod]
        public void Resolve_ReturnsExpectedValueForKnownKinds()
        {
            Assert.AreEqual("\r", NewLineResolver.Resolve("CR"));
            Assert.AreEqual("\n", NewLineResolver.Resolve("LF"));
            Assert.AreEqual("\r\n", NewLineResolver.Resolve("CRLF"));
        }

        [TestMethod]
        public void Resolve_IsCaseInsensitive()
        {
            Assert.AreEqual("\r\n", NewLineResolver.Resolve("crlf"));
        }

        [TestMethod]
        public void Resolve_ReturnsNullForUnknownKinds()
        {
            Assert.IsNull(NewLineResolver.Resolve("invalid"));
        }
    }
}
