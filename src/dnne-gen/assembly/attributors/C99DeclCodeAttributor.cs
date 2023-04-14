namespace DNNE.Assembly.Attributors
{
    internal class C99DeclCodeAttributor : Attributor
    {
        protected override string GetAttributeName() => "C99DeclCodeAttribute";

        protected override string GetLanguage() => "C99";
        
        protected override bool ApplicableToReturn() => false;
    }
}