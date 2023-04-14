namespace DNNE.Assembly.Attributors
{
    internal class ClarionDeclCodeAttributor : Attributor
    {
        protected override string GetAttributeName() => "ClarionDeclCodeAttribute";

        protected override string GetLanguage() => "Clarion";

        protected override bool ApplicableToReturn() => false;
    }
}