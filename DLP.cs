using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ARTSManager
{
    using FlagsLookup = Dictionary<PatchFlags, string>;

    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public void CopyTo(asStream stream)
        {
            stream.Put(X);
            stream.Put(Y);
        }

        public Vector2(asStream stream)
        {
            X = stream.GetFloat();
            Y = stream.GetFloat();
        }
    }

    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public void CopyTo(asStream stream)
        {
            stream.Put(X);
            stream.Put(Y);
            stream.Put(Z);
        }

        public Vector3(asStream stream)
        {
            X = stream.GetFloat();
            Y = stream.GetFloat();
            Z = stream.GetFloat();
        }
    }

    public struct ColorRGB
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }

        public void CopyTo(asStream stream)
        {
            stream.Put(R);
            stream.Put(G);
            stream.Put(B);
        }

        public ColorRGB(asStream stream)
        {
            R = stream.GetFloat();
            G = stream.GetFloat();
            B = stream.GetFloat();
        }
    }

    public struct ColorRGBA
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }
        
        public void CopyTo(asStream stream)
        {
            stream.Put(R);
            stream.Put(G);
            stream.Put(B);
            stream.Put(A);
        }

        public ColorRGBA(asStream stream)
        {
            R = stream.GetFloat();
            G = stream.GetFloat();
            B = stream.GetFloat();
            A = stream.GetFloat();
        }
    }

    public abstract class asStreamData
    {
        public virtual void Load(asStream stream)
        {
            return;
        }

        public virtual void Save(asStream stream)
        {
            return;
        }

        public virtual void Print(StringBuilder writer)
        {
            return;
        }
    }

    public class agiMaterial : asStreamData
    {
        public string Name { get; set; }

        public ColorRGBA Emission { get; set; }

        public ColorRGBA Ambient { get; set; }
        public ColorRGBA Diffuse { get; set; }

        public ColorRGBA Specular { get; set; }

        public float Shininess { get; set; }

        public short Unknown { get; set; }

        public override void Load(asStream stream)
        {
            Name = stream.GetString(32);

            Emission = new ColorRGBA(stream);

            Ambient = new ColorRGBA(stream);
            Diffuse = new ColorRGBA(stream);

            Specular = new ColorRGBA(stream);

            Shininess = stream.GetFloat();

            Unknown = stream.GetShort();
        }

        public override void Save(asStream stream)
        {
            stream.PutString(Name, 32);

            Emission.CopyTo(stream);

            Ambient.CopyTo(stream);
            Diffuse.CopyTo(stream);

            Specular.CopyTo(stream);

            stream.Put(Shininess);
            stream.Put(Unknown);
        }

        public override void Print(StringBuilder writer)
        {
            /*===============================================
               Temporary print method copied from DLPMaterial
              ===============================================*/
            var writeParam = new Action<string, ColorRGBA>((name, param) => {
                writer.AppendLine($"\t{name,-16} {param.R,-12:F3} {param.G,-12:F3} {param.B,-12:F3} {param.A,-12:F3}");
            });

            writer.AppendLine($"material {Name} {{");
            writeParam("emission", Emission);
            writeParam("ambient", Ambient);
            writeParam("diffuse", Diffuse);
            writeParam("specular", Specular);
            writer.AppendLine($"\t{"shininess",-16} {Shininess,-12:F2}");

            writer.AppendLine("}");
        }

        public agiMaterial() { }
        public agiMaterial(asStream stream)
        {
            Load(stream);
        }
    }

    public class agiTexture : asStreamData
    {
        public string Name { get; set; }

        public byte Flags { get; set; }

        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }

        public override void Load(asStream stream)
        {
            Name = stream.GetString(32);

            Flags = stream.GetByte();

            Unknown1 = stream.GetByte();
            Unknown2 = stream.GetByte();

            // ignore alignment
            stream.Skip(1);
        }

        public override void Save(asStream stream)
        {
            stream.PutString(Name, 32);

            stream.Put(Flags);

            stream.Put(Unknown1);
            stream.Put(Unknown2);

            // alignment
            stream.Put((byte)0);
        }
        
        public override void Print(StringBuilder writer)
        {
            /*===============================================
               Temporary print method copied from DLPTexture
              ===============================================*/
            writer.AppendLine($"texture {Name} {{");
            writer.AppendLine($"\tsource \"..\\tex\\{Name.ToLower()}.tif\"");
            writer.Append($"\t");

            if ((Flags & 1) != 0)
                writer.Append("color ");
            if ((Flags & 2) != 0)
                writer.Append("swrap ");
            if ((Flags & 4) != 0)
                writer.Append("twrap ");

            // not sure how many possible flags there are
            if ((Flags & 8) != 0)
                writer.Append("FLAG_8 ");
            if ((Flags & 16) != 0)
                writer.Append("FLAG_16 ");
            if ((Flags & 32) != 0)
                writer.Append("FLAG_32 ");
            if ((Flags & 64) != 0)
                writer.Append("FLAG_64 ");
            if ((Flags & 128) != 0)
                writer.Append("FLAG_128 ");

            writer.AppendLine("\n}");
        }

        public agiTexture() { }
        public agiTexture(asStream stream)
        {
            Load(stream);
        }
    }

    public class agiPhysics : asStreamData
    {
        public string Name { get; set; }

        public float Friction { get; set; }
        public float Elasticity { get; set; }
        public float Drag { get; set; }

        public float BumpHeight { get; set; }
        public float BumpWidth { get; set; }

        public float SinkDepth { get; set; }
        public float PtxRate { get; set; }

        public int Type { get; set; }
        public int Sound { get; set; }

        public Vector2 Velocity { get; set; }
        public ColorRGB PtxColor { get; set; }

        public override void Load(asStream stream)
        {
            Name = stream.GetString(32);

            Friction = stream.GetFloat();
            Elasticity = stream.GetFloat();
            Drag = stream.GetFloat();

            BumpHeight = stream.GetFloat();
            BumpWidth = stream.GetFloat();

            SinkDepth = stream.GetFloat();
            PtxRate = stream.GetFloat();

            Type = stream.GetInt();
            Sound = stream.GetInt();

            Velocity = new Vector2(stream);
            PtxColor = new ColorRGB(stream);
        }

        public override void Save(asStream stream)
        {
            stream.PutString(Name, 32);

            stream.Put(Friction);
            stream.Put(Elasticity);
            stream.Put(Drag);

            stream.Put(BumpHeight);
            stream.Put(BumpWidth);

            stream.Put(SinkDepth);
            stream.Put(PtxRate);

            stream.Put(Type);
            stream.Put(Sound);

            Velocity.CopyTo(stream);
            PtxColor.CopyTo(stream);
        }

        public override void Print(StringBuilder writer)
        {
            /*===============================================
               Temporary print method copied from DLPPhysics
              ===============================================*/
            var writeFloatParam = new Action<string, float>((name, param) => {
                writer.AppendLine($"\t{name,-10} {param,12:F3}");
            });

            var writeIntParam = new Action<string, float>((name, param) => {
                writer.AppendLine($"\t{name,-5} {param}");
            });

            writer.AppendLine($"physics {Name} {{");

            writeFloatParam("friction", Friction);
            writeFloatParam("elasticity", Elasticity);
            writeFloatParam("drag", Drag);

            writeFloatParam("bumpheight", BumpHeight);
            writeFloatParam("bumpwidth", BumpWidth);

            writeFloatParam("sinkdepth", SinkDepth);
            writeFloatParam("ptxrate", PtxRate);

            writeIntParam("type", Type);
            writeIntParam("sound", Sound);

            writer.AppendLine($"\t{"velocity",-10} {Velocity.X,12:F3} {Velocity.Y,12:F3}");
            writer.AppendLine($"\t{"ptxcolor",-10} {PtxColor.R,12:F3} {PtxColor.G,12:F3} {PtxColor.B,12:F3}");

            writer.AppendLine("}");
        }

        public agiPhysics() { }
        public agiPhysics(asStream stream)
        {
            Load(stream);
        }
    }

    public abstract class DLPData : asStreamData
    {
        protected DLPFile Template { get; set; }
    }

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
            var patchesCount = (Patches != null) ? Vertices.Length : 0;

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

    [Flags]
    public enum PatchFlags : int
    {
        CPV             = (1 << 0),
        Emission        = (1 << 1),
        Shade           = (1 << 2),
        Solid           = (1 << 3),
        Cull            = (1 << 4),
        ZWrite          = (1 << 5),
        ZRead           = (1 << 6),

        Shadow          = (1 << 7),

        Flat            = (1 << 8),
        AntiAlias       = (1 << 9),
        Interpenetrate  = (1 << 10),
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

    public class DLPVertex : DLPData
    {
        public short Vertex { get; set; }

        public Vector3 Normals { get; set; }

        public float SMap { get; set; }
        public float TMap { get; set; }

        public int Color { get; set; }

        public override void Load(asStream stream)
        {
            Vertex = stream.GetShort();

            Normals = new Vector3() {
                X = stream.GetFloat(),
                Y = stream.GetFloat(),
                Z = stream.GetFloat()
            };

            SMap = stream.GetFloat();
            TMap = stream.GetFloat();

            Color = stream.GetInt();
        }

        public override void Save(asStream stream)
        {
            stream.Put(Vertex);

            var norms = new [] {
                Normals.X,
                Normals.Y,
                Normals.Z
            };

            stream.Put(norms, 3);

            stream.Put(SMap);
            stream.Put(TMap);

            stream.Put(Color);
        }

        public DLPVertex() { }
        public DLPVertex(asStream stream)
        {
            Load(stream);
        }
    }
    
    public class DLPMaterial : agiMaterial
    {
        public override void Print(StringBuilder writer)
        {
            var writeParam = new Action<string, ColorRGBA>((name, param) => {
                writer.AppendLine($"\t{name, -16} {param.R,-12:F3} {param.G,-12:F3} {param.B,-12:F3} {param.A,-12:F3}");
            });

            writer.AppendLine($"material {Name} {{");
            writeParam("emission", Emission);
            writeParam("ambient", Ambient);
            writeParam("diffuse", Diffuse);
            writeParam("specular", Specular);
            writer.AppendLine($"\t{"shininess",-16} {Shininess,-12:F2}");

            writer.AppendLine("}");
        }

        public DLPMaterial() { }
        public DLPMaterial(asStream stream)
        {
            Load(stream);
        }
    }

    public class DLPTexture : agiTexture
    {
        public override void Print(StringBuilder writer)
        {
            writer.AppendLine($"texture {Name} {{");
            writer.AppendLine($"\tsource \"..\\tex\\{Name.ToLower()}.tif\"");
            writer.Append($"\t");

            if ((Flags & 1) != 0)
                writer.Append("color ");
            if ((Flags & 2) != 0)
                writer.Append("swrap ");
            if ((Flags & 4) != 0)
                writer.Append("twrap ");

            // not sure how many possible flags there are
            if ((Flags & 8) != 0)
                writer.Append("FLAG_8 ");
            if ((Flags & 16) != 0)
                writer.Append("FLAG_16 ");
            if ((Flags & 32) != 0)
                writer.Append("FLAG_32 ");
            if ((Flags & 64) != 0)
                writer.Append("FLAG_64 ");
            if ((Flags & 128) != 0)
                writer.Append("FLAG_128 ");

            writer.AppendLine("\n}");
        }

        public DLPTexture() { }
        public DLPTexture(asStream stream)
        {
            Load(stream);
        }
    }

    public class DLPPhysics : agiPhysics
    {
        public override void Print(StringBuilder writer)
        {
            var writeFloatParam = new Action<string, float>((name, param) => {
                writer.AppendLine($"\t{name,-10} {param,12:F3}");
            });

            var writeIntParam = new Action<string, float>((name, param) => {
                writer.AppendLine($"\t{name,-5} {param}");
            });

            writer.AppendLine($"physics {Name} {{");

            writeFloatParam("friction", Friction);
            writeFloatParam("elasticity", Elasticity);
            writeFloatParam("drag", Drag);

            writeFloatParam("bumpheight", BumpHeight);
            writeFloatParam("bumpwidth", BumpWidth);

            writeFloatParam("sinkdepth", SinkDepth);
            writeFloatParam("ptxrate", PtxRate);

            writeIntParam("type", Type);
            writeIntParam("sound", Sound);

            writer.AppendLine($"\t{"velocity",-10} {Velocity.X,12:F3} {Velocity.Y,12:F3}");
            writer.AppendLine($"\t{"ptxcolor",-10} {PtxColor.R,12:F3} {PtxColor.G,12:F3} {PtxColor.B,12:F3}");

            writer.AppendLine("}");
        }

        public DLPPhysics() { }
        public DLPPhysics(asStream stream)
        {
            Load(stream);
        }
    }
    
    public class DLPFile
    {
        // could be useful for DLP->GEO conversions
        internal static readonly Dictionary<int, string> GEO3SortOrder = new Dictionary<int, string> {
            { 6, "active" },
            { 7, "group" },
            { 8, "vx" },
            { 9, "point" },
            { 10, "curve" },
            { 11, "facet" },
            { 12, "patch" },
            { 13, "res" },
            { 14, "priority" },
            { 15, "material" },
            { 16, "texture" },
            { 17, "physics" },
            { 18, "map" },
            { 19, "tension" },
            { 20, "smap" },
            { 21, "tmap" },
            { 22, "tile" },
            { 23, "offset" },
            { 24, "rotation" },
            { 25, "random" },
            { 26, "bulge" },
            { 27, "flags" },
            { 28, "cpv" },
            { 29, "emission" },
            { 30, "shade" },
            { 31, "solid" },
            { 32, "cull" },
            { 33, "zread" },
            { 34, "zwrite" },
            { 35, "flat" },
            { 36, "antialias" },
            { 37, "interpenetrate" },
            { 38, "userprops" },
            { 39, "normals" },
            { 40, "color" },
            { 41, "ambient" },
            { 42, "diffuse" },
            { 43, "specular" },
            { 44, "alpha" },
            { 45, "shininess" },
            { 46, "emult" },
            { 47, "source" },
            { 48, "opaque" },
            { 49, "swrap" },
            { 50, "twrap" },
            { 51, "decal" },
            { 52, "envmap" },
        };

        protected asStream Stream { get; set; }
        
        public List<DLPGroup> Groups { get; set; }
        public List<DLPPatch> Patches { get; set; }
        public List<Vector3> Vertices { get; set; }

        public List<DLPMaterial> Materials { get; set; }
        public List<DLPTexture> Textures { get; set; }
        public List<DLPPhysics> Physics { get; set; }

        public string FileName { get; set; }

        public static DLPFile Open(string filename)
        {
            var dlp = new DLPFile(filename);

            dlp.LoadBinary();
            return dlp;
        }

        public void WriteLog()
        {
            WriteLog(Path.ChangeExtension(FileName, ".log"));
        }

        public void WriteLog(string filename)
        {
            var writer = new StringBuilder();

            writer.AppendLine($"# ARTS7 {FileName}");
            writer.AppendLine($"# {"Vertices", -14} {Vertices.Count}");
            writer.AppendLine($"# {"Patches", -14} {Patches.Count}");
            writer.AppendLine($"# {"Groups",-14} {Groups.Count}");
            writer.AppendLine($"# {"Materials",-14} {Materials.Count}");
            writer.AppendLine($"# {"Textures",-14} {Textures.Count}");
            writer.AppendLine($"# {"Physics",-14} {Physics.Count}");
            writer.AppendLine();
            
            foreach (var material in Materials)
            {
                material.Print(writer);
                writer.AppendLine();
            }

            foreach (var texture in Textures)
            {
                texture.Print(writer);
                writer.AppendLine();
            }

            foreach (var physics in Physics)
            {
                physics.Print(writer);
                writer.AppendLine();
            }

            foreach (var vertex in Vertices)
                writer.AppendLine($"vx {vertex.X,12:F6} {vertex.Y,12:F6} {vertex.Z,12:F6}");

            writer.AppendLine();

            foreach (var patch in Patches)
            {
                patch.Print(writer);
                writer.AppendLine();
            }

            foreach (var group in Groups)
            {
                group.Print(writer);
                writer.AppendLine();
            }

            writer.AppendLine("# EOF");

            File.WriteAllText(filename, writer.ToString());
        }

        protected void LoadBinary()
        {
            /*
              Header
            */
            var sig = Stream.GetInt();

            if ((sig >> 8) != 0x444C50)
                throw new InvalidOperationException("Unknown DLP file.");

            var version = (sig & 0x0F);

            if (version != 7)
                throw new InvalidOperationException($"Unknown DLP version ({version}).");

            var numGroups = Stream.GetInt();
            var numPatches = Stream.GetInt();
            var numVertices = Stream.GetInt();

            Console.WriteLine($"  Groups: {numGroups}");
            Console.WriteLine($"  Patches: {numPatches}");
            Console.WriteLine($"  Vertices: {numVertices}");

            /*
              Groups
            */
            Groups = new List<DLPGroup>(numGroups);

            for (int i = 0; i < numGroups; i++)
            {
                var group = new DLPGroup(Stream);
                Groups.Add(group);
            }

            /*
              Patches
            */
            Patches = new List<DLPPatch>(numPatches);

            for (int i = 0; i < numPatches; i++)
            {
                var patch = new DLPPatch(this, Stream);
                Patches.Add(patch);
            }

            /*
              Vertices
            */
            Vertices = new List<Vector3>(numVertices);

            for (int i = 0; i < numVertices; i++)
            {
                var vx = new Vector3() {
                    X = Stream.GetFloat(),
                    Y = Stream.GetFloat(),
                    Z = Stream.GetFloat()
                };
                Vertices.Add(vx);
            }

            /*
              Materials
            */
            var numMaterials = Stream.GetInt();

            if (numMaterials > 0)
                Console.WriteLine($"  Materials: {numMaterials}");

            Materials = new List<DLPMaterial>(numMaterials);

            for (int i = 0; i < numMaterials; i++)
            {
                var material = new DLPMaterial(Stream);
                Materials.Add(material);
            }

            /*
              Textures
            */
            var numTextures = Stream.GetInt();

            if (numTextures > 0)
                Console.WriteLine($"  Textures: {numTextures}");

            Textures = new List<DLPTexture>(numTextures);

            for (int i = 0; i < numTextures; i++)
            {
                var texture = new DLPTexture(Stream);
                Textures.Add(texture);
            }

            /*
              Physics
            */
            var numPhysics = Stream.GetInt();

            if (numPhysics > 0)
                Console.WriteLine($"  Physics: {numPhysics}");

            Physics = new List<DLPPhysics>(numPhysics);

            for (int i = 0; i < numPhysics; i++)
            {
                var physics = new DLPPhysics(Stream);
                Physics.Add(physics);
            }

            Console.WriteLine();

            // Done!
            Console.WriteLine($"  Finished reading file up to 0x{Stream.Tell():X}.\r\n");
        }

        protected DLPFile()
        {
            Groups = new List<DLPGroup>();
        }

        protected DLPFile(string filename)
        {
            FileName = filename;
            Stream = new asStream(filename);
        }
    }
}
