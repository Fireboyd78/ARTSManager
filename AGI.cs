using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARTSManager
{
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
}
