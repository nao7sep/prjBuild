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

        public bool CreateZipArchive(string sourceDirectory, string destinationArchiveFile,
            List<string>? ignoredObjectNames = null, List<string>? ignoredObjectRelativePaths = null)
        {
            try
            {
                // Ensure the destination directory exists
                string? destinationDirectory = Path.GetDirectoryName(destinationArchiveFile);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // Create the archive with relative paths
                using (var archive = ZipFile.Open(destinationArchiveFile, ZipArchiveMode.Create))
                {
                    // Get all files in the source directory (recursively)
                    var allFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
                    var filesToArchive = new List<string>();

                    // Filter out ignored files
                    foreach (var file in allFiles)
                    {
                        string fileName = Path.GetFileName(file);
                        string relativePath = Path.GetRelativePath(sourceDirectory, file);

                        bool shouldIgnore = false;

                        // Check if file name is in the ignored names list
                        if (ignoredObjectNames != null && ignoredObjectNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                        {
                            _loggingService.Debug("Ignoring {Path} because its name is in the ignore list", file);
                            shouldIgnore = true;
                        }

                        // Check if relative path contains any ignored path patterns
                        if (!shouldIgnore && ignoredObjectRelativePaths != null)
                        {
                            foreach (var ignoredPath in ignoredObjectRelativePaths)
                            {
                                if (relativePath.Contains(ignoredPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    _loggingService.Debug("Ignoring {Path} because its relative path contains a pattern in the ignore list", file);
                                    shouldIgnore = true;
                                    break;
                                }
                            }
                        }

                        if (!shouldIgnore)
                        {
                            filesToArchive.Add(file);
                        }
                    }

                    foreach (var file in filesToArchive)
                    {
                        // Use the relative path without the base directory
                        string entryName = Path.GetRelativePath(sourceDirectory, file);

                        // Replace Windows backslashes with forward slashes for zip entries
                        entryName = entryName.Replace('\\', '/');

                        // Create the entry and copy the file content
                        archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
                    }
                }

                _loggingService.Information("Created archive {ArchiveFile} from {SourceDirectory} with relative paths",
                    destinationArchiveFile, sourceDirectory);
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error creating archive {ArchiveFile} from {SourceDirectory}",
                    destinationArchiveFile, sourceDirectory);
                return false;
            }
        }

        /// <summary>
        /// Gets the archive directory path for a solution
        /// </summary>
        /// <param name="solution">The solution to get the archive directory for</param>
        /// <returns>The archive directory path</returns>
        public string GetSolutionArchiveDirectory(SolutionInfo solution)
        {
            // Find the root directory config for this solution
            var rootDir = solution.DirectoryPath;
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
                throw new InvalidOperationException($"No archive directory path configured for root directory: {rootDir}");
            }

            // Create solution-specific subdirectory in the archive directory
            string solutionArchiveDir = Path.Combine(archiveDirectory, solution.Name);

            return solutionArchiveDir;
        }

        /// <summary>
        /// Checks if all archives for a solution exist (source code and all project binaries)
        /// </summary>
        /// <param name="solution">The solution to check</param>
        /// <returns>True if all archives exist, false otherwise</returns>
        public bool AreAllArchivesExisting(SolutionInfo solution)
        {
            // Check if solution source archive exists
            if (!File.Exists(solution.SourceArchivePath))
            {
                return false;
            }

            // Check all binary archives for all projects in the solution
            foreach (var project in solution.Projects)
            {
                foreach (var archivePath in project.RuntimeArchivePaths.Values)
                {
                    if (!File.Exists(archivePath))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the source code archive path for a solution
        /// </summary>
        /// <param name="solution">The solution to get the source archive for</param>
        /// <returns>The source archive file path</returns>
        public string GetSolutionSourceArchivePath(SolutionInfo solution)
        {
            // Use the solution's archive directory path directly
            string archiveDirectory = solution.ArchiveDirectoryPath;

            // Ensure the directory exists
            CreateDirectory(archiveDirectory);

            string fileName;

            // Try to get a primary version from one of the solution's projects
            if (solution.Projects.Count > 0)
            {
                // Find the first project with a valid primary version
                foreach (var project in solution.Projects)
                {
                    var primaryVersionSource = project.VersionManager.GetPrimaryVersionSource();
                    if (primaryVersionSource?.ParsedVersion != null)
                    {
                        try
                        {
                            // Use our formatting method to get version in the appropriate format
                            string formattedVersion = VersionManager.FormatVersion(primaryVersionSource.ParsedVersion);
                            _loggingService.Debug("Using formatted version {Version} from project {ProjectName} for solution archive",
                                formattedVersion, project.Name);

                            // Solution archive: (solutionName)-(formattedVersion)-src.zip
                            fileName = $"{solution.Name}-{formattedVersion}-src.zip";

                            // Return the full path
                            return Path.Combine(archiveDirectory, fileName);
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Error(ex, "Error formatting version for solution archive");
                        }
                    }
                }
            }

            // If we get here, no valid version was found - this is an error condition
            string errorMessage = "No valid version found in solution projects";
            _loggingService.Error(null, errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        /// <summary>
        /// Gets the runtime-specific executable archive file paths for a project
        /// </summary>
        /// <param name="project">The project to get the runtime archive files for</param>
        /// <returns>A dictionary mapping runtime identifiers to executable archive paths</returns>
        public Dictionary<string, string> GetProjectRuntimeArchivePaths(ProjectInfo project)
        {
            // Get the primary version from the project's VersionManager
            var primaryVersionSource = project.VersionManager.GetPrimaryVersionSource();

            if (primaryVersionSource?.ParsedVersion == null)
            {
                string errorMessage = $"No valid version found for project {project.Name}";
                _loggingService.Error(null, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Format the version using our new method
            string formattedVersion = VersionManager.FormatVersion(primaryVersionSource.ParsedVersion);

            // Use the solution's archive directory path directly - no project subdirectory
            string archiveDirectory = project.Solution.ArchiveDirectoryPath;

            // Ensure the directory exists
            CreateDirectory(archiveDirectory);

            // Create a dictionary to hold the results
            var archivePaths = new Dictionary<string, string>();

            // Add runtime archive paths (binary archives)
            foreach (var runtime in project.SupportedRuntimes)
            {
                string runtimeFileName = $"{project.Name}-{formattedVersion}-{runtime}.zip";
                string runtimeFilePath = Path.Combine(archiveDirectory, runtimeFileName);
                archivePaths[runtime] = runtimeFilePath;
            }

            return archivePaths;
        }
    }
}