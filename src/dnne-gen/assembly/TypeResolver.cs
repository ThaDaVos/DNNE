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

using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Win32;

namespace DNNE.Assembly
{
    public class TypeResolver : ICustomAttributeTypeProvider<KnownType>
    {
        public KnownType GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            return typeCode switch
            {
                PrimitiveTypeCode.Int32 => KnownType.I4,
                PrimitiveTypeCode.String => KnownType.String,
                _ => KnownType.Unknown
            };
        }

        public KnownType GetSystemType()
        {
            return KnownType.SystemType;
        }

        public KnownType GetSZArrayType(KnownType elementType)
        {
            return elementType switch
            {
                KnownType.SystemType => KnownType.SystemTypeArray,
                _ => throw new BadImageFormatException("Unexpectedly got an array of unsupported type.")
            };
        }

        public KnownType GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            SignatureTypeKind signatureTypeKind = reader.ResolveSignatureTypeKind(handle, rawTypeKind);
            var definition = reader.GetTypeDefinition(handle);

            if (definition.Name.IsNil == false) {
                return DecodeTypeReferenceName(reader.GetString(definition.Name), signatureTypeKind);
            }

            return DecodeSignatureTypeKind(signatureTypeKind);
        }

        public KnownType GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            SignatureTypeKind signatureTypeKind = reader.ResolveSignatureTypeKind(handle, rawTypeKind);
            var reference = reader.GetTypeReference(handle);

            if (reference.Name.IsNil == false)
            {
                return DecodeTypeReferenceName(reader.GetString(reference.Name), signatureTypeKind);
            }

            return DecodeSignatureTypeKind(signatureTypeKind);
        }

        private KnownType DecodeTypeReferenceName(string typeName, SignatureTypeKind kind)
        {
            return typeName switch
            {
                "Type" => KnownType.Type,
                _ => DecodeSignatureTypeKind(kind)
            };
        }

        private KnownType DecodeSignatureTypeKind(SignatureTypeKind kind)
        {
            return kind switch
            {
                SignatureTypeKind.Class => KnownType.Class,
                SignatureTypeKind.ValueType => KnownType.ValueType,
                SignatureTypeKind.Unknown => KnownType.Unknown,
                _ => throw new NotImplementedException(),
            };
        }

        public KnownType GetTypeFromSerializedName(string name)
        {
            int typeAssemblySeparator = name.IndexOf(',');
            string typeName = name[..typeAssemblySeparator];
            string assemblyName = name[(typeAssemblySeparator + 1)..];
            string assemblySimpleName = assemblyName;
            int simpleNameEnd = assemblySimpleName.IndexOf(',');
            if (simpleNameEnd != -1)
            {
                assemblySimpleName = assemblySimpleName[..simpleNameEnd];
            }

            return (typeName, assemblySimpleName.TrimStart()) switch
            {
                ("System.Runtime.InteropServices.CallingConvention", "System.Runtime.InteropServices") => KnownType.CallingConvention,
                ("System.Runtime.CompilerServices.CallConvCdecl", "System.Runtime") => KnownType.CallConvCdecl,
                ("System.Runtime.CompilerServices.CallConvStdcall", "System.Runtime") => KnownType.CallConvStdcall,
                ("System.Runtime.CompilerServices.CallConvThiscall", "System.Runtime") => KnownType.CallConvThiscall,
                ("System.Runtime.CompilerServices.CallConvFastcall", "System.Runtime") => KnownType.CallConvFastcall,
                _ => KnownType.Unknown
            };
        }

        public PrimitiveTypeCode GetUnderlyingEnumType(KnownType type)
        {
            return type switch
            {
                KnownType.CallingConvention => PrimitiveTypeCode.Int32,
                KnownType.I4 => PrimitiveTypeCode.Int32,
                KnownType.String => PrimitiveTypeCode.String,
                KnownType.Type => PrimitiveTypeCode.String,
                _ => throw new BadImageFormatException("Unexpectedly got an enum parameter for an attribute.")
            };
        }

        public bool IsSystemType(KnownType type)
        {
            return type == KnownType.SystemType;
        }
    }
}
