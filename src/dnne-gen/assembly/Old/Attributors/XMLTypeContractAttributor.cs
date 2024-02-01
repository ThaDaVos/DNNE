namespace DNNE.Assembly.Attributors
{
    internal class XMLTypeContractAttributor : Attributor
    {
        protected override string GetAttributeName() => "XMLTypeContractAttribute";

        protected override string GetLanguage() => "XML";

        protected override bool ApplicableToReturn() => false;
    }
}