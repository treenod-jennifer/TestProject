using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//에리어에 속해있는 로밍에리어 데이터
[System.Serializable]
public class RoamingAreaData
{
    public int areaIndex = 0;
    public List<LobbyAnimalAI.Circle> listRoamingArea = new List<LobbyAnimalAI.Circle>();
}

public class ManagerAreaAnimal : MonoBehaviour, NPCHost
{
    //어떤 에리어의 로밍에리어 데이터를 사용할지에 대한 클래스
    [System.Serializable]
    public class RoamingAreaIndexData
    {
        public int areaIndex = 0;
        public List<int> listRoamingAreaIdx = new List<int>();
    }

    //입력을 위한 에리어 동물 데이터
    [System.Serializable]
    public class AreaAnimalData_InputData
    { 
         public CharacterAndCostumeData charData = new CharacterAndCostumeData();
         public List<RoamingAreaIndexData> listRomingAreaIndexData = new List<RoamingAreaIndexData>();
         public int openMission = 0;
    }

    //실제 사용하는 에리어 동물 데이터
    public class AreaAnimalData
    {
        public CharacterAndCostumeData charData = new CharacterAndCostumeData();
        public Dictionary<int, List<int>> dicRomingAreaIndexData = new Dictionary<int, List<int>>();
        public int openMission = 0;
    }

    public static ManagerAreaAnimal instance = null;

    public static List<AreaAnimalData> listAreaAnimalData = new List<AreaAnimalData>();
    public static Dictionary<int, List<LobbyAnimalAI.Circle>> dicRoamingAreaData = new Dictionary<int, List<LobbyAnimalAI.Circle>>();

    public void Awake()
    {
        instance = this;
    }

    public void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void Init()
    {
        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerAreaAnimal>();
        if (instance == null)
            return;
    }

    public static void OnReboot()
    {
        listAreaAnimalData.Clear();
        dicRoamingAreaData.Clear();
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    static public bool CheckStartable()
    {
        return true;
    }

    public static bool IsActiveEvent()
    {
        return ServerRepos.LoginCdn.deployVillagers == 1;
    }

    public static IEnumerator OnLobbyEnter()
    {
        if (!IsActiveEvent())
            yield break;

        List<TypeCharacterType> listLoadCharacter = new List<TypeCharacterType>();
        for (int i = 0; i < listAreaAnimalData.Count; i++)
        {
            listLoadCharacter.Add(listAreaAnimalData[i].charData.charType);
        }
        ManagerCharacter._instance.AddLoadList(listLoadCharacter, null, null);
        yield return ManagerCharacter._instance.LoadCharacters();

        CheckRegisterAreaAnimals();
    }

    #region 에리어 동물 리스트 등록 관련
    //에리어 동물 리스트에 등록할 캐릭터들과 제거할 동물 구분.
    public static void CheckRegisterAreaAnimals()
    {
        List<CharacterAndCostumeData> listRemoveData = new List<CharacterAndCostumeData>();
        for (int i = 0; i < listAreaAnimalData.Count; i++)
        {
            AreaAnimalData data = listAreaAnimalData[i];

            //등록할 수 없는 상태의 동물 데이터들은 등록하지 않고, 등록돼있으면 제거해줌.
            if (IsCanRegisterAreaAniamls(i) == false)
            {
                listRemoveData.Add(data.charData);
                continue;
            }

            //해당 동물 데이터에 등록된 로밍 에리어 중, 움직일 수 있는 에리어의 로밍 데이터를 가져옴.
            List<LobbyAnimalAI.Circle> listRoamingData = new List<LobbyAnimalAI.Circle>(GetListCanEnterRoamingArea(i));

            //등록할 수 있는 동물들의 데이터 등록 시켜줌.
            ManagerAIAnimal.instance.RegisterAreaAnimal(data.charData, listRoamingData, ManagerAreaAnimal.instance);
        }

        //캐릭터를 생성할 수 없는 상태의 동물 데이터들이 등록되어 있다면 레지스터에서 지워줌.
        for (int i = 0; i < listRemoveData.Count; i++)
        {
            if (ManagerAIAnimal.instance.IsRegisterAreaAnimal(listRemoveData[i]) == true)
            {
                ManagerAIAnimal.instance.UnregisterAreaAnimal(listRemoveData[i]);
            }
        }
    }

    //에리어 동물 리스트에 등록할 수 있는지 검사하는 함수.
    private static bool IsCanRegisterAreaAniamls(int checkIdx)
    {
        AreaAnimalData checkData = listAreaAnimalData[checkIdx];

        //미션 진행을 덜 한 상태면 등록하지 않음.
        if (checkData.openMission > 0 && ManagerData._instance._missionData.ContainsKey(checkData.openMission) &&
            ManagerData._instance._missionData[checkData.openMission].state != TypeMissionState.Clear)
            return false;

        //생성될 수 있는 에리어가 없다면 등록하지 않음.
        List<int> listCanEnterAreaIndex = new List<int>(GetCanEnterAreaIndex(new List<int>(checkData.dicRomingAreaIndexData.Keys)));
        if (listCanEnterAreaIndex.Count == 0)
            return false;

        //로비에 존재하는 캐릭터 중 트리거에서 생성한 캐릭터라면 등록하지 않음.
        if (ManagerLobby._instance.CheckCharacter_MakeByTrigger((int)checkData.charData.charType) == true)
            return false;

        //로비에 존재하는지의 여부와 상관없이, 트리거에서 사용중이라고 등록된 캐릭터라면 등록하지 않음.
        if (ManagerLobby._instance.IsUseCharacter_UseCharacterRegisterList(checkData.charData.charType) == true)
            return false;

        return true;
    }

    //로밍 데이터 들고오기
    private static List<LobbyAnimalAI.Circle> GetListCanEnterRoamingArea(int checkIdx)
    {
        AreaAnimalData checkData = listAreaAnimalData[checkIdx];
        List<LobbyAnimalAI.Circle> listRoamingData = new List<LobbyAnimalAI.Circle>();

        var enumerator = checkData.dicRomingAreaIndexData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            int areaIdx = enumerator.Current.Key;

            //검사하는 에리어가 오픈되어 있는지.
            if (ManagerLobby._instance.GetOpenAreaListIndex(areaIdx) == -1)
                continue;

            //검사하는 에리어가 테이블에 등록되어 있는지.
            if (ManagerAreaAnimal.dicRoamingAreaData.ContainsKey(areaIdx) == false)
                continue;

            //사용하고자 하는 로밍에리어 데이터를 리스트에 추가해줌.
            List<int> listRoamingAreaIdx = new List<int>(enumerator.Current.Value);
            for (int i = 0; i < listRoamingAreaIdx.Count; i++)
            {
                int roamingIdx = listRoamingAreaIdx[i];
                if (roamingIdx < ManagerAreaAnimal.dicRoamingAreaData[areaIdx].Count)
                {
                    listRoamingData.Add(ManagerAreaAnimal.dicRoamingAreaData[areaIdx][roamingIdx]);
                }
            }
        }
        return listRoamingData;
    }

