using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARTSManager
{
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
}
