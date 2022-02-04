using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace XmlComplex.Tests
{
    [TestClass]
    public class OptionHelperTests
    {
        [TestMethod]
        public void GetOptions_ConvertsShortOptionToLongOptionAndKeepsFirstValue()
        {
            var helper = new OptionHelper()
                .AddOption("o", "output", "FILE", "output file");

            var options = helper.GetOptions(new[] { "-o=first.xml", "--output=second.xml" });

            Assert.AreEqual(1, options.Count);
            Assert.AreEqual("first.xml", options["output"]);
        }

        [TestMethod]
        public void GetNoOption_ReturnsOnlyNonOptionArguments()
        {
            var helper = new OptionHelper();

            var args = helper.GetNoOption(new[] { "base.xml", "-o=out.xml", "import.xml", "--help" }).ToArray();

            CollectionAssert.AreEqual(new[] { "base.xml", "import.xml" }, args);
        }

        [TestMethod]
        public void GetHelp_ContainsOptionSyntaxAndIndentedDescription()
        {
            var helper = new OptionHelper()
                .AddOption("o", "output", "FILE", "line1", "line2");

            var helpLines = helper.GetHelp().ToArray();

            Assert.IsTrue(helpLines.Contains("\t-o=FILE [--output=FILE]"));
            Assert.IsTrue(helpLines.Contains("\t\tline1"));
            Assert.IsTrue(helpLines.Contains("\t\tline2"));
            Assert.AreEqual(string.Empty, helpLines.Last());
        }
    }
}
