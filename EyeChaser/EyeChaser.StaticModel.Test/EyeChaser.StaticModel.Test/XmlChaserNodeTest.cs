using EyeChaser.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.StaticModel.Test
{
    [TestClass]
    public class XnlChaserNodeTest
    {
        static async Task CheckSameAsync(IChaserNode expected, IChaserNode actual)
        {
            Assert.AreEqual(expected.Caption, actual.Caption);
            Assert.AreEqual(expected.Probability, actual.Probability);

            var expectedEnumerator = (await expected.GetChildrenAsync(0)).GetEnumerator();
            var actualEnumerator = (await actual.GetChildrenAsync(0)).GetEnumerator();

            var maxProbability = 1.0;
            var probabilitySum = 0.0;
            var isLeaf = true;
            while (expectedEnumerator.MoveNext())
            {
                isLeaf = false;

                Assert.IsTrue(actualEnumerator.MoveNext());

                var expectedChild = expectedEnumerator.Current;
                var actualChild = actualEnumerator.Current;

                await CheckSameAsync(expectedChild, actualChild);

                Assert.IsTrue(expectedChild.Probability <= maxProbability);
                maxProbability = expectedChild.Probability;
                probabilitySum += expectedChild.Probability;
            }
            Assert.IsTrue(isLeaf || !actualEnumerator.MoveNext());

            Assert.IsTrue(0 <= maxProbability);
            Assert.IsTrue(probabilitySum <= 1.0);
        }

        [TestMethod]
        public async Task WriteXmlTest()
        {
            var root = new XmlChaserNode { Caption = "Root Node", Probability = 1 };
            root.Add(new XmlChaserNode { Caption = "Hello", Probability = 0.5 });
            root.Add(new XmlChaserNode { Caption = "World", Probability = 0.45 });

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true };
            var writer = XmlWriter.Create(builder, settings);
            root.WriteXml(writer);
            writer.Flush();

            var xml = builder.ToString();

            Debug.WriteLine(xml);

            var readerSettings = new XmlReaderSettings { };
            var stream = new StringReader(xml);
            var reader = XmlReader.Create(stream, readerSettings);

            var copyRoot = XmlChaserNode.ReadXml(reader);

            await CheckSameAsync(root, copyRoot);
        }

        [TestMethod]
        public async Task WriteXmlAsyncTest()
        {
            var root = new XmlChaserNode { Caption = "Root Node", Probability = 1 };
            root.Add(new XmlChaserNode { Caption = "Hello", Probability = 0.5 });
            root.Add(new XmlChaserNode { Caption = "World", Probability = 0.45 });

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true, Async = true };
            var writer = XmlWriter.Create(builder, settings);
            await root.WriteXmlAsync(writer);
            await writer.FlushAsync();

            var xml = builder.ToString();

            Debug.WriteLine(xml);

            var readerSettings = new XmlReaderSettings { Async = true };
            var stream = new StringReader(xml);
            var reader = XmlReader.Create(stream, readerSettings);

            var copyRoot = await XmlChaserNode.ReadXmlAsync(reader);

            await CheckSameAsync(root, copyRoot);
        }
    }
}
