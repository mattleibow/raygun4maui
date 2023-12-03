using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Raygun4Maui.Raygun4Net.RaygunLogger
{
    public sealed class RaygunLoggerProvider : ILoggerProvider
    {
        private RaygunLoggerConfiguration _raygunLoggerConfiguration;
        private readonly ConcurrentDictionary<string, RaygunLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

        public RaygunLoggerProvider(RaygunLoggerConfiguration raygunLoggerConfiguration)
        {
            _raygunLoggerConfiguration = raygunLoggerConfiguration;
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new RaygunLogger(name, _raygunLoggerConfiguration));

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
