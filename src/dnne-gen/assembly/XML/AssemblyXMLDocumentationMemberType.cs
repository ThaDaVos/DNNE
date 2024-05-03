namespace DNNE.Assembly.XML;

internal enum AssemblyXMLDocumentationMemberType
{
    NAMESPACE = 0,
    TYPE = 1,
    FIELD = 2,
    PROPERTY = 3,
    METHOD = 4,
    EVENT = 5,
    ERROR = int.MaxValue - 1,
    UNKNOWN = int.MaxValue
}
