using EyeChaser.Queries;
using EyeChaser.StaticModel;
using EyeChaser.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EyeChaser.Queries
{
    public class XmlChaserQueryEngine : QueryEngine
    {
        XmlChaserQueryEngine(ChaserQueryNode root)
        {

        }

        static async Task<XmlChaserQueryEngine> CreateAsync(XmlReader reader)
        {
            var xmlRoot = await XmlChaserNode.ReadXmlAsync(reader);
            var sortedRoot = new AlphabeticChaserNode(xmlRoot, 0.0);
            var root = new ChaserQueryNode(sortedRoot);
            var engine = new XmlChaserQueryEngine(root);
            return engine;
        }
    }
}
