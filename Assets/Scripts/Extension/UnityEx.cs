using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

/// <summary>
/// 扩展方法类
/// </summary>
[LuaCallCSharp]
public static class UnityEx
{
    /// <summary>
    /// 对按钮进行事件监听的扩展
    /// </summary>
    /// <param name="button">要扩展的组件</param>
    /// <param name="callback">lua中执行的函数</param>
    public static void OnClickSet(this Button button, object callback)
    {
        LuaFunction func = callback as LuaFunction;
        //每次执行要移除所有的监听事件，不然可能造成多次执行该函数会执行多次监听
        button.onClick.RemoveAllListeners();
        //增加按钮点击监听事件
        button.onClick.AddListener(
            () =>
            {
                //执行lua中的函数
                func?.Call();
            });
    }

    /// <summary>
    /// 对滑动条滑动事件进行监听的扩展
    /// </summary>
    /// <param name="slider">要扩展的组件</param>
    /// <param name="callback">lua中执行的函数</param>
    public static void OnValueChangedSet(this Slider slider, object callback)
    {
        LuaFunction func = callback as LuaFunction;
        slider.onValueChanged.RemoveAllListeners();
        //增加滑动条滑动监听事件
        slider.onValueChanged.AddListener(
            (float value) =>
            {
                func?.Call(value);
            });
    }
}
