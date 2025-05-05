using System;
using System.Collections.Generic;

namespace prjBuildApp.Models.Project
{
    /// <summary>
    /// Extension methods for the Version class
    /// </summary>
    public static class VersionExtensions
    {
        /// <summary>
        /// Compares two Version objects field-by-field
        /// </summary>
        /// <param name="version1">The first version to compare</param>
        /// <param name="version2">The second version to compare</param>
        /// <returns>True if the versions are equal, false otherwise</returns>
        public static bool AreVersionsEqual(this Version? version1, Version? version2)
        {
            // If both are null, they're considered undefined, return false
            if (version1 == null && version2 == null)
                return false;

            // If only one is null, they're not equal
            if (version1 == null || version2 == null)
                return false;

            // Get normalized build values (treat -1 or lower as 0)
            int build1 = version1.Build <= 0 ? 0 : version1.Build;
            int build2 = version2.Build <= 0 ? 0 : version2.Build;

            // Get normalized revision values (treat -1 or lower as 0)
            int revision1 = version1.Revision <= 0 ? 0 : version1.Revision;
            int revision2 = version2.Revision <= 0 ? 0 : version2.Revision;

            // Compare all fields with normalized values for Build and Revision
            return version1.Major == version2.Major &&
                   version1.Minor == version2.Minor &&
                   build1 == build2 &&
                   revision1 == revision2;
        }
    }

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
        public bool ValidateProjectVersions()
        {
            if (VersionSources.Count == 0)
                return false;

            var primaryVersion = GetPrimaryVersionSource()?.ParsedVersion;
            if (primaryVersion == null)
                return false;

            foreach (var source in VersionSources)
            {
                if (source.ParsedVersion == null || !source.ParsedVersion.AreVersionsEqual(primaryVersion))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that all projects in a solution have the same primary version
        /// </summary>
        /// <param name="solution">The solution containing projects to validate</param>
        /// <returns>True if all projects have the same primary version, false otherwise</returns>
        public static bool ValidateSolutionAndProjectVersions(SolutionInfo solution)
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

                if (projectPrimaryVersion == null || !projectPrimaryVersion.AreVersionsEqual(primaryVersion))
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
        /// Converts a Version object to a formatted string according to these rules:
        /// 1. Always displays Major and Minor (first 2 fields)
        /// 2. If Build (3rd field) is not 0, it is displayed
        /// 3. If Revision (4th field) is not 0, both Build and Revision are displayed regardless of Build's value
        /// Note: If Minor or Revision is -1, they will be treated as not specified in the output format
        /// </summary>
        /// <param name="version">The Version object to convert</param>
        /// <returns>A string representation of the version in the format "v{major}.{minor}[.{build}[.{revision}]]"</returns>
        /// <exception cref="ArgumentNullException">Thrown when the version is null</exception>
        public static string FormatVersion(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version), "Version cannot be null");
            }

            // Always display Major and Minor
            string formattedVersion = $"v{version.Major}.{version.Minor}";

            // If Revision (4th field) is not 0, display both Build and Revision
            if (version.Revision > 0)
            {
                formattedVersion = $"{formattedVersion}.{version.Build}.{version.Revision}";
            }
            // If only Build (3rd field) is not 0, display just the Build
            else if (version.Build > 0)
            {
                formattedVersion = $"{formattedVersion}.{version.Build}";
            }

            return formattedVersion;
        }
    }
}