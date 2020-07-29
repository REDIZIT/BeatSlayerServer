using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Services.Logging
{
    public class ConsoleLogger : ILogger
    {
        private readonly string category;

        public ConsoleLogger(string category)
        {
            this.category = category;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                Console.ForegroundColor = GetColor(logLevel);
                Console.WriteLine(LogLevelToString(logLevel) + $": [{category}]");
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine("    " + state + " " + exception);
            }
        }

        private ConsoleColor GetColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace: return ConsoleColor.DarkYellow;
                case LogLevel.Debug: return ConsoleColor.Yellow;
                case LogLevel.Information: return ConsoleColor.DarkGreen;
                case LogLevel.Warning: return ConsoleColor.Magenta;
                case LogLevel.Error: return ConsoleColor.Red;
                case LogLevel.Critical: return ConsoleColor.Red;
                default: return ConsoleColor.White;
            }
        }

        private string LogLevelToString(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace: return "Trace";
                case LogLevel.Debug: return "Debug";
                case LogLevel.Information: return "Info";
                case LogLevel.Warning: return "Warn";
                case LogLevel.Error: return "Error";
                case LogLevel.Critical: return "Crit";
                default: return "Unknown";
            }
        }
    }
}
