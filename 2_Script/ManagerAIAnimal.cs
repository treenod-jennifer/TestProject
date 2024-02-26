using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//캐릭터 타입과 코스튬 데이터
[System.Serializable]
public class CharacterAndCostumeData
{
    public TypeCharacterType charType = TypeCharacterType.None;
    public int costumeIdx = 0;
}

public class ManagerAIAnimal : MonoBehaviour
{
    public static ManagerAIAnimal instance = null;
    const int MAX_LOBBY_INSTALL = 5;
    
    public class CharacterPositionData
    {
        public Vector3 startPos;
        public List<LobbyAnimalAI.Circle> roamingArea = new List<LobbyAnimalAI.Circle>();
    }
    
    public List<CharacterPositionData> characterPosList = new List<CharacterPositionData>()
    {
        new CharacterPositionData(){startPos = new Vector3(-28.87f, 0, -41.95f), roamingArea = new List<LobbyAnimalAI.Circle>(){ new LobbyAnimalAI.Circle() {center = new Vector3(-12f, 0, -71.9f), radius = 60}}},
        new CharacterPositionData(){startPos = new Vector3(-22.34f, 0, -42.08f), roamingArea = new List<LobbyAnimalAI.Circle>(){ new LobbyAnimalAI.Circle() {center = new Vector3(2.9f, 0, -57.1f), radius = 60}}},
        new CharacterPositionData(){startPos = new Vector3(-13.1f, 0, -37.5f), roamingArea = new List<LobbyAnimalAI.Circle>(){ new LobbyAnimalAI.Circle() {center = new Vector3(-1.2f, 0, -73.2f), radius = 60}}},
        new CharacterPositionData(){startPos = new Vector3(-4.37f, 0, -40.31f), roamingArea = new List<LobbyAnimalAI.Circle>(){ new LobbyAnimalAI.Circle() {center = new Vector3(-24.5f, 0, -58.4f), radius = 60}}},
        new CharacterPositionData(){startPos = new Vector3(8.84f, 0, -38.07f), roamingArea = new List<LobbyAnimalAI.Circle>(){ new LobbyAnimalAI.Circle() {center = new Vector3(-39.1f, 0, -73.1f), radius = 60}}},
    };

    class UserData
    {
        public const string ChangedKey = "AIListChanged";
        
        List<int> selectedAnimals = new List<int>();
        List<int> installableAnimals = new List<int>();
        List<int> newComers = new List<int>();

        public void SyncFromServer(bool init = false)
        {
            if (init)
            {
                if (PlayerPrefs.HasKey(ChangedKey))
                {
                    bool   dataBroken          = false;
                    string saved               = PlayerPrefs.GetString(ChangedKey);
                    int[]  cacheInstallAnimals = JsonFx.Json.JsonReader.Deserialize<int[]>(saved);

                    // 캐싱된 로비 설치 동물 리스트가 서버에서 받은 유저가 보유한 동물 리스트에 포함 되어있을 경우 installableAnimals에 추가.
                    // 포함되지 않았을 경우 installableAnimals 초기화.
                    for (int i = 0; i < cacheInstallAnimals.Length; ++i)
                    {
                        if (ServerRepos.UserAdventureAnimals.Exists(x => x.animalId == cacheInstallAnimals[i]) == false)
                        {
                            dataBroken = true;
                            break;
                        }

                        if (installableAnimals.Exists(x => x == cacheInstallAnimals[i]) == false)
                        {
                            installableAnimals.Add(cacheInstallAnimals[i]);
                        }
                    }

                    // 데이터가 안맞는 경우. 다 날려버리고 처음부터 시작하는게 나음.
                    if (dataBroken)
                    {
                        newComers.Clear();
                        installableAnimals.Clear();
                        selectedAnimals.Clear();
                    }
                }
            }


            selectedAnimals.Clear();
            // 서버에서 받은 유저 로비 설치 동물 리스트 selectedAnimals에 추가.
            for (int i = 0; i < ServerRepos.UserAdventureLobbyAnimals.Count; ++i)
            {
                var lobbyAnimal = ServerRepos.UserAdventureLobbyAnimals[i];
                selectedAnimals.Add(lobbyAnimal.animalId);
            }

            // 서버에서 받은 유저가 보유한 동물 리스트 중 로비에 설치 가능하고, installableAnimals에 포함되어있지 않은 동물 인덱스 installableAnimals 추가.
            for (int i = 0; i < ServerRepos.UserAdventureAnimals.Count; ++i)
            {
                int  animalId    = ServerRepos.UserAdventureAnimals[i].animalId;
                bool installable = ServerContents.AdvAnimals[animalId].lobby_installable != 0;

                if (installable)
                {
                    if (installableAnimals.Exists(x => x == animalId) == false)
                    {
                        installableAnimals.Add(animalId);
                        newComers.Add(animalId);
                    }
                }
            }
        }

