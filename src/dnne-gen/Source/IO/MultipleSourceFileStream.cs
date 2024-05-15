using System.Collections.Generic;
using System.IO;

namespace DNNE.Source.IO;

public class MultipleSourceFileStream<TStream> : Stream where TStream : Stream,new()
{
    private readonly IDictionary<string, TStream> _streams = new Dictionary<string, TStream>();
    private string _currentSourceFileName;

    public MultipleSourceFileStream(string sourceFilename)
    {
        _streams.Add(sourceFilename, new TStream());
        _currentSourceFileName = sourceFilename;
    }

#region  Stream Implementation
    public override bool CanRead => _streams[_currentSourceFileName].CanRead;
    public override bool CanSeek => _streams[_currentSourceFileName].CanSeek;
    public override bool CanWrite => _streams[_currentSourceFileName].CanWrite;
    public override long Length => _streams[_currentSourceFileName].Length;
    public override long Position { 
        get => _streams[_currentSourceFileName].Position; 
        set => _streams[_currentSourceFileName].Position = value; 
    }

    public override void Flush() => _streams[_currentSourceFileName].Flush();

    public override int Read(byte[] buffer, int offset, int count) => _streams[_currentSourceFileName].Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _streams[_currentSourceFileName].Seek(offset, origin);

    public override void SetLength(long value) => _streams[_currentSourceFileName].SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _streams[_currentSourceFileName].Write(buffer, offset, count);
    #endregion

    internal MultipleSourceFileStream<TStream> UseSourceFile(string sourceFilename) => UseSourceFile(sourceFilename, out _);

    internal MultipleSourceFileStream<TStream> UseSourceFile(string sourceFilename, out string lastSourceFileName)
    {
        if (!_streams.ContainsKey(sourceFilename))
        {
            _streams.Add(sourceFilename, new TStream());
        }

        lastSourceFileName = _currentSourceFileName;
        
        _currentSourceFileName = sourceFilename;
        
        return this;
    }
}
