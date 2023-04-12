using System;

namespace DNNE.Exceptions
{
    internal class GeneratorException : Exception
    {
        public string AssemblyPath { get; private set; }

        public GeneratorException(string assemblyPath, string message)
            : base(message)
        {
            this.AssemblyPath = assemblyPath;
        }
    }
}