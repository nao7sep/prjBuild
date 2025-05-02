using Microsoft.Extensions.Configuration;
using prjBuildApp.Models.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace prjBuildApp.Services
{
    public class ConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly Settings _settings;

        public ConfigurationService()
        {
            try
            {
                // Build configuration
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                // Try to find appsettings.json in parent directories if not found in current directory
                string? directory = Directory.GetCurrentDirectory();
                while (directory != null && !File.Exists(Path.Combine(directory, "appsettings.json")))
                {
                    directory = Directory.GetParent(directory)?.FullName;
                    if (directory != null)
                    {
                        builder.AddJsonFile(Path.Combine(directory, "appsettings.json"), optional: true);
                    }
                }

                _configuration = builder.Build();

                // Bind settings
                _settings = new Settings();

                // Initialize with default values if configuration is empty
                if (!_configuration.GetChildren().Any())
                {
                    Console.WriteLine("No configuration found, using default settings");
                    InitializeDefaultSettings();
                }
                else
                {
                    _configuration.Bind(_settings);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to load configuration", ex);
            }
        }

        private void InitializeDefaultSettings()
        {
            // Set default root directory to current directory
            var rootDir = new RootDirectoryConfig
            {
                DirectoryPath = Directory.GetCurrentDirectory(),
                ArchiveDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "archives")
            };

            _settings.RootDirectories.Add(rootDir);

            // Set default ignored objects
            _settings.IgnoredObjectNames = new List<string>
            {
                "bin",
                "obj",
                ".vs",
                ".git",
                "node_modules"
            };

            _settings.IgnoredObjectRelativePaths = new List<string>
            {
                "packages"
            };
        }

        public Settings Settings => _settings;

        public IConfiguration Configuration => _configuration;
    }
}