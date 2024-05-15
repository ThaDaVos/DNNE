using System;

namespace DNNE.Source.IO;

public class SourceWriterFileScope : IDisposable
{
    private readonly SourceWriter _writer;
    private bool _disposed;
    private string _lastSourceFileName;
    public SourceWriterFileScope(SourceWriter writer, string sourceFileName)
    {
        _writer = writer.UseSourceFile(sourceFileName, out _lastSourceFileName);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _writer.UseSourceFile(_lastSourceFileName, out _);
            _disposed = true;
        }
    }
}
