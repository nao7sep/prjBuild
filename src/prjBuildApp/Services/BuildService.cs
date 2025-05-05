using prjBuildApp.Models.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace prjBuildApp.Services
{
    public class BuildService
    {
        private readonly LoggingService _loggingService;
        private readonly FileSystemService _fileSystemService;

        public BuildService(LoggingService loggingService, FileSystemService fileSystemService)
        {
            _loggingService = loggingService;
            _fileSystemService = fileSystemService;
        }

        public List<string> UpdateNuGetPackages(ProjectInfo project)
        {
            var output = new List<string>();

            try
            {
                // First, restore the project to ensure dependencies are properly resolved
                _loggingService.Information("Restoring project {ProjectName} before updating packages", project.Name);
                var restoreArguments = new List<string> { "restore", $"\"{project.FilePath}\"" };
                var restoreResult = RunDotNetCommand(restoreArguments, project.DirectoryPath);
                output.AddRange(restoreResult);

                // Then, list outdated packages
                var listArguments = new List<string> { "list", $"\"{project.FilePath}\"", "package", "--outdated" };
                var listResult = RunDotNetCommand(listArguments, project.DirectoryPath);
                output.AddRange(listResult);

                // Extract package names from the output
                var packageLines = listResult.Where(line => line.Contains('>')).ToList();
                if (packageLines.Count == 0)
                {
                    _loggingService.Information("No outdated packages found for project {ProjectName}", project.Name);
                    return output;
                }

                // Update each package
                foreach (var packageLine in packageLines)
                {
                    var parts = packageLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        string packageName = parts[1];
                        var updateArguments = new List<string> { "add", $"\"{project.FilePath}\"", "package", packageName };
                        var updateResult = RunDotNetCommand(updateArguments, project.DirectoryPath);
                        output.AddRange(updateResult);

                        _loggingService.Information("Updated package {PackageName} for project {ProjectName}", packageName, project.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error updating NuGet packages for project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        /// <summary>
        /// Checks if a project can be built based on its validation status
        /// </summary>
        /// <param name="project">The project to check</param>
        /// <param name="runtime">The runtime to build for, if specified</param>
        /// <returns>A tuple containing a boolean indicating if the project can be built and a list of validation messages</returns>
        public (bool CanBuild, List<string> ValidationMessages) ValidateProjectForBuild(ProjectInfo project, string? runtime = null)
        {
            var messages = new List<string>();
            bool canBuild = true;

            // Check if versions match within the project
            if (!project.ValidateVersions())
            {
                messages.Add($"Project {project.Name} has mismatched versions");
                canBuild = false;
            }

            // Check if the project has supported runtimes
            if (project.SupportedRuntimes.Count == 0)
            {
                messages.Add($"Project {project.Name} has no supported runtimes defined");
                canBuild = false;
            }

            // If a specific runtime is provided, check if it's supported
            if (!string.IsNullOrEmpty(runtime) && !project.SupportedRuntimes.Contains(runtime))
            {
                messages.Add($"Runtime {runtime} is not supported by project {project.Name}");
                canBuild = false;
            }

            return (canBuild, messages);
        }

        public List<string> BuildProject(ProjectInfo project)
        {
            var output = new List<string>();

            // Validate the project before building
            var (canBuild, validationMessages) = ValidateProjectForBuild(project);

            // Add validation messages to output
            output.AddRange(validationMessages);

            // If the project can't be built, return early with validation messages
            if (!canBuild)
            {
                _loggingService.Warning("Cannot build project {ProjectName} due to validation failures", project.Name);
                return output;
            }

            try
            {
                var arguments = new List<string> { "build", $"\"{project.FilePath}\"", "--configuration", "Release" };

                var result = RunDotNetCommand(arguments, project.DirectoryPath);
                output.AddRange(result);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error building project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        public List<string> CleanupProject(ProjectInfo project, bool deleteObjDirectory = true)
        {
            var output = new List<string>();

            try
            {
                _loggingService.Information("Cleaning up project {ProjectName}", project.Name);

                // Clean the project using dotnet clean
                var cleanArguments = new List<string> { "clean", $"\"{project.FilePath}\"", "--configuration", "Release" };
                var cleanResult = RunDotNetCommand(cleanArguments, project.DirectoryPath);
                output.AddRange(cleanResult);

                // Delete obj directory if specified
                if (deleteObjDirectory)
                {
                    string objDir = Path.Combine(project.DirectoryPath, "obj");
                    if (Directory.Exists(objDir))
                    {
                        Directory.Delete(objDir, true);
                        output.Add($"Deleted directory: {objDir}");
                        _loggingService.Information("Deleted obj directory for project {ProjectName}", project.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error cleaning up project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        public List<string> PublishProject(ProjectInfo project, string runtime)
        {
            var output = new List<string>();

            // Validate the project before publishing
            var (canBuild, validationMessages) = ValidateProjectForBuild(project, runtime);

            // Add validation messages to output
            output.AddRange(validationMessages);

            // If the project can't be published, return early with validation messages
            if (!canBuild)
            {
                _loggingService.Warning("Cannot publish project {ProjectName} due to validation failures", project.Name);
                return output;
            }

            try
            {
                _loggingService.Information("Publishing project {ProjectName}", project.Name);

                // Construct output directory: project root + bin + Publish + runtime
                string outputDirectory = Path.Combine(project.DirectoryPath, "bin", "Publish", runtime);

                // Delete the output directory if it already exists
                if (Directory.Exists(outputDirectory))
                {
                    Directory.Delete(outputDirectory, true);
                    output.Add($"Deleted existing directory: {outputDirectory}");
                }

                // Create output directory
                Directory.CreateDirectory(outputDirectory);
                output.Add($"Created directory: {outputDirectory}");

                var arguments = new List<string> {
                    "publish",
                    $"\"{project.FilePath}\"",
                    "--configuration", "Release",
                    "--output", $"\"{outputDirectory}\"",
                    "--runtime", runtime
                };

                var result = RunDotNetCommand(arguments, project.DirectoryPath);
                output.AddRange(result);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error publishing project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        public List<string> ArchiveProject(ProjectInfo project, string runtime)
        {
            var output = new List<string>();

            // Validate the project before archiving
            var (canBuild, validationMessages) = ValidateProjectForBuild(project, runtime);

            // Add validation messages to output
            output.AddRange(validationMessages);

            // If the project can't be archived, return early with validation messages
            if (!canBuild)
            {
                _loggingService.Warning("Cannot archive project {ProjectName} due to validation failures", project.Name);
                return output;
            }

            try
            {
                _loggingService.Information("Archiving project {ProjectName} for runtime {Runtime}", project.Name, runtime);

                // Create binary archive - assuming the project has already been published
                string binDir = Path.Combine(project.DirectoryPath, "bin", "Publish", runtime);
                if (Directory.Exists(binDir))
                {
                    // Get the archive path for this runtime
                    var archivePaths = _fileSystemService.GetProjectRuntimeArchivePaths(project);

                    if (!archivePaths.ContainsKey(runtime))
                    {
                        output.Add($"No archive path defined for runtime {runtime}");
                        _loggingService.Warning("No archive path defined for runtime {Runtime} in project {ProjectName}",
                            runtime, project.Name);
                        return output;
                    }

                    string archiveFilePath = archivePaths[runtime];
                    string? archiveDirectory = Path.GetDirectoryName(archiveFilePath);

                    // Create archive directory if it doesn't exist
                    if (!string.IsNullOrEmpty(archiveDirectory) && !Directory.Exists(archiveDirectory))
                    {
                        Directory.CreateDirectory(archiveDirectory);
                    }

                    string archiveFileName = Path.GetFileName(archiveFilePath);

                    if (_fileSystemService.CreateZipArchive(binDir, archiveFilePath, project.IgnoredObjectNames, project.IgnoredObjectRelativePaths))
                    {
                        output.Add($"Created binary archive: {archiveFilePath}");
                        _loggingService.Information("Created binary archive for project {ProjectName} and runtime {Runtime}: {ArchiveFileName}",
                            project.Name, runtime, archiveFileName);
                    }
                    else
                    {
                        output.Add($"Failed to create binary archive: {archiveFilePath}");
                        _loggingService.Warning("Failed to create binary archive for project {ProjectName} and runtime {Runtime}",
                            project.Name, runtime);
                    }
                }
                else
                {
                    output.Add($"Binary directory not found: {binDir}");
                    _loggingService.Warning("Binary directory not found for project {ProjectName} and runtime {Runtime}: {BinDir}",
                        project.Name, runtime, binDir);
                }

                // Archive source code (at solution level)
                var solution = project.Solution;
                string sourceArchiveFilePath = _fileSystemService.GetSolutionSourceArchivePath(solution);
                string? sourceArchiveDirectory = Path.GetDirectoryName(sourceArchiveFilePath);

                // Create source archive directory if it doesn't exist
                if (!string.IsNullOrEmpty(sourceArchiveDirectory) && !Directory.Exists(sourceArchiveDirectory))
                {
                    Directory.CreateDirectory(sourceArchiveDirectory);
                }

                string sourceArchiveFileName = Path.GetFileName(sourceArchiveFilePath);

                if (_fileSystemService.CreateZipArchive(solution.DirectoryPath, sourceArchiveFilePath, solution.IgnoredObjectNames, solution.IgnoredObjectRelativePaths))
                {
                    output.Add($"Created source archive for solution: {sourceArchiveFilePath}");
                    _loggingService.Information("Created source archive for solution {SolutionName}: {ArchiveFileName}",
                        solution.Name, sourceArchiveFileName);
                }
                else
                {
                    output.Add($"Failed to create source archive for solution: {sourceArchiveFilePath}");
                    _loggingService.Warning("Failed to create source archive for solution {SolutionName}",
                        solution.Name);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error archiving project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        private List<string> RunDotNetCommand(List<string> arguments, string workingDirectory)
        {
            var output = new List<string>();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = string.Join(" ", arguments),
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                // Log the command we're about to run
                _loggingService.Information("Running dotnet command: {Command} in {Directory}",
                    string.Join(" ", arguments), workingDirectory);

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                // Read output
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        string prefixedLine = $"[OUT] {line}";
                        output.Add(line);  // Store original line without prefix in output

                        // Log to both console and log file
                        _loggingService.Information(prefixedLine);
                    }
                }

                // Read error
                while (!process.StandardError.EndOfStream)
                {
                    var line = process.StandardError.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        string prefixedLine = $"[ERR] {line}";
                        output.Add($"Error: {line}");  // Store error line in output

                        // Log to both console and log file
                        _loggingService.Warning(prefixedLine);
                    }
                }

                process.WaitForExit();

                // Log the exit code
                _loggingService.Information("Process exited with code: {ExitCode}", process.ExitCode);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error running dotnet command: {Command}", string.Join(" ", arguments));
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }
    }
}