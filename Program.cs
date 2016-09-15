using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;

namespace ARTSManager
{
    class Program
    {
        public enum IlluminationModel : int
        {
            Color = 0,
            ColorAmbient = 1,
            Highlight = 2,
            ReflectionRayTrace = 3,
            GlassRayTrace = 4,
            FresnelRayTrace = 5,
            RefractionRayTrace = 6,
            RefractionRayTraceFresnel = 7,
            Reflection = 8,
            Glass = 9,
            InvisibleSurfaceShadows = 10
        }

        public enum ComponentType
        {
            Ambient,
            Diffuse,
            Specular
        }

        public sealed class ObjMaterial
        {
            public string Name { get; set; }

            public Dictionary<string, string> Properties { get; private set; }

            public string this[string key]
            {
                get { return (Properties.ContainsKey(key)) ? Properties[key] : null; }
                set
                {
                    if (!Properties.ContainsKey(key))
                    {
                        Properties.Add(key, value);
                    }
                    else
                    {
                        Properties[key] = value;
                    }
                }
            }

            public void Save(StringBuilder mtl)
            {
                mtl.AppendLine($"newmtl {Name}");

                foreach (KeyValuePair<string, string> kv in Properties)
                {
                    if (kv.Value != null)
                        mtl.AppendLine($"\t{kv.Key} {kv.Value}");
                }

                mtl.AppendLine();
            }
            
            private void SetColor(string colorKey, ColorRGBA color)
            {
                Properties[colorKey] = String.Format("{0:F4} {1:F4} {2:F4}",
                    Math.Max(0.0, Math.Min(color.R, 1.0)),
                    Math.Max(0.0, Math.Min(color.G, 1.0)),
                    Math.Max(0.0, Math.Min(color.B, 1.0)));
            }

            private void SetMap(string mapKey, string map)
            {
                Properties[mapKey] = map;
            }

            private void SetColorAndMap(string colorKey, string mapKey, ColorRGBA color, string map)
            {
                SetColor(colorKey, color);
                SetMap(mapKey, map);
            }

            public void SetComponent(ComponentType componentType, ColorRGBA color, string map = null)
            {
                switch (componentType)
                {
                case ComponentType.Ambient: SetColorAndMap("Ka", "map_Ka", color, map); break;
                case ComponentType.Diffuse: SetColorAndMap("Kd", "map_Kd", color, map); break;
                case ComponentType.Specular: SetColorAndMap("Ks", "map_Ks", color, map); break;
                }
            }

            public void SetComponentColor(ComponentType componentType, ColorRGBA color)
            {
                switch (componentType)
                {
                case ComponentType.Ambient: SetColor("Ka", color); break;
                case ComponentType.Diffuse: SetColor("Kd", color); break;
                case ComponentType.Specular: SetColor("Ks", color); break;
                }
            }

            public void SetComponentMap(ComponentType componentType, string map)
            {
                switch (componentType)
                {
                case ComponentType.Ambient: SetMap("map_Ka", map); break;
                case ComponentType.Diffuse: SetMap("map_Kd", map); break;
                case ComponentType.Specular: SetMap( "map_Ks", map); break;
                }
            }

            public void SetOpacity(double opacity, string map = null)
            {
                Properties["d"] = String.Format("{0:F1}", Math.Max(0.0, Math.Min(opacity, 1.0)));

                if (map != null)
                    Properties["map_d"] = map;
            }

            private string _spec
            {
                get { return Properties["Ns"]; }
                set { Properties["Ns"] = value; }
            }

            public double Specularity
            {
                get { return (_spec != null) ? double.Parse(_spec) : 0.0; }
                set { _spec = String.Format("{0:F4}", Math.Max(0.0, Math.Min(value, 1000.0))); }
            }

            public string DiffuseMap
            {
                get
                {
                    if (Properties.ContainsKey("map_Kd"))
                        return Properties["map_Kd"];

                    return null;
                }
            }

            public ObjMaterial(string name)
            {
                Name = name;
                Properties = new Dictionary<string, string>() {
                    { "Ns",         "0.0000"                },
                    { "Ni",         "1.5000"                },
                    { "d",          "1.0"                   },
                    { "Tr",         "0.0000"                },
                    { "Tf",         "1.0000 1.0000 1.0000"  },
                    { "illum",      "2"                     },
                    { "Ka",         "1.0000 1.0000 1.0000"  },
                    { "Kd",         "1.0000 1.0000 1.0000"  },
                    { "Ks",         "0.0000 0.0000 0.0000"  },
                    { "map_Ka",     null                    },
                    { "map_Kd",     null                    },
                    { "map_Ks",     null                    },
                    { "map_d",      null                    },
                };
            }
        }
        
