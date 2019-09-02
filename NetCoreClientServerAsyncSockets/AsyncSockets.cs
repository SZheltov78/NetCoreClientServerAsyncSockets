using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AsyncSocketClientServer
{
    //для хранения и передачи сокета и сообщения
    public class ReceiveObj
    {
        public Socket socket;
        public byte[] buffer = new byte[1024];
        public string message;
    }

    public class AsyncSockets
    {
        //события
        public delegate void AcceptHandle(object sender, Socket e);
        public event AcceptHandle OnAccept;

        public delegate void ReceiveHandle(object sender, ReceiveObj e);
        public event ReceiveHandle OnReceive;

        public delegate void ErrorHandle(object sender, ReceiveObj e);
        public event ErrorHandle OnError;
        
        public Socket ClientStart(string ip, int port)
        {
            Socket soket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

            ReceiveObj r = new ReceiveObj();
            r.socket = soket;

            //начало приема сообщений
            soket.BeginReceive(r.buffer, 0, 1024, SocketFlags.None, Read, r);

            return soket;
        }
        public void ServerStart(string ip, int port)
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint local = new IPEndPoint(IPAddress.Parse(ip), port);
            listener.Bind(local);
            listener.Listen(10);
            //начало приема соединений
            listener.BeginAccept(Accept, listener);
        }

        private void Accept(IAsyncResult obj)
        {
            //получить прослушивателя
            Socket listener = (Socket)obj.AsyncState;
            //получить сокет нового клиента
            Socket clientSocket = listener.EndAccept(obj);

            //событие
            OnAccept(this, clientSocket);

            //для нового клиента создается экземпляр для передачи всего необходимого
            ReceiveObj r = new ReceiveObj();
            r.socket = clientSocket;
            
            //начать прием сообщений на новом сокете 
            r.socket.BeginReceive(r.buffer, 0, 1024, SocketFlags.None, Read, r);
            
            //продолжить прием соединений
            listener.BeginAccept(Accept, listener);
        }

        void Read(IAsyncResult obj)
        {
            ReceiveObj client = (ReceiveObj)obj.AsyncState;
            int size = 0;
            try
            {
                //прочитать сообщение 
                size = client.socket.EndReceive(obj);
            }
            catch (Exception ex)
            {
                client.message = ex.ToString();
                //текст ошибки передается через тотже тип что и сообщение
                OnError(this, client);
                return;
            }

            client.message = Encoding.UTF8.GetString(client.buffer, 0, size);

            //событие - пием сообщения
            OnReceive(this, client);

            //продолжить прием сообщение с этого сокета
            try
            {
                client.socket.BeginReceive(client.buffer, 0, 1024, SocketFlags.None, Read, client);
            }
            catch (Exception ex)
            {
                client.message = ex.ToString();
                OnError(this, client);
            }
        }

        public void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            //начало асинхронной отправки
            handler.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                //завершение отправки
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                //в случае ошибки событие
                OnError(this, new ReceiveObj { socket = (Socket)ar.AsyncState, message = e.ToString() });
            }
        }

        //методопределения "живой" ли клиент
        public bool IsConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }
}
