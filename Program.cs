using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace rename_tool
{
    class Program
    {
        private static readonly string sourceDir = @"C:\Users\thordur\Documents\Test_Harness\Source";
        private static readonly string destinationDir = @"C:\Users\thordur\Documents\Test_Harness\Destination";

        static void Main(string[] args)
        {
            string[] n64Extensions = new string[] { ".rom", ".z64", ".n64", ".v64" };

            Console.WriteLine("Fetching folders");
            Console.WriteLine();
            foreach (string gameDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(gameDir);
                Console.WriteLine($"{dirName} (FOLDER)");
                Console.WriteLine("  Searching for zip file");
                var files = Directory.GetFiles(gameDir).Where(x => Path.GetExtension(x) == ".zip");
                Console.WriteLine($"  Found {files.Count()} files");

                if (!files.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    break;
                }

                var file = files.First();
                Console.WriteLine($"  Inspecting ZIP file {Path.GetFileName(file)}");
                using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(file)))
                {
                    ZipEntry  entry;

                    while ((entry = zipStream.GetNextEntry()) != null)
                    {
                        string fileName = entry.Name;
                        string extension = Path.GetExtension(fileName);

                        var buffer = new byte[4096];
                        
                        if (n64Extensions.Contains(extension))
                        {
                            Console.WriteLine($"    Extension is {extension}");

                            string targetPath = Path.Combine(destinationDir, dirName + Path.GetExtension(entry.Name));

                            if (Path.GetExtension(targetPath) == ".rom")
                            {
                                targetPath = Path.ChangeExtension(targetPath, "n64");
                            }

                            Console.WriteLine($"    Target path is {targetPath}");
                            Console.WriteLine("Begin transfering from stream to file");
                            using (FileStream streamWriter = File.Create(targetPath))
                            {
                                StreamUtils.Copy(zipStream, streamWriter, buffer);
                            }
                            Console.WriteLine("Done");
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
