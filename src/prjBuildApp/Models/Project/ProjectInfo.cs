using System.Collections.Generic;
using prjBuildApp.Models.Configuration;

namespace prjBuildApp.Models.Project
{
    public class ProjectInfo : InheritedPropertiesBase
    {
        public SolutionInfo Solution { get; }
        public string Name { get; }
        public string DirectoryPath { get; }
        public string FilePath { get; }
        public VersionManager VersionManager { get; }
        public LinkedList<ProjectInfo> ReferencedProjects { get; }

        public ProjectInfo(SolutionInfo solution, string name, string directoryPath, string filePath)
        {
            Solution = solution;
            Name = name;
            DirectoryPath = directoryPath;
            FilePath = filePath;
            VersionManager = new VersionManager();
            ReferencedProjects = new LinkedList<ProjectInfo>();
        }

        /// <summary>
        /// Initializes inherited properties from global settings, solution configuration, and project configuration
        /// </summary>
        /// <param name="globalSettings">Global application settings</param>
        /// <param name="solutionConfig">Solution-specific configuration</param>
        /// <param name="projectConfig">Project-specific configuration</param>
        public void InitializeInheritedProperties(Settings? globalSettings, SolutionConfig? solutionConfig, ProjectConfig projectConfig)
        {
            if (projectConfig != null)
            {
                IsObsolete = projectConfig.IsObsolete;

                // Merge ignore lists from all three levels: global, solution, and project
                IgnoredObjectNames.Clear();
                IgnoredObjectNames.AddRange(MergeIgnoreLists(
                    globalSettings?.IgnoredObjectNames,
                    solutionConfig?.IgnoredObjectNames,
                    projectConfig.IgnoredObjectNames
                ));

                IgnoredObjectRelativePaths.Clear();
                IgnoredObjectRelativePaths.AddRange(MergeIgnoreLists(
                    globalSettings?.IgnoredObjectRelativePaths,
                    solutionConfig?.IgnoredObjectRelativePaths,
                    projectConfig.IgnoredObjectRelativePaths
                ));
            }
        }

        // Methods
        public List<string> Build() => new();
        public List<string> Restore() => new();
        public List<string> UpdateNuGetPackages() => new();
        public List<string> Cleanup() => new();
        public List<string> Rebuild() => new();
        public List<string> Archive() => new();
    }
}