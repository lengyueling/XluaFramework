using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Object = UnityEngine.Object;
using UnityEditor;

/// <summary>
/// 资源加载管理器
/// </summary>
public class ResourceManager : MonoBehaviour
{
    internal class BundleInfo
    {
        public string AssetsName;
        public string BundleName;
        /// <summary>
        /// 依赖文件的列表
        /// </summary>
        public List<string> Dependences;
    }

    /// <summary>
    /// ab资源数据
    /// 用于对象池
    /// </summary>
    internal class BundleData
    {
        public AssetBundle Bundle;
        /// <summary>
        /// 引用次数
        /// </summary>
        public int Count;

        /// <summary>
        /// 内部类构造函数
        /// </summary>
        /// <param name="ab"></param>
        public BundleData(AssetBundle ab)
        {
            Bundle = ab;
            Count = 1;
        }
    }

    /// <summary>
    /// 存放解析出来的Bundle信息（BundleInfo>）的集合
    /// </summary>
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// 存放Bundle资源的集合
    /// 每次将加载的bundle存起来
    /// 就不会重复加载造成报错了
    /// </summary>
    private Dictionary<string, BundleData> m_AssetBundles = new Dictionary<string, BundleData>();

    /// <summary>
    /// 解析filelist文件
    /// </summary>
    public void ParseVersonFile()
    {
        //获取版本文件路径
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        //读出文件信息
        string[] data = File.ReadAllLines(url);
        //解析文件信息
        for (int i = 0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split('|');
            //文件排列方式：文件路径|bundle名|依赖文件列表
            bundleInfo.AssetsName = info[0];
            bundleInfo.BundleName = info[1];
            bundleInfo.Dependences = new List<string>(info.Length - 2);

            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependences.Add(info[j]);
            }
            m_BundleInfos.Add(bundleInfo.AssetsName, bundleInfo);

            //如果是解析的是lua文件（LuaScripts字符串第一次完整出现时的索引）
            if (info[0].IndexOf("LuaScripts") > 0)
            {
                //加入列表 info[0]已经包含了完整的Assets路径
                Manager.Lua.LuaNames.Add(info[0]);
            }
        }
    }

    /// <summary>
    /// 异步加载资源
    /// LoadAsset的内部函数
    /// </summary>
    /// <param name="assetName">加载资源名</param>
    /// <param name="action">使用委托表示回调函数</param>
    /// <returns></returns>
    IEnumerator LoadBundleAsync(string assetName,Action<Object> action = null)
    {
        string bundleName = m_BundleInfos[assetName].BundleName;
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);
        List<string> dependences = m_BundleInfos[assetName].Dependences;

        //这样写就不会重复加载资源了
        BundleData bundle = GetBundle(bundleName);
        if (bundle == null)
        {
            //尝试从对象池中取出ab资源
            Object obj = Manager.Pool.Spawn("AssetBundle", assetName);
            if (obj != null)
            {
                AssetBundle ab = obj as AssetBundle;
                bundle = new BundleData(ab);
            }
            else
            {
                //加载资源路径
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return request;
                bundle = new BundleData(request.assetBundle);
            }
            m_AssetBundles.Add(bundleName, bundle);
        }

        if (dependences != null && dependences.Count > 0)
        {
            for (int i = 0; i < dependences.Count; i++)
            {
                yield return LoadBundleAsync(dependences[i]);
            }
        }

        //ab包无法加载场景，如果加载的是场景则直接返回
        if (assetName.EndsWith(".unity"))
        {
            action?.Invoke(null);
            yield break;
        }
        //如果的是依赖资源，则不需要去加载资源，这里退出协程
        //性能优化
        if (action == null)
        {
            yield break;
        }

        AssetBundleRequest bundleRequest = bundle.Bundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        //调用回调函数，告诉应用层已经完成加载
        //action?.Invoke(bundleRequest?.asset);与下面语句效果相同
        if (action != null && bundleRequest != null)
        {
            action.Invoke(bundleRequest.asset);
        }
    }
    /// <summary>
    /// 获取m-AssetBundles字典中的bundle资源
    /// BundleData引用次数+1
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    BundleData GetBundle(string name)
    {
        BundleData bundle = null;
        if (m_AssetBundles.TryGetValue(name,out bundle))
        {
            bundle.Count++;
            Debug.Log("bundle引用计数 :" + name + " 当前引用次数 : " + bundle.Count);
            return bundle;
        }
        return null;
    }

    /// <summary>
    /// 减去一个ab资源的引用次数
    /// 释放资源时调用
    /// </summary>
    /// <param name="name"></param>
    private void MinusOneBundleCount(string bundleName)
    {
        if (m_AssetBundles.TryGetValue(bundleName, out BundleData bundle))
        {
            if (bundle.Count > 0)
            {
                bundle.Count--;
                Debug.Log("bundle名 :" + bundleName + " 当前引用次数 : " + bundle.Count);
            }
            if (bundle.Count <= 0)
            {
                Debug.Log("放入bundle对象池 :" + bundleName);
                Manager.Pool.UnSpawn("AssetBundle", bundleName, bundle.Bundle);
                m_AssetBundles.Remove(bundleName);
            }
        }
    }

    /// <summary>
    /// 减去ab资源及其依赖的引用计数
    /// </summary>
    /// <param name="assetName"></param>
    public void MinusBundleCount(string assetName)
    {
        string bundleName = m_BundleInfos[assetName].BundleName;

        MinusOneBundleCount(bundleName);

        //依赖资源
        List<string> dependences = m_BundleInfos[assetName].Dependences;
        if (dependences != null)
        {
            foreach (string dependence in dependences)
            {
                string name = m_BundleInfos[dependence].BundleName;
                MinusOneBundleCount(name);
            }
        }
    }



