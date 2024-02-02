namespace DNNE.Assembly;

public struct UsedAttributeArgument<TType>
{
    public TType Type { get; init; }
    public object? Value { get; init; }
}
