namespace DNNE.Assembly.Attributors
{
    internal class MatrixMethodOfStringsAttributor : Attributor
    {
        protected override string GetAttributeName() => "MatrixMethodOfStringsAttribute";

        protected override string GetLanguage() => null;
        
        protected override bool ApplicableToReturn() => false;
    }
}