using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SceneLogic : LuaBehaviour
{
    /// <summary>
    /// 场景名
    /// 可以在lua脚本中调用
    /// </summary>
    public string SceneName;

    Action m_LuaActive;
    Action m_LuaInActive;
    Action m_LuaOnEnter;
    Action m_LuaOnQuit;

    public override void Init(string luaName)
    {
        base.Init(luaName);
        m_ScriptEnv.Get("OnActive", out m_LuaActive);
        m_ScriptEnv.Get("OnInActive", out m_LuaInActive);
        m_ScriptEnv.Get("OnEnter", out m_LuaOnEnter);
        m_ScriptEnv.Get("OnQuit", out m_LuaOnQuit);
    }

    public void OnActive()
    {
        m_LuaInActive?.Invoke();
    }

    public void OnInActive()
    {
        m_LuaInActive?.Invoke();
    }

    public void OnEnter()
    {
        m_LuaOnEnter?.Invoke();
    }

    public void OnQuit()
    {
        m_LuaOnQuit?.Invoke();
    }

    protected override void Clear()
    {
        base.Clear();
        m_LuaActive = null;
        m_LuaInActive = null;
        m_LuaOnEnter = null;
        m_LuaOnQuit = null;
    }
}
