# prjBuild

A .NET command-line application for automating the build and archive processes of .NET projects across multiple repositories.

## Overview

prjBuild streamlines development workflows by automating the discovery, building, and archiving of .NET projects. It identifies which projects have changed, manages dependencies to ensure correct build order, and provides a consistent interface for updating NuGet packages and creating standardized archives.

## Features

- **Automated Project Discovery** - Scans configured root directories to find .NET solutions and projects
- **Version Management** - Extracts and validates version information from multiple sources
- **Dependency Resolution** - Builds dependency graphs to ensure correct build order
- **NuGet Package Updates** - Provides tools to update packages across multiple projects
- **Multi-Runtime Support** - Builds and archives for multiple target runtimes
- **Standardized Archiving** - Creates consistent archives for both binaries and source code
- **Interactive CLI** - Offers an intuitive command-line interface with interactive menus

## System Requirements

- .NET 9.0 Runtime
- Windows, macOS, or Linux operating system

## Configuration

prjBuild uses an `appsettings.json` file for configuration. Key configuration elements include:

- **Root Directories** - Base directories to scan for solutions and projects
- **Archive Directories** - Where to store archived binaries and source code
- **Ignore Patterns** - Files and directories to exclude from operations
- **Solution and Project Settings** - Specific configurations for individual solutions and projects
- **Runtime Support** - Target runtimes for building and archiving

Configuration can be hierarchical, with settings inherited from global to solution to project level.

## Usage

### Project Discovery

prjBuild automatically discovers projects by:
1. Loading configuration from `appsettings.json`
2. Scanning configured root directories for solutions and projects
3. Extracting version information and building dependency graphs
4. Determining which projects need to be archived

### Project Operations

The interactive menu provides options to:
- Select/deselect projects for operations
- Update NuGet packages for selected projects
- Build selected projects
- Rebuild and archive selected projects

### Archiving

When archiving projects, prjBuild:
1. Cleans and rebuilds the project for each supported runtime
2. Archives the built binaries for each runtime
3. Creates a source code archive at the solution level

## Architecture

prjBuild follows a modular, layered architecture:

- **User Interface Layer** - Command-line interface with interactive menus
- **Application Logic Layer** - Project discovery, dependency resolution, and operation coordination
- **Core Services Layer** - Configuration, logging, file system operations, and tool interfaces

Key components include:
- Configuration Management
- Logging System
- File System Operations
- Solution/Project Management
- Build and Archive Service
- Command-line UI

## Error Handling

prjBuild implements robust error handling:

- Input validation for all configuration settings and user inputs
- Clear error reporting with context and suggestions
- Recovery strategies for common error conditions
- Automatic retry for transient failures
- Graceful degradation when non-critical components fail

## Development

### Requirements

- .NET 9.0 SDK
- C# 12.0 or later
- Visual Studio 2022 or later / Visual Studio Code with C# extensions

### Dependencies

- System.CommandLine - Advanced command-line parsing
- System.Text.Json - JSON configuration handling
- Serilog - Structured logging
