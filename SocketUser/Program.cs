using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SocketUser;

namespace SocketTcpClient
{
    class Program
    {
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8005; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера
        static void Main(string[] args)
        {
            while (true)
            {

                Console.WriteLine("New iter!");
                try
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    // подключаемся к удаленному хосту

                    socket.Connect(ipPoint);
                    Console.WriteLine("Успешное подключение!");

                    // IAsyncResult result = socket.BeginConnect(address, port, null, null);
                    // bool success = result.AsyncWaitHandle.WaitOne(5000, true);

                    while (socket.Connected)
                    {
                      
                        Console.Write("Введите сообщение:");

                        string message = Console.ReadLine();
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        socket.Send(data);

                        // получаем ответ
                        data = new byte[256]; // буфер для ответа
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0; // количество полученных байт

                        do
                        {
                            bytes = socket.Receive(data, data.Length, 0);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (socket.Available > 0);
                        Console.WriteLine("ответ сервера: " + builder.ToString());

                        if (builder.ToString() == "Подключение закрыто!")
                        {

                            // закрываем сокет
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();

                            Console.WriteLine("Подключение закрыто!");
                            Environment.Exit(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}