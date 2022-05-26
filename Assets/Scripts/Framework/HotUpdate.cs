using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Object = UnityEngine.Object;

public class HotUpdate : MonoBehaviour
{
    byte[] m_ReadPathFileListData;
    byte[] m_ServerFileListData;

    /// <summary>
    /// 文件信息
    /// </summary>
    internal class DownFileInfo
    {
        public string url;
        public string fileName;
        public DownloadHandler fileData;
    }

    /// <summary>
    /// 下载单个文件
    /// </summary>
    /// <param name="info"></param>
    /// <param name="Complete"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        yield return webRequest.SendWebRequest();
        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            Debug.Log("下载文件出错" + info.url);
            yield break;
            //TODO retry
        }
        info.fileData = webRequest.downloadHandler;
        Complete?.Invoke(info);
        webRequest.Dispose();
    }

    /// <summary>
    /// 下载多个文件
    /// </summary>
    /// <param name="infos"></param>
    /// <param name="Complete"></param>
    /// <param name="DownLoadAllComplete"></param>
    /// <returns></returns>
    IEnumerator DownLoadFile(List<DownFileInfo> infos, Action<DownFileInfo> Complete,Action DownLoadAllComplete)
    {
        foreach (var info in infos)
        {
            yield return DownLoadFile(info, Complete);
        }
        DownLoadAllComplete?.Invoke();
    }

    /// <summary>
    /// 获取filelist文件信息
    /// </summary>
    /// <param name="fileData"></param>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileData,string path)
    {
        string content = fileData.Trim().Replace("\r", "");
        string[] files = content.Split('|');
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            string[] info = files[i].Split('|');
            DownFileInfo fileInfo = new DownFileInfo();
            fileInfo.fileName = info[i];
            fileInfo.url = Path.Combine(path, info[1]);
            downFileInfos.Add(fileInfo);
        }
        return downFileInfos;
    }

    private void Start()
    {
        if (IsFirstInstall())
        {
            ReleaseResources();
        }
        else
        {
            CheckUpdate();
        }
    }

    /// <summary>
    /// 是否初次安装
    /// </summary>
    /// <returns></returns>
    private bool IsFirstInstall()
    {
        //判断只读目录是否存在版本文件
        bool isExistReadPath = FileUtil.isExists(Path.Combine(PathUtil.ReadPath, AppConst.FileListName));
        //判断可读写目录是否存在版本文件
        bool isExistsReadWritePath = FileUtil.isExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));
        return isExistReadPath && !isExistsReadWritePath;
    }

    /// <summary>
    /// 释放资源
    /// 下载只读文件夹的filelist
    /// </summary>
    private void ReleaseResources()
    {
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownLoadFile(info, OnDownLoadReadPathFileListComplete));
    }

    /// <summary>
    /// 解析文件信息
    /// 下载热更新资源
    /// </summary>
    /// <param name="file"></param>
    private void OnDownLoadReadPathFileListComplete(DownFileInfo file)
    {
        m_ReadPathFileListData = file.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        StartCoroutine(DownLoadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));
    }

    /// <summary>
    /// 下载所有热更新资源后
    /// 在可读写目录写入filelist
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdate();
    }

    /// <summary>
    /// 每下载一个热更新资源
    /// 写入对应的热更新资源
    /// </summary>
    /// <param name="obj"></param>
    private void OnReleaseFileComplete(DownFileInfo fileInfo)
    {
        Debug.Log("OnReleaseFileComplete:" + fileInfo.url);
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    private void CheckUpdate()
    {
        string url = Path.Combine(AppConst.ResourceUrl, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownLoadFile(info, OnDownLoadServerFileListComplete));
    }

    /// <summary>
    /// 对比现有本地目录和服务器目录的不同
    /// 下载未下载的资源
    /// </summary>
    /// <param name="file"></param>
    private void OnDownLoadServerFileListComplete(DownFileInfo file)
    {
        m_ServerFileListData = file.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResourceUrl);
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();
        for (int i = 0; i < fileInfos.Count; i++)
        {
            string loalFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            if (!FileUtil.isExists(loalFile))
            {
                fileInfos[i].url = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
                downListFiles.Add(fileInfos[i]);
            }
        }
        //还有服务器资源没有被下载，下载资源
        if (downListFiles.Count > 0)
        {
            StartCoroutine(DownLoadFile(fileInfos, OnUpdateFileComplete, OnUpdateAllFileComplete));
        }
        else
        {
            EnterGame();
        }
    }

    /// <summary>
    /// 更新所有热更新资源后
    /// 写入新的filelist
    /// </summary>
    private void OnUpdateAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ServerFileListData);
        EnterGame();
    }

    /// <summary>
    /// 每更新一个热更新资源
    /// 写入对应的热更新资源
    /// </summary>
    /// <param name="file"></param>
    private void OnUpdateFileComplete(DownFileInfo file)
    {
        Debug.Log("OnReleaseFileComplete:" + file.url);
        string writeFile = Path.Combine(PathUtil.ReadWritePath, file.fileName);
        FileUtil.WriteFile(writeFile, file.fileData.data);
    }

    private void EnterGame()
    {
        //测试
        Manager.Resource.ParseVersonFile();
        Manager.Resource.LoadUI("TestUI", OnComplete);
    }

    private void OnComplete(Object obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
