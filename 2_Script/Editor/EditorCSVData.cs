using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class EditorCSVData
{
    static EditorCSVData window;
    static GUIStyle guiStyle;

    string strSceneName;
    string strScriptName;

    [MenuItem("Assets/CSV Change")]
    private static void Change(MenuCommand menuCommand)
    {
        List<string> textList = new List<string>();
        TextAsset textAsset = Selection.activeObject as TextAsset;
        string CSVPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        char[] separator = new char[] { ',' };
        if (textAsset != null)
        {
            string str = textAsset.text;

            //문자열을 배열로 반환.
            string[] strChatList = str.Split('\n');
            for (int i = 0; i < strChatList.Length; i++)
            {
                string[] strChat = strChatList[i].Split(separator, 2);
                if (strChat.Length == 2)
                {
                    if (strChat[0].Length > 1 && strChat[1].Length > 1)
                    {   
                        string ss = strChat[1].Replace("\"", "");
                        ss = ss.Replace("\\n", "\n");
                        string key = strChat[0].Replace("\"", "");
                        textList.Add(key + "," + ss);
                    }
                }
            }
            if (textList != null)
            {
                string[] strChatTest = textList.ToArray();
                SaveCSV(CSVPath, strChatTest);
            }
        }
    }

    [MenuItem("Assets/CSV Change", true)]
    private static bool CSVChangeOptionValidation()
    {
        if (Selection.activeObject == null)
            return false;

        return Selection.activeObject.GetType() == typeof(TextAsset);
    }

    private static void SaveCSV(string path, string[] strChat)
    {   
        //덮어쓰기 형식으로 파일 생성.
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        StreamWriter outStream = new StreamWriter(fs, Encoding.UTF8);

        for (int i = 0; i < (strChat.Length - 1); i++)
        {
            outStream.Write(strChat[i]);
            outStream.Write("\n=");
        }
        outStream.Write(strChat[(strChat.Length - 1)]);
        outStream.Flush();
        outStream.Close();
        fs.Close();

#if UNITY_EDITOR
        Debug.Log("File Save at " + path);
#endif
    }
}
#endif