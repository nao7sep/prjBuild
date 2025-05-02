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
        /// Indicates if the object is obsolete and should be excluded from normal operations
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// List of object names that should be ignored during operations
        /// </summary>
        public List<string> IgnoredObjectNames { get; } = new();

        /// <summary>
        /// List of relative paths that should be ignored during operations
        /// </summary>
        public List<string> IgnoredObjectRelativePaths { get; } = new();

        /// <summary>
        /// Merges ignore lists from multiple sources
        /// </summary>
        /// <param name="sources">Collections of strings to merge</param>
        /// <returns>A new list containing all unique items from the sources</returns>
        protected static List<string> MergeIgnoreLists(params IEnumerable<string>?[] sources)
        {
            return sources
                .Where(source => source != null)
                .SelectMany(source => source!)
                .Distinct()
                .ToList();
        }
    }
}