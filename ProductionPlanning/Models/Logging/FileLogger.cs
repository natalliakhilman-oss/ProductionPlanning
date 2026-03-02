using System.Text;

namespace ProductionPlanning.Models.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _name;
        private readonly FileLoggerConfiguration _config;
        private readonly object _lock = new object();

        public FileLogger(string name, FileLoggerConfiguration config)
        {
            _name = name;
            _config = config;

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_config.FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _config.LogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter == null) return;

            var message = FormatLogEntry(logLevel, _name, eventId, formatter(state, exception), exception);

            // Используем легковесный подход с File.AppendAllText
            WriteToFile(message);
        }

        private void WriteToFile(string message)
        {
            // Используем Monitor для более безопасного locking
            if (Monitor.TryEnter(_lock, TimeSpan.FromMilliseconds(100)))
            {
                try
                {
                    // Проверяем размер файла перед записью
                    CheckFileSizeAndRotate();

                    // File.AppendAllText атомарно открывает, пишет и закрывает файл
                    File.AppendAllText(_config.FilePath, message + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    // Fallback
                    Console.WriteLine($"File logging failed: {ex.Message}");
                    Console.WriteLine(message);
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
            else
            {
                // Если не удалось получить lock, пишем в консоль
                Console.WriteLine($"Log lock timeout: {message}");
            }
        }

        private void CheckFileSizeAndRotate()
        {
            try
            {
                var fileInfo = new FileInfo(_config.FilePath);
                if (fileInfo.Exists && fileInfo.Length > _config.MaxFileSize)
                {
                    var timestamp = DateTime.Now.ToString("dd_MM_yyyy HH_mm");
                    var rotatedPath = $"{_config.FilePath}.{timestamp}.bak";

                    File.Move(_config.FilePath, rotatedPath);
                    //CleanupOldLogFiles();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log rotation check failed: {ex.Message}");
            }
        }

        private void CleanupOldLogFiles()
        {
            try
            {
                var directory = Path.GetDirectoryName(_config.FilePath);
                var baseName = Path.GetFileName(_config.FilePath);
                var logFiles = Directory.GetFiles(directory, $"{baseName}.*.bak");

                if (logFiles.Length > _config.RetainedFileCount)
                {
                    Array.Sort(logFiles);
                    var filesToDelete = logFiles.Length - _config.RetainedFileCount;

                    for (int i = 0; i < filesToDelete; i++)
                    {
                        try
                        {
                            File.Delete(logFiles[i]);
                        }
                        catch
                        {
                            // Ignore deletion errors
                        }
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        private string FormatLogEntry(LogLevel logLevel, string category, EventId eventId, string message, Exception exception)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
            sb.Append(" [");
            sb.Append(logLevel.ToString().ToUpperInvariant());
            sb.Append("] ");

            // Output category only for Errors
            if (logLevel >= LogLevel.Error)
            {
                sb.Append(" ( ");
                sb.Append(category);
                sb.Append(" ) ");
            }

            sb.Append(": ");
            sb.Append(message);

            if (exception != null)
            {
                sb.AppendLine();
                sb.Append("EXCEPTION: ");
                sb.Append(exception);
            }

            return sb.ToString();
        }
    }
}
