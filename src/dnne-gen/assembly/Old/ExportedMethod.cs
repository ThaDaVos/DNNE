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
using System.Runtime.Serialization;

namespace DNNE.Assembly.Old
{
    [DataContract]
    public struct ExportedMethod
    {
        [DataMember]
        public ExportType Type { get; init; }
        public string EnclosingTypeName { get; init; }
        [DataMember]
        public string MethodName { get; init; }
        [DataMember]
        public string ExportName { get; init; }
        [DataMember]
        public SignatureCallingConvention CallingConvention { get; init; }
        [DataMember]
        public PlatformSupport Platforms { get; init; }
        [DataMember]
        public string ReturnType { get; init; }
        [DataMember]
        public string RawReturnType { get; init; }
        [DataMember]
        public string XmlDoc { get; init; }
        [DataMember]
        public ImmutableList<UsedAttribute> Attributes { get; init; }
        [DataMember]
        public ImmutableList<ExportedMethodArgument> Arguments { get; init; }
        [DataMember]
        public ImmutableArray<string> ArgumentTypes { get; init; }
        [DataMember]
        public ImmutableArray<string> ArgumentNames { get; init; }
    }
}
