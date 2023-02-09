using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArchitectureAnalyzer
{
    internal class ProjectDependeciesAnalyzer
    {
        internal static void CheckDependenciesOrAssert(Solution solution, string projectName, string[] allowedDependendProjectNames)
        {
            var projectId = solution.Projects.Single(p => p.Name == projectName).Id;
            var projectDependecyGraph = solution.GetProjectDependencyGraph();

            var projectsThatDependOnThisProject = projectDependecyGraph
                .GetProjectsThatTransitivelyDependOnThisProject(projectId)
                .Select(p => solution.GetProject(p).Name);

            var projectsThatThisProjectDependOn = projectDependecyGraph
                .GetProjectsThatThisProjectTransitivelyDependsOn(projectId)
                .Select(p => solution.GetProject(p).Name);

            var projectDependencyNames = projectsThatDependOnThisProject.Union(projectsThatThisProjectDependOn);

            if (projectDependencyNames.OrderBy(i => i).SequenceEqual(allowedDependendProjectNames.OrderBy(i => i)) == false)
            {
                var missingDependency = allowedDependendProjectNames.Where(a => projectDependencyNames.Any(d => d == a) == false);
                var invalidDependency = projectDependencyNames.Where(d => allowedDependendProjectNames.Any(a => a == d) == false);

                var validationErrors = new List<string>();

                if (missingDependency.Any())
                {
                    validationErrors.Add($"'{projectName}' expects dependecy for {string.Join(", ", missingDependency)}");
                }

                if (invalidDependency.Any())
                {
                    validationErrors.Add($"'{projectName}' didn't expects dependecy for {string.Join(", ", invalidDependency)}");
                }

                throw new AssertException($"There are invalid dependencies: {string.Join(", ", validationErrors)}");
            }
        }

        internal static string GeneratePlantUmlOfComponents(Solution solution)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("@startuml");
            stringBuilder.AppendLine("set namespaceSeparator none");

            foreach (var project in solution.Projects)
            {
                stringBuilder.AppendLine($"component {project.Name}");
            }

            foreach (var project in solution.Projects)
            {
                foreach(var dependecy in solution.GetProjectDependencyGraph().GetProjectsThatTransitivelyDependOnThisProject(project.Id))
                {
                    stringBuilder.AppendLine($"{project.Name} -0)- {solution.GetProject(dependecy).Name}");
                }
            }

            stringBuilder.AppendLine("@enduml");

            return stringBuilder.ToString();
        }
    }
}