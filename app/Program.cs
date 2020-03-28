using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STDLib.Saveable;


namespace AutoMusicImport
{
    class Program
    {
        static string settingsFile = "settings.json";
        static void Main(string[] args)
        {

            CancellationTokenSource cts = new CancellationTokenSource();
            Task task = new Task(Work, cts.Token);
            task.Start();
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
            cts.Cancel();
            Console.WriteLine("Bye");
        }



        static void Work(object obj)
        {
            CancellationToken token = (CancellationToken)obj;
            Settings settings = new Settings();
            if (File.Exists(settingsFile))
                settings.Load(settingsFile);
            else
                settings.Save(settingsFile);

            var ext = new List<string> { "mp3" };

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

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(1000);
                var files = Directory.EnumerateFiles(settings.ImportFolder, "*.*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    if (token.IsCancellationRequested)
                        break;

                    string relPath = Path.GetRelativePath(settings.ImportFolder, file);

                    string title = Path.GetFileNameWithoutExtension(relPath);
                    string artist = Path.GetFileName(Path.GetDirectoryName(relPath));
                    string category = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(relPath)));

                    Console.WriteLine(" + " + relPath);

                    if (title != "" && artist != "")
                    {
                        switch (Path.GetExtension(relPath))
                        {
                            case ".mp3":


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
                                    Directory.Delete(artistPath);
                                }

                                if (category != "")
                                {
                                    string catFile = Path.Combine(settings.PlaylistFolder, category);
                                    using (StreamWriter wrt = new StreamWriter(File.Open(catFile, FileMode.Append, FileAccess.Write)))
                                        wrt.WriteLine(relPath);
                                }
                                break;

                            default:
                                Console.WriteLine("No support for '" + Path.GetExtension(relPath) + "' files!");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Can't determain artist or title");
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
