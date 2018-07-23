// TouchType SwiftKey SDK
// Copyright TouchType 2012

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace TouchType.TextPrediction.Sample
{
    class Context : IDisposable
    {
        private readonly Dictionary<String, Command> commands;
        private readonly Session session;

        private CapitalizationHint capitalization = CapitalizationHint.Default;
        private ModelSelector selector = ModelSelectors.AllModels;

        public Context(string license)
        {
            RegisterLogger();

            session = SwiftKeySDK.CreateSession(license);

            session.Trainer.SetParameterLearning(false); // prevents prefixes from being over-penalized

            commands = GetCommands();

            Console.Error.WriteLine("SwiftKey SDK {0} interactive...", SwiftKeySDK.Version);
        }

        public void Dispose()
        {
            session.Dispose();
        }

        public Command[] Commands { get { return commands.Values.ToArray(); } }
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

        public void DispatchCommand(String line)
        {
            String trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                Console.Error.WriteLine("No command given");
                return;
            }

            String instruction = trimmed.Split(' ')[0].ToLower();
            if (!commands.ContainsKey(instruction))
            {
                Console.Error.WriteLine("Unknown command \"{0}\"", instruction);
                return;
            }

            commands[instruction].Run(this, trimmed.Substring(instruction.Length).Trim());
        }

        private static Dictionary<String, Command> GetCommands()
        {
            var results = new Dictionary<String, Command>();

            Type[] types = Assembly.GetAssembly(typeof(Context)).GetTypes();
            foreach (Type t in types)
                if (!t.IsAbstract && t.IsSubclassOf(typeof(Command)))
                {
                    Command c = (Command)t.GetConstructor(Type.EmptyTypes).Invoke(new Object[0]);
                    results.Add(c.Name, c);
                }

            return results;
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
    }
}
