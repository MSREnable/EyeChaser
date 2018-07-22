using EyeChaser.Queries.Test.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.Queries.Test
{
    [TestClass]
    public class XmlChaserQueryTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var xml = Resources.MockData;
            var reader = new StringReader(xml);
            var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { Async = true });
            var query = await XmlChaserQueryEngine.CreateAsync(xmlReader);
        }
    }
}
