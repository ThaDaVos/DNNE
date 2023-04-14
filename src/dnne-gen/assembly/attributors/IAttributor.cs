using System.Reflection.Metadata;

namespace DNNE.Assembly.Attributors
{
    interface IAttributor
    {
        bool IsApplicable(MetadataReader reader, CustomAttribute attribute, bool isReturn = false);
        UsedAttribute Parse(MetadataReader reader, ICustomAttributeTypeProvider<KnownType> resolver, CustomAttribute attribute, string target);
    }
}