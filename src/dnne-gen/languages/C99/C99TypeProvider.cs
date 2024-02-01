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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DNNE.Assembly.Old;
using DNNE.Exceptions;

namespace DNNE.Languages.C99
{
    internal class C99TypeProvider : ISignatureTypeProvider<string, UnusedGenericContext>
    {
        PrimitiveTypeCode? lastUnsupportedPrimitiveType;

        public string GetArrayType(string elementType, ArrayShape shape)
        {
            throw new NotSupportedTypeException(elementType);
        }

        public string GetByReferenceType(string elementType)
        {
            throw new NotSupportedTypeException(elementType);
        }

        public string GetFunctionPointerType(MethodSignature<string> signature)
        {
            // Define the native function pointer type in a comment.
            string args = this.GetPrimitiveType(PrimitiveTypeCode.Void);
            if (signature.ParameterTypes.Length != 0)
            {
                var argsBuffer = new StringBuilder();
                var delim = "";
                foreach (var type in signature.ParameterTypes)
                {
                    argsBuffer.Append(delim);
                    argsBuffer.Append(type);
                    delim = ", ";
                }

                args = argsBuffer.ToString();
            }

            var callingConvention = GetC99CallConv(signature.Header.CallingConvention);
            var typeComment = $"/* {signature.ReturnType}({callingConvention} *)({args}) */ ";
            return typeComment + this.GetPrimitiveType(PrimitiveTypeCode.IntPtr);
        }

        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
        {
            throw new NotSupportedTypeException($"Generic - {genericType}");
        }

        public string GetGenericMethodParameter(UnusedGenericContext genericContext, int index)
        {
            throw new NotSupportedTypeException($"Generic - {index}");
        }

        public string GetGenericTypeParameter(UnusedGenericContext genericContext, int index)
        {
            throw new NotSupportedTypeException($"Generic - {index}");
        }

        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired)
        {
            throw new NotSupportedTypeException($"{modifier} {unmodifiedType}");
        }

        public string GetPinnedType(string elementType)
        {
            throw new NotSupportedTypeException($"Pinned - {elementType}");
        }

        public string GetPointerType(string elementType)
        {
            this.lastUnsupportedPrimitiveType = null;
            return elementType + "*";
        }

        public string GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            ThrowIfUnsupportedLastPrimitiveType();

            if (typeCode == PrimitiveTypeCode.Char)
            {
                // Record the current type here with the expectation
                // it will be of pointer type to Char, which is supported.
                this.lastUnsupportedPrimitiveType = typeCode;
                return "DNNE_WCHAR";
            }

