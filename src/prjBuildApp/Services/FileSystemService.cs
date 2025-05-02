using prjBuildApp.Models.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace prjBuildApp.Services
{
    public class FileSystemService
    {
        private readonly LoggingService _loggingService;
        private readonly Settings _settings;

        public FileSystemService(LoggingService loggingService, Settings settings)
        {
            _loggingService = loggingService;
            _settings = settings;
        }

        public bool IsIgnored(string path, string relativePath)
        {
            // Check if the file or directory should be ignored based on name
            string name = Path.GetFileName(path);
            if (_settings.IgnoredObjectNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                _loggingService.Debug("Ignoring {Path} because its name is in the global ignore list", path);
                return true;
            }

            // Check if the file or directory should be ignored based on relative path
            if (_settings.IgnoredObjectRelativePaths.Any(ignoredPath =>
                relativePath.Contains(ignoredPath, StringComparison.OrdinalIgnoreCase)))
            {
                _loggingService.Debug("Ignoring {Path} because its relative path contains a pattern in the global ignore list", path);
                return true;
            }

            return false;
        }

        public IEnumerable<string> EnumerateDirectories(string path, bool recursive = false)
        {
            try
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                return Directory.EnumerateDirectories(path, "*", searchOption);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error enumerating directories in {Path}", path);
                return Enumerable.Empty<string>();
            }
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", bool recursive = false)
        {
            try
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                return Directory.EnumerateFiles(path, searchPattern, searchOption);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error enumerating files in {Path} with pattern {Pattern}", path, searchPattern);
                return Enumerable.Empty<string>();
            }
        }

        public bool CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error creating directory {Path}", path);
                return false;
            }
        }

        public bool CreateZipArchive(string sourceDirectory, string destinationArchiveFile, bool includeBaseDirectory = true)
        {
            try
            {
                // Ensure the destination directory exists
                string? destinationDirectory = Path.GetDirectoryName(destinationArchiveFile);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // Create the archive
                ZipFile.CreateFromDirectory(sourceDirectory, destinationArchiveFile, CompressionLevel.Optimal, includeBaseDirectory);
                _loggingService.Information("Created archive {ArchiveFile} from {SourceDirectory}", destinationArchiveFile, sourceDirectory);
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error creating archive {ArchiveFile} from {SourceDirectory}", destinationArchiveFile, sourceDirectory);
                return false;
            }
        }
    }
}