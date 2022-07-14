using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using XLua;

public class GameStart : MonoBehaviour
{
    public GameMode GameMode;
    public bool OpenLog;
    void Start()
    {
        Manager.Event.Subscribe((int)GameEvent.StartLua, StartLua);
        Manager.Event.Subscribe((int)GameEvent.GameInit, GameInit);
        AppConst.GameMode = this.GameMode;
        AppConst.OpenLog = this.OpenLog;
        DontDestroyOnLoad(this);

        if (AppConst.GameMode == GameMode.UpdateMode)
        {
            this.gameObject.AddComponent<HotUpdate>();
        } 
        else
        {
            Manager.Event.Fire((int)GameEvent.GameInit);
        }
    }

    /// <summary>
    /// GameInit的回调函数
    /// </summary>
    /// <param name="args"></param>
    private void GameInit(object args)
    {
        if (AppConst.GameMode != GameMode.EditorMode)
        {
            Manager.Resource.ParseVersionFile();
        }
        Manager.Lua.Init();
    }

    /// <summary>
    /// StartLua的回调函数
    /// </summary>
    /// <param name="args"></param>
    private void StartLua(object args)
    {
        Manager.Lua.StartLua("main");
        LuaFunction func = Manager.Lua.LuaEnv.Global.Get<LuaFunction>("Main");
        func.Call();

        Manager.Pool.CreateGameObjectPool("UI", 10);
        Manager.Pool.CreateGameObjectPool("Monster", 120);
        Manager.Pool.CreateGameObjectPool("Effect", 120);
        Manager.Pool.CreateAssetPool("AssetBundle", 10);
    }

    private void OnApplicationQuit()
    {
        Manager.Event.UnSubscribe((int)GameEvent.StartLua, StartLua);
        Manager.Event.UnSubscribe((int)GameEvent.GameInit, GameInit);
    }
}
