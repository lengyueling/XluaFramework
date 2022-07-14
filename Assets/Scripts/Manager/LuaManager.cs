using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;
using XLua.LuaDLL;
using Object = UnityEngine.Object;

public class LuaManager : MonoBehaviour
{
    /// <summary>
    /// 所有需要加载的Lua文件文件名
    /// </summary>
    public List<string> LuaNames = new List<string>();

    /// <summary>
    /// lua脚本的内容的缓存
    /// </summary>
    private Dictionary<string, byte[]> m_LuaScripts;

    /// <summary>
    /// xlua虚拟机
    /// 全局只有一个
    /// </summary>
    public LuaEnv LuaEnv;
    
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        LuaEnv = new LuaEnv();
        //调用AddBuildin
        LuaEnv.AddBuildin("rapidjson", Lua.LoadRapidJson);
        //lua虚拟机注册回调获取lua脚本
        LuaEnv.AddLoader(Loader);

        m_LuaScripts = new Dictionary<string, byte[]>();
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
        {
            EditorLoadLuaScript();
        }
        else
#endif
        {
            LoadLuaScript();
        }
    }

    public void StartLua(string name)
    {
        //加载lua文件
        LuaEnv.DoString(string.Format("require '{0}'", name));
    }

    /// <summary>
    /// 自定义Loader获取Lua脚本
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    /// <summary>
    /// 获取Lua脚本
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte[] GetLuaScript(string name)
    {
        //requeir ui.login.register 将.取代为/
        name = name.Replace(".", "/");
        string fileName = PathUtil.GetLuaPath(name);
        //获取lua脚本的数据
        byte[] luaScript = null;
        //如果之前加载过这个数据，直接在缓存里取即可
        if (!m_LuaScripts.TryGetValue(fileName,out luaScript))
        {
            Debug.LogError("Lua脚本不存在:" + fileName);
        }
        return luaScript;
    }

    /// <summary>
    /// 加载lua脚本
    /// </summary>
    void LoadLuaScript()
    {
        foreach (string name in LuaNames)
        {
            Manager.Resource.LoadLua(name, (Object obj) =>
             {
                 AddLuaScript(name, (obj as TextAsset).bytes);
                 //如果缓存中文件的数量>=需要加载的数量
                 //就说明加载完成了
                 if (m_LuaScripts.Count >= LuaNames.Count)
                 {
                     //执行init事件
                     Manager.Event.Fire((int)GameEvent.StartLua);
                     //重置需要加载文件的列表
                     LuaNames.Clear();
                     LuaNames = null;
                 }
             });
        }
    }

    /// <summary>
    /// 添加lua脚本到缓存中
    /// </summary>
    /// <param name="assetNmae"></param>
    /// <param name="luaScript"></param>
    public void AddLuaScript(string assetsName,byte[] luaScript)
    {
        //覆盖添加,比较安全
        //m_LuaScripts.Add(assetNmae, luaScript);需要再判断是否重复添加
        m_LuaScripts[assetsName] = luaScript;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式加载lua
    /// 编辑器模式下直接在assets资源目录找对应的lua脚本
    /// </summary>
    void EditorLoadLuaScript()
    {
        string[] luaFiles = Directory.GetFiles(PathUtil.LuaPath, "*.bytes", SearchOption.AllDirectories);
        for (int i = 0; i < luaFiles.Length; i++)
        {
            string fileName = PathUtil.GetStandardPath(luaFiles[i]);
            byte[] file = File.ReadAllBytes(fileName);
            AddLuaScript(PathUtil.GetUnityPath(fileName), file);
        }
        //执行init事件
        Manager.Event.Fire((int)GameEvent.StartLua);
    }
#endif

    private void Update()
    {
        if (LuaEnv != null)
        {
            //释放内存
            //GC回收
            LuaEnv.Tick();
        }
    }

    private void OnDestroy()
    {
        if (LuaEnv != null)
        {
            //销毁虚拟机
            LuaEnv.Dispose();
            LuaEnv = null;
        }
    }
}
