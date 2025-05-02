using System.Collections.Generic;

namespace prjBuildApp.Models.Project
{
    public class SolutionInfo
    {
        public string Name { get; }
        public string DirectoryPath { get; }
        public string FilePath { get; }
        public List<ProjectInfo> Projects { get; } = new();
        public string SourceArchivePath { get; set; }
        public bool IsObsolete { get; set; }

        public SolutionInfo(string name, string directoryPath, string filePath)
        {
            Name = name;
            DirectoryPath = directoryPath;
            FilePath = filePath;
            SourceArchivePath = string.Empty;
            IsObsolete = false;
        }
    }
}