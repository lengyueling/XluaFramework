using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Server
    {
        private Socket m_ServerSocket;
        private SocketAsyncEventArgs m_AcceptArgs;

        //最大连接数
        private int m_MaxCount;


        public Server()
        {
            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start(int port, int maxCount)
        {
            try
            {
                m_MaxCount = maxCount;
                IPEndPoint point = new IPEndPoint(IPAddress.Any, port);
                m_ServerSocket.Bind(point);
                m_ServerSocket.Listen(10);
                Console.WriteLine("服务器启动成功...");
                //初始化连接池
                ConnectionPool.Init(m_MaxCount);

                m_AcceptArgs = new SocketAsyncEventArgs();
                m_AcceptArgs.Completed += OnConnectComplete;
                StartAccept(m_AcceptArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine("服务器启动失败...");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 开始等待服务器连接
        /// </summary>
        /// <param name="e"></param>
        private void StartAccept(SocketAsyncEventArgs acceptArgs)
        {
            Console.WriteLine("等待客户端连接...");
            m_ServerSocket.AcceptAsync(acceptArgs);
        }

        /// <summary>
        /// 连接客户端完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectComplete(object sender, SocketAsyncEventArgs acceptArgs)
        {
            Connection client = ConnectionPool.Dequeue();

            if (client == null)
                throw new Exception("服务器人数已满");
            client.ClientSocket = acceptArgs.AcceptSocket;

            Console.WriteLine("客户端连接：" + client.ClientSocket.RemoteEndPoint.ToString());

            client.StartReceive();

            acceptArgs.AcceptSocket = null;
            StartAccept(acceptArgs);
        }
    }
}
