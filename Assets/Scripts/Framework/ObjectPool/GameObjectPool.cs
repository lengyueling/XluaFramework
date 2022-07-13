using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏对象对象池类
/// </summary>
public class GameObjectPool : PoolBase
{
    /// <summary>
    /// 游戏物体取出对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override Object Spwan(string name)
    {
        Object obj = base.Spwan(name);
        if (obj == null)
        {
            return null;
        }
        GameObject go = obj as GameObject;
        go.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 游戏物体回收对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public override void UnSpwan(string name, Object obj)
    {
        GameObject go = obj as GameObject;
        go.SetActive(false);
        go.transform.SetParent(this.transform, false);
        base.UnSpwan(name, obj);
    }

    /// <summary>
    /// 游戏对象释放资源
    /// 释放资源需要销毁对象
    /// </summary>
    public override void Release()
    {
        base.Release();
        foreach (PoolObject item in m_Objects)
        {
            //遍历每一个对象，到了该释放的时间，就释放资源
            if (System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("GameObjectPool 释放时间:" + System.DateTime.Now);
                Destroy(item.Object);
                //Manager.Resource.MinusBundleCount(item.Name);
                m_Objects.Remove(item);
                //递归自己，让自己能够跳出函数
                Release();
                return;
            }
        }
    }
}
