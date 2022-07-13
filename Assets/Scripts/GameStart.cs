using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using XLua;

public class GameStart : MonoBehaviour
{
    public GameMode GameMode;
    void Start()
    {
        Manager.Event.Subscribe(10000, onLuaInit);
        AppConst.GameMode = GameMode;
        DontDestroyOnLoad(this);

        Manager.Resource.ParseVersonFile();
        Manager.Lua.Init();
    }

    void onLuaInit(object args)
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
        Manager.Event.UnSubscribe(10000, onLuaInit);
    }
}
