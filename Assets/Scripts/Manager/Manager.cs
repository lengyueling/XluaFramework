using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