        public List<int> GetNewComers()
        {
            return newComers;
        }

        public void MarkAsRead()
        {
            PlayerPrefs.SetString(ChangedKey, JsonFx.Json.JsonWriter.Serialize(installableAnimals));
            newComers.Clear();
        }

        public List<int> GetSelectedList()
        {
            return new List<int>(selectedAnimals);
        }

        public List<int> GetInstallables(bool exceptSelected, bool sort)
        {
            List<int> retList = exceptSelected ? this.installableAnimals.FindAll(x => {
                if( this.selectedAnimals.Contains(x) )
                    return false;
                if( ServerContents.AdvAnimals.ContainsKey(x) == false)
                    return false;

                return true;
            } ) : installableAnimals;

            if( sort )
            {
                retList.Sort(new InstallableAnimalSorter() { newComers = this.newComers } );
            }

            return new List<int>(retList);
        }

        public class InstallableAnimalSorter : IComparer<int>
        {
            public List<int> newComers = null;
            public int Compare(int x, int y)
            {
                var animalX = ServerContents.AdvAnimals[x];
                var animalY = ServerContents.AdvAnimals[y];

                if( animalX == null || animalY == null)
                    return 0;

                if( newComers.Contains(x) != newComers.Contains(y) )
                {
                    if( newComers.Contains(x) == true )
                    {
                        return -1;
                    }
                    else return 1;
                }
                else
                {
                    if ( animalX.lobby_rewards.Count == 0 || animalY.lobby_rewards.Count == 0)
                        return 0;

                    if( animalX.lobby_rewards[0].type < animalY.lobby_rewards[0].type )
                        return -1;
                    else if (animalX.lobby_rewards[0].type > animalY.lobby_rewards[0].type)
                        return 1;
                    return 0;
                }
            }

        }
    }

