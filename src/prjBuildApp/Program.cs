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
                var commandLineUIService = new CommandLineUIService(loggingService, projectManagementService, buildService);

                // Run the application
                commandLineUIService.Run();
            }
            catch (Exception ex)
            {
                // Fallback error handling in case logging service isn't initialized
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();

                // Ensure logs directory exists
                Directory.CreateDirectory("logs");

                // Log the error to a file
                File.AppendAllText(
                    Path.Combine("logs", $"prjBuild-error-{DateTime.Now:yyyyMMdd}.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [FATAL] {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}"
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
