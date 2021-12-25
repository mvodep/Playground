using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynHelloWorld
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            var slnFile = new FileInfo(@"C:\Users\michael.vodep\source\repos\HelloWorld\HelloWorld.sln");

            RestoreNuget(slnFile);

            using (var workspace = MSBuildWorkspace.Create())
            {
                workspace.WorkspaceFailed += WorkspaceFailed;

                var solution = await workspace.OpenSolutionAsync(slnFile.FullName);

                var helloWorldProject = solution.Projects.Single(p => p.Name == "HelloWorld");

                var helloWorldCompilation = await helloWorldProject.GetCompilationAsync();

                PrintErrors(helloWorldCompilation);

                // Was a reference Foo added?
                var referenceAdded = helloWorldCompilation.References.Any(r => r.Display.Contains("Foo"));

                Console.WriteLine($"Reference Foo was added: {referenceAdded}");

                // Was a method called?
                var serializeMethodCalls = await FindSymbolInfowByMethodAsync(helloWorldProject, "System.Text.Json.JsonSerializer", "Serialize");

                foreach(var methodCall in serializeMethodCalls)
                {
                    Console.WriteLine($"{methodCall.SymbolInfo.Name} -> {string.Join(",", methodCall.Arguments)}");
                }

                // Does the class with the interface IWeatherForcast has a specific member?
                var weatherForcastSymbolInfo = GetNamedTypeSymbols(helloWorldCompilation).FirstOrDefault(s => s.Interfaces.Any(i => i.Name == "IWeatherForecast"));

                var hasMember = weatherForcastSymbolInfo.MemberNames.Any(m => m == "Date");

                Console.WriteLine($"Class with interface IWeatherForecast has member Date: {hasMember}");
            }

            Console.ReadKey();
        }

        private static void PrintErrors(Compilation compilation)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            foreach (var error in compilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error))
            {
                Console.WriteLine($"ERROR {error.GetMessage()}");
            }

            Console.ResetColor();
        }

        private static void WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
        {
            Console.WriteLine(e.Diagnostic.ToString());
        }

        private static void RestoreNuget(FileInfo slnFile)
        {
            using (Process compiler = new Process())
            {
                compiler.StartInfo.FileName = "dotnet";
                compiler.StartInfo.Arguments = $"restore {slnFile}";
                compiler.StartInfo.UseShellExecute = true;
                compiler.Start();

                compiler.WaitForExit();
            }
        }

        private class MethodCallFindResult
        {
            public IEnumerable<string> Arguments { get; internal set; }
            public ISymbol SymbolInfo { get; internal set; }
        }

        private static async Task<IList<MethodCallFindResult>> FindSymbolInfowByMethodAsync(Project project, string containingType, string methodName)
        {
            IList<MethodCallFindResult> result = new List<MethodCallFindResult>();

            foreach (var document in project.Documents)
            {
                var syntaxRoot = await document.GetSyntaxRootAsync();
                var semanticModel = await document.GetSemanticModelAsync();

                foreach (var methodInvocation in syntaxRoot.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    var findResult = new MethodCallFindResult
                    {
                        Arguments = methodInvocation.ArgumentList.Arguments.Select(a => a.ToString())
                    };

                    var symbolInfo = semanticModel.GetSymbolInfo(methodInvocation).Symbol;

                    if (symbolInfo.Name == methodName && symbolInfo.ContainingType.ToString() == containingType)
                    {
                        findResult.SymbolInfo = symbolInfo;

                        result.Add(findResult);
                    }
                }
            }

            return result;
        }

        private static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(Compilation compilation)
        {
            var stack = new Stack<INamespaceSymbol>();

            stack.Push(compilation.GlobalNamespace);

            while (stack.Count > 0)
            {
                var @namespace = stack.Pop();

                foreach (var member in @namespace.GetMembers())
                {
                    if (member is INamespaceSymbol memberAsNamespace)
                    {
                        stack.Push(memberAsNamespace);
                    }
                    else if (member is INamedTypeSymbol memberAsNamedTypeSymbol)
                    {
                        yield return memberAsNamedTypeSymbol;
                    }
                }
            }
        }
    }
}
