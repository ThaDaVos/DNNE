using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml;
using DNNE.Assembly.Entities;
using DNNE.Assembly.Entities.Interfaces;
using DNNE.Assembly.XML;

namespace DNNE.Assembly
{
    internal class Parser
    {
        private string AssemblyPath { get; }
        private string? XmlDocFilePath { get; }

        public Parser(string assemblyPath, string? xmlDocFilePath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                throw new ArgumentNullException("assemblyPath");

            if (File.Exists(assemblyPath) == false)
                throw new FileNotFoundException("Assembly file not found.", assemblyPath);

            AssemblyPath = assemblyPath;

            if (string.IsNullOrEmpty(xmlDocFilePath) == false && File.Exists(xmlDocFilePath) == false)
                throw new FileNotFoundException("XmlDoc file not found.", xmlDocFilePath);

            XmlDocFilePath = xmlDocFilePath;
        }

        public AssemblyInformation Parse()
        {
            IExportedAssembly assembly = ParseAssembly();
            AssemblyXMLDocumentation? documentation = ParseXmlDoc();

            return new()
            {
                Assembly = assembly,
                Documentation = documentation
            };
        }

        private IExportedAssembly ParseAssembly()
        {
            using FileStream stream = File.OpenRead(AssemblyPath);
            using PEReader peReader = new PEReader(stream);

            MetadataReader metadataReader = peReader.GetMetadataReader();

            AssemblyDefinition assemblyDefinition = metadataReader.GetAssemblyDefinition();

            IExportedAssembly assembly = new ExportedAssembly(metadataReader, assemblyDefinition, AssemblyPath);

#if DEBUG
            WriteOutAssembly(assembly);
#endif

            return assembly;
        }

#if DEBUG
        private void WriteOutAssembly(IExportedAssembly assembly)
        {
            Console.WriteLine($"Assembly: {assembly.Name} ({assembly.SafeName}) [{assembly.Path}]");

            foreach (IExportedType type in assembly.ExportedTypes)
            {
                Console.WriteLine($"Type: {type.Name}");

                foreach (ExportedProperty property in type.Properties)
                {
                    Console.WriteLine($"\tProperty[{property.Type}|{property.KnownType}]: {property.Name} = {property.Value ?? "NULL"}");
                }

                foreach (ExportedField field in type.Fields)
                {
                    Console.WriteLine($"\tField[{field.Type}|{field.KnownType}]: {field.Name} = {field.Value ?? "NULL"}");
                }

                foreach (IExportedMethod method in type.Methods)
                {
                    Console.WriteLine($"\tMethod[{method.ReturnType}]: {method.Name}");

                    foreach (IExportedAttribute attribute in method.CustomAttributes)
                    {
                        Console.WriteLine($"\t\tAttribute[{attribute.Namespace}]: {attribute.Name}");
                    }

                    foreach (ExportedMethodParameter parameter in method.Parameters)
                    {
                        Console.WriteLine($"\t\tParameter[{parameter.Type}]: {parameter.Name} => {parameter.Value ?? "NULL"}");

                        foreach (IExportedAttribute attribute in parameter.CustomAttributes)
                        {
                            Console.WriteLine($"\t\t\tAttribute[{attribute.Namespace}]: {attribute.Name}");
                        }
                    }
                }

            }
        }
#endif

        private AssemblyXMLDocumentation? ParseXmlDoc()
        {
            if (string.IsNullOrEmpty(XmlDocFilePath))
                return null;

            using XmlReader xmlReader = XmlReader.Create(XmlDocFilePath);

            string? assemblyName = null;
            IEnumerable<AssemblyXMLDocumentationMember>? assemblyMembers = null;

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "doc")
                {
                    continue;
                }

                if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "doc")
                {
                    continue;
                }

                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "assembly")
                {
                    assemblyName = ParseAssemblyName(xmlReader);
                    continue;
                }

                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "members")
                {
                    assemblyMembers = ParseMembers(xmlReader);
                    continue;
                }
            }

            if (string.IsNullOrEmpty(assemblyName) || assemblyMembers == null)
                return null;

            return new()
            {
                Name = assemblyName,
                Members = assemblyMembers
            };
        }

        private string ParseAssemblyName(XmlReader xmlReader)
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "name")
                {
                    return xmlReader.ReadElementContentAsString();
                }

                if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "assembly")
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        private IEnumerable<AssemblyXMLDocumentationMember> ParseMembers(XmlReader xmlReader)
        {
            List<AssemblyXMLDocumentationMember> members = new();
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "member")
                {
                    AssemblyXMLDocumentationMember? member = ParseMember(xmlReader);
                    if (member != null)
                        members.Add(member);
                }

                if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "members")
                {
                    break;
                }
            }

            return members;
        }

        private AssemblyXMLDocumentationMember? ParseMember(XmlReader xmlReader)
        {
            string id = xmlReader.GetAttribute("name") ?? string.Empty;

            if (string.IsNullOrEmpty(id))
                return null;

            string? summary = null, remarks = null, returns = null;
            Dictionary<string, string>? parameters = null;

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.EndElement && (xmlReader.Name == "member" || xmlReader.Name == "members"))
                {
                    break;
                }

                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "summary")
                {
                    summary = xmlReader.ReadElementContentAsString();
                }

                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "remarks")
                {
                    remarks = xmlReader.ReadElementContentAsString();
                }

                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "returns")
                {
                    returns = xmlReader.ReadElementContentAsString();
                }

                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "param")
                {
                    string name = xmlReader.GetAttribute("name") ?? string.Empty;
                    string value = xmlReader.ReadElementContentAsString();

                    if (parameters == null)
                        parameters = new Dictionary<string, string>();

                    parameters.Add(name, value);
                }
            }

            return new(id)
            {
                Summary = summary,
                Remarks = remarks,
                Returns = returns,
                Parameters = parameters
            };
        }
    }
}
