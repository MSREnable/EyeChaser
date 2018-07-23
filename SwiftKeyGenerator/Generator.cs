// TouchType SwiftKey SDK
// Copyright TouchType 2012

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace TouchType.TextPrediction.Sample
{
    class Words
    {
        public static int TotalNumWords { get; private set; }

        static int PowToMakeOne(double d)
        {
            return Enumerable.Range(0, int.MaxValue).SkipWhile(i => (d * Math.Pow(10, i)) < 1).First();
        }
        // Each time we look for predictions, include everything above this, and one word for each keyboard character
        protected const double WordProbThreshold = 0.01;
        protected const int NResults = (int)(1.0 / WordProbThreshold);
        protected static readonly string ProbFormat = "F" + PowToMakeOne(WordProbThreshold);

        // Keep expanding tree until total probability drops below this
        protected const double MinCumuProb = 0.0005;
        protected static readonly string CumuProbFormat = "F" + PowToMakeOne(MinCumuProb);

        public static readonly ResultsFilter DefaultFilter =
            new ResultsFilter(NResults, CapitalizationHint.Default, VerbatimMode.Enabled);

        String allText { get; }
        String word { get; }
        public double prob { get; }
        public double cumuProb { get; }

        Words[] children;

        public Words(String parent, String word, double cumuProb, double prob)
        {
            TotalNumWords++;
            this.allText = (parent.Length>0 ? parent + " " : parent) + word;
            this.word = word;
            this.cumuProb = cumuProb;
            this.prob = prob;
        }

        public void makeChildren(Context context, SortedSet<Words> queue)
        {
            Sequence seq = context.Session.Tokenizer.Split(allText);

            Predictor predictor = context.Session.Predictor;
            Prediction[] predictions =
                predictor.GetPredictions(seq, new TouchHistory(), DefaultFilter.With(context.Capitalization));

            double totProb = predictions.Select(p => p.Probability).Sum();
            //Assume that's 100%, which is clearly bogus
            var seenEachChar = new Boolean['z' - 'a'];
            //int distinctCharsSeen = 0;
            children = predictions
                .Select(p => new Words(allText, p.FullPrediction, this.cumuProb * p.Probability / totProb, p.Probability / totProb))
                .TakeWhile(w => {
                    int idx = w.word.ToLower().First() - 'a';
                    bool forceInclude = false;// distinctCharsSeen < seenEachChar.Length;
                    if (idx >= 0 && idx < seenEachChar.Length) {
                        if (!seenEachChar[idx]) {
                            seenEachChar[idx] = true;
                            //distinctCharsSeen++;
                            forceInclude = true;
                        }
                    }
                    return (w.prob > WordProbThreshold) || forceInclude;
                })
                .OrderBy(w => w.word)
                .ToArray();
            foreach (Words w in children.TakeWhile(w => w.cumuProb > MinCumuProb)) queue.Add(w);
        }

        public void printXML(TextWriter w, String indent)
        {
            bool hasChildren = (children != null) && (children.Count() > 0);
            w.WriteLine("{0}<Node Caption=\"{1}\" Probability=\"{2}\" allText=\"{3}\" cumulative=\"{4}\" {5}>",
                indent,
                word,
                this.prob.ToString(ProbFormat),
                allText,
                this.cumuProb.ToString(CumuProbFormat),
                hasChildren ? "" : "/");

            if (hasChildren) {
                foreach (Words ch in children)
                    ch.printXML(w, indent + "    ");
                w.WriteLine("{0}</Node>", indent);
            }
        }

    }

    class HighProbFirst : IComparer<Words>
    {
        public int Compare(Words x, Words y)
        {
            if (x == y) return 0;
            return x.cumuProb > y.cumuProb ? -1 : 1;
        }
    }

    public class CommandLoop
    {
        //TODO insert license key, and update ModelPath.
        private const string License = ;
        private const string ModelPath = "C:\\Users\\allawr\\Downloads\\en_gb";
        public static void Main(String[] args)
        {
            using (Context context = new Context(License))
            {
                new Load().Run(context, ModelPath);
                SortedSet<Words> queue = new SortedSet<Words>(new HighProbFirst());
                Words root = new Words("", "", 1.0, 1.0);
                queue.Add(root);
                while (queue.Count > 0)
                {
                    Console.WriteLine("Choosing - first {0}, last {1} count {2}", queue.First().cumuProb, queue.Last().cumuProb, Words.TotalNumWords);
                    Words w = queue.First();
                    queue.Remove(w);
                    w.makeChildren(context, queue);
                }
                using (var file = new StreamWriter("tree.xml")) {
                    file.WriteLine("<?xml version=\"1.0\" encoding=\"utf - 8\" ?>");
                    root.printXML(file, "");
                }
                Console.WriteLine("Got {0} Sentences", Words.TotalNumWords);
                Console.ReadLine();
            }
        }
    }
}
