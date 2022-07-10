using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    private LuaEnv m_LuaEnv = Manager.Lua.LuaEnv;
    /// <summary>
    /// Lua运行环境
    /// 子类需要用到 所以不是private的
    /// </summary>
    protected LuaTable m_ScriptEnv;

    private Action m_LuaInit;

    private Action m_LuaUpdate;

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

    public virtual void Init(string luaName)
    {
        m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);
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
