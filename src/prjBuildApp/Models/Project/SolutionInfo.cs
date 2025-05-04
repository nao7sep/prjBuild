using System.Collections.Generic;
using prjBuildApp.Models.Configuration;

namespace prjBuildApp.Models.Project
{
    public class SolutionInfo : InheritedPropertiesBase
    {
        public SolutionInfo(string name, string directoryPath, string filePath)
        {
            Name = name;
            DirectoryPath = directoryPath;
            FilePath = filePath;
            SourceArchivePath = string.Empty;
            ArchiveDirectoryPath = string.Empty;
        }

        public string Name { get; }
        public string DirectoryPath { get; }
        public string FilePath { get; }
        public List<ProjectInfo> Projects { get; } = new();

        /// <summary>
        /// Path to the directory where solution archives are stored
        /// </summary>
        public string ArchiveDirectoryPath { get; set; }

        /// <summary>
        /// Path to the source code archive file
        /// </summary>
        public string SourceArchivePath { get; set; }

        /// <summary>
        /// Initializes inherited properties from global settings and solution configuration
        /// </summary>
        /// <param name="globalSettings">Global application settings</param>
        /// <param name="solutionConfig">Solution-specific configuration</param>
        public void InitializeInheritedProperties(Settings? globalSettings, SolutionConfig? solutionConfig)
        {
            // Merge ignore lists from global settings and solution configuration
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
            IgnoredObjectRelativePaths.AddRange(mergedPaths.Distinct());
        }

        /// <summary>
        /// Validates that all projects in this solution have the same primary version
        /// </summary>
        /// <returns>True if all projects have the same primary version, false otherwise</returns>
        public bool ValidateVersions()
        {
            return VersionManager.ValidateSolutionAndProjectVersions(this);
        }
    }
}