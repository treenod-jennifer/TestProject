using System.IO;
using System.Collections;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

public class CDNLoadUtility : MonoBehaviour
{
    private static CDNLoadUtility instance = null;
    private static CDNLoadUtility Instance 
    {
        get 
        {
            if (instance == null)
            {
                GameObject obj = new GameObject();
                instance = obj.AddComponent<CDNLoadUtility>();
                obj.name = nameof(CDNLoadUtility);
                obj.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }


    public static Coroutine Load(string cdnFolderName, string fileName, System.Action<byte[]> completeEvent)
    {
        return Load(cdnFolderName, cdnFolderName, fileName, completeEvent);
    }

    public static Coroutine Load(string localFolderName, string cdnFolderName, string fileName, System.Action<byte[]> completeEvent)
    {
        string localizingFileName = LanguageUtility.FileNameConversion(fileName);

        // CDN에 국가코드가 붙은 파일이 있는 경우 
        if (HashChecker.IsExist(cdnFolderName, localizingFileName))
        {
            fileName = localizingFileName;
        }

        string loadPath = $"{Application.persistentDataPath}/{localFolderName}/{fileName}";
        FileInfo fileInfo = new FileInfo(loadPath);

        if (!fileInfo.Exists || HashChecker.IsHashChanged(cdnFolderName, fileName, fileInfo))
        {
            return Instance.StartCoroutine(LoadFromCDN(localFolderName, cdnFolderName, fileName, completeEvent));
        }
        else
        {
            return Instance.StartCoroutine(LoadFromFile(localFolderName, fileName, completeEvent));
        }
    }


    public static Coroutine Load(string cdnFolderName, string fileName, System.Action<Texture2D> completeEvent = null)
    {
        return Load(cdnFolderName, cdnFolderName, fileName, completeEvent);
    }

    public static Coroutine Load(string localFolderName, string cdnFolderName, string fileName, System.Action<Texture2D> completeEvent = null)
    {
        string localizingFileName = LanguageUtility.FileNameConversion(fileName);

        // CDN에 국가코드가 붙은 파일이 있는 경우 
        if (HashChecker.IsExist(cdnFolderName, localizingFileName))
        {
            fileName = localizingFileName;
        }

        string localFileName = GetLocalTextureFileName(cdnFolderName, fileName);

        string loadPath = $"{Application.persistentDataPath}/{localFolderName}/{localFileName}";

        FileInfo fileInfo = new FileInfo(loadPath);
        if (!fileInfo.Exists || HashChecker.IsHashChanged(cdnFolderName, fileName, fileInfo))
        {
            return Instance.StartCoroutine(LoadFromCDN(localFolderName, cdnFolderName, fileName, completeEvent));
        }
        else
        {
            return Instance.StartCoroutine(LoadFromFile(localFolderName, localFileName, completeEvent));
        }
    }


    public static Coroutine Load(string cdnFolderName, string fileName, System.Action<AudioClip> completeEvent)
    {
        return Load(cdnFolderName, cdnFolderName, fileName, completeEvent);
    }

    public static Coroutine Load(string localFolderName, string cdnFolderName, string fileName, System.Action<AudioClip> completeEvent)
    {
        return Load(localFolderName, cdnFolderName, fileName, (byte[] primitiveData) =>
        {
            if (primitiveData != null)
            {
                completeEvent?.Invoke(DataConverter.DataToAudioClip(primitiveData, DataConverter.DataOption.Decrypt));
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        });
    }

    
    public static Coroutine Load(string cdnFolderName, string fileName, System.Action<string> completeEvent)
    {
        return Load(cdnFolderName, cdnFolderName, fileName, completeEvent);
    }

    public static Coroutine Load(string localFolderName, string cdnFolderName, string fileName, System.Action<string> completeEvent)
    {
        return Load(localFolderName, cdnFolderName, fileName, (byte[] primitiveData) =>
        {
            if (primitiveData != null)
            {
                completeEvent?.Invoke(DataConverter.DataToText(primitiveData, DataConverter.DataOption.Decrypt));
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        });
    }


    public static Coroutine Load(string cdnFolderName, string fileName, System.Action<APNGInfo> completeEvent)
    {
        return Load(cdnFolderName, cdnFolderName, fileName, completeEvent);
    }

    public static Coroutine Load(string localFolderName, string cdnFolderName, string fileName, System.Action<APNGInfo> completeEvent)
    {
        return Load(localFolderName, cdnFolderName, fileName, (byte[] primitiveData) =>
        {
            if(primitiveData != null)
            {
                completeEvent?.Invoke(DataConverter.DataToAPNG(primitiveData));
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        });
    }


    private static IEnumerator LoadFromCDN(string localFolderName, string cdnFolderName, string fileName, System.Action<byte[]> completeEvent)
    {
        string cdnPath = $"{Global._cdnAddress}{cdnFolderName}/{fileName}";

        using (UnityWebRequest www = UnityWebRequest.Get(cdnPath))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null && www.downloadHandler.data != null)
            {
                SaveFile(Path.Combine(localFolderName, fileName), www.downloadHandler.data);
                completeEvent?.Invoke(www.downloadHandler.data);
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        }
    }

    private static IEnumerator LoadFromFile(string localFolderName, string fileName, System.Action<byte[]> completeEvent)
    {
        string loadPath = $"{Global.FileUri}{Application.persistentDataPath}/{localFolderName}/{fileName}";

        using (UnityWebRequest www = UnityWebRequest.Get(loadPath))
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null && www.downloadHandler.data != null)
            {
                completeEvent?.Invoke(www.downloadHandler.data);
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        }
    }

    private static IEnumerator LoadFromCDN(string localFolderName, string cdnFolderName, string fileName, System.Action<Texture2D> completeEvent = null)
    {
        string cdnPath = $"{Global._cdnAddress}{cdnFolderName}/{fileName}";

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(cdnPath))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null && www.downloadHandler.data != null)
            {
                DownloadHandlerTexture downloadHandlerTexture = www.downloadHandler as DownloadHandlerTexture;
                SaveFile(Path.Combine(localFolderName, GetLocalTextureFileName(cdnFolderName, fileName)), downloadHandlerTexture.data);
                completeEvent?.Invoke(downloadHandlerTexture.texture);
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        }
    }

    private static IEnumerator LoadFromFile(string localFolderName, string fileName, System.Action<Texture2D> completeEvent = null)
    {
        string loadPath = $"{Global.FileUri}{Application.persistentDataPath}/{localFolderName}/{fileName}";

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(loadPath))
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null && www.downloadHandler.data != null)
            {
                DownloadHandlerTexture downloadHandlerTexture = www.downloadHandler as DownloadHandlerTexture;
                completeEvent?.Invoke(downloadHandlerTexture.texture);
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        }
    }

    private static IEnumerator LoadFromCDN(string localFolderName, string cdnFolderName, string fileName, AudioType audioType, System.Action<AudioClip> completeEvent = null)
    {
        string cdnPath = $"{Global._cdnAddress}{cdnFolderName}/{fileName}";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(cdnPath, audioType))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null && www.downloadHandler.data != null)
            {
                DownloadHandlerAudioClip downloadHandlerAudio = www.downloadHandler as DownloadHandlerAudioClip;
                SaveFile(Path.Combine(localFolderName, fileName), www.downloadHandler.data);
                completeEvent?.Invoke(downloadHandlerAudio.audioClip);
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        }
    }

    private static IEnumerator LoadFromFile(string localFolderName, string fileName, AudioType audioType, System.Action<AudioClip> completeEvent = null)
    {
        string loadPath = $"{Global.FileUri}{Application.persistentDataPath}/{localFolderName}/{fileName}";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(loadPath, audioType))
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null && www.downloadHandler.data != null)
            {
                DownloadHandlerAudioClip downloadHandlerAudio = www.downloadHandler as DownloadHandlerAudioClip;
                completeEvent?.Invoke(downloadHandlerAudio.audioClip);
            }
            else
            {
                completeEvent?.Invoke(null);
            }
        }
    }


    public static (string localFolderName, string cdnFolderName, string fileName) LegacyPathConvert(string localFolderName, string cdnFolderName, string fileName)
    {
        localFolderName = localFolderName.Replace(Application.persistentDataPath, "");
        localFolderName = RemoveSlash(localFolderName);
        cdnFolderName = RemoveSlash(cdnFolderName);

        if (!fileName.Contains("."))
        {
            fileName += ".png";
        }

        return (localFolderName, cdnFolderName, fileName);

        string RemoveSlash(string text)
        {
            if (text.StartsWith("/"))
            {
                text = text.Remove(0, 1);
            }

            if (text.EndsWith("/"))
            {
                text = text.Remove(text.Length - 1, 1);
            }

            return text;
        }
    }

    private static string GetLocalTextureFileName(string cdnFolderName, string fileName)
    {
        string key = $"{cdnFolderName}/{fileName.Replace(".png", "")}";
        return $"{Global.GetHashfromText(key)}.png";
    }

    private static void SaveFile(string path, byte[] bytes)
    {
        path = Path.Combine(Application.persistentDataPath, path);

        if (bytes == null)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream file = File.Create(path))
            {
                file.Write(bytes, 0, bytes.Length);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}

