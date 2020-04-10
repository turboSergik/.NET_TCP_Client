using System;
using System.Collections.Generic;
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
            Console.Write("Введите сообщение:");

            string message = Console.ReadLine();

            Packet packet = new Packet();

            /*
             /login 
            */

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
                    if (needCommand.ToLower() == "get") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "help", Encoding.Unicode.GetBytes(needData));
                    if (needCommand.ToLower() == "connect") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "help", Encoding.Unicode.GetBytes(needData));
                    if (needCommand.ToLower() == "disconnect") packet = Protocol.ConfigurePacket(Command.UTILS, Settings.name, "help", Encoding.Unicode.GetBytes(needData));
                    if (needCommand.ToLower() == "send") packet = Protocol.ConfigurePacket(Command.BIN, Settings.name, "help", Encoding.Unicode.GetBytes(needData));

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




        public void getAnswer()
        {
            StringBuilder builder = new StringBuilder();
            int bytes = 0; // number of bytes received

            byte[] metaData = new byte[256];
            bytes = socket.Receive(metaData, metaData.Length, 0);
            Dictionary<string, string> meta = Protocol.ParseMeta(Encoding.UTF8.GetString(metaData, 0, bytes));

            List<byte[]> list = new List<byte[]>();
            do
            {
                byte[] data = new byte[255]; // buffer for answer

                socket.Receive(data, data.Length, 0);
                list.Add(data);
            }
            while (socket.Available > 0);

            // TODO: отследить тип запроса

            switch (Enum.Parse(typeof(Command), meta["Command"]))
            {
                case Command.TEXT:
                    break;
                case Command.BIN:
                    break;
            }

            Console.WriteLine("Ответ сервера: " + builder.ToString());

            if (builder.ToString() == "Connection closed by user!")
            {
                closeConnection();
            }
        }

        public void closeConnection()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Environment.Exit(0);
        }
    }
}
