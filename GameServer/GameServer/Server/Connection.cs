using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
   public class Connection
    {
        //客户端Socket
        private Socket m_ClientSocket;
        private SocketAsyncEventArgs m_ReceiveArgs;
        private SocketAsyncEventArgs m_SendArgs;
        private MessageHandler m_MessageHandler;

        //消息缓存区
        private List<byte> m_DataCache = new List<byte>();

        public Connection()
        {
            m_MessageHandler = new MessageHandler(this);
            m_ReceiveArgs = new SocketAsyncEventArgs();
            m_ReceiveArgs.Completed += OnReceiveComplete;
            m_ReceiveArgs.SetBuffer(new byte[64 * 1024], 0, 64 * 1024);

            m_SendArgs = new SocketAsyncEventArgs();
            m_SendArgs.Completed += OnSendComplete;
            m_SendArgs.SetBuffer(new byte[64 * 1024], 0, 64 * 1024);
        }

        

        public Socket ClientSocket
        {
            set { m_ClientSocket = value; }
            get { return m_ClientSocket; }
        }

        public SocketAsyncEventArgs SocketArgs
        {
            get { return m_ReceiveArgs; }
        }

        public void StartReceive()
        {
            if (ClientSocket.Connected)
            {
                bool result = m_ClientSocket.ReceiveAsync(SocketArgs);
            }
        }

        private void OnReceiveComplete(object sender, SocketAsyncEventArgs receiveArgs)
        {
            if (receiveArgs.SocketError == SocketError.Success && receiveArgs.BytesTransferred > 0)
            {
                byte[] data = new byte[receiveArgs.BytesTransferred];
                Buffer.BlockCopy(receiveArgs.Buffer, 0, data, 0, receiveArgs.BytesTransferred);
                //解析数据
                Receive(data);
                StartReceive();
            }
            else if (receiveArgs.BytesTransferred == 0)
            {
                if (receiveArgs.SocketError == SocketError.Success)
                {
                    //Disconnect(client, "客户端主动断开连接");
                }
                else
                {
                    //Disconnect(client, client.SocketArgs.SocketError.ToString());
                }
            }
        }

        private void OnSendComplete(object sender, SocketAsyncEventArgs e)
        {
            
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        public void Receive(byte[] data)
        {
            if (data.Length < 8)
                return;
            m_MessageHandler.ParseMessage(data);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public void Send<T>(T message)
        {
            byte[] data = m_MessageHandler.PackMessage(message);
            m_SendArgs.SetBuffer(data, 0, data.Length);
            ClientSocket.SendAsync(m_SendArgs);
        }

        private void Disconnect(Connection client, string reason)
        {
            try
            {
                
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
