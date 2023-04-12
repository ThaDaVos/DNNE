using System.Runtime.InteropServices;

namespace DNNE.Language.Clarion
{
    [StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1, Size = 7)]
    internal struct LibFileHeader
    {
        public byte typ { get; set; } // 1 Byte
        public ushort len { get; set; } // 2 Bytes
        public ushort kind { get; set; } // 2 Bytes
        public byte bla { get; set; } // 1 Byte
        public byte ordFlag { get; set; } // 1 Byte
    }
}
