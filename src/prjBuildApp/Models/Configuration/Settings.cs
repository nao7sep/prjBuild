using System.Collections.Generic;

namespace prjBuildApp.Models.Configuration
{
    public class Settings
    {
        // Core configuration
        public List<RootDirectoryConfig> RootDirectories { get; set; } = new();
        public List<SolutionConfig> Solutions { get; set; } = new();

        // Global ignore patterns
        public List<string> IgnoredObjectNames { get; set; } = new(); // File system objects (files or directories) to ignore by name
        public List<string> IgnoredObjectRelativePaths { get; set; } = new(); // File system objects to ignore by relative path
    }

    public class RootDirectoryConfig
    {
        public string DirectoryPath { get; set; } = string.Empty;
        public string ArchiveDirectoryPath { get; set; } = string.Empty;
    }

    public class SolutionConfig
    {
        public string Name { get; set; } = string.Empty;
        public RootDirectoryConfig? ParentRootDirectory { get; set; }
        public List<ProjectConfig> Projects { get; set; } = new();
    }

    public class ProjectConfig
    {
        // Core configuration
        public string Name { get; set; } = string.Empty;
        public List<string> SupportedRuntimes { get; set; } = new();

        // Project-specific ignore patterns
        public List<string> IgnoredObjectNames { get; set; } = new(); // File system objects (files or directories) to ignore by name
        public List<string> IgnoredObjectRelativePaths { get; set; } = new(); // File system objects to ignore by relative path
    }
}