public static class DataConverter
{
    public enum DataOption
    {
        Default = 0b_0000,
        
        /// <summary>
        /// 암호화 된 데이터인 경우 복호화 시켜 줍니다.
        /// </summary>
        Decrypt = 0b_0001

        //0b_0010
        //0b_0100
        //0b_1000
    }

    private static byte[] ApplyOption(byte[] primitiveData, DataOption option)
    {
        //암호화 옵션
        if (Contains(option, DataOption.Decrypt))
        {
            primitiveData = Decrypt(primitiveData);
        }

        //if (Contains(option, DataOption.))
        //{
        //}

        //if (Contains(option, DataOption.))
        //{
        //}

        //if (Contains(option, DataOption.))
        //{
        //}

        return primitiveData;

        bool Contains(DataOption targetOption, DataOption checkOption)
        {
            return ((int)targetOption & (int)checkOption) == (int)checkOption;
        }
    }

    private const string PASSWORD = "iGki2W12fM93h8UA";

    private static byte[] Decrypt(byte[] encryptedData)
    {
        using (MemoryStream memoryStream = new MemoryStream(encryptedData))
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            {
                SharpAESCrypt.SharpAESCrypt.Decrypt(PASSWORD, memoryStream, outMemoryStream);
                return outMemoryStream.ToArray();
            }
        }        
    }


    public static AudioClip DataToAudioClip(byte[] primitiveData, DataOption option = DataOption.Default)
    {
        primitiveData = ApplyOption(primitiveData, option);

        AudioClip audio = WavUtility.ToAudioClip(primitiveData);

        return audio;
    }
    
    public static string DataToText(byte[] primitiveData, DataOption option = DataOption.Default)
    {
        primitiveData = ApplyOption(primitiveData, option);

        string text = Encoding.Default.GetString(primitiveData);

        return text;
    }

    public static Texture2D DataToTexture2D(byte[] primitiveData, DataOption option = DataOption.Default)
    {
        primitiveData = ApplyOption(primitiveData, option);

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        texture.wrapMode = TextureWrapMode.Clamp;

        texture.LoadImage(primitiveData);

        return texture;
    }

    public static APNGInfo DataToAPNG(byte[] primitiveData, DataOption option = DataOption.Default)
    {
        primitiveData = ApplyOption(primitiveData, option);

        LibAPNG.APNG apng = new LibAPNG.APNG(primitiveData);

        if (apng.IsSimplePNG)
        {
            var newFrame = new FrameInfo()
            {
                tex = DataToTexture2D(primitiveData),
                interval = 0f
            };
            FrameInfo[] singleFrame = new FrameInfo[1];
            singleFrame[0] = newFrame;

            return new APNGInfo(singleFrame);
        }
        else
        {
            return TextureUtility.ParseAPNGFrames(apng);
        }
    }
}
