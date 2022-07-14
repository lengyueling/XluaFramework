using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetClient
{
    /// <summary>
    /// 一个tcp socket
    /// </summary>
    private TcpClient m_Client;
    /// <summary>
    /// tcp网络数据流
    /// </summary>
    private NetworkStream m_TcpStream;
    /// <summary>
    /// 接受数据的大小
    /// </summary>
    private const int BufferSize = 1024 * 64;
    /// <summary>
    /// 接受数据的数据缓冲区
    /// </summary>
    private byte[] m_Buffer = new byte[BufferSize];

    //解析接收到的数据
    private MemoryStream m_MemStream;
    private BinaryReader m_BinaryReader;

    public NetClient()
    {
        //实例化
        m_MemStream = new MemoryStream();
        m_BinaryReader = new BinaryReader(m_MemStream);
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public void OnConnectServer(string host, int port)
    {
        try
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length == 0)
            {
                Debug.LogError("host无效");
                return;
            }
            if (addresses[0].AddressFamily == AddressFamily.InterNetworkV6)
            {
                m_Client = new TcpClient(AddressFamily.InterNetworkV6);
            }
            else
            {
                m_Client = new TcpClient(AddressFamily.InterNetwork);
            }
                
            m_Client.SendTimeout = 1000;
            m_Client.ReceiveTimeout = 1000;
            m_Client.NoDelay = true;
            m_Client.BeginConnect(host, port, OnConnect, null);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// 连接服务器的回调函数
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnConnect(IAsyncResult asyncResult)
    {

        if (m_Client == null || !m_Client.Connected)
        {
            Debug.LogError("connect server error!!!");
            return;
        }
        Manager.Net.OnNetConnected();
        m_TcpStream = m_Client.GetStream();
        //开始接受数据
        m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
    }

    /// <summary>
    /// 接收到数据的回调函数
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnRead(IAsyncResult asyncResult)
    {
        try
        {
            if (m_Client == null || m_TcpStream == null)
            {
                return;
            }
                
            //收到的消息长度
            int length = m_TcpStream.EndRead(asyncResult);

            if (length < 1)
            {
                OnDisConnected();
                return;
            }
            ReceiveData(length);
            lock (m_TcpStream)
            {
                Array.Clear(m_Buffer, 0, m_Buffer.Length);
                m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            OnDisConnected();
        }
    }
    /// <summary>
    /// 解析数据
    /// </summary>
    private void ReceiveData(int len)
    {
        m_MemStream.Seek(0, SeekOrigin.End);
        m_MemStream.Write(m_Buffer, 0, len);
        m_MemStream.Seek(0, SeekOrigin.Begin);
        while (RemainingBytesLength() >= 8)
        {
            int msgId = m_BinaryReader.ReadInt32();
            int msgLen = m_BinaryReader.ReadInt32();
            if (RemainingBytesLength() >= msgLen)
            {
                byte[] data = m_BinaryReader.ReadBytes(msgLen);
                string message = System.Text.Encoding.UTF8.GetString(data);

                //转到lua
                Manager.Net.Receive(msgId, message);
            }
            else
            {
                m_MemStream.Position = m_MemStream.Position - 8;
                break;
            }
        }
        //剩余字节
        byte[] leftover = m_BinaryReader.ReadBytes(RemainingBytesLength());
        m_MemStream.SetLength(0);
        m_MemStream.Write(leftover, 0, leftover.Length);
    }

    /// <summary>
    /// 剩余长度
    /// </summary>
    /// <returns></returns>
    private int RemainingBytesLength()
    {
        return (int)(m_MemStream.Length - m_MemStream.Position);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="msgID"></param>
    /// <param name="message"></param>
    public void SendMessage(int msgID, string message)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            //协议id
            bw.Write(msgID);
            //消息长度
            bw.Write((int)data.Length);
            //消息内容
            bw.Write(data);
            bw.Flush();
            if (m_Client != null && m_Client.Connected)
            {
                byte[] sendData = ms.ToArray();
                m_TcpStream.BeginWrite(sendData, 0, sendData.Length, OnEndSend, null);
            }
            else
            {
                Debug.LogError("服务器未连接");
            }
        }
    }

    void OnEndSend(IAsyncResult ar)
    {
        try
        {
            m_TcpStream.EndWrite(ar);
        }
        catch (Exception ex)
        {
            OnDisConnected();
            Debug.LogError(ex.Message);
        }
    }


    public void OnDisConnected()
    {
        if (m_Client != null && m_Client.Connected)
        {
            m_Client.Close();
            m_Client = null;

            m_TcpStream.Close();
            m_TcpStream = null;
        }
        Manager.Net.OnDisConnected();
    }
}
