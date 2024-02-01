using System.Runtime.Serialization;

namespace DNNE.Assembly.Old;

[DataContract]
public struct AttributeArgument
{
    [DataMember]
    public KnownType Type { get; init; }
    [DataMember]
    public object Value { get; init; }
}
