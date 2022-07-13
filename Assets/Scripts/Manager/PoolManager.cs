using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    /// <summary>
    /// 管理器的根节点
    /// </summary>
    Transform m_PoolParent;

    /// <summary>
    /// 对象池字典
    /// 管理所有不同类型的对象池
    /// </summary>
    Dictionary<string, PoolBase> m_Pools = new Dictionary<string, PoolBase>();

    void Awake()
    {
        m_PoolParent = this.transform.parent.Find("Pool");
    }

    /// <summary>
    /// 创建对象池
    /// </summary>
    /// <typeparam name="T">游戏物体对象池或者ab资源对象池</typeparam>
    /// <param name="poolName">对象池名字</param>
    /// <param name="releaseTime">自动释放时间</param>
    private void CreatePool<T>(string poolName, float releaseTime) where T : PoolBase
    {
        //如果对象池字典中没有对应的对象则新建一个对象池
        if (!m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            GameObject go = new GameObject(poolName);
            go.transform.SetParent(m_PoolParent);
            pool = go.AddComponent<T>();
            pool.Init(releaseTime);
            m_Pools.Add(poolName, pool);
        }
    }

    /// <summary>
    /// 创建游戏物体对象池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    public void CreateGameObjectPool(string poolName, float releaseTime)
    {
        CreatePool<GameObjectPool>(poolName, releaseTime);
    }

    /// <summary>
    /// 创建ab资源对象池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    public void CreateAssetPool(string poolName, float releaseTime)
    {
        CreatePool<AssetPool>(poolName, releaseTime);
    }

    /// <summary>
    /// 取出对象池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public Object Spawn(string poolName, string assetName)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            return pool.Spwan(assetName);
        }
        return null;
    }

    /// <summary>
    /// 回收对象池
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="assetName"></param>
    /// <param name="asset"></param>
    public void UnSpawn(string poolName, string assetName, Object asset)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            pool.UnSpwan(assetName, asset);
        }
    }

}
