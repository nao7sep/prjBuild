# prjBuild v0.1 Specifications

## 1. Overview

prjBuild is a .NET-based command-line application designed to automate the building and archiving of .NET projects across multiple repositories. It identifies changed projects, manages their dependencies, and provides an interactive interface for building, updating, and archiving these projects.

## 2. Purpose

The primary purpose of prjBuild is to streamline the development workflow by:

- Automatically identifying which projects have changed and need to be rebuilt
- Managing project dependencies to ensure correct build order
- Providing a consistent way to update NuGet packages across projects
- Creating standardized archives of both binaries and source code
- Supporting multiple target runtimes

## 3. Architecture

### 3.1 Core Architecture

prjBuild follows a modular, layered architecture:

```
┌─────────────────────────────────────────────────────────┐
│                     User Interface                       │
│  (Command-line interface with interactive menu options)  │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────┴─────────────────────────────┐
│                    Application Logic                     │
│  (Project discovery, dependency resolution, operations)  │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────┴─────────────────────────────┐
│                       Core Services                      │
│  (Configuration, logging, file operations, .NET tools)   │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Component Diagram

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Configuration  │     │     Logging     │     │  File System    │
│    Management   │     │                 │     │   Operations    │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         └───────────────┬───────┴───────────────┬───────┘
                         │                       │
                ┌────────┴────────┐     ┌────────┴────────┐
                │  Solution/Project│     │   Build and     │
                │    Management   │     │ Archive Service │
                └────────┬────────┘     └────────┬────────┘
                         │                       │
                         └───────────┬───────────┘
                                     │
                           ┌─────────┴─────────┐
                           │  Command-line UI  │
                           └───────────────────┘
```

## 4. Core Components

### 4.1 Configuration Management

**Purpose**: Manage application settings and configuration.

**Key Features**:
- Load settings from JSON configuration files
- Support for both public (repository-level) and private (user-level) settings
- Merge settings from multiple sources with appropriate precedence
- Provide strongly-typed access to configuration values

**Implementation Details**:
- Use `System.Text.Json` for JSON parsing
- Support environment variable expansion in configuration values
- Implement change tracking for configuration modifications
- Provide validation for configuration values

### 4.2 Logging System

**Purpose**: Provide comprehensive logging capabilities.

**Key Features**:
- Log to both console and file simultaneously
- Support different log levels (Info, Warning, Error, Debug)
- Color-coded console output for better readability
- Structured logging for machine-readable logs
- Log rotation and archiving

**Implementation Details**:
- Use custom logging implementation for maximum control
- Support for indentation levels in log messages
- Automatic timestamp and log level prefixing
- Configurable log file location and retention policy

### 4.3 File System Operations

**Purpose**: Handle file and directory operations safely and efficiently.

**Key Features**:
- File reading and writing with encoding detection
- Directory creation and enumeration
- File archiving (ZIP)
- Path manipulation utilities

**Implementation Details**:
- Use `System.IO` namespace for file operations
- Implement safe file writing with backup capabilities
- Support for UTF-8 and other encodings
- Efficient handling of large files

### 4.4 Solution/Project Management

**Purpose**: Discover, analyze, and manage .NET solutions and projects.

**Key Features**:
- Solution and project discovery
- Project reference resolution
- Version extraction and validation
- Dependency graph construction

**Implementation Details**:
- Parse .csproj and .sln files using XML APIs
- Extract version information from various sources (csproj, AssemblyInfo.cs, app.manifest)
- Build dependency graph for correct build ordering
- Support for both SDK-style and legacy project formats

### 4.5 Build and Archive Service

**Purpose**: Build projects and create standardized archives.

**Key Features**:
- Project building with appropriate options
- NuGet package updates
- Binary and source code archiving
- Support for multiple target runtimes

**Implementation Details**:
- Use `dotnet` CLI commands for building and package management
- Implement clean, build, and publish operations
- Create ZIP archives with appropriate filtering
- Handle build errors and warnings

### 4.6 Command-line UI

**Purpose**: Provide an interactive interface for the user.

**Key Features**:
- Display project information in a readable format
- Interactive menu for common operations
- Progress reporting for long-running operations
- Error handling and reporting

**Implementation Details**:
- Use `System.Console` for user interaction
- Implement a menu-based interface
- Support for both interactive and non-interactive modes
- Color-coded output for better readability

## 5. Data Models

### 5.1 Settings Model

```csharp
public class Settings
{
    public string RepositoriesDirectoryPath { get; set; }
    public string ArchivesDirectoryPath { get; set; }
    public List<string> IgnoredDirectoryNames { get; set; }
    public List<string> ObsoleteSolutionNames { get; set; }
    public List<string> SupportedRuntimes { get; set; }
    public List<string> NotArchivedDirectoryNames { get; set; }
    public List<string> NotArchivedFileNames { get; set; }
}
```

