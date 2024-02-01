namespace DNNE.Assembly.Attributors
{
    internal class StringMatrixMethodAttributor : Attributor
    {
        protected override string GetAttributeName() => "StringMatrixMethodAttribute";

        protected override string GetLanguage() => null;
        
        protected override bool ApplicableToReturn() => false;
    }
}