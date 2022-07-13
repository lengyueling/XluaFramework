using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILogic : LuaBehaviour
{
    /// <summary>
    /// 资源名字
    /// 用于对象池
    /// </summary>
	public string AssetName;
    /// <summary>
    /// 打开UI资源执行的委托函数
    /// </summary>
    private Action m_LuaOnOpen;
    /// <summary>
    /// 关闭UI资源执行的委托函数
    /// </summary>
    private Action m_LuaOnClose;

    /// <summary>
    /// UI初始化时候执行
    /// 重写父类虚函数
    /// </summary>
    /// <param name="luaName"></param>
    public override void Init(string luaName)
    {
        base.Init(luaName);
        //让lua OnOpen函数指定在 m_LuaOnOpen执行时候触发
        m_ScriptEnv.Get("OnOpen", out m_LuaOnOpen);
        m_ScriptEnv.Get("OnClose", out m_LuaOnClose);
    }
    /// <summary>
    /// 打开UI资源的时候执行
    /// </summary>
    public void OnOpen()
    {
        m_LuaOnOpen?.Invoke();
    }

    /// <summary>
    /// 关闭UI资源的时候执行
    /// </summary>
    public void OnClose()
    {
        m_LuaOnClose?.Invoke();
        Manager.Pool.UnSpawn("UI", AssetName, this.gameObject);
    }

    /// <summary>
    /// 释放资源
    /// 重写父类虚方法
    /// </summary>
    protected override void Clear()
    {
        base.Clear();
        m_LuaOnOpen = null;
        m_LuaOnClose = null;
    }
}