### 5.2 Solution Model

```csharp
public class SolutionInfo
{
    public string Name { get; }
    public string DirectoryPath { get; }
    public string FilePath { get; }
    public bool IsObsolete { get; }
    public List<ProjectInfo> Projects { get; }
    public string CommonVersionString { get; }
    public string SourceArchiveFilePath { get; }

    // Methods
    public List<string> Archive(List<string> notArchivedDirectoryNames, List<string> notArchivedFileNames);
}
```

### 5.3 Project Model

```csharp
public class ProjectInfo
{
    public SolutionInfo Solution { get; }
    public string Name { get; }
    public string DirectoryPath { get; }
    public string FilePath { get; }
    public string VersionString { get; }
    public List<ProjectInfo> ReferencedProjects { get; }

    // Methods
    public List<string> Build(bool noRestore);
    public List<string> UpdateNuGetPackages();
    public List<string> Clean(List<string> supportedRuntimes, bool deleteObjDirectory);
    public List<string> RebuildAndArchive(List<string> supportedRuntimes,
                                         List<string> notArchivedDirectoryNames,
                                         List<string> notArchivedFileNames);
}
```

## 6. Key Features and Workflows

### 6.1 Project Discovery Workflow

1. Load configuration settings
2. Enumerate directories in the repositories directory
3. Filter out ignored directories
4. Find solution files in each directory
5. Create SolutionInfo objects for valid solutions
6. For each solution:
   - Find project directories
   - Filter out ignored directories
   - Find project files in each directory
   - Create ProjectInfo objects for valid projects
   - Associate projects with their solutions

### 6.2 Version and Reference Resolution

1. For each project:
   - Extract version information from project file, AssemblyInfo.cs, or app.manifest
   - Validate version format
   - Extract project references from project file
   - Resolve referenced projects
   - Build dependency graph

### 6.3 Build and Archive Workflow

1. Identify projects that need to be built (those without archives)
2. Add referenced projects to the build list
3. Sort projects by dependency order
4. Present interactive menu with options:
   - Build projects
   - Update NuGet packages
   - Rebuild and archive projects
   - Exclude a project from the build list
   - Exit

### 6.4 Interactive Menu Options

#### 6.4.1 Build
- For each project in the build list:
  - Execute `dotnet build` with appropriate options
  - Display build output
  - Handle build errors

#### 6.4.2 Update NuGet Packages
- For each project in the build list:
  - List outdated packages
  - Update each package to the latest version
  - Display update output
  - Handle update errors

#### 6.4.3 Rebuild and Archive
- For each project in the build list:
  - Clean the project
  - Rebuild the project for each supported runtime
  - Archive the built binaries
  - Archive the source code
  - Display archive output
  - Handle archive errors

#### 6.4.4 Exclude Project
- Display list of projects in the build list
- Allow user to select a project to exclude
- Remove the selected project from the build list

## 7. Technical Requirements

### 7.1 Development Environment
- .NET 9.0 SDK
- C# 12.0 or later
- Visual Studio 2022 or later / Visual Studio Code with C# extensions

### 7.2 Runtime Requirements
- .NET 9.0 Runtime
- Windows, macOS, or Linux operating system

### 7.3 Dependencies
- System.CommandLine (for advanced command-line parsing)
- System.Text.Json (for JSON configuration)

### 7.4 Performance Considerations
- Efficient handling of large solutions with many projects
- Minimal memory footprint
- Responsive user interface even during long-running operations

## 8. Implementation Details

### 8.1 Configuration System

The configuration system will use a layered approach:

1. Default settings hardcoded in the application
2. Public settings in a JSON file in the application directory
3. Private settings in a JSON file in the user's home directory

Settings will be merged with later layers taking precedence over earlier ones.

```csharp
// Example configuration loading
public static async Task<Settings> LoadAsync()
{
    var settings = new Settings();

    // Load defaults
    settings.ApplyDefaults();

    // Load public settings
    if (File.Exists(GetPublicSettingsPath()))
    {
        var publicSettings = await LoadSettingsFileAsync(GetPublicSettingsPath());
        settings.Merge(publicSettings);
    }

    // Load private settings
    if (File.Exists(GetPrivateSettingsPath()))
    {
        var privateSettings = await LoadSettingsFileAsync(GetPrivateSettingsPath());
        settings.Merge(privateSettings);
    }

    return settings;
}
```

### 8.2 Logging System

The logging system will support both console and file logging:

```csharp
public static void LogInfo(string message, string indentation = "", bool flush = false)
{
    // Write to console with color
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"{indentation}{message}");
    Console.ResetColor();

    // Write to log file
    File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} INFO: {indentation}{message}{Environment.NewLine}");

    if (flush)
        FlushLogFile();
}
```

### 8.3 Project Discovery

Project discovery will use a combination of directory enumeration and file pattern matching:

