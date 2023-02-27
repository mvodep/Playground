using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace ArchitectureAnalyzer
{
    internal static class SemanticModelHelper
    {
        public static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(Compilation compilation)
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