using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;

namespace ARTSManager
{
    // This is the name Angel Studios uses, so I'll continue the tradition here :P
    public abstract class agiRefreshable : asStreamData
    {
        /*
            Abstract class for grouping together all the AGI stuff
        */
    }

    public interface IAGILibraryData
    {
        void LibraryHeader(CsvWriter writer);

        void LibraryLoad(CsvReader reader);
        void LibrarySave(CsvWriter writer);
    }

    public class agiMaterial : agiRefreshable, IAGILibraryData
    {
        public string Name { get; set; }

        public ColorRGBA Emission { get; set; }

        public ColorRGBA Ambient { get; set; }
        public ColorRGBA Diffuse { get; set; }

        public ColorRGBA Specular { get; set; }

        public float Shininess { get; set; }

        public short Reserved { get; set; }
        
        public override void Load(asStream stream)
        {
            Name = stream.GetString(32);

            Emission = new ColorRGBA(stream);

            Ambient = new ColorRGBA(stream);
            Diffuse = new ColorRGBA(stream);

            Specular = new ColorRGBA(stream);

            Shininess = stream.GetFloat();

            Reserved = stream.GetShort();
        }

        public override void Save(asStream stream)
        {
            stream.PutString(Name, 32);

            Emission.CopyTo(stream);

            Ambient.CopyTo(stream);
            Diffuse.CopyTo(stream);

            Specular.CopyTo(stream);

            stream.Put(Shininess);
            stream.Put(Reserved);
        }

        void IAGILibraryData.LibraryHeader(CsvWriter writer)
        {
            writer.WriteField("name");

            writer.WriteField("emisR");
            writer.WriteField("emisG");
            writer.WriteField("emisB");
            writer.WriteField("emisA");

            writer.WriteField("ambR");
            writer.WriteField("ambG");
            writer.WriteField("ambB");
            writer.WriteField("ambA");

            writer.WriteField("difR");
            writer.WriteField("difG");
            writer.WriteField("difB");
            writer.WriteField("difA");

            writer.WriteField("specR");
            writer.WriteField("specG");
            writer.WriteField("specB");
            writer.WriteField("specA");

            writer.WriteField("shininess");
            writer.WriteField("reserved");

            writer.NextRecord();
        }

        void IAGILibraryData.LibraryLoad(CsvReader reader)
        {
            Name = reader.GetField<string>("name");

            Emission = new ColorRGBA() {
                iR = reader.GetField<int>("emisR"),
                iG = reader.GetField<int>("emisG"),
                iB = reader.GetField<int>("emisB"),
                iA = reader.GetField<int>("emisA"),
            };

            Ambient = new ColorRGBA() {
                iR = reader.GetField<int>("ambR"),
                iG = reader.GetField<int>("ambG"),
                iB = reader.GetField<int>("ambB"),
                iA = reader.GetField<int>("ambA"),
            };

            Diffuse = new ColorRGBA() {
                iR = reader.GetField<int>("difR"),
                iG = reader.GetField<int>("difG"),
                iB = reader.GetField<int>("difB"),
                iA = reader.GetField<int>("difA"),
            };

            Specular = new ColorRGBA() {
                iR = reader.GetField<int>("specR"),
                iG = reader.GetField<int>("specG"),
                iB = reader.GetField<int>("specB"),
                iA = reader.GetField<int>("specA"),
            };

            Shininess = reader.GetField<float>("shininess");
            Reserved = reader.GetField<short>("reserved");
        }