        static List<T> ParseDB<T>(string filename, string libName, string entriesName)
            where T : agiRefreshable, IAGILibraryData, new()
        {
            var stream = new asStream(filename);

            Console.WriteLine($"Parsing DB: \"{filename}\"");

            var numEntries = stream.GetInt();            
            var entries = new List<T>(numEntries);

            for (int i = 0; i < numEntries; i++)
            {
                var entry = new T();
                entry.Load(stream);

                entries.Add(entry);
            }

            //var writer = new StringBuilder();
            //
            //writer.AppendLine($"# {libName}");
            //writer.AppendLine($"# {entriesName,-12} {entries.Count}");
            //writer.AppendLine();

            //foreach (var entry in entries)
            //{
            //    entry.Print(writer);
            //    writer.AppendLine();
            //}

            //File.WriteAllText(Path.ChangeExtension(filename, ".log"), writer.ToString());

            using (var file = new StreamWriter(Path.ChangeExtension(filename, ".csv")))
            using (var writer = new CsvWriter(file))
            {
                var writeHeader = true;

                foreach (var entry in entries)
                {
                    if (writeHeader)
                    {
                        entry.LibraryHeader(writer);
                        writeHeader = false;
                    }

                    entry.LibrarySave(writer);
                }

                file.Flush();
            }
            
            return entries;
        }

        static void ConvertDLPToOBJ(DLPFile dlp)
        {
            if (dlp == null)
                throw new InvalidOperationException("Cannot convert a null DLP object to OBJ!");

            var rootDir = Path.GetFullPath($"{Path.GetDirectoryName(dlp.FileName)}.\\..\\");

            var objPath = Path.ChangeExtension(dlp.FileName, ".obj");
            var mtlPath = Path.ChangeExtension(dlp.FileName, ".mtl");

            var obj = new StringBuilder();
            var mtl = new StringBuilder();

            obj.AppendLine($"# DLP converted to OBJ using ARTS Manager");
            obj.AppendLine($"# Source: {dlp.FileName}");
            obj.AppendLine();

            obj.AppendLine($"# Vertices: {dlp.Vertices.Count}");
            obj.AppendLine($"# Patches: {dlp.Patches.Count}");
            obj.AppendLine($"# Groups: {dlp.Groups.Count}");
            obj.AppendLine();

            obj.AppendLine($"mtllib {Path.GetFileName(mtlPath)}");
            obj.AppendLine();

            mtl.AppendLine($"# DLP7 materials");
            mtl.AppendLine();

            var vxIndex = 0; // increases when we add a new vertex
            var minIndex = 0; // for normals/uvs

            var materialLookup = new Dictionary<string, ObjMaterial>();
            var materials = new Dictionary<string, ObjMaterial>();

            foreach (var group in dlp.Groups)
            {
                var objF = new StringBuilder();

                var objVx = new StringBuilder();
                var objVn = new StringBuilder();
                var objVt = new StringBuilder();
                
                var vBuf = new Dictionary<int, int>();

                var curMtl = 0;
                var curTex = 0;

                foreach (var p in group.Patches)
                {
                    var patch = dlp.Patches[p];

                    if (patch.Stride != 1)
                        continue;
                    if ((patch.Resolution < 3) || (patch.Resolution > 4))
                        continue;
                    
                    if ((patch.Material != curMtl) || (patch.Texture != curTex))
                    {
                        curMtl = patch.Material ;
                        curTex = patch.Texture;

                        var mat = (curMtl != 0) ? dlp.Materials[curMtl - 1] : null;
                        var tex = (curTex != 0) ? dlp.Textures[curTex - 1] : null;

                        var materialName = "";

                        if ((mat != null) && (tex != null))
                        {
                            materialName = $"{mat.Name}|{tex.Name}";
                        }
                        else
                        {
                            materialName = (mat != null) ? mat.Name : (tex != null) ? tex.Name : "";
                        }

                        if (String.IsNullOrEmpty(materialName))
                            throw new InvalidOperationException("apparently this patch doesn't have any clothes on...");

                        if (!materialLookup.ContainsKey(materialName))
                        {
                            var material = new ObjMaterial(materialName);
                            var texture = (tex != null) ? tex.Name : null;

                            var hasAlpha = false;

                            if (texture != null)
                            {
                                var tex16OPath = Path.Combine(rootDir, $"TEX16O\\{texture}.DDS");
                                var tex16APath = Path.Combine(rootDir, $"TEX16A\\{texture}.DDS");

                                var hasTex16O = File.Exists(tex16OPath);
                                var hasTex16A = File.Exists(tex16APath);

                                texture = (hasTex16O) ? tex16OPath : ((hasAlpha = hasTex16A)) ? tex16APath : $"{texture}.DDS";
                            }

                            if (mat != null)
                            {
                                material.SetComponentColor(ComponentType.Ambient, mat.Ambient);
                                material.SetComponentColor(ComponentType.Diffuse, mat.Diffuse);
                                material.SetComponentColor(ComponentType.Specular, mat.Specular);

                                //material.Specularity = mat.Shininess;
                            }

                            if (tex != null)
                            {
                                // maybe have a thing that checks if its 3ds max?
                                //material.SetComponentMap(ComponentType.Ambient, texture);
                                material.SetComponentMap(ComponentType.Diffuse, texture);

                                if (hasAlpha)
                                {
                                    material.Properties["d"] = "0.0000";
                                    material.Properties["map_d"] = texture;
                                }
                            }

                            // compile
                            material.Save(mtl);

                            materialLookup.Add(materialName, material);
                            materials.Add(materialName, material);
                        }

                        objF.AppendLine($"usemtl {materialLookup[materialName].Name}");
                    }
                    
                    // open face
                    objF.Append("f ");

                    for (int i = 0; i < patch.Resolution; i++)
                    {
                        var v = patch.Vertices[i];

                        int vx = v.Vertex;

                        var vt1 = v.SMap;
                        var vt2 = (-v.TMap - 1);

                        var vn = v.Normals;

                        if (!vBuf.ContainsKey(vx))
                        {
                            var vertex = dlp.Vertices[vx];

                            objVx.AppendLine($"v {vertex.X:F6} {vertex.Y:F6} {vertex.Z:F6}");

                            vBuf.Add(vx, vxIndex++);
                        }

                        // translate to index in buffer
                        vx = vBuf[vx];

                        objVt.AppendLine($"vt {vt1:F6} {vt2:F6} 1.000000");
                        objVn.AppendLine($"vn {vn.X:F6} {vn.Y:F6} {vn.Z:F6}");
                        
                        // append face
                        objF.AppendFormat("{0}/{1}/{1} ", (vx + 1), (i + minIndex + 1));
                    }

                    minIndex += patch.Resolution;

                    // move to next face
                    objF.AppendLine();
                }

                // commit changes, move along
                obj.AppendLine($"o {group.Name}");
                obj.AppendLine(objVx.ToString());
                obj.AppendLine(objVn.ToString());
                obj.AppendLine(objVt.ToString());
                
                obj.AppendLine($"g {group.Name}");
                obj.AppendLine(objF.ToString());
            }
            
            File.WriteAllText(objPath, obj.ToString());
            File.WriteAllText(mtlPath, mtl.ToString());
        }

