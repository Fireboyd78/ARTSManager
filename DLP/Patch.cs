using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARTSManager
{
    using FlagsLookup = Dictionary<PatchFlags, string>;

    [Flags]
    public enum PatchFlags : int
    {
        CPV = (1 << 0),
        Emission = (1 << 1),
        Shade = (1 << 2),
        Solid = (1 << 3),
        Cull = (1 << 4),
        ZWrite = (1 << 5),
        ZRead = (1 << 6),

        Shadow = (1 << 7),

        Flat = (1 << 8),
        AntiAlias = (1 << 9),
        Interpenetrate = (1 << 10),
    }

    public class DLPPatch : DLPData
    {
        private static readonly FlagsLookup _flagsLookup = new FlagsLookup() {
            { PatchFlags.CPV, "cpv" },
            { PatchFlags.Emission, "emission" },
            { PatchFlags.Shade, "shade" },
            { PatchFlags.Solid, "solid" },
            { PatchFlags.Cull, "cull" },
            { PatchFlags.ZRead, "zread" },
            { PatchFlags.ZWrite, "zwrite" },
            { PatchFlags.Flat, "flat" },
            { PatchFlags.AntiAlias, "antialias" },
            { PatchFlags.Interpenetrate, "interpenetrate" },

            { PatchFlags.Shadow, "shadow" },
        };

        public short Resolution { get; set; }
        public short Stride { get; set; } // assumed name

        public short Unknown { get; set; }

        public short Flags { get; set; }

        public short Material { get; set; }
        public short Texture { get; set; }
        public short Physics { get; set; }

        public List<DLPVertex> Vertices { get; set; }

        public string UserData { get; set; }

        public override void Load(asStream stream)
        {
            Resolution = stream.GetShort();
            Stride = stream.GetShort();

            Unknown = stream.GetShort();

            Flags = stream.GetShort();

            Material = stream.GetShort();
            Texture = stream.GetShort();
            Physics = stream.GetShort();

            var numVerts = (Resolution * Stride);

            Vertices = new List<DLPVertex>(numVerts);

            for (int i = 0; i < numVerts; i++)
            {
                var vertex = new DLPVertex(stream);
                Vertices.Add(vertex);
            }

            UserData = stream.GetString();
        }

        public override void Save(asStream stream)
        {
            stream.Put(Resolution);
            stream.Put(Stride);
            stream.Put(Unknown);
            stream.Put(Flags);

            stream.Put(Material);
            stream.Put(Texture);
            stream.Put(Physics);

            foreach (var vertex in Vertices)
                vertex.Save(stream);

            stream.PutString(UserData);
        }

        public override void Print(StringBuilder writer)
        {
            writer.AppendLine("patch {");
            writer.AppendLine($"\t# unknown: {Unknown}");
            /*
            writer.AppendLine($"\t# material: {Material}");
            writer.AppendLine($"\t# texture: {Texture}");
            writer.AppendLine($"\t# physics: {Physics}");
            */
            writer.AppendLine($"\t{"res",-8} {Resolution} {Stride}");
            writer.AppendLine($"\t{"priority",-8} 50");
            writer.AppendLine($"\t{"map",-8} 1 1");
            writer.AppendLine($"\t{"tile",-8} {1.0:F6} {1.0:F6}");

            if (Material != 0)
                writer.AppendLine($"\t{"material",-8} {Template.Materials[Material - 1].Name}");
            if (Texture != 0)
                writer.AppendLine($"\t{"texture",-8} {Template.Textures[Texture - 1].Name}");
            if (Physics != 0)
                writer.AppendLine($"\t{"physics",-8} {Template.Physics[Physics - 1].Name}");

            var numVerts = (Resolution * Stride);

            var vx = new short[numVerts];
            var smap = new float[numVerts];
            var tmap = new float[numVerts];
            var norm = new Vector3[numVerts];
            var color = new int[numVerts];

            for (int i = 0; i < numVerts; i++)
            {
                var v = Vertices[i];

                vx[i] = v.Vertex;

                smap[i] = v.SMap;
                tmap[i] = v.TMap;

                norm[i] = v.Normals;

                color[i] = v.Color;
            }

            if (UserData.Length > 0)
                writer.AppendLine($"\tuserprops \"{UserData}\"");

            writer.Append("\tflags { ");
            foreach (var flag in _flagsLookup)
            {
                if ((Flags & (int)flag.Key) != 0)
                    writer.Append($"{flag.Value} ");
            }
            writer.AppendLine("}");

            writer.Append("\tvx\t");
            foreach (var v in vx)
                writer.Append($"{v} ");
            writer.AppendLine();

            writer.Append("\tsmap\n\t\t");
            foreach (var s in smap)
                writer.Append($"{s,9:F6} ");
            writer.AppendLine();

            writer.Append("\ttmap\n\t\t");
            foreach (var t in tmap)
                writer.Append($"{t,9:F6} ");
            writer.AppendLine();

            writer.AppendLine("\tnormals {");
            foreach (var n in norm)
                writer.AppendLine($"\t\t{n.X,9:F6} {n.Y,9:F6} {n.Z,9:F6}");
            writer.AppendLine("\t}");

            writer.AppendLine("}");
        }

        public DLPPatch() { }
        public DLPPatch(DLPFile template, asStream stream)
        {
            Template = template;
            Load(stream);
        }
    }
}
