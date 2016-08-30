using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ARTSManager
{ 
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

        public string FileName { get; set; }

        public List<DLPGroup> Groups { get; set; }
        public List<DLPPatch> Patches { get; set; }
        public List<Vector3> Vertices { get; set; }

        public List<DLPMaterial> Materials { get; set; }
        public List<DLPTexture> Textures { get; set; }
        public List<DLPPhysics> Physics { get; set; }

        public static DLPFile Open(string filename)
        {
            var dlp = new DLPFile(filename);

            dlp.LoadBinary();
            return dlp;
        }

        public void SaveASCII()
        {
            SaveASCII(Path.ChangeExtension(FileName, ".arts.txt"));
        }

        public void SaveASCII(string filename)
        {
            var writer = new StringBuilder();

            /*
                Similar to a GEO3 file, but includes _ALL_ dumped information from the DLP file
            */
            writer.AppendLine($"# ARTS7 {FileName}");
            writer.AppendLine($"# {"Vertices",-14} {Vertices.Count}");
            writer.AppendLine($"# {"Patches",-14} {Patches.Count}");
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
