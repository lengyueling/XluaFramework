using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class PoolBase : MonoBehaviour
{
    /// <summary>
    /// 自动释放时间(秒)
    /// </summary>
    protected float m_ReleaseTime;

    /// <summary>
    /// 上次释放资源的时间(毫微秒)   
    /// 1秒 = 10^7毫微秒
    /// 到了时间就会自动释放资源
    /// </summary>
    protected long m_LastReleaseTime = 0;

    /// <summary>
    /// 真正放对象的对象池
    /// </summary>
    protected List<PoolObject> m_Objects;

    public void Start()
    {
        //初始化释放时间
        m_LastReleaseTime = DateTime.Now.Ticks;
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    /// <param name="time">自动释放时间</param>
    public void Init(float time)
    {
        m_ReleaseTime = time;
        m_Objects = new List<PoolObject>();
    }

    /// <summary>
    /// 取出对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns>返回被取出的对象</returns>
    public virtual Object Spwan(string name)
    {
        foreach (PoolObject po in m_Objects)
        {
            //如果找到了对象则移出对象池list集合，否则返回空
            if (po.Name == name)
            {
                m_Objects.Remove(po);
                return po.Object;
            }
        }
        return null;
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public virtual void UnSpwan(string name, Object obj)
    {
        //实例化对象并增加到对象池
        PoolObject po = new PoolObject(name, obj);
        m_Objects.Add(po);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public virtual void Release()
    {

    }

    void Update()
    {
        //到了该释放资源的时间，重置时间并释放资源
        if (DateTime.Now.Ticks - m_LastReleaseTime >= m_ReleaseTime * 10000000)
        {
            m_LastReleaseTime = DateTime.Now.Ticks;
            Release();
        }
    }
}
