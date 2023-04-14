namespace DNNE.Assembly.Attributors
{
    internal class ClarionIncCodeAttributor : Attributor
    {
        protected override string GetAttributeName() => "ClarionIncCodeAttribute";

        protected override string GetLanguage() => "Clarion";

        protected override bool ApplicableToReturn() => false;
    }
}