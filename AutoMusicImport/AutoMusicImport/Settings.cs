using STDLib.Saveable;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoMusicImport
{
    public class Settings : BaseSettings
    {
        public static string ImportFolder { get { return GetPar<string>("/data/Music/Import"); } set { SetPar<string>(value); } }
        public static string MusicFolder { get { return GetPar<string>("/mnt/Music/Music"); } set { SetPar<string>(value); } }
        public static string PlaylistFolder { get { return GetPar<string>("/mnt/Music/Playlists"); } set { SetPar<string>(value); } }
        public static string LowQualityFile { get { return GetPar<string>("/mnt/Music/Playlists/LowQuality.txt"); } set { SetPar<string>(value); } }
        public static int ScanInterval { get { return GetPar<int>(1000); } set { SetPar<int>(value); } }
        public static double GoodQuality { get { return GetPar<double>(320000); } set { SetPar<double>(value); } }

        public override void _GenerateSettings(string file)
        {
            var v = this.GetType().GetProperties();

        }
    }
}