        static void Main(string[] args)
        {
            var directories = new[] {
                @"C:\Dev\Research\MM1\",
                //@"C:\Dev\Research\MM1\midvwtrial\",
            };

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                if (args.Length == 0)
                {
                    directories[0] = Environment.CurrentDirectory;
                }
                else
                {
                    directories = new string[args.Length];

                    for (int i = 0; i < args.Length; i++)
                        directories[i] = args[i];
                }
            }
            
            var nFiles = 0;

            foreach (var directory in directories)
            {
                var dlpDir = Path.Combine(directory, "DLP");
                var mtlDir = Path.Combine(directory, "MTL");

                if (Directory.Exists(mtlDir))
                {
                    Console.WriteLine("Parsing MTLs...");

                    ParseDB<agiMaterial>(Path.Combine(mtlDir, "MATERIAL.DB"), "Material library", "Materials");
                    ParseDB<agiTexture>(Path.Combine(mtlDir, "TEXTURE.DB"), "Texture library", "Textures");
                    ParseDB<agiPhysics>(Path.Combine(mtlDir, "PHYSICS.DB"), "Physics library", "Physics");
                }

                if (Directory.Exists(dlpDir))
                {
                    Console.WriteLine("Parsing DLPs...");

                    foreach (var filename in Directory.EnumerateFiles(dlpDir, "*.DLP", SearchOption.AllDirectories))
                    {
                        Console.WriteLine($"[{nFiles + 1}] {filename}");

                        var dlp = DLPFile.Open(filename);

                        // write an ASCII file describing the DLP
                        dlp.SaveASCII();

                        // convert to obj
                        ConvertDLPToOBJ(dlp);

                        nFiles += 1;
                    }
                }
            }

            if (nFiles == 0)
            {
                Console.WriteLine("No DLP files found to process.");
            }
            else
            {
                Console.WriteLine($"Finished parsing {nFiles} DLP files.");
            }


            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Process complete. Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
