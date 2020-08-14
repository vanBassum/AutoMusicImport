using System;

namespace AutoMusicImport
{
    class Program
    {
        static void Main(string[] args)
        {
            Importer importer = new Importer();
            string imput = Console.ReadLine();
            importer.Stop();
        }
    }
}
