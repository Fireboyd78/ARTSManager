using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARTSManager
{
    public class DLPGroup : DLPData
    {
        public string Name { get; set; }

        public short[] Vertices { get; set; }
        public short[] Patches { get; set; }

        public override void Load(asStream stream)
        {
            int strLen = stream.GetByte();
            Name = stream.GetString(strLen);

            var vertsCount = stream.GetInt();
            var patchesCount = stream.GetInt();

            Vertices = new short[vertsCount];
            Patches = new short[patchesCount];

            stream.Get(Vertices, vertsCount);
            stream.Get(Patches, patchesCount);
        }

        public override void Save(asStream stream)
        {
            var strLen = (byte)(Name.Length + 1);

            stream.Put(strLen);
            stream.PutString(Name, strLen);

            var vertsCount = (Vertices != null) ? Vertices.Length : 0;
            var patchesCount = (Patches != null) ? Patches.Length : 0;

            stream.Put(vertsCount);
            stream.Put(patchesCount);

            stream.Put(Vertices);
            stream.Put(Patches);
        }

        public override void Print(StringBuilder writer)
        {
            writer.AppendLine($"group {Name} {{");

            var numVerts = Vertices.Length;
            var numPatches = Patches.Length;

            if (numVerts > 0)
            {
                writer.Append("\tvx\n\t");
                for (int v = 0; v < numVerts; v++)
                {
                    if ((v > 0) && ((v % 10) == 0.0))
                        writer.Append("\n\t");

                    writer.Append($"{Vertices[v],7}");
                }
                writer.AppendLine();
            }

            if (numPatches > 0)
            {
                writer.Append("\tpatch\n\t");
                for (int p = 0; p < numPatches; p++)
                {
                    if ((p > 0) && ((p % 10) == 0.0))
                        writer.Append("\n\t");

                    writer.Append($"{Patches[p],7}");
                }
                writer.AppendLine();
            }

            writer.AppendLine("}");
        }

        public DLPGroup() { }
        public DLPGroup(asStream stream)
        {
            Load(stream);
        }
    }
}
