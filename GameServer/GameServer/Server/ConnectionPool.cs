using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
   public static class ConnectionPool
    {
        private static Queue<Connection> ClientConnect;

        public static void Init(int maxCount)
        {
            ClientConnect = new Queue<Connection>(maxCount);
            Connection connect  = null;
            for (int i = 0; i < maxCount; i++)
            {
                connect = new Connection();
                ClientConnect.Enqueue(connect);
            }
        }

        public static void Enqueue(Connection connect)
        {
            ClientConnect.Enqueue(connect);
        }

        public static Connection Dequeue()
        {
            if (ClientConnect.Count > 0)
            {
                return ClientConnect.Dequeue();
            }
            return null;
        }
    }
}
