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

        static void Main(string[] args)
        {
            var directories = new[] {
                @"C:\Dev\Research\MM1\",
                //@"C:\Dev\Research\MM1\midvwtrial\",
            };
            
            var nFiles = 0;

            foreach (var directory in directories)
            {
                var dlpDir = Path.Combine(directory, "DLP");
                var mtlDir = Path.Combine(directory, "MTL");

                ParseDB<agiMaterial>(Path.Combine(mtlDir, "MATERIAL.DB"), "Material library", "Materials");
                ParseDB<agiTexture>(Path.Combine(mtlDir, "TEXTURE.DB"), "Texture library", "Textures");
                ParseDB<agiPhysics>(Path.Combine(mtlDir, "PHYSICS.DB"), "Physics library", "Physics");

                foreach (var filename in Directory.EnumerateFiles(dlpDir, "*.DLP", SearchOption.AllDirectories))
                {
                    Console.WriteLine($"[{nFiles + 1}] {filename}");

                    var dlp = DLPFile.Open(filename);

                    // write an ASCII file describing the DLP
                    dlp.SaveASCII();

                    nFiles += 1;
                }
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
