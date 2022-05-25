using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUtil
{
    /// <summary>
    /// 需要打Bundle的目录
    /// </summary>
    public static readonly string BuildResourcesPath = Application.dataPath + "/BuildResources/";

    /// <summary>
    /// Bundle输出目录
    /// </summary>
    public static readonly string BundleOutPath = Application.streamingAssetsPath;

    /// <summary>
    /// 根目录
    /// </summary>
    public static readonly string AssetsPath = Application.dataPath;

    /// <summary>
    /// Bundle资源路径
    /// </summary>
    public static string BundleResourcePath
    {
        get { return Application.streamingAssetsPath; }
    }

    /// <summary>
    /// 获取Unity相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetUnityPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        return path.Substring(path.IndexOf("Assets"));
    }

    /// <summary>
    /// 获取标准路径
    /// \ 变为 /
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        return path.Trim().Replace("\\", "/");
    }
}
