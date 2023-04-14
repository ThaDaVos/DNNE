using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using DNNE.Languages.C99;

namespace DNNE.Assembly.Attributors
{
    abstract class Attributor : IAttributor
    {
        protected abstract string GetLanguage();
        protected abstract string GetAttributeName();
        protected abstract bool ApplicableToReturn();

        public virtual bool IsApplicable(MetadataReader reader, CustomAttribute attribute, bool isReturn = false)
        {
            var (namespaceMaybe, nameMaybe) = ParseCustomAttribute(reader, attribute);

            return namespaceMaybe.HasValue && nameMaybe.HasValue
                && reader.StringComparer.Equals(namespaceMaybe.Value, "DNNE")
                && reader.StringComparer.Equals(nameMaybe.Value, this.GetAttributeName());
        }
        public virtual UsedAttribute Parse(MetadataReader reader, ICustomAttributeTypeProvider<KnownType> resolver, CustomAttribute attribute, string target)
        {
            var (namespaceMaybe, nameMaybe) = ParseCustomAttribute(reader, attribute);

            if (namespaceMaybe.HasValue == false || nameMaybe.HasValue == false) {
                throw new ArgumentNullException("The given attribute resolves into a null namespace or null name");
            }

            return new UsedAttribute()
            {
                Namespace = reader.GetString(namespaceMaybe.Value),
                Name = reader.GetString(nameMaybe.Value),
                TargetLanguage = this.GetLanguage(),
                Target = target,
                Value = C99TypeProvider.GetFirstFixedArgAsStringValue(resolver, attribute)
            };
        }

        internal static (StringHandle? parsedNamespace, StringHandle? parsedName) ParseCustomAttribute(MetadataReader reader, CustomAttribute attribute)
        {
            switch (attribute.Constructor.Kind)
            {
                case HandleKind.MemberReference:
                    MemberReference refConstructor = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                    TypeReference refType = reader.GetTypeReference((TypeReferenceHandle)refConstructor.Parent);
                    return (refType.Namespace, refType.Name);

                case HandleKind.MethodDefinition:
                    MethodDefinition defConstructor = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
                    TypeDefinition defType = reader.GetTypeDefinition(defConstructor.GetDeclaringType());
                    return (defType.Namespace, defType.Name);

                default:
                    Debug.Assert(false, "Unknown attribute constructor kind");
                    return (null, null);
            }
        }
    }
}