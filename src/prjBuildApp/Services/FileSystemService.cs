using prjBuildApp.Models.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using prjBuildApp.Models.Project;

namespace prjBuildApp.Services
{
    public class FileSystemService
    {
        // Fields
        private readonly LoggingService _loggingService;
        private readonly Settings _settings;

        // Constructor
        public FileSystemService(LoggingService loggingService, Settings settings)
        {
            _loggingService = loggingService;
            _settings = settings;
        }

        // Public Methods - File and Directory Operations
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

        /// <summary>
        /// Gets the archive directory path for a project
        /// </summary>
        /// <param name="project">The project to get the archive directory for</param>
        /// <returns>The archive directory path</returns>
        public string GetArchiveDirectory(ProjectInfo project)
        {
            // Find the root directory config for this project
            var rootDir = project.Solution.DirectoryPath;
            var rootDirConfig = _settings.RootDirectories.FirstOrDefault(rd =>
                rootDir.StartsWith(rd.DirectoryPath, StringComparison.OrdinalIgnoreCase));

            string archiveDirectory;
            if (rootDirConfig != null && !string.IsNullOrEmpty(rootDirConfig.ArchiveDirectoryPath))
            {
                // Use the configured archive directory path
                archiveDirectory = rootDirConfig.ArchiveDirectoryPath;
            }
            else
            {
                // Fall back to default archive directory
                archiveDirectory = Path.Combine(rootDir, "archives");
            }

            // Create project-specific subdirectory in the archive directory
            return Path.Combine(archiveDirectory, project.Name);
        }

        // Public Methods - Archive Operations
        /// <summary>
        /// Checks if all archives for a project exist
        /// </summary>
        /// <param name="project">The project to check</param>
        /// <param name="supportedRuntimes">List of supported runtimes for the project</param>
        /// <returns>True if all archives exist, false otherwise</returns>
        public bool AreAllArchivesExisting(ProjectInfo project, List<string> supportedRuntimes)
        {
            // Check source archive
            var (_, sourceArchivePath) = GetArchiveFileInfo(project);
            if (!File.Exists(sourceArchivePath))
            {
                return false;
            }

            // Check binary archives for each runtime
            foreach (var runtime in supportedRuntimes)
            {
                var (_, binaryArchivePath) = GetArchiveFileInfo(project, runtime);
                if (!File.Exists(binaryArchivePath))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the archive file name and path for a project
        /// </summary>
        /// <param name="project">The project to get the archive file for</param>
        /// <param name="runtime">The runtime identifier (null for source archives)</param>
        /// <returns>A tuple containing the archive file name and full path</returns>
        public (string fileName, string filePath) GetArchiveFileInfo(ProjectInfo project, string? runtime = null)
        {
            // Get the primary version string from the project's VersionManager
            var primaryVersionSource = project.VersionManager.GetPrimaryVersionSource();
            string versionString = primaryVersionSource?.VersionString ?? "0.0.0.0";

            string fileName;
            if (runtime == null)
            {
                // Source archive: (projectName)-(primaryVersionString)-src.zip
                fileName = $"{project.Name}-v{versionString}-src.zip";
            }
            else
            {
                // Executable archive: (projectName)-(primaryVersionString)-(supportedRuntime).zip
                fileName = $"{project.Name}-v{versionString}-{runtime}.zip";
            }

            // Get the archive directory
            string archiveDirectory = GetArchiveDirectory(project);

            // Ensure the directory exists
            CreateDirectory(archiveDirectory);

            string filePath = Path.Combine(archiveDirectory, fileName);
            return (fileName, filePath);
        }
    }
}