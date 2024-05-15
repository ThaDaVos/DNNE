using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace DNNE.Source.IO;

public class SourceWriter : StreamWriter
{
    public SourceWriter(string sourceFileName) : base(new MultipleSourceFileStream<MemoryStream>(sourceFileName))
    {
    }

    public SourceWriter(string sourceFileName, Encoding encoding) : base(new MultipleSourceFileStream<MemoryStream>(sourceFileName), encoding)
    {
    }

    public SourceWriter(string sourceFileName, Encoding encoding, int bufferSize) : base(new MultipleSourceFileStream<MemoryStream>(sourceFileName), encoding, bufferSize)
    {
    }

    public SourceWriter(string sourceFileName, Encoding? encoding = null, int bufferSize = -1, bool leaveOpen = false) : base(new MultipleSourceFileStream<MemoryStream>(sourceFileName), encoding, bufferSize, leaveOpen)
    {
    }

    internal SourceWriterFileScope UseSourceFileScope(string sourceFileName) => new SourceWriterFileScope(this, sourceFileName);

    internal SourceWriter UseSourceFile(string sourceFilename) => UseSourceFile(sourceFilename, out _);
    internal SourceWriter UseSourceFile(string sourceFilename, out string lastSourceFileName)
    {
        ((MultipleSourceFileStream<MemoryStream>)BaseStream).UseSourceFile(sourceFilename, out lastSourceFileName);

        return this;
    }

    internal SourceWriter UseSourceFile(string sourceFilename, Action<SourceWriter> action)
    {
        UseSourceFile(sourceFilename, out string last);

        action(this);

        UseSourceFile(last, out string temporarySourceFileName);

        if (sourceFilename.Equals(temporarySourceFileName) == false)
        {
            throw new InvalidOperationException(@"The last source file name is not the same as the temporary source file name. The action may have been performed on a wrong stream!");
        }

        return this;
    }

    public void WriteTo(string sourceFileName, [StringSyntax("CompositeFormat")] string format, params object?[] arg)
        => UseSourceFile(sourceFileName, (writer) => writer.Write(format, arg));
    public void WriteTo(string sourceFileName, [StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1, object? arg2)
        => UseSourceFile(sourceFileName, (writer) => writer.Write(format, arg0, arg1, arg2));
    public void WriteTo(string sourceFileName, [StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1)
        => UseSourceFile(sourceFileName, (writer) => writer.Write(format, arg0, arg1));
    public void WriteTo(string sourceFileName, string? value)
        => UseSourceFile(sourceFileName, (writer) => writer.Write(value));
    public void WriteTo(string sourceFileName, char[] buffer, int index, int count)
        => UseSourceFile(sourceFileName, (writer) => writer.Write(buffer, index, count));
    public void WriteTo(string sourceFileName, char[]? buffer)
        => UseSourceFile(sourceFileName, (writer) => writer.Write(buffer));
    public void WriteTo(string sourceFileName, char value)
        => UseSourceFile(sourceFileName, (writer) => writer.Write(value));
    public void WriteTo(string sourceFileName, [StringSyntax("CompositeFormat")] string format, object? arg0)
        => UseSourceFile(sourceFileName, (writer) => writer.Write(format, arg0));
    public void WriteLineTo(string sourceFileName, [StringSyntax("CompositeFormat")] string format, params object?[] arg)
        => UseSourceFile(sourceFileName, (writer) => writer.WriteLine(format, arg));
    public void WriteLineTo(string sourceFileName, [StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1, object? arg2)
        => UseSourceFile(sourceFileName, (writer) => writer.WriteLine(format, arg0, arg1, arg2));
    public void WriteLineTo(string sourceFileName, [StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1)
        => UseSourceFile(sourceFileName, (writer) => writer.WriteLine(format, arg0, arg1));
    public void WriteLineTo(string sourceFileName, string? value)
        => UseSourceFile(sourceFileName, (writer) => writer.WriteLine(value));
    public void WriteLineTo(string sourceFileName, [StringSyntax("CompositeFormat")] string format, object? arg0)
        => UseSourceFile(sourceFileName, (writer) => writer.WriteLine(format, arg0));
}
