using System;

namespace DNNE.Source;

internal class CodeFiles
{
    internal const string ATTRIBUTE_ASYNC_UNMANAGED_CALLERS_ONLY_ATTRIBUTE = @"
#nullable enable
using System;
namespace AsyncToUnManaged;
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

    internal const string CLASS_BRIDGING_CONTEXT = @"
#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
namespace AsyncToUnManaged;
/// <summary>
/// PumpingSyncContext, based on AsyncPump
/// http://blogs.msdn.com/b/pfxteam/archive/2012/02/02/await-synchronizationcontext-and-console-apps-part-3.aspx
/// </summary>
public sealed class BridgingContext : SynchronizationContext, IDisposable
{
    BlockingCollection<Action>? _actions;
    int _pendingOps = 0;

    public TResult Run<TResult>(Func<Task<TResult>> taskFunc, CancellationToken token = default(CancellationToken))
    {
        _actions = new BlockingCollection<Action>();
        SynchronizationContext.SetSynchronizationContext(this);
        try
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

            var task = Task.Factory.StartNew(
                async () =>
                {
                    OperationStarted();
                    try
                    {
                        return await taskFunc();
                    }
                    finally
                    {
                        OperationCompleted();
                    }
                },
                token, TaskCreationOptions.None, scheduler).Unwrap();

            // pumping loop
            foreach (var action in _actions.GetConsumingEnumerable())
                action();

            return task.GetAwaiter().GetResult();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }
    }

    void Complete()
    {
        _actions?.CompleteAdding();
    }

    // SynchronizationContext methods
    public override SynchronizationContext CreateCopy()
    {
        return this;
    }

    public override void OperationStarted()
    {
        // called when async void method is invoked 
        Interlocked.Increment(ref _pendingOps);
    }

    public override void OperationCompleted()
    {
        // called when async void method completes 
        if (Interlocked.Decrement(ref _pendingOps) == 0)
            Complete();
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        _actions?.Add(() => d(state));
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        throw new NotImplementedException(""Send"");
    }

    public void Dispose()
    {
        _actions?.Dispose();
    }
}
";
}
