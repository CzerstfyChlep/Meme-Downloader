using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;

namespace MemeDownloader
{
    class Program
    {
        static void Main(string[] args)
        {                      
            Thread UpdaterT = new Thread(Updater);
            Console.WriteLine("Enter to load files");
            Console.ReadLine();
            Console.Clear();
            Console.CursorVisible = false;
            files = Directory.GetFiles("Webpages").Where(x => x.Contains(".html")).ToArray();
            Console.WriteLine("Found files: " + files.Length);
            Dictionary<string, int> DayOcc = new Dictionary<string, int>();
            foreach (string f in files)
            {
                string[] datesplit = f.Split('\\').Last().Split('x');
                FileList.Add(f, datesplit[0]);
                if (!Directory.Exists("Download/" + datesplit[0]))
                    Directory.CreateDirectory("Download/" + datesplit[0]);
                if (DayOcc.Keys.Contains(datesplit[0]))
                    DayOcc[datesplit[0]]++;
                else
                    DayOcc.Add(datesplit[0], 1);
                FileTask.Add(f, false);
            }
            Console.WriteLine("\nFound days: ");
            foreach(string day in DayOcc.Keys)
            {
                Console.WriteLine(day + " (" + DayOcc[day] + ")");
            }
            Console.WriteLine("\n\nEnter number of downloaders to proceed");
            string dcount = Console.ReadLine();
            int.TryParse(dcount, out int dowcount);
            if (dowcount > 4)
                dowcount = 4;
            Console.Clear();

            if (dowcount > 0)
            {
                Thread t = new Thread(() => Search(0));
                t.Start();
            }
            if (dowcount > 1)
            {
                Thread t = new Thread(() => Search(1));
                t.Start();
            }
            if (dowcount > 2)
            {
                Thread t = new Thread(() => Search(2));
                t.Start();
            }
            if (dowcount > 3)
            {
                Thread t = new Thread(() => Search(3));
                t.Start();
            }
            
                      
            UpdaterT.Start();
            Console.ReadLine();
        }

        public static string[] files;
        public static Dictionary<string, string> FileList = new Dictionary<string, string>();
        public static Dictionary<string, bool> FileTask = new Dictionary<string, bool>();

        public static string[] DownloaderDays = new string[4];
        public static int[] DownloaderAmounts = new int[4];
        public static bool[] DownloaderWorking = new bool[4];
        public static string[] DownloaderFiles = new string[4];
        public static bool TaskModified = false;
        public static Random r = new Random();

        static void Updater()
        {
            int c = 0;
            Thread.Sleep(500);
            while (true)
            {                
                if (c == 0)
                {
                    c = 300;
                    Console.Clear();
                    int DownloaderCR = 0;
                    foreach (bool b in DownloaderWorking)
                    {           
                        if (b)
                        {
                            Console.WriteLine("Downloading day " + (Array.IndexOf(files, DownloaderFiles[DownloaderCR]) + 1) + "/" + files.Length + " - " + DownloaderDays[DownloaderCR]);
                            Console.WriteLine("Downloaded: ");
                            
                        }
                        else
                        {
                            Console.WriteLine("Downloading suspendned");
                            Console.WriteLine("Downloaded: " + 000);
                        }
                        DownloaderCR++;
                    }

                }
                c--;
                Thread.Sleep(100);
                int DownloaderC = 0;
                foreach (bool b in DownloaderWorking)
                {
                    if (b)
                    {
                        Console.SetCursorPosition(12, 2 * DownloaderC + 1);
                        for (int a = 0; a < 3 - DownloaderAmounts[DownloaderC].ToString().Length; a++)
                        {
                            Console.Write(0);
                        }
                        Console.Write(DownloaderAmounts[DownloaderC]);
                    }
                    DownloaderC++;
                }              
            }
        }

        static void Search(int DownloaderID)
        {
            DownloaderWorking[DownloaderID] = true;
            using (WebClient client = new WebClient())
            {
                Thread.Sleep(500);                                
                while (FileTask.Any(x => x.Value == false))
                {                   
                    string file = "";
                    while (TaskModified)
                        Thread.Sleep(50);
                    TaskModified = true;
                    string keytomodify = "";
                    foreach(string key in FileTask.Keys)
                    {
                        if(FileTask[key] == false)
                        {
                            keytomodify = key;
                            file = key;
                        }
                    }
                    FileTask[keytomodify] = true;
                    TaskModified = false;
                    DownloaderDays[DownloaderID] = FileList[file];
                    DownloaderFiles[DownloaderID] = file;
                    string txt = File.ReadAllText(file);
                    List<string> URLs = txt.Split(' ').ToList();
                    DownloaderAmounts[DownloaderID] = 0;
                    foreach (string s in URLs.Where(x => x.Contains("data-url")))
                    {
                        DownloaderAmounts[DownloaderID]++;
                        string[] sp = s.Split('"');
                        if (sp[1].Contains("i.redd") || (sp[1].Contains("i.imgur") && !sp[1].Contains(".gifv")))
                        {
                            try
                            {
                                if (sp[1].Contains(".gif"))
                                    client.DownloadFile(sp[1], "Download/" + FileList[file] + "/" + DownloaderAmounts[DownloaderID] + "-" + r.Next(0, 10) + r.Next(0, 10) + r.Next(0, 10) + r.Next(0, 10) + r.Next(0, 10) + ".gif");
                                else
                                    client.DownloadFile(sp[1], "Download/" + FileList[file] + "/" + DownloaderAmounts[DownloaderID] + "-" + r.Next(0, 10) + r.Next(0, 10) + r.Next(0, 10) + r.Next(0, 10) + r.Next(0, 10) + ".jpg");
                            }
                            catch { }
                        }
                        else
                            File.AppendAllText("Download/" + FileList[file] + "/Other.txt", "\n" + sp[1]);
                    }                   
                }
                DownloaderWorking[DownloaderID] = false;
            }
        }
    }
}
