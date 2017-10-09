using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoToMp3
{
    class Program
    {
        static int counterForDone = 0;
        static int processesCount = 0;
        static String path = "";
        static List<String> filetypes = new List<String>();

        static void Main(string[] args)
        {
            //
            //Settings

            // Filetypes
            filetypes.Add(".m4a");
            filetypes.Add(".ogg");


            // 
            // Converting and Co

            Console.Write("Enter Path: ");
            path = Console.ReadLine();

            Dictionary<System.Diagnostics.Process, Boolean> processes = new Dictionary<System.Diagnostics.Process, Boolean>();

            foreach (var item in Directory.GetFiles(path))
            {
                if (filetypes.Contains(Path.GetExtension(item)))
                {
                    String filename = Path.GetFileNameWithoutExtension(item) + ".mp3";
                    String directory = Path.GetDirectoryName(item);
                    filename = filename.Replace(" (192kbit_AAC)", "");
                    filename = filename.Replace(" (128kbit_AAC)", "");
                    filename = filename.Replace(" (152kbit_AAC)", "");
                    filename = filename.Replace(" (128kbit_Opus)", "");
                    filename = filename.Replace(" (152kbit_Opus)", "");

                    String newFilename = "\"" + directory + "\\" + filename + "\"";
                    String oldFilename = "\"" + item + "\"";

                    String command = "ffmpeg -i " + oldFilename + " " + newFilename;

                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C " + command;
                    process.StartInfo = startInfo;

                    processes.Add(process, false);
                }
            }


            // Actual Converting
            int counter = 5;
            int lastItem = 0;

            processesCount = processes.Count;

            Thread counterThread = new Thread(PrintStatus);
            counterThread.IsBackground = true;
            counterThread.Start();

            List<System.Diagnostics.Process> itemsToWait = new List<System.Diagnostics.Process>();

            for (int i = 0; i < processes.Count; i++)
            {
                if(counter % (i + 1) == 0)
                {
                    lastItem = i;
                    itemsToWait.Add(processes.ElementAt(i).Key);

                    foreach (var item in itemsToWait)
                    {
                        item.Start();
                    }

                    foreach (var item in itemsToWait)
                    {
                        item.WaitForExit();
                        counterForDone++;
                    }

                    itemsToWait.Clear();
                }

                else
                {
                    itemsToWait.Add(processes.ElementAt(i).Key);
                }
            }

            if(processes.Count % counter != 0)
            {
                lastItem++;
                for (int i = lastItem; i < processes.Count; i++)
                {
                    processes.ElementAt(i).Key.Start();
                    processes.ElementAt(i).Key.WaitForExit();
                    counterForDone++;
                }
            }

            counterThread.Abort();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit . . .");
            Console.ReadLine();
        }

        static void PrintStatus()
        {
            while (true)
            {
                Console.Clear();

                Console.WriteLine("Enter Path: " + path);
                Console.WriteLine((Math.Round((double)counterForDone / processesCount, 4) * 100) + "%");
                Thread.Sleep(3000);
            }
        }
    }
}
