using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    /// <summary>
    /// 事件的回调
    /// </summary>
    /// <param name="args">
    /// 给lua用来传递参数用的，
    /// 多个参数可以封装成一个table
    /// </param>
    public delegate void EventHandler(object args);

    /// <summary>
    /// 管理所有的事件的字典
    /// </summary>
    Dictionary<int, EventHandler> m_Events = new Dictionary<int, EventHandler>();

    /// <summary>
    /// 指定id订阅事件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void Subscribe(int id, EventHandler e)
    {
        //如果指定id已经订阅过委托了
        if (m_Events.ContainsKey(id))
        {
            //使用多播委托，让一个id可以存放多个事件
            m_Events[id] += e;
        }
        else
        {
            //如果没有订阅过则直接添加即可
            m_Events.Add(id, e);
        }
    }

    /// <summary>
    /// 指定id取消订阅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void UnSubscribe(int id, EventHandler e)
    {
        if (m_Events.ContainsKey(id))
        {
            if (m_Events[id] != null)
                m_Events[id] -= e;

            if (m_Events[id] == null)
                m_Events.Remove(id);
        }
    }

    /// <summary>
    /// 执行事件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="args">参数</param>
    public void Fire(int id, object args = null)
    {
        //定义委托变量
        EventHandler handler;
        if (m_Events.TryGetValue(id, out handler))
        {
            //执行委托的事件
            handler(args);
        }
    }
}
