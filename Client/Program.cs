using AsyncSocketClientServer;
using System;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //экземпляр класса
            AsyncSockets ClientObj = new AsyncSockets();
            //подписка на события
            ClientObj.OnReceive += OnReceive;
            ClientObj.OnError += OnError;

            //старт клиента
            Socket socket =null;
            try
            {
                socket = ClientObj.ClientStart("127.0.0.1", 5555);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
                Environment.Exit(0);
            }            

            string com = "";
            while (com != "пока")
            {
                com = Console.ReadLine();
                if (com != "") ClientObj.Send(socket, com);
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        static void OnReceive(object s, ReceiveObj e)
        {
            Console.WriteLine("Бот:");
            Console.WriteLine(e.message);       
        }
        static void OnError(object s, ReceiveObj e)
        {
            Console.WriteLine("Error: " + e.message);
        }
    }
}
