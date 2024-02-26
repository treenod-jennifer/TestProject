using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringHelper
{
    [System.Serializable]
    public class StringJsonKeyData
    {
        public string key;
        public string str;
    }

    public class StringVersionJsonData
    {
        public string version;
        public List<StringJsonKeyData> datas = new List<StringJsonKeyData>();
    }

    public class StringJsonData
    {
        public List<StringVersionJsonData> versionDatas = new List<StringVersionJsonData>();
    }

    public static bool LoadString(TextAsset textAsset, Dictionary<string, string> _stringData, string usingKey = "", bool isIncludeUsingKey = false)
    {
        char[] separator = new char[] { ',' };

        if (textAsset != null)
        {
            if (ConvertJson(textAsset.text))
            {
                return true;
            }
            else if (ConvertCSV(textAsset.text))
            {
                return true;
            }
        }

        return false;

        bool ConvertJson(string text)
        {
            if (!text.StartsWith("{") || !text.EndsWith("}")) return false;

            StringJsonData stringData = JsonFx.Json.JsonReader.Deserialize<StringJsonData>(text);
            
            if (stringData == null) return false;

            int appVersion = VerFromString(NetworkSettings.Instance.GetAppVersion());

            for (int i = 0; i < stringData.versionDatas.Count; i++)
            {
                string version = stringData.versionDatas[i].version;
                List<StringJsonKeyData> datas = stringData.versionDatas[i].datas;

                for (int j = 0; j < datas.Count; j++)
                {
                    if (IsCanAddStringData(usingKey, isIncludeUsingKey, datas[j].key) == false)
                    {
                        continue;
                    }

                    if (version == "Default")
                    {
                        if (!string.IsNullOrEmpty(datas[j]?.key))
                        {
                            _stringData[datas[j].key] = datas[j].str;
                        }
                    }
                    else
                    {
                        int textVersion = VerFromString(stringData.versionDatas[i].version);
                        if (textVersion <= appVersion)
                        {
                            if (_stringData.ContainsKey(datas[j].key))
                            {
                                _stringData.Remove(datas[j].key);
                            }
                            _stringData.Add(datas[j].key, datas[j].str);
                        }
                    }
                }
            }

            return true;
        }

        bool ConvertCSV(string text)
        {
            if (!text.Contains("\n=")) return false;

            string[] strSeperator = new string[] { "\n=" };
            string[] strChatList = text.Split(strSeperator, System.StringSplitOptions.None);
            for (int i = 0; i < strChatList.Length; i++)
            {
                string[] strChat = strChatList[i].Split(separator, 2);
                if (strChat.Length == 2)
                {
                    if (strChat[0].Length > 1 && strChat[1].Length > 1)
                    {
                        string key = strChat[0];
                        if (IsCanAddStringData(usingKey, isIncludeUsingKey, key) == false)
                            continue;
                        if (_stringData.ContainsKey(key))
                            Debug.LogWarningFormat("String Key Duplicated: {0}", key);
                        else
                            _stringData.Add(key, strChat[1]);
                    }
                }
            }

            return true;
        }
    }

    public static bool LoadStringFromJson(string jsonData, ref Dictionary<string, string> _stringData, string usingKey = "", bool isIncludeUsingKey = false)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            return false;
        }

        StringJsonData stringData = JsonFx.Json.JsonReader.Deserialize<StringJsonData>(jsonData);
        if (stringData == null)
            return false;

        int appVersion = VerFromString(NetworkSettings.Instance.GetAppVersion());
        for (int i = 0; i < stringData.versionDatas.Count; i++)
        {
            string version = stringData.versionDatas[i].version;
            List<StringJsonKeyData> datas = stringData.versionDatas[i].datas;

            for (int j = 0; j < datas.Count; j++)
            {
                if (IsCanAddStringData(usingKey, isIncludeUsingKey, datas[j].key) == false)
                    continue;
                if (version == "Default")
                {
                    if (!_stringData.ContainsKey(datas[j].key))
                        _stringData.Add(datas[j].key, datas[j].str);
                }
                else
                {
                    int textVersion = VerFromString(stringData.versionDatas[i].version);
                    if (textVersion <= appVersion)
                    {
                        if (_stringData.ContainsKey(datas[j].key))
                        {
                            _stringData.Remove(datas[j].key);
                        }
                        _stringData.Add(datas[j].key, datas[j].str);
                    }
                }
            }
        }
        return true;
    }

    public static bool LoadStringFromCSV(string csvData, ref Dictionary<string, string> _stringData, string usingKey = "", bool isIncludeUsingKey = false)
    {
        if (csvData == null)
            return false;

        char[] separator = new char[] { ',' };
        string[] strSeperator = new string[] { "\n=" };
        string[] strChatList = csvData.Split(strSeperator, System.StringSplitOptions.None);
        for (int i = 0; i < strChatList.Length; i++)
        {
            string[] strChat = strChatList[i].Split(separator, 2);
            if (strChat.Length == 2)
            {
                if (strChat[0].Length > 1 && strChat[1].Length > 1)
                {
                    string key = strChat[0];
                    if (IsCanAddStringData(usingKey, isIncludeUsingKey, key) == false)
                        continue;
                    if (!_stringData.ContainsKey(key))
                        _stringData.Add(key, strChat[1]);
                }
            }
        }

        return true;
    }

    public static void LoadStringFromCDN(string fileName, Dictionary<string, string> _stringData, string usingKey = "", bool isIncludeUsingKey = false, System.Action<bool> complete = null)
    {
        var cdnPath = GetCDNPath($"CachedScript/{fileName}");

        if (!cdnPath.isSuccess)
        {
            return;
        }

        Dictionary<string, string> stringData = _stringData;

        switch (GetFileType(cdnPath.fileName))
        {
            case "json":
                CDNLoadUtility.Load(cdnPath.folderName, cdnPath.fileName + "e", (string data) =>
                {
                    if (data == null)
                    {
                        Debug.LogWarning("Not Found Json");
                        complete?.Invoke(false);
                    }
                    else
                    {
                        LoadStringFromJson(data, ref stringData, usingKey, isIncludeUsingKey);
                        complete?.Invoke(true);
                    }
                });
                break;
            case "csv":
                CDNLoadUtility.Load(cdnPath.folderName, cdnPath.fileName + "e", (string data) =>
                {
                    if (data == null)
                    {
                        Debug.LogWarning("Not Found CSV");
                        complete?.Invoke(false);
                    }
                    else
                    {
                        LoadStringFromCSV(data, ref stringData, usingKey, isIncludeUsingKey);
                        complete?.Invoke(true);
                    }
                });
                break;
            default:
                Debug.LogWarning(cdnPath.fileName + " is Unknown Type");
                complete?.Invoke(false);
                break;
        }
    }

    //현재 텍스트를 string 데이터에 넣을 수 있는지 검사
    private static bool IsCanAddStringData(string usingKey, bool isIncludeUsingKey, string key)
    {
        if (usingKey == "")
            return true;

        //특정 키를 가진 텍스트를 추가하는 상황에서, 키가 포함 되어 있다면 추가 가능.
        //특정 키를 가진 텍스트를 빼는 상황에서, 키가 포함이 안 되어 있다면 추가 가능.
        return isIncludeUsingKey == key.Contains(usingKey);
    }

    private static string GetFileType(string fileName)
    {
        string[] words = fileName.Split('.');

        if (words.Length <= 1)
            return "UnknownType";

        return words[words.Length - 1].ToLower();
    }

    static int VerFromString(string ver)
    {
        if (ver.Length == 0)
            return 0;
        string[] vers = ver.Split('.');
        int v = 0;
        int verMx = 100 * 100;
        for (int j = 0; j < 2 && j < vers.Length; ++j)
        {
            v += verMx * System.Convert.ToInt32(vers[j]);
            verMx /= 100;
        }
        return v;
    }

    private static string AbsolutePath(string path)
    {
        path = path.Replace('\\', '/');
        return path;
    }

    private static string[] SplitFoldersAndFiles(string path)
    {    
        string[] foldersAndFiles = new string[2] { string.Empty, path };

        for(int i = path.Length - 1; i >= 0; i --)
        {
            if(path[i].Equals('/'))
            {
                foldersAndFiles[0] = path.Substring(0, i);
                foldersAndFiles[1] = path.Substring(i + 1, path.Length - i - 1);
                break;
            }
        }

        return foldersAndFiles;
    }

    public static bool TagContains(string tags, string tag)
    {
        return tags.Contains(tag);
    }

    public static string IntToString(int value)
    {
        if (value == 0) return "0";
        else return value.ToString("#,#");
    }

    public static (bool isSuccess, string folderName, string fileName) GetCDNPath (string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return (false, string.Empty, string.Empty);
        }

        var foldersAndFiles = SplitFoldersAndFiles(AbsolutePath(path));

        string folderName = foldersAndFiles[0];
        string fileName = foldersAndFiles[1];

        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(folderName))
        {
            return (false, string.Empty, string.Empty);
        }

        return (true, folderName, fileName);
    }
}

public static class UILabelExt
{
    public static void SetText( this UILabel[] labels, string t )
    {
        for(int i = 0; i < labels.Length; ++i)
        {
            labels[i].text = t;
        }
    }

    public static void SetColor(this UILabel[] labels, Color c)
    {
        for (int i = 0; i < labels.Length; ++i)
        {
            labels[i].color = c;
        }
    }

    public static void SetEffectColor(this UILabel[] labels, Color c)
    {
        for (int i = 0; i < labels.Length; ++i)
        {
            labels[i].effectColor = c;
        }
    }
}