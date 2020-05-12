using System;
using System.Collections.Generic;
using System.Linq;

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
}
