using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARTSManager
{
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
}
