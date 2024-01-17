using System;

namespace DNNE.Source.Wrappings.InstancedUnManaged;

internal class CodeFiles
{
    internal const string ATTRIBUTE_INSTANCED_UNMANAGED_CALLERS_ONLY_ATTRIBUTE = @"
#nullable enable
using System;
namespace DNNE.Wrappings.InstancedUnManaged;
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[System.Diagnostics.Conditional(""InstancedUnManagedGenerator_DEBUG"")]
public sealed class InstancedUnmanagedCallersOnlyAttribute : Attribute
{
    //
    // Summary:
    //     Optional. If omitted, the runtime will use the default platform calling convention.
    public Type[]? CallConvs;
    //
    // Summary:
    //     Optional. If omitted, no named export is emitted during compilation.
    public string? EntryPoint;


    public InstancedUnmanagedCallersOnlyAttribute()
    {
    }
}
";
}
