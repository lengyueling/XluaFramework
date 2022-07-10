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

    public void OpenUI(string uiName,string luaName)
    {
        GameObject ui = null;
        //如果之前已经缓存过了这个ui则直接调用
        if (m_UI.TryGetValue(uiName,out ui))
        {
            UILogic uILogic = ui.GetComponent<UILogic>();
            uILogic.OnOpen();
            return;
        }
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
