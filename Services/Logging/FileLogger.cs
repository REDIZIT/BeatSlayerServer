using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Services.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string logsFolderPath;
        private readonly string category;

        public FileLogger(string logsFolder, string category)
        {
            logsFolderPath = logsFolder;
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
                string time = DateTime.UtcNow.ToString("H:mm:ss");

                var msg = $"{time} [{category}]\n  " + state + exception;

                string filepath = GetAndCreateFolder() + "/" + GetFileName(logLevel);

                File.AppendAllText(filepath, msg + "\n");
            }
        }

        private string GetAndCreateFolder()
        {
            string date = DateTime.UtcNow.ToString("dd.MM.yyyy");
            string folder = logsFolderPath + "/" + date;

            Directory.CreateDirectory(folder);

            return folder;
        }
        private string GetFileName(LogLevel level)
        {
            return level.ToString() + ".log";
        }
    }
}
