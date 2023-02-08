using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureAnalyzer
{
    internal class ProjectDependeciesAnalyzer
    {
        internal static async Task CheckDependenciesAsync(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                Console.WriteLine(project.Name);
                var foo = solution.GetProjectDependencyGraph().GetProjectsThatTransitivelyDependOnThisProject(project.Id);

                foreach(var dependency in foo) {
                    Console.WriteLine($"--> {dependency}");
                }
            }
            
        }
    }
}
