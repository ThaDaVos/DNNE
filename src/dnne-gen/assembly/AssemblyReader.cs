using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DNNE.Assembly.Attributors;
using DNNE.Exceptions;
using DNNE.Languages.C99;

namespace DNNE.Assembly
{
    internal class AssemblyReader : IDisposable
    {
        internal bool isDisposed = false;
        internal readonly ICustomAttributeTypeProvider<KnownType> typeResolver = new TypeResolver();
        internal readonly string assemblyPath;
        internal readonly PEReader peReader;
        internal readonly MetadataReader mdReader;
        internal readonly Scope assemblyScope;
        internal readonly Scope moduleScope;
        internal readonly IDictionary<TypeDefinitionHandle, Scope> typePlatformScenarios = new Dictionary<TypeDefinitionHandle, Scope>();
        internal readonly Dictionary<string, string> loadedXmlDocumentation;

        internal readonly static List<IAttributor> ATTRIBUTORS = new()
        {
            // C99
            new C99DeclCodeAttributor(),
            new C99TypeAttributor(),
            // Clarion
            new ClarionDeclCodeAttributor(),
            new ClarionIncCodeAttributor(),
            new ClarionTypeAttributor(),
            new ClarionReturnTypeAttributor(),
            // XML
            new XMLTypeContractAttributor(),
            // Utility
            new StringMatrixMethodAttributor(),
            new IntegerMatrixMethodAttributor(),
        };

        public AssemblyReader(string validAssemblyPath, string xmlDocFile)
        {
            this.assemblyPath = validAssemblyPath;
            this.peReader = new PEReader(File.OpenRead(this.assemblyPath));
            this.mdReader = this.peReader.GetMetadataReader(MetadataReaderOptions.None);
            this.loadedXmlDocumentation = LoadXmlDocumentation(xmlDocFile);

            // Check for platform scenario attributes
            AssemblyDefinition asmDef = this.mdReader.GetAssemblyDefinition();
            this.assemblyScope = this.GetOSPlatformScope(asmDef.GetCustomAttributes());

            ModuleDefinition modDef = this.mdReader.GetModuleDefinition();
            this.moduleScope = this.GetOSPlatformScope(modDef.GetCustomAttributes());
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.peReader.Dispose();

            this.isDisposed = true;
        }

        internal Dictionary<string, string> LoadXmlDocumentation(string xmlDocumentation)
        {
            var actXml = new Dictionary<string, string>();
            if (xmlDocumentation is null)
                return actXml;

            // See https://docs.microsoft.com/dotnet/csharp/language-reference/xmldoc/
            // for xml documenation definition
            using XmlReader xmlReader = XmlReader.Create(xmlDocumentation);

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "member")
                {
                    string raw_name = xmlReader["name"];
                    actXml[raw_name] = xmlReader.ReadInnerXml();
                }
            }

