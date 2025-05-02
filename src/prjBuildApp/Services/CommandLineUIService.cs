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

        private readonly List<ProjectInfo> _selectedProjects = new();

        public CommandLineUIService(
            LoggingService loggingService,
            ProjectManagementService projectManagementService,
            BuildService buildService)
        {
            _loggingService = loggingService;
            _projectManagementService = projectManagementService;
            _buildService = buildService;
        }

        public void Run()
        {
            _loggingService.Information("Starting prjBuild application");

            // Discover solutions and projects
            _projectManagementService.DiscoverSolutions();

            if (_projectManagementService.Solutions.Count == 0)
            {
                Console.WriteLine("No solutions found. Please check your configuration.");
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
                            Console.WriteLine("No projects selected. Please select at least one project.");
                        }
                        break;
                    case "6":
                    case "q":
                    case "quit":
                    case "exit":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
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
                .SelectMany(s => s.Projects)
                .OrderBy(p => p.Solution.Name)
                .ThenBy(p => p.Name)
                .ToList();

            Console.WriteLine("Projects:");
            for (int i = 0; i < allProjects.Count; i++)
            {
                var project = allProjects[i];
                string selectionStatus = _selectedProjects.Contains(project) ? "[X]" : "[ ]";
                Console.WriteLine($"{i + 1}. {selectionStatus} {project.Solution.Name} - {project.Name}");
            }

            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("1. Select project");
            Console.WriteLine("2. Deselect project");
            Console.WriteLine("3. Select all projects");
            Console.WriteLine("4. Deselect all projects");
            Console.WriteLine("5. Operations menu");
            Console.WriteLine("6. Exit");
            Console.WriteLine();
        }

        private void SelectProject()
        {
            var allProjects = _projectManagementService.Solutions
                .SelectMany(s => s.Projects)
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
                    Console.WriteLine($"Selected project: {project.Solution.Name} - {project.Name}");
                }
                else
                {
                    Console.WriteLine($"Project already selected: {project.Solution.Name} - {project.Name}");
                }
            }
            else
            {
                Console.WriteLine("Invalid project number.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void DeselectProject()
        {
            if (_selectedProjects.Count == 0)
            {
                Console.WriteLine("No projects selected.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Selected projects:");
            for (int i = 0; i < _selectedProjects.Count; i++)
            {
                var project = _selectedProjects[i];
                Console.WriteLine($"{i + 1}. {project.Solution.Name} - {project.Name}");
            }

            Console.Write("Enter project number to deselect: ");
            if (int.TryParse(Console.ReadLine(), out int projectNumber) && projectNumber >= 1 && projectNumber <= _selectedProjects.Count)
            {
                var project = _selectedProjects[projectNumber - 1];
                _selectedProjects.Remove(project);
                Console.WriteLine($"Deselected project: {project.Solution.Name} - {project.Name}");
            }
            else
            {
                Console.WriteLine("Invalid project number.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void SelectAllProjects()
        {
            _selectedProjects.Clear();

            foreach (var solution in _projectManagementService.Solutions)
            {
                foreach (var project in solution.Projects)
                {
                    _selectedProjects.Add(project);
                }
            }

            Console.WriteLine($"Selected all {_selectedProjects.Count} projects.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void DeselectAllProjects()
        {
            int count = _selectedProjects.Count;
            _selectedProjects.Clear();

            Console.WriteLine($"Deselected all {count} projects.");
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
                foreach (var project in _selectedProjects)
                {
                    Console.WriteLine($"- {project.Solution.Name} - {project.Name}");
                }

                Console.WriteLine();
                Console.WriteLine("Operations:");
                Console.WriteLine("1. Update NuGet packages");
                Console.WriteLine("2. Restore dependencies");
                Console.WriteLine("3. Quick build");
                Console.WriteLine("4. Cleanup");
                Console.WriteLine("5. Rebuild");
                Console.WriteLine("6. Archive");
                Console.WriteLine("7. Back to project selection");
                Console.WriteLine();

                Console.Write("Enter your choice: ");
                string? choice = Console.ReadLine();

                switch (choice?.ToLower())
                {
                    case "1":
                        UpdateNuGetPackages();
                        break;
                    case "2":
                        RestoreDependencies();
                        break;
                    case "3":
                        QuickBuild();
                        break;
                    case "4":
                        Cleanup();
                        break;
                    case "5":
                        Rebuild();
                        break;
                    case "6":
                        Archive();
                        break;
                    case "7":
                    case "b":
                    case "back":
                        back = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
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
                Console.WriteLine($"Updating NuGet packages for {project.Solution.Name} - {project.Name}...");
                var output = _buildService.UpdateNuGetPackages(project);

                foreach (var line in output)
                {
                    Console.WriteLine($"  {line}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("NuGet package update completed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void RestoreDependencies()
        {
            Console.Clear();
            Console.WriteLine("=== Restore Dependencies ===");
            Console.WriteLine();

            foreach (var project in _selectedProjects)
            {
                Console.WriteLine($"Restoring dependencies for {project.Solution.Name} - {project.Name}...");
                var output = _buildService.RestoreProject(project);

                foreach (var line in output)
                {
                    Console.WriteLine($"  {line}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Dependency restoration completed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void QuickBuild()
        {
            Console.Clear();
            Console.WriteLine("=== Quick Build ===");
            Console.WriteLine();

            foreach (var project in _selectedProjects)
            {
                Console.WriteLine($"Building {project.Solution.Name} - {project.Name}...");
                var output = _buildService.BuildProject(project);

                foreach (var line in output)
                {
                    Console.WriteLine($"  {line}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Build completed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void Cleanup()
        {
            Console.Clear();
            Console.WriteLine("=== Cleanup ===");
            Console.WriteLine();

            foreach (var project in _selectedProjects)
            {
                Console.WriteLine($"Cleaning up {project.Solution.Name} - {project.Name}...");
                var output = _buildService.CleanupProject(project);

                foreach (var line in output)
                {
                    Console.WriteLine($"  {line}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Cleanup completed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void Rebuild()
        {
            Console.Clear();
            Console.WriteLine("=== Rebuild ===");
            Console.WriteLine();

            foreach (var project in _selectedProjects)
            {
                Console.WriteLine($"Rebuilding {project.Solution.Name} - {project.Name}...");

                // Clean the project first
                var cleanOutput = _buildService.CleanupProject(project);
                foreach (var line in cleanOutput)
                {
                    Console.WriteLine($"  {line}");
                }

                // Then build it
                var buildOutput = _buildService.BuildProject(project);
                foreach (var line in buildOutput)
                {
                    Console.WriteLine($"  {line}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Rebuild completed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void Archive()
        {
            Console.Clear();
            Console.WriteLine("=== Archive ===");
            Console.WriteLine();

            foreach (var project in _selectedProjects)
            {
                Console.WriteLine($"Archiving {project.Solution.Name} - {project.Name}...");

                // Get the parent root directory for the solution
                var solution = project.Solution;
                var rootDir = _projectManagementService.Solutions
                    .SelectMany(s => s.Projects)
                    .FirstOrDefault(p => p.Solution == solution)?
                    .Solution.DirectoryPath;

                if (string.IsNullOrEmpty(rootDir))
                {
                    Console.WriteLine("  Error: Could not determine root directory for archiving.");
                    continue;
                }

                string archiveDirectory = Path.Combine(rootDir, "archives");

                // For simplicity, we'll just use a default runtime
                var supportedRuntimes = new List<string> { "win-x64" };

                var output = _buildService.ArchiveProject(project, archiveDirectory, supportedRuntimes);
                foreach (var line in output)
                {
                    Console.WriteLine($"  {line}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Archive completed.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}