using System.Collections.Generic;
using System.Linq;
using prjBuildApp.Models.Configuration;

namespace prjBuildApp.Models.Project
{
    /// <summary>
    /// Base class for models that inherit properties from configuration sources
    /// </summary>
    public abstract class InheritedPropertiesBase
    {
        /// <summary>
        /// List of object names that should be ignored during operations
        /// </summary>
        public List<string> IgnoredObjectNames { get; } = new();

        /// <summary>
        /// List of relative paths that should be ignored during operations
        /// </summary>
        public List<string> IgnoredObjectRelativePaths { get; } = new();

    }
}