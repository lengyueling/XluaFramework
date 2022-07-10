using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 监听者模式
/// 作为所有manager的入口
/// </summary>
public class Manager : MonoBehaviour
{
    private static ResourceManager _resource;
    public static ResourceManager Resource
    {
        get { return _resource; }
    }

    private void Awake()
    {
        _resource = this.gameObject.AddComponent<ResourceManager>();
    }
}