    //들어갈 수 있는 에리어 리스트 가져오기
    private static List<int> GetCanEnterAreaIndex(List<int> listCheckIndex)
    {
        List<int> listAreaIndex = new List<int>();
        for (int i = 0; i < listCheckIndex.Count; i++)
        {
            if (ManagerLobby._instance.GetOpenAreaListIndex(listCheckIndex[i]) > -1)
                listAreaIndex.Add(listCheckIndex[i]);
        }
        return listAreaIndex;
    }
    #endregion

    public static void SetAreaAnimalData(List<AreaAnimalData_InputData> listData_a, List<RoamingAreaData> listData_r)
    {
        //동물 데이터 복사
        listAreaAnimalData = new List<AreaAnimalData>();
        for (int i = 0; i < listData_a.Count; i++)
        {
            AreaAnimalData_InputData originData = listData_a[i];

            //리스트로 저장된 데이터 딕셔너리로 변환해서 저장.
            Dictionary<int, List<int>> dicRoamingIndexData = new Dictionary<int, List<int>>();
            for (int j = 0; j < originData.listRomingAreaIndexData.Count; j++)
            {
                RoamingAreaIndexData indexData = originData.listRomingAreaIndexData[j];
                dicRoamingIndexData.Add(indexData.areaIndex, indexData.listRoamingAreaIdx);
            }

            listAreaAnimalData.Add(
                new AreaAnimalData
                {
                    charData = originData.charData,
                    dicRomingAreaIndexData = dicRoamingIndexData,
                    openMission = originData.openMission,
                });
        }

        //로밍 에리어 데이터 복사
        dicRoamingAreaData = new Dictionary<int, List<LobbyAnimalAI.Circle>>();
        for (int i = 0; i < listData_r.Count; i++)
        {
            List<LobbyAnimalAI.Circle> listRoamingData = new List<LobbyAnimalAI.Circle>(listData_r[i].listRoamingArea);
            dicRoamingAreaData.Add(listData_r[i].areaIndex, listRoamingData);
        }
    }

    #region interface
    public void OnNPCCreated(LobbyAnimalAI ai)
    {
        Debug.Log("Created");
    }

    public void OnNPCDestroyed(LobbyAnimalAI ai)
    {
        Debug.Log("Destroyed");
    }

    public void OnNPCTap(LobbyAnimalAI ai)
    {
        //캐릭터 사운드 출력
        if (ai.agent.tapSound != null && ai.agent.tapSound.Count > 0)
            ManagerSound.AudioPlay(ai.agent.tapSound[Random.Range(0, ai.agent.tapSound.Count)]);

        //캐릭터 텍스트 출력
        if (ai.aiHint == null || ai.aiHint.touchScripts == null || ai.aiHint.touchScripts.Count == 0)
            return;

        var txtKey = ai.aiHint.touchScripts[UnityEngine.Random.Range(0, ai.aiHint.touchScripts.Count)];
        string speechTxt = Global._instance.GetString(txtKey);
        ai.agent.MakeSpeech(speechTxt, AudioLobby.DEFAULT_AUDIO, true);
    }
    #endregion
}
