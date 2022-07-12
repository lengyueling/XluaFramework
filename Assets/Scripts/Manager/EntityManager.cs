using System;
using System.Collections.Generic;
using Unity;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 实体热更新管理（模型特效等）
/// </summary>
public class EntityManager : MonoBehaviour
{
    /// <summary>
    /// 实体缓存
    /// </summary>
    Dictionary<string, GameObject> m_Entities = new Dictionary<string, GameObject>();
    /// <summary>
    /// 实体分组
    /// </summary>
    Dictionary<string, Transform> m_Groups = new Dictionary<string, Transform>();
    /// <summary>
    /// 实体根节点
    /// </summary>
    private Transform m_EntityParent;

    private void Awake()
    {
        m_EntityParent = this.transform.parent.Find("Entity");
    }

    /// <summary>
    /// 设置实体层级分组
    /// UI分组也需要热更新
    /// 因此该在lua脚本中被调用
    /// </summary>
    /// <param name="groups">lua代码中进行热更新传递分组</param>
    public void SetEntityGroup(List<string> groups)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            GameObject group = new GameObject("Group-" + groups[i]);
            group.transform.SetParent(m_EntityParent, false);
            m_Groups[groups[i]] = group.transform;
        }
    }

    /// <summary>
    /// 获取实体分组
    /// </summary>
    /// <param name="group">返回具体的UI分组，作为载入实体的父物体</param>
    /// <returns></returns>
    Transform GetGroup(string group)
    {
        if (!m_Groups.ContainsKey(group))
            Debug.LogError("group is not exist");
        return m_Groups[group];
    }

    /// <summary>
    /// 显示实体
    /// 实例化对应实体
    /// Lua代码中编写
    /// </summary>
    /// <param name="name">要打开UI资源的名字</param>
    /// <param name="group">实体分组名</param>
    /// <param name="luaName">Lua模拟MonoBehaviour使用的文件</param>
    public void ShowEntity(string name, string group, string luaName)
    {
        GameObject entity = null;
        if (m_Entities.TryGetValue(name, out entity))
        {
            EntityLogic logic = entity.GetComponent<EntityLogic>();
            logic.OnShow();
            return;
        }

        Manager.Resource.LoadPrefab(name, (UnityEngine.Object obj) =>
        {
            entity = Instantiate(obj) as GameObject;
            Transform parent = GetGroup(group);
            entity.transform.SetParent(parent, false);
            m_Entities.Add(name, entity);
            EntityLogic entityLogic = entity.AddComponent<EntityLogic>();
            entityLogic.Init(luaName);
            entityLogic.OnShow();
        });
    }
}
