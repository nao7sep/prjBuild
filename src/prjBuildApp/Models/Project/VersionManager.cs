using System;
using System.Collections.Generic;

namespace prjBuildApp.Models.Project
{
    public enum VersionSourceType
    {
        /// <summary>
        /// Version from .csproj project file.
        /// Typically appears in XML format like:
        /// <Version>1.2.3.4</Version> or
        /// <PackageVersion>1.2.3.4</PackageVersion> or
        /// <AssemblyVersion>1.2.3.4</AssemblyVersion>
        /// Usually found within a PropertyGroup element.
        /// </summary>
        CsprojFile,

        /// <summary>
        /// Version from AssemblyInfo.cs file.
        /// Appears as C# attributes like:
        /// [assembly: AssemblyVersion("1.2.3.4")] or
        /// [assembly: AssemblyFileVersion("1.2.3.4")]
        /// Usually found in Properties/AssemblyInfo.cs.
        /// May include multiple version attributes (AssemblyVersion, AssemblyFileVersion, etc.)
        /// </summary>
        AssemblyInfo,

        /// <summary>
        /// Version from app.manifest file.
        /// Appears in XML format like:
        /// <assemblyIdentity version="1.2.3.4" name="MyApplication.app"/>
        /// Usually found within the application manifest file.
        /// Used for ClickOnce deployments and Windows application manifests.
        /// </summary>
        Manifest
    }

    public class VersionSource
    {
        public VersionSourceType SourceType { get; }
        public string FilePath { get; }
        public string VersionString { get; }
        public Version? ParsedVersion { get; }

        public VersionSource(VersionSourceType sourceType, string filePath, string versionString)
        {
            SourceType = sourceType;
            FilePath = filePath;
            VersionString = versionString;

            try
            {
                ParsedVersion = Version.Parse(versionString);
            }
            catch
            {
                ParsedVersion = null;
            }
        }
    }

    public class VersionManager
    {
        public List<VersionSource> VersionSources { get; } = new();

        // Methods
        /// <summary>
        /// Validates that all version sources within this project have the same version
        /// </summary>
        /// <returns>True if all versions match, false otherwise</returns>
        public bool ValidateVersions()
        {
            if (VersionSources.Count == 0)
                return false;

            var primaryVersion = GetPrimaryVersionSource()?.ParsedVersion;
            if (primaryVersion == null)
                return false;

            foreach (var source in VersionSources)
            {
                if (source.ParsedVersion == null || source.ParsedVersion != primaryVersion)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that all projects in a solution have the same primary version
        /// </summary>
        /// <param name="solution">The solution containing projects to validate</param>
        /// <returns>True if all projects have the same primary version, false otherwise</returns>
        public static bool ValidateSolutionVersions(SolutionInfo solution)
        {
            if (solution.Projects.Count == 0)
                return false;

            // Get the primary version from the first project
            var firstProject = solution.Projects[0];
            var primaryVersion = firstProject.VersionManager.GetPrimaryVersionSource()?.ParsedVersion;

            if (primaryVersion == null)
                return false;

            // Check that all projects have the same primary version
            foreach (var project in solution.Projects)
            {
                var projectPrimaryVersion = project.VersionManager.GetPrimaryVersionSource()?.ParsedVersion;

                if (projectPrimaryVersion == null || projectPrimaryVersion != primaryVersion)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the primary version source for this project
        /// </summary>
        /// <returns>The primary version source, or null if none exists</returns>
        public VersionSource? GetPrimaryVersionSource()
        {
            // Prioritize csproj file version
            foreach (var source in VersionSources)
            {
                if (source.SourceType == VersionSourceType.CsprojFile)
                    return source;
            }

            // If no csproj file version, return the first one
            return VersionSources.Count > 0 ? VersionSources[0] : null;
        }

        /// <summary>
        /// Converts a Version object to a string in the format "v{major}.{minor}"
        /// </summary>
        /// <param name="version">The Version object to convert</param>
        /// <returns>A string representation of the version in the format "v{major}.{minor}"</returns>
        /// <exception cref="ArgumentNullException">Thrown when the version is null</exception>
        public static string FormatVersion(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version), "Version cannot be null");
            }

            return $"v{version.Major}.{version.Minor}";
        }

        /// <summary>
        /// Gets the formatted version string from the primary version source
        /// </summary>
        /// <returns>A formatted version string in the format "v{major}.{minor}", or throws an exception if no valid version exists</returns>
        /// <exception cref="InvalidOperationException">Thrown when no valid version source exists</exception>
        public string GetFormattedVersion()
        {
            var primarySource = GetPrimaryVersionSource();
            if (primarySource?.ParsedVersion == null)
            {
                throw new InvalidOperationException("No valid version information available");
            }

            return FormatVersion(primarySource.ParsedVersion);
        }
    }
}