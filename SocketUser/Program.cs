using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SocketUser;
using System.IO;
using System.Threading;

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

                    socketInteraction.Login();

                    Thread acceptMessageManager = new Thread(new ThreadStart(() =>
                    {
                        while (true)
                            socketInteraction.getAnswer();
                    }));

                    acceptMessageManager.Start();

                    while (true)
                    {

                        socketInteraction.sendMessage();
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