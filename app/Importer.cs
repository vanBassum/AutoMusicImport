using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Streams;

namespace AutoMusicImport
{
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
            double bitrate = GetBitrate(file);

            if (title != "" && artist != "")
            {
                string dest = Path.Combine(settings.MusicFolder, artist, title + ".mp3");
                string existingFile = null;
                bool newFileAccepted = false;
                if(Directory.Exists(Path.GetDirectoryName(dest)))
                    existingFile = Directory.EnumerateFiles(Path.GetDirectoryName(dest)).FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).ToLower() == title.ToLower());


                if(existingFile != null)    //The file already exists!
                {
                    double bitrateExisting = GetBitrate(existingFile);

                    if (bitrate > bitrateExisting)  //Keep new file
                    {
                        File.Delete(existingFile);
                        File.Move(file, dest);
                        newFileAccepted = true;
                    }
                    else                                    //Keep old file
                    {
                        File.Delete(file);
                    }
                }
                else                                        //No file found, copy this one!
                {
                    if (!Directory.Exists(Path.GetDirectoryName(dest)))
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Move(file, dest);
                    newFileAccepted = true;
                }

                if(newFileAccepted)
                {
                    if(bitrate < 320000)
                    {
                        using (StreamWriter wrt = new StreamWriter(File.Open(settings.LowQualityFile, FileMode.Append, FileAccess.Write)))
                        {
                            string relPlFile = Path.GetRelativePath(Path.GetDirectoryName(settings.LowQualityFile), dest);
                            wrt.WriteLine(bitrate + "\t " + relPlFile);
                        }
                    }
                }

                string artistPath = Path.GetDirectoryName(file);

                if (!Directory.EnumerateFileSystemEntries(artistPath).Any())
                {
                    try
                    {
                        Directory.Delete(artistPath, true);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("coulnt remove " + artistPath);
                    }
                }

                if (category != "")
                {
                    /*
                    string catFile = Path.Combine(settings.PlaylistFolder, "Raw", category + ".txt");
                    if (!Directory.Exists(Path.Combine(settings.PlaylistFolder, "Raw")))
                        Directory.CreateDirectory(Path.Combine(settings.PlaylistFolder, "Raw"));

                    using (StreamWriter wrt = new StreamWriter(File.Open(catFile, FileMode.Append, FileAccess.Write)))
                        wrt.WriteLine(relPath);
                    */

                    string playlistFile = Path.Combine(settings.PlaylistFolder, category + ".m3u");
                    bool newFile = !File.Exists(playlistFile);
                    using (StreamWriter wrt = new StreamWriter(File.Open(playlistFile, FileMode.Append, FileAccess.Write)))
                    {
                        if (newFile)
                            wrt.WriteLine("#EXTM3U");

                        wrt.WriteLine("#EXTINF:{0},{1} - {2}", GetDuration(file).TotalSeconds.ToString("#"), artist, title);

                        string relPlFile = Path.GetRelativePath(Path.GetDirectoryName(playlistFile), dest);

                        wrt.WriteLine(relPlFile.Replace('\\', '/'));
                    }
                        

                }
            }
            else
            {
                Console.WriteLine("Can't determain artist or title");
            }


        }



        double GetBitrate(string file)
        {
            try
            {
                Task<IMediaInfo> t = MediaInfo.Get(file);
                t.Wait();
                IAudioStream stream = t.Result.AudioStreams.FirstOrDefault();
                if(stream != null)
                return stream.Bitrate;
            }
            catch
            {

            }
            return 0;
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

        TimeSpan GetDuration(string file)
        {
            TimeSpan res = TimeSpan.Zero;
            try
            {
                Task<IMediaInfo> t1 = MediaInfo.Get(file);
                t1.Wait();
                res = t1.Result.Duration;
            }
            catch
            {

            }
            return res;
        }
        

        void Work(CancellationToken token)
        {
            //Now do the actual work


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
                        File.Delete(file);
                    }
                }
            }
        }
    }
}
