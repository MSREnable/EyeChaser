using EyeChaser.StaticModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EyeChaser.MokeDataMaker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        static IEnumerable<XmlChaserNode> CreateChildren(int cursor, int position, int depth, params string[][] phrases)
        {
            Debug.Assert(phrases.Length == 1);

            foreach (var phrase in phrases)
            {
                foreach (var word in phrase)
                {
                    var node = new XmlChaserNode
                    {
                        Caption = word,
                        Probability = cursor < phrase.Length ? (word == phrase[cursor] ? 0.75 : 0.25 / phrase.Length) : 1.0 / phrase.Length
                    };

                    if (position < depth)
                    {
                        foreach (var child in CreateChildren(Array.IndexOf(phrase, node.Caption) + 1, position + 1, depth, phrases))
                        {
                            node.Add(child);
                        }
                    }

                    yield return node;
                }
            }
        }

        static XmlChaserNode CreateTree(int depth, params string[][] phrases)
        {
            var root = new XmlChaserNode
            {
                Caption = "Root",
                Probability = 1
            };

            foreach (var node in CreateChildren(0, 0, depth, phrases))
            {
                root.Add(node);
            }

            return root;
        }

        void OnTheQuickBrownFox(object sender, RoutedEventArgs e)
        {
            var vocabString = "the quick brown fox";
            var vocabArray = vocabString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var root = CreateTree(5, vocabArray);

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true };
            var writer = XmlWriter.Create(builder, settings);
            root.WriteXml(writer);
            writer.Flush();

            var xml = builder.ToString();

            var package = new DataPackage();
            package.SetText(xml);

            Clipboard.SetContent(package);
        }
    }
}
