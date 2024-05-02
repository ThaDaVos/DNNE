using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DNNE.Assembly.Old;
using DNNE.Language.Clarion;

namespace DNNE.Generators
{
    internal partial class ClarionLibGenerator : ClarionGenerator
    {
        internal ClarionLibGenerator(AssemblyInformation assemblyInformation) : base(assemblyInformation)
        {
        }

        public override string ParseOutPutFileName(string outputFile)
        {
            return outputFile.Replace("g.c", "g.lib");
        }

        protected override void Write(Stream outputStream)
        {
            var exportQ = new List<ExportQ>();

            int ordinal = 1;

            foreach (var enclosingType in this.assemblyInformation.ExportedTypes)
            {
                string className = ResolveClassName(enclosingType);

                foreach (var export in enclosingType.ExportedMethods.OrderBy(ex => ex.MethodName))
                {
                    var safeTypeName = export.EnclosingTypeName.Replace("_", "").Replace(".", "_");

                    exportQ.Add(new ExportQ
                    {
                        Module = $"{this.assemblyInformation.Name}NE.dll",
                        Symbol = $"{className}_{export.MethodName}",
                        Ordinal = ordinal,
                        OrgOrder = ordinal,
                        TreeLevel = 2,
                    });

                    ordinal++;
                }
            }

            exportQ.Add(new ExportQ
            {
                Module = $"{this.assemblyInformation.Name}NE.dll",
                Symbol = $"_{this.assemblyInformation.Name}_try_preload_runtime@0",
                Ordinal = ordinal,
                OrgOrder = ordinal,
                TreeLevel = 2,
            });

            ordinal++;

            exportQ.Add(new ExportQ
            {
                Module = $"{this.assemblyInformation.Name}NE.dll",
                Symbol = $"_{this.assemblyInformation.Name}_preload_runtime@0",
                Ordinal = ordinal,
                OrgOrder = ordinal,
                TreeLevel = 2,
            });

            ordinal++;

            Generate(exportQ, outputStream);
        }

        public void Generate(List<ExportQ> exportQ, Stream outputStream)
        {
            using var libFile = new BinaryWriter(outputStream);

            for (int i = 0; i < exportQ.Count; i++)
            {
                var export = exportQ[i];
                if (export.TreeLevel == 2)
                {
                    var libHeader = new LibFileHeader
                    {
                        typ = 0x88,
                        kind = 0x0A000,
                        bla = 1,
                        ordFlag = 1
                    };

                    int moduleByteCount = Encoding.UTF8.GetByteCount(export.Module);
                    int symbolByteCount = Encoding.UTF8.GetByteCount(export.Symbol);
                    int headerSize = Marshal.SizeOf(libHeader);

                    var recordSize = moduleByteCount /* Module Size */
                        + symbolByteCount /* Symbol Size */
                        + 2 /* Length Bytes */
                        + 2 /* Ordinal Size  */
                        + headerSize /* Header Size */
                        - 3 /* Exclude first three bytes of Header */;

                    libHeader.len = (ushort)recordSize;

                    WriteStruct(libFile, libHeader);
                    WritePString(libFile, export.Symbol);
                    WritePString(libFile, export.Module);
                    WriteUShort(libFile, (ushort)export.Ordinal);
                }
            }
        }

        private static void WriteStruct<T>(BinaryWriter writer, T structure)
        {
            var handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
            try
            {
                var ptr = handle.AddrOfPinnedObject();
                var bytes = new byte[Marshal.SizeOf(typeof(T))];
                Marshal.Copy(ptr, bytes, 0, bytes.Length);
                writer.Write(bytes);
            }
            finally
            {
                handle.Free();
            }
        }

        private static void WritePString(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            writer.Write((byte)bytes.Length); // First write the length
            writer.Write(bytes); // Then write the actual string
        }

        private static void WriteUShort(BinaryWriter writer, ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            // if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            writer.Write(bytes);
        }
    }
}
