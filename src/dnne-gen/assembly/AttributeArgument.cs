using System.Runtime.Serialization;

namespace DNNE.Assembly;

[DataContract]
public struct AttributeArgument
{
    [DataMember]
    public KnownType Type { get; init; }
    [DataMember]
    public object Value { get; init; }
}