        void IAGILibraryData.LibrarySave(CsvWriter writer)
        {
            writer.WriteField(Name);

            writer.WriteField(Emission.iR);
            writer.WriteField(Emission.iG);
            writer.WriteField(Emission.iB);
            writer.WriteField(Emission.iA);

            writer.WriteField(Ambient.iR);
            writer.WriteField(Ambient.iG);
            writer.WriteField(Ambient.iB);
            writer.WriteField(Ambient.iA);

            writer.WriteField(Diffuse.iR);
            writer.WriteField(Diffuse.iG);
            writer.WriteField(Diffuse.iB);
            writer.WriteField(Diffuse.iA);

            writer.WriteField(Specular.iR);
            writer.WriteField(Specular.iG);
            writer.WriteField(Specular.iB);
            writer.WriteField(Specular.iA);

            writer.WriteField(Shininess);
            writer.WriteField(Reserved);

            writer.NextRecord();
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

    public class agiTexture : agiRefreshable, IAGILibraryData
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

            /* flags I'm not sure about yet:
                - opaque
                - decal
                - envmap
            */
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

        void IAGILibraryData.LibraryHeader(CsvWriter writer)
        {
            writer.WriteField("name");

            writer.WriteField("flags");

            writer.WriteField("unk1");
            writer.WriteField("unk2");

            writer.NextRecord();
        }

        void IAGILibraryData.LibraryLoad(CsvReader reader)
        {
            Name = reader.GetField<string>("name");

            Flags = reader.GetField<byte>("flags");

            Unknown1 = reader.GetField<byte>("unk1");
            Unknown2 = reader.GetField<byte>("unk2");
        }

        void IAGILibraryData.LibrarySave(CsvWriter writer)
        {
            writer.WriteField(Name);

            writer.WriteField(Flags);

            writer.WriteField(Unknown1);
            writer.WriteField(Unknown2);

            writer.NextRecord();
        }

        public agiTexture() { }
        public agiTexture(asStream stream)
        {
            Load(stream);
        }
    }

    public class agiPhysics : agiRefreshable, IAGILibraryData
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

        void IAGILibraryData.LibraryHeader(CsvWriter writer)
        {
            writer.WriteField("name");

            writer.WriteField("friction");
            writer.WriteField("elasticity");
            writer.WriteField("drag");

            writer.WriteField("bumpheight");
            writer.WriteField("bumpwidth");

            writer.WriteField("sinkdepth");
            writer.WriteField("ptxrate");

            writer.WriteField("type");
            writer.WriteField("sound");

            writer.WriteField("velX");
            writer.WriteField("velY");

            writer.WriteField("ptxcolorR");
            writer.WriteField("ptxcolorG");
            writer.WriteField("ptxcolorB");

            writer.NextRecord();
        }

        void IAGILibraryData.LibraryLoad(CsvReader reader)
        {
            Name = reader.GetField<string>("name");

            Friction = reader.GetField<float>("friction");
            Elasticity = reader.GetField<float>("elasticity");
            Drag = reader.GetField<float>("drag");

            BumpHeight = reader.GetField<float>("bumpheight");
            BumpWidth = reader.GetField<float>("bumpwidth");

            SinkDepth = reader.GetField<float>("sinkdepth");
            PtxRate = reader.GetField<float>("ptxrate");

            Type = reader.GetField<int>("type");
            Sound = reader.GetField<int>("sound");

            Velocity = new Vector2() {
                X = reader.GetField<float>("velX"),
                Y = reader.GetField<float>("velY")
            };

            PtxColor = new ColorRGB() {
                iR = reader.GetField<int>("ptxcolorR"),
                iG = reader.GetField<int>("ptxcolorG"),
                iB = reader.GetField<int>("ptxcolorB"),
            };
        }

        void IAGILibraryData.LibrarySave(CsvWriter writer)
        {
            writer.WriteField(Name);

            writer.WriteField(Friction);
            writer.WriteField(Elasticity);
            writer.WriteField(Drag);

            writer.WriteField(BumpHeight);
            writer.WriteField(BumpWidth);

            writer.WriteField(SinkDepth);
            writer.WriteField(PtxRate);

            writer.WriteField(Type);
            writer.WriteField(Sound);

            writer.WriteField(Velocity.X);
            writer.WriteField(Velocity.Y);

            writer.WriteField(PtxColor.iR);
            writer.WriteField(PtxColor.iG);
            writer.WriteField(PtxColor.iB);

            writer.NextRecord();
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
}