            return actXml;
        }

        public AssemblyInformation Read()
        {
            List<string> additionalCodeStatements = new List<string>();
            List<ExportedMethod> exportedMethods = new List<ExportedMethod>();
            foreach (MethodDefinitionHandle methodDefHandle in this.mdReader.MethodDefinitions)
            {
                MethodDefinition methodDef = this.mdReader.GetMethodDefinition(handle: methodDefHandle);

                // Only check public static functions
                if (!methodDef.Attributes.HasFlag(flag: MethodAttributes.Public | MethodAttributes.Static))
                {
                    continue;
                }

                List<OSPlatform> supported = new List<OSPlatform>();
                List<OSPlatform> unsupported = new List<OSPlatform>();
                List<UsedAttribute> methodAttributes = new List<UsedAttribute>();
                SignatureCallingConvention callConv = SignatureCallingConvention.Unmanaged;
                ExportType exportAttrType = ExportType.None;
                string managedMethodName = this.mdReader.GetString(handle: methodDef.Name);
                string exportName = managedMethodName;
                // Check for target attribute
                foreach (CustomAttributeHandle customAttrHandle in methodDef.GetCustomAttributes())
                {
                    CustomAttribute customAttr = this.mdReader.GetCustomAttribute(handle: customAttrHandle);
                    ExportType currAttrType = this.GetExportAttributeType(attribute: customAttr);

                    methodAttributes.AddRange(
                        collection: ATTRIBUTORS
                            .Where(predicate: (IAttributor attributor) => attributor.IsApplicable(reader: mdReader, attribute: customAttr))
                            .Select(selector: (IAttributor attributor) => attributor.Parse(reader: mdReader, resolver: this.typeResolver, attribute: customAttr, target: "Method"))
                    );

                    if (currAttrType == ExportType.None)
                    {
                        // Check if method has other supported attributes.
                        if (C99TypeProvider.TryGetC99DeclCodeAttributeValue(reader: this.mdReader, typeResolver: this.typeResolver, attribute: customAttr, c99Decl: out string c99Decl))
                        {
                            additionalCodeStatements.Add(item: c99Decl);
                        }
                        else if (this.TryGetOSPlatformAttributeValue(attribute: customAttr, support: out bool isSupported, platform: out OSPlatform scen))
                        {
                            if (isSupported)
                            {
                                supported.Add(item: scen);
                            }
                            else
                            {
                                unsupported.Add(item: scen);
                            }
                        }

                        continue;
                    }

                    exportAttrType = currAttrType;
                    if (exportAttrType == ExportType.Export)
                    {
                        CustomAttributeValue<KnownType> data = customAttr.DecodeValue(provider: this.typeResolver);
                        if (data.NamedArguments.Length == 1)
                        {
                            exportName = (string)data.NamedArguments[index: 0].Value;
                        }
                    }
                    else
                    {
                        Debug.Assert(condition: exportAttrType == ExportType.UnmanagedCallersOnly);
                        CustomAttributeValue<KnownType> data = customAttr.DecodeValue(provider: this.typeResolver);
                        foreach (CustomAttributeNamedArgument<KnownType> arg in data.NamedArguments)
                        {
                            switch (arg.Type)
                            {
                                case KnownType.I4:
                                case KnownType.CallingConvention:
                                    callConv = (CallingConvention)arg.Value switch
                                    {
                                        CallingConvention.Winapi => SignatureCallingConvention.Unmanaged,
                                        CallingConvention.Cdecl => SignatureCallingConvention.CDecl,
                                        CallingConvention.StdCall => SignatureCallingConvention.StdCall,
                                        CallingConvention.ThisCall => SignatureCallingConvention.ThisCall,
                                        CallingConvention.FastCall => SignatureCallingConvention.FastCall,
                                        _ => throw new NotSupportedException(message: $"Unknown CallingConvention: {arg.Value}")
                                    };
                                    break;

                                case KnownType.SystemTypeArray:
                                    if (arg.Value != null)
                                    {
                                        foreach (CustomAttributeTypedArgument<KnownType> cct in (ImmutableArray<CustomAttributeTypedArgument<KnownType>>)arg.Value)
                                        {
                                            Debug.Assert(condition: cct.Type == KnownType.SystemType);

                                            switch ((KnownType)cct.Value)
                                            {
                                                case KnownType.CallConvCdecl:
                                                    callConv = SignatureCallingConvention.CDecl;
                                                    break;
                                                case KnownType.CallConvStdcall:
                                                    callConv = SignatureCallingConvention.StdCall;
                                                    break;
                                                case KnownType.CallConvThiscall:
                                                    callConv = SignatureCallingConvention.ThisCall;
                                                    break;
                                                case KnownType.CallConvFastcall:
                                                    callConv = SignatureCallingConvention.FastCall;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    break;

                                case KnownType.String:
                                    exportName = (string)arg.Value;
                                    break;

                                default:
                                    throw new GeneratorException(assemblyPath: this.assemblyPath, message: $"Method '{managedMethodName}' has unknown Attribute value type.");
                            }
                        }
                    }
                }

                // Didn't find target attribute. Move onto next method.
                if (exportAttrType == ExportType.None)
                {
                    continue;
                }

                // Extract method details
                TypeDefinition typeDef = this.mdReader.GetTypeDefinition(handle: methodDef.GetDeclaringType());
                string enclosingTypeName = this.ComputeEnclosingTypeName(typeDef: typeDef);

                // Process method signature.
                MethodSignature<string> signature;
                try
                {
                    C99TypeProvider typeProvider = new();

                    signature = methodDef.DecodeSignature(provider: typeProvider, genericContext: null);

                    typeProvider.ThrowIfUnsupportedLastPrimitiveType();
                }
                catch (NotSupportedTypeException nste)
                {
                    throw new GeneratorException(assemblyPath: this.assemblyPath, message: $"Method '{managedMethodName}' has non-exportable type '{nste.Type}'");
                }

                string returnType = signature.ReturnType;
                string[] argumentTypes = signature.ParameterTypes.ToArray();
                string[] argumentNames = new string[signature.ParameterTypes.Length];
                List<ExportedMethodArgument> arguments = new List<ExportedMethodArgument>();

                // Process each parameter.
                foreach (ParameterHandle paramHandle in methodDef.GetParameters())
                {
                    Parameter param = this.mdReader.GetParameter(handle: paramHandle);

                    // Sequence number starts from 1 for arguments.
                    // Number of 0 indicates return value.
                    // Update arg index to be from [0..n-1]
                    // Return index is -1.
                    const int ReturnIndex = -1;
                    int argIndex = param.SequenceNumber - 1;
                    if (argIndex != ReturnIndex)
                    {
                        Debug.Assert(condition: argIndex >= 0);
                        argumentNames[argIndex] = this.mdReader.GetString(handle: param.Name);
                    }

                    // Check custom attributes for additional code.
                    List<UsedAttribute> argumentAttributes = new List<UsedAttribute>();
                    foreach (CustomAttributeHandle attr in param.GetCustomAttributes())
                    {
                        CustomAttribute custAttr = this.mdReader.GetCustomAttribute(handle: attr);

                        if (argIndex == ReturnIndex)
                        {
                            IEnumerable<UsedAttribute> attrs = ATTRIBUTORS
                                    .Where(predicate: (IAttributor attributor) => attributor.IsApplicable(reader: mdReader, attribute: custAttr, isReturn: true))
                                    .Select(selector: (IAttributor attributor) => attributor.Parse(reader: mdReader, resolver: this.typeResolver, attribute: custAttr, target: "Return"));

                            methodAttributes.AddRange(
                                collection: attrs
                            );
                            argumentAttributes.AddRange(
                                collection: attrs
                            );
                        }
                        else
                        {
                            argumentAttributes.AddRange(
                                collection: ATTRIBUTORS
                                    .Where(predicate: (IAttributor attributor) => attributor.IsApplicable(reader: mdReader, attribute: custAttr))
                                    .Select(selector: (IAttributor attributor) => attributor.Parse(reader: mdReader, resolver: this.typeResolver, attribute: custAttr, target: "Argument"))
                            );
                        }

                        if (C99TypeProvider.TryGetC99TypeAttributeValue(reader: this.mdReader, typeResolver: this.typeResolver, attribute: custAttr, c99Type: out string c99Type))
                        {
                            // Overridden type defined.
                            if (argIndex == ReturnIndex)
                            {
                                returnType = c99Type;
                            }
                            else
                            {
                                Debug.Assert(condition: argIndex >= 0);
                                argumentTypes[argIndex] = c99Type;
                            }
                        }
                        else if (C99TypeProvider.TryGetC99DeclCodeAttributeValue(reader: this.mdReader, typeResolver: this.typeResolver, attribute: custAttr, c99Decl: out string c99Decl))
                        {
                            additionalCodeStatements.Add(item: c99Decl);
                        }
                    }

                    arguments.Add(item: new()
                    {
                        Index = argIndex,
                        Name = argIndex != ReturnIndex ? argumentNames[argIndex] : "Return",
                        Type = argIndex != ReturnIndex ? argumentTypes[argIndex] : returnType,
                        Attributes = argumentAttributes.ToImmutableList(),
                        HasDefault = !param.GetDefaultValue().IsNil
                    });
                }

                string xmlDoc = FindXmlDoc(fullMethodName: enclosingTypeName.Replace(oldChar: '+', newChar: '.') + Type.Delimiter + managedMethodName, argumentTypes: argumentTypes);

                exportedMethods.Add(item: new ExportedMethod()
                {
                    Type = exportAttrType,
                    EnclosingTypeName = enclosingTypeName,
                    MethodName = managedMethodName,
                    ExportName = exportName,
                    CallingConvention = callConv,
                    Platforms = new PlatformSupport()
                    {
                        Assembly = this.assemblyScope,
                        Module = this.moduleScope,
                        Type = GetTypeOSPlatformScope(methodDef: methodDef),
                        Method = new Scope()
                        {
                            Support = supported.ToImmutableList(),
                            NoSupport = unsupported.ToImmutableList(),
                        }
                    },
                    ReturnType = returnType,
                    RawReturnType = signature.ReturnType,
                    XmlDoc = xmlDoc,
                    Attributes = methodAttributes.ToImmutableList(),
                    Arguments = arguments.ToImmutableList(),
                    ArgumentTypes = ImmutableArray.Create(items: argumentTypes),
                    ArgumentNames = ImmutableArray.Create(items: argumentNames),
                });
            }

            if (exportedMethods.Count == 0)
            {
                throw new GeneratorException(assemblyPath: this.assemblyPath, message: "Nothing to export.");
            }

            AssemblyDefinition assemblyDefinition = this.mdReader.GetAssemblyDefinition();

            string assemblyName = this.mdReader.GetString(handle: assemblyDefinition.Name);

            return new AssemblyInformation
            {
                Name = assemblyName,
                Path = assemblyPath,
                ExportedTypes = exportedMethods
                    .GroupBy(keySelector: (ExportedMethod exportedMethod) => exportedMethod.EnclosingTypeName)
                    .Select(
                        selector: (IGrouping<string, ExportedMethod> grouping) => new ExportedType()
                        {
                            Name = grouping.Key.Split(separator: '.').Last(),
                            FullName = grouping.Key,
                            ExportedMethods = grouping.ToImmutableList()
                        }
                    )
                    .ToImmutableList(),
                AdditionalStatements = additionalCodeStatements.ToImmutableList(),
            };
        }

        internal bool IsDnneCustomAttribute(MetadataReader reader, CustomAttribute attribute)
        {
            var (namespaceMaybe, _) = ParseCustomAttribute(reader, attribute);

            return namespaceMaybe.HasValue && reader.StringComparer.Equals(namespaceMaybe.Value, "DNNE");
        }

        internal Scope GetTypeOSPlatformScope(MethodDefinition methodDef)
        {
            TypeDefinitionHandle typeDefHandle = methodDef.GetDeclaringType();
            if (this.typePlatformScenarios.TryGetValue(typeDefHandle, out Scope scope))
            {
                return scope;
            }

            TypeDefinition typeDef = this.mdReader.GetTypeDefinition(typeDefHandle);
            var typeScope = this.GetOSPlatformScope(typeDef.GetCustomAttributes());

            // Record and return the scenarios.
            this.typePlatformScenarios.Add(typeDefHandle, typeScope);
            return typeScope;
        }

        internal Scope GetOSPlatformScope(CustomAttributeHandleCollection attrs)
        {
            var supported = new List<OSPlatform>();
            var unsupported = new List<OSPlatform>();
            foreach (var customAttrHandle in attrs)
            {
                CustomAttribute customAttr = this.mdReader.GetCustomAttribute(customAttrHandle);

                if (this.TryGetOSPlatformAttributeValue(customAttr, out bool isSupported, out OSPlatform scen))
                {
                    if (isSupported)
                    {
                        supported.Add(scen);
                    }
                    else
                    {
                        unsupported.Add(scen);
                    }
                }
            }

            return new Scope()
            {
                Support = supported.ToImmutableList(),
                NoSupport = unsupported.ToImmutableList()
            };
        }

        internal bool TryGetOSPlatformAttributeValue(
            CustomAttribute attribute,
            out bool support,
            out OSPlatform platform)
        {
            platform = default;

            support = IsAttributeType(this.mdReader, attribute, "System.Runtime.Versioning", nameof(SupportedOSPlatformAttribute));
            if (!support)
            {
                // If the unsupported attribute exists the "support" value is properly set.
                bool nosupport = IsAttributeType(this.mdReader, attribute, "System.Runtime.Versioning", nameof(UnsupportedOSPlatformAttribute));
                if (!nosupport)
                {
                    return false;
                }
            }

            string value = GetFirstFixedArgAsStringValue(this.typeResolver, attribute);
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            const string platformPrefix = "DNNE_";
            if (value.Contains(nameof(OSPlatform.Windows), StringComparison.OrdinalIgnoreCase))
            {
                platform = OSPlatform.Create($"{platformPrefix}{OSPlatform.Windows}");
            }
            else if (value.Contains(nameof(OSPlatform.OSX), StringComparison.OrdinalIgnoreCase))
            {
                platform = OSPlatform.Create($"{platformPrefix}{OSPlatform.OSX}");
            }
            else if (value.Contains(nameof(OSPlatform.Linux), StringComparison.OrdinalIgnoreCase))
            {
                platform = OSPlatform.Create($"{platformPrefix}{OSPlatform.Linux}");
            }
            else if (value.Contains(nameof(OSPlatform.FreeBSD), StringComparison.OrdinalIgnoreCase))
            {
                platform = OSPlatform.Create($"{platformPrefix}{OSPlatform.FreeBSD}");
            }
            else
            {
                platform = OSPlatform.Create(value);
            }

            return true;
        }

        internal string FindXmlDoc(string fullMethodName, string[] argumentTypes)
        {
            string xmlDoc = "";
            foreach (var item in loadedXmlDocumentation)
            {
                if (item.Key.StartsWith("M:" + fullMethodName))
                {
                    xmlDoc = item.Value;
                    break;
                }
            }
            if (xmlDoc == "")
                return "";

            var lines = xmlDoc.TrimStart('\n').TrimEnd().Split("\n");
            string prefix = "/// ";
            var result = lines
             .Select(x => prefix + x.Trim())
             .ToList();

            return Environment.NewLine + string.Join(Environment.NewLine, result);
        }

        internal string ComputeEnclosingTypeName(TypeDefinition typeDef)
        {
            var enclosingTypes = new List<string>() { this.mdReader.GetString(typeDef.Name) };
            TypeDefinition parentTypeDef = typeDef;
            while (parentTypeDef.IsNested)
            {
                parentTypeDef = this.mdReader.GetTypeDefinition(parentTypeDef.GetDeclaringType());
                enclosingTypes.Add(this.mdReader.GetString(parentTypeDef.Name));
            }

            enclosingTypes.Reverse();
            string name = string.Join('+', enclosingTypes);
            if (!parentTypeDef.Namespace.IsNil)
            {
                name = $"{this.mdReader.GetString(parentTypeDef.Namespace)}{Type.Delimiter}{name}";
            }

            return name;
        }

        internal ExportType GetExportAttributeType(CustomAttribute attribute)
        {
            if (IsAttributeType(this.mdReader, attribute, "DNNE", "ExportAttribute"))
            {
                return ExportType.Export;
            }
            else if (IsAttributeType(this.mdReader, attribute, "System.Runtime.InteropServices", nameof(UnmanagedCallersOnlyAttribute)))
            {
                return ExportType.UnmanagedCallersOnly;
            }
            else
            {
                return ExportType.None;
            }
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

        internal static bool IsAttributeType(MetadataReader reader, CustomAttribute attribute, string targetNamespace, string targetName)
        {
            var (namespaceMaybe, nameMaybe) = ParseCustomAttribute(reader, attribute);

            if (namespaceMaybe == null || nameMaybe == null)
            {
                return false;
            }

#if DEBUG
            string attrNamespace = reader.GetString(namespaceMaybe.Value);
            string attrName = reader.GetString(nameMaybe.Value);
#endif
            return reader.StringComparer.Equals(namespaceMaybe.Value, targetNamespace)
                && reader.StringComparer.Equals(nameMaybe.Value, targetName);
        }

        internal static (StringHandle? parsedNamespace, StringHandle? parsedName) ParseCustomAttribute(MetadataReader reader, CustomAttribute attribute)
        {
            switch (attribute.Constructor.Kind)
            {
                case HandleKind.MemberReference:
                    MemberReference refConstructor = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);

                    if (refConstructor.Parent.IsNil)
                    {
                        Debug.Assert(false, "Unknown parent reference kind");
                        return (null, null);
                    }

                    switch (refConstructor.Parent.Kind)
                    {
                        case HandleKind.TypeReference:
                            TypeReference refType = reader.GetTypeReference((TypeReferenceHandle)refConstructor.Parent);
                            return (refType.Namespace, refType.Name);
                        default:
                            Debug.Assert(false, "Unknown parent reference kind");
                            return (null, null);
                    }
                case HandleKind.MethodDefinition:
                    MethodDefinition defConstructor = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
                    TypeDefinition defType = reader.GetTypeDefinition(defConstructor.GetDeclaringType());
                    return (defType.Namespace, defType.Name);

                default:
                    Debug.Assert(false, "Unknown attribute constructor kind");
                    return (null, null);
            }
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
                if (!string.IsNullOrEmpty(pre_support)
                    || !string.IsNullOrEmpty(pre_nosupport))
                {
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
    }
}