using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityLogic : LuaBehaviour
{
    /// <summary>
    /// 实体资源展示的委托函数
    /// </summary>
    private Action m_LuaOnShow;
    /// <summary>
    /// 实体资源隐藏的委托函数
    /// </summary>
    private Action m_LuaOnHide;

    /// <summary>
    /// 实体资源初始化时候执行
    /// 重写父类虚函数
    /// </summary>
    /// <param name="luaName"></param>
    public override void Init(string luaName)
    {
        base.Init(luaName);
        //让lua OnOpen函数指定在 m_LuaOnOpen执行时候触发
        m_ScriptEnv.Get("OnShow", out m_LuaOnShow);
        m_ScriptEnv.Get("OnHide", out m_LuaOnHide);
    }
    /// <summary>
    /// 展示实体资源的时候执行
    /// </summary>
    public void OnShow()
    {
        m_LuaOnShow?.Invoke();
    }

    /// <summary>
    /// 隐藏实体资源的时候执行
    /// </summary>
    public void OnHide()
    {
        m_LuaOnHide?.Invoke();
    }

    /// <summary>
    /// 释放资源
    /// 重写父类虚方法
    /// </summary>
    protected override void Clear()
    {
        base.Clear();
        m_LuaOnShow = null;
        m_LuaOnHide = null;
    }
}
