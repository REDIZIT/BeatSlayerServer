using Microsoft.Extensions.Logging;

namespace BeatSlayerServer.Services.Logging
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        public ConsoleLoggerProvider() { }

        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(categoryName);
        }

        public void Dispose()
        {

        }
    }
}
