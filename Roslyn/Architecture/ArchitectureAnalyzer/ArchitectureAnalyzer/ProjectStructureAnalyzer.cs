using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArchitectureAnalyzer
{
    internal class ProjectStructureAnalyzer
    {
        /// <summary>
        /// <seealso cref="https://stackoverflow.com/a/9302642/444033"/>
        /// </summary>        
        internal static async Task CheckSymbolsInSubnamespaceAreInternalOrAssert(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();

                var publicSymbols = SemanticModelHelper.GetNamedTypeSymbols(compilation)
                    .Where(symbol => IsProjectSourceSymbolByNamespace(symbol, project))
                    .Where(symbol => symbol.DeclaredAccessibility == Accessibility.Public);

                foreach (var publicSymbol in publicSymbols)
                {
                    foreach (var location in publicSymbol.Locations)
                    {
                        if (Path.GetDirectoryName(location.SourceTree.FilePath) != Path.GetDirectoryName(project.FilePath))
                        {
                            throw new AssertException($"public classes are not allowed to be in subfolders: {publicSymbol}");
                        }
                    }
                }
            }
        }

        internal static async Task CheckAllSymbolsHaveDefaultNamespaceOrAssert(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();

                var sourceBasedSymbols = SemanticModelHelper.GetNamedTypeSymbols(compilation).Where(symbol => IsProjectSourceSymbolByFileLocation(symbol, project));

                foreach (var symbol in sourceBasedSymbols)
                {
                    foreach (var location in symbol.Locations)
                    {
                        var expectedNamespace = GetExpectedNamespaceByLocation(project, location);

                        if (symbol.ContainingNamespace.ToDisplayString() != expectedNamespace)
                        {
                            throw new AssertException($"{symbol.ToDisplayString()} should be in namespace {expectedNamespace}");
                        }

                    }
                }
            }
        }

        internal static void CheckDefaultNamespaceEqualsProjectNameOrAssert(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                if (Path.GetFileNameWithoutExtension(project.FilePath).Replace(" ", "_") != project.DefaultNamespace)
                {
                    throw new AssertException($"Defaultnamespace must be projectname project: '{project.FilePath}' default namespace: '{project.DefaultNamespace}'");
                }
            }
        }

        internal static void CheckAllProjectsHaveMarkdownDocumentationOrAssert(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                if (File.Exists(Path.Combine(Path.GetDirectoryName(project.FilePath), "InterfaceDescription.md")) == false)
                {
                    throw new AssertException($"Every project needs an 'InterfaceDescription.md' in root. Project: '{project.Name}'");
                }
            }
        }

        private static bool IsProjectSourceSymbolByFileLocation(INamedTypeSymbol namedTypeSymbol, Project project)
        {
            return namedTypeSymbol.TypeKind == TypeKind.Class && namedTypeSymbol.Locations.Any(location => location.IsInSource && location.SourceTree.FilePath.StartsWith(Path.GetDirectoryName(project.FilePath)));
        }

        private static bool IsProjectSourceSymbolByNamespace(INamedTypeSymbol namedTypeSymbol, Project project)
        {
            return namedTypeSymbol.TypeKind == TypeKind.Class && namedTypeSymbol.ContainingNamespace.ToDisplayString().StartsWith(project.DefaultNamespace);
        }

        private static string GetExpectedNamespaceByLocation(Project project, Location location)
        {
            var expectedNamespace = project.DefaultNamespace;

            if (Path.GetDirectoryName(location.SourceTree.FilePath) != Path.GetDirectoryName(project.FilePath))
            {
                var relativeFilePath = Path.GetDirectoryName(location.SourceTree.FilePath).Substring(Path.GetDirectoryName(project.FilePath).Length);

                expectedNamespace += $"{relativeFilePath.Replace(Path.DirectorySeparatorChar, '.').Replace(" ", "_")}";
            }

            return expectedNamespace;
        }
    }
}