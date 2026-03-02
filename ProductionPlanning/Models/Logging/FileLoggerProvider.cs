using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ProductionPlanning.Models.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, FileLogger> _loggers =
            new ConcurrentDictionary<string, FileLogger>(StringComparer.OrdinalIgnoreCase);

        private readonly FileLoggerOptions _options;
        private readonly IDisposable _onChangeToken;
        private FileLoggerConfiguration _currentConfig;

        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
        {
            _options = options.CurrentValue;
            _currentConfig = new FileLoggerConfiguration
            {
                LogLevel = _options.DefaultLogLevel,
                FilePath = _options.FilePath,
                MaxFileSize = _options.MaxFileSize,
                RetainedFileCount = _options.RetainedFileCount
            };

            _onChangeToken = options.OnChange(updatedConfig =>
            {
                _currentConfig = new FileLoggerConfiguration
                {
                    LogLevel = updatedConfig.DefaultLogLevel,
                    FilePath = updatedConfig.FilePath,
                    MaxFileSize = updatedConfig.MaxFileSize,
                    RetainedFileCount = updatedConfig.RetainedFileCount
                };
            });
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _currentConfig));
        }

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }

    //----------------------------------------------------------
    //----------------------------------------------------------
    public class FileLoggerConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public string FilePath { get; set; } = "logs/app.log";
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
        public int RetainedFileCount { get; set; } = 5;
        public bool IncludeScopes { get; set; } = true;
    }

    public class FileLoggerOptions
    {
        public LogLevel DefaultLogLevel { get; set; } = LogLevel.Information;
        public string FilePath { get; set; } = "logs/app.log";
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
        public int RetainedFileCount { get; set; } = 5;
        public bool IncludeScopes { get; set; } = true;
    }
}
