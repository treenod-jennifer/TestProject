using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LocalText : MonoBehaviour
{
    private static LocalText instance = null;

    public static LocalText Instance
    {
        get
        {
            if(instance == null)
            {
                GameObject obj = new GameObject();
                instance = obj.AddComponent<LocalText>();
                obj.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(obj);
            }

            return instance;
        }
    }



    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }



    private const string LOCAL_TEXT_PATH = "LocalText/lc_g.json";



    private Dictionary<string, string> localTextDictionary = new Dictionary<string, string>();



    public void Init()
    {
        string path = LanguageUtility.FileNameConversion(LOCAL_TEXT_PATH);

        Box.LoadStreaming<string>(path, (localText) =>
        {
            localTextDictionary.Clear();
            StringHelper.LoadStringFromJson (localText, ref localTextDictionary);
        });
    }

    public bool HasString (string key)
    {
        if (localTextDictionary == null) return false;

        return localTextDictionary.ContainsKey(key);
    }

    public bool TryGetString(string key, out string text)
    {
        return localTextDictionary.TryGetValue(key, out text);
    }
}
