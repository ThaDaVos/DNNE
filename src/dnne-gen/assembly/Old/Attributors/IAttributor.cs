using System.Reflection.Metadata;
using DNNE.Assembly.Old;

namespace DNNE.Assembly.Attributors
{
    interface IAttributor
    {
        bool IsApplicable(MetadataReader reader, CustomAttribute attribute, bool isReturn = false);
        UsedAttribute Parse(MetadataReader reader, ICustomAttributeTypeProvider<Old.KnownType> resolver, CustomAttribute attribute, string target);
    }
}