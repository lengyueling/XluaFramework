using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 监听者模式
/// 作为所有manager的入口
/// 管理所有的manager
/// </summary>
public class Manager : MonoBehaviour
{
    private static ResourceManager _resource;
    public static ResourceManager Resource
    {
        get { return _resource; }
    }
    private static LuaManager _lua;
    public static LuaManager Lua
    {
        get { return _lua; }
    }

    private static UIManager _ui;
    public static UIManager UI
    {
        get { return _ui; }
    }

    private static EntityManager _entity;
    public static EntityManager Entity
    {
        get { return _entity; }
    }

    private static MySceneManager _scene;
    public static MySceneManager Scene
    {
        get { return _scene; }
    }

    private static SoundManager _sound;
    public static SoundManager Sound
    {
        get { return _sound; }
    }

    private static EventManager _event;
    public static EventManager Event
    {
        get { return _event; }
    }

    private void Awake()
    {
        _resource = this.gameObject.AddComponent<ResourceManager>();
        _lua = this.gameObject.AddComponent<LuaManager>();
        _ui = this.gameObject.AddComponent<UIManager>();
        _entity = this.gameObject.AddComponent<EntityManager>();
        _scene = this.gameObject.AddComponent<MySceneManager>();
        _sound = this.gameObject.AddComponent<SoundManager>();
        _event = this.gameObject.AddComponent<EventManager>();

    }
}
