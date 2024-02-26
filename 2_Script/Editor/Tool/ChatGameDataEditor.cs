using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChatGameData))]
public class ChatGameDataEditor : Editor
{
    public  string       filePath = "";
    private ChatGameData chatGameData;

    private void OnEnable()
    {
        chatGameData = (ChatGameData)target;
    }

    public override void OnInspectorGUI()
    {
#if UNITY_EDITOR
        EditorGUILayout.LabelField("Chat Data 설정 파일 불러오기", EditorStyles.boldLabel);

        if (GUILayout.Button("Load CSV Data(교체)"))
        {
            filePath = EditorUtility.OpenFilePanel("Load CSV Data", Application.streamingAssetsPath, "csv");
            if (filePath.Length != 0)
            {
                string       source;
                StreamReader sr = new StreamReader(filePath);
                source = sr.ReadToEnd();
                sr.Close();

                List<Dictionary<string, object>> dataList = CSVReader.Read(source);
                chatGameData._listChatData = new List<ChatData>();

                for (int i = 0; i < dataList.Count; i++)
                {
                    ChatData chatData = new ChatData
                    {
                        tempTitle             = dataList[i]["tempTitle"].ToString(),
                        userInputBoxFunction  = dataList[i]["userInputBoxFunction"].ToString(),
                        stringKey             = dataList[i]["stringKey"].ToString(),
                        bChatColor            = ConvertToBool(dataList[i]["bChatColor"].ToString()),
                        bCharPositionChange   = ConvertToBool(dataList[i]["bCharPositionChange"].ToString()),
                        character_0           = ConvertToCharacterType(dataList[i]["character_0"]),
                        characterMotion_0     = dataList[i]["characterMotion_0"].ToString(),
                        characterExpression_0 = dataList[i]["characterExpression_0"].ToString(),
                        emoticon_0            = StringToEnum<TypeChatEmoticon>(dataList[i]["emoticon_0"].ToString()),
                        chatBack_0            = dataList[i]["chatBack_0"].ToString(),
                        character_1           = ConvertToCharacterType(dataList[i]["character_1"]),
                        characterMotion_1     = dataList[i]["characterMotion_1"].ToString(),
                        characterExpression_1 = dataList[i]["characterExpression_1"].ToString(),
                        emoticon_1            = StringToEnum<TypeChatEmoticon>(dataList[i]["emoticon_1"].ToString()),
                        chatBack_1            = dataList[i]["chatBack_1"].ToString(),
                        boxType               = StringToEnum<TypeChatBoxType>(dataList[i]["boxType"].ToString()),
                        chatBackCenter        = dataList[i]["chatBackCenter"].ToString(),
                    };
                    chatGameData._listChatData.Add(chatData);
                }

                Debug.Log("[Complete] ChatData 설정 완료.");
            }
        }

        if (GUILayout.Button("Load CSV Data(덮어씌우기)"))
        {
            filePath = EditorUtility.OpenFilePanel("Load CSV Data", Application.streamingAssetsPath, "csv");
            if (filePath.Length != 0)
            {
                string       source;
                StreamReader sr = new StreamReader(filePath);
                source = sr.ReadToEnd();
                sr.Close();

                List<Dictionary<string, object>> dataList = CSVReader.Read(source);
                if (chatGameData._listChatData == null || chatGameData._listChatData.Count != dataList.Count)
                {
                    Debug.LogError("[Error] 데이터 리스트 갯수가 맞지 않습니다.");
                }
                else
                {
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        var test = dataList[i]["character_0"];
                        chatGameData._listChatData[i].tempTitle             = dataList[i]["tempTitle"].ToString();
                        chatGameData._listChatData[i].userInputBoxFunction  = dataList[i]["userInputBoxFunction"].ToString();
                        chatGameData._listChatData[i].stringKey             = dataList[i]["stringKey"].ToString();
                        chatGameData._listChatData[i].bChatColor            = ConvertToBool(dataList[i]["bChatColor"].ToString());
                        chatGameData._listChatData[i].bCharPositionChange   = ConvertToBool(dataList[i]["bCharPositionChange"].ToString());
                        chatGameData._listChatData[i].character_0           = ConvertToCharacterType(dataList[i]["character_0"]);
                        chatGameData._listChatData[i].characterMotion_0     = dataList[i]["characterMotion_0"].ToString();
                        chatGameData._listChatData[i].characterExpression_0 = dataList[i]["characterExpression_0"].ToString();
                        chatGameData._listChatData[i].emoticon_0            = StringToEnum<TypeChatEmoticon>(dataList[i]["emoticon_0"].ToString());
                        chatGameData._listChatData[i].chatBack_0            = dataList[i]["chatBack_0"].ToString();
                        chatGameData._listChatData[i].character_1           = ConvertToCharacterType(dataList[i]["character_1"]);
                        chatGameData._listChatData[i].characterMotion_1     = dataList[i]["characterMotion_1"].ToString();
                        chatGameData._listChatData[i].characterExpression_1 = dataList[i]["characterExpression_1"].ToString();
                        chatGameData._listChatData[i].emoticon_1            = StringToEnum<TypeChatEmoticon>(dataList[i]["emoticon_1"].ToString());
                        chatGameData._listChatData[i].chatBack_1            = dataList[i]["chatBack_1"].ToString();
                        chatGameData._listChatData[i].boxType               = StringToEnum<TypeChatBoxType>(dataList[i]["boxType"].ToString());
                        chatGameData._listChatData[i].chatBackCenter        = dataList[i]["chatBackCenter"].ToString();
                    }


                    Debug.Log("[Complete] ChatData 덮어씌우기 완료.");
                }
            }
        }

        EditorGUILayout.Space();
#endif
        base.OnInspectorGUI();
    }

    private static T StringToEnum<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return (T)Enum.ToObject(typeof(T), 0);
        }

        return (T)Enum.Parse(typeof(T), key);
    }

    private static bool ConvertToBool(string key)
    {
        return key.Contains("TRUE");
    }

    private static TypeCharacterType ConvertToCharacterType(object key)
    {
        if (string.IsNullOrEmpty(key.ToString()))
        {
            return TypeCharacterType.None;
        }

        return (TypeCharacterType)key;
    }
}