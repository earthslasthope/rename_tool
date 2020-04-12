using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace rename_tool
{
    class Program
    {
        private static readonly string sourceDir = @"F:\backup\3DS_Roms_0001-0800_USA";
        private static readonly string destinationDir = @"H:\batocera\roms\3ds";
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

            Console.WriteLine("Fetching folders");
            Console.WriteLine();

            foreach (string gameDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(gameDir);
                Console.WriteLine($"{dirName} (FOLDER)");

                var files = Directory.GetFiles(gameDir).Where(x => compressionExtensions.Contains(Path.GetExtension(x)));
                Console.WriteLine($"  Found {files.Count()} files");

                if (!files.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    continue;
                }

                var file = files.First();
                string ext = Path.GetExtension(file);
                Console.WriteLine($"  Inspecting {ext.Substring(1).ToUpper()} file {Path.GetFileName(file)}");
                
                if (ext == ".zip")
                {
                    try 
                    {
                        using (var archive = ZipFile.OpenRead(file))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                string fileName = entry.Name;
                                string extension = Path.GetExtension(fileName);

                                var buffer = new byte[4096];
                                
                                if (n64Extensions.Contains(extension))
                                {
                                    Console.WriteLine($"    {fileName}");

                                    string targetPath = Path.Combine(destinationDir, dirName + Path.GetExtension(entry.Name));

                                    if (Path.GetExtension(targetPath) == ".rom")
                                    {
                                        targetPath = Path.ChangeExtension(targetPath, "n64");
                                    }

                                    Console.WriteLine($"    Target path is {targetPath}");
                                    Console.WriteLine("    Begin transfering from stream to file");
                                    try 
                                    {
                                        entry.ExtractToFile(targetPath);
                                        Console.WriteLine("    Done");
                                    }
                                    catch (Exception)
                                    {
                                        File.Delete(targetPath);
                                        Console.WriteLine("    Failure");
                                    }
                                }
                            }
                        }
                    }
                    catch (SystemException) {}
                }
                else if (ext == ".rar")
                {
                    List<string> entries = new List<string>();

                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = rarExecPath;
                        process.StartInfo.Arguments = $"lb {file}";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.EnableRaisingEvents = true;
                        process.OutputDataReceived += (sender, e) => 
                        {
                            entries.Add(e.Data);
                        };
                        process.Start();
                        process.BeginOutputReadLine();
                        process.WaitForExit();
                        process.CancelOutputRead();
                    }

                    foreach (var entry in entries)
                    {
                        if (Path.GetExtension(entry) != ".3ds")
                        {
                            continue;
                        }

                        string fileName = entry;
                        string targetExistingPath = Path.Combine(destinationDir, fileName);
                        Console.WriteLine($"    Checking if {fileName} exists in the destination dir at");
                        Console.WriteLine($"    {targetExistingPath}");

                        if (File.Exists(targetExistingPath))
                        {
                            string newFilePath = Path.Combine(destinationDir, dirName + Path.GetExtension(targetExistingPath));
                            Console.WriteLine("    File exists. Going to rename the file and the new path will be:");
                            Console.WriteLine($"    {newFilePath}");

                            try
                            {
                                File.Move(targetExistingPath, newFilePath);
                            }
                            catch (Exception exc)
                            {
                                Console.WriteLine($"Unable to rename file: {exc.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("    Not existent. No need to do anything.");
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
