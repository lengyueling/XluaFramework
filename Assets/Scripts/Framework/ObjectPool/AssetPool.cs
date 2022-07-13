using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// ab资源对象池类
/// </summary>
public class AssetPool : PoolBase
{
    public override Object Spwan(string name)
    {
        return base.Spwan(name);
    }

    public override void UnSpwan(string name, Object obj)
    {
        base.UnSpwan(name, obj);
    }

    /// <summary>
    /// ab资源释放对象
    /// 需要卸载ab包
    /// </summary>
    public override void Release()
    {
        base.Release();
        foreach (PoolObject item in m_Objects)
        {
            if (DateTime.Now.Ticks - item.LastUseTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.Log("AssetPool 释放时间:" + DateTime.Now + " 卸载 ab :" + item.Name);
                Manager.Resource.UnloadBundle(item.Object);
                m_Objects.Remove(item);
                Release();
                return;
            }
        }
    }
}
