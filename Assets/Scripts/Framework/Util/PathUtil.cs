using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUtil
{
    /// <summary>
    /// 根目录
    /// </summary>
    public static readonly string AssetsPath = Application.dataPath;

    /// <summary>
    /// 需要打Bundle的目录
    /// </summary>
    public static readonly string BuildResourcesPath = AssetsPath + "/BuildResources/";;

    /// <summary>
    /// Bundle输出目录
    /// </summary>
    public static readonly string BundleOutPath = Application.streamingAssetsPath;

    /// <summary>
    /// 只读目录
    /// </summary>
    public static readonly string ReadPath = Application.streamingAssetsPath;

    /// <summary>
    /// 可读写目录
    /// </summary>
    public static readonly string ReadWritePath = Application.persistentDataPath;

    /// <summary>
    /// Bundle资源路径
    /// </summary>
    public static string BundleResourcePath
    {
        get
        {
            if (AppConst.GameMode == GameMode.UpdateMode)
            {
                //可读写的目录
                return ReadWritePath;
            }
            //只读目录
            return ReadPath;
        }
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

    public static string GetLuaPath(string name)
    {
        return string.Format("Assets/BuildResources/LuaScripts/{0}.bytes", name);
    }

    public static string GetUIPath(string name)
    {
        return string.Format("Assets/BuildResources/UI/Prefabs/{0}.prefab", name);
    }

    public static string GetMusicPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Music/{0}", name);
    }

    public static string GetSoundPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Sound/{0}", name);
    }

    public static string GetEffectPath(string name)
    {
        return string.Format("Assets/BuildResources/Effect/Prefabs/{0}.prefab", name);
    }

    public static string GetSpritePath(string name)
    {
        return string.Format("Assets/BuildResources/Sprites/{0}", name);
    }

    public static string GetScenePath(string name)
    {
        return string.Format("Assets/BuildResources/Scenes/{0}.unity", name);
    }
}
