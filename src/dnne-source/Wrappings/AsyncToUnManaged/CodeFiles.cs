using System;

namespace DNNE.Source.Wrappings.AsyncToUnManaged;

internal class CodeFiles
{
    internal const string ATTRIBUTE_ASYNC_UNMANAGED_CALLERS_ONLY_ATTRIBUTE = @"
#nullable enable
using System;
namespace DNNE.Wrappings.AsyncToUnManaged;
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[System.Diagnostics.Conditional(""AsyncToUnManagedGenerator_DEBUG"")]
public sealed class AsyncUnmanagedCallersOnlyAttribute : Attribute
{
    //
    // Summary:
    //     Optional. If omitted, the runtime will use the default platform calling convention.
    public Type[]? CallConvs;
    //
    // Summary:
    //     Optional. If omitted, no named export is emitted during compilation.
    public string? EntryPoint;


    public AsyncUnmanagedCallersOnlyAttribute()
    {
    }
}
";
}
