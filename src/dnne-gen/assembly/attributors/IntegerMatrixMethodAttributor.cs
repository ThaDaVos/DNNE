namespace DNNE.Assembly.Attributors
{
    internal class IntegerMatrixMethodAttributor : Attributor
    {
        protected override string GetAttributeName() => "IntegerMatrixMethodAttribute";

        protected override string GetLanguage() => null;
        
        protected override bool ApplicableToReturn() => false;
    }
}