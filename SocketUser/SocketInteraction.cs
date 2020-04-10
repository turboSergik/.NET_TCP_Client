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
            socket.Connect(ipPoint);
            Console.WriteLine("Успешное подключение!");

        }
         
        public void Login()
        {
            Packet packet = Protocol.ConfigurePacket(Command.LOGIN, Settings.name, null);
            socket.Send(packet.MetaBytes());
        }

        public void sendMessage()
        {
    
            string message = Console.ReadLine();
            Packet packet = new Packet();

            if (message.Length > 0)
            {
                string needData = "";
                string needCommand = "";
                int pos = 0;

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

                    if (needCommand.ToLower() == "help") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "help", Encoding.Unicode.GetBytes(needData));
                    else if (needCommand.ToLower() == "get") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "get", Encoding.Unicode.GetBytes(needData));
                    else if (needCommand.ToLower() == "connect") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "connect", Encoding.Unicode.GetBytes(needData));
                    else if (needCommand.ToLower() == "disconnect") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "disconect", Encoding.Unicode.GetBytes(needData));
                    else if (needCommand.ToLower() == "send")
                    {
                        if (File.Exists(needData) == false)
                        {
                            Console.WriteLine("File does not exist!");
                            return;
                        }
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
                    needData = message;
                    packet = Protocol.ConfigurePacket(Command.TEXT, Settings.name, Encoding.Unicode.GetBytes(needData));
                }
            }
            else
            {                
                return;
            }

            socket.Send(packet.MetaBytes());
            socket.Send(packet.Response);
        }


        public string getFileName(string path)
        {
            int pos = path.LastIndexOf("\\");
            string file_name = path.Substring(pos + 1);

            return file_name;
        }

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

        public void getAnswer()
        {

            byte[] metaData = new byte[255];

            int bytes = socket.Receive(metaData, metaData.Length, 0);
            Dictionary<string, string> meta = Protocol.ParseMeta(Encoding.UTF8.GetString(metaData, 0, bytes));


            // TODO: отследить тип запроса +Completed

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

        public string receiveText()
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0; // number of bytes received

            byte[] data = new byte[255];

            do
            {
                bytes = socket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);

            return builder.ToString();
        }


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


        public void closeConnection()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Environment.Exit(0);
        }
    }
}
