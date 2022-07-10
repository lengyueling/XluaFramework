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
    /// 存放解析出来的Bundle信息的集合
    /// </summary>
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();

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
        List<string> dependence = m_BundleInfos[assetName].Dependences;
        //加载依赖资源
        if (dependence != null && dependence.Count > 0)
        {
            for (int i = 0; i < dependence.Count; i++)
            {
                yield return LoadBundleAsync(dependence[i]);
            }
        }
        //加载资源路径
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;

        //加载资源
        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        //调用回调函数，告诉应用层已经完成加载
        //action?.Invoke(bundleRequest?.asset);与下面语句效果相同
        if (action != null && bundleRequest != null)
        {
            action.Invoke(bundleRequest.asset);
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

    //TODO: 卸载
}
