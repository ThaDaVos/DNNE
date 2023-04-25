using System.IO;
using System.Runtime.Serialization;
using System.Xml;
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
            var settings = new XmlWriterSettings
            {
                Indent = true,
            };

            var serializer = new DataContractSerializer(typeof(AssemblyInformation));
            using var writer = XmlWriter.Create(outputStream, settings);
            serializer.WriteObject(writer, this.assemblyInformation);
            writer.Flush();
        }
    }
}