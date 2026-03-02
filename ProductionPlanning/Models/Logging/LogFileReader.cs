using Microsoft.Extensions.Options;

namespace ProductionPlanning.Models.Logging
{
    public class LogFileReader
    {
        private readonly string _logFilePath;
        private string _logFilePathDefault = $"{AppInfo.GetAppPath()}/Logs/Genezis.log";
            

        public LogFileReader(string? logFilePath = null)
        {
            _logFilePath = logFilePath ?? _logFilePathDefault;
        }

        public async Task<List<string>> ReadAllLinesAsync(int maxLines = 100)
        {
            if (!File.Exists(_logFilePath))
            {
                return new List<string>();
            }

            try
            {
                var lines = await File.ReadAllLinesAsync(_logFilePath);

                // Берем последние maxLines строк
                return lines.Length <= maxLines
                    ? lines.Reverse().ToList()
                    : lines.Skip(lines.Length - maxLines).Reverse().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error reading log file: {ex.Message}", ex);
            }
        }

        
    }
}
