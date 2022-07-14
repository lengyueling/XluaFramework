using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Log
{
    public static void Info(string msg)
    {
        if (!AppConst.OpenLog)
        {
            return;
        }
        Debug.Log(msg);
    }

    public static void Warning(string msg)
    {
        if (!AppConst.OpenLog)
        {
            return;
        }
        Debug.LogWarning(msg);
    }

    public static void Error(string msg)
    {
        if (!AppConst.OpenLog)
        {
            return;
        } 
        Debug.LogError(msg);
    }
}
