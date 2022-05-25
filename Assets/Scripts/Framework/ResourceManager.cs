using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Object = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    internal class BundleInfo
    {
        public string AssetsName;
        public string BundleName;
        public List<string> Dependences;
    }
    /// <summary>
    /// 存放解析出来的Bundle信息的集合
    /// </summary>
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// 解析版本文件
    /// </summary>
    private void ParseVersonFile()
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
        }
    }

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
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;

        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        if (action != null && bundleRequest != null)
        {
            action.Invoke(bundleRequest.asset);
        }
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadAsset(string assetName, Action<Object> action)
    {
        StartCoroutine(LoadBundleAsync(assetName, action));
    }

    private void Start()
    {
        ParseVersonFile();
        //测试
        LoadAsset("Assets/BuildResources/UI/Prefabs/TestUI.prefab", OnComplete);
    }

    private void OnComplete(Object obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
