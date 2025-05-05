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

        public ProjectManagementService(LoggingService loggingService, FileSystemService fileSystemService, Settings settings)
        {
            _loggingService = loggingService;
            _fileSystemService = fileSystemService;
            _settings = settings;
        }

        public List<SolutionInfo> Solutions => _solutions;

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
                        _solutions.Add(solution);

                        // Initialize inherited properties from configuration
                        var solutionConfig = _settings.Solutions.FirstOrDefault(s => s.Name == solutionName);

                        // Call InitializeInheritedProperties even if solutionConfig is null
                        // This ensures the solution inherits properties from global settings
                        solution.InitializeInheritedProperties(_settings, solutionConfig);

                        // Initialize archive directory path
                        solution.ArchiveDirectoryPath = _fileSystemService.GetSolutionArchiveDirectory(solution);
                        _loggingService.Debug("Solution archive directory: {ArchiveDirectoryPath}", solution.ArchiveDirectoryPath);

                        // Discover projects in the solution
                        DiscoverProjects(solution);

                        // Initialize source archive path
                        string sourceArchivePath = _fileSystemService.GetSolutionSourceArchivePath(solution);
                        solution.SourceArchivePath = sourceArchivePath;
                        _loggingService.Debug("Solution source archive path: {SourceArchivePath}", solution.SourceArchivePath);

                        // Validate versions across all projects in the solution
                        if (!solution.ValidateVersions())
                        {
                            _loggingService.Warning("Solution {SolutionName} has projects with mismatched versions", solution.Name);
                        }
                        else
                        {
                            _loggingService.Information("Solution {SolutionName} has consistent versions across all projects", solution.Name);
                        }

                        // Check if all archives for this solution exist
                        solution.AreAllArchivesExisting = _fileSystemService.AreAllArchivesExisting(solution);

                        if (solution.AreAllArchivesExisting)
                        {
                            _loggingService.Debug("Solution {SolutionName} is already archived", solution.Name);
                        }
                        else
                        {
                            _loggingService.Debug("Solution {SolutionName} needs to be archived", solution.Name);
                        }
                    }
                }
            }

            _loggingService.Information("Discovered {SolutionCount} solutions with {ProjectCount} projects",
                _solutions.Count, _solutions.Sum(s => s.Projects.Count));
        }


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
                solution.Projects.Add(project);

                // Extract version information
                ExtractVersionInfo(project);

                // Initialize inherited properties from configuration
                var solutionConfig = _settings.Solutions.FirstOrDefault(s => s.Name == solution.Name);
                var projectConfig = solutionConfig?.Projects.FirstOrDefault(p => p.Name == projectName);

                // Call InitializeInheritedProperties even if projectConfig is null
                // This ensures the project inherits properties from global settings and solution configuration
                project.InitializeInheritedProperties(_settings, solutionConfig, projectConfig);

                // Get all binary archive paths
                var archivePaths = _fileSystemService.GetProjectRuntimeArchivePaths(project);

                // Initialize runtime archive paths
                foreach (var runtime in project.SupportedRuntimes)
                {
                    project.RuntimeArchivePaths[runtime] = archivePaths[runtime];
                    _loggingService.Debug("Project runtime archive path for {Runtime}: {ArchivePath}",
                        runtime, archivePaths[runtime]);
                }

                // Initialize ExcludeFromArchiving property based on project configuration
                project.ExcludeFromArchiving = projectConfig?.ExcludeFromArchiving; // Null by default (will be archived)

                // Check if all versions within the project match
                if (!project.ValidateVersions())
                {
                    _loggingService.Warning("Project {ProjectName} has mismatched versions", project.Name);
                }
                else if (project.VersionManager.VersionSources.Count > 0)
                {
                    _loggingService.Information("Project {ProjectName} has consistent versions", project.Name);
                }

                // Check if the project has at least one supported runtime
                if (project.SupportedRuntimes.Count == 0)
                {
                    _loggingService.Warning("Project {ProjectName} has no supported runtimes defined", project.Name);
                }
                else
                {
                    _loggingService.Information("Project {ProjectName} has {RuntimeCount} supported runtime(s): {Runtimes}",
                        project.Name, project.SupportedRuntimes.Count, string.Join(", ", project.SupportedRuntimes));
                }
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
                        if (line.Contains("AssemblyVersion") && line.Contains('\"'))
                        {
                            int startIndex = line.IndexOf('\"') + 1;
                            int endIndex = line.LastIndexOf('\"');
                            if (startIndex > 0 && endIndex > startIndex)
                            {
                                string versionString = line.Substring(startIndex, endIndex - startIndex);
                                var versionSource = new VersionSource(VersionSourceType.AssemblyInfo, assemblyInfoPath, versionString);
                                project.VersionManager.VersionSources.Add(versionSource);

                                _loggingService.Debug("Extracted version {Version} from assembly info file {AssemblyInfoFile}",
                                    versionString, assemblyInfoPath);
                            }
                        }
                    }
                }

                // Extract version from app.manifest if it exists
                string manifestPath = Path.Combine(project.DirectoryPath, "app.manifest");
                if (!File.Exists(manifestPath))
                {
                    // Also check in Properties folder
                    manifestPath = Path.Combine(project.DirectoryPath, "Properties", "app.manifest");
                }

                if (File.Exists(manifestPath))
                {
                    try
                    {
                        var manifestXml = XDocument.Load(manifestPath);
                        XNamespace ns = "urn:schemas-microsoft-com:asm.v1";

                        // Look for assemblyIdentity element with version attribute
                        var assemblyIdentity = manifestXml.Descendants(ns + "assemblyIdentity").FirstOrDefault();
                        if (assemblyIdentity != null)
                        {
                            var versionAttribute = assemblyIdentity.Attribute("version");
                            if (versionAttribute != null)
                            {
                                string versionString = versionAttribute.Value;
                                var versionSource = new VersionSource(VersionSourceType.Manifest, manifestPath, versionString);
                                project.VersionManager.VersionSources.Add(versionSource);

                                _loggingService.Debug("Extracted version {Version} from manifest file {ManifestFile}",
                                    versionString, manifestPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Warning("Error parsing app.manifest file {ManifestPath}: {ErrorMessage}", manifestPath, ex.Message);
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