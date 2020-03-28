using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STDLib.Saveable;
using Xabe.FFmpeg;

namespace AutoMusicImport
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Importer importer = new Importer();
            bool exitPending = false;
            while (!exitPending)
            {
                string imput = Console.ReadLine();
                string cmd = imput.Split(' ').First();

                switch(cmd.ToLower())
                {
                    case "exit":
                        exitPending = true;
                        break;

                    case "help":
                    default:
                        Console.WriteLine("exit     - exits the application");
                        break;
                }
            }
            importer.Stop();
            Console.WriteLine("Bye");
        }

        
    }

    public class Importer
    {
        string[] SupportedExtentions { get; } = { ".mp3", ".m4a", ".flac" };
        static string settingsFile = "settings.json";
        Settings settings = new Settings();
        CancellationTokenSource cts = new CancellationTokenSource();

        public Importer()
        {

            if (File.Exists(settingsFile))
                settings.Load(settingsFile);
            else
                settings.Save(settingsFile);

            if (!Directory.Exists(settings.ImportFolder))
            {
                Console.WriteLine("Import directory doenst exits");
                return;
            }

            if (!Directory.Exists(settings.MusicFolder))
            {
                Console.WriteLine("Music directory doenst exits");
                return;
            }

            Task task = new Task(() => Work(cts.Token));
            task.Start();
        }

        public void Stop()
        {
            cts.Cancel();
        }

        


        void ImportFile(string file)
        {
            string relPath = Path.GetRelativePath(settings.ImportFolder, file);
            Console.WriteLine("Importing: " + relPath);
            string title = Path.GetFileNameWithoutExtension(relPath);
            string artist = Path.GetFileName(Path.GetDirectoryName(relPath));
            string category = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(relPath)));

            if (title != "" && artist != "")
            {
                string dest = Path.Combine(settings.MusicFolder, artist, title + ".mp3");
                bool keepNewFile = false;

                if (File.Exists(dest))
                {
                    long newFileLength = new System.IO.FileInfo(file).Length;
                    long existingFileLength = new System.IO.FileInfo(dest).Length;
                    keepNewFile = newFileLength > existingFileLength;
                }
                else
                {
                    if (!Directory.Exists(Path.GetDirectoryName(dest)))
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    keepNewFile = true;
                }


                if (keepNewFile)
                {
                    File.Delete(dest);
                    File.Move(file, dest);
                }
                else
                {
                    File.Delete(file);
                }

                string artistPath = Path.GetDirectoryName(file);

                if (!Directory.EnumerateFileSystemEntries(artistPath).Any())
                {
                    try
                    {
                        Directory.Delete(artistPath);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("coulnt remove " + artistPath);
                    }
                }

                if (category != "")
                {
                    string catFile = Path.Combine(settings.PlaylistFolder, category);
                    using (StreamWriter wrt = new StreamWriter(File.Open(catFile, FileMode.Append, FileAccess.Write)))
                        wrt.WriteLine(relPath);
                }
            }
            else
            {
                Console.WriteLine("Can't determain artist or title");
            }


        }


        string ChangeExtention(string file, string newExt)
        {
            file = file.Substring(0, file.LastIndexOf('.'));
            return file + newExt;
        }

        bool TryConvert(string input, string output)
        {
            bool sucess = false;
            try
            {
                string extention = Path.GetExtension(input);
                if(SupportedExtentions.Contains(extention))
                {
                    IConversion conv = Conversion.Convert(input, output);
                    conv.SetAudioBitrate("320k");
                    conv.Start().Wait();
                    sucess = true;
                }
            }
            catch
            {

            }
            return sucess;
        }

        void Work(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(settings.ScanInterval);
                var files = Directory.EnumerateFiles(settings.ImportFolder, "*.*", SearchOption.AllDirectories);

                foreach (string sourceFile in files)
                {
                    string file = sourceFile;
                    if (token.IsCancellationRequested)
                        break;

                    if (Path.GetExtension(file) != ".mp3")
                    {
                        string output = ChangeExtention(file, ".mp3");

                        if (TryConvert(file, output))
                        {
                            File.Delete(file);
                            file = output;
                        }
                    }

                    if (Path.GetExtension(file) == ".mp3")
                    {
                        ImportFile(file);
                    }
                    else
                    {
                        Console.WriteLine("No support for '" + Path.GetExtension(file) + "' files!");
                    }
                }
            }
        }
    }




    public class Settings : SaveableSettings
    {
        public string ImportFolder { get; set; } = "/mnt/Music/Import";
        public string MusicFolder { get; set; } = "/mnt/Music/Music";
        public string PlaylistFolder { get; set; } = "/mnt/Music/Playlists/Raw";
        public int ScanInterval { get; set; } = 1000;

    }
}
