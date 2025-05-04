using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace prjBuildApp.Services
{
    public class LoggingService
    {
        private readonly ILogger _logger;

        public LoggingService(IConfiguration configuration)
        {
            try
            {
                // Use AppContext.BaseDirectory instead of Directory.GetCurrentDirectory()
                string logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDirectory);

                // Configure Serilog directly in code instead of reading from appsettings.json
                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    // Add WriteTo configurations directly in code
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(
                        path: Path.Combine(logsDirectory, $"prjBuild-{DateTime.Now:yyyyMMdd-HHmmss}.log"),
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    // Add Enrich configurations directly in code
                    .Enrich.FromLogContext();

                _logger = loggerConfig.CreateLogger();

                // Set as static logger for global usage
                Log.Logger = _logger;

                _logger.Information("Logging initialized");
            }
            catch (Exception ex)
            {
                // Create logs directory for fallback logger if it doesn't exist
                string logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDirectory);

                // Fallback logger that matches configuration structure but with Debug level
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(
                        Path.Combine(logsDirectory, $"prjBuild-{DateTime.Now:yyyyMMdd-HHmmss}.log"),
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                // Set as static logger for global usage
                Log.Logger = _logger;

                _logger.Error(ex, "Failed to initialize logging from configuration");
            }
        }

        public ILogger Logger => _logger;

        public void Information(string messageTemplate, params object[] propertyValues)
        {
            _logger.Information(messageTemplate, propertyValues);
        }

        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            _logger.Warning(messageTemplate, propertyValues);
        }

        public void Error(Exception? exception, string messageTemplate, params object[] propertyValues)
        {
            _logger.Error(exception, messageTemplate, propertyValues);
        }

        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            _logger.Debug(messageTemplate, propertyValues);
        }
    }
}