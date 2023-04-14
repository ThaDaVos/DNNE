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

using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Xml.Serialization;

namespace DNNE.Assembly
{

    public struct ExportedMethod
    {
        public ExportType Type { get; init; }
        public string EnclosingTypeName { get; init; }
        public string MethodName { get; init; }
        public string ExportName { get; init; }
        public SignatureCallingConvention CallingConvention { get; init; }
        public PlatformSupport Platforms { get; init; }
        public string ReturnType { get; init; }
        public string RawReturnType { get; init; }
        public string XmlDoc { get; init; }
        public ImmutableList<UsedAttribute> UsedAttributes { get; init; }
        public ImmutableList<ExportedMethodArgument> Arguments { get; init; }
        [XmlIgnore]
        public ImmutableArray<string> ArgumentTypes { get; init; }
        [XmlIgnore]
        public ImmutableArray<string> ArgumentNames { get; init; }
    }
}
