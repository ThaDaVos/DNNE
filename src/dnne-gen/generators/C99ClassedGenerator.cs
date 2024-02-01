﻿// Copyright 2020 Aaron R Robinson
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DNNE.Assembly.Old;
using DNNE.Languages.C99;

namespace DNNE.Generators
{
    internal class C99ClassedGenerator : Generator
    {
        internal C99ClassedGenerator(AssemblyInformation assemblyInformation) : base(assemblyInformation)
        {
        }

        protected override void Write(Stream outputStream)
        {
            using var writer = new StreamWriter(outputStream);
            // Convert the assembly name into a supported string for C99 macros.
            var assemblyNameMacroSafe = Regex.Replace(this.assemblyInformation.Name, Program.SafeMacroRegEx, "_");
            var generatedHeaderDefine = $"__DNNE_GENERATED_HEADER_{assemblyNameMacroSafe.ToUpperInvariant()}__";
            var compileAsSourceDefine = "DNNE_COMPILE_AS_SOURCE";

            // Emit declaration preamble
            writer.WriteLine(
$@"//
// Auto-generated by dnne-gen
//
// .NET Assembly: {this.assemblyInformation.Name}
//

//
// Declare exported functions
//
#ifndef {generatedHeaderDefine}
#define {generatedHeaderDefine}

#include <stddef.h>
#include <stdint.h>
#ifdef {compileAsSourceDefine}
    #include <dnne.h>
#else
    // When used as a header file, the assumption is
    // dnne.h will be next to this file.
    #include ""dnne.h""
#endif // !{compileAsSourceDefine}
");

            // Emit additional code statements
            if (this.assemblyInformation.AdditionalStatements.Any())
            {
                writer.WriteLine(
$@"//
// Additional code provided by user
//");
                foreach (var stmt in this.assemblyInformation.AdditionalStatements)
                {
                    writer.WriteLine(stmt);
                }

                writer.WriteLine();
            }

            var implStream = new StringWriter();

            // Emit definition preamble
            implStream.WriteLine(
$@"//
// Define exported functions
//
#ifdef {compileAsSourceDefine}

#ifdef DNNE_WINDOWS
    #ifdef _WCHAR_T_DEFINED
        typedef wchar_t char_t;
    #else
        typedef unsigned short char_t;
    #endif
#else
    typedef char char_t;
#endif

//
// Forward declarations
//

extern void* get_callable_managed_function(
    const char_t* dotnet_type,
    const char_t* dotnet_type_method,
    const char_t* dotnet_delegate_type);

extern void* get_fast_callable_managed_function(
    const char_t* dotnet_type,
    const char_t* dotnet_type_method);
");

            // Emit string table
            implStream.WriteLine(
@"//
// String constants
//
");
            int count = 1;

            foreach (var enclosingType in this.assemblyInformation.ExportedTypes)
            {
                string classNameConstant = $"t{count++}_name";

                implStream.WriteLine(
$@"#ifdef DNNE_TARGET_NET_FRAMEWORK
    static const char_t* {classNameConstant} = DNNE_STR(""{enclosingType.FullName}"");
#else
    static const char_t* {classNameConstant} = DNNE_STR(""{enclosingType.FullName}, {this.assemblyInformation.Name}"");
#endif // !DNNE_TARGET_NET_FRAMEWORK
");

                // Emit the exports
                implStream.WriteLine(
    @$"
//
// Exports for {enclosingType.Name}
//
");
                foreach (var export in enclosingType.ExportedMethods)
                {
                    (var preguard, var postguard) = C99TypeProvider.GetC99PlatformGuards(export.Platforms);

                    // Create declaration and call signature.
                    string delim = "";
                    var declsig = new StringBuilder();
                    var callsig = new StringBuilder();
                    for (int i = 0; i < export.ArgumentTypes.Length; ++i)
                    {
                        var argName = export.ArgumentNames[i] ?? $"arg{i}";
                        declsig.AppendFormat("{0}{1} {2}", delim, export.ArgumentTypes[i], argName);
                        callsig.AppendFormat("{0}{1}", delim, argName);
                        delim = ", ";
                    }

                    // Special casing for void signature.
                    if (declsig.Length == 0)
                    {
                        declsig.Append("void");
                    }

                    // Special casing for void return.
                    string returnStatementKeyword = "return ";
                    if (export.ReturnType.Equals("void"))
                    {
                        returnStatementKeyword = string.Empty;
                    }

                    string callConv = C99TypeProvider.GetC99CallConv(export.CallingConvention);

                    Debug.Assert(!string.IsNullOrEmpty(classNameConstant));

                    // Generate the acquire managed function based on the export type.
                    string acquireManagedFunction;
                    if (export.Type == ExportType.Export)
                    {
                        acquireManagedFunction =
    $@"const char_t* methodName = DNNE_STR(""{export.MethodName}"");
        const char_t* delegateType = DNNE_STR(""{enclosingType.Name}+{export.MethodName}Delegate, {this.assemblyInformation.Name}"");
        {enclosingType.Name}_{export.ExportName}_ptr = ({export.ReturnType}({callConv}*)({declsig}))get_callable_managed_function({classNameConstant}, methodName, delegateType);";

                    }
                    else
                    {
                        Debug.Assert(export.Type == ExportType.UnmanagedCallersOnly);
                        acquireManagedFunction =
    $@"const char_t* methodName = DNNE_STR(""{export.MethodName}"");
        {enclosingType.Name}_{export.ExportName}_ptr = ({export.ReturnType}({callConv}*)({declsig}))get_fast_callable_managed_function({classNameConstant}, methodName);";
                    }

                    // Declare export
                    writer.WriteLine(
    $@"{preguard}// Computed from {enclosingType.FullName}{Type.Delimiter}{export.MethodName}{export.XmlDoc}
DNNE_EXTERN_C DNNE_API {export.ReturnType} {callConv} {enclosingType.Name}_{export.ExportName}({declsig});
{postguard}");

                    // Define export in implementation stream
                    implStream.WriteLine(
    $@"{preguard}// Computed from {enclosingType.FullName}{Type.Delimiter}{export.MethodName}
static {export.ReturnType} ({callConv}* {enclosingType.Name}_{export.ExportName}_ptr)({declsig});
DNNE_EXTERN_C DNNE_API {export.ReturnType} {callConv} {enclosingType.Name}_{export.ExportName}({declsig})
{{
    if ({enclosingType.Name}_{export.ExportName}_ptr == NULL)
    {{
        {acquireManagedFunction}
    }}
    {returnStatementKeyword}{enclosingType.Name}_{export.ExportName}_ptr({callsig});
}}
{postguard}");
                }
            }

            // Emit implementation closing
            implStream.Write($"#endif // {compileAsSourceDefine}");

            // Emit output closing for header and merge in implementation
            writer.WriteLine(
$@"#endif // {generatedHeaderDefine}

{implStream}");
        }
    }
}
