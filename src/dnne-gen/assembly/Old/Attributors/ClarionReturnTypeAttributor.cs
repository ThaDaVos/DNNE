using System.Reflection.Metadata;

namespace DNNE.Assembly.Attributors
{
    internal class ClarionReturnTypeAttributor : Attributor
    {
        protected override string GetAttributeName() => "ClarionTypeAttribute";

        protected override string GetLanguage() => "Clarion";

        protected override bool ApplicableToReturn() => true;
    }
}