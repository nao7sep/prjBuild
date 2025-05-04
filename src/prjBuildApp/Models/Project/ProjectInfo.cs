using System.Collections.Generic;
using prjBuildApp.Models.Configuration;

namespace prjBuildApp.Models.Project
{
    public class ProjectInfo : InheritedPropertiesBase
    {
        public ProjectInfo(SolutionInfo solution, string name, string directoryPath, string filePath)
        {
            Solution = solution;
            Name = name;
            DirectoryPath = directoryPath;
            FilePath = filePath;
            VersionManager = new VersionManager();
            ReferencedProjects = new List<ProjectInfo>();
            RuntimeArchivePaths = new Dictionary<string, string>();
        }

        public SolutionInfo Solution { get; }
        public string Name { get; }
        public string DirectoryPath { get; }
        public string FilePath { get; }
        public VersionManager VersionManager { get; }
        public List<ProjectInfo> ReferencedProjects { get; }
        public List<string> SupportedRuntimes { get; } = new();

        /// <summary>
        /// Dictionary mapping runtime identifiers to their executable archive file paths
        /// </summary>
        public Dictionary<string, string> RuntimeArchivePaths { get; }

        /// <summary>
        /// Initializes inherited properties from global settings, solution configuration, and project configuration
        /// </summary>
        /// <param name="globalSettings">Global application settings</param>
        /// <param name="solutionConfig">Solution-specific configuration</param>
        /// <param name="projectConfig">Project-specific configuration</param>
        public void InitializeInheritedProperties(Settings? globalSettings, SolutionConfig? solutionConfig, ProjectConfig? projectConfig)
        {
            // Initialize supported runtimes from project configuration
            SupportedRuntimes.Clear();
            if (projectConfig?.SupportedRuntimes != null && projectConfig.SupportedRuntimes.Count > 0)
            {
                SupportedRuntimes.AddRange(projectConfig.SupportedRuntimes);
            }
            // No fallback to win-x64 - if no runtimes are specified, the list remains empty

            // Merge ignore lists from all three levels: global, solution, and project
            IgnoredObjectNames.Clear();
            var mergedNames = new List<string>();
            if (globalSettings?.IgnoredObjectNames != null)
            {
                mergedNames.AddRange(globalSettings.IgnoredObjectNames);
            }

            if (solutionConfig?.IgnoredObjectNames != null)
            {
                mergedNames.AddRange(solutionConfig.IgnoredObjectNames);
            }

            if (projectConfig?.IgnoredObjectNames != null)
            {
                mergedNames.AddRange(projectConfig.IgnoredObjectNames);
            }
            IgnoredObjectNames.AddRange(mergedNames.Distinct());

            IgnoredObjectRelativePaths.Clear();
            var mergedPaths = new List<string>();
            if (globalSettings?.IgnoredObjectRelativePaths != null)
            {
                mergedPaths.AddRange(globalSettings.IgnoredObjectRelativePaths);
            }

            if (solutionConfig?.IgnoredObjectRelativePaths != null)
            {
                mergedPaths.AddRange(solutionConfig.IgnoredObjectRelativePaths);
            }

            if (projectConfig?.IgnoredObjectRelativePaths != null)
            {
                mergedPaths.AddRange(projectConfig.IgnoredObjectRelativePaths);
            }
            IgnoredObjectRelativePaths.AddRange(mergedPaths.Distinct());
        }

        /// <summary>
        /// Validates that all version sources within this project have the same version
        /// </summary>
        /// <returns>True if all versions match, false otherwise</returns>
        public bool ValidateVersions()
        {
            return VersionManager.ValidateProjectVersions();
        }
    }
}