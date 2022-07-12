using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    /// <summary>
    /// SceneLogic所在游戏物体的名字
    /// </summary>
    private string m_LogicName = "[SceneLogic]";

    private void Awake()
    {
        //切换场景时调用的事件
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    /// <summary>
    /// 场景切换时的回调
    /// </summary>
    /// <param name="s1">被切换的场景</param>
    /// <param name="s2">切换后的场景</param>
    private void OnActiveSceneChanged(Scene s1, Scene s2)
    {
        if (!s1.isLoaded || !s2.isLoaded)
            return;
        //获取两个场景的SceneLogic
        SceneLogic logic1 = GetSceneLogic(s1);
        SceneLogic logic2 = GetSceneLogic(s2);
        //执行相应委托，被切换的场景执行InActive()，切换后的场景执行OnActive
        logic1?.OnInActive();
        logic2?.OnActive();
    }
    /// <summary>
    /// 激活场景
    /// 场景叠加多个场景的时候需要设置一个被激活的场景 
    /// </summary>
    /// <param name="sceneName"></param>
    public void SetActive(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
    }

    /// <summary>
    /// 叠加加载场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName">使用的lua脚本</param>
    public void LoadScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Additive));
        });
    }

    /// <summary>
    /// 切换场景
    /// 是异步操作
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void ChangeScene(string sceneName, string luaName)
    {
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Single));
        });
    }
    /// <summary>
    /// 卸载场景
    /// 叠加加载的需要手动去卸载
    /// 是异步操作
    /// </summary>
    /// <param name="sceneName"></param>
    public void UnLoadSceneAsync(string sceneName)
    {
        StartCoroutine(UnLoadScene(sceneName));
    }

    /// <summary>
    /// 判断是否已经加载了对应的场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private bool IsLoadedScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }

    /// <summary>
    /// 开始加载场景协程
    /// </summary>
    /// <param name="sceneName">场景名</param>
    /// <param name="luaName">Lua使用的文件</param>
    /// <param name="mode">加载模式</param>
    /// <returns></returns>
    IEnumerator StartLoadScene(string sceneName, string luaName, LoadSceneMode mode)
    {
        //如果已经加载了场景则直接直接终止方法，退出协程
        if (IsLoadedScene(sceneName))
            yield break;

        //异步加载场景
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode);
        //决定加载完场景是否需要跳转，如果选否则只会加载到90%
        async.allowSceneActivation = true;
        yield return async;

        //获取加载的场景对象
        Scene scene = SceneManager.GetSceneByName(sceneName);
        //实例化一个将来挂载场景逻辑的物体
        GameObject go = new GameObject(m_LogicName);
        //将刚刚实例化的对象移动到加载的场景中
        SceneManager.MoveGameObjectToScene(go, scene);
        //挂载场景逻辑脚本
        SceneLogic logic = go.AddComponent<SceneLogic>();
        logic.SceneName = sceneName;
        //执行初始化逻辑
        logic.Init(luaName);
        logic.OnEnter();
    }

    /// <summary>
    /// 开始卸载场景协程
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private IEnumerator UnLoadScene(string sceneName)
    {
        //判断场景是否已经被加载，如果场景没有被加载，那就无法被卸载
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            Debug.LogError("场景还没有被加载，无法被卸载");
            yield break;
        }

        SceneLogic logic = GetSceneLogic(scene);
        logic?.OnQuit();
        //异步卸载场景
        AsyncOperation async = SceneManager.UnloadSceneAsync(scene);
        yield return async;
    }

    /// <summary>
    /// 获取游戏场景中的SceneLogic组件
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private SceneLogic GetSceneLogic(Scene scene)
    {
        GameObject[] gameObjects = scene.GetRootGameObjects();
        //遍历所有的根游戏对象
        foreach (GameObject go in gameObjects)
        {
            //若Str1等于参数字符串Str2字符串，则返回0；
            //若该Str1按字典顺序小于参数字符串Str2，则返回值小于0；
            //若Str1按字典顺序大于参数字符串Str2，则返回值大于0。
            if (go.name.CompareTo(m_LogicName) == 0)
            {
                SceneLogic logic = go.GetComponent<SceneLogic>();
                return logic;
            }
        }
        return null;
    }
}
