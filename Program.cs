using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace rename_tool
{
    class Program
    {
        private static readonly string sourceDir = @"F:\backup\DOSCollection";
        private static readonly string destinationDir = @"H:\batocera\roms\dos";
        private static readonly string needMoreWorkTxt = @"H:\batocera\roms\dos\_need_more_work.txt";
        private static readonly string rarExecPath = @"C:\Program Files\WinRAR\unrar.exe";

        static void Main(string[] args)
        {
            string[] n64Extensions = new string[] { ".rom", ".z64", ".n64", ".v64" };
            string [] compressionExtensions = new string[] { ".rar", ".zip" };

            if (!Directory.Exists(sourceDir))
            {
                Console.Error.WriteLine($"No volume mapped to {sourceDir}");
                return;
            }

            if (!Directory.Exists(destinationDir))
            {
                Console.Error.WriteLine($"No volume mapped to {destinationDir}");
                return;
            }

            Console.WriteLine("Checking zip files");
            Console.WriteLine();

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                Console.WriteLine($"  {file}");
                string ext = Path.GetExtension(file);

                if (ext != ".zip")
                {
                    Console.WriteLine("  Not a zip file. Not doing anything more here.");
                }

                Console.WriteLine($"  Inspecting {ext.Substring(1).ToUpper()} file {Path.GetFileName(file)}");
                
                try 
                {
                    using (var archive = ZipFile.OpenRead(file))
                    {
                        string archiveDestPath = Path.Combine(destinationDir, Path.GetFileNameWithoutExtension(file) + ".dos");
                        Console.WriteLine($"    Archive destiantion path: {archiveDestPath}");

                        try
                        {
                            archive.ExtractToDirectory(destinationDir);
                            Console.WriteLine("    Success");
                            Console.WriteLine("    Next step is to create a dosbox.bat file which opens the exe file (if found)");

                            var exeFiles = Directory.GetFiles(destinationDir).Where(f => Path.GetExtension(f) == ".exe");
                            int exeFileCount = exeFiles.Count();

                            if (exeFileCount != 1)
                            {
                                Console.WriteLine($"    Found {exeFileCount} exe files. Unable to create dosbox.bat file.");
                                File.AppendAllText(needMoreWorkTxt, Path.GetFileNameWithoutExtension(file));
                                continue;
                            }

                            string exeFile = exeFiles.First();
                            Console.WriteLine($"    Creating the dosbox.bat file with instructions to execute {exeFile}");

                            File.WriteAllText(destinationDir, exeFile);
                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine($"    Failed to extract file: {exc.Message}");
                        }
                        // continue;
                        // foreach (var entry in archive.Entries)
                        // {
                        //     string fileName = entry.Name;
                        //     string extension = Path.GetExtension(fileName);

                        //     var buffer = new byte[4096];
                            
                        //     if (n64Extensions.Contains(extension))
                        //     {
                        //         Console.WriteLine($"    {fileName}");

                        //         string targetPath = Path.Combine(destinationDir, dirName + Path.GetExtension(entry.Name));

                        //         if (Path.GetExtension(targetPath) == ".rom")
                        //         {
                        //             targetPath = Path.ChangeExtension(targetPath, "n64");
                        //         }

                        //         Console.WriteLine($"    Target path is {targetPath}");
                        //         Console.WriteLine("    Begin transfering from stream to file");
                        //         try 
                        //         {
                        //             entry.ExtractToFile(targetPath);
                        //             Console.WriteLine("    Done");
                        //         }
                        //         catch (Exception)
                        //         {
                        //             File.Delete(targetPath);
                        //             Console.WriteLine("    Failure");
                        //         }
                        //     }
                        // }
                    }
                }
                catch (SystemException) {}

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
