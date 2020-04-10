using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketUser
{
    public class SocketInteraction
    {

        public Socket socket;
        public IPEndPoint ipPoint;

        public SocketInteraction()
        {
            ipPoint = new IPEndPoint(IPAddress.Parse(Settings.address), Settings.port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect()
        {
            // Connecting to remote host
            socket.Connect(ipPoint);
            Console.WriteLine("Successfully connected!");

        }
        
        /**
         * User login 
         * Sending user creditals to remote server
         */
        public void Login()
        {
            Packet packet = Protocol.ConfigurePacket(Command.LOGIN, Settings.name, null);
            socket.Send(packet.MetaBytes());
        }

        /**
         * Send Message
         * Reading client input and send configurated info to server/client
         */
        public void sendMessage()
        {
            // Reading client input
            string message = Console.ReadLine();
            Packet packet = new Packet();

            // Parsing user input
            if (message.Length > 0)
            {
                string needData = "";
                string needCommand = "";
                int pos = 0;

                // Parse command
                if (message[0] == '/')
                {
                    for (int i = 1; i < message.Length; i++)
                    {
                        if (message[i] == ' ')
                        {
                            pos = i;
                            break;
                        }
                        needCommand += message[i];
                    }

                    for (int i = pos + 1; i < message.Length; i++) needData += message[i];
                    if (needData.Length == 0) needData += " ";

                    if (needCommand.ToLower() == "help") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "help", Encoding.Unicode.GetBytes(needData)); // Configure help command
                    else if (needCommand.ToLower() == "get") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "get", Encoding.Unicode.GetBytes(needData)); // Configure get users command
                    else if (needCommand.ToLower() == "connect") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "connect", Encoding.Unicode.GetBytes(needData)); // Configure connect to client command
                    else if (needCommand.ToLower() == "disconnect") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "disconect", Encoding.Unicode.GetBytes(needData)); // Configure desconnect comamnd
                    else if (needCommand.ToLower() == "send")
                    {
                        // Validating file existence
                        if (File.Exists(needData) == false)
                        {
                            Console.WriteLine("File does not exist!");
                            return;
                        }
                        // Sending file bytes to other client
                        packet = Protocol.ConfigurePacket(Command.BIN, Settings.name, getFileName(needData), getFileBytes(needData));
                    }
                    else
                    {
                        Console.WriteLine("Wrong command!");
                        return;
                    }

                }
                else
                {
                    // Plain text
                    needData = message;
                    // Configuring message to other client
                    packet = Protocol.ConfigurePacket(Command.TEXT, Settings.name, Encoding.Unicode.GetBytes(needData));
                }
            }
            else
            {                
                return;
            }

            // Sending meta + bytes
            socket.Send(packet.MetaBytes());
            socket.Send(packet.Response);
        }

        /**
         * Get filename
         * returns corrent filename
         */
        public string getFileName(string path)
        {
            int pos = path.LastIndexOf("\\");
            string file_name = path.Substring(pos + 1);

            return file_name;
        }

        /**
         * Get file bytes
         * returns file bytes
         */
        public byte[] getFileBytes(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            // Create a byte array of file stream length
            byte[] data = new byte[fs.Length];

            //Read block of bytes from stream into the byte array
            fs.Read(data, 0, System.Convert.ToInt32(fs.Length));
            fs.Close();

            return data;
        }

        /**
         * Get answer 
         * Reading incoming messages
         */
        public void getAnswer()
        {
            // Receiving meta header
            byte[] metaData = new byte[255];

            int bytes = 0;
            try
            {
                bytes = socket.Receive(metaData, metaData.Length, 0);
            } catch(Exception exc)
            {
                Console.WriteLine(exc);
                
            }
            Dictionary<string, string> meta = Protocol.ParseMeta(Encoding.UTF8.GetString(metaData, 0, bytes));

            // Parsing meta command
            switch (Enum.Parse(typeof(Command), meta["Command"]))
            {
                case Command.TEXT:

                    string response = receiveText();
                    Console.WriteLine(meta["User"] + ": " + response);
                    break;
                case Command.BIN:

                    receiveFile(meta["Utils"]);
                    break;
            }
        }

        /**
         * Receive text
         * returns received text
         */
        public string receiveText()
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0; // number of bytes received

            // Receiving packets each equals 256 bytes
            do
            {
                byte[] data = new byte[256];
                bytes = socket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);

            return builder.ToString();
        }

        /**
         * Receiving file
         * Read incoming messages and configure file
         */
        public void receiveFile(string file_name)
        {

            List<byte[]> list = new List<byte[]>();
            do
            {
                byte[] data = new byte[255];

                socket.Receive(data, data.Length, 0);
                list.Add(data);
            }
            while (socket.Available > 0);


            ///write file in file system
           string workingDirectory = Environment.CurrentDirectory;
           string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;

            byte[] allData = list
                            .SelectMany(a => a)
                            .ToArray();


            File.WriteAllBytes(projectDirectory + "//" + file_name, allData);

            Console.WriteLine("File was reveive in dir: " + projectDirectory + "//" + file_name);
        }

        /**
         * Closing connection
         */
        public void closeConnection()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Environment.Exit(0);
        }
    }
}
