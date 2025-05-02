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
    }
}