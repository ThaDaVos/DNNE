using System.Runtime.CompilerServices;

namespace DNNE.SourceGeneratorTests;

public partial class Test
{
    [DNNE.Wrappings.InstancedUnManaged.InstancedUnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public int TestInstanceToUnManaged(int a, int @event)
    {
        return 0;
    }

    [DNNE.Wrappings.InstancedUnManaged.InstancedUnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public async Task<int> TestInstanceToUnManagedAsync(int a, int @event)
    {
        return 0;
    }

    [DNNE.Wrappings.AsyncToUnManaged.AsyncUnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public async static Task<int> TestAsyncToUnManaged(int a, int @event)
    {
        return 0;
    }
    
    [DNNE.StringMatrixMethod("paramA", ["a", "b", "c"])]
    public static int TestStringMatrixMethod(string paramA, int otherParam)
    {
        return 0;
    }

    [DNNE.StringMatrixMethod("paramA", ["a", "b", "c"])]
    public int TestStringMatrixMethodFromInstanced(string paramA, int otherParam)
    {
        return 0;
    }
    
    [DNNE.IntegerMatrixMethod("paramA", [1, 2, 3])]
    public static int TestIntegerMatrixMethod(int paramA, int otherParam)
    {
        return 0;
    }

    [DNNE.IntegerMatrixMethod("paramA", [1, 2, 3])]
    public int TestIntegerMatrixMethodFromInstanced(int paramA, int otherParam)
    {
        return 0;
    }
    
    [DNNE.StringMatrixMethod("paramA", ["a", "b", "c"])]
    [DNNE.IntegerMatrixMethod("paramB", [1, 2, 3])]
    public static int TestMultipleMatrixMethod(string paramA, int paramB, int otherParam)
    {
        return 0;
    }

    [DNNE.StringMatrixMethod("paramA", ["a", "b", "c"])]
    [DNNE.IntegerMatrixMethod("paramB", [1, 2, 3])]
    public int TestMultipleMatrixMethodFromInstanced(string paramA, int paramB, int otherParam)
    {
        return 0;
    }
}
