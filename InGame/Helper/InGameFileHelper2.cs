using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class InGameFileHelper2 : MonoBehaviour
{
    static public string UpLoadAddress = "https://sms.analytic.treenod.com/api/stage/save";
    static public string DownLoadAddress = "https://sms.analytic.treenod.com/api/stage/load";
    static public string TaggingAddress = "https://sms.analytic.treenod.com/api/stage/tagging";
    static public string GetInfoAddress = "https://sms.analytic.treenod.com/api/stage/info";
    static public string ServiceCode { get
        {
            if (ToolSettings.Instance.toolType == ToolSettings.ToolType.TOOL_EXT)
            {
                return "pkv-ext";
            }
            else return "pkv-line";
        }
    }

    static public string DownLoadServiceCode
    {
        get
        {
            if (EditManager.instance != null && EditManager.instance.downLoadAdressType == DownLoadAdressType.Download_Ext)
                return "pkv-ext";
            else
                return ServiceCode;
        }
    }

    public static void SaveLocal<T>(string path, T data)
    {
        MemoryStream ms = new MemoryStream();
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, Encoding.UTF8);
        serializer.Serialize(xmlTextWriter, data);

        byte[] bytes = ms.ToArray();

        Save(path, bytes);
    }

    public static void Save(string path, byte[] bytes)
    {
        CreateDirectoryFromFullFilePath(path);
        File.WriteAllBytes(path, bytes);
    }

    public struct UploadResult
    {
        [JsonProperty("service")]
        public string service;
        [JsonProperty("status")]
        public string status;
        [JsonProperty("fileName")]
        public string fileName;
        [JsonProperty("version")]
        public int version;
        [JsonProperty("message")]
        public string message;
    }

    public static UploadResult uploadResult = new UploadResult();

    public struct ExtraImageInfo
    {
        public ExtraImageInfo(string fileName, byte[] image)
        {
            this.fileName = fileName;
            this.image = image;
        }
        
        public string fileName;
        public byte[] image;
    }
    
    public static IEnumerator WWW_Upload(string path, byte[] image = null, List<ExtraImageInfo> extraImages = null, string gimmickString = "", string targetString = "", string modeString = "", string descriptionString = "", string author = "", string extraDescription = "")
    {
        uploadResult = new UploadResult();
        FileInfo t = new FileInfo(Application.persistentDataPath + "/" + path);
        if (t.Exists)
        {
            UnityWebRequest localFile = new UnityWebRequest(Global.FileUri + Application.persistentDataPath + "/" + path);
            localFile.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            yield return localFile.SendWebRequest();

            if (localFile.error != null)
            {
                EditManager.instance.SetSaveLoadFailed(localFile.error);
                yield break;
            }

            WWWForm postForm = new WWWForm();
            postForm.AddField("service", ServiceCode);
            postForm.AddBinaryData("data", localFile.downloadHandler.data, path);

            if (image != null)
                postForm.AddBinaryData("image", image);
            if (gimmickString.Length > 0)
                postForm.AddField("gimmicks", gimmickString);
            if (targetString.Length > 0)
                postForm.AddField("goal", targetString);
            if (modeString.Length > 0)
                postForm.AddField("mode", modeString);
            if (descriptionString.Length > 0)
                postForm.AddField("description", descriptionString);
            if (extraDescription.Length > 0)
                postForm.AddField("extraDescription", extraDescription);
            if (author.Length > 0)
                postForm.AddField("author", author);
            
            if (extraImages != null && extraImages.Count > 0)
            {
                foreach (var info in extraImages)
                {
                    postForm.AddBinaryData("extraImages", info.image, info.fileName);
                }
            }

            UnityWebRequest upload = UnityWebRequest.Post(UpLoadAddress, postForm);
            upload.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            yield return upload.SendWebRequest();

            if (upload.error != null)
            {
                EditManager.instance.SetSaveLoadFailed(upload.error);
                yield break;
            }

            uploadResult = JsonConvert.DeserializeObject<UploadResult>(upload.downloadHandler.text);
        }
    }

    public static IEnumerator WWW_DownLoad<T>(string name, int version = -1)
    {
        Task<T> task = Async_WWW_DownLoad<T>(name, version);
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            EditManager.instance.SetSaveLoadFailed();
            throw task.Exception;
        }
        yield return task.Result;
    }

    public static async Task<T> Async_WWW_DownLoad<T>(string name, int version = -1)
    {
        byte[] data;
        using (WebClient webClient = new WebClient())
        {
            webClient.QueryString.Add("service", DownLoadServiceCode);
            webClient.QueryString.Add("fileName", name);
            if (version != -1)
                webClient.QueryString.Add("version", version.ToString());

            data = await webClient.DownloadDataTaskAsync(DownLoadAddress);
        }

        T result = default(T);
        try
        {
            MemoryStream stream = new MemoryStream(data);

            var serializer = new XmlSerializer(typeof(T));
            result = (T)serializer.Deserialize(stream);
        }
        catch (Exception ex)
        {
        }
        return result;
    }

    public static bool CreateDirectoryFromFullFilePath(string filePath)
    {
        string folder = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(folder))
            return true;

        DirectoryInfo directoryInfo = new DirectoryInfo(folder);
        if (directoryInfo.Exists)
            return true;

        directoryInfo = Directory.CreateDirectory(folder);
        return directoryInfo.Exists;
    }

    public static byte[] Load(string path)
    {
        try
        {
            return File.ReadAllBytes(path);
        }
        catch
        {
            return null;
        }
    }

    #region info 가져오기
    public struct InfoData
    {
        [JsonProperty("author")]
        public string author;
        [JsonProperty("tags")]
        public string[] tags;
    }

    public struct InfoResult
    {
        [JsonProperty("status")]
        public string status;

        [JsonProperty("message")]
        public string message;

        [JsonProperty("data")]
        public InfoData data;
    }

    public struct InfoParameterData
    {
        public string service;
        public string fileName;
    }

    public static InfoResult infoResult = new InfoResult();
    public static IEnumerator WWW_GetInfo(string name, int version = -1)
    {
        Task<string> task = Async_WWW_GetInfo(name, version);
        yield return new WaitUntil(() => task.IsCompleted == true);

        if (task.IsFaulted)
        {
            Debug.LogError("맵 정보 로딩 실패");
            throw task.Exception;
        }

        try
        {
            infoResult = new InfoResult();
            infoResult = JsonConvert.DeserializeObject<InfoResult>(task.Result);
        }
        catch (Exception ex)
        {
            Debug.Log("error : " + ex.Message);
        }
    }

    public static async Task<string> Async_WWW_GetInfo(string fileName, int version = -1)
    {
        string resultString;
        using (WebClient webClient = new WebClient())
        {
            webClient.QueryString.Add("service", DownLoadServiceCode);
            webClient.QueryString.Add("fileName", fileName);
            if (version != -1)
                webClient.QueryString.Add("version", version.ToString());
            
            using (Stream data = await webClient.OpenReadTaskAsync(GetInfoAddress))
            {
                using (StreamReader reader = new StreamReader(data))
                {
                    string s = reader.ReadToEnd();
                    resultString = s;
                    reader.Close();
                    data.Close();
                }
            }
        }
        return resultString;
    }
    #endregion

    #region 태그 저장
    public struct TaggingResult
    {
        [JsonProperty("status")]
        public string status;
    }

    public static TaggingResult taggingResult = new TaggingResult();

    public struct JsonTagData
    {
        public string service;
        public string fileName;
        public string action;
        public string[] tags;
        public string message;
    }

    public static IEnumerator WWW_Tagging(string fileName, string[] tags = null, string message = "")
    {
        taggingResult = new TaggingResult();

        JsonTagData jsonTagData = new JsonTagData();
        jsonTagData.service = ServiceCode;
        jsonTagData.fileName = fileName;
        jsonTagData.action = "set";
        jsonTagData.tags = tags;
        jsonTagData.message = message;

        string jsonString = JsonConvert.SerializeObject(jsonTagData);

        var request = new UnityWebRequest(TaggingAddress, "POST");
        request.SetRequestHeader("Content-Type", "application/json");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        taggingResult = JsonConvert.DeserializeObject<TaggingResult>(request.downloadHandler.text);
        Debug.Log("taggingResult.status : " + taggingResult.status);
    }
    #endregion
}
