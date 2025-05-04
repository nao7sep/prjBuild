using prjBuildApp.Models.Configuration;
using prjBuildApp.Models.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace prjBuildApp.Services
{
    public class ProjectManagementService
    {
        // Fields
        private readonly LoggingService _loggingService;
        private readonly FileSystemService _fileSystemService;
        private readonly Settings _settings;
        private List<SolutionInfo> _solutions = new();

        // Constructor
        public ProjectManagementService(LoggingService loggingService, FileSystemService fileSystemService, Settings settings)
        {
            _loggingService = loggingService;
            _fileSystemService = fileSystemService;
            _settings = settings;
        }

        // Properties
        public List<SolutionInfo> Solutions => _solutions;

        // Public Methods

        public void DiscoverSolutions()
        {
            _solutions.Clear();

            foreach (var rootDir in _settings.RootDirectories)
            {
                if (!Directory.Exists(rootDir.DirectoryPath))
                {
                    _loggingService.Warning("Root directory {DirectoryPath} does not exist", rootDir.DirectoryPath);
                    continue;
                }

                _loggingService.Information("Discovering solutions in {DirectoryPath}", rootDir.DirectoryPath);

                // Get all directories in the root directory
                var directories = _fileSystemService.EnumerateDirectories(rootDir.DirectoryPath)
                    .Where(dir => !_fileSystemService.IsIgnored(dir, Path.GetRelativePath(rootDir.DirectoryPath, dir)));

                foreach (var directory in directories)
                {
                    // Find solution files in the directory
                    var solutionFiles = _fileSystemService.EnumerateFiles(directory, "*.sln");

                    foreach (var solutionFile in solutionFiles)
                    {
                        string solutionName = Path.GetFileNameWithoutExtension(solutionFile);
                        _loggingService.Information("Found solution: {SolutionName} at {SolutionFile}", solutionName, solutionFile);

                        var solution = new SolutionInfo(solutionName, directory, solutionFile);

                        // Initialize inherited properties from configuration
                        var solutionConfig = _settings.Solutions.FirstOrDefault(s => s.Name == solutionName);

                        // Call InitializeInheritedProperties even if solutionConfig is null
                        // This ensures the solution inherits properties from global settings
                        solution.InitializeInheritedProperties(_settings, solutionConfig);

                        _solutions.Add(solution);

                        // Discover projects in the solution
                        DiscoverProjects(solution);
                    }
                }
            }

            _loggingService.Information("Discovered {SolutionCount} solutions with {ProjectCount} projects",
                _solutions.Count, _solutions.Sum(s => s.Projects.Count));

            // Update archive status for all projects
            UpdateProjectArchiveStatus();
        }

        /// <summary>
        /// Updates the IsArchived status for all projects
        /// </summary>
        public void UpdateProjectArchiveStatus()
        {
            foreach (var solution in _solutions)
            {
                foreach (var project in solution.Projects)
                {
                    // Check if all archives for this project exist
                    project.IsArchived = _fileSystemService.AreAllArchivesExisting(project, project.SupportedRuntimes);

                    if (project.IsArchived)
                    {
                        _loggingService.Debug("Project {ProjectName} is already archived", project.Name);
                    }
                    else
                    {
                        _loggingService.Debug("Project {ProjectName} needs to be archived", project.Name);
                    }
                }
            }
        }

        // Private Methods
        private void DiscoverProjects(SolutionInfo solution)
        {
            // Find all C# project files in the solution directory and subdirectories
            var projectFiles = _fileSystemService.EnumerateFiles(solution.DirectoryPath, "*.csproj", true);

            foreach (var projectFile in projectFiles)
            {
                string projectDirectory = Path.GetDirectoryName(projectFile) ?? string.Empty;
                string projectName = Path.GetFileNameWithoutExtension(projectFile);

                if (_fileSystemService.IsIgnored(projectFile, Path.GetRelativePath(solution.DirectoryPath, projectFile)))
                {
                    _loggingService.Debug("Ignoring project {ProjectName} because it matches ignore patterns", projectName);
                    continue;
                }

                _loggingService.Information("Found project: {ProjectName} at {ProjectFile}", projectName, projectFile);

                var project = new ProjectInfo(solution, projectName, projectDirectory, projectFile);

                // Initialize inherited properties from configuration
                var solutionConfig = _settings.Solutions.FirstOrDefault(s => s.Name == solution.Name);
                var projectConfig = solutionConfig?.Projects.FirstOrDefault(p => p.Name == projectName);

                // Call InitializeInheritedProperties even if projectConfig is null
                // This ensures the project inherits properties from global settings and solution configuration
                project.InitializeInheritedProperties(_settings, solutionConfig, projectConfig);

                solution.Projects.Add(project);

                // Extract version information
                ExtractVersionInfo(project);
            }
        }

        private void ExtractVersionInfo(ProjectInfo project)
        {
            try
            {
                // Extract version from project file
                if (File.Exists(project.FilePath))
                {
                    var projectXml = XDocument.Load(project.FilePath);
                    var versionElement = projectXml.Descendants("Version").FirstOrDefault();

                    if (versionElement != null)
                    {
                        string versionString = versionElement.Value;
                        var versionSource = new VersionSource(VersionSourceType.CsprojFile, project.FilePath, versionString);
                        project.VersionManager.VersionSources.Add(versionSource);

                        _loggingService.Debug("Extracted version {Version} from project file {ProjectFile}",
                            versionString, project.FilePath);
                    }
                }

                // Extract version from AssemblyInfo.cs if it exists
                string assemblyInfoPath = Path.Combine(project.DirectoryPath, "Properties", "AssemblyInfo.cs");
                if (File.Exists(assemblyInfoPath))
                {
                    string[] lines = File.ReadAllLines(assemblyInfoPath);
                    foreach (var line in lines)
                    {
                        if (line.Contains("AssemblyVersion") && line.Contains("\""))
                        {
                            int startIndex = line.IndexOf("\"") + 1;
                            int endIndex = line.LastIndexOf("\"");
                            if (startIndex > 0 && endIndex > startIndex)
                            {
                                string versionString = line.Substring(startIndex, endIndex - startIndex);
                                var versionSource = new VersionSource(VersionSourceType.AssemblyInfo, assemblyInfoPath, versionString);
                                project.VersionManager.VersionSources.Add(versionSource);

                                _loggingService.Debug("Extracted version {Version} from AssemblyInfo.cs", versionString);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error extracting version information for project {ProjectName}", project.Name);
            }
        }

    }
}