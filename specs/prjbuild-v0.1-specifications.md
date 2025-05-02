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
4. [Data Models](#4-data-models)
   1. [Settings Model](#41-settings-model)
   2. [Solution Model](#42-solution-model)
   3. [Project Model](#43-project-model)
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
8. [Conclusion](#8-conclusion)

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

### 3.2 Logging System

**Purpose**: Provide comprehensive logging capabilities.

**Key Features**:
- Log to both console and file simultaneously
- Support different log levels (Info, Warning, Error, Debug)
- Color-coded console output for better readability
- Structured logging using NDJSON format

### 3.3 File System Operations

**Purpose**: Handle file and directory operations safely and efficiently.

**Key Features**:
- File reading and writing with encoding detection
- Directory creation and enumeration
- File archiving (ZIP)
- Path manipulation utilities

### 3.4 Solution/Project Management

**Purpose**: Discover, analyze, and manage .NET solutions and projects.

**Key Features**:
- Solution and project discovery across multiple directories
- Project reference resolution
- Version extraction and validation
- Dependency graph construction

### 3.5 Build and Archive Service

**Purpose**: Build projects and create standardized archives.

**Key Features**:
- Project building with appropriate options
- NuGet package updates
- Binary and source code archiving
- Support for multiple target runtimes

### 3.6 Command-line UI

**Purpose**: Provide an interactive interface for the user.

**Key Features**:
- Display project information in a readable format
- Interactive menu for common operations
- Progress reporting for long-running operations
- Error handling and reporting

## 4. Data Models

### 4.1 Settings Model

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

### 4.2 Solution Model

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
    public List<string> Archive(List<string> notArchivedDirectoryNames, List<string> notArchivedFileNames, List<string> notArchivedFilePaths, List<string> notArchivedDirectoryPaths);
}
```

### 4.3 Project Model

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
    public List<string> Cleanup();
    public List<string> Rebuild(List<string> supportedRuntimes);
    public List<string> Archive(List<string> supportedRuntimes, List<string> notArchivedDirectoryNames, List<string> notArchivedFileNames, List<string> notArchivedFilePaths, List<string> notArchivedDirectoryPaths);
}

public class VersionInfo
{
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
   - Store version information in VersionInfo object
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
  - Archive the built binaries for each supported runtime
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

## 7. Error Handling and Validation

### 7.1 Input Validation

The application will validate all inputs to ensure they meet the required format and constraints:

- Configuration settings will be validated during loading
- File paths will be checked for existence and accessibility
- Project references will be validated for correctness
- User input will be validated before processing

### 7.2 Error Reporting

Errors will be reported in a clear and consistent manner:

- Console output will use color coding to highlight errors
- Log files will contain detailed error information
- Error messages will include context and suggestions where possible
- Stack traces will be included for unexpected exceptions in debug mode

### 7.3 Recovery Strategies

The application will implement strategies to recover from common error conditions:

- Automatic retry for transient failures (e.g., file access issues)
- Graceful degradation when non-critical components fail
- Safe rollback of partial operations
- Preservation of state to allow resuming after failure

## 8. Conclusion

prjBuild v0.1 provides a solid foundation for automating the build and archive process for .NET projects. By implementing the specifications outlined in this document, the application will offer a significant improvement in developer productivity and build consistency.