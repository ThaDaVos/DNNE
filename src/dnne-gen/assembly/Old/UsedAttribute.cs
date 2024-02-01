using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace DNNE.Assembly.Old
{
    [DataContract]
    public struct UsedAttribute
    {
        [DataMember]
        public string Namespace { get; init; }
        [DataMember]
        public string Name { get; init; }
        [DataMember]
        public string Group
        {
            get
            {
                return this.Name
                    .Replace(this.TargetLanguage ?? "<NULL>", "", true, CultureInfo.InvariantCulture)
                    .Replace("Attribute", "", true, CultureInfo.InvariantCulture);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public string TargetLanguage { get; init; }
        [DataMember]
        public string Target { get; init; }
        [DataMember]
        public string Value { get; init; }
        [DataMember]
        public Dictionary<string, AttributeArgument> Values { get; init; }
    }
}