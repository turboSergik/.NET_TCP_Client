using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SocketUser;
using System.IO;


namespace SocketTcpClient
{
    class Program
    {

        static void Main(string[] args)
        {
            while (true)
            {
                try
                {

                    SocketInteraction socketInteraction = new SocketInteraction();

                    // подключаемся к удаленному хосту
                    socketInteraction.Connect();
                   
                    while (true)
                    {

                        socketInteraction.sendMessage();
                        socketInteraction.getAnswer();

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