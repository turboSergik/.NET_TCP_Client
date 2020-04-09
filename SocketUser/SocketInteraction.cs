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

            /*
             /login 
             */


            Packet packet = Protocol.ConfigurePacket(Command.TEXT, Settings.name, Encoding.Unicode.GetBytes(message));
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
                byte[] data = new byte[256]; // buffer for answer

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
