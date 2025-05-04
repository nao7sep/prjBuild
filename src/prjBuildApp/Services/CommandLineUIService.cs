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
        private bool _showArchivedOrExcludedProjects = false;

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
                        ToggleArchivedOrExcludedProjectsDisplay();
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

            // Display all projects with selection status
            var allProjects = _projectManagementService.Solutions
                .SelectMany(s => s.Projects.Where(p =>
                    // Show only non-archived projects or all projects if _showArchivedOrExcludedProjects is true
                    (!s.AreAllArchivesExisting || _showArchivedOrExcludedProjects) &&
                    // Hide projects where ExcludeFromArchiving is true unless _showArchivedOrExcludedProjects is true
                    (p.ExcludeFromArchiving != true || _showArchivedOrExcludedProjects)))
                .OrderBy(p => p.Solution.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            Console.WriteLine("Projects:");
            for (int i = 0; i < allProjects.Count; i++)
            {
                var project = allProjects[i];

                // Format the number based on total count
                string numberFormat;
                if (allProjects.Count < 10)
                    numberFormat = $"{i + 1}.";
                else if (allProjects.Count < 100)
                    numberFormat = $"{i + 1,2}.";
                else if (allProjects.Count < 1000)
                    numberFormat = $"{i + 1,3}.";
                else
                    numberFormat = $"{i + 1,4}.";

                string selectionStatus = _selectedProjects.Contains(project) ? "[X]" : "[ ]";

                // Save the current console color
                ConsoleColor originalColor = Console.ForegroundColor;

                // Default view - simplified
                if (!_showArchivedOrExcludedProjects)
                {
                    // Write the basic information first
                    Console.Write($"{numberFormat} {selectionStatus} {project.Solution.Name} / {project.Name}");

                    // Collect problem messages
                    List<string> problems = new List<string>();
                    if (!project.ValidateVersions())
                    {
                        problems.Add("Version Mismatch");
                    }

                    if (project.SupportedRuntimes.Count == 0)
                    {
                        problems.Add("No Runtimes");
                    }

                    // Display problems if any exist
                    if (problems.Count > 0)
                    {
                        Console.Write(" ");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"({string.Join("; ", problems)})");
                        Console.ForegroundColor = originalColor;
                    }

                    // End the line
                    Console.WriteLine();
                }
                // Detailed view when toggle is on - without brackets
                else
                {
                    // Write the basic information first
                    Console.Write($"{numberFormat} {selectionStatus} {project.Solution.Name} / {project.Name}");

                    // Display all statuses with appropriate colors
                    Console.Write(" (");

                    // Archive status
                    if (project.Solution.AreAllArchivesExisting)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("Archived");
                        Console.ForegroundColor = originalColor;
                    }
                    else
                    {
                        Console.Write("Not archived");
                    }

                    Console.Write("; ");

                    // Archive needed status
                    if (project.ExcludeFromArchiving == true)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("Excluded from archiving");
                        Console.ForegroundColor = originalColor;
                    }
                    else
                    {
                        Console.Write("Will be archived");
                    }

                    Console.Write("; ");

                    // Version status
                    if (!project.ValidateVersions())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Version mismatch");
                        Console.ForegroundColor = originalColor;
                    }
                    else
                    {
                        Console.Write("Versions OK");
                    }

                    Console.Write("; ");

                    // Runtime status
                    if (project.SupportedRuntimes.Count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("No runtimes");
                        Console.ForegroundColor = originalColor;
                    }
                    else
                    {
                        Console.Write($"Runtimes: {project.SupportedRuntimes.Count}");
                    }

                    Console.Write(")");

                    // End the line
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Options:");
            Console.WriteLine("1. Select project");
            Console.WriteLine("2. Deselect project");
            Console.WriteLine("3. Select all projects");
            Console.WriteLine("4. Deselect all projects");
            Console.WriteLine("5. Operations menu");
            Console.WriteLine($"6. {(_showArchivedOrExcludedProjects ? "Simple view" : "Detailed view")}");
            Console.WriteLine("7. Exit");
        }

        private void SelectProject()
        {
            var allProjects = _projectManagementService.Solutions
                .SelectMany(s => s.Projects.Where(p =>
                    // Show only non-archived projects or all projects if _showArchivedOrExcludedProjects is true
                    (!s.AreAllArchivesExisting || _showArchivedOrExcludedProjects) &&
                    // Hide projects where ExcludeFromArchiving is true unless _showArchivedOrExcludedProjects is true
                    (p.ExcludeFromArchiving != true || _showArchivedOrExcludedProjects)))
                .OrderBy(p => p.Solution.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
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
            // Sort the selected projects by solution name and then by project name
            var sortedProjects = _selectedProjects
                .OrderBy(p => p.Solution.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (int i = 0; i < sortedProjects.Count; i++)
            {
                var project = sortedProjects[i];
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
                foreach (var project in solution.Projects.Where(p =>
                    // Show only non-archived projects or all projects if _showArchivedOrExcludedProjects is true
                    (!solution.AreAllArchivesExisting || _showArchivedOrExcludedProjects) &&
                    // Hide projects where ExcludeFromArchiving is true unless _showArchivedOrExcludedProjects is true
                    (p.ExcludeFromArchiving != true || _showArchivedOrExcludedProjects)))
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

        private void ToggleArchivedOrExcludedProjectsDisplay()
        {
            _showArchivedOrExcludedProjects = !_showArchivedOrExcludedProjects;
            _loggingService.Information("Switched to {ViewType} view", _showArchivedOrExcludedProjects ? "detailed" : "simple");
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

                Console.WriteLine($"Selected projects: {_selectedProjects.Count}");

                // Group projects by solution and sort projects within each solution
                var projectsBySolution = _selectedProjects
                    .GroupBy(p => p.Solution)
                    .OrderBy(g => g.Key.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var solutionGroup in projectsBySolution)
                {
                    var solution = solutionGroup.Key;
                    ConsoleColor originalSolutionColor = Console.ForegroundColor;

                    // Write solution name
                    Console.Write($"Solution: {solution.Name}");

                    // Add solution version problem if exists
                    if (!solution.ValidateVersions())
                    {
                        Console.Write(" ");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("(Solution version mismatch)");
                        Console.ForegroundColor = originalSolutionColor;
                    }

                    // Sort projects within each solution group by project name
                    foreach (var project in solutionGroup.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        // Save the current console color
                        ConsoleColor originalColor = Console.ForegroundColor;

                        // Default view - simplified
                        if (!_showArchivedOrExcludedProjects)
                        {
                            // Write the project name
                            Console.Write($"  - {project.Name}");

                            // Collect problem messages
                            List<string> problems = new List<string>();
                            if (!project.ValidateVersions())
                            {
                                problems.Add("Version Mismatch");
                            }

                            if (project.SupportedRuntimes.Count == 0)
                            {
                                problems.Add("No Runtimes");
                            }

                            // Display problems if any exist
                            if (problems.Count > 0)
                            {
                                Console.Write(" ");
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write($"({string.Join("; ", problems)})");
                                Console.ForegroundColor = originalColor;
                            }

                            // End the line
                            Console.WriteLine();
                        }
                        // Detailed view when toggle is on
                        else
                        {
                            // Write the basic information first
                            Console.Write("  - ");
                            Console.Write($"{project.Name}");

                            // Display all statuses with appropriate colors
                            Console.Write(" (");

                            // Archive status
                            if (project.Solution.AreAllArchivesExisting)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.Write("Archived");
                                Console.ForegroundColor = originalColor;
                            }
                            else
                            {
                                Console.Write("Not archived");
                            }

                            Console.Write("; ");

                            // Archive needed status
                            if (project.ExcludeFromArchiving == true)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.Write("Excluded from archiving");
                                Console.ForegroundColor = originalColor;
                            }
                            else
                            {
                                Console.Write("Will be archived");
                            }

                            Console.Write("; ");

                            // Version status
                            if (!project.ValidateVersions())
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("Version mismatch");
                                Console.ForegroundColor = originalColor;
                            }
                            else
                            {
                                Console.Write("Versions OK");
                            }

                            Console.Write("; ");

                            // Runtime status
                            if (project.SupportedRuntimes.Count == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("No runtimes");
                                Console.ForegroundColor = originalColor;
                            }
                            else
                            {
                                Console.Write($"Runtimes: {project.SupportedRuntimes.Count}");
                            }

                            Console.Write(")");

                            // End the line
                            Console.WriteLine();
                        }
                    }
                }

                Console.WriteLine("Operations:");
                Console.WriteLine("1. Update NuGet packages - Check for and update all NuGet packages");
                Console.WriteLine("2. Build projects - Restore dependencies and build projects");
                Console.WriteLine("3. Rebuild and archive projects - Cleanup/rebuild projects and create archives");
                Console.WriteLine("4. Return to selection");

                Console.Write("Enter your choice: ");
                string? choice = Console.ReadLine();

                switch (choice?.ToLower())
                {
                    case "1":
                        UpdateNuGetPackages();
                        break;
                    case "2":
                        BuildProjects();
                        break;
                    case "3":
                        RebuildAndArchiveProjects();
                        break;
                    case "4":
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

        /// <summary>
        /// Sorts projects based on their dependencies using topological sort.
        /// Projects with no dependencies come first, followed by projects that depend on them.
        /// </summary>
        /// <param name="projects">The list of projects to sort</param>
        /// <returns>A new list with projects sorted by dependency order</returns>
        private List<ProjectInfo> SortProjectsByDependencies(List<ProjectInfo> projects)
        {
            // Create a dictionary to track visited status for each project
            var visited = new Dictionary<ProjectInfo, bool>();

            // Initialize all projects as not visited
            foreach (var project in projects)
            {
                visited[project] = false;
            }

            // Result list to store the sorted projects
            var sortedProjects = new List<ProjectInfo>();

            // Process each project
            foreach (var project in projects)
            {
                if (!visited[project])
                {
                    // Perform depth-first search starting from this project
                    VisitProjectDependencies(project, visited, sortedProjects, projects);
                }
            }

            // The list is built in reverse order (dependencies last), so reverse it
            sortedProjects.Reverse();

            return sortedProjects;
        }

        /// <summary>
        /// Helper method for the topological sort that performs a depth-first traversal
        /// </summary>
        private void VisitProjectDependencies(
            ProjectInfo project,
            Dictionary<ProjectInfo, bool> visited,
            List<ProjectInfo> sortedProjects,
            List<ProjectInfo> allowedProjects)
        {
            // Mark the current project as visited
            visited[project] = true;

            // Visit all referenced projects first (if they are in our allowed list)
            foreach (var referencedProject in project.ReferencedProjects)
            {
                // Only process references that are in our selected projects list
                if (allowedProjects.Contains(referencedProject) && !visited[referencedProject])
                {
                    VisitProjectDependencies(referencedProject, visited, sortedProjects, allowedProjects);
                }
            }

            // After visiting all dependencies, add this project to the result
            sortedProjects.Add(project);
        }

        private void UpdateNuGetPackages()
        {
            Console.Clear();
            Console.WriteLine("=== Update NuGet packages ===");

            // Sort projects by dependencies
            var sortedProjects = SortProjectsByDependencies(_selectedProjects);

            foreach (var project in sortedProjects)
            {
                _loggingService.Information("Updating NuGet packages for {SolutionName} / {ProjectName}...",
                    project.Solution.Name, project.Name);
                var output = _buildService.UpdateNuGetPackages(project);

                foreach (var line in output)
                {
                    _loggingService.Debug("Output: {Line}", line);
                }
            }

            _loggingService.Information("NuGet package update completed");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void BuildProjects()
        {
            Console.Clear();
            Console.WriteLine("=== Build projects ===");

            // Sort projects by dependencies
            var sortedProjects = SortProjectsByDependencies(_selectedProjects);

            foreach (var project in sortedProjects)
            {
                _loggingService.Information("Building {SolutionName} / {ProjectName}...",
                    project.Solution.Name, project.Name);
                var output = _buildService.BuildProject(project);

                foreach (var line in output)
                {
                    _loggingService.Debug("Output: {Line}", line);
                }
            }

            _loggingService.Information("Build completed");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void RebuildAndArchiveProjects()
        {
            Console.Clear();
            Console.WriteLine("=== Rebuild and archive projects ===");
            Console.WriteLine("This will cleanup/rebuild projects and create archives");

            // Sort projects by dependencies
            var sortedProjects = SortProjectsByDependencies(_selectedProjects);

            foreach (var project in sortedProjects)
            {
                _loggingService.Information("Rebuilding and archiving {SolutionName} / {ProjectName}...",
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

                // First cleanup the project
                _loggingService.Information("Cleaning up project {ProjectName}...", project.Name);
                var cleanupOutput = _buildService.CleanupProject(project, true, true);
                foreach (var line in cleanupOutput)
                {
                    _loggingService.Debug("Output: {Line}", line);
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
                        _loggingService.Debug("Output: {Line}", line);
                    }
                }

                // Then archive the project
                // Use the solution's archive directory path directly
                string archiveDirectory = project.Solution.ArchiveDirectoryPath;
                _loggingService.Information("Using archive directory: {ArchiveDirectory}", archiveDirectory);

                var archiveOutput = _buildService.ArchiveProject(project, archiveDirectory, supportedRuntimes);
                foreach (var line in archiveOutput)
                {
                    _loggingService.Debug("Output: {Line}", line);
                }

                // Log that archiving is complete
                _loggingService.Information("Project {ProjectName} archiving completed", project.Name);
            }

            _loggingService.Information("Rebuild and archive completed");
        }
    }
}