using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DLPTool
{
    public static class BinaryReaderExtensions
    {
        public static double ReadFloat(this BinaryReader @this)
        {
            return @this.ReadSingle();
        }

        public static string ReadString(this BinaryReader @this, int length)
        {
            var buf = @this.ReadBytes(length);
            var strLen = length;

            if (buf.Last() == '\0')
                strLen--;

            return Encoding.UTF8.GetString(buf, 0, strLen);
        }
    }

    public class BigEndianBinaryReader : BinaryReader
    {
        private byte[] ReadBigEndianBytes(int length)
        {
            var bytes = new byte[length];
            
            for (int i = (length - 1); i >= 0; i--)
                bytes[i] = ReadByte();

            return bytes;
        }

        public override decimal ReadDecimal()
        {
            throw new NotImplementedException();
        }

        public override double ReadDouble()
        {
            var buf = ReadBigEndianBytes(8);
            return BitConverter.ToDouble(buf, 0);
        }

        public override float ReadSingle()
        {
            var buf = ReadBigEndianBytes(4);
            return BitConverter.ToSingle(buf, 0);
        }

        public override short ReadInt16()
        {
            var buf = ReadBigEndianBytes(2);
            return BitConverter.ToInt16(buf, 0);
        }

        public override int ReadInt32()
        {
            var buf = ReadBigEndianBytes(4);
            return BitConverter.ToInt32(buf, 0);
        }

        public override long ReadInt64()
        {
            var buf = ReadBigEndianBytes(8);
            return BitConverter.ToInt64(buf, 0);
        }

        public override ushort ReadUInt16()
        {
            var buf = ReadBigEndianBytes(2);
            return BitConverter.ToUInt16(buf, 0);
        }

        public override uint ReadUInt32()
        {
            var buf = ReadBigEndianBytes(4);
            return BitConverter.ToUInt32(buf, 0);
        }

        public override ulong ReadUInt64()
        {
            var buf = ReadBigEndianBytes(8);
            return BitConverter.ToUInt64(buf, 0);
        }

        public BigEndianBinaryReader(Stream input) : base(input) {}
        public BigEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding) {}
    }

    class Program
    {
        static FileStream stream;
        static BinaryReader reader;

        static StringBuilder sb = new StringBuilder();

        static void ReadGroup()
        {
            var strLen = reader.ReadByte();
            var name = reader.ReadString(strLen);

            var vertCount = reader.ReadInt32();
            var patchCount = reader.ReadInt32();

            var verts = new short[vertCount];
            var patches = new short[patchCount];

            for (int i = 0; i < vertCount; i++)
                verts[i] = reader.ReadInt16();
            for (int i = 0; i < patchCount; i++)
                patches[i] = reader.ReadInt16();

            sb.AppendLine($"group {name} {{");

            if (vertCount > 0)
            {
                Console.WriteLine($"Group {name} has {vertCount} verts defined!");
                sb.AppendLine($"\t# nVerts: {vertCount}");
            }

            if (patchCount > 0)
            {
                sb.Append("\tpatch\n\t");
                for (int p = 0; p < patchCount; p++)
                {
                    if ((p > 0) && (p % 10 == 0.0))
                        sb.Append("\n\t");

                    sb.Append($"{patches[p],7}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("}\n");
        }

        static void ReadPatch()
        {
            var patch_id1 = reader.ReadInt16();
            var patch_id2 = reader.ReadInt16();
            var unk_04 = reader.ReadInt16();
            var unk_06 = reader.ReadInt16();
            var material_id = reader.ReadInt16();
            var texture_id = reader.ReadInt16();
            var phys_id = reader.ReadInt16();

            var vert_count = patch_id1 * patch_id2;
            
            sb.AppendLine($"facet {{");

            sb.AppendLine($"\t# unk_04: {unk_04}");
            sb.AppendLine($"\t# unk_06: {unk_06}");
            sb.AppendLine($"\t# material_id: {material_id}");
            sb.AppendLine($"\t# texture_id: {texture_id}");
            sb.AppendLine($"\t# phys_id: {phys_id}");

            sb.AppendLine($"\t{"res",-8} {patch_id1}");
            sb.AppendLine($"\t{"priority",-8} 50");
            sb.AppendLine($"\t{"map",-8} 1 1");
            sb.AppendLine($"\t{"tile",-8} {1.0:F6} {1.0:F6}");
            
            var vx = new short[vert_count];
            var smap = new double[vert_count];
            var tmap = new double[vert_count];
            var norm = new double[vert_count * 3];
            var color = new int[vert_count];
            
            for (int i = 0; i < vert_count; i++)
            {
                vx[i] = reader.ReadInt16();

                norm[i] = reader.ReadFloat();
                norm[i + 1] = reader.ReadFloat();
                norm[i + 2] = reader.ReadFloat();

                smap[i] = reader.ReadFloat();
                tmap[i] = reader.ReadFloat();

                color[i] = reader.ReadInt32();
            }

            sb.Append("\tvx\t");
            foreach (var v in vx)
                sb.Append($"{v} ");
            sb.AppendLine();

            sb.Append("\tsmap\n\t\t");
            foreach (var s in smap)
                sb.Append($"{s,9:F6} ");
            sb.AppendLine();
            
            sb.Append("\ttmap\n\t\t");
            foreach (var t in tmap)
                sb.Append($"{t,9:F6} ");
            sb.AppendLine();

            sb.AppendLine("\tnormals {");
            for (int n = 0; n < vert_count; n += 3)
                sb.AppendLine($"\t\t{norm[n],9:F6} {norm[n + 1],9:F6} {norm[n + 2],9:F6}");
            sb.AppendLine("\t}");

            var t1DataSize = reader.ReadInt32();

            if (t1DataSize > 0)
            {
                Console.WriteLine($"patch {patch_id1} x {patch_id2} has t1Data (size: 0x{t1DataSize:X8})");

                // skip data for now
                stream.Position += t1DataSize;
            }

            sb.AppendLine("}\n");
        }

        static void ReadNormal()
        {
            var x = reader.ReadFloat();
            var y = reader.ReadFloat();
            var z = reader.ReadFloat();

            sb.AppendLine($"vx {x,12:F6} {y,12:F6} {z,12:F6}");
        }

        static void Main(string[] args)
        {
            var file = @"C:\Dev\Scripts\3dsmax\MMBuilder\vck\gold\dlp\vpredcar_s.dlp";

            using (stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (reader = new BigEndianBinaryReader(stream))
            {
                if (reader.ReadInt32() != 0x444C5037)
                {
                    Console.WriteLine("not a DLP file :(");
                    return;
                }

                var numGroups = reader.ReadInt32();
                var numPatches = reader.ReadInt32();
                var numNormals = reader.ReadInt32();

                sb.AppendLine($"# numGroups: {numGroups}");
                sb.AppendLine($"# numPatches: {numPatches}");
                sb.AppendLine($"# numNormals: {numNormals}");

                for (int g = 0; g < numGroups; g++)
                    ReadGroup();
                for (int p = 0; p < numPatches; p++)
                    ReadPatch();
                for (int n = 0; n < numNormals; n++)
                    ReadNormal();

                var numMaterials = reader.ReadInt32();

                if (numMaterials > 0)
                {
                    sb.AppendLine($"# numMaterials: {numMaterials}");
                    stream.Position += (numMaterials * 0x66);
                }

                var numTextures = reader.ReadInt32();

                if (numTextures > 0)
                {
                    sb.AppendLine($"# numTextures: {numTextures}");
                    stream.Position += (numTextures * 0x24);
                }

                var numPhys = reader.ReadInt32();

                if (numPhys > 0)
                {
                    sb.AppendLine($"# numPhys: {numPhys}");
                    stream.Position += (numPhys * 0x64);
                }

                Console.WriteLine($"Finished reading file up to 0x{stream.Position:X8}");

                if (System.Diagnostics.Debugger.IsAttached)
                    Console.ReadKey();
            }

            File.WriteAllText(Path.ChangeExtension(file, ".log"), sb.ToString());
        }
    }
}
