using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameStart : MonoBehaviour
{
    public GameMode GameMode;
    void Start()
    {
        AppConst.GameMode = GameMode;
        DontDestroyOnLoad(this);

        Manager.Resource.ParseVersonFile();
        Manager.Lua.Init(
            ()=> 
            {
                Manager.Lua.StartLua("main");
                XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
                func.Call();
            });
    }
}
