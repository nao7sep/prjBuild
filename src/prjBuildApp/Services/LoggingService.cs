using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;

namespace prjBuildApp.Services
{
    public class LoggingService
    {
        private readonly ILogger _logger;

        public LoggingService(IConfiguration configuration, ConsoleThemeService consoleThemeService)
        {
            try
            {
                // Use AppContext.BaseDirectory instead of Directory.GetCurrentDirectory()
                string logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDirectory);

                // Set global minimum level to Verbose to capture everything
                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    // Configure console with restricted level (Information and above)
                    .WriteTo.Console(
                        theme: consoleThemeService.GetSerilogTheme(),
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    // Configure file with all levels (Verbose and above)
                    .WriteTo.File(
                        path: Path.Combine(logsDirectory, $"prjBuild-{DateTime.Now:yyyyMMdd-HHmmss}.log"),
                        restrictedToMinimumLevel: LogEventLevel.Verbose,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .Enrich.FromLogContext();

                _logger = loggerConfig.CreateLogger();

                // Set as static logger for global usage
                Log.Logger = _logger;

                Information("Logging initialized with {Theme} theme",
                    consoleThemeService.CurrentTheme);
                Information("Detected terminal environment: {Environment}",
                    consoleThemeService.DetectedEnvironment);
            }
            catch (Exception ex)
            {
                // Create a simple console logger for the error
                Console.WriteLine($"Error initializing logger: {ex.Message}");

                // Create a minimal logger
                _logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();

                // Set as static logger for global usage
                Log.Logger = _logger;
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