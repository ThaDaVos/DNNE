using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;
using DNNE.Assembly;

namespace DNNE.Generators
{
    internal class XMLTypeContractsGenerator : Generator
    {
        internal XMLTypeContractsGenerator(AssemblyInformation assemblyInformation) : base(assemblyInformation)
        {
        }

        public override string ParseOutPutFileName(string outputFile)
        {
            return outputFile.Replace("g.c", "g.xds");
        }

        protected override void Write(Stream outputStream)
        {
            var context = new ProjectLoadContext(this.assemblyInformation.Name, true, Path.GetDirectoryName(this.assemblyInformation.Path));

            var assembly = context.LoadFromAssemblyPath(this.assemblyInformation.Path);

            XsdDataContractExporter exporter = new XsdDataContractExporter();

            var exportedTypes = new List<string>();

            foreach (var enclosingType in this.assemblyInformation.ExportedTypes)
            {
                foreach (var export in enclosingType.ExportedMethods.OrderBy(ex => ex.MethodName))
                {
                    foreach (var argument in export.Arguments)
                    {
                        foreach (var attribute in argument.Attributes.Where(attr => attr.TargetLanguage == "XML" && attr.Group == "TypeContract" && attr.Target == "Argument"))
                        {
                            if (exportedTypes.Contains(attribute.Value)) continue;

                            var contractedType = assembly.GetType(attribute.Value);

                            if (contractedType == null)
                            {
                                Console.WriteLine("Cannot find type: " + attribute.Value);
                                continue;
                            }

                            if (exporter.CanExport(contractedType))
                            {
                                exporter.Export(contractedType);

                                exportedTypes.Add(attribute.Value);

                                foreach (XmlSchema schema in exporter.Schemas.Schemas(contractedType.Namespace))
                                {
                                    schema.Write(outputStream);
                                }
                            }
                        }
                    }
                }
            }

            context.Unload();
        }
    }
}