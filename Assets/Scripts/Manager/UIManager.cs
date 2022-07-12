using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// UI热更新管理
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// UI缓存字典
    /// </summary>
    Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();
    /// <summary>
    /// UI层级分组字典
    /// </summary>
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();
    /// <summary>
    /// UI根节点
    /// </summary>
    private Transform m_UIParent;
    private void Awake()
    {
        m_UIParent = this.transform.parent.Find("UI");
    }

    /// <summary>
    /// 设置UI层级分组
    /// UI分组也需要热更新
    /// 因此该在lua脚本中被调用
    /// </summary>
    /// <param name="group">lua代码中进行热更新传递分组</param>
    public void SetUIGroup(List<string> group)
    {
        for (int i = 0; i < group.Count; i++)
        {
            GameObject go = new GameObject("Group-" + group[i]);
            //flase意味着不保持世界坐标而是跟随父节点
            go.transform.SetParent(m_UIParent, false);
            m_UIGroups.Add(group[i], go.transform);
        }
    }

    /// <summary>
    /// 获取UI分组
    /// </summary>
    /// <returns>返回具体的UI分组，作为载入ui的父物体</returns>
    private Transform GetUIGroup(string group)
    {
        if (!m_UIGroups.ContainsKey(group))
        {
            Debug.Log("分组不存在");
        }
        return m_UIGroups[group];
    }

    /// <summary>
    /// 打开UI
    /// 执行实例化对应UI
    /// lua代码中编写
    /// </summary>
    /// <param name="uiName">要打开UI资源的名字</param>
    /// <param name="luaName">使用的Lua文件</param>
    /// <param name="group">ui分组名</param>
    public void OpenUI(string uiName, string group, string luaName)
    {
        GameObject ui = null;
        //如果之前已经缓存（使用）过了这个ui则直接调用，不需要重新加载资源
        if (m_UI.TryGetValue(uiName,out ui))
        {
            UILogic uiLogic = ui.GetComponent<UILogic>();
            uiLogic.OnOpen();
            return;
        }
        //资源加载并执行lambda表达式
        Manager.Resource.LoadUI(uiName, (Object obj) =>
         {
             ui = Instantiate(obj) as GameObject;
             m_UI.Add(uiName, ui);
             //设置当前ui的父物体，是各个分组名所在的对象
             Transform parent = GetUIGroup(group);
             ui.transform.SetParent(parent, false);
             //执行初始化 OnOpen()函数
             UILogic uiLogic = ui.AddComponent<UILogic>();
             uiLogic.Init(luaName);
             uiLogic.OnOpen();
         });
    }
}