```csharp
public static List<SolutionInfo> DiscoverSolutions(string repositoriesPath, List<string> ignoredDirectories)
{
    var solutions = new List<SolutionInfo>();

    foreach (var directory in Directory.GetDirectories(repositoriesPath))
    {
        var dirInfo = new DirectoryInfo(directory);

        if (ignoredDirectories.Contains(dirInfo.Name, StringComparer.OrdinalIgnoreCase))
            continue;

        var solutionFiles = Directory.GetFiles(directory, "*.sln");

        if (solutionFiles.Length == 1)
        {
            var isObsolete = obsoleteSolutionNames.Contains(dirInfo.Name, StringComparer.OrdinalIgnoreCase);
            solutions.Add(new SolutionInfo(solutions, archivesDirectoryPath, dirInfo.Name, directory, solutionFiles[0], isObsolete));
        }
    }

    return solutions;
}
```

### 8.4 Version Extraction

Version extraction will support multiple sources:

```csharp
public string ExtractVersionFromCsproj(string projectFilePath)
{
    var doc = XDocument.Load(projectFilePath);
    var propertyGroup = doc.Root.Element("PropertyGroup");

    if (propertyGroup != null)
    {
        var versionElement = propertyGroup.Element("Version");
        if (versionElement != null)
            return versionElement.Value;
    }

    return null;
}

public string ExtractVersionFromAssemblyInfo(string assemblyInfoPath)
{
    var content = File.ReadAllText(assemblyInfoPath);
    var match = Regex.Match(content, @"\[assembly:\s*AssemblyVersion\s*\(""(?<version>\d+\.\d+(\.\d+){0,2})""\)\]", RegexOptions.IgnoreCase);

    if (match.Success)
        return match.Groups["version"].Value;

    return null;
}
```

### 8.5 Dependency Resolution

Dependency resolution will build a graph of project dependencies:

```csharp
public List<ProjectInfo> SortProjectsByDependency(List<ProjectInfo> projects)
{
    var referenceTable = projects.ToDictionary(p => p, p => GetAllReferencedProjects(p));
    var sortedProjects = new List<ProjectInfo>();

    foreach (var project in projects)
    {
        bool isReferenced = false;

        for (int i = 0; i < sortedProjects.Count; i++)
        {
            if (referenceTable[sortedProjects[i]].Contains(project))
            {
                sortedProjects.Insert(i, project);
                isReferenced = true;
                break;
            }
        }

        if (!isReferenced)
            sortedProjects.Add(project);
    }

    return sortedProjects;
}
```

### 8.6 Build Operations

Build operations will use the .NET CLI:

```csharp
public List<string> Build(bool noRestore)
{
    var arguments = new List<string> { "build", FilePath, "--configuration", "Release" };

    if (noRestore)
        arguments.Add("--no-restore");

    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = string.Join(" ", arguments),
            WorkingDirectory = DirectoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.Start();
    var output = process.StandardOutput.ReadToEnd();
    var error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    var messages = new List<string>();

    if (!string.IsNullOrEmpty(output))
        messages.AddRange(output.Split(Environment.NewLine));

    if (!string.IsNullOrEmpty(error))
        messages.AddRange(error.Split(Environment.NewLine));

    return messages;
}
```

### 8.7 Archive Operations

Archive operations will create ZIP files:

```csharp
public List<string> ArchiveSolution(List<string> notArchivedDirectoryNames, List<string> notArchivedFileNames)
{
    var messages = new List<string>();
    var archiveDirectory = Path.GetDirectoryName(SourceArchiveFilePath);

    Directory.CreateDirectory(archiveDirectory);

    using (var archive = ZipFile.Open(SourceArchiveFilePath, ZipArchiveMode.Create))
    {
        var fileCount = AddDirectoryToArchive(archive, DirectoryPath, "", notArchivedDirectoryNames, notArchivedFileNames);

        if (fileCount > 0)
            messages.Add($"Archive file created: {SourceArchiveFilePath}");
        else
        {
            File.Delete(SourceArchiveFilePath);
            messages.Add($"Empty archive file deleted: {SourceArchiveFilePath}");
        }
    }

    return messages;
}
```

## 9. Future Enhancements

### 9.1 Version 0.2
- Add support for non-interactive mode (command-line arguments)
- Implement parallel building for independent projects
- Add support for custom build configurations

### 9.2 Version 0.3
- Add support for MSBuild projects (.vcxproj, etc.)
- Implement build caching for faster rebuilds
- Add support for custom build steps

### 9.3 Version 1.0
- Add a simple web interface for remote build monitoring
- Implement CI/CD integration
- Add support for cross-platform builds

## 10. Conclusion

prjBuild v0.1 provides a solid foundation for automating the build and archive process for .NET projects. By implementing the specifications outlined in this document, the application will offer a significant improvement in developer productivity and build consistency.