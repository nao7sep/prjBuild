# prjBuild

A .NET-based command-line application designed to automate the building and archiving of .NET projects across multiple repositories.

## Overview

prjBuild streamlines the development workflow by automatically identifying changed projects, managing dependencies, and providing an interactive interface for building, updating, and archiving .NET projects. It's designed to handle complex project structures with multiple solutions and projects across different repositories.

## Key Features

- **Automatic Project Discovery**: Scans configured directories to find .NET solutions and projects
- **Dependency Management**: Resolves project references and ensures correct build order
- **Version Management**: Extracts and validates version information from various sources
- **NuGet Package Updates**: Provides a consistent way to update NuGet packages across projects
- **Multi-Runtime Support**: Builds and archives projects for multiple target runtimes
- **Standardized Archiving**: Creates archives of both binaries and source code with consistent naming
- **Interactive CLI**: Offers an intuitive command-line interface with interactive menus

## System Requirements

- **.NET Runtime**: .NET 9.0 or later
- **Operating Systems**: Windows, macOS, or Linux
- **Dependencies**:
  - System.CommandLine (for advanced command-line parsing)
  - System.Text.Json (for JSON configuration)
  - Serilog (for structured logging)

## Installation

1. Ensure you have the .NET 9.0 SDK installed on your system
2. Clone this repository or download the latest release
3. Build the application using `dotnet build`
4. Run the application using `dotnet run` or the compiled executable

## Usage

### Basic Usage

```bash
# Run with default settings
dotnet run

# Specify a custom configuration file
dotnet run --config path/to/appsettings.json
```

### Interactive Menu

The application provides an interactive menu with the following options:

1. **Update NuGet Packages**: Updates all NuGet packages for selected projects
2. **Build Projects**: Builds selected projects
3. **Rebuild and Archive Projects**: Rebuilds and creates archives for selected projects
4. **Exit**: Exits the application

### Project Selection

- Projects that need archiving are auto-selected on first run
- You can manually select/deselect individual projects
- Options to select/deselect all projects or by pattern
- Toggle display of archived or excluded projects

## Configuration

prjBuild uses an `appsettings.json` file for configuration. The application will search for this file in the current directory and parent directories.

### Sample Configuration

```json
{
  "RootDirectories": [
    {
      "DirectoryPath": "C:/Projects",
      "ArchiveDirectoryPath": "C:/Archives"
    }
  ],
  "Solutions": [
    {
      "Name": "MySolution",
      "IgnoredObjectNames": ["bin", "obj"],
      "IgnoredObjectRelativePaths": ["Tests"],
      "Projects": [
        {
          "Name": "MyProject",
          "SupportedRuntimes": ["win-x64", "linux-x64"],
          "ExcludeFromArchiving": false
        }
      ]
    }
  ],
  "IgnoredObjectNames": ["node_modules", ".git"],
  "IgnoredObjectRelativePaths": ["Temp"]
}
```

## Architecture

prjBuild follows a modular, layered architecture:

### Architectural Layers

1. **User Interface Layer**: Command-line interface with interactive menu options
2. **Application Logic Layer**: Project discovery, dependency resolution, and operation coordination
3. **Core Services Layer**: Configuration management, logging, file system operations

### Core Components

1. **Configuration Management**: Loads and manages application settings
2. **Logging System**: Records application activities and errors
3. **File System Operations**: Handles file and directory operations
4. **Solution/Project Management**: Discovers and analyzes .NET solutions and projects
5. **Build and Archive Service**: Executes build operations and creates archives
6. **Command-line UI**: Presents interactive menus and displays information

## Error Handling

- Input validation for all configuration settings and user inputs
- Color-coded console output for error highlighting
- Detailed error logging with context and suggestions
- Recovery strategies for common error conditions

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
