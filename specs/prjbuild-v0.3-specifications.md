# prjBuild v0.3 Specifications

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
4. [Data Models](#4-data-models)
   1. [Configuration Model](#41-configuration-model)
   2. [Inherited Properties Base Model](#42-inherited-properties-base-model)
   3. [Solution Model](#43-solution-model)
   4. [Project Model](#44-project-model)
   5. [Version Management Model](#45-version-management-model)
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
7. [Error Handling and Validation](#7-error-handling-and-validation)
   1. [Input Validation](#71-input-validation)
   2. [Error Reporting](#72-error-reporting)
   3. [Recovery Strategies](#73-recovery-strategies)

## 1. Introduction

### 1.1 Overview

prjBuild is a .NET-based command-line application designed to automate the building and archiving of .NET projects across multiple repositories. It identifies changed projects, manages their dependencies, and provides an interactive interface for building, updating, and archiving these projects.

### 1.2 Purpose and Goals

The primary purpose of prjBuild is to streamline the development workflow by:

- Automatically identifying which projects have changed and need to be rebuilt
- Managing project dependencies to ensure correct build order
- Providing a consistent way to update NuGet packages across projects
- Creating standardized archives of both binaries and source code
- Supporting multiple target runtimes (e.g., win-x64, osx-arm64)
- Providing a clear, interactive user interface for project operations

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
   - Manages configuration settings
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
   - Searches for configuration files in parent directories

2. **Logging System**
   - Records application activities and errors
   - Outputs to both console and file
   - Provides different log levels and formatting
   - Implements fallback logging for initialization failures

3. **File System Operations**
   - Handles file and directory operations
   - Manages file archiving and compression
   - Provides path manipulation utilities
   - Implements ignore patterns for files and directories

4. **Solution/Project Management**
   - Discovers and analyzes .NET solutions and projects
   - Extracts version information
   - Builds dependency graphs
   - Tracks archive status

5. **Build and Archive Service**
   - Executes build operations
   - Updates NuGet packages
   - Creates standardized archives
   - Validates projects before operations

6. **Command-line UI**
   - Presents interactive menus
   - Displays project information
   - Reports progress and errors
   - Provides both simple and detailed views

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
- Search for configuration file in parent directories if not found in current directory
- Bind configuration to strongly-typed Settings model

**Implementation Details**:
- Uses Microsoft.Extensions.Configuration for JSON configuration handling
- Searches for appsettings.json in the application directory and parent directories
- Provides fallback error handling for configuration loading failures
- Exposes both raw IConfiguration and typed Settings objects

### 3.2 Logging System

**Purpose**: Provide comprehensive logging capabilities.

**Key Features**:
- Log to both console and file simultaneously
- Support different log levels (Info, Warning, Error, Debug)
- Color-coded console output for better readability
- Structured logging using Serilog
- Fallback logging mechanism for initialization failures

**Implementation Details**:
- Uses Serilog for structured logging
- Creates timestamped log files in a logs directory
- Configures different minimum levels for console (Information) and file (Verbose)
- Implements color-coded console output for different log levels
- Provides fallback console logging if initialization fails

### 3.3 File System Operations

**Purpose**: Handle file and directory operations safely and efficiently.

**Key Features**:
- File reading and writing with encoding detection
- Directory creation and enumeration
- File archiving (ZIP)
- Path manipulation utilities
- Ignore patterns for files and directories
- Archive path generation for solutions and projects

**Implementation Details**:
- Implements robust error handling for all file operations
- Creates ZIP archives with consistent directory structures
- Generates file lists for archives
- Handles UTF-8 encoding with BOM for proper CJK character support
- Provides methods to check if archives exist
- Implements configurable ignore patterns at global, solution, and project levels

### 3.4 Solution/Project Management

**Purpose**: Discover, analyze, and manage .NET solutions and projects.

**Key Features**:
- Solution and project discovery across multiple directories
- Project reference resolution
- Version extraction and validation
- Dependency graph construction
- Archive status tracking

**Implementation Details**:
- Recursively discovers solutions and projects in configured root directories
- Extracts version information from multiple sources (csproj, AssemblyInfo.cs, app.manifest)
- Initializes inherited properties from configuration
- Validates version consistency within projects and across solutions
- Tracks archive status for solutions and projects

### 3.5 Build and Archive Service

**Purpose**: Build projects and create standardized archives.

**Key Features**:
- Project building with appropriate options
- NuGet package updates
- Binary and source code archiving
- Support for multiple target runtimes
- Project validation before build operations

**Implementation Details**:
- Executes dotnet CLI commands for build operations
- Validates projects before building or archiving
- Cleans projects before rebuilding
- Creates runtime-specific archives for each supported runtime
- Archives source code at the solution level
- Captures and logs command output

### 3.6 Command-line UI

**Purpose**: Provide an interactive interface for the user.

**Key Features**:
- Display project information in a readable format
- Interactive menu for common operations
- Progress reporting for long-running operations
- Error handling and reporting
- Toggle between simple and detailed views
- Auto-selection of projects needing archiving
- Project dependency-based sorting for operations

**Implementation Details**:
- Implements a main menu for project selection
- Provides an operations menu for selected projects
- Sorts projects by dependencies for correct operation order
- Displays color-coded status information
- Supports filtering to show/hide archived or excluded projects
- Auto-selects projects that need archiving on startup

## 4. Data Models

### 4.1 Configuration Model

```csharp
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
    public List<string> SupportedRuntimes { get; set; } = new();

    // Project-specific ignore patterns
    public List<string> IgnoredObjectNames { get; set; } = new();
    public List<string> IgnoredObjectRelativePaths { get; set; } = new();

    // Archiving configuration
    public bool? ExcludeFromArchiving { get; set; } = null; // Default to null (will be archived)
}
```

### 4.2 Inherited Properties Base Model

```csharp
public abstract class InheritedPropertiesBase
{
    /// <summary>
    /// List of object names that should be ignored during operations
    /// </summary>
    public List<string> IgnoredObjectNames { get; } = new();

    /// <summary>
    /// List of relative paths that should be ignored during operations
    /// </summary>
    public List<string> IgnoredObjectRelativePaths { get; } = new();
}
```

### 4.3 Solution Model

```csharp
public class SolutionInfo : InheritedPropertiesBase
{
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
    /// Indicates if all archives of the current version already exist (source code and all project binaries)
    /// </summary>
    public bool AreAllArchivesExisting { get; set; }

    // Method to initialize inherited properties
    public void InitializeInheritedProperties(Settings? globalSettings, SolutionConfig? solutionConfig);

    // Method to validate versions across all projects in the solution
    public bool ValidateVersions();
}
```

### 4.4 Project Model

```csharp
public class ProjectInfo : InheritedPropertiesBase
{
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
    /// When set to true, indicates this project should be excluded from archiving.
    /// If null or false, the project will be archived.
    /// </summary>
    public bool? ExcludeFromArchiving { get; set; }

    // Method to initialize inherited properties
    public void InitializeInheritedProperties(Settings? globalSettings, SolutionConfig? solutionConfig, ProjectConfig? projectConfig);

    // Method to validate versions within the project
    public bool ValidateVersions();
}
```

### 4.5 Version Management Model

```csharp
public enum VersionSourceType
{
    /// <summary>
    /// Version from .csproj project file.
    /// Typically appears in XML format like:
    /// <Version>1.2.3.4</Version> or
    /// <PackageVersion>1.2.3.4</PackageVersion> or
    /// <AssemblyVersion>1.2.3.4</AssemblyVersion>
    /// Usually found within a PropertyGroup element.
    /// </summary>
    CsprojFile,

    /// <summary>
    /// Version from AssemblyInfo.cs file.
    /// Appears as C# attributes like:
    /// [assembly: AssemblyVersion("1.2.3.4")] or
    /// [assembly: AssemblyFileVersion("1.2.3.4")]
    /// Usually found in Properties/AssemblyInfo.cs.
    /// May include multiple version attributes (AssemblyVersion, AssemblyFileVersion, etc.)
    /// </summary>
    AssemblyInfo,

    /// <summary>
    /// Version from app.manifest file.
    /// Appears in XML format like:
    /// <assemblyIdentity version="1.2.3.4" name="MyApplication.app"/>
    /// Usually found within the application manifest file.
    /// Used for ClickOnce deployments and Windows application manifests.
    /// </summary>
    Manifest
}

public class VersionSource
{
    public VersionSourceType SourceType { get; }
    public string FilePath { get; }
    public string VersionString { get; }
    public Version? ParsedVersion { get; }
}

public class VersionManager
{
    public List<VersionSource> VersionSources { get; } = new();

    // Methods
    public bool ValidateProjectVersions();
    public static bool ValidateSolutionAndProjectVersions(SolutionInfo solution);
    public VersionSource? GetPrimaryVersionSource();
    public static string FormatVersion(Version version);
}

public static class VersionExtensions
{
    public static bool AreVersionsEqual(this Version? version1, Version? version2);
}
```

## 5. Key Features and Workflows

### 5.1 Project Discovery Workflow

1. Load configuration settings from appsettings.json
2. For each root directory in the configuration:
   - Enumerate directories in the root directory
   - Filter out ignored file system objects (files and directories) based on names and relative paths
   - Find solution files in each directory
   - Create SolutionInfo objects for valid solutions
   - Associate solutions with their parent root directory
   - Initialize archive directory path for each solution
   - For each solution:
     - Find project directories (including subdirectories)
     - Filter out ignored file system objects (files and directories)
     - Find project files in each directory
     - Create ProjectInfo objects for valid projects
     - Associate projects with their solutions
     - Extract version information from various sources
     - Initialize runtime archive paths for each project
     - Check if the solution is already archived

### 5.2 Version and Reference Resolution

1. For each project:
   - Extract version information from various sources (project file, AssemblyInfo.cs, app.manifest)
   - Create VersionSource objects for each source
   - Add these to the project's VersionManager
   - Extract project references from project file
   - Resolve referenced projects
   - Build dependency graph using project references
   - Validate that all version sources within a project have the same version
   - Validate that all projects in a solution have the same primary version

### 5.3 Project Selection and Operations

1. Discover all projects across configured root directories
2. Filter out archived solutions and projects unless explicitly shown
3. Auto-select projects that need archiving on first run
4. Present list of all visible projects with selection status
5. Provide options to:
   - Select/deselect individual projects
   - Select/deselect all projects
   - Select projects by pattern or criteria
   - Toggle display of archived or excluded projects
6. For selected projects, present operations menu:
   - Update NuGet packages
   - Build projects
   - Rebuild and archive projects
   - Exit

### 5.4 Interactive Menu Options

#### 5.4.1 Update NuGet Packages
- Sort projects by dependencies to ensure correct update order
- For each selected project:
  - Restore dependencies
  - List outdated packages
  - Update each package to the latest version
  - Display update output
  - Handle update errors

#### 5.4.2 Build Projects
- Sort projects by dependencies to ensure correct build order
- For each selected project:
  - Validate the project before building
  - Execute `dotnet build` with appropriate options
  - Display build output
  - Handle build errors

#### 5.4.3 Rebuild and Archive Projects
- Sort projects by dependencies to ensure correct build order
- For each selected project:
  - Clean the project
  - Retrieve supported runtimes from project configuration
  - Rebuild the project for each supported runtime
  - Archive the built binaries for each supported runtime
  - Archive the source code at the solution level
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
- Microsoft.Extensions.Configuration (for configuration management)

### 6.4 Performance Considerations
- Efficient handling of large solutions with many projects
- Minimal memory footprint
- Responsive user interface even during long-running operations
- Efficient file archiving with appropriate compression levels

## 7. Error Handling and Validation

### 7.1 Input Validation

The application validates all inputs to ensure they meet the required format and constraints:

- Configuration settings are validated during loading
- File paths are checked for existence and accessibility
- Project references are validated for correctness
- User input is validated before processing
- Version information is validated for consistency

### 7.2 Error Reporting

Errors are reported in a clear and consistent manner:

- Console output uses color coding to highlight errors
- Log files contain detailed error information
- Error messages include context and suggestions where possible
- Stack traces are included for unexpected exceptions in debug mode

### 7.3 Recovery Strategies

The application implements strategies to recover from common error conditions:

- Automatic retry for transient failures (e.g., file access issues)
- Graceful degradation when non-critical components fail
- Safe rollback of partial operations
- Preservation of state to allow resuming after failure
- Fallback mechanisms for critical services like logging

End of specification.