using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;

using System.Net;
using System.IO;

namespace ConsoleApp2
{
    class Program
    {

        void OpenApplication(string myFavoritesPath)
        {
            // Display the contents of the favorites folder in the browser.
            Process.Start(myFavoritesPath);
        }

        void ExecuteCom(string comanda)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = startInfo };

            process.Start();
            process.StandardInput.WriteLine(comanda);
            process.StandardInput.WriteLine("exit");

            process.WaitForExit();
        }

        void Update(string filename,string date)
        {
            using (StreamWriter sw = File.AppendText(filename))
            {
                sw.WriteLine(date);
            }
            //File.WriteAllText(filename, date);
        }

        String GetHttp()
        {
            String rez = "";
            string html = string.Empty;
            string url = @"https://api.stackexchange.com/2.2/answers?order=desc&sort=activity&site=stackoverflow";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            Console.WriteLine(html);
            return rez;
        }

        static void Main(string[] args)
        {
            String comanda;
            Program myProcess = new Program();

            do
            {
                comanda = Console.ReadLine();
                comanda = System.Text.RegularExpressions.Regex.Replace(comanda, @"\s+", " ");
                string[] words = comanda.Split(' ');

                switch (words[0])
                {
                    case "open":
                        myProcess.OpenApplication(words[1]);
                        break;
                    case "cmd":
                        myProcess.ExecuteCom(comanda.Remove(0,4));
                        break;
                    case "update":
                        myProcess.Update(words[1], comanda.Remove(0, 8 + words[1].Length));
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
                Console.WriteLine(comanda);
            } while (comanda != "exit");

            comanda = Console.ReadLine();

        }
    }
}

