using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// 缓存UI
    /// </summary>
    Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();

    /// <summary>
    /// 打开UI
    /// 执行实例化对应UI
    /// lua代码中编写
    /// </summary>
    /// <param name="uiName">要打开UI资源的名字</param>
    /// <param name="luaName">Lua模拟MonoBehaviour使用的文件</param>
    public void OpenUI(string uiName,string luaName)
    {
        GameObject ui = null;
        //如果之前已经缓存（使用）过了这个ui则直接调用，不需要重新加载资源
        if (m_UI.TryGetValue(uiName,out ui))
        {
            UILogic uILogic = ui.GetComponent<UILogic>();
            uILogic.OnOpen();
            return;
        }
        //资源加载并执行lambda表达式，初始化并执行OnOpen()函数
        Manager.Resource.LoadUI(uiName, (Object obj) =>
         {
             ui = Instantiate(obj) as GameObject;
             m_UI.Add(uiName, ui);
             UILogic uiLogic = ui.AddComponent<UILogic>();
             uiLogic.Init(luaName);
             uiLogic.OnOpen();
         });
    }
}
