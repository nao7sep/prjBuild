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
                _loggingService.Information("Updating NuGet packages for project {ProjectName}", project.Name);

                // First, list outdated packages
                var listArguments = new List<string> { "list", project.FilePath, "package", "--outdated" };
                var listResult = RunDotNetCommand(listArguments, project.DirectoryPath);
                output.AddRange(listResult);

                // Extract package names from the output
                var packageLines = listResult.Where(line => line.Contains(">")).ToList();
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
                        var updateArguments = new List<string> { "add", project.FilePath, "package", packageName };
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

        public List<string> RestoreProject(ProjectInfo project)
        {
            var output = new List<string>();

            try
            {
                _loggingService.Information("Restoring dependencies for project {ProjectName}", project.Name);

                var arguments = new List<string> { "restore", project.FilePath };
                var result = RunDotNetCommand(arguments, project.DirectoryPath);
                output.AddRange(result);

                if (result.Any(line => line.Contains("Restore completed")))
                {
                    _loggingService.Information("Successfully restored dependencies for project {ProjectName}", project.Name);
                }
                else
                {
                    _loggingService.Warning("Restore may have failed for project {ProjectName}", project.Name);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error restoring dependencies for project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        public List<string> BuildProject(ProjectInfo project, string? runtime = null, bool noRestore = false)
        {
            var output = new List<string>();

            try
            {
                _loggingService.Information("Building project {ProjectName}", project.Name);

                var arguments = new List<string> { "build", project.FilePath, "--configuration", "Release" };

                if (!string.IsNullOrEmpty(runtime))
                {
                    arguments.Add("--runtime");
                    arguments.Add(runtime);
                    arguments.Add("--self-contained");
                }

                if (noRestore)
                {
                    arguments.Add("--no-restore");
                }

                var result = RunDotNetCommand(arguments, project.DirectoryPath);
                output.AddRange(result);

                if (result.Any(line => line.Contains("Build succeeded")))
                {
                    _loggingService.Information("Successfully built project {ProjectName}", project.Name);
                }
                else
                {
                    _loggingService.Warning("Build may have failed for project {ProjectName}", project.Name);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error building project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        public List<string> PublishProject(ProjectInfo project, string outputDirectory, string? runtime = null)
        {
            var output = new List<string>();

            try
            {
                _loggingService.Information("Publishing project {ProjectName}", project.Name);

                // Create output directory if it doesn't exist
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                    output.Add($"Created directory: {outputDirectory}");
                }

                var arguments = new List<string> {
                    "publish",
                    project.FilePath,
                    "--configuration", "Release",
                    "--output", outputDirectory
                };

                if (!string.IsNullOrEmpty(runtime))
                {
                    arguments.Add("--runtime");
                    arguments.Add(runtime);
                    arguments.Add("--self-contained");
                }

                var result = RunDotNetCommand(arguments, project.DirectoryPath);
                output.AddRange(result);

                if (result.Any(line => line.Contains("Published")))
                {
                    _loggingService.Information("Successfully published project {ProjectName} to {OutputDirectory}",
                        project.Name, outputDirectory);
                }
                else
                {
                    _loggingService.Warning("Publish may have failed for project {ProjectName}", project.Name);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error publishing project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        public List<string> CleanupProject(ProjectInfo project, bool deleteBinDirectory = false, bool deleteObjDirectory = true)
        {
            var output = new List<string>();

            try
            {
                _loggingService.Information("Cleaning up project {ProjectName}", project.Name);

                // Clean the project using dotnet clean
                var cleanArguments = new List<string> { "clean", project.FilePath };
                var cleanResult = RunDotNetCommand(cleanArguments, project.DirectoryPath);
                output.AddRange(cleanResult);

                // Delete bin directory if specified
                if (deleteBinDirectory)
                {
                    string binDir = Path.Combine(project.DirectoryPath, "bin");
                    if (Directory.Exists(binDir))
                    {
                        Directory.Delete(binDir, true);
                        output.Add($"Deleted directory: {binDir}");
                        _loggingService.Information("Deleted bin directory for project {ProjectName}", project.Name);
                    }
                }

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

        public List<string> ArchiveProject(ProjectInfo project, string archiveDirectory, List<string> supportedRuntimes)
        {
            var output = new List<string>();

            try
            {
                _loggingService.Information("Archiving project {ProjectName}", project.Name);

                // Create archive directory if it doesn't exist
                if (!Directory.Exists(archiveDirectory))
                {
                    Directory.CreateDirectory(archiveDirectory);
                }

                // Archive binaries for each supported runtime
                foreach (var runtime in supportedRuntimes)
                {
                    // Build the project for this runtime with noRestore=true for efficiency
                    var buildOutput = BuildProject(project, runtime, true);
                    output.AddRange(buildOutput);

                    // Create binary archive
                    string binDir = Path.Combine(project.DirectoryPath, "bin", "Release", "net9.0", runtime);
                    if (Directory.Exists(binDir))
                    {
                        // Get all runtime archive paths
                        var archivePaths = _fileSystemService.GetProjectRuntimeArchivePaths(project);
                        string archiveFilePath = archivePaths[runtime];
                        string archiveFileName = Path.GetFileName(archiveFilePath);

                        if (_fileSystemService.CreateZipArchive(binDir, archiveFilePath))
                        {
                            output.Add($"Created binary archive: {archiveFilePath}");
                            _loggingService.Information("Created binary archive for project {ProjectName} and runtime {Runtime}: {ArchiveFileName}",
                                project.Name, runtime, archiveFileName);
                        }
                    }
                }

                // Archive source code (at solution level)
                var solution = project.Solution;
                string sourceArchiveFilePath = _fileSystemService.GetSolutionSourceArchivePath(solution);
                string sourceArchiveFileName = Path.GetFileName(sourceArchiveFilePath);

                if (_fileSystemService.CreateZipArchive(solution.DirectoryPath, sourceArchiveFilePath))
                {
                    output.Add($"Created source archive for solution: {sourceArchiveFilePath}");
                    _loggingService.Information("Created source archive for solution {SolutionName}: {ArchiveFileName}",
                        solution.Name, sourceArchiveFileName);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error archiving project {ProjectName}", project.Name);
                output.Add($"Error: {ex.Message}");
            }

            return output;
        }

        /// <summary>
        /// Archives a solution by creating a zip file of its source code
        /// </summary>
        /// <param name="solution">The solution to archive</param>
        /// <returns>A list of output messages from the archiving process</returns>
        public List<string> ArchiveSolution(SolutionInfo solution)
        {
            var output = new List<string>();

            try
            {
                _loggingService.Information("Archiving solution {SolutionName}", solution.Name);

                // Get the archive file path for the solution
                string archiveFilePath = _fileSystemService.GetSolutionSourceArchivePath(solution);

                // Create the archive directory if it doesn't exist
                string? archiveDirectory = Path.GetDirectoryName(archiveFilePath);
                if (!string.IsNullOrEmpty(archiveDirectory) && !Directory.Exists(archiveDirectory))
                {
                    Directory.CreateDirectory(archiveDirectory);
                }

                // Archive the solution source code
                if (_fileSystemService.CreateZipArchive(solution.DirectoryPath, archiveFilePath))
                {
                    output.Add($"Created solution archive: {archiveFilePath}");
                    string fileName = Path.GetFileName(archiveFilePath);
                    _loggingService.Information("Created archive for solution {SolutionName}: {ArchiveFileName}",
                        solution.Name, fileName);
                }
                else
                {
                    output.Add($"Failed to create solution archive: {archiveFilePath}");
                    _loggingService.Warning("Failed to create archive for solution {SolutionName}", solution.Name);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error archiving solution {SolutionName}", solution.Name);
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
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                // Read output
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        output.Add(line);
                    }
                }

                // Read error
                while (!process.StandardError.EndOfStream)
                {
                    var line = process.StandardError.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        output.Add($"Error: {line}");
                    }
                }

                process.WaitForExit();
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