using STDLib.Saveable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoMusicImport
{
    public class Settings : BaseSettings<Settings>
    {
        public static string ImportFolder { get { return GetPar<string>(Path.Combine(defaultAppFolder, "Music/Import")); } set { SetPar<string>(value); } }
        public static string MusicFolder { get { return GetPar<string>(Path.Combine(defaultAppFolder, "Music/Music")); } set { SetPar<string>(value); } }
        public static string PlaylistFolder { get { return GetPar<string>(Path.Combine(defaultAppFolder, "Music/Playlists")); } set { SetPar<string>(value); } }
        public static string LowQualityFile { get { return GetPar<string>(Path.Combine(defaultAppFolder, "Music/Playlists/LowQuality.txt")); } set { SetPar<string>(value); } }
        public static int ScanInterval { get { return GetPar<int>(60000); } set { SetPar<int>(value); } }
        public static double GoodQuality { get { return GetPar<double>(320000); } set { SetPar<double>(value); } }

    }
}
