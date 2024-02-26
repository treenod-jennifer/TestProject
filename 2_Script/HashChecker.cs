using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

public class HashChecker : MonoBehaviour {

    // 현재 타임아웃 0으로 무한 대기라 기본 값 설정
    private const int DefaultTimeout = 180;
    private const int DefaultRetry = 10;

    private string fileHash = "";
    public static string ShortHash
    {
        get
        {
            if (instance == null)
                return "404X";

            return instance.fileHash;
        }
    }

    static public HashChecker instance = null;
    Dictionary<string, Dictionary<string, string>> dicHashData = new Dictionary<string, Dictionary<string, string>>();

    public enum ResultCode
    {
        OK,
        FILELIST_DOWNLOAD_FAILED = -1,
        ASSET_NOT_EXIST = -2,
        ASSET_DOWNLOAD_FAILED = -3,
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static IEnumerator Init()
    {
        if( instance != null)
            yield return instance.Load();
        yield break;
    }

    public static void OnReboot()
    {
        Clear();
    }

    public static void Clear()
    {
        if (instance != null)
            instance.dicHashData.Clear();
    }


    bool IsError(UnityWebRequest req)
    {
        return req.isNetworkError;
    }

    public IEnumerator Load(System.Action<ResultCode> failCallback = null)
    {
        if (dicHashData.Count == 0)
        {
            string filePath = FileHelper.MergePath(NetworkSettings.Instance.GetCDN_URL(), "fileList2.json");
            for (int i = 0; i < DefaultRetry; i++)
            {
                UnityWebRequest request = UnityWebRequest.Get(filePath);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
                request.SetRequestHeader("Pragma", "no-cache");
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                request.timeout = DefaultTimeout;
                yield return request.SendWebRequest();

                if (IsError(request))
                {
                    Debug.LogError("request error and retry: " + request.error);
                    request.Dispose();

                    if (failCallback != null)
                        failCallback(ResultCode.FILELIST_DOWNLOAD_FAILED);
                    yield break;
                }
                else
                {
                    MemoryStream stream = new MemoryStream(request.downloadHandler.data);
                    StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);

                    this.fileHash = checkMD5_b64(request.downloadHandler.data);
                    if( fileHash.Length > 4)
                        fileHash = fileHash.Substring(0, 4);

                    string readToEnd = reader.ReadToEnd();
                    reader.Close();
                    stream.Close();

                    dicHashData = JsonFx.Json.JsonReader.Deserialize<Dictionary<string, Dictionary<string, string>>>(readToEnd);
                    break;
                }

                yield return new WaitForSeconds(0.5f * i);
            }
            if (dicHashData.Count == 0)
            {
                if (failCallback != null)
                    failCallback(ResultCode.FILELIST_DOWNLOAD_FAILED);
                yield break;
            }
        }
        yield return null;
    }

    public static bool IsHashChanged(string folderName, string fileName, FileInfo fileInfo)
    {
        if (instance == null)
            return true;

        string folder = folderName.Replace(Path.DirectorySeparatorChar, '/');
        folder = folderName.Replace('\\', '/');
        folder = folderName.Replace("//", "/");
        folder = folder.TrimEnd('/');

        if (instance.dicHashData.Count > 0)
        {
            Dictionary<string, string> dataList = null;
            if (instance.dicHashData.TryGetValue(folder, out dataList))
            {
                if( dataList.ContainsKey(fileName) )
                {
                    var checksum = checkMD5_b64(fileInfo).Substring(0, 20);
                    if (dataList[fileName].Equals(checksum))
                        return false;
                }
            }
        }
        return true;
    }

    public static bool IsHashChanged(string folderName, string fileName, byte[] byteArray)
    {
        if (instance == null)
            return true;

        string folder = folderName.Replace(Path.DirectorySeparatorChar, '/');
        folder = folderName.Replace('\\', '/');
        folder = folderName.Replace("//", "/");
        folder = folder.TrimEnd('/');

        if (instance.dicHashData.Count > 0)
        {
            Dictionary<string, string> dataList = null;
            if (instance.dicHashData.TryGetValue(folder, out dataList))
            {
                if (dataList.ContainsKey(fileName))
                {
                    var checksum = checkMD5_b64(byteArray).Substring(0, 20);
                    if (dataList[fileName].Equals(checksum))
                        return false;
                }
            }
        }
        return true;
    }


    private static string checkMD5_b64(FileInfo fileInfo)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = fileInfo.OpenRead())
            {
                return System.Convert.ToBase64String(md5.ComputeHash(stream));
            }
        }
    }

    private static string checkMD5_b64(byte[] byteArray)
    {
        if (byteArray == null || byteArray.Length == 0)
            return "";

        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            return System.Convert.ToBase64String(md5.ComputeHash(byteArray));
        }
    }

    public static string GetHash(string folderName, string fileName)
    {
        if (instance == null)
            return "";

        string folder = folderName.Replace(Path.DirectorySeparatorChar, '/');
        folder = folderName.Replace('\\', '/');
        folder = folderName.Replace("//", "/");
        folder = folder.TrimEnd('/');

        if (instance.dicHashData.Count > 0)
        {
            Dictionary<string, string> dataList = null;
            if (instance.dicHashData.TryGetValue(folder, out dataList))
            {
                if (dataList.ContainsKey(fileName))
                {
                    return dataList[fileName];
                }
            }
        }
        return "";
    }

    public static string GetHash(FileInfo fileInfo)
    {
        if (fileInfo.Exists == false)
            return "";

        return checkMD5_b64(fileInfo).Substring(0, 20);
    }

    public static bool IsExist(string folderName, string fileName)
    {
        if (instance == null)
            return false;

        string folder = folderName.Replace(Path.DirectorySeparatorChar, '/');
        folder = folderName.Replace('\\', '/');
        folder = folderName.Replace("//", "/");
        folder = folder.TrimEnd('/');

        if (instance.dicHashData.Count > 0)
        {
            Dictionary<string, string> dataList = null;
            if (instance.dicHashData.TryGetValue(folder, out dataList))
            {
                if (dataList.ContainsKey(fileName))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
