using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARTSManager
{
    public class DLPMaterial : agiMaterial
    {
        public override void Print(StringBuilder writer)
        {
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

        public DLPMaterial() { }
        public DLPMaterial(asStream stream)
        {
            Load(stream);
        }
    }
}
