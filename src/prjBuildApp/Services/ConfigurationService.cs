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
                // Build configuration using AppContext.BaseDirectory instead of Directory.GetCurrentDirectory()
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                // Try to find appsettings.json in parent directories if not found in current directory
                string? directory = AppContext.BaseDirectory;
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

                // Fail if no configuration is found
                if (!_configuration.GetChildren().Any())
                {
                    throw new ApplicationException("No configuration found. Please ensure appsettings.json exists and is accessible.");
                }

                _configuration.Bind(_settings);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to load configuration", ex);
            }
        }

        public Settings Settings => _settings;

        public IConfiguration Configuration => _configuration;
    }
}