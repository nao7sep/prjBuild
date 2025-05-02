# prjBuild v0.1 Specifications

## Table of Contents

1. [Introduction](#1-introduction)
   1. [Overview](#11-overview)
   2. [Purpose and Goals](#12-purpose-and-goals)
2. [System Architecture](#2-system-architecture)
   1. [Architectural Layers](#21-architectural-layers)
   2. [Component Structure](#22-component-structure)
   3. [Component Interactions](#23-component-interactions)
3. [Core Components](#3-core-components)
   1. [Configuration Management](#31-configuration-management)
   2. [Logging System](#32-logging-system)
   3. [File System Operations](#33-file-system-operations)
   4. [Solution/Project Management](#34-solutionproject-management)
   5. [Build and Archive Service](#35-build-and-archive-service)
   6. [Command-line UI](#36-command-line-ui)
4. [Data Models and Conventions](#4-data-models-and-conventions)
   1. [Naming Conventions](#41-naming-conventions)
   2. [Settings Model](#42-settings-model)
   3. [Solution Model](#43-solution-model)
   4. [Project Model](#44-project-model)
5. [Key Features and Workflows](#5-key-features-and-workflows)
   1. [Project Discovery Workflow](#51-project-discovery-workflow)
   2. [Version and Reference Resolution](#52-version-and-reference-resolution)
   3. [Project Selection and Operations](#53-project-selection-and-operations)
   4. [Interactive Menu Options](#54-interactive-menu-options)
6. [Technical Requirements](#6-technical-requirements)
   1. [Development Environment](#61-development-environment)
   2. [Runtime Requirements](#62-runtime-requirements)
   3. [Dependencies](#63-dependencies)
   4. [Performance Considerations](#64-performance-considerations)
7. [Implementation Details](#7-implementation-details)
   1. [Configuration System](#71-configuration-system)
   2. [Logging System](#72-logging-system)
   3. [Project Discovery](#73-project-discovery)
   4. [Version Extraction](#74-version-extraction)
   5. [Dependency Resolution](#75-dependency-resolution)
   6. [Build Operations](#76-build-operations)
   7. [Archive Operations](#77-archive-operations)
8. [Error Handling and Validation](#8-error-handling-and-validation)
   1. [Input Validation](#81-input-validation)
   2. [Error Reporting](#82-error-reporting)
   3. [Recovery Strategies](#83-recovery-strategies)
9. [Future Enhancements](#9-future-enhancements)
   1. [Planned Features](#91-planned-features)
   2. [Extensibility Points](#92-extensibility-points)
10. [Conclusion](#10-conclusion)

## 1. Introduction

### 1.1 Overview

prjBuild is a .NET-based command-line application designed to automate the building and archiving of .NET projects across multiple repositories. It identifies changed projects, manages their dependencies, and provides an interactive interface for building, updating, and archiving these projects.

### 1.2 Purpose and Goals

The primary purpose of prjBuild is to streamline the development workflow by:

- Automatically identifying which projects have changed and need to be rebuilt
- Managing project dependencies to ensure correct build order
- Providing a consistent way to update NuGet packages across projects
- Creating standardized archives of both binaries and source code
- Supporting multiple target runtimes

## 2. System Architecture

### 2.1 Architectural Layers

prjBuild follows a modular, layered architecture consisting of three main layers:

1. **User Interface Layer**
   - Provides a command-line interface with interactive menu options
   - Handles user input and displays information in a readable format
   - Manages the interactive workflow for project operations

2. **Application Logic Layer**
   - Implements project discovery and analysis
   - Handles dependency resolution between projects
   - Coordinates operations like building, updating, and archiving

3. **Core Services Layer**
   - Manages configuration and settings
   - Provides logging capabilities
   - Handles file system operations
   - Interfaces with .NET tools and commands

This layered approach ensures separation of concerns and makes the application more maintainable and extensible.

### 2.2 Component Structure

The application consists of six main components that work together to provide the complete functionality:

1. **Configuration Management**
   - Loads and manages application settings
   - Provides access to configuration values
   - Handles platform-specific settings

2. **Logging System**
   - Records application activities and errors
   - Outputs to both console and file
   - Provides different log levels and formatting

3. **File System Operations**
   - Handles file and directory operations
   - Manages file archiving and compression
   - Provides path manipulation utilities

4. **Solution/Project Management**
   - Discovers and analyzes .NET solutions and projects
   - Extracts version information
   - Builds dependency graphs

5. **Build and Archive Service**
   - Executes build operations
   - Updates NuGet packages
   - Creates standardized archives

6. **Command-line UI**
   - Presents interactive menus
   - Displays project information
   - Reports progress and errors

### 2.3 Component Interactions

The components interact in the following manner:

- The **Command-line UI** depends on both the **Solution/Project Management** and **Build and Archive Service** to present options and execute operations.
- The **Solution/Project Management** and **Build and Archive Service** both rely on the three core services: **Configuration Management**, **Logging System**, and **File System Operations**.
- All components use the **Logging System** to record their activities.
- The **Configuration Management** component provides settings that influence the behavior of all other components.
- The **File System Operations** component is used by most other components for file access and manipulation.

This design allows for clear separation of responsibilities while enabling efficient collaboration between components.

## 3. Core Components

### 3.1 Configuration Management

**Purpose**: Manage application settings and configuration.

**Key Features**:
- Load settings from appsettings.json configuration file
- Provide strongly-typed access to configuration values
- Support platform-specific settings in a single configuration file

**Implementation Details**:
- Use `System.Text.Json` for JSON parsing
- Support environment variable expansion in configuration values
- Provide validation for configuration values

### 3.2 Logging System

**Purpose**: Provide comprehensive logging capabilities.

**Key Features**:
- Log to both console and file simultaneously
- Support different log levels (Info, Warning, Error, Debug)
- Color-coded console output for better readability
- Structured logging using NDJSON format

**Implementation Details**:
- Use Serilog for logging implementation
- JSON-formatted log entries
- Configurable log file location and retention policy

### 3.3 File System Operations

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

### 3.4 Solution/Project Management

**Purpose**: Discover, analyze, and manage .NET solutions and projects.

**Key Features**:
- Solution and project discovery across multiple directories
- Project reference resolution
- Version extraction and validation
- Dependency graph construction

**Implementation Details**:
- Parse .csproj and .sln files using XML APIs
- Extract version information from various sources (csproj, AssemblyInfo.cs, app.manifest)
- Build dependency graph for correct build ordering
- Support for both SDK-style and legacy project formats

### 3.5 Build and Archive Service

**Purpose**: Build projects and create standardized archives.

**Key Features**:
- Project building with appropriate options
- NuGet package updates
- Binary and source code archiving
- Support for multiple target runtimes

**Implementation Details**:
- Use `dotnet` CLI commands for building and package management
- Implement clean, build, restore, and publish operations
- Create ZIP archives with appropriate filtering
- Handle build errors and warnings

### 3.6 Command-line UI

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

## 4. Data Models and Conventions

### 4.1 Naming Conventions

Throughout this document and the application code, the following naming conventions are used:

- **Singular nouns as adjectives**: When a noun is used as an adjective to describe another noun, the singular form is used even when it conceptually represents multiple items. For example:
  - `SourceArchivePath` (not "SourcesArchivePath") - A path to an archive containing source code
  - `ArchiveDirectoryPath` (not "ArchivesDirectoryPath") - A path to a directory containing archives

- **Plural for collections**: Collections of items use the plural form. For example:
  - `NotArchivedDirectoryNames` - A list of directory names that should not be archived
  - `Repositories` - A list of repository configurations

These conventions follow standard English usage patterns and help maintain consistency throughout the codebase.

### 4.2 Settings Model

```csharp
public class Settings
{
    public List<RepositoryConfig> Repositories { get; set; }
    public List<string> IgnoredDirectoryNames { get; set; }
    public List<string> ObsoleteSolutionNames { get; set; }
    public List<string> ObsoleteProjectNames { get; set; }
    public List<string> SupportedRuntimes { get; set; }
    public List<string> NotArchivedDirectoryPaths { get; set; }
    public List<string> NotArchivedFilePaths { get; set; }
    public List<string> NotArchivedDirectoryNames { get; set; }
    public List<string> NotArchivedFileNames { get; set; }
}

public class RepositoryConfig
{
    public string DirectoryPath { get; set; }
    public string ArchiveDirectoryPath { get; set; }
}
```

### 4.3 Solution Model

```csharp
public class SolutionInfo
{
    public string Name { get; }
    public string DirectoryPath { get; }
    public string FilePath { get; }
    public bool IsObsolete { get; }
    public List<ProjectInfo> Projects { get; }
    public string SourceArchivePath { get; }

    // Methods
    public List<string> Archive(List<string> notArchivedDirectoryNames, List<string> notArchivedFileNames);
}
```

### 4.4 Project Model

```csharp
public class ProjectInfo
{
    public SolutionInfo Solution { get; }
    public string Name { get; }
    public string DirectoryPath { get; }
    public string FilePath { get; }
    public VersionInfo Version { get; }
    public LinkedList<ProjectInfo> ReferencedProjects { get; }

    // Methods
    public List<string> Build();
    public List<string> Restore();
    public List<string> UpdateNuGetPackages();
    public List<string> Cleanup(List<string> supportedRuntimes, bool deleteObjDirectory);
    public List<string> Rebuild(List<string> supportedRuntimes);
    public List<string> Archive(List<string> notArchivedDirectoryNames, List<string> notArchivedFileNames);
}

public class VersionInfo
{
    public int? Major { get; }
    public int? Minor { get; }
    public int? Build { get; }
    public int? Revision { get; }

    // Source properties
    public string? CsprojVersion { get; }
    public string? AssemblyVersion { get; }
    public string? AssemblyFileVersion { get; }
    public string? ManifestVersion { get; }

    // Methods
    public string GetVersion();
}
```

## 5. Key Features and Workflows

### 5.1 Project Discovery Workflow

1. Load configuration settings from appsettings.json
2. For each repository directory in the configuration:
   - Enumerate directories in the repository directory
   - Filter out ignored directories
   - Find solution files in each directory
   - Create SolutionInfo objects for valid solutions
   - For each solution:
     - Find project directories (including subdirectories)
     - Filter out ignored directories
     - Find project files in each directory
     - Create ProjectInfo objects for valid projects
     - Associate projects with their solutions

### 5.2 Version and Reference Resolution

1. For each project:
   - Extract version information from project file, AssemblyInfo.cs, or app.manifest
   - Parse version components into VersionInfo object
   - Extract project references from project file
   - Resolve referenced projects
   - Build dependency graph using linked list structure

### 5.3 Project Selection and Operations

1. Discover all projects across configured repositories
2. Present list of all projects with selection status
3. Provide options to:
   - Select/deselect individual projects
   - Select/deselect all projects
   - Select projects by pattern or criteria
4. For selected projects, present operations menu:
   - Update NuGet packages
   - Restore dependencies
   - Quick build (for testing)
   - Cleanup
   - Rebuild
   - Archive
   - Exit

### 5.4 Interactive Menu Options

#### 5.4.1 Update NuGet Packages
- For each selected project:
  - List outdated packages
  - Update each package to the latest version
  - Display update output
  - Handle update errors

#### 5.4.2 Restore Dependencies
- For each selected project:
  - Execute `dotnet restore` with appropriate options
  - Display restore output
  - Handle restore errors

#### 5.4.3 Quick Build
- For each selected project:
  - Execute `dotnet build` with appropriate options
  - Display build output
  - Handle build errors

#### 5.4.4 Cleanup
- For each selected project:
  - Remove bin and obj directories
  - Clean temporary files
  - Display cleanup output
  - Handle cleanup errors

#### 5.4.5 Rebuild
- For each selected project:
  - Clean the project
  - Rebuild the project for each supported runtime
  - Display rebuild output
  - Handle rebuild errors

#### 5.4.6 Archive
- For each selected project:
  - Archive the built binaries
  - Archive the source code
  - Display archive output
  - Handle archive errors

## 6. Technical Requirements

### 6.1 Development Environment
- .NET 9.0 SDK
- C# 12.0 or later
- Visual Studio 2022 or later / Visual Studio Code with C# extensions

### 6.2 Runtime Requirements
- .NET 9.0 Runtime
- Windows, macOS, or Linux operating system

### 6.3 Dependencies
- System.CommandLine (for advanced command-line parsing)
- System.Text.Json (for JSON configuration)
- Serilog (for structured logging)

### 6.4 Performance Considerations
- Efficient handling of large solutions with many projects
- Minimal memory footprint
- Responsive user interface even during long-running operations

## 7. Implementation Details

### 7.1 Configuration System

The configuration system will use appsettings.json:

```csharp
// Example configuration loading
public static async Task<Settings> LoadAsync()
{
    var settings = new Settings();

    // Load settings from appsettings.json
    if (File.Exists("appsettings.json"))
    {
        var configJson = await File.ReadAllTextAsync("appsettings.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        settings = JsonSerializer.Deserialize<Settings>(configJson, options) ?? new Settings();
    }

    return settings;
}
```

### 7.2 Logging System

The logging system will use Serilog for structured logging:

```csharp
// Example Serilog configuration
public static void ConfigureLogging()
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            "logs/prjbuild-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            formatter: new CompactJsonFormatter())
        .CreateLogger();
}
```

### 7.3 Project Discovery

Project discovery will search for projects in subdirectories:

```csharp
public static List<ProjectInfo> DiscoverProjects(SolutionInfo solution)
{
    var projects = new List<ProjectInfo>();
    var projectFiles = Directory.GetFiles(solution.DirectoryPath, "*.csproj", SearchOption.AllDirectories);

    foreach (var projectFile in projectFiles)
    {
        var projectDir = Path.GetDirectoryName(projectFile);
        var projectName = Path.GetFileNameWithoutExtension(projectFile);

        // Skip projects in ignored directories
        if (IsIgnoredDirectory(projectDir))
            continue;

        // Skip obsolete projects
        if (IsObsoleteProject(projectName))
            continue;

        var project = new ProjectInfo(solution, projectName, projectDir, projectFile);
        projects.Add(project);
    }

    return projects;
}
```

### 7.4 Version Extraction

Version extraction will support multiple sources and parse into components:

```csharp
public VersionInfo ExtractVersion(string projectFilePath)
{
    string? csprojVersion = null;
    string? assemblyVersion = null;
    string? assemblyFileVersion = null;
    string? manifestVersion = null;

    // Extract from csproj
    var doc = XDocument.Load(projectFilePath);
    var propertyGroup = doc.Root?.Element("PropertyGroup");
    if (propertyGroup != null)
    {
        var versionElement = propertyGroup.Element("Version");
        if (versionElement != null)
            csprojVersion = versionElement.Value;
    }

    // Extract from AssemblyInfo.cs if exists
    var projectDir = Path.GetDirectoryName(projectFilePath);
    var assemblyInfoPath = Path.Combine(projectDir, "Properties", "AssemblyInfo.cs");
    if (File.Exists(assemblyInfoPath))
    {
        var content = File.ReadAllText(assemblyInfoPath);

        var versionMatch = Regex.Match(content, @"\[assembly:\s*AssemblyVersion\s*\(""(?<version>\d+\.\d+(\.\d+){0,2})""\)\]");
        if (versionMatch.Success)
            assemblyVersion = versionMatch.Groups["version"].Value;

        var fileVersionMatch = Regex.Match(content, @"\[assembly:\s*AssemblyFileVersion\s*\(""(?<version>\d+\.\d+(\.\d+){0,2})""\)\]");
        if (fileVersionMatch.Success)
            assemblyFileVersion = fileVersionMatch.Groups["version"].Value;
    }

    // Extract from app.manifest if exists
    var manifestPath = Path.Combine(projectDir, "app.manifest");
    if (File.Exists(manifestPath))
    {
        var content = File.ReadAllText(manifestPath);
        var match = Regex.Match(content, @"version=""\d+\.\d+\.\d+\.\d+""");
        if (match.Success)
            manifestVersion = match.Value.Substring(9, match.Value.Length - 10);
    }

    return new VersionInfo(csprojVersion, assemblyVersion, assemblyFileVersion, manifestVersion);
}
```

### 7.5 Dependency Resolution

Dependency resolution will use a linked list structure for better traversal:

```csharp
public LinkedList<ProjectInfo> ResolveProjectDependencies(List<ProjectInfo> allProjects)
{
    var sortedProjects = new LinkedList<ProjectInfo>();
    var visited = new HashSet<ProjectInfo>();

    foreach (var project in allProjects)
    {
        if (!visited.Contains(project))
        {
            VisitProject(project, allProjects, sortedProjects, visited, new HashSet<ProjectInfo>());
        }
    }

    return sortedProjects;
}

private void VisitProject(
    ProjectInfo project,
    List<ProjectInfo> allProjects,
    LinkedList<ProjectInfo> sortedProjects,
    HashSet<ProjectInfo> visited,
    HashSet<ProjectInfo> visiting)
{
    visiting.Add(project);

    foreach (var reference in project.ReferencedProjects)
    {
        if (visiting.Contains(reference))
        {
            // Circular reference detected
            throw new InvalidOperationException($"Circular reference detected: {project.Name} -> {reference.Name}");
        }

        if (!visited.Contains(reference))
        {
            VisitProject(reference, allProjects, sortedProjects, visited, visiting);
        }
    }

    visiting.Remove(project);
    visited.Add(project);
    sortedProjects.AddLast(project);
}
```

### 7.6 Build Operations

Build and restore operations will be separate:

```csharp
public List<string> Build()
{
    var arguments = new List<string> { "build", FilePath, "--configuration", "Release" };

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

public List<string> Restore()
{
    var arguments = new List<string> { "restore", FilePath };

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

### 7.7 Archive Operations

Archive operations will create ZIP files:

```csharp
public List<string> Archive(List<string> notArchivedDirectoryNames, List<string> notArchivedFileNames)
{
    var messages = new List<string>();
    var archiveDirectory = Path.GetDirectoryName(SourceArchivePath);

    Directory.CreateDirectory(archiveDirectory);

    using (var archive = ZipFile.Open(SourceArchivePath, ZipArchiveMode.Create))
    {
        var fileCount = AddDirectoryToArchive(archive, DirectoryPath, "", notArchivedDirectoryNames, notArchivedFileNames);

        if (fileCount > 0)
            messages.Add($"Archive file created: {SourceArchivePath}");
        else
        {
            File.Delete(SourceArchivePath);
            messages.Add($"Empty archive file deleted: {SourceArchivePath}");
        }
    }

    return messages;
}
```

## 8. Error Handling and Validation

### 8.1 Input Validation

The application will validate all inputs to ensure they meet the required format and constraints:

- Configuration settings will be validated during loading
- File paths will be checked for existence and accessibility
- Project references will be validated for correctness
- User input will be validated before processing

### 8.2 Error Reporting

Errors will be reported in a clear and consistent manner:

- Console output will use color coding to highlight errors
- Log files will contain detailed error information
- Error messages will include context and suggestions where possible
- Stack traces will be included for unexpected exceptions in debug mode

### 8.3 Recovery Strategies

The application will implement strategies to recover from common error conditions:

- Automatic retry for transient failures (e.g., file access issues)
- Graceful degradation when non-critical components fail
- Safe rollback of partial operations
- Preservation of state to allow resuming after failure

## 9. Future Enhancements

### 9.1 Planned Features

The following features are planned for future versions:

- Integration with CI/CD pipelines
- Support for custom build steps and scripts
- Parallel building of independent projects
- Incremental builds based on file changes
- Integration with source control systems
- Web-based UI for remote management

### 9.2 Extensibility Points

The application is designed with the following extensibility points:

- Plugin system for custom operations
- Extensible configuration format
- Custom build step hooks
- Templating system for archive naming and organization
- API for integration with other tools

## 10. Conclusion

prjBuild v0.1 provides a solid foundation for automating the build and archive process for .NET projects. By implementing the specifications outlined in this document, the application will offer a significant improvement in developer productivity and build consistency.