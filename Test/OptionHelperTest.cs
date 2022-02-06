using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using XmlComplex;

namespace Test
{
    [TestClass]
    public class OptionHelperTest
    {
        [TestMethod]
        public void OptionTranscodeToLongName()
        {
            var helper = new OptionHelper();
            helper.AddOption("t", "test", "Test Name", "This is test");
            var dic = helper.GetOptions(new[] { "-t" });
            Assert.IsTrue(dic.ContainsKey("test"));
        }
        [TestMethod]
        public void OptionLongName()
        {
            var helper = new OptionHelper();
            helper.AddOption("t", "test", "Test Name", "This is test");
            var dic = helper.GetOptions(new[] { "-test" });
            Assert.IsTrue(dic.ContainsKey("test"));
        }
    }
}
