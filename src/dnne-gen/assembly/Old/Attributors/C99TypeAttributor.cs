namespace DNNE.Assembly.Attributors
{
    internal class C99TypeAttributor : Attributor
    {
        protected override string GetAttributeName() => "C99TypeAttribute";

        protected override string GetLanguage() => "C99";
        
        protected override bool ApplicableToReturn() => false;
    }
}