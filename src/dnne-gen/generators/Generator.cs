// Copyright 2020 Aaron R Robinson
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.IO;
using System.Linq;
using DNNE.Assembly.Old;
using DNNE.Exceptions;

namespace DNNE.Generators
{
    internal abstract class Generator : IGenerator
    {
        protected AssemblyInformation assemblyInformation;

        internal Generator(AssemblyInformation assemblyInformation)
        {
            this.assemblyInformation = assemblyInformation;
        }

        public void Emit(string outputFile)
        {
            outputFile = ParseOutPutFileName(outputFile);

            // Check if the file exists
            if (File.Exists(outputFile) == true)
            {
                File.Delete(outputFile);
            }

            string directory = Path.GetDirectoryName(outputFile);

            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            Emit(File.OpenWrite(outputFile));
        }

        public virtual string ParseOutPutFileName(string outputFile)
        {
            return outputFile;
        }

        protected virtual string ResolveClassName(ExportedType enclosingType)
        {
            return enclosingType.Name.Split('.').Last();
        }

        public void Emit(Stream outputStream)
        {
            if (!this.assemblyInformation.ExportedTypes.Any())
            {
                throw new GeneratorException(this.assemblyInformation.Path, "Nothing to export.");
            }

            Write(outputStream);
        }

        protected abstract void Write(Stream outputStream);
    }
}
