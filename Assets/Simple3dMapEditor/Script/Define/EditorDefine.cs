using System;
using UnityEngine;

public class EditorDefine
{
    public const int VERSION_CODE = 1;
    public const string VERSION_STR   = "1.";
    
    public static string GetVersionStr()
    {
        return string.Format("{0}{1}", VERSION_STR, VERSION_CODE);
    }
    public static int GetVersionCode()
    {
        return VERSION_CODE;
    }
    public static string GetVersionCode(string versionStr)
    {
        return versionStr.Replace(VERSION_STR, "");
    }
}