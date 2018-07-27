// TouchType SwiftKey SDK
// Copyright TouchType 2012

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using TouchType.TextPrediction;

namespace EyeChaser.SwiftKeyModel
{
    class Context : IDisposable
    {
        private readonly Session session;

        private CapitalizationHint capitalization = CapitalizationHint.Default;
        private ModelSelector selector = ModelSelectors.AllModels;

        //TODO insert private license key
        private static string License = ;

        public Context()
        {
            RegisterLogger();

            session = SwiftKeySDK.CreateSession(License);

            session.Trainer.SetParameterLearning(false); // prevents prefixes from being over-penalized

            Console.Error.WriteLine("SwiftKey SDK {0}...", SwiftKeySDK.Version);
        }

        public void Dispose()
        {
            session.Dispose();
        }

        public Session Session { get { return session; } }
        public CapitalizationHint Capitalization
        {
            get { return capitalization; }
            set { capitalization = value; }
        }
        public ModelSelector Selector
        {
            get { return selector; }
            set { selector = value; }
        }

        private static void RegisterLogger()
        {
            var colourMap = new Dictionary<LogLevel, ConsoleColor>();
            colourMap.Add(LogLevel.Debug, ConsoleColor.DarkGray);
            colourMap.Add(LogLevel.Warning, ConsoleColor.DarkYellow);
            colourMap.Add(LogLevel.Severe, ConsoleColor.Red);

            SwiftKeySDK.Log += new LogHandler(delegate(LogLevel level, string message) 
            {
                ConsoleColor original = Console.ForegroundColor;
                Console.ForegroundColor = colourMap[level];
                Console.Error.Write("[{0}]: {1}", level, message);
                Console.ForegroundColor = original;
            });
        }

        public void Load(string arguments)
        {
            string directory = arguments.Split(' ')[0];

            var description = ModelSetDescription.FromFile(directory);

            Session.Load(description);

            Console.Error.WriteLine("Successfully loaded \"{0}\"", directory);
        }
    }
}
