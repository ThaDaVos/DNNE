namespace DNNE.Source.Attributes;

internal class CodeFiles
{
    internal const string ATTRIBUTE_DeclCode = @"
#nullable enable
using System;
namespace DNNE;
/// <summary>
/// Provide {%language%} code to be defined early in the generated {%language%} file.
/// </summary>
/// <remarks>
/// This attribute is respected on an exported method declaration or on a parameter for the method.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = true)]
internal class {%language%}DeclCodeAttribute : Attribute
{
    public {%language%}DeclCodeAttribute(string code) { }
}";
    internal const string ATTRIBUTE_TypeCode = @"
#nullable enable
using System;
namespace DNNE;
/// <summary>
/// Define the {%language%} type to be used.
/// </summary>
/// <remarks>
/// The level of indirection should be included in the supplied string.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = true)]
internal class {%language%}TypeAttribute : Attribute
{
    public {%language%}TypeAttribute(string code) { }
}";
    internal const string ATTRIBUTE_TypeContractCode = @"
#nullable enable
using System;
namespace DNNE;
/// <summary>
/// Define the {%language%} type to be serialized.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = true)]
internal class {%language%}TypeContractAttribute : Attribute
{

    public {%language%}TypeContractAttribute(Type? nestedIn = null){ }
}";
    internal const string ATTRIBUTE_IncludeCode = @"
#nullable enable
using System;
namespace DNNE;
/// <summary>
/// Provide {%language%} code to be defined early in the generated {%language%} include file.
/// </summary>
/// <remarks>
/// This attribute is respected on an exported method declaration or on a parameter for the method.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = true)]
internal class {%language%}IncCodeAttribute : Attribute
{
    public {%language%}IncCodeAttribute(string code) { }
}";
    internal const string ATTRIBUTE_MatrixMethodCode = @"
#nullable enable
using System;
namespace DNNE;
/// <summary>
/// Generates multiple methods based on the defined matrix and {%type%} parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class {%language%}MatrixMethodAttribute : Attribute
{
    public string? ConnectBy { get; set; }
    public {%language%}MatrixMethodAttribute(string parameter, params {%type%}[] values) { }
}";
}
