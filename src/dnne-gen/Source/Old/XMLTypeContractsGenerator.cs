using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using DNNE.Assembly.Old;

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

                            var contractedType = assembly.GetType(attribute.Value)
                                ?? assembly.ExportedTypes.Where(t => t.Name == attribute.Value || t.FullName == attribute.Value).FirstOrDefault();

                            if (contractedType == null)
                            {
                                Console.WriteLine($"Cannot find type: `{attribute.Value}`");
                                continue;
                            }

                            if (exporter.CanExport(contractedType))
                            {
                                Console.WriteLine($"Export schema for `{contractedType.FullName}`");
                                exporter.Export(contractedType);

                                XmlSchemaSet mySchemas = exporter.Schemas;

                                XmlQualifiedName XmlNameValue = exporter.GetRootElementName(contractedType);

                                foreach (XmlSchema schema in exporter.Schemas.Schemas(XmlNameValue.Namespace))
                                {
                                    schema.Write(outputStream);
                                }

                                exportedTypes.Add(attribute.Value);
                            } else {
                                Console.WriteLine($"Cannot export schema for `{contractedType.Name}`");
                            }
                        }
                    }
                }
            }

            // context.Unload();
        }
    }
}