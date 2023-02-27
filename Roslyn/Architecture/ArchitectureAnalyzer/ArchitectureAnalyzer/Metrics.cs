using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArchitectureAnalyzer
{
    internal class Metrics
    {
        internal static async Task MaximalLinesOfCodePerMethodOrAssertAsync(Solution solution, int maximalMethodLength)
        {
            foreach (var project in solution.Projects)
            {
                var compilation = await project.GetCompilationAsync();

                foreach (var document in project.Documents)
                {
                    var tree = await document.GetSyntaxTreeAsync();

                    var model = compilation.GetSemanticModel(tree);

                    foreach (var method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                    {
                        int linesOfCode = GetLinesOfCode(method);

                        var declaredSymbol = model.GetDeclaredSymbol(method);

                        if (linesOfCode > maximalMethodLength)
                        {
                            Console.WriteLine($"{declaredSymbol.ContainingType}.{method.Identifier} too long: {linesOfCode}");
                        }
                    }
                }
            }
        }

        private static int GetLinesOfCode(MethodDeclarationSyntax method)
        {
            var span = method.SyntaxTree.GetLineSpan(method.Span);

            return span.EndLinePosition.Line - span.StartLinePosition.Line;
        }
    }
}