    int refCount = 0;
    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (this == instance)
            instance = null;
    }

    public static int GetMaxLobbyInstallCount()
    {
        return MAX_LOBBY_INSTALL;
    }

    class NPCData
    {
        public int lobbyCharIdx;
        public int charCostume;
        public NPCHost npcHost;
    }

    class AreaAnimalData
    {
        public CharacterAndCostumeData charData;
        public List<LobbyAnimalAI.Circle> listRoamingAreaData;
        public NPCHost npcHost;
    }

    SortedDictionary<int, int> animalToLobbyDic = new SortedDictionary<int, int>();
    SortedDictionary<int, LobbyAnimalAI> aiDic = new SortedDictionary<int, LobbyAnimalAI>();

    private List<NPCData> npcList = new List<NPCData>();
    private List<AreaAnimalData> areaAnimalList = new List<AreaAnimalData>();

    static UserData User = new UserData();

    public static bool HaveNewcomer()
    {
        return User.GetNewComers().Count > 0;
    }

    public static void MarkAsRead()
    {
        User.MarkAsRead();
    }

    public static List<int> GetNewComers()
    {
        return User.GetNewComers();
    }

    public static List<int> GetInstallables()
    {
        return User.GetInstallables(true, true);
    }

    public static List<int> GetSelected()
    {
        return User.GetSelectedList();
    }

    public static IEnumerator StartSync(bool firstLoad)
    {
        User.SyncFromServer(firstLoad);

        if (ManagerLobby._instance == null || ManagerLobby._instance._state != TypeLobbyState.Wait)
            yield break;

        if (instance == null)
            yield break;

        instance.refCount++;
        if (instance.refCount == 1)
            yield return instance.StartCoroutine(instance.CoSync());

        yield break;
    }

    static public void Sync()
    {
        User.SyncFromServer(false);

        if (ManagerLobby._instance == null || ManagerLobby._instance._state != TypeLobbyState.Wait)
            return;
        
        if (instance == null)
            return;

        instance.StartCoroutine(instance.CoSync());
            

    }

    IEnumerator CoSync()
    {
        if( ManagerLobby.landIndex != 0)
        {
            yield break;
        }

        Debug.Log("CoSync Start");
        ManagerLobby._instance.SyncUseCharacterRegisterList();

        yield return CheckRegisterAdventureLobbyAnimals();
        CheckRegisterAreaAnimals();

        User.SyncFromServer(false);

        yield return InitAdventureLobbyAnimals();
        yield return InitLobbyNPC();
        yield return InitAreaAnimal();
        Debug.Log("CoSync End");

        if (instance.refCount < 1)
            OnRefZero();

        if( User.GetInstallables(false, false).Count <= 5 )
        {
            User.MarkAsRead();
        }

        if (HaveNewcomer() == true)
        {
            if (ManagerUI._instance != null)
                ManagerUI._instance.MakeLobbyGuestIcon();
            if (UIDiaryController._instance != null)
                UIDiaryController._instance.SetBtnDiaryAlarm();
        }
    }

    IEnumerator CheckRegisterAdventureLobbyAnimals()
    {
        HashSet<int> registeredAnimals = new HashSet<int>();
        for (int i = 0; i < ServerRepos.UserAdventureLobbyAnimals.Count; ++i)
        {
            var lobbyAnimal = ServerRepos.UserAdventureLobbyAnimals[i];
            registeredAnimals.Add(lobbyAnimal.animalId);
        }

        for (int i = 0; i < ServerRepos.UserAdventureAnimals.Count; ++i)
        {
            if (ServerRepos.UserAdventureLobbyAnimals.Count >= MAX_LOBBY_INSTALL)
                break;

            bool installable = ServerContents.AdvAnimals[ServerRepos.UserAdventureAnimals[i].animalId].lobby_installable != 0;
            if (installable && registeredAnimals.Contains(ServerRepos.UserAdventureAnimals[i].animalId) == false)
            {
                bool ret = false;
                ServerAPI.AdventureRegisterLobbyAnimal(ServerRepos.UserAdventureAnimals[i].animalId,
                    (resp) =>
                    {
                        ret = true;
                        if (resp.IsSuccess)
                        {

                        }
                    });

                while (ret == false)
                {
                    yield return null;
                }

            }
        }
    }

    void CheckRegisterAreaAnimals()
    {
        if (ManagerAreaAnimal.IsActiveEvent())
        {
            ManagerAreaAnimal.CheckRegisterAreaAnimals();
        }
    }

    public IEnumerator CoSelectLobbyAnimalCostumChange(int animalIdx)
    {
        if( ManagerAdventure.Active == false )
            yield break;
        
        List<TypeCharacterType> loadCandidates = new List<TypeCharacterType>();
        int lobbyCharIdx = ManagerAdventure.Animal.GetAnimal(animalIdx).lobbyCharIdx;
            
        //캐릭터가 로비에 설치되어 있지 않다면 동작하지 않음. 
        if (ManagerLobby._instance.IsCharacterExist((TypeCharacterType)lobbyCharIdx) == false)
            yield break;

        //현재 설치된 캐릭터 해제
        Uninstall(lobbyCharIdx);

        //로딩할 캐릭터 등록 및 로드
        if (lobbyCharIdx != -1)
            loadCandidates.Add((TypeCharacterType)lobbyCharIdx);
        ManagerCharacter._instance.AddLoadList(loadCandidates, null, null);
        yield return ManagerCharacter._instance.LoadCharacters();
        
        var ai = AttachAI(lobbyCharIdx, animalIdx);

        //로비에 배치가 되지 않을 때 초기화 해주기
        yield return InitAdventureLobbyAnimals();
    }
    
    public IEnumerator InitAdventureLobbyAnimals()
    {
        if( ManagerAdventure.Active == false )
            yield break;

        var userSelected = User.GetSelectedList();

        List<int> uninstallList = new List<int>();
        foreach(var a in this.animalToLobbyDic)
        {
            if(userSelected.Contains(a.Key)  == false )
                uninstallList.Add(a.Value);            
        }

        for(int i = 0; i < uninstallList.Count; ++i)
        {
            Uninstall(uninstallList[i]);
        }

        // 혹시 로딩안되어있는 경우를 위해 로드
        List<TypeCharacterType> loadCandidates = new List<TypeCharacterType>();
        for (int i = 0; i < userSelected.Count; ++i)
        {
            var animalId = User.GetSelectedList()[i];
            var animalBase = ManagerAdventure.Animal.GetAnimal(animalId);
            if(animalBase.lobbyCharIdx != -1)
                loadCandidates.Add((TypeCharacterType)animalBase.lobbyCharIdx);
        }
        ManagerCharacter._instance.AddLoadList(loadCandidates, null, null);
        yield return ManagerCharacter._instance.LoadCharacters();

        for (int i = 0; i < userSelected.Count; ++i)
        {
            var animalBase = ManagerAdventure.Animal.GetAnimal(userSelected[i]);
            
            if (animalBase.lobbyCharIdx == -1)
                continue;

            var ai = AttachAI(animalBase.lobbyCharIdx, userSelected[i]);
        }
    }

    IEnumerator InitLobbyNPC()
    {
        // 혹시 로딩안되어있는 경우를 위해 로드
        List<CharCostumePair> loadCandidates = new List<CharCostumePair>();
        for (int i = 0; i < npcList.Count; ++i)
        {
            CharCostumePair charCostumePair = new CharCostumePair();
            charCostumePair.character = (TypeCharacterType) npcList[i].lobbyCharIdx;
            charCostumePair.costumeIdx = npcList[i].charCostume;
            loadCandidates.Add(charCostumePair);
        }
        ManagerCharacter._instance.AddLoadList(null, loadCandidates, null);
        yield return ManagerCharacter._instance.LoadCharacters();

        for (int i = 0; i < npcList.Count; ++i)
        {
            var ai = AttachNPC(npcList[i].lobbyCharIdx, npcList[i].charCostume, npcList[i].npcHost);
        }
    }

    IEnumerator InitAreaAnimal()
    {
        // 혹시 로딩안되어있는 경우를 위해 로드
        List<CharCostumePair> loadCandidates = new List<CharCostumePair>();
        for (int i = 0; i < areaAnimalList.Count; ++i)
        {
            CharCostumePair charCostumePair = new CharCostumePair();
            charCostumePair.character = areaAnimalList[i].charData.charType;
            charCostumePair.costumeIdx = areaAnimalList[i].charData.costumeIdx;
            loadCandidates.Add(charCostumePair);
        }
        ManagerCharacter._instance.AddLoadList(null, loadCandidates, null);
        yield return ManagerCharacter._instance.LoadCharacters();

        for (int i = 0; i < areaAnimalList.Count; ++i)
        {
            AreaAnimalData data = areaAnimalList[i];
            var ai = AttachNPC((int)data.charData.charType, data.charData.costumeIdx, data.npcHost);
            int findindex = ManagerAreaAnimal.listAreaAnimalData.FindIndex
                (x => x.charData.charType == ai.agent._type && x.charData.costumeIdx == ai.agent.costumeId);

            if (findindex == -1)
                continue;

            //로밍 데이터 갱신.
            ai.aiHint.SetRoamingArea(data.listRoamingAreaData);
        }
    }

    internal void Unregister(int lobbyCharIdx)
    {
        if( aiDic.ContainsKey(lobbyCharIdx) )
        {
            var rewModule = aiDic[lobbyCharIdx].GetRewardModule();
            if ( rewModule != null)
            {
                animalToLobbyDic.Remove(rewModule.animalIdx);
            }
            
            aiDic.Remove(lobbyCharIdx);
        }
    }

    internal void Uninstall(int lobbyCharIdx)
    {
        if (ManagerLobby._instance != null)
        {
            if (AreaEvent.instance == null || AreaEvent.instance._characters.Contains((TypeCharacterType)lobbyCharIdx) == false)
            {
                var c = ManagerLobby._instance.GetCharacter((TypeCharacterType)lobbyCharIdx);
                if (c != null)
                {
                    ManagerLobby._instance.RemoveCharacter((TypeCharacterType)lobbyCharIdx);
                }
            }
            else
            {
                var c = ManagerLobby._instance.GetCharacter((TypeCharacterType)lobbyCharIdx);
                if (c != null)
                {
                    var installedAI = c.gameObject.GetComponentInChildren<LobbyAnimalAI>();
                    if( installedAI != null )
                    {
                        Destroy(installedAI);
                    }
                }
            }
        }
    }

    public IEnumerator ChangeInstalled(int from, int to)
    {
        bool ret = false;

        if( from == to )
            yield break;

        ServerAPI.AdventureChangeLobbyAnimal(from, to, (resp) =>
                    {
                        ret = true;
                        if (resp.IsSuccess)
                        {

                        }
                    });

        while (ret == false)
        {
            yield return null;
        }

        User.SyncFromServer(false);

        if (ManagerLobby._instance == null || ManagerLobby._instance._state != TypeLobbyState.Wait)
            yield break;

        if (ManagerAdventure.Active == false)
            yield break;

        if (instance == null)
            yield break;

        yield return CoSync();
    }
   
    public LobbyAnimalAI AttachAI(int lobbyCharIdx, int animalIdx)
    {
        int costuimeId = ManagerCharacter._instance.GetCostumeIdx((TypeCharacterType)lobbyCharIdx);
        var charData = ManagerCharacter._instance.GetCharacter(lobbyCharIdx, costuimeId);
        
        foreach (var animal in aiDic)
        {
            if (animal.Value.LobbyCharIdx == lobbyCharIdx && animal.Value.CostumeIdx == costuimeId)
                return null;
        }

        Vector3 defaultPos = charData.aiHint == null ? Vector3.zero : GetAnimalPositionData(animalIdx, charData.aiHint).startPos;

        var agent = ManagerLobby._instance.MakeOrGetCharacter((TypeCharacterType)lobbyCharIdx, costuimeId, defaultPos) as CharacterNPC;
        
        var animalAI = agent.gameObject.AddComponent<LobbyAnimalAI>();
        animalAI.Init(agent, lobbyCharIdx, costuimeId);
        animalAI.SetRewardModule(animalIdx);

        aiDic.Add(lobbyCharIdx, animalAI);

        animalToLobbyDic.Add(animalIdx, lobbyCharIdx);

        return animalAI;
    }

    public CharacterPositionData GetAnimalPositionData(int animalIdx, LobbyAIHints aiHints)
    {
        bool isSelectCharacter = false;
        CharacterPositionData posData = characterPosList[0];
        for (int i = 0; i < GetSelected().Count; i++)
        {
            if (GetSelected()[i] == animalIdx)
            {
                isSelectCharacter = true;
                posData = characterPosList[i];
            }
        }

        if (!isSelectCharacter && aiHints != null)
            posData = new CharacterPositionData() {startPos = aiHints.startPos, roamingArea = aiHints.roamingArea};

        return posData;
    }

    public LobbyAnimalAI AttachNPC(int lobbyCharIdx, int costumeId, NPCHost f)
    {
        if (aiDic.ContainsKey(lobbyCharIdx))
            return null;
        
        var charData = ManagerCharacter._instance.GetCharacter(lobbyCharIdx, costumeId);
        Vector3 defaultPos = charData.aiHint == null ? Vector3.zero : charData.aiHint.startPos;

        var agent = ManagerLobby._instance.MakeOrGetCharacter((TypeCharacterType)lobbyCharIdx, costumeId, defaultPos) as CharacterNPC;

        //트리거에 의해 생성된 Npc는 트리거에서 지정해준 동작을 해야하기 때문에 AI 스크립트를 붙이지 않음.
        if (agent.isMakeByTrigger == false)
        {
            var animalAI = agent.gameObject.GetComponent<LobbyAnimalAI>();
            if (animalAI == null)
            {
                animalAI = agent.gameObject.AddComponent<LobbyAnimalAI>();
                animalAI.Init(agent, lobbyCharIdx, costumeId);
                animalAI.SetNpcHost(f);
            }
            return animalAI;
        }
        return null;
    }

    public static void OnReboot()
    {
        // static 데이터기 때문에 instance 체크하지 않고 초기화.
        User = new UserData();
        
        if (instance != null)
        {
            foreach (var c in instance.aiDic)
            {
                ManagerLobby._instance.RemoveCharacter((TypeCharacterType)c.Key);
                Destroy(c.Value);
            }

            foreach (var c in instance.npcList)
            {
                ManagerLobby._instance.RemoveCharacter((TypeCharacterType)c.lobbyCharIdx);
            }

            foreach (var c in instance.areaAnimalList)
            {
                ManagerLobby._instance.RemoveCharacter((TypeCharacterType)c.charData.charType);
            }

            instance.npcList.Clear();
            instance.areaAnimalList.Clear();
            instance.aiDic.Clear();
            instance.animalToLobbyDic.Clear();
            Destroy(instance);
        }
    }

    //트리거가 시작될 때 관리.
    static public void BeginTriggerControl()
    {
        if (instance != null)
        {
            instance.refCount--;
            Debug.Log("ManagerAIAnimal.BeginTriggerControl() " + instance.refCount);
            if (instance == null)
                return;

            OnRefZero();
        }
    }

    static void OnRefZero()
    {
        for (int i = 0; i < instance.npcList.Count; ++i)
        {
            ManagerLobby._instance.RemoveCharacter((TypeCharacterType)instance.npcList[i].lobbyCharIdx);
        }

        for (int i = 0; i < instance.areaAnimalList.Count; ++i)
        {
            ManagerLobby._instance.RemoveCharacter((TypeCharacterType)instance.areaAnimalList[i].charData.charType);
        }

        foreach (var c in instance.aiDic)
        {
            // passive 가 true 라는 말은, 이벤트 씬에서 먼저 만들어뒀던 캐릭터라는 말이기 때문에
            // 이걸 없애버리면 안되지않을까

            if (c.Value.passive == false)
            {
                ManagerLobby._instance.RemoveCharacter((TypeCharacterType)c.Key);
                Destroy(c.Value);
            }
            else
            {
                var animalObj = ManagerLobby._instance.GetCharacter((TypeCharacterType)c.Key);
                var animalAiList = animalObj.gameObject.GetComponents<LobbyAnimalAI>();
                for (int i = 0; i < animalAiList.Length; ++i)
                {
                    Destroy(animalAiList[i]);
                }
            }
        }
        instance.aiDic.Clear();
        instance.animalToLobbyDic.Clear();
    }

    static public IEnumerator EndTriggerControl()
    {
        if( instance != null )
        {
            instance.refCount++;
            Debug.Log("ManagerAIAnimal.EndTriggerControl() " + instance.refCount);

            if (instance.refCount == 1)
            {
                User.SyncFromServer(false);

                if (ManagerLobby._instance == null)
                    yield break;

                yield return instance.CoSync();
            }
        }
    }

    LobbyAnimalAI GetAnimalAI_byAnimalIdx(int animalIdx)
    {
        if (animalToLobbyDic.ContainsKey(animalIdx))
        {
            if( aiDic.ContainsKey(animalToLobbyDic[animalIdx]) )
            {
                return aiDic[animalToLobbyDic[animalIdx]];
            }
            else return null;
        }
        else return null;
    }


    internal void OnReceivedLobbyAnimalReward(int animalIdx)
    {
        var ai = GetAnimalAI_byAnimalIdx(animalIdx);
        if( ai != null )
            ai.OnReceivedLobbyAnimalReward();
    }

    #region NPC 등록
    public void RegisterNPC(int _lobbyCharIdx, int _costumeIdx, NPCHost _npcHost)
    {
        this.npcList.Add(
            new NPCData()
            {
                lobbyCharIdx = _lobbyCharIdx,
                charCostume = _costumeIdx,
                npcHost = _npcHost
            });
    }
    #endregion

    #region 에리어 동물 등록
    //에리어 동물 등록
    public void RegisterAreaAnimal(CharacterAndCostumeData _cData, List<LobbyAnimalAI.Circle> _rDataList, NPCHost _npcHost)
    {
        AreaAnimalData newData = new AreaAnimalData()
        {
            charData = _cData,
            listRoamingAreaData = new List<LobbyAnimalAI.Circle>(_rDataList),
            npcHost = _npcHost
        };

        int findIndex = areaAnimalList.FindIndex(x => (x.charData == _cData));
        if (findIndex != -1)
            this.areaAnimalList[findIndex] = newData;
        else
            this.areaAnimalList.Add(newData);
    }

    //해당 동물이 등록되어 있는지
    public bool IsRegisterAreaAnimal(CharacterAndCostumeData _cData)
    {
        int findIndex = areaAnimalList.FindIndex(x => (x.charData == _cData));
        return (findIndex != -1);
    }

    public void UnregisterAreaAnimal(CharacterAndCostumeData _cData)
    {
        int findIndex = areaAnimalList.FindIndex(x => (x.charData == _cData));
        if (findIndex != -1)
            areaAnimalList.Remove(areaAnimalList[findIndex]);
    }
    #endregion
}


public interface NPCHost
{
    void OnNPCCreated(LobbyAnimalAI ai);
    void OnNPCDestroyed(LobbyAnimalAI ai);
    void OnNPCTap(LobbyAnimalAI ai);
}