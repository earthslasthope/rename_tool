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
        private static readonly string sourceDir = @"F:\backup\3DS_Roms_0001-0800_USA";
        private static readonly string destinationDir = @"F:\backup\3DS_Roms_0001-0800_USA_Destination";
        private static readonly string rarExecPath = @"C:\Program Files\WinRAR\unrar.exe";

        static void Main(string[] args)
        {
            string[] n64Extensions = new string[] { ".rom", ".z64", ".n64", ".v64" };
            string [] compressionExtensions = new string[] { ".rar", ".zip" };

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
                        Console.WriteLine($"    {fileName}");

                        string targetPath = Path.Combine(destinationDir, dirName + Path.GetExtension(fileName));
                        string extractArgs = $"e {file} {entry} {destinationDir}";
                        Console.WriteLine($"    Target path is {targetPath}");
                        Console.WriteLine("    Begin running the unrar command");
                        Console.WriteLine($"    Args: {extractArgs}");
                        using (var process = new Process())
                        {
                            process.StartInfo.FileName = rarExecPath;
                            process.StartInfo.Arguments = extractArgs;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardError = true;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.EnableRaisingEvents = true;
                            List<string> errors = new List<string>();
                            process.ErrorDataReceived += (sender, e) => 
                            {
                                if (!string.IsNullOrWhiteSpace(e.Data))
                                {
                                    errors.Add(e.Data);
                                }
                            };
                            process.Start();
                            process.BeginErrorReadLine();
                            process.WaitForExit();
                            process.CancelErrorRead();

                            if (errors.Any())
                            {
                                Console.WriteLine("    Failure");
                                foreach (var error in errors)
                                {
                                    Console.WriteLine(error);
                                }
                            }
                            else
                            {
                                Console.WriteLine("    Done");

                                File.Move(
                                    Path.Combine(destinationDir, fileName),
                                    targetPath
                                );
                            }
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
