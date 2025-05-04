using prjBuildApp.Models.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace prjBuildApp.Services
{
    public class CommandLineUIService
    {
        private readonly LoggingService _loggingService;
        private readonly ProjectManagementService _projectManagementService;
        private readonly BuildService _buildService;
        private readonly FileSystemService _fileSystemService;
        private readonly Models.Configuration.Settings _settings;
        private readonly List<ProjectInfo> _selectedProjects = new();
        private bool _showArchivedProjects = false;

        public CommandLineUIService(
            LoggingService loggingService,
            ProjectManagementService projectManagementService,
            BuildService buildService,
            FileSystemService fileSystemService,
            Models.Configuration.Settings settings)
        {
            _loggingService = loggingService;
            _projectManagementService = projectManagementService;
            _buildService = buildService;
            _fileSystemService = fileSystemService;
            _settings = settings;
        }

        public void Run()
        {
            _loggingService.Information("Starting prjBuild application");

            // Discover solutions and projects
            _projectManagementService.DiscoverSolutions();

            if (_projectManagementService.Solutions.Count == 0)
            {
                _loggingService.Warning("No solutions found. Please check your configuration.");
                return;
            }

            // Main application loop
            bool exit = false;
            while (!exit)
            {
                // Display project selection menu
                DisplayProjectSelectionMenu();

                // Get user choice
                Console.Write("Enter your choice: ");
                string? choice = Console.ReadLine();

                switch (choice?.ToLower())
                {
                    case "1":
                        SelectProject();
                        break;
                    case "2":
                        DeselectProject();
                        break;
                    case "3":
                        SelectAllProjects();
                        break;
                    case "4":
                        DeselectAllProjects();
                        break;
                    case "5":
                        if (_selectedProjects.Count > 0)
                        {
                            DisplayOperationsMenu();
                        }
                        else
                        {
                            _loggingService.Information("No projects selected. Please select at least one project.");
                        }
                        break;
                    case "6":
                        ToggleArchivedProjectsDisplay();
                        break;
                    case "7":
                    case "q":
                    case "quit":
                    case "exit":
                        exit = true;
                        break;
                    default:
                        _loggingService.Information("Invalid choice. Please try again.");
                        break;
                }
            }

            _loggingService.Information("Exiting prjBuild application");
        }

        private void DisplayProjectSelectionMenu()
        {
            Console.Clear();
            Console.WriteLine("=== prjBuild Project Selection ===");
            Console.WriteLine();

            // Display all projects with selection status
            var allProjects = _projectManagementService.Solutions
                .SelectMany(s => s.Projects.Where(p => !s.AreAllArchivesExisting || _showArchivedProjects))
                .OrderBy(p => p.Solution.Name)
                .ThenBy(p => p.Name)
                .ToList();

            Console.WriteLine("Projects:");
            for (int i = 0; i < allProjects.Count; i++)
            {
                var project = allProjects[i];
                string selectionStatus = _selectedProjects.Contains(project) ? "[X]" : "[ ]";
                string archiveStatus = project.Solution.AreAllArchivesExisting ? "[Archived]" : "[Not Archived]";
                string archiveNeededStatus = project.ExcludeFromArchiving == true ? "[Excluded From Archiving]" : "[Will Be Archived]";
                string versionStatus = project.ValidateVersions() ? "[Versions OK]" : "[Version Mismatch]";
                string runtimeStatus = project.SupportedRuntimes.Count > 0 ? $"[Runtimes: {project.SupportedRuntimes.Count}]" : "[No Runtimes]";
                Console.WriteLine($"{i + 1}. {selectionStatus} {archiveStatus} {archiveNeededStatus} {versionStatus} {runtimeStatus} {project.Solution.Name} / {project.Name}");
            }

            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("1. Select project");
            Console.WriteLine("2. Deselect project");
            Console.WriteLine("3. Select all projects");
            Console.WriteLine("4. Deselect all projects");
            Console.WriteLine("5. Operations menu");
            Console.WriteLine($"6. {(_showArchivedProjects ? "Hide" : "Show")} archived projects");
            Console.WriteLine("7. Exit");
            Console.WriteLine();
        }

        private void SelectProject()
        {
            var allProjects = _projectManagementService.Solutions
                .SelectMany(s => s.Projects.Where(p => !s.AreAllArchivesExisting || _showArchivedProjects))
                .OrderBy(p => p.Solution.Name)
                .ThenBy(p => p.Name)
                .ToList();

            Console.Write("Enter project number to select: ");
            if (int.TryParse(Console.ReadLine(), out int projectNumber) && projectNumber >= 1 && projectNumber <= allProjects.Count)
            {
                var project = allProjects[projectNumber - 1];
                if (!_selectedProjects.Contains(project))
                {
                    _selectedProjects.Add(project);
                    _loggingService.Information("Selected project: {SolutionName} / {ProjectName}", project.Solution.Name, project.Name);
                }
                else
                {
                    _loggingService.Information("Project already selected: {SolutionName} / {ProjectName}", project.Solution.Name, project.Name);
                }
            }
            else
            {
                _loggingService.Warning("Invalid project number entered");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void DeselectProject()
        {
            if (_selectedProjects.Count == 0)
            {
                _loggingService.Information("No projects selected for deselection");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Selected projects for deselection:");
            for (int i = 0; i < _selectedProjects.Count; i++)
            {
                var project = _selectedProjects[i];
                Console.WriteLine($"{i + 1}. {project.Solution.Name} / {project.Name}");
            }

            Console.Write("Enter project number to deselect: ");
            if (int.TryParse(Console.ReadLine(), out int projectNumber) && projectNumber >= 1 && projectNumber <= _selectedProjects.Count)
            {
                var project = _selectedProjects[projectNumber - 1];
                _selectedProjects.Remove(project);
                _loggingService.Information("Deselected project: {SolutionName} / {ProjectName}", project.Solution.Name, project.Name);
            }
            else
            {
                _loggingService.Warning("Invalid project number entered for deselection");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void SelectAllProjects()
        {
            _selectedProjects.Clear();

            foreach (var solution in _projectManagementService.Solutions)
            {
                foreach (var project in solution.Projects.Where(p => !solution.AreAllArchivesExisting || _showArchivedProjects))
                {
                    _selectedProjects.Add(project);
                }
            }

            _loggingService.Information("Selected all {Count} projects", _selectedProjects.Count);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void DeselectAllProjects()
        {
            int count = _selectedProjects.Count;
            _selectedProjects.Clear();

            _loggingService.Information("Deselected all {Count} projects", count);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void ToggleArchivedProjectsDisplay()
        {
            _showArchivedProjects = !_showArchivedProjects;
            _loggingService.Information("{Action} archived projects", _showArchivedProjects ? "Showing" : "Hiding");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void DisplayOperationsMenu()
        {
            bool back = false;
            while (!back)
            {
                Console.Clear();
                Console.WriteLine("=== prjBuild Operations ===");
                Console.WriteLine();

                Console.WriteLine($"Selected projects: {_selectedProjects.Count}");

                // Group projects by solution
                var projectsBySolution = _selectedProjects.GroupBy(p => p.Solution);

                foreach (var solutionGroup in projectsBySolution)
                {
                    var solution = solutionGroup.Key;
                    string solutionVersionStatus = solution.ValidateVersions() ? "[Solution Versions OK]" : "[Solution Version Mismatch]";
                    Console.WriteLine($"Solution: {solution.Name} {solutionVersionStatus}");

                    foreach (var project in solutionGroup)
                    {
                        string archiveStatus = project.Solution.AreAllArchivesExisting ? "[Archived]" : "[Not Archived]";
                        string archiveNeededStatus = project.ExcludeFromArchiving == true ? "[Excluded From Archiving]" : "[Will Be Archived]";
                        string versionStatus = project.ValidateVersions() ? "[Versions OK]" : "[Version Mismatch]";
                        string runtimeStatus = project.SupportedRuntimes.Count > 0 ? $"[Runtimes: {project.SupportedRuntimes.Count}]" : "[No Runtimes]";
                        Console.WriteLine($"  - {archiveStatus} {archiveNeededStatus} {versionStatus} {runtimeStatus} {project.Name}");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.WriteLine("Operations:");
                Console.WriteLine("1. Update NuGet packages - Check for and update all NuGet dependencies");
                Console.WriteLine("2. Build - Restore dependencies and compile the project");
                Console.WriteLine("3. Create Release - Clean, publish binaries, and archive source and binaries");
                Console.WriteLine("4. Archive Solutions - Create source archives for the solutions of selected projects");
                Console.WriteLine("5. Back to project selection");
                Console.WriteLine();

                Console.Write("Enter your choice: ");
                string? choice = Console.ReadLine();

                switch (choice?.ToLower())
                {
                    case "1":
                        UpdateNuGetPackages();
                        break;
                    case "2":
                        Build();
                        break;
                    case "3":
                        CreateRelease();
                        break;
                    case "4":
                        ArchiveSolutions();
                        break;
                    case "5":
                    case "b":
                    case "back":
                        back = true;
                        break;
                    default:
                        _loggingService.Warning("Invalid operation choice entered");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void UpdateNuGetPackages()
        {
            Console.Clear();
            Console.WriteLine("=== Update NuGet Packages ===");
            Console.WriteLine();

            foreach (var project in _selectedProjects)
            {
                _loggingService.Information("Updating NuGet packages for {SolutionName} - {ProjectName}...",
                    project.Solution.Name, project.Name);
                var output = _buildService.UpdateNuGetPackages(project);

                foreach (var line in output)
                {
                    _loggingService.Debug("NuGet update output: {Line}", line);
                }

                Console.WriteLine();
            }

            _loggingService.Information("NuGet package update completed");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void Build()
        {
            Console.Clear();
            Console.WriteLine("=== Quick Build ===");
            Console.WriteLine();

            foreach (var project in _selectedProjects)
            {
                _loggingService.Information("Building {SolutionName} - {ProjectName}...",
                    project.Solution.Name, project.Name);
                var output = _buildService.BuildProject(project);

                foreach (var line in output)
                {
                    _loggingService.Debug("Build output: {Line}", line);
                }

                Console.WriteLine();
            }

            _loggingService.Information("Build completed");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void CreateRelease()
        {
            Console.Clear();
            Console.WriteLine("=== Create Release ===");
            Console.WriteLine("This will clean the project, publish binaries, and create archives");
            Console.WriteLine();

            foreach (var project in _selectedProjects)
            {
                _loggingService.Information("Creating release for {SolutionName} - {ProjectName}...",
                    project.Solution.Name, project.Name);

                // Get the parent root directory for the solution
                var solution = project.Solution;
                var rootDir = _projectManagementService.Solutions
                    .SelectMany(s => s.Projects)
                    .FirstOrDefault(p => p.Solution == solution)?
                    .Solution.DirectoryPath;

                if (string.IsNullOrEmpty(rootDir))
                {
                    _loggingService.Error(null, "Could not determine root directory for project {ProjectName}", project.Name);
                    continue;
                }

                // First clean the project
                _loggingService.Information("Cleaning project {ProjectName}...", project.Name);
                var cleanOutput = _buildService.CleanupProject(project, true, true);
                foreach (var line in cleanOutput)
                {
                    _loggingService.Debug("Clean output: {Line}", line);
                }

                // Then publish the project
                string publishDirectory = Path.Combine(rootDir, "publish", project.Name);

                // Use the project's supported runtimes
                var supportedRuntimes = project.SupportedRuntimes;

                foreach (var runtime in supportedRuntimes)
                {
                    var publishOutput = _buildService.PublishProject(project, publishDirectory, runtime);
                    foreach (var line in publishOutput)
                    {
                        _loggingService.Debug("Publish output: {Line}", line);
                    }
                }

                // Then archive the project
                // Get the solution archive directory and create project-specific subdirectory
                string solutionArchiveDir = _fileSystemService.GetSolutionArchiveDirectory(project.Solution);
                string archiveDirectory = Path.Combine(solutionArchiveDir, project.Name);
                _loggingService.Information("Using archive directory: {ArchiveDirectory}", archiveDirectory);

                var archiveOutput = _buildService.ArchiveProject(project, archiveDirectory, supportedRuntimes);
                foreach (var line in archiveOutput)
                {
                    _loggingService.Debug("Archive output: {Line}", line);
                }

                // Update solution archive status if needed
                bool archivesExist = _fileSystemService.AreAllArchivesExisting(project.Solution);
                project.Solution.AreAllArchivesExisting = archivesExist;
                if (archivesExist)
                {
                    _loggingService.Information("Project {ProjectName} has been successfully archived", project.Name);
                }
                else
                {
                    _loggingService.Warning("Project {ProjectName} archiving may have failed", project.Name);
                }

                Console.WriteLine();
            }

            _loggingService.Information("Release creation completed");
        }

        private void ArchiveSolutions()
        {
            Console.Clear();
            Console.WriteLine("=== Archive Solutions ===");
            Console.WriteLine("This will create source archives for the solutions of selected projects");
            Console.WriteLine();

            // Get unique solutions from selected projects
            var solutions = _selectedProjects
                .Select(p => p.Solution)
                .Distinct()
                .ToList();

            if (solutions.Count == 0)
            {
                _loggingService.Warning("No solutions found for the selected projects");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            _loggingService.Information("Found {SolutionCount} solutions to archive", solutions.Count);

            foreach (var solution in solutions)
            {
                _loggingService.Information("Archiving solution {SolutionName}...", solution.Name);

                // Archive the solution
                var archiveOutput = _buildService.ArchiveSolution(solution);
                foreach (var line in archiveOutput)
                {
                    _loggingService.Debug("Archive output: {Line}", line);
                }

                // Check if the solution archive exists
                bool isArchived = File.Exists(solution.SourceArchivePath);
                if (isArchived)
                {
                    _loggingService.Information("Solution {SolutionName} has been successfully archived", solution.Name);
                    Console.WriteLine($"Solution {solution.Name} has been successfully archived");
                }
                else
                {
                    _loggingService.Warning("Solution {SolutionName} archiving may have failed", solution.Name);
                    Console.WriteLine($"Solution {solution.Name} archiving may have failed");
                }

                Console.WriteLine();
            }

            _loggingService.Information("Solution archiving completed");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}