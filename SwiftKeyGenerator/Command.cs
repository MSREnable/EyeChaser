// TouchType SwiftKey SDK
// Copyright TouchType 2012

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TouchType.TextPrediction.Sample
{
    abstract class Command
    {
        public string Name { get { return GetType().Name.ToLower(); } }

        public void Run(Context context, string arguments)
        {
            try
            {
                Execute(context, arguments);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("An error occurred: {0}", exception.ToString());
                Console.Error.WriteLine("Usage: {0} {1}", Name, Usage);
            }
        }

        protected virtual string Usage { get { return ""; } }
        protected abstract void Execute(Context context, string arguments);
    }

    class Help : Command
    {
        protected override void Execute(Context context, string arguments)
        {
            foreach (Command c in context.Commands)
                Console.WriteLine("\t{0}", c.Name);
        }
    }

    class Load : Command
    {
        protected override string Usage { get { return "<model-set>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string directory = arguments.Split(' ')[0];

            var description = ModelSetDescription.FromFile(directory);

            context.Session.Load(description);

            Console.Error.WriteLine("Successfully loaded \"{0}\"", directory);
        }
    }

    class Predict : Command
    {
        protected const int NResults = 6;
        public static readonly ResultsFilter DefaultFilter =
            new ResultsFilter(NResults, CapitalizationHint.Default, VerbatimMode.Enabled);

        private readonly ResultsFilter filter;

        public Predict() : this(DefaultFilter) { }
        public Predict(ResultsFilter filter)
        {
            this.filter = filter;
        }

        protected override string Usage { get { return "\"<context>\" <input>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string document, input;
            SplitContextAndInput(arguments, out document, out input);
            MakePredictions(context, document, input);
        }

        public static void SplitContextAndInput(string arguments, out string document, out string input)
        {
            int start = arguments.IndexOf('"') + 1;
            int end = arguments.IndexOf('"', start);
            document = arguments.Substring(start, end - start);
            input = arguments.Substring(end + 1, arguments.Length - end - 1).Trim();
        }

        protected void MakePredictions(Context context, string document, TouchHistory input)
        {
            Sequence seq = context.Session.Tokenizer.Split(document);

            Predictor predictor = context.Session.Predictor;
            Prediction[] predictions =
                predictor.GetPredictions(seq, input, filter.With(context.Capitalization));

            foreach (Prediction p in predictions)
                Console.WriteLine("\t[{0}]", p.FullPrediction);
        }

        protected void MakePredictions(Context context, string document, string input)
        {
            TouchHistory touchHistory = new TouchHistory();
            touchHistory.AddStringByGraphemeClusters(input);
            MakePredictions(context, document, touchHistory);
        }
    }

    class Correct : Command
    {
        protected const int NResults = 6;
        public static readonly ResultsFilter DefaultFilter =
            new ResultsFilter(NResults, CapitalizationHint.Default, VerbatimMode.Enabled);

        private readonly ResultsFilter filter;

        public Correct() : this(DefaultFilter) { }
        public Correct(ResultsFilter filter)
        {
            this.filter = filter;
        }

        protected override string Usage { get { return "\"<context>\" <input> \"<post-context>\""; } }

        protected override void Execute(Context context, string arguments)
        {
            string preDocument, input, postDocument;
            SplitContextAndInput(arguments, out preDocument, out input, out postDocument);
            MakePredictions(context, preDocument, input, postDocument);
        }

        private static readonly Regex regex = new Regex("\\\"(.*?)\\\"(.*)\\\"(.*?)\\\"$");

        public static void SplitContextAndInput(
                string arguments, out string preDocument, out string input, out string postDocument)
        {
            MatchCollection matches = regex.Matches(arguments);
            foreach (Match m in matches)
            {
                GroupCollection groups = m.Groups;
                preDocument = groups[1].Value;
                input = groups[2].Value;
                postDocument = groups[3].Value;
                return;
            }
            preDocument = input = postDocument = "";
        }

        protected void MakePredictions(
                Context context, string preDocument, string input, string postDocument)
        {
            Sequence preSeq = context.Session.Tokenizer.Split(preDocument);
            Sequence postSeq = context.Session.Tokenizer.Split(postDocument);
            TouchHistory touchHistory = new TouchHistory();
            touchHistory.AddStringByGraphemeClusters(input);

            Predictor predictor = context.Session.Predictor;
            Prediction[] predictions =
                predictor.GetCorrections(preSeq, touchHistory, postSeq, filter.With(context.Capitalization));
            foreach (Prediction p in predictions)
                Console.WriteLine("\t[{0}]", p.FullPrediction);
        }
    }

    class BulkCorrect : Command
    {
        protected const int NResults = 6;
        public static readonly ResultsFilter DefaultFilter =
            new ResultsFilter(NResults, CapitalizationHint.Default, VerbatimMode.Enabled);

        private readonly ResultsFilter filter;

        public BulkCorrect() : this(DefaultFilter) { }
        public BulkCorrect(ResultsFilter filter)
        {
            this.filter = filter;
        }

        protected override string Usage { get { return "<input>"; } }

        protected override void Execute(Context context, string arguments)
        {
            MakePredictions(context, arguments);
        }

        protected void MakePredictions(Context context, string text)
        {
            Sequence tokens = context.Session.Tokenizer.Split(text);
            const string delim = " ";
            int nTokens = tokens.Length;
            Predictor predictor = context.Session.Predictor;
            List<Prediction[]> allCorrections = new List<Prediction[]>();
            for (int i = 0; i < nTokens; ++i)
            {
                string current = tokens.TermAt(i);
                Console.Write(current);
                TouchHistory input = new TouchHistory();
                input.AddStringByGraphemeClusters(current);
                Prediction[] predictions =
                    predictor.GetCorrections(tokens.DropLast(nTokens-i), input, tokens.DropFirst(i+1), filter.With(context.Capitalization));
                if (0 < predictions.Length && !(predictions[0].FullPrediction.Equals(current)))
                {
                    allCorrections.Add(predictions);
                    Console.Write("[{0}]", allCorrections.Count);
                }
                Console.Write(delim);
            }
            Console.WriteLine();
            for (int i = 0; i < allCorrections.Count; ++i)
            {
                Console.Write("{0}: ", i+1);
                foreach (Prediction p in allCorrections[i])
                {
                    Console.Write("[{0}: {1}], ", p.FullPrediction, p.Probability);
                }
                Console.WriteLine();
            }
        }
    }

    class Capitalization : Command
    {
        protected override string Usage { get { return "<Default|InitialUpperCase|UpperCase>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string hint = arguments.Split(' ')[0];
            context.Capitalization = (CapitalizationHint)Enum.Parse(typeof(CapitalizationHint), hint, true);
        }
    }

    class AddCharacterMap : Command
    {
        protected override string Usage { get { return "<character-map-path>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string path = arguments.Split(' ')[0];

            var inputMapper = context.Session.Predictor.InputMapping;

            inputMapper.AddCharacterMapFromFile(path);

            Console.Error.WriteLine("Successfully added the character map \"{0}\"", path);
        }
    }

    class SetLayout : Command
    {
        protected override string Usage { get { return "<character-map-path>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string path = arguments.Split(' ')[0];

            context.Session.Predictor.InputMapping.SetLayoutFromFile(path);

            Console.Error.WriteLine("Successfully set the layout \"{0}\"", path);
        }
    }

    class CreateDynamic : Command
    {
        protected override string Usage { get { return "<directory> <order>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string directory = arguments.Split(' ')[0];
            int order = Int32.Parse(arguments.Split(' ')[1]);
            context.Session.Load(ModelSetDescription.DynamicWithFile(directory, order, new string[0], ModelSetDescription.DynamicModelType.PrimaryDynamicModel));

            Console.Error.WriteLine("Successfully added {0}-gram model in \"{1}\"", order, directory);
        }
    }

    class CreateTemporaryDynamic : Command
    {
        protected override string Usage { get { return "<order>"; } }

        protected override void Execute(Context context, string arguments)
        {
            int order = Int32.Parse(arguments.Split(' ')[0]);

            context.Session.Load(ModelSetDescription.DynamicTemporary(order, new string[0]));

            Console.Error.WriteLine("Successfully loaded {0}-gram temporary model", order);
        }
    }

    class Write : Command
    {
        protected override void Execute(Context context, string arguments)
        {
            context.Session.Trainer.Write();

            Console.Error.WriteLine("Successfully written dynamic models");
        }
    }

    class AddSequence : Command
    {
        protected override string Usage { get { return "<words>..."; } }

        protected override void Execute(Context context, string arguments)
        {
            var sequence = context.Session.Tokenizer.Split(arguments);

            context.Session.Trainer.AddSequence(sequence, context.Selector);
        }
    }

    class Tags : Command
    {
        protected override void Execute(Context context, string arguments)
        {
            string[] tags = context.Session.GetTags(context.Selector);

            Console.Write("\t");
            foreach (string tag in tags)
                Console.Write("{0}, ", tag);
            Console.WriteLine();
        }
    }

    class WithTag : Command
    {
        protected override string Usage { get { return "<tag> : <command>..."; } }

        private static ModelSelector Select(string tag)
        {
            return ModelSelectors.TaggedWith(tag);
        }

        protected override void Execute(Context context, string arguments)
        {
            int separator = arguments.IndexOf(':');
            string tag = arguments.Substring(0, separator).Trim();
            string nextCommand = arguments.Substring(separator + 1);

            ModelSelector original = context.Selector;
            context.Selector = Select(tag);
            context.DispatchCommand(nextCommand);
            context.Selector = original;
        }
    }

    class EnableWildcards : Command
    {
        protected override string Usage { get { return "<true|false>"; } }

        protected override void Execute(Context context, string arguments)
        {
            bool enable = Boolean.Parse(arguments.Split(' ')[0]);

            ParameterSet parameters = context.Session.Parameters;

            var parameter = parameters["input-model", "use-wildcards"].Of<Boolean>();

            parameter.Value = enable;
        }
    }

    class Parameters : Command
    {
        protected override void Execute(Context context, string arguments)
        {
            ParameterSet parameters = context.Session.Parameters;

            foreach (string t in parameters.Targets)
            {
                Console.WriteLine("{0}:", t);

                foreach (string p in parameters.GetProperties(t))
                    Console.WriteLine("\t{0}: {1}", p, parameters[t, p]);
            }
        }
    }

    class Get : Command
    {
        protected override string Usage { get { return "<target> <property>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string target = arguments.Split(' ')[0];
            string property = arguments.Split(' ')[1];
            ParameterBase parameter = context.Session.Parameters[target, property];
            Console.WriteLine(parameter);
        }
    }

    class Set : Command
    {
        private const String ERR_EMPTY_INPUT    = "Empty input character string.";
        private const String ERR_INVALID_INPUT  = "Invalid input character string: ";

        protected override string Usage { get { return "<target> <property> <value>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string[] args = arguments.Split(' ');
            string target = args[0];
            string property = args[1];
            string value = args[2];

            SetParameter(context, target, property, value);
        }

        private void SetParameter(Context context, string target, string property, string value)
        {
            ParameterBase parameter = context.Session.Parameters[target, property];

            setters[parameter.ValueType](parameter, value);
        }

        public Set()
        {
            AddSetter<UInt32>(UInt32.Parse);
            AddSetter<Double>(Double.Parse);
            AddSetter<Boolean>(Boolean.Parse);
            AddSetter<Range<UInt32>>(s => ParseRange<UInt32>(s, UInt32.Parse));
            AddSetter<Range<Double>>(s => ParseRange<Double>(s, Double.Parse));
            AddSetter<UInt32[]>(s => ParseArray<UInt32>(s, UInt32.Parse));
            AddSetter<Double[]>(s => ParseArray<Double>(s, Double.Parse));
            AddSetter<Boolean[]>(s => ParseArray<Boolean>(s, Boolean.Parse));
        }
        private void AddSetter<T>(Func<string, T> parse)
        {
            setters.Add(typeof(T), (p, v) => p.Of<T>().Value = parse(v));
        }
        private Range<T> ParseRange<T>(String s, Func<string, T> parse) where T : IComparable
        {
            String  inputNoDelimiters   = GetRangeStringNoDelimiters(s, "[", "]");
            T[]     inputList           = ParseArray(inputNoDelimiters, ',', parse);

            if (inputList.Length != 2) { throw new FormatException(ERR_INVALID_INPUT + s); }

            return new Range<T>(inputList[0], inputList[1]);
        }
        private String GetRangeStringNoDelimiters(String input, String beginDelimiter, String endDelimiter)
        {
            if ((input == null) || (input.Length == 0)) { throw new FormatException(ERR_EMPTY_INPUT); }

            if ((input.Length <= (beginDelimiter.Length + endDelimiter.Length)) ||
                !input.StartsWith(beginDelimiter) || !input.EndsWith(endDelimiter))
            {
                throw new FormatException(ERR_INVALID_INPUT + input);
            }

            return input.Substring(beginDelimiter.Length, input.Length - beginDelimiter.Length - endDelimiter.Length);
        }
        private T[] ParseArray<T>(String s, Func<string, T> parse)
        {
            return ParseArray(s, ',', parse);
        }
        private T[] ParseArray<T>(String s, Char delimiter, Func<string, T> parse)
        {
            return s.Split(delimiter).Select(parse).ToArray();
        }

        private readonly Dictionary<Type, Action<ParameterBase, string>> setters
            = new Dictionary<Type, Action<ParameterBase, string>>();
    }

    class LoadSettings : Command
    {
        protected override string Usage { get { return "<settings-path>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string path = arguments.Split(' ')[0];
            context.Session.Parameters.LoadFile(path);
            Console.Error.WriteLine("Successfully loaded settings from \"{0}\"", path);
        }
    }

    class SaveSettings : Command
    {
        protected override string Usage { get { return "<settings-path>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string path = arguments.Split(' ')[0];
            context.Session.Parameters.SaveFile(path);
            Console.Error.WriteLine("Successfully saved settings to \"{0}\"", path);
        }
    }

    class LoadKeyModel : Command
    {
        protected override string Usage { get { return "<key-model-path>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string path = arguments.Split(' ')[0];
            context.Session.Predictor.GetKeyPressModel().LoadFile(path);
            Console.Error.WriteLine("Successfully set the keypress model to \"{0}\"", path);
        }
    }

    class TouchHistoryHelper
    {
        public static TouchHistory Parse(string input, bool flow)
        {
            var result = new TouchHistory();
            foreach (string pair in input.Split(';'))
            {
                string[] p = pair.Split(',');
                if (p.Length == 2)
                {
                    Point at = new Point(Double.Parse(p[0].Trim()), Double.Parse(p[1].Trim()));
                    if (flow) result.AppendSample(at);
                    else      result.AddPress(at, ShiftState.Unshifted);
                }
            }
            return result;
        }
    }

    class PredictTouch : Predict
    {
        protected override string Usage { get { return "\"<context>\" <x1>,<y1>; <x2>,<y2>; ..."; } }

        protected override void Execute(Context context, string arguments)
        {
            string document, input;
            SplitContextAndInput(arguments, out document, out input);
            MakePredictions(context, document, TouchHistoryHelper.Parse(input, false));
        }
    }

    class LearnTouch : Command
    {
        protected override string Usage { get { return "\"<context>\" <x1>,<y1>; <x2>,<y2>; ... | <selection-index>"; } }

        protected override void Execute(Context context, string arguments)
        {
            int selectionStart = arguments.IndexOf('|');
            int selection = Int32.Parse(arguments.Substring(selectionStart + 1).Trim());

            string document, input;
            Predict.SplitContextAndInput(arguments.Substring(0, selectionStart), out document, out input);
            TouchHistory touches = TouchHistoryHelper.Parse(input, false);

            LearnFromTouch(context, document, touches, selection);
        }

        private void LearnFromTouch(Context context, string document, TouchHistory touches, int selection)
        {
            Tokenizer tokenizer = context.Session.Tokenizer;
            Predictor predictor = context.Session.Predictor;

            Prediction[] predictions =
                predictor.GetPredictions(tokenizer.Split(document),
                                         touches,
                                         Predict.DefaultFilter.With(context.Capitalization));

            context.Session.Trainer.LearnFrom(touches, predictions[selection]);
        }
    }

    class PredictFlow : Predict
    {
        protected override string Usage { get { return "\"<context>\" <x1>,<y1>; <x2>,<y2>; ..."; } }

        protected override void Execute(Context context, string arguments)
        {
            string document, input;
            SplitContextAndInput(arguments, out document, out input);
            MakePredictions(context, document, TouchHistoryHelper.Parse(input, true));
        }
    }

    class AddPunctuationRules : Command
    {
        protected override string Usage { get { return "<rules-file-path>"; } }

        protected override void Execute(Context context, string arguments)
        {
            string path = arguments.Split(' ')[0];
            context.Session.Punctuator.AddRulesFromFile(path);
            Console.Error.WriteLine("Successfully added punctuation rules \"{0}\"", path);
        }
    }

    class Punctuate : Predict
    {
        protected override string Usage { get { return "\"<input>\""; } }

        protected override void Execute(Context context, string arguments)
        {
            string input, ignored;
            Predict.SplitContextAndInput(arguments, out input, out ignored);

            ProcessPunctuation(context, input);
        }

        private void ProcessPunctuation(Context context, String input)
        {
            if (input.Length != 0)
            {
                String existing = input.Substring(0, input.Length - 1);
                String added = input.Substring(input.Length - 1);

                Punctuator punctuator = context.Session.Punctuator;

                Punctuator.Action[] actions = punctuator.Punctuate(existing, added, "");

                foreach (var action in actions)
                    Console.WriteLine("  {0}", action);
            }
            else Console.Error.WriteLine("Must pass at least one character to 'punctuate'");
        }
    }
}
