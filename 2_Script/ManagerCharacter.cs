using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCharacter : MonoBehaviour {

    public static ManagerCharacter _instance = null;

    public Dictionary<int, ImportData_Live2D.ImportData> _live2dObjects = new Dictionary<int, ImportData_Live2D.ImportData>();
    public Dictionary<int, Dictionary<int, ImportData_Character.ImportData>> _characterObjects = new Dictionary<int, Dictionary<int, ImportData_Character.ImportData>>();

    public HashSet<TypeCharacterType> charLoadCandidates = new HashSet<TypeCharacterType>();
    public HashSet<TypeCharacterType> l2dcharLoadCandidates = new HashSet<TypeCharacterType>();

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    // Use this for initialization
    void Start () {

        SetDefaultData();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public ImportData_Character.ImportData GetCharacter(int charTypeNo, int skinIndex)
    {
        if ( _characterObjects.ContainsKey(charTypeNo) &&
            _characterObjects[charTypeNo] != null &&
            _characterObjects[charTypeNo].ContainsKey(skinIndex))
        {
            return (_characterObjects[charTypeNo])[skinIndex];
        }

        return null;

    }

    public ImportData_Live2D.ImportData GetLive2DCharacter(int charTypeNo)
    {
        if (_live2dObjects.ContainsKey(charTypeNo))
            return _live2dObjects[charTypeNo];

        // dynamic load
        return null;
    }

    public void LoadFromInternal(TypeCharacterType t, int skinIndex)
    {
        string path = "Assets/5_OutResource/chars/char_" + ((int)t).ToString() + "_" + skinIndex + "/ImportData.prefab";
#if UNITY_EDITOR
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (obj == null)
            return;
        ImportData_Character importInfo = obj.GetComponent<ImportData_Character>();

        if (!_characterObjects.ContainsKey((int)t))
            _characterObjects.Add((int)t, new Dictionary<int, ImportData_Character.ImportData>());

        if (importInfo != null)
        {
            _characterObjects[(int)t].Add(skinIndex, importInfo.data);
        }
        else
            Debug.LogErrorFormat("LoadFromInternal Failed: ImportData not found in {0}", name);
#endif
    }

    public IEnumerator LoadFromBundle(TypeCharacterType t, int skinIndex)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);

        string name = "char_" + ((int)t).ToString() + "_" + skinIndex;
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(name);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                if (!_characterObjects.ContainsKey((int)t))
                    _characterObjects.Add((int)t, new Dictionary<int, ImportData_Character.ImportData>());

                GameObject obj = assetBundle.LoadAsset<GameObject>("ImportData");
                if( obj)
                {
                    ImportData_Character importInfo = Instantiate(obj).GetComponent<ImportData_Character>();
                    if (importInfo != null)
                    {
                        _characterObjects[(int)t].Add(skinIndex, importInfo.data);
                    }
                    else
                        Debug.LogErrorFormat("Load From Bundle Failed: ImportData not found in {0}", name);
                }
            }
        }

        NetworkLoading.EndNetworkLoading();
    }

    void LoadL2DFromInternal(TypeCharacterType t)
    {
#if UNITY_EDITOR
        string path = "Assets/5_OutResource/l2dchars/l2d_" + ((int)t).ToString() + "/ImportData.prefab";
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (obj == null)
            return;
        ImportData_Live2D importInfo = obj.GetComponent<ImportData_Live2D>();
        var data = importInfo.data;

        if (!_live2dObjects.ContainsKey((int)t))
            _live2dObjects.Add((int)t, data);
        else
        {
            _live2dObjects[(int)t] = data;
        }
#endif
    }

    IEnumerator LoadL2DFromBundle(TypeCharacterType t)
    {
        NetworkLoading.MakeNetworkLoading(0.5f);

        string name = "l2d_" + ((int)t).ToString();
        IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(name);
        while (e.MoveNext())
            yield return e.Current;
        if (e.Current != null)
        {
            AssetBundle assetBundle = e.Current as AssetBundle;
            if (assetBundle != null)
            {
                GameObject obj = assetBundle.LoadAsset<GameObject>("ImportData");

                if( obj)
                {
                    ImportData_Live2D importInfo = Instantiate(obj).GetComponent<ImportData_Live2D>();
                    var data = importInfo.data;

                    if (!_live2dObjects.ContainsKey((int)t))
                        _live2dObjects.Add((int)t, data);
                    else
                    {
                        _live2dObjects[(int)t] = data;
                    }
                }
                
            }
        }

        NetworkLoading.EndNetworkLoading();
    }

    public void AddLoadList(List<TypeCharacterType> characters, List<TypeCharacterType> live2dChars)
    {
        for(int i = 0; i < characters.Count; ++i)
        {
            charLoadCandidates.Add(characters[i]);            
        }

        for (int i = 0; i < live2dChars.Count; ++i)
        {
            l2dcharLoadCandidates.Add(live2dChars[i]);
        }
    }

    public IEnumerator LoadCharacters()
    {
        foreach(var c in charLoadCandidates)
        {
            if (this._characterObjects.ContainsKey((int)c) 
                && _characterObjects[(int)c] != null
                && _characterObjects[(int)c].ContainsKey(0) )
            {
                continue;
            }

            if (!Global._instance.ForceLoadBundle && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer))
                LoadFromInternal(c, 0);
            else
                yield return LoadFromBundle(c, 0);
        }
        charLoadCandidates.Clear();

        foreach (var c in l2dcharLoadCandidates)
        {
            if (this._live2dObjects.ContainsKey((int)c) && _live2dObjects[(int)c] != null)
                continue;

            if (!Global._instance.ForceLoadBundle && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer))
                LoadL2DFromInternal(c);
            else
                yield return LoadL2DFromBundle(c);
        }
        l2dcharLoadCandidates.Clear();
    }

    public AudioClip GetChatSound(TypeCharacterType t)
    {
        var l2dChar = GetLive2DCharacter((int)t);
        if (l2dChar == null)
            return null;

        if (l2dChar.chatSound.Count == 0)
            return null;

        return l2dChar.chatSound[Random.Range(0, l2dChar.chatSound.Count)];
    }



    private void SetDefaultData()
    {
        _live2dObjects.Add((int)TypeCharacterType.Boni, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objBoni,
            defaultScale = -16f,
            emoticonOffset = new Vector3(-115f, 200f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_Boni], ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_Other] }
        });
        _live2dObjects.Add((int)TypeCharacterType.Coco, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objCoco,
            defaultScale = 26f,
            emoticonOffset = new Vector3(-55f, 230f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_coco] }
        });
        _live2dObjects.Add((int)TypeCharacterType.BlueBird, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objBlueBird,
            defaultScale = 10f,
            emoticonOffset = new Vector3(-145f, 170f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.m_bird_aham], ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.m_bird_hehe] }
        });
        _live2dObjects.Add((int)TypeCharacterType.Pang, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objPeng,
            defaultScale = 20f,
            emoticonOffset = new Vector3(-115f, 200f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_peng] }
        });
        _live2dObjects.Add((int)TypeCharacterType.Jeff, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objJeff,
            defaultScale = 26f,
            emoticonOffset = new Vector3(-55f, 230f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_jeff] }
        });
        _live2dObjects.Add((int)TypeCharacterType.Zelly, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objZelly,
            defaultScale = 18f,
            emoticonOffset = new Vector3(-115f, 200f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_jelly] }
        });
        _live2dObjects.Add((int)TypeCharacterType.Aroo, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objAroo,
            defaultScale = 28f,
            emoticonOffset = new Vector3(-55f, 230f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_aroo] }
        });
        _live2dObjects.Add((int)TypeCharacterType.Alphonse, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objAlphonse,
            defaultScale = 20f,
            emoticonOffset = new Vector3(-115f, 200f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_alphonse] }
        });
        _live2dObjects.Add((int)TypeCharacterType.Mai, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objMai,
            defaultScale = 25f,
            emoticonOffset = new Vector3(-115f, 200f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_Other] }
        });
        _live2dObjects.Add((int)TypeCharacterType.Kiri, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objKiri,
            defaultScale = 26f,
            emoticonOffset = new Vector3(-55f, 230f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance._bankLobby._audioList[(int)AudioLobby.Chat_Other] }
        });

        for(int i = 0; i < ManagerLobby._instance._objCharacter.Count; ++i)
        {
            if (ManagerLobby._instance._objCharacter[i] == null)
                continue;
            var dic = new Dictionary<int, ImportData_Character.ImportData>();
            dic.Add(0, new ImportData_Character.ImportData() { obj = ManagerLobby._instance._objCharacter[i] } );
            this._characterObjects.Add(i, dic);
        }
    }
}