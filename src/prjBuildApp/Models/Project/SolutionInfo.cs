using System.Collections.Generic;
using prjBuildApp.Models.Configuration;

namespace prjBuildApp.Models.Project
{
    public class SolutionInfo : InheritedPropertiesBase
    {
        public string Name { get; }
        public string DirectoryPath { get; }
        public string FilePath { get; }
        public List<ProjectInfo> Projects { get; } = new();
        public string SourceArchivePath { get; set; }

        public SolutionInfo(string name, string directoryPath, string filePath)
        {
            Name = name;
            DirectoryPath = directoryPath;
            FilePath = filePath;
            SourceArchivePath = string.Empty;
        }

        /// <summary>
        /// Initializes inherited properties from global settings and solution configuration
        /// </summary>
        /// <param name="globalSettings">Global application settings</param>
        /// <param name="solutionConfig">Solution-specific configuration</param>
        public void InitializeInheritedProperties(Settings? globalSettings, SolutionConfig? solutionConfig)
        {
            // Set IsObsolete from solution configuration if available
            if (solutionConfig != null)
            {
                IsObsolete = solutionConfig.IsObsolete;
            }

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
    }
}