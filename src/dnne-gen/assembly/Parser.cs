using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using DNNE.Assembly.Entities;
using DNNE.Assembly.Entities.Interfaces;

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

        public Old.AssemblyInformation Parse()
        {
            using FileStream stream = File.OpenRead(AssemblyPath);
            using PEReader peReader = new PEReader(stream);

            MetadataReader metadataReader = peReader.GetMetadataReader();

            AssemblyDefinition assemblyDefinition = metadataReader.GetAssemblyDefinition();

            string assemblyName = metadataReader.GetString(assemblyDefinition.Name);

            Console.WriteLine($"Assembly: {assemblyName}");

            IEnumerable<IExportedType>? types = metadataReader.GetExportedTypes();

            foreach (IExportedType type in types)
            {
                Console.WriteLine($"Type: {type.Name}");

                foreach (IExportedMethod method in type.Methods)
                {
                    Console.WriteLine($"\tMethod[{method.ReturnType}]: {method.Name}");

                    foreach (IExportedAttribute attribute in method.CustomAttributes)
                    {
                        Console.WriteLine($"\t\tAttribute[{attribute.Namespace}]: {attribute.Name}");
                    }
                }

                foreach (ExportedProperty property in type.Properties)
                {
                    Console.WriteLine($"\tProperty[{property.Type}|{property.KnownType}]: {property.Name} = {property.Value ?? "NULL"}");
                }

                foreach (ExportedField field in type.Fields)
                {
                    Console.WriteLine($"\tField[{field.Type}|{field.KnownType}]: {field.Name} = {field.Value ?? "NULL"}");
                }
            }

            return new Old.AssemblyInformation()
            {
                ExportedTypes = new List<Assembly.Old.ExportedType>().ToImmutableList(),
            };
        }
    }
}
