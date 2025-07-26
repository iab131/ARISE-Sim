#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLFileDownloader
{
    [DllImport("__Internal")]
    private static extern void DownloadFile(string fileName, string content);

    public static void DownloadJson(string fileName, string json)
    {
        DownloadFile(fileName, json);
    }
}
#endif
