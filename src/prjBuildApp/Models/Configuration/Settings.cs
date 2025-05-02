using System.Collections.Generic;

namespace prjBuildApp.Models.Configuration
{
    public class Settings
    {
        // Core configuration
        public List<RootDirectoryConfig> RootDirectories { get; set; } = new();
        public List<SolutionConfig> Solutions { get; set; } = new();

        // Global ignore patterns
        public List<string> IgnoredObjectNames { get; set; } = new();
        public List<string> IgnoredObjectRelativePaths { get; set; } = new();
    }

    public class RootDirectoryConfig
    {
        public string DirectoryPath { get; set; } = string.Empty;
        public string ArchiveDirectoryPath { get; set; } = string.Empty;
    }

    public class SolutionConfig
    {
        public string Name { get; set; } = string.Empty;
        public bool IsObsolete { get; set; } = false;
        public RootDirectoryConfig? ParentRootDirectory { get; set; }
        public List<ProjectConfig> Projects { get; set; } = new();

        // Solution-specific ignore patterns
        public List<string> IgnoredObjectNames { get; set; } = new();
        public List<string> IgnoredObjectRelativePaths { get; set; } = new();
    }

    public class ProjectConfig
    {
        // Core configuration
        public string Name { get; set; } = string.Empty;
        public bool IsObsolete { get; set; } = false;
        public List<string> SupportedRuntimes { get; set; } = new();

        // Project-specific ignore patterns
        public List<string> IgnoredObjectNames { get; set; } = new();
        public List<string> IgnoredObjectRelativePaths { get; set; } = new();
    }
}