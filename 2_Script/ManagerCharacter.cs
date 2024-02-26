using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCharacter : MonoBehaviour {

    public static ManagerCharacter _instance = null;

    public Dictionary<int, ImportData_Live2D.ImportData> _live2dObjects = new Dictionary<int, ImportData_Live2D.ImportData>();
    public Dictionary<int, Dictionary<int, ImportData_Character.ImportData>> _characterObjects = new Dictionary<int, Dictionary<int, ImportData_Character.ImportData>>();

    public HashSet<int> charLoadCandidates = new HashSet<int>();
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

        ImportData_Character_Clone cloneInfo = obj.GetComponent<ImportData_Character_Clone>();
        if ( cloneInfo )
        {
            Dictionary<int, ImportData_Character.ImportData> orgChar = null;
            if( _characterObjects.TryGetValue((int)cloneInfo.originCharacter, out orgChar) )
            {
                if( _characterObjects.ContainsKey((int)t) )
                {
                    _characterObjects.Remove((int)t);
                }

                _characterObjects.Add((int)t, orgChar);
            }
            else
                Debug.LogErrorFormat("Load From Bundle(Clone) Failed: origin not Found", cloneInfo.originCharacter);
        }
        else
        {
            ImportData_Character importInfo = obj.GetComponent<ImportData_Character>();

            if (!_characterObjects.ContainsKey((int)t))
                _characterObjects.Add((int)t, new Dictionary<int, ImportData_Character.ImportData>());

            if (importInfo != null)
            {
                _characterObjects[(int)t].Add(skinIndex, importInfo.data);
            }
            else
                Debug.LogErrorFormat("LoadFromInternal Failed: ImportData not found in {0}", name);
        }
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
                    ImportData_Character_Clone cloneInfo = obj.GetComponent<ImportData_Character_Clone>();
                    if( cloneInfo )
                    {
                        Dictionary<int, ImportData_Character.ImportData> orgChar = null;
                        if (_characterObjects.TryGetValue((int)cloneInfo.originCharacter, out orgChar))
                        {
                            if (_characterObjects.ContainsKey((int)t))
                            {
                                _characterObjects.Remove((int)t);
                            }

                            _characterObjects.Add((int)t, orgChar);
                        }
                        else
                            Debug.LogErrorFormat("Load From Bundle(Clone) Failed: origin not Found", cloneInfo.originCharacter);
                    }
                    else
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
        }

        NetworkLoading.EndNetworkLoading();
    }

    void LoadL2DFromInternal(TypeCharacterType t)
    {
#if UNITY_EDITOR
        string path = "Assets/5_OutResource/SpineChars/spc_" + ((int)t).ToString() + "/ImportData.prefab";
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (obj == null)
        {
            Debug.LogError("Asset Not found: " + path + "(not in assetDataList)");
            return;
        }
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

        string name = "spc_" + ((int)t).ToString();
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

    int CreateCharLoadKey(TypeCharacterType c, int costumeId)
    {
        return ((int)c) * 100000 + costumeId;
    }

    void ParseCharLoadKey(int key, out TypeCharacterType t, out int costumeId)
    {
        t = (TypeCharacterType)((int)(key / 100000));
        costumeId = key % 100000;
    }

    //코스튬 인덱스를 받아오는 함수.
    public int GetCostumeIdx(TypeCharacterType lobbyCharacterIdx)
    {
        int coustumeIdx = 0;

        //탐험모드 오픈전에는 풀중첩이여도 외형이 변경이 안되도록.
        if (!ManagerAdventure.CheckStartable()) return coustumeIdx;

        //미션 24를 클리어 했을 때는 ManagerAdventure의 데이터가 세팅이 되지 않아 추가로 검사
        if (ManagerAdventure.Animal == null) return coustumeIdx;
        
        var animalData = ManagerAdventure.Animal.GetAnimal(lobbyCharacterIdx);
        
        if(animalData != null)
        {
            var animalInstance = ManagerAdventure.User.GetAnimalInstance(animalData.idx);

            if(IsSpecialLobbyChar(animalData.idx))
                coustumeIdx = animalInstance.lookId;
        }
        
        return coustumeIdx;
    }
    
    //스페셜 로비 배치 캐릭터인지 확인
    public bool IsSpecialLobbyChar(int? animalIdx)
    {
        if (ServerRepos.LoginCdn.isLobbySpecialAnimal == 0) return false;
        
        if (animalIdx == null) return false;
        
        //탐험모드 오픈전에는 스페셜 로비 배치 기능 사용하지 않도록.
        if (!ManagerAdventure.CheckStartable()) return false;
        
        //미션 24를 클리어 했을 때는 ManagerAdventure의 데이터가 세팅이 되지 않아 추가로 검사 
        if (ManagerAdventure.Animal == null) return false;
        
        var animalData = ManagerAdventure.Animal.GetAnimal(animalIdx.Value);

        return animalData != null && animalData.specialLobby > 0;
    }
    
    public void AddLoadList(List<TypeCharacterType> characters, List<CharCostumePair> charCostumePair, List<TypeCharacterType> live2dChars)
    {
        if( characters != null )
        {
            for (int i = 0; i < characters.Count; ++i)
            {
                int costuimeId = GetCostumeIdx(characters[i]);
                int charLoadKey = CreateCharLoadKey(characters[i], costuimeId);
                charLoadCandidates.Add(charLoadKey);
            }
        }
        

        if( charCostumePair != null )
        {
            for (int i = 0; i < charCostumePair.Count; ++i)
            {
                int charLoadKey = CreateCharLoadKey(charCostumePair[i].character, charCostumePair[i].costumeIdx);
                charLoadCandidates.Add(charLoadKey);
            }
        }

        if( live2dChars != null)
        {
            for (int i = 0; i < live2dChars.Count; ++i)
            {
                l2dcharLoadCandidates.Add(live2dChars[i]);
            }
        }
    }

    public void AddLoadLive2DList(List<TypeCharacterType> live2dChars)
    {
        for (int i = 0; i < live2dChars.Count; ++i)
        {
            l2dcharLoadCandidates.Add(live2dChars[i]);
        }
    }

    public IEnumerator LoadCharacters()
    {
        foreach(var c in charLoadCandidates)
        {
            TypeCharacterType charType = TypeCharacterType.Boni;
            int costumeId = 0;
            ParseCharLoadKey(c, out charType, out costumeId);

            if (this._characterObjects.ContainsKey((int)charType) 
                && _characterObjects[(int)charType] != null
                && _characterObjects[(int)charType].ContainsKey(costumeId) )
            {
                continue;
            }

            if (Global.LoadFromInternal)
                LoadFromInternal(charType, costumeId);
            else
                yield return LoadFromBundle(charType, costumeId);
        }
        charLoadCandidates.Clear();

        foreach (var c in l2dcharLoadCandidates)
        {
            if (this._live2dObjects.ContainsKey((int)c) && _live2dObjects[(int)c] != null)
                continue;

            if (Global.LoadFromInternal)
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
            chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_Boni), ManagerSound._instance.GetAudioClip(AudioLobby.Chat_Other) }
        });
        _live2dObjects.Add((int)TypeCharacterType.Coco, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objCoco,
            defaultScale = 26f,
            emoticonOffset = new Vector3(-55f, 230f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_coco) }
        });
        _live2dObjects.Add((int)TypeCharacterType.BlueBird, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objBlueBird,
            defaultScale = 10f,
            emoticonOffset = new Vector3(-145f, 170f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.m_bird_aham), ManagerSound._instance.GetAudioClip(AudioLobby.m_bird_hehe) }
        });
        _live2dObjects.Add((int)TypeCharacterType.Pang, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objPeng,
            defaultScale = 20f,
            emoticonOffset = new Vector3(-115f, 200f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_peng) }
        });
        _live2dObjects.Add((int)TypeCharacterType.Jeff, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objJeff,
            defaultScale = 26f,
            emoticonOffset = new Vector3(-55f, 230f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_jeff) }
        });
        _live2dObjects.Add((int)TypeCharacterType.Alphonse, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objAlphonse,
            defaultScale = -30f,
            emoticonOffset = new Vector3(-115f, 200f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_alphonse) }
        });
        _live2dObjects.Add((int)TypeCharacterType.COLLABO_1004, new ImportData_Live2D.ImportData()
        {
            obj = ManagerUI._instance._objNoi,
            defaultScale = 22f,
            emoticonOffset = new Vector3(-80f, 250f, 0f),
            chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_noi) }
        });

        //빌드에 포함되어 있는 캐릭터들의 ImportData를 설정해주는 부분.
        for (int i = 0; i < ManagerLobby._instance._objCharacter.Count; ++i)
        {
            if (ManagerLobby._instance._objCharacter[i] == null)
                continue;

            var dic = new Dictionary<int, ImportData_Character.ImportData>();

            ImportData_Character.ImportData importData = new ImportData_Character.ImportData();
            importData.obj = ManagerLobby._instance._objCharacter[i];
            if (ManagerLobby._instance._objCharacter[i].GetComponent<LobbyAIHints>() != null)
                importData.aiHint = ManagerLobby._instance._objCharacter[i].GetComponent<LobbyAIHints>();

            dic.Add(0, importData);
            this._characterObjects.Add(i, dic);
        }

        //기본 캐릭터 키 하드코딩
        _characterObjects[(int)TypeCharacterType.Alphonse][0].characterHeightOffset = 2.0f;
    }

    public void ReattachDefaultCharacterSound()
    {
        _live2dObjects[(int)TypeCharacterType.Boni].chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_Boni), ManagerSound._instance.GetAudioClip(AudioLobby.Chat_Other) };
        _live2dObjects[(int)TypeCharacterType.Coco].chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_coco) };
        _live2dObjects[(int)TypeCharacterType.BlueBird].chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.m_bird_aham), ManagerSound._instance.GetAudioClip(AudioLobby.m_bird_hehe) };
        _live2dObjects[(int)TypeCharacterType.Pang].chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_peng) };
        _live2dObjects[(int)TypeCharacterType.Jeff].chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_jeff) };
        _live2dObjects[(int)TypeCharacterType.Alphonse].chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_alphonse) };
        _live2dObjects[(int)TypeCharacterType.COLLABO_1004].chatSound = new List<AudioClip>() { ManagerSound._instance.GetAudioClip(AudioLobby.Chat_noi) };
    }
}