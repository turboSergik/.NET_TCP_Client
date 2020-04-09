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

        public void sendMessage()
        {
            Console.Write("Введите сообщение:");

            string message = Console.ReadLine();
            byte[] data = Encoding.Unicode.GetBytes(message);

            socket.Send(data);
        }

        public void getAnswer()
        {
            byte[] data = new byte[256]; // buffer for answer
            StringBuilder builder = new StringBuilder();
            int bytes = 0; // number of bytes received

            do
            {
                bytes = socket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);
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
