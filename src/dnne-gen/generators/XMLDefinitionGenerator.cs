using System.IO;
using System.Xml.Serialization;
using DNNE.Assembly;

namespace DNNE.Generators
{
    internal class XMLDefinitionGenerator : Generator
    {
        internal XMLDefinitionGenerator(AssemblyInformation assemblyInformation) : base(assemblyInformation)
        {
        }

        public override string ParseOutPutFileName(string outputFile)
        {
            return outputFile.Replace("g.c", "g.xml");
        }

        protected override void Write(Stream outputStream)
        {
            var serializer = new XmlSerializer(typeof(AssemblyInformation));
            using var writer = new StreamWriter(outputStream);
            serializer.Serialize(writer, this.assemblyInformation);
        }
    }
}