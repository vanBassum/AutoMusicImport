using STDLib.Saveable;

namespace AutoMusicImport
{
    public class Settings : SaveableSettings
    {
        public string ImportFolder { get; set; } = "/mnt/Music/Import";
        public string MusicFolder { get; set; } = "/mnt/Music/Music";
        public string PlaylistFolder { get; set; } = "/mnt/Music/Playlists";
        public int ScanInterval { get; set; } = 1000;
        public string LowQualityFile { get; set; } = "/mnt/Music/Playlists/LowQuality.txt";
        public double GoodQuality { get; set; } = 320000;

    }
}
