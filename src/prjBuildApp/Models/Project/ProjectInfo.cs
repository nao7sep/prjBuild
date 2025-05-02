using System.Collections.Generic;

namespace prjBuildApp.Models.Project
{
    public class ProjectInfo
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

        // Methods
        public List<string> Build() => new();
        public List<string> Restore() => new();
        public List<string> UpdateNuGetPackages() => new();
        public List<string> Cleanup() => new();
        public List<string> Rebuild() => new();
        public List<string> Archive() => new();
    }
}