using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetGlobal : MonoBehaviour
{
    public TextAsset _jpStringData = null;
    public TextAsset _exStringData = null;


	// Use this for initialization
	void Start () {
        if (Global._instance._stringData.Count == 0)
        {
            char[] separator = new char[] { ',' };
            TextAsset asset = null;
            if (Global._systemLanguage == CurrLang.eJap)
                asset = _jpStringData;
            else
                asset = _exStringData;

            if (asset != null)
            {
                string str = asset.text;
                string[] strSeperator = new string[] { "\n=" };
                string[] strChatList = str.Split(strSeperator, System.StringSplitOptions.None);
                for (int i = 0; i < strChatList.Length; i++)
                {
                    string[] strChat = strChatList[i].Split(separator, 2);
                    if (strChat.Length == 2)
                    {
                        if (strChat[0].Length > 1 && strChat[1].Length > 1)
                        {
                            string key = strChat[0];
                            Global._instance._stringData.Add(key, strChat[1]);
                        }
                    }
                }
            }
        }

	}

}
