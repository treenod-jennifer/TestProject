using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;
using Newtonsoft.Json;

public class InGameFileHelper : MonoBehaviour
{
    static public string _UpLoadAddress = "https://server-tool.lgpkv.treenod.com:3300/upload/stageTool";
    static public string _DownLoadAddress = "https://server-tool.lgpkv.treenod.com:3300/download/stageTool/";

    static public string UpLoadAddress
    {
        get {

            string ret = _UpLoadAddress;
            if( ToolSettings.Instance.toolType == ToolSettings.ToolType.TOOL_EXT )
                ret = "https://server-tool.lgpkv.treenod.com:3300/upload/stageTool_ext";

            Debug.LogFormat("IngameFileHelper Upload url : {0}", ret);

            return ret;
        }
    }
    static public string DownLoadAddress
    {
        get {
            string ret = _DownLoadAddress;
            if( ToolSettings.Instance.toolType == ToolSettings.ToolType.TOOL_EXT )
                ret = "https://server-tool.lgpkv.treenod.com:3300/download/stageTool_ext/";

            Debug.LogFormat("IngameFileHelper Download url : {0}", ret);
            return ret;
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

    public static IEnumerator WWW_Upload(string path)
    {
        FileInfo t = new FileInfo(Application.persistentDataPath + "/" + path);
        if (t.Exists)
        {
            WWW localFile = new WWW(Global.FileUri + Application.persistentDataPath + "/" + path);

            yield return localFile;

            if (localFile.error != null)
            {
                //Debug.Log("Open file error: " + localFile.error);
                yield break;
            }

            WWWForm postForm = new WWWForm();
            postForm.AddBinaryData("file", localFile.bytes,  path);

            WWW upload = new WWW(UpLoadAddress, postForm);

            yield return upload;
        }
    }

    public static IEnumerator WWW_DownLoad<T>(string name)
    {
        WWW www;
        WWWForm form = new WWWForm();
        form.AddField("Cache-Control", "no-cache");

        www = new WWW(DownLoadAddress + name);//, form);

        yield return www;
        yield return null;

        if (www.error != null)
            yield return www;

        if (www.error != null)
        {
            www.Dispose();
            yield return null;
        }
        else
        {
            MemoryStream memoryStream = new MemoryStream(www.bytes);
            www.Dispose();
            memoryStream.Position = 0;
            T result = default(T);
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                result = (T)serializer.Deserialize(memoryStream);
            }
            catch (Exception ex)
            {
            }
            yield return result;
        }
    }

    public static IEnumerator WWW_DownMapLoad<T>(string name)
    {
        WWW www;
        WWWForm form = new WWWForm();
        form.AddField("Cache-Control", "no-cache");

        www = new WWW(Global._cdnAddress + "/stage/" + name);//, form);

        yield return www;
        yield return null;

        if (www.error != null)
            yield return www;

        if (www.error != null)
        {
            www.Dispose();
            yield return null;
        }
        else
        {
            MemoryStream memoryStream = new MemoryStream(www.bytes);
            var serializer = new XmlSerializer(typeof(T));
            www.Dispose();
            memoryStream.Position = 0;
            yield return (T)serializer.Deserialize(memoryStream);
        }
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
}

