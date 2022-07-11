using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    /// <summary>
    /// 复用LuaManager中的xlua虚拟机
    /// 因为虚拟机全局只能有一个
    /// </summary>
    private LuaEnv m_LuaEnv = Manager.Lua.LuaEnv;
    /// <summary>
    /// Lua运行环境
    /// 子类需要用到，所以不是private的
    /// </summary>
    protected LuaTable m_ScriptEnv;
    /// <summary>
    /// 初始化完成后执行的委托函数
    /// 代替MonoBehaviour中类似Start的函数
    /// </summary>
    private Action m_LuaInit;
    /// <summary>
    /// 每帧执行的委托函数
    /// Lua中模拟MonoBehaviour中的Update
    /// </summary>
    private Action m_LuaUpdate;
    /// <summary>
    /// 销毁资源时执行
    /// Lua中模拟MonoBehaviour中的OnDestroy
    /// </summary>
    private Action m_LuaOnDestroy;

    private void Awake()
    {
        m_ScriptEnv = m_LuaEnv.NewTable();
        // 为每个继承的脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        LuaTable meta = m_LuaEnv.NewTable();
        meta.Set("__index", m_LuaEnv.Global);
        m_ScriptEnv.SetMetaTable(meta);
        meta.Dispose();
        m_ScriptEnv.Set("self", this);
    }

    /// <summary>
    /// 初始化资源时候执行
    /// </summary>
    /// <param name="luaName"></param>
    public virtual void Init(string luaName)
    {
        m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);
        //初始化完成后执行，开始
        m_ScriptEnv.Get("Update", out m_LuaUpdate);
        m_ScriptEnv.Get("OnInit", out m_LuaInit);
        m_LuaInit?.Invoke();
    }

    void Update()
    {
        m_LuaUpdate?.Invoke();
    }

    /// <summary>
    /// 释放资源
    /// 虚方法允许子类重写
    /// 关闭游戏或者资源销毁时执行
    /// </summary>
    protected virtual void Clear()
    {
        m_LuaOnDestroy = null;
        m_ScriptEnv?.Dispose();
        m_ScriptEnv = null;
        m_LuaInit = null;
        m_LuaUpdate = null;
    }

    private void OnDestroy()
    {
        m_LuaOnDestroy?.Invoke();
        Clear();
    }

    private void OnApplicationQuit()
    {
        Clear();
    }
}
