using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Services.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string folderpath;

        public FileLoggerProvider(string folderpath)
        {
            this.folderpath = folderpath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(folderpath, categoryName);
        }

        public void Dispose()
        {
            
        }
    }
}
