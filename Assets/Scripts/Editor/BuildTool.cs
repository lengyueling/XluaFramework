﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildTool : Editor
{
    [MenuItem("Tools/Build Windows Bundle")]
    static void BundleWindowsBuild()
    {
        Build(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Tools/Build Android Bundle")]
    static void BundleAndroidBuild()
    {
        Build(BuildTarget.Android);
    }

    [MenuItem("Tools/Build IOS Bundle")]
    static void BundleIOSBuild()
    {
        Build(BuildTarget.iOS);
    }

    static void Build(BuildTarget target)
    {
        //实例化AssetBundleBuild管理列表
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
        //获取所有文件
        string[] files = Directory.GetFiles(PathUtil.BuildResourcesPath, "*", SearchOption.AllDirectories);
        
        for (int i = 0; i < files.Length; i++)
        {
            //排除.meta文件
            if (files[i].EndsWith(".meta"))
            {
                continue;
            }
            //实例化每个要打包的对象
            AssetBundleBuild assetBundle = new AssetBundleBuild();

            string fileName = PathUtil.GetStandardPath(files[i]);
            Debug.Log("file:" + fileName);

            string assetName = PathUtil.GetUnityPath(fileName);
            //获取要打包的资源名字
            assetBundle.assetNames = new string[] { assetName };
            string bundleName = fileName.Replace(PathUtil.BuildResourcesPath, "").ToLower();
            //设置打包后资源的名字
            assetBundle.assetBundleName = bundleName + ".ab";
            assetBundleBuilds.Add(assetBundle);
        }
        //如果存在该文件夹则把他递归删除
        if (Directory.Exists(PathUtil.BundleOutPath))
        {
            Directory.Delete(PathUtil.BundleOutPath, true);
        }
        Directory.CreateDirectory(PathUtil.BundleOutPath);
        //打包Bundle 目录，压缩方式，目标平台
        BuildPipeline.BuildAssetBundles(PathUtil.BundleOutPath,assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, target);
    }
}
