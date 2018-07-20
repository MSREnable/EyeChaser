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
        static void CheckSame(IChaserNode expected, IChaserNode actual)
        {
            Assert.AreEqual(expected.Caption, actual.Caption);
            Assert.AreEqual(expected.SortKey, actual.SortKey);
            Assert.AreEqual(expected.Probability, actual.Probability);

            var expectedEnumerator = expected.GetEnumerator();
            var actualEnumerator = actual.GetEnumerator();

            while (expectedEnumerator.MoveNext())
            {
                Assert.IsTrue(actualEnumerator.MoveNext());

                CheckSame((IChaserNode)expectedEnumerator.Current, (IChaserNode)actualEnumerator.Current);
            }
            Assert.IsFalse(actualEnumerator.MoveNext());
        }

        [TestMethod]
        public void WriteXmlTest()
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

            CheckSame(root, copyRoot);
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

            CheckSame(root, copyRoot);
        }
    }
}
