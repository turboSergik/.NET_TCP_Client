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

                    // connecting to remote host
                    socketInteraction.Connect();

                    socketInteraction.Login();

                    Thread acceptMessageManager = new Thread(new ThreadStart(() =>
                    {
                        try
                        {
                            while (true) {
                                socketInteraction.getAnswer();
                            }
                        } catch(Exception exc)
                        {
                            Console.WriteLine("Server error: " + exc);
                            Environment.Exit(0);
                        }
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