            return typeCode switch
            {
                PrimitiveTypeCode.SByte => "int8_t",
                PrimitiveTypeCode.Byte => "uint8_t",
                PrimitiveTypeCode.Int16 => "int16_t",
                PrimitiveTypeCode.UInt16 => "uint16_t",
                PrimitiveTypeCode.Int32 => "int32_t",
                PrimitiveTypeCode.UInt32 => "uint32_t",
                PrimitiveTypeCode.Int64 => "int64_t",
                PrimitiveTypeCode.UInt64 => "uint64_t",
                PrimitiveTypeCode.IntPtr => "intptr_t",
                PrimitiveTypeCode.UIntPtr => "uintptr_t",
                PrimitiveTypeCode.Single => "float",
                PrimitiveTypeCode.Double => "double",
                PrimitiveTypeCode.Void => "void",
                _ => throw new NotSupportedTypeException(typeCode.ToString())
            };
        }

        public void ThrowIfUnsupportedLastPrimitiveType()
        {
            if (this.lastUnsupportedPrimitiveType.HasValue)
            {
                throw new NotSupportedTypeException(this.lastUnsupportedPrimitiveType.Value.ToString());
            }
        }

        public string GetSZArrayType(string elementType)
        {
            throw new NotSupportedTypeException($"Array - {elementType}");
        }

        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            return SupportNonPrimitiveTypes(rawTypeKind);
        }

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            return SupportNonPrimitiveTypes(rawTypeKind);
        }

        public string GetTypeFromSpecification(MetadataReader reader, UnusedGenericContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            return SupportNonPrimitiveTypes(rawTypeKind);
        }

        internal static bool TryGetC99TypeAttributeValue(MetadataReader reader, ICustomAttributeTypeProvider<KnownType> typeResolver, CustomAttribute attribute, out string c99Type)
        {
            c99Type = IsAttributeType(reader, attribute, "DNNE", "C99TypeAttribute")
                ? GetFirstFixedArgAsStringValue(typeResolver, attribute)
                : null;
            return !string.IsNullOrEmpty(c99Type);
        }

        internal static bool TryGetC99DeclCodeAttributeValue(MetadataReader reader, ICustomAttributeTypeProvider<KnownType> typeResolver, CustomAttribute attribute, out string c99Decl)
        {
            c99Decl = IsAttributeType(reader, attribute, "DNNE", "C99DeclCodeAttribute")
                ? GetFirstFixedArgAsStringValue(typeResolver, attribute)
                : null;
            return !string.IsNullOrEmpty(c99Decl);
        }

        internal static string GetFirstFixedArgAsStringValue(ICustomAttributeTypeProvider<KnownType> typeResolver, CustomAttribute attribute)
        {
            CustomAttributeValue<KnownType> data = attribute.DecodeValue(typeResolver);
            if (data.FixedArguments.Length == 1)
            {
                return (string)data.FixedArguments[0].Value;
            }

            return null;
        }

        private static bool IsAttributeType(MetadataReader reader, CustomAttribute attribute, string targetNamespace, string targetName)
        {
            StringHandle namespaceMaybe;
            StringHandle nameMaybe;
            switch (attribute.Constructor.Kind)
            {
                case HandleKind.MemberReference:
                    MemberReference refConstructor = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                    TypeReference refType = reader.GetTypeReference((TypeReferenceHandle)refConstructor.Parent);
                    namespaceMaybe = refType.Namespace;
                    nameMaybe = refType.Name;
                    break;

                case HandleKind.MethodDefinition:
                    MethodDefinition defConstructor = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
                    TypeDefinition defType = reader.GetTypeDefinition(defConstructor.GetDeclaringType());
                    namespaceMaybe = defType.Namespace;
                    nameMaybe = defType.Name;
                    break;

                default:
                    Debug.Assert(false, "Unknown attribute constructor kind");
                    return false;
            }

#if DEBUG
            string attrNamespace = reader.GetString(namespaceMaybe);
            string attrName = reader.GetString(nameMaybe);
#endif
            return reader.StringComparer.Equals(namespaceMaybe, targetNamespace) && reader.StringComparer.Equals(nameMaybe, targetName);
        }


        internal static string GetC99CallConv(SignatureCallingConvention callConv)
        {
            return callConv switch
            {
                SignatureCallingConvention.CDecl => "DNNE_CALLTYPE_CDECL",
                SignatureCallingConvention.StdCall => "DNNE_CALLTYPE_STDCALL",
                SignatureCallingConvention.ThisCall => "DNNE_CALLTYPE_THISCALL",
                SignatureCallingConvention.FastCall => "DNNE_CALLTYPE_FASTCALL",
                SignatureCallingConvention.Unmanaged => "DNNE_CALLTYPE",
                _ => throw new NotSupportedException($"Unknown CallingConvention: {callConv}"),
            };
        }

        internal static (string preguard, string postguard) GetC99PlatformGuards(in PlatformSupport platformSupport)
        {
            var pre = new StringBuilder();
            var post = new StringBuilder();

            var postAssembly = ConvertScope(platformSupport.Assembly, ref pre);
            var postModule = ConvertScope(platformSupport.Module, ref pre);
            var postType = ConvertScope(platformSupport.Type, ref pre);
            var postMethod = ConvertScope(platformSupport.Method, ref pre);

            // Append the post guards in reverse order
            post.Append(postMethod);
            post.Append(postType);
            post.Append(postModule);
            post.Append(postAssembly);

            return (pre.ToString(), post.ToString());

            static string ConvertScope(in Scope scope, ref StringBuilder pre)
            {
                (string pre_support, string post_support) = ConvertCollection(scope.Support, "(", ")");
                (string pre_nosupport, string post_nosupport) = ConvertCollection(scope.NoSupport, "!(", ")");

                var post = new StringBuilder();
                if (
                    !string.IsNullOrEmpty(pre_support) ||
                    !string.IsNullOrEmpty(pre_nosupport)
                ) {
                    // Add the preamble for the guard
                    pre.Append("#if ");
                    post.Append("#endif // ");

                    // Append the "support" clauses because if they don't exist they are string.Empty
                    pre.Append(pre_support);
                    post.Append(post_support);

                    // Check if we need to chain the clauses
                    if (!string.IsNullOrEmpty(pre_support) && !string.IsNullOrEmpty(pre_nosupport))
                    {
                        pre.Append(" && ");
                        post.Append(" && ");
                    }

                    // Append the "nosupport" clauses because if they don't exist they are string.Empty
                    pre.Append($"{pre_nosupport}");
                    post.Append($"{post_nosupport}");

                    pre.Append('\n');
                    post.Append('\n');
                }

                return post.ToString();
            }

            static (string pre, string post) ConvertCollection(in IEnumerable<OSPlatform> platforms, in string prefix, in string suffix)
            {
                var pre = new StringBuilder();
                var post = new StringBuilder();

                var delim = prefix;
                foreach (OSPlatform os in platforms)
                {
                    if (pre.Length != 0)
                    {
                        delim = " || ";
                    }

                    var platformMacroSafe = Regex.Replace(os.ToString(), Program.SafeMacroRegEx, "_").ToUpperInvariant();
                    pre.Append($"{delim}defined({platformMacroSafe})");
                    post.Append($"{post}{delim}{platformMacroSafe}");
                }

                if (pre.Length != 0)
                {
                    pre.Append(suffix);
                    post.Append(suffix);
                }

                return (pre.ToString(), post.ToString());
            }
        }

        private static string SupportNonPrimitiveTypes(byte rawTypeKind)
        {
            // See https://docs.microsoft.com/dotnet/framework/unmanaged-api/metadata/corelementtype-enumeration
            const byte ELEMENT_TYPE_VALUETYPE = 0x11;
            if (rawTypeKind == ELEMENT_TYPE_VALUETYPE)
            {
                return "/* SUPPLY TYPE */";
            }

            throw new NotSupportedTypeException("Non-primitive");
        }
    }
}
