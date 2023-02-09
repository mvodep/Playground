using System;

namespace ArchitectureAnalyzer
{
    internal class AssertException : Exception
    {
        public AssertException(string message) : base(message) { }
    }
}
