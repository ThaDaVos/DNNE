using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
                Value = C99TypeProvider.GetFirstFixedArgAsStringValue(resolver, attribute),
                Values = GetAttributeArgs(resolver, attribute, reader)
        };
        }

        private Dictionary<string, AttributeArgument> GetAttributeArgs(ICustomAttributeTypeProvider<KnownType> typeResolver, CustomAttribute attribute, MetadataReader reader)
        {
            string[] constructorArgumentNames = GetArgumentNamesFromCustomAttribute(reader, attribute);

            var arguments = new Dictionary<string, AttributeArgument>();

            CustomAttributeValue<KnownType> data = attribute.DecodeValue(typeResolver);

            int count = 0;
            foreach (CustomAttributeTypedArgument<KnownType> item in data.FixedArguments)
            {
                arguments.Add(constructorArgumentNames[count], new AttributeArgument() {
                    Type = item.Type,
                    Value = item.Value,
                });

                count++;
            }

            foreach (CustomAttributeNamedArgument<KnownType> item in data.NamedArguments)
            {
                arguments.Add(item.Name, new AttributeArgument()
                {
                    Type = item.Type,
                    Value = item.Value,
                });
            }

            return arguments;
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

        internal static string[] GetArgumentNamesFromCustomAttribute(MetadataReader reader, CustomAttribute attribute)
        {
            switch (attribute.Constructor.Kind)
            {
                case HandleKind.MemberReference:
                    MemberReference refConstructor = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                    return [];

                case HandleKind.MethodDefinition:
                    MethodDefinition defConstructor = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);

                    ParameterHandleCollection parameters = defConstructor.GetParameters();
                    List<string> names = new (parameters.Count);
                    foreach (ParameterHandle param in parameters)
                    {
                        names.Add(reader.GetString(reader.GetParameter(param).Name));
                    }

                    return names.ToArray();

                default:
                    Debug.Assert(false, "Unknown attribute constructor kind");
                    return [];
            }
        }
    }
}