#if UNITY_EDITOR
    /// <summary>
    /// 编辑器环境加载资源
    /// LoadAsset的内部函数
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    void EditorLoadAsset(string assetName,Action<Object> action = null)
    {
        Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(Object));
        if (obj == null)
        {
            Debug.LogError("资源文件不存在" + assetName);
        }
        //执行回调函数，前提是action不为null
        action?.Invoke(obj);
    }
#endif

    /// <summary>
    /// 加载资源
    /// Load各种类型资源的内部函数
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    private void LoadAsset(string assetName, Action<Object> action)
    {
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
        {
            EditorLoadAsset(assetName, action);
        }
        else
#endif
        {
            StartCoroutine(LoadBundleAsync(assetName, action));
        }
    }


    public void LoadUI(string assetName, Action<Object> action = null)
    {
        LoadAsset(PathUtil.GetUIPath(assetName),action);
    }

    public void LoadMusic(string assetName, Action<Object> action = null)
    {
        LoadAsset(PathUtil.GetMusicPath(assetName), action);
    }

    public void LoadSound(string assetName, Action<Object> action = null)
    {
        LoadAsset(PathUtil.GetSoundPath(assetName), action);
    }

    public void LoadEffect(string assetName, Action<Object> action = null)
    {
        LoadAsset(PathUtil.GetEffectPath(assetName), action);
    }

    public void LoadScene(string assetName, Action<Object> action = null)
    {
        LoadAsset(PathUtil.GetScenePath(assetName), action);
    }

    public void LoadLua(string assetName, Action<Object> action = null)
    {
        LoadAsset(assetName, action);
    }

    public void LoadPrefab(string path, Action<Object> action = null)
    {
        LoadAsset(path, action);
    }


    /// <summary>
    /// 卸载资源
    /// </summary>
    /// <param name="name"></param>
    public void UnloadBundle(Object obj)
    {
        AssetBundle ab = obj as AssetBundle;
        ab.Unload(true);
    }
}
