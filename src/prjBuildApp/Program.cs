using prjBuildApp.Models.Configuration;
using prjBuildApp.Services;
using Serilog;
using System;
using System.IO;

namespace prjBuildApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Create configuration service
                var configService = new ConfigurationService();

                // Create logging service
                var loggingService = new LoggingService(configService.Configuration);

                // Log application start
                loggingService.Information("prjBuild application starting");

                // Create file system service
                var fileSystemService = new FileSystemService(loggingService, configService.Settings);

                // Create project management service
                var projectManagementService = new ProjectManagementService(loggingService, fileSystemService, configService.Settings);

                // Create build service
                var buildService = new BuildService(loggingService, fileSystemService);

                // Create command-line UI service
                var commandLineUIService = new CommandLineUIService(loggingService, projectManagementService, buildService, fileSystemService, configService.Settings);

                // Run the application
                commandLineUIService.Run();
            }
            catch (Exception ex)
            {
                // Fallback error handling in case logging service isn't initialized
                // Format console output to match Serilog's console format
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss} FTL] {ex.Message}");
                if (ex.StackTrace != null)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                Console.ResetColor();

                // Ensure logs directory exists
                Directory.CreateDirectory("logs");

                // Log the error to a file with format matching Serilog's file output format exactly
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string timeZone = DateTime.Now.ToString("zzz");
                string logContent = $"{timestamp} {timeZone} [FTL] {ex.Message}{Environment.NewLine}";

                // Only add stack trace if available (avoid redundant newline)
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    logContent += $"{ex.StackTrace}{Environment.NewLine}";
                }

                File.AppendAllText(
                    Path.Combine("logs", $"prjBuild-error-{DateTime.Now:yyyyMMdd-HHmmss}.log"),
                    logContent
                );
            }
            finally
            {
                // Flush and close Serilog
                Log.CloseAndFlush();
            }
        }
    }
}
