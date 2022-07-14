using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace GameServer
{
    public class MessageHandler
    {
        private Connection m_Connnection;
        private MemoryStream m_MemoryStream;
        private BinaryWriter m_BinaryWriter;
        private BinaryReader m_BinaryReader;
        public MessageHandler(Connection c)
        {
            m_Connnection = c;
            m_MemoryStream = new MemoryStream(64 * 1024);
            m_BinaryReader = new BinaryReader(m_MemoryStream);
            m_BinaryWriter = new BinaryWriter(m_MemoryStream);
        }
        /// <summary>
        /// 消息打包
        /// </summary>
        public byte[] PackMessage<T>(T message)
        {
            //转json
            string jsonData = JsonConvert.SerializeObject(message);
            //获取消息id
            MessageID id = Message.Instance.GetMessageID<T>();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);

            //写入数据
            m_BinaryWriter.Write((int)id);
            m_BinaryWriter.Write(data.Length);
            m_BinaryWriter.Write(data);
            m_BinaryWriter.Flush();

            //拷贝消息包
            byte[] package = new byte[m_MemoryStream.Length];
            Buffer.BlockCopy(m_MemoryStream.GetBuffer(), 0, package, 0, (int)m_MemoryStream.Length);
            m_MemoryStream.Seek(0,SeekOrigin.Begin);
            return package;
        }

        /// <summary>
        /// 消息解析
        /// </summary>
        public void ParseMessage(byte [] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    if (data.Length < 8)
                        return;
                    int msgID = br.ReadInt32();
                    int length = br.ReadInt32();
                    if (ms.Length - ms.Position >= length)
                    {
                        byte[] msg = br.ReadBytes(length);
                        string msgStr = System.Text.Encoding.UTF8.GetString(msg, 0, length);
                        string msgName = ((MessageID)msgID).ToString();
                        Console.WriteLine("message id = " + msgID + "  message:" + msgStr);
                        Type t = Type.GetType(msgName);
                        object msgData = JsonConvert.DeserializeObject(msgStr,t);
                        Message.Instance.Distribute(new MessageArgs() { sender = m_Connnection, id = (MessageID)msgID, message = msgData });
                    }
                }
            }
        }
    }
}
