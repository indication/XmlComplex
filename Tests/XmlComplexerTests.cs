using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Xml;
using XmlComplex;

namespace XmlComplex.Tests
{
    [TestClass]
    public class XmlComplexerTests
    {
        [TestMethod]
        public void Combine_AppendsMissingChildElement()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                var basePath = Path.Combine(tempDir, "base.xml");
                var importPath = Path.Combine(tempDir, "import.xml");

                File.WriteAllText(basePath, "<?xml version='1.0' encoding='utf-8'?><root><a id='1'/></root>");
                File.WriteAllText(importPath, "<?xml version='1.0' encoding='utf-8'?><root><b id='2'/></root>");

                XmlDocument result = XmlComplexer.Combine(basePath, importPath);

                var root = result.DocumentElement;
                Assert.IsNotNull(root);
                Assert.AreEqual(2, root.ChildNodes.Cast<XmlNode>().Count(n => n.NodeType == XmlNodeType.Element));
                Assert.IsNotNull(root.SelectSingleNode("b[@id='2']"));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [TestMethod]
        public void Combine_MergesIntoMatchingElementRecursively()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                var basePath = Path.Combine(tempDir, "base.xml");
                var importPath = Path.Combine(tempDir, "import.xml");

                File.WriteAllText(basePath, "<?xml version='1.0' encoding='utf-8'?><root><item id='1'><name>base</name></item></root>");
                File.WriteAllText(importPath, "<?xml version='1.0' encoding='utf-8'?><root><item id='1'><value>new</value></item></root>");

                XmlDocument result = XmlComplexer.Combine(basePath, importPath);

                var mergedItem = result.DocumentElement.SelectSingleNode("item[@id='1']") as XmlElement;
                Assert.IsNotNull(mergedItem);
                Assert.IsNotNull(mergedItem.SelectSingleNode("name"));
                Assert.IsNotNull(mergedItem.SelectSingleNode("value"));
                Assert.AreEqual("new", mergedItem.SelectSingleNode("value").InnerText);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [TestMethod]
        public void Combine_AppendsWholeElementWhenRootElementsDiffer()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                var basePath = Path.Combine(tempDir, "base.xml");
                var importPath = Path.Combine(tempDir, "import.xml");

                File.WriteAllText(basePath, "<?xml version='1.0' encoding='utf-8'?><root><a /></root>");
                File.WriteAllText(importPath, "<?xml version='1.0' encoding='utf-8'?><other><x /></other>");

                XmlDocument result = XmlComplexer.Combine(basePath, importPath);

                Assert.IsNotNull(result.DocumentElement.SelectSingleNode("other/x"));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
