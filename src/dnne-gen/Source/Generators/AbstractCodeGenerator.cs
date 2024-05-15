using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DNNE.Assembly;
using DNNE.Assembly.Entities;
using DNNE.Assembly.Entities.Interfaces;
using DNNE.Source.IO;
using DNNE.Source.Naming;

namespace DNNE.Source.Generators;

internal abstract class AbstractCodeGenerator
{
    // This class is the base class for all code generators. It provides the basic functionality to generate code for a given assembly.
    public AssemblyInformation AssemblyInformation { get; }
    public INamingHelper NamingHelper { get; }

    internal AbstractCodeGenerator(AssemblyInformation assemblyInformation, INamingHelper namingHelper)
    {
        AssemblyInformation = assemblyInformation;
        NamingHelper = namingHelper;
    }

    // Steps:
    // 1. Resolve file name
    // 2. Resolve file path
    // 3. Go over each exported type in the assembly
    // 4. Generate code for each type by (each step below may cause a file split, somehow):
    //    a. Processing the class definition and if needed generate code
    //    b. Processing the properties and if needed generate code
    //    c. Processing the methods and if needed generate code
    // 5. Write the generated code to the file
    public void GenerateCode()
    {
        string fileName = ResolveFileName();
        string filePath = ResolveFileDirectory();
        string sourceFileName = Path.Combine(filePath, fileName);

        using SourceWriter? writer = new SourceWriter(sourceFileName);

        foreach (IExportedType type in AssemblyInformation.Assembly.ExportedTypes)
        {
            ProcessExportedTypeToFile(writer, type, sourceFileName);
        }
    }

    protected abstract bool WriteCommentToSourceFile(SourceWriter writer, SourceGeneratorCommentStyle style, [StringSyntax("CompositeFormat")] string format, params object?[] arg);
    protected abstract bool HandleInclusionOfSourceFile(SourceWriter writer, string sourceFileName);

    protected virtual bool ProcessExportedTypeToFile(SourceWriter writer, IExportedType type, string sourceFileName)
    {
        string workingSourceFileName = sourceFileName.Replace("%{type.name}%", type.Name, StringComparison.InvariantCultureIgnoreCase);

        if (workingSourceFileName.Equals(sourceFileName, StringComparison.InvariantCultureIgnoreCase) == false)
        {
            if (
                !HandleInclusionOfSourceFile(writer, workingSourceFileName)
                && !WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.COMMENT, "Split source file for type {Name} into {workingSourceFileName}", type.Name, workingSourceFileName)
            )
            {
                Console.WriteLine("Tried to add include or comment to source file for type {Name}, but was not able to.", type.Name);
            }
        }

        using (SourceWriterFileScope? scope = writer.UseSourceFileScope(workingSourceFileName))
        {
            HandleExportedType(writer, type, workingSourceFileName);
        }

        return true;
    }

    protected virtual void HandleExportedType(SourceWriter writer, IExportedType type, string workingSourceFileName)
    {
        // Write the class opening based on the type
        if (
            HandleClassOpening(writer, type) == false
            && WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.COMMENT, "No class opening to be written") == false
        ) Console.WriteLine("Tried to write class opening or comment about class opening for type {Name}, but was not able to.", type.Name);

        // Write the fields of the type
        if (
            HandleFieldsOfType(writer, type) == false
            && WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.COMMENT, "No fields to be written") == false
        ) Console.WriteLine("Tried to write fields or comment about fields for type {Name}, but was not able to.", type.Name);

        // Write the properties of the type
        if (
            HandlePropertiesOfType(writer, type) == false
            && WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.COMMENT, "No properties to be written") == false
        ) Console.WriteLine("Tried to write properties or comment about properties for type {Name}, but was not able to.", type.Name);

        // Write the methods of the type
        if (
            HandleMethodsOfType(writer, type) == false
            && WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.COMMENT, "No methods to be written") == false
        ) Console.WriteLine("Tried to write methods or comment about methods for type {Name}, but was not able to.", type.Name);

        // Write the nested types of the type
        if (
            HandleNestedTypesOfType(writer, type, workingSourceFileName) == false
            && WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.COMMENT, "No nested types to be written") == false
        ) Console.WriteLine("Tried to write nested types or comment about nested types for type {Name}, but was not able to.", type.Name);

        // Write the class closing based on the type
        if (
            HandleClassClosing(writer, type) == false
            && WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.COMMENT, "No class closing to be written") == false
        ) Console.WriteLine("Tried to write class closing or comment about class closing for type {Name}, but was not able to.", type.Name);
    }

    protected virtual bool HandleFieldsOfType(SourceWriter writer, IExportedType type)
    {
        if (type.Fields.Any()) return false;

        WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.REGION_START, "Class {Name} | Methods", type.Name);

        foreach (ExportedField field in type.Fields)
            HandleField(writer, field);

        WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.REGION_END, "Class {Name} | Methods", type.Name);

        return true;
    }

    protected abstract bool HandleField(SourceWriter writer, ExportedField field);

    protected virtual bool HandlePropertiesOfType(SourceWriter writer, IExportedType type)
    {
        if (type.Properties.Any()) return false;

        WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.REGION_START, "Class {Name} | Properties", type.Name);

        foreach (ExportedProperty property in type.Properties)
            HandleProperty(writer, property);

        WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.REGION_END, "Class {Name} | Properties", type.Name);

        return true;
    }

    protected abstract bool HandleProperty(SourceWriter writer, ExportedProperty property);

    protected virtual bool HandleMethodsOfType(SourceWriter writer, IExportedType type)
    {
        if (type.Methods.Any()) return false;

        WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.REGION_START, "Class {Name} | Methods", type.Name);

        foreach (IExportedMethod method in type.Methods)
            HandleMethod(writer, method);

        WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.REGION_END, "Class {Name} | Methods", type.Name);

        return true;
    }

    protected abstract bool HandleMethod(SourceWriter writer, IExportedMethod method);

    protected virtual bool HandleNestedTypesOfType(SourceWriter writer, IExportedType type, string workingSourceFileName)
    {
        if (type.NestedExportedTypes.Any()) return false;

        WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.REGION_START, "Class {Name} | Nested Types", type.Name);

        foreach (IExportedType nestedType in type.NestedExportedTypes)
            HandleNestedType(writer, type, workingSourceFileName, nestedType);

        WriteCommentToSourceFile(writer, SourceGeneratorCommentStyle.REGION_END, "Class {Name} | Nested Types", type.Name);

        return true;
    }

    protected virtual bool HandleNestedType(SourceWriter writer, IExportedType type, string workingSourceFileName, IExportedType nestedType)
    {
        string nestedTypeFileName = workingSourceFileName.Replace("%{nestedType.name}%", $"{type.Name}.%{{type.name}}%", StringComparison.InvariantCultureIgnoreCase);

        return ProcessExportedTypeToFile(writer, nestedType, nestedTypeFileName);
    }

    protected abstract bool HandleClassClosing(SourceWriter writer, IExportedType type);
    protected abstract bool HandleClassOpening(SourceWriter writer, IExportedType type);
    protected virtual string ResolveFileDirectory()
        => Path.GetDirectoryName(AssemblyInformation.Assembly.Path ?? Directory.GetCurrentDirectory()) ?? Directory.GetCurrentDirectory();

    protected abstract string ResolveFileName();
}
