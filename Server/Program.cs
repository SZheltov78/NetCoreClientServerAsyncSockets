using AsyncSocketClientServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        //класс для пользоателей
        class User
        {
            public Socket socket;
            public string name;
        }
        //список пользователей
        static List<User> Users = new List<User>();
        //
        static AsyncSockets ServerObj;
        //строка с доступными командами
        static string commandString;
        static void Main(string[] args)
        {
            //
            commandString = "Список команд:" + Environment.NewLine;
            commandString += "Как дела?" + Environment.NewLine;
            commandString += "Который час?" + Environment.NewLine;
            commandString += "Список клиентов." + Environment.NewLine;
            //экземпляр
            ServerObj = new AsyncSockets();
            //подписка на события
            ServerObj.OnAccept += OnAccept;
            ServerObj.OnError += OnError;
            ServerObj.OnReceive += OnReceive;

            //старт сервера
            try
            {
                ServerObj.ServerStart("127.0.0.1", 5555);
                Console.WriteLine("Сервер запущен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }
        static void OnAccept(object s, Socket e)
        {
            //только подключился
            Console.WriteLine(e.RemoteEndPoint.ToString() + " accept client");
            ServerObj.Send(e, "Напишите свое имя.");
        }

        static void OnReceive(object s, ReceiveObj e)
        {
            User u;
            try
            {
                //этот пользователь уже есть в списке?
                u = Users.Where(c => c.socket == e.socket).First();

                //если уже есть:
                string message = e.message;
                message = message.ToUpper();

                //строка с ответом
                string answer = "";

                //валидность команды
                bool valid = false;

                if (message.Contains("СПИСОК КЛИЕНТОВ"))
                {
                    valid = true;
                    foreach (User c in Users)
                    {
                        answer += c.name + " online=" + ServerObj.IsConnected(c.socket) + Environment.NewLine;
                    }
                }

                if (message.Contains("КАК ДЕЛА"))
                {
                    valid = true;
                    answer += "Хорошо." + Environment.NewLine;
                }

                if (message.Contains("КОТОРЫЙ ЧАС"))
                {
                    valid = true;
                    answer += "Сейчас: " + DateTime.Now.ToShortTimeString() + Environment.NewLine;
                }

                if (message == "ПОКА")
                {
                    valid = true;
                    answer += "Досвидания." + Environment.NewLine;
                }

                //если команда не опознана, напомнить список доступных
                if (!valid) answer = commandString;
                ServerObj.Send(e.socket, answer);

            }
            catch
            {
                //первый ответ пользователя = его имя
                u = new User();
                u.socket = e.socket;
                u.name = e.message;
                Users.Add(u);
                ServerObj.Send(e.socket, $"Здравствуйте {u.name}" + Environment.NewLine + commandString);
            }

            Console.WriteLine(e.socket.RemoteEndPoint.ToString() + " Mssage: " + e.message);
        }

        static void OnError(object s, ReceiveObj e)
        {
            Console.WriteLine(e.socket.RemoteEndPoint.ToString() + " Error: " + e.message);
        }

    }
}
