# prjBuild

A .NET-based command-line application designed to automate the building and archiving of .NET projects across multiple repositories.

## Overview

prjBuild streamlines the development workflow by identifying changed projects, managing dependencies, and providing an interactive interface for building, updating, and archiving .NET projects. It's designed to work across multiple repositories and supports various target runtimes.

## Key Features

- **Automatic Project Discovery** - Identifies solutions and projects across configured root directories
- **Version Management** - Extracts and validates versions across projects and solutions
- **Dependency Resolution** - Manages project dependencies to ensure correct build order
- **NuGet Package Management** - Provides a consistent way to update packages across projects
- **Standardized Archiving** - Creates archives of both binaries and source code with consistent naming
- **Multi-Runtime Support** - Builds and packages for multiple target runtimes
- **Interactive Interface** - Provides an intuitive command-line interface for project operations

## System Architecture

prjBuild follows a modular, layered architecture consisting of:

1. **User Interface Layer** - Command-line interface with interactive menus
2. **Application Logic Layer** - Project discovery, dependency resolution, and operations
3. **Core Services Layer** - Configuration, logging, and file system operations

## Core Components

- **Configuration Management** - Handles application settings with platform-specific support
- **Logging System** - Provides comprehensive logging to both console and files
- **File System Operations** - Manages file operations, archiving, and path manipulation
- **Solution/Project Management** - Discovers and analyzes .NET solutions and projects
- **Build and Archive Service** - Handles building, updating, and archiving operations
- **Command-line UI** - Presents interactive menus and displays project information

## Technical Requirements

- **.NET 9.0** SDK and Runtime
- **Windows, macOS, or Linux** operating system

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE)

## Author

Created by [nao7sep](https://github.com/nao7sep)

---

For detailed specifications, see the [specifications documentation](specs/prjbuild-v0.2-specifications.md).