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
        string path=null;
        string OpenApplication(string myFavoritesPath)
        {

            try
            {
                Process.Start(myFavoritesPath);
                return myFavoritesPath+" Ok";
            }
            catch
            {
                return "Eroare la deschiderea " + myFavoritesPath;
            }
        }

        string ExecuteCom(string comanda)
        {
            string s="";
            bool outcmd= false;
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
            if (path != null)
                process.StandardInput.WriteLine("cd " + path);
            process.StandardInput.WriteLine(comanda);

            process.StandardInput.WriteLine("exit");
            using (StreamReader streamReader = process.StandardOutput)
            {
                s = streamReader.ReadToEnd();
            }
            process.WaitForExit();
            StringReader strReader = new StringReader(s);
            s = "";
            while (true)
            {
                path = strReader.ReadLine();
                if (path.Contains("exit") == true)
                {
                    path = path.Remove(path.Length - 5);
                    break;
                }
                if (outcmd == true)
                    s += path+"\n";
                if (path.Length > comanda.Length)
                {
                    path = path.Substring(path.Length - comanda.Length);
                }
                if (path.Equals(comanda) == true)
                {
                    outcmd = true;
                }
                
            }
            return s;
        }

        string Update(string filename,string date)
        {
            using (StreamWriter sw = File.AppendText(filename))
            {
                sw.WriteLine(date);
            }
            return null;
        }

        string Read(string filename,string date)
        {
            string s = "";
            using (StreamReader sr = File.OpenText(filename))
            {
                while ((s = sr.ReadLine()) != null)
                {
                    Console.WriteLine(s);
                }
            }
            return s;
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
            string s="";
            do
            {
                comanda = Console.ReadLine();
                comanda = System.Text.RegularExpressions.Regex.Replace(comanda, @"\s+", " ");
                string[] words = comanda.Split(' ');

                switch (words[0])
                {
                    case "open":
                        s = myProcess.OpenApplication(words[1]);
                        break;
                    case "cmd":
                        s = myProcess.ExecuteCom(comanda.Remove(0,4));
                        
                        break;
                    case "update":
                        s = myProcess.Update(words[1], comanda.Remove(0, 8 + words[1].Length));
                        break;
                    case "read":
                        s = myProcess.Read(words[1], comanda.Remove(0, 5));
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
                Console.WriteLine("!!!!!\n" + s + "?????");
                Console.WriteLine(comanda);
            } while (comanda != "exit");

            
            comanda = Console.ReadLine();

        }
    }
}

