using System;
using System.Collections.Generic;

namespace prjBuildApp.Models.Project
{
    public enum VersionSourceType
    {
        CsprojFile,    // Version from project file
        AssemblyInfo,  // Version from AssemblyInfo.cs
        AssemblyFileInfo, // Version from assembly file version attribute
        Manifest,      // Version from app.manifest
        Other          // Version from other sources
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