namespace DNNE.Assembly.Attributors
{
    internal class ClarionTypeAttributor : Attributor
    {
        protected override string GetAttributeName() => "ClarionTypeAttribute";

        protected override string GetLanguage() => "Clarion";

        protected override bool ApplicableToReturn() => false;
    }
}