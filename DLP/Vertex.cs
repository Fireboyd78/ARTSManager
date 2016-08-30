using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARTSManager
{
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

            var norms = new[] {
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
}
