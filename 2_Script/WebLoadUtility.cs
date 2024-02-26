using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebLoadUtility : MonoBehaviour
{
    private static WebLoadUtility instance = null;

    private static WebLoadUtility Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                instance = gameObject.AddComponent<WebLoadUtility>();
                gameObject.hideFlags = HideFlags.HideAndDontSave;
                gameObject.name = nameof(WebLoadUtility);
            }

            return instance;
        }
    }

    public static Coroutine Load(string url, Action<byte[]> complete)
    {
        return Instance.StartCoroutine(Instance.LoadData(url, complete));
    }

    public static Coroutine Load(string url, Action<Texture2D> complete)
    {
        return Instance.StartCoroutine(Instance.LoadData(url, complete));
    }

    public static Coroutine Load(string url, AudioType audioType, Action<AudioClip> complete)
    {
        return Instance.StartCoroutine(Instance.LoadData(url, audioType, complete));
    }

    private IEnumerator LoadData(string url, Action<byte[]> complete)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            NetworkSettings.PreprocessCert(webRequest);

            yield return webRequest.SendWebRequest();

            if (webRequest.IsError())
            {
                complete(null);
            }
            else
            {
                complete(webRequest.downloadHandler.data);
            }
        }
    }

    private IEnumerator LoadData(string url, Action<Texture2D> complete)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            NetworkSettings.PreprocessCert(webRequest);

            yield return webRequest.SendWebRequest();

            if (webRequest.IsError())
            {
                complete(null);
            }
            else
            {
                DownloadHandlerTexture downloadHandler = webRequest.downloadHandler as DownloadHandlerTexture;
                complete(downloadHandler.texture);
            }
        }
    }

    private IEnumerator LoadData(string url, AudioType audioType, Action<AudioClip> complete)
    {
        using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            NetworkSettings.PreprocessCert(webRequest);

            yield return webRequest.SendWebRequest();

            if (webRequest.IsError())
            {
                complete(null);
            }
            else
            {
                DownloadHandlerAudioClip downloadHandler = webRequest.downloadHandler as DownloadHandlerAudioClip;
                complete(downloadHandler.audioClip);
            }
        }
    }
}
