using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class StreamingAssetsUtility : MonoBehaviour
{
    private static StreamingAssetsUtility instance = null;

    private static StreamingAssetsUtility Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                instance = gameObject.AddComponent<StreamingAssetsUtility>();
                gameObject.hideFlags = HideFlags.HideAndDontSave;
                gameObject.name = nameof(StreamingAssetsUtility);
            }

            return instance;
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public static Coroutine Load(string path, Action<byte[]> complete)
    {
        path = Path.Combine(Application.streamingAssetsPath, path);
        return Instance.StartCoroutine(LoadData(path, complete));
    }

    public static Coroutine Load(string path, Action<Texture2D> complete)
    {
        path = Path.Combine(Application.streamingAssetsPath, path);
        return Instance.StartCoroutine(LoadData(path, complete));
    }

    public static Coroutine Load(string path, Action<AudioClip> complete)
    {
        path = Path.Combine(Application.streamingAssetsPath, path);
        return Instance.StartCoroutine(LoadData(path, complete));
    }

    public static Coroutine Load(string path, Action<string> complete)
    {
        path = Path.Combine(Application.streamingAssetsPath, path);
        return Instance.StartCoroutine(LoadData(path, complete));
    }

    public static Coroutine Load(string path, Action<APNGInfo> complete)
    {
        path = Path.Combine(Application.streamingAssetsPath, path);
        return Instance.StartCoroutine(LoadData(path, complete));
    }

    private static IEnumerator LoadData(string path, Action<byte[]> complete)
    {
#if !UNITY_ANDROID
        path  = "file://" + path ;
#endif

        using (UnityWebRequest webRequest = UnityWebRequest.Get(path))
        {
            yield return webRequest.SendWebRequest();
            complete(webRequest.downloadHandler?.data);
        }
    }

    private static IEnumerator LoadData(string path, Action<Texture2D> complete)
    {
#if !UNITY_ANDROID
        path  = "file://" + path ;
#endif

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(path))
        {
            yield return webRequest.SendWebRequest();
            complete((webRequest.downloadHandler as DownloadHandlerTexture)?.texture);
        }
    }

    private static IEnumerator LoadData(string path, Action<AudioClip> complete)
    {
#if !UNITY_ANDROID
        path  = "file://" + path ;
#endif

        using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            yield return webRequest.SendWebRequest();
            complete((webRequest.downloadHandler as DownloadHandlerAudioClip)?.audioClip);
        }
    }

    private static IEnumerator LoadData(string path, Action<string> complete)
    {
        yield return LoadData(path, (byte[] loadData) => complete(DataConverter.DataToText(loadData)));
    }

    private static IEnumerator LoadData(string path, Action<APNGInfo> complete)
    {
        yield return LoadData(path, (byte[] loadData) => complete(DataConverter.DataToAPNG(loadData)));
    }
}
