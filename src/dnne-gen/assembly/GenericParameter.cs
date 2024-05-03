using System.Reflection;

namespace DNNE.Assembly;

public struct GenericParameter
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required GenericParameterAttributes Attributes { get; init; }
}
