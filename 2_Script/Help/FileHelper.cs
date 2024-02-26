using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;
//using JsonFx.Json;
//using Pathfinding.Serialization.JsonFx;

public static class FileHelper
{
    const string RESOURCES_FORLDER = "Assets/Resources";

    public static void ForEachFileInFolder(string path, string fileNamePattern, Action<FileInfo> onAction)
    {
        if (onAction == null)
            return;

        DirectoryInfo dirInfo = new DirectoryInfo(path);
        FileInfo[] fileInfos = dirInfo.GetFiles(fileNamePattern);
        foreach (FileInfo fileInfo in fileInfos)
        {
            onAction(fileInfo);
        }
    }

    public static string[] GetFiles(string path, string fileNamePattern)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        FileInfo[] fileInfos = dirInfo.GetFiles(fileNamePattern);
        return Array.ConvertAll<FileInfo, string>(fileInfos, (value) =>
        {
            return value.Name;
        });
    }

    public static string[] GetDirctories(string path, string pattern)
    {
        DirectoryInfo info = new DirectoryInfo(path);
        DirectoryInfo[] cInfo = info.GetDirectories(pattern, System.IO.SearchOption.TopDirectoryOnly);
        return Array.ConvertAll<DirectoryInfo, string>(cInfo, (value) =>
        {
            return value.Name;
        });
    }

    #region 파일 경로, 파일명 추출
    public static string CreateDataFilePath(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        if (fileName.StartsWith("/"))
            return Application.persistentDataPath + fileName;
        return Application.persistentDataPath + "/" + fileName;
    }

    public static string CreateResourceFilePath(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        if (fileName.StartsWith("/"))
            return RESOURCES_FORLDER + fileName;
        return RESOURCES_FORLDER + "/" + fileName;
    }

    public static string GetFileNameWithoutExtension(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        string name = fileInfo.Name;
        int extensionStartIndex = name.IndexOf(".");

        if (extensionStartIndex >= 0)
            return name.Substring(0, extensionStartIndex);
        else
            return name;
    }

    public static string GetPathWithoutExtension(string filePath)
    {
        int extensionStartIndex = filePath.IndexOf(".");

        if (extensionStartIndex >= 0)
            return filePath.Substring(0, extensionStartIndex);
        else
            return filePath;
    }

    public static bool IsExtension(string filePath, string paramExt)
    {
        string ext = Path.GetExtension(filePath);
        return String.Equals(ext, paramExt);
    }

    public static string MergePath(string folderPath, string fileName)
    {
        if (folderPath.EndsWith("/"))
            return folderPath + fileName;
        else
            return folderPath + "/" + fileName;
    }

    #endregion

    #region XML로 저장 / 로드
    public static void SaveXml<T>(string path, T data)
    {
        MemoryStream ms = new MemoryStream();
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, Encoding.UTF8);
        serializer.Serialize(xmlTextWriter, data);

        byte[] bytes = ms.ToArray();
        Save(path, bytes);
    }

    public static T LoadXml<T>(string path)
    {
        T data = default(T);

        FileInfo fileInfo = new FileInfo(path);
        if (!fileInfo.Exists)
            return data;

        XmlSerializer serializer = new XmlSerializer(typeof(T));
        FileStream fs = null;
        try
        {
            fs = new FileStream(path, FileMode.Open);
            data = (T)serializer.Deserialize(fs);
            fs.Close();
        }
        finally
        {
            if (fs != null)
                fs.Close();
        }
        return data;
    }
    #endregion

    #region 암호화/복호화해서 저장 / 로드
    public static void SaveWithEnc(string path, string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        SaveWithEnc(path, bytes);
    }

    public static void SaveWithEnc(string path, byte[] bytes)
    {
        byte[] encryptedBytes = GetEncryptedBytes(bytes);
        Save(path, encryptedBytes);
    }

    private static byte[] GetEncryptedBytes(byte[] data)
    {
        int offset = Random.Range(3, 9);
        string base64String = Convert.ToBase64String(data);

        StringBuilder builder = new StringBuilder();
        builder.Append(offset.ToString());

        for (int i = 0; i < base64String.Length; i++)
        {
            if (i % offset == 0)
            {
                char c = (char)Random.Range(33, 126);
                builder.Append(c);
            }
            builder.Append(base64String[i]);
        }
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public static byte[] LoadWithDec(string path)
    {
        byte[] bytes = Load(path);
        if (bytes == null)
            return bytes;

        return GetDecryptedBytes(bytes);
    }

    private static byte[] GetDecryptedBytes(byte[] data)
    {
        string str = Encoding.UTF8.GetString(data);
        int offset = int.Parse(str.Substring(0, 1)) + 1;
        string dataStr = str.Substring(1, str.Length - 1);
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < dataStr.Length; i++)
        {
            if (i % offset != 0)
            {
                builder.Append(dataStr[i]);
            }
        }
        return Convert.FromBase64String(builder.ToString());
    }
    #endregion

    #region 저장 / 로드
    public static void Save(string path, byte[] bytes)
    {
#if UNITY_EDITOR
        Debug.Log("File Save at " + path);
#endif
        CreateDirectoryFromFullFilePath(path);
        File.WriteAllBytes(path, bytes);
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
    #endregion

 /*   #region Json으로 저장 / 로드

    public static void SaveJson<T>(string path, T data, bool isPrettyFormat = false)
    {
        byte[] bytes = null;
        if (!isPrettyFormat)
        {
            string str = JsonWriter.Serialize(data);
            bytes = Encoding.UTF8.GetBytes(str);
        }
        else
        {
            StringBuilder builder = new StringBuilder();
            JsonWriterSettings setterJson = new JsonWriterSettings();
            setterJson.PrettyPrint = true;

            var writer = new JsonWriter(builder, setterJson);

            writer.Write(data);
            bytes = Encoding.UTF8.GetBytes(builder.ToString());
        }

        Save(path, bytes);

    }

    public static T LoadJson<T>(string path)
    {
        byte[] bytes = Load(path);
        if (bytes == null)
            return default(T);

        string str = Encoding.UTF8.GetString(bytes);
        return JsonReader.Deserialize<T>(str);
    }

    #endregion

    #region 암호/복호화해서 Json으로 저장 / 로드

    public static void SaveJsonWithEnc<T>(string path, T data)
    {
        string str = JsonWriter.Serialize(data);
        SaveWithEnc(path, str);
    }

    public static T LoadJsonWithDec<T>(string path)
    {
        byte[] bytes = LoadWithDec(path);
        if (bytes == null)
            return default(T);

        string str = Encoding.UTF8.GetString(bytes);
        return JsonReader.Deserialize<T>(str);
    }

    #endregion*/
}
