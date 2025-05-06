# prjBuild

A .NET-based command-line application designed to automate the building and archiving of .NET projects across multiple repositories.

## Overview

prjBuild streamlines the development workflow by automatically identifying changed projects, managing dependencies, and providing an interactive interface for building, updating, and archiving .NET projects.

## Key Features

- **Automatic Project Discovery**: Identifies solutions and projects across multiple directories
- **Dependency Management**: Resolves project references and builds dependency graphs
- **Version Management**: Extracts and validates version information from various sources
- **NuGet Package Updates**: Provides options to update NuGet packages across projects
- **Build Automation**: Builds projects with appropriate options for different target runtimes
- **Standardized Archiving**: Creates archives for both binaries and source code
- **Interactive Interface**: Offers a command-line UI with interactive menu options

## System Requirements

- .NET 9.0 Runtime
- Windows, macOS, or Linux operating system

## Core Components

### Configuration Management
Loads settings from appsettings.json and provides access to configuration values.

### Logging System
Provides comprehensive logging to both console and file with different log levels.

### File System Operations
Handles file and directory operations, including archiving and path manipulation.

### Solution/Project Management
Discovers and analyzes .NET solutions and projects, extracts version information, and builds dependency graphs.

### Build and Archive Service
Executes build operations, updates NuGet packages, and creates standardized archives.

### Command-line UI
Presents interactive menus, displays project information, and reports progress and errors.

## Usage

prjBuild provides an interactive menu-driven interface with the following main operations:

1. **Project Selection**: Select projects to operate on based on various criteria
2. **Update NuGet Packages**: Update NuGet packages for selected projects
3. **Build Projects**: Build selected projects with appropriate options
4. **Rebuild and Archive**: Rebuild selected projects and create standardized archives

The application automatically identifies which projects have changed and need to be rebuilt, making it easy to maintain large codebases with multiple interdependent projects.