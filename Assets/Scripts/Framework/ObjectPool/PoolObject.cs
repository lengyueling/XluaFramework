using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 存放各种类型对象池的数据类
/// </summary>
public class PoolObject
{
    /// <summary>
    /// 具体对象
    /// </summary>
    public Object Object;

    /// <summary>
    /// 对象名字
    /// </summary>
    public string Name;

    /// <summary>
    /// 最后一次使用时间
    /// （定期销毁）
    /// </summary>
    public DateTime LastUseTime;

    /// <summary>
    /// 对象池对象
    /// 构造函数
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public PoolObject(string name, Object obj)
    {
        Name = name;
        Object = obj;
        LastUseTime = DateTime.Now;
    }
}
