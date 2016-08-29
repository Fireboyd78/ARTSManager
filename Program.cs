using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ARTSManager
{
    class Program
    {
        static List<T> ParseDB<T>(string filename, string libName, string entriesName)
            where T : asStreamData, new()
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

            var writer = new StringBuilder();

            writer.AppendLine($"# {libName}");
            writer.AppendLine($"# {entriesName,-12} {entries.Count}");
            writer.AppendLine();

            foreach (var entry in entries)
            {
                entry.Print(writer);
                writer.AppendLine();
            }

            File.WriteAllText(Path.ChangeExtension(filename, ".log"), writer.ToString());

            return entries;
        }

        static void Main(string[] args)
        {
            /*
            var file = @"C:\Dev\Research\MM1\vck\gold\dlp\vpredcar_s.dlp";
            var dlp = DLPFile.Open(file);
            
            // write an ASCII file describing the DLP
            dlp.WriteLog();
            */

            var mtlLib = ParseDB<agiMaterial>(@"C:\Dev\Research\MM1\MTL\MATERIAL.DB", "Material library", "Materials");
            var texLib = ParseDB<agiTexture>(@"C:\Dev\Research\MM1\MTL\TEXTURE.DB", "Texture library", "Textures");
            var physLib = ParseDB<agiPhysics>(@"C:\Dev\Research\MM1\MTL\PHYSICS.DB", "Physics library", "Physics");
            
            var directory = @"C:\Dev\Research\MM1\midvwtrial\DLP\";
            var nFiles = 0;

            foreach (var filename in Directory.EnumerateFiles(directory, "*.DLP", SearchOption.AllDirectories))
            {
                Console.WriteLine($"[{nFiles + 1}] {filename}");

                var dlp = DLPFile.Open(filename);

                // write an ASCII file describing the DLP
                dlp.WriteLog();

                nFiles += 1;
            }

            Console.WriteLine($"Finished parsing {nFiles} DLP files.");


            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Process complete. Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
