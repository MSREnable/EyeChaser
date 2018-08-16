using EyeChaser.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchType.TextPrediction;

namespace EyeChaser.DynamicSwiftKeyModel
{
    public class SwiftKeyNode : IChaserNode
    {
        public static SwiftKeyNode CreateRoot()
        {
            Context cs = new Context();
            cs.Load("C:\\Users\\allawr\\Downloads\\en_gb");
            //We'll never actually dispose that Context, but it wants to live as long as *any* node
            return new SwiftKeyNode(cs, new Sequence());
        }

        // Each time we look for predictions, include everything above this, and one word for each keyboard character
        protected const double WordProbThreshold = 0.01;
        protected const int NResults = (int)(1.0 / WordProbThreshold);

        private static readonly ResultsFilter DefaultFilter =
            new ResultsFilter(NResults, CapitalizationHint.Default, VerbatimMode.Enabled);

        private Context _context;
        private Sequence _seq;

        SwiftKeyNode(Context context, Sequence seq)
        {
            _context = context;
            _seq = seq;
        }

        public string Caption { get; private set; }

        public double Probability { get; private set; }

        private static readonly IReadOnlyList<SwiftKeyNode> empty = new SwiftKeyNode[0];
        private IReadOnlyList<SwiftKeyNode> _children;

        public IEnumerator<SwiftKeyNode> GetEnumerator()
        {
            return (_children ?? null).GetEnumerator();
        }

        public Task<IEnumerable<IChaserNode>> GetChildrenAsync(double precision)
        {
            if (_children != null) return Task.FromResult<IEnumerable<IChaserNode>>(_children);
            return Task.Run<IEnumerable<IChaserNode>>(() =>
            {
                Prediction[] predictions = _context.Session.Predictor.GetPredictions(
                    _seq, new TouchHistory(), DefaultFilter.With(_context.Capitalization));
                
                double totProb = predictions.Select(p => p.Probability).Sum();
                //Assume that's 100%, which is clearly bogus
                var seenEachChar = new Boolean['z' - 'a'];

                //int distinctCharsSeen = 0;
                var keptPredictions = predictions
                    .TakeWhile(p => {
                        int idx = p.FullPrediction.ToLower().First() - 'a';
                        bool forceInclude = false;// distinctCharsSeen < seenEachChar.Length;
                        if (idx >= 0 && idx < seenEachChar.Length)
                        {
                            if (!seenEachChar[idx])
                            {
                                seenEachChar[idx] = true;
                                //distinctCharsSeen++;
                                forceInclude = true;
                            }
                        }
                        return (p.Probability / totProb > WordProbThreshold) || forceInclude;
                    }).ToArray();
                totProb = keptPredictions.Select(p => p.Probability).Sum();
                _children = keptPredictions.Select(p =>
                    {
                        Sequence newSeq = _seq.DropLast(0); // Clone.
                        newSeq.AddLast(p.FullPrediction);
                        return new SwiftKeyNode(_context, newSeq)
                        {
                            Caption = p.FullPrediction,
                            Probability = p.Probability / totProb
                        };
                    }).ToArray();
                return _children;
            });
        }
    }
}
