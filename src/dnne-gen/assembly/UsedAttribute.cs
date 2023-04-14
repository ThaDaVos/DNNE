using System;
using System.Globalization;

namespace DNNE.Assembly
{
    public struct UsedAttribute
    {
        public string Namespace { get; init; }
        public string Name { get; init; }
        public string Group
        {
            get
            {
                return this.Name
                    .Replace(this.TargetLanguage, "", true, CultureInfo.InvariantCulture)
                    .Replace("Attribute", "", true, CultureInfo.InvariantCulture);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public string TargetLanguage { get; init; }
        public string Target { get; init; }
        public string Value { get; init; }
    }
}