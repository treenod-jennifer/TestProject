using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

public class ControllerHousingComparer : IComparer<PlusHousingModelData>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(PlusHousingModelData a, PlusHousingModelData b)
    {
        //하우징 인덱스 검사.
        if (a.housingIndex < b.housingIndex)
            return -1;
        else if (a.housingIndex > b.housingIndex)
            return 1;
        else
        {   
            if (a.modelIndex < b.modelIndex)
                return -1;
            else if (a.modelIndex > b.modelIndex)
                return 1;
            else
                return 0;
        }
    }
}

public class HousingIndexAndCostumeJsonData
{
    public List<string> keyList = new List<string>();

    public HousingIndexAndCostumeJsonData(List<string> indexList)
    {
        this.keyList = indexList;
    }

    public void ConvertJsonData(HousingIndexAndCostumeJsonData jsonData, List<string> indexList)
    {
        for (int i = 0; i < jsonData.keyList.Count; i++)
        {
            indexList.Add(jsonData.keyList[i]);
        }
    }
}

[System.Serializable]
public class DictionaryJsonData<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField]
    List<TKey> keys;
    [SerializeField]
    List<TValue> values;

    Dictionary<TKey, TValue> keyList;
    public Dictionary<TKey, TValue> ToDictionary() { return keyList; }

    public DictionaryJsonData(Dictionary<TKey, TValue> indexList)
    {
        this.keyList = indexList;
    }

    public void OnBeforeSerialize()
    {
        keys = new List<TKey>(keyList.Keys);
        values = new List<TValue>(keyList.Values);
    }

    public void OnAfterDeserialize()
    {
        var count = System.Math.Min(keys.Count, values.Count);
        keyList = new Dictionary<TKey, TValue>(count);
        for (var i = 0; i < count; ++i)
        {
            keyList.Add(keys[i], values[i]);
        }
    }

    public void ConvertJsonData(DictionaryJsonData<TKey,TValue> jsonData, Dictionary<TKey, TValue> indexList)
    {
        var enumerator_Q = jsonData.keyList.GetEnumerator();
        while (enumerator_Q.MoveNext())
        {
            indexList.Add(enumerator_Q.Current.Key, enumerator_Q.Current.Value);
        }
    }
}

public class UIDiaryController : MonoBehaviour
{
    public static UIDiaryController _instance = null;

    UIDiaryData data = null;
    UIDiaryTap[] arrayDiaryTap = null;
    bool refreshMaterialGetInfo = false;

    public bool isDiaryActionComplete = true;

    void Awake()
    {
        _instance = this;
    }

    void OnDestroy()
    {
        if( _instance == this)
            _instance = null;
    }

    public IEnumerator CoDiaryActionComplete(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        DiaryActionComplete();
    }

    public void DiaryActionComplete()
    {
        isDiaryActionComplete = true;
    }

    public void InitDiaryData()
    {
        if (data == null)
        {
            data = new UIDiaryData();
        }
        CheckMissionData();
        CheckQuestData();
        CheckQuestProgressData();
        CheckHousingData();
        CheckCostumeData();
        CheckStampData();
        SetBtnDiaryAlarm();
    }

    public void CheckMissionData()
    {
        data.MissionAlarm = false;
        var enumerator_M = ServerRepos.OpenMission.GetEnumerator();
        while (enumerator_M.MoveNext())
        {
            ServerUserMission mData = enumerator_M.Current;
            if (mData.state == (int)TypeMissionState.Active)
            {
                if (ManagerData._instance._missionData.ContainsKey(mData.idx))
                {
                    MissionData mission = ManagerData._instance._missionData[mData.idx];
                    if (mission.waitTime > 0 && mData.clearTime > 0)
                    {
                        if (Global.GetTime() > mData.clearTime)
                        {
                            data.MissionAlarm = true;
                            break;
                        }
                    }
                    else if (Global.star >= mission.needStar)
                    {
                        data.MissionAlarm = true;
                        break;
                    }
                }
            }
        }
    }

    public void CheckQuestData()
    {
        bool bEvent = false;
        if (data.QuestGetData == null)
            data.QuestGetData = new List<QuestGameData>();
        else
            data.QuestGetData.Clear();

        data.QuestRewardCount = 0;
        data.EventQuestRewardCount = 0;
        //퀘스트 데이터 들고와서 세팅.
        var enumerator_Q = ManagerData._instance._questGameData.GetEnumerator();
        while (enumerator_Q.MoveNext())
        {
            QuestGameData qData = enumerator_Q.Current.Value;
            //끝나는 시간이 없거나, 이벤트가 끝나는 시간이 현재시간보다 더 많이 남았을 경우 리스트에 추가.
            if (qData.valueTime1 == 1 || qData.valueTime1 >= Global.GetTime())
            {
                data.QuestGetData.Add(enumerator_Q.Current.Value);
            }
        }

        //퀘스트 데이터 정렬한 뒤, 해당 퀘스트 생성.
        if (data.QuestGetData.Count > 0)
        {
            for (int i = 0; i < data.QuestGetData.Count; i++)
            {
                QuestGameData qData = data.QuestGetData[i];
                if (qData.state != QuestState.Finished)
                {
                    //이벤트 여부 검사.
                    if (qData.valueTime1 > 1)
                    {
                        bEvent = true;
                    }
                }
                if (qData.state == QuestState.Completed)
                {
                    if (qData.valueTime1 > 1)
                        data.EventQuestRewardCount++;
                    else
                        data.QuestRewardCount++;
                }
            }
        }
    }

    public void CheckQuestProgressData()
    {
        CheckQuestProgressPlayerPrefs();
        SaveQuestProgressPlayerPrefs(false);
    }

    public void CheckHousingData()
    {
        if (data.ProgressHousingData == null)
            data.ProgressHousingData = new List<PlusHousingModelData>();
        else
            data.ProgressHousingData.Clear();

        if (data.DicProgressHousingPreview == null)
            data.DicProgressHousingPreview = new Dictionary<int, bool>();
        else
            data.DicProgressHousingPreview.Clear();

        //하우징 타입이 미션 타입인거 제외하고 리스트에 추가(active 상태에 따라 Progrss 리스트 구분).
        var enumerator = ManagerHousing.GetHousingList().GetEnumerator();
        while (enumerator.MoveNext())
        {
            PlusHousingModelData hData = enumerator.Current;
            if (hData.type != PlusHousingModelDataType.byProgress)
            {
                continue;
            }

            if (hData.active == 0)
            {
                if (hData.openMission == 0)
                    continue;
                var missionIndex = hData.openMission;
                if (!ManagerData._instance._missionData.ContainsKey(missionIndex))
                {
                    continue;
                }
                
                if (hData.expire_ts == 0)
                {   // 일반 하우징의 경우, 해당 하우징의 미션 까지 완료되지 않은 상태이면 표시 안 함.
                    if (ManagerData._instance._missionData[missionIndex].state != TypeMissionState.Clear)
                        continue;
                }
                else
                {   // 이벤트 하우징의 경우, 일정 미션 이상을 진행한 상태이고 시간이 다 끝나지 않은 상태면 추가.
                    if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventHousingOpen
                        || hData.expire_ts < Global.GetTime())
                    {
                        continue;
                    }
                    if (!data.DicProgressHousingPreview.ContainsKey(hData.housingIndex))
                        data.DicProgressHousingPreview.Add(hData.housingIndex, true);
                }
                data.ProgressHousingData.Add(hData);
            }
        }
        //하우징 데이터 카테고리 순으로 정렬.
        ControllerHousingComparer comparer = new ControllerHousingComparer();
        data.ProgressHousingData.Sort(comparer);

        CheckHousingNewIconPlayerPrefs();
        CheckGetHousingData();
    }

    public void CheckGetHousingData()
    {
        if (data.DicHousingCateroryAlarmData == null)
            data.DicHousingCateroryAlarmData = new Dictionary<int, int>();
        else
            data.DicHousingCateroryAlarmData.Clear();

        data.HousingGetCount = 0;
        int matCount = 0;
        int needCount = 0;
        PlusHousingModelData plusData;

        int categoryIdx = -1;
        int getCount = 0;
        bool bFirstModel = true;

        List<int> newModelList = new List<int>();
        for (int i = 0; i < data.ProgressHousingData.Count; i++)
        {   
            //해당 카테고리에서 제작할 수 있는 아이템 수 갱신.
            if (categoryIdx != data.ProgressHousingData[i].housingIndex)
            {
                if(getCount > 0)
                    data.DicHousingCateroryAlarmData.Add(categoryIdx, getCount);
                categoryIdx = data.ProgressHousingData[i].housingIndex;
                bFirstModel = true;
                getCount = 0;

                //new 아이템 목록 들고옴.
                newModelList = new List<int>();
                if (data.DicProgressHousingNewData.ContainsKey(data.ProgressHousingData[i].housingIndex))
                {
                    data.DicProgressHousingNewData.TryGetValue(data.ProgressHousingData[i].housingIndex, out newModelList);
                }
            }
            matCount = 0;
            needCount = 0;
            plusData = data.ProgressHousingData[i];
            for (int j = 0; j < plusData.material.Count; j++)
            {
                needCount += plusData.material[j]._count;
                matCount += GetMaterialCount(plusData.material[j]);
            }
            //재료 수 다 채웠을 때는 버튼활성화.
            if (matCount >= needCount)
            {
                data.HousingGetCount++;
                //미리보기로 보이는 데코는 getCount 에서 제외.
                if (data.DicProgressHousingPreview.ContainsKey(categoryIdx) && IsPreviewItem(newModelList, plusData) == false ||
                    !data.DicProgressHousingPreview.ContainsKey(categoryIdx) && bFirstModel == false)
                {
                    getCount++;
                }
            }
            bFirstModel = false;
        }
        //마지막 데이터까지 넣어줌.
        if (getCount > 0)
        {
            data.DicHousingCateroryAlarmData.Add(categoryIdx, getCount);
        }
    }

    public void CheckMaterialGetInfo()
    {
        if (data.DicMaterialGetInfoData == null)
            data.DicMaterialGetInfoData = new Dictionary<int, MaterialGetInfo>();
        else if (data.DicMaterialGetInfoData.Count > 0)
            return;

        StartCoroutine(LoadMaterialGetInfoFile());
    }

    IEnumerator LoadMaterialGetInfoFile()
    {
        string url = Global.FileUri + Application.persistentDataPath + "/MaterialGetInfo.xml";
        using (var www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (!www.IsError() && www.downloadHandler != null)
            {
                if (www.downloadHandler.text.Length > 0)
                {
                    System.IO.StringReader stringReader = new System.IO.StringReader(www.downloadHandler.text);
                    //stringReader.Read(); // skip BOM

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(stringReader.ReadToEnd());

                    //최상위 노드 선택.
                    XmlNode rootNode = doc.ChildNodes[1];
                    for (int i = 0; i < rootNode.ChildNodes.Count; ++i)
                    {
                        XmlNode node = rootNode.ChildNodes[i];
                        MaterialGetInfo info = new MaterialGetInfo();
                        int index = xmlhelper.GetInt(node, "Index", 0);
                        info.bFiled = xmlhelper.GetBool(node, "Field", false);
                        info.bBox1 = xmlhelper.GetBool(node, "Box1", false);
                        info.bBox2 = xmlhelper.GetBool(node, "Box2", false);
                        info.bBox3 = xmlhelper.GetBool(node, "Box3", false);

                        if (info != null)
                        {
                            if (data.DicMaterialGetInfoData.ContainsKey(index))
                                data.DicMaterialGetInfoData.Remove(index);
                            data.DicMaterialGetInfoData.Add(index, info);
                        }
                    }
                }
                else yield break;
            }
            else
                yield break;
        }
    }

    public MaterialGetInfo GetMaterialGetInfo(int matIndex)
    {
        MaterialGetInfo info = null;
        if (data.DicMaterialGetInfoData.ContainsKey(matIndex))
            data.DicMaterialGetInfoData.TryGetValue(matIndex, out info);
        return info;
    }
    
    private bool IsPreviewItem(List<int> newModelList, PlusHousingModelData pData)
    {
        //이벤트 타입인지 검사.
        if (pData.expire_ts > 0)
            return true;

        //New 아이템인지 검사.
        for (int i = 0; i < newModelList.Count; i++)
        {
            if (newModelList[i] == pData.modelIndex)
                return true;
        }
        return false;
    }

    private int GetMaterialCount(HousingMaterialData data)
    {
        int materialCount = 0;
        for (int i = 0; i < ServerRepos.UserMaterials.Count; i++)
        {
            if (ServerRepos.UserMaterials[i].index == data._index)
            {
                materialCount = ServerRepos.UserMaterials[i].count;
            }
        }

        if (materialCount > data._count)
        {
            materialCount = data._count;
        }
        //재료 수 반환.
        return materialCount;
    }

    public void CheckCostumeData()
    {
        if (data.CostumeData == null)
            data.CostumeData = new List<CdnCostume>();
        else
            data.CostumeData.Clear();

        //코스튬 데이터 추가.
        var enumerator = ManagerData._instance._costumeData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            CdnCostume costumeData = enumerator.Current.Value;
            //코스튬 데이터가 기본형이거나, 1번 타입이라면 (코인으로 구매가능한) 코스튬이라면 추가.
            if (costumeData.costume_id == 0 || costumeData.get_type == 1)
            {
                data.CostumeData.Add(costumeData);
            }
        }

        ////현재 유저가 들고있는 데이터 값 들고오기.
        for (int i = 0; i < ServerRepos.UserCostumes.Count; i++)
        {
            string key = string.Format("{0}_{1}", ServerRepos.UserCostumes[i].char_id, ServerRepos.UserCostumes[i].costume_id);
            if (ManagerData._instance._costumeData.ContainsKey(key))
            {
                CdnCostume userCostumeData = ManagerData._instance._costumeData[key];

                //1번 타입의 데이터는 위에서 이미 추가했으니까 제외하고 추가.
                if (userCostumeData.get_type != 1)
                {
                    data.CostumeData.Add(userCostumeData);
                }
            }
        }
        //코스튬 데이터 캐릭터 순으로 정렬.
        CostumeComparer comparer = new CostumeComparer();
        data.CostumeData.Sort(comparer);

        //코스튬 새로운 아이템 있는지 검사 / PlayerPrefs에 저장.
        CheckCostumeNewIconPlayerPrefs();

        if (!PlayerPrefs.HasKey("Costume"))
        {
            SaveCostumeNewIconPlayerPrefs();
        }
    }

    public List<CdnCostume> GetCostumeData()
    {
        return data.CostumeData;
    }

    public void CheckStampData()
    {
        data.StampAlarm = false;
        int stampCount = ServerRepos.UserStamps.Count;

        //스탬프 카운트 저장돼있지 않다면 현재 유저가 들고있는 스탬프 갯수 저장.
        if (!PlayerPrefs.HasKey("StampCount"))
        {
            PlayerPrefs.SetInt("StampCount", stampCount);
            return;
        }
        
        if (PlayerPrefs.GetInt("StampCount") < stampCount)
        {   //스탬프 카운트가 이전보다 많으면 StampAlarm 상태 변환.
            data.StampAlarm = true;
        }
        else if(PlayerPrefs.GetInt("StampCount") > stampCount)
        {   //스탬프 카운트가 이전보다 적어졌으면 현재 스탬프 수 저장.
            PlayerPrefs.SetInt("StampCount", stampCount);
        }
    }

    public void OnClickBtnGetQuest(QuestGameData qData)
    {
        //보상받은 퀘스트 데이터는 삭제.
        for (int i = 0; i < data.QuestGetData.Count; i++)
        {
            if (data.QuestGetData[i].index == qData.index)
            {
                if( data.QuestGetData[i].valueTime1 > 1 )
                    data.EventQuestRewardCount--;
                else
                    data.QuestRewardCount--;

                data.QuestGetData.Remove(data.QuestGetData[i]);
                break;
            }
        }
        
        SetBtnDiaryAlarm();
    }

    public void OnClickStampTap()
    {
        data.StampAlarm = false;
        int stampCount = ServerRepos.UserStamps.Count;
        //스탬프 카운트 갱신.
        if (!PlayerPrefs.HasKey("StampCount") || PlayerPrefs.GetInt("StampCount") != stampCount)
        {
            PlayerPrefs.SetInt("StampCount", stampCount);
        }
        SetBtnDiaryAlarm();
    }

    public void UpdateMissionData()
    {
        CheckMissionData();
        SetBtnDiaryAlarm();
    }

    public void UpdateQuestData(bool isShowAlarmImmediately)
    {
        CheckQuestData();
        SetBtnDiaryAlarm();
        //알람 데이터 갱신 & 팝업 표시
        SaveQuestProgressPlayerPrefs(isShowAlarmImmediately);
    }

    public void UpdateProgressHousingData()
    {
        CheckHousingData();
        SetBtnDiaryAlarm();
    }

    public void UpdateCostumeData()
    {
        CheckCostumeData();
        SetBtnDiaryAlarm();
    }

    public void UpdateStampData()
    {
        CheckStampData();
        SetBtnDiaryAlarm();
    }

    public void SetBtnDiaryAlarm()
    {
        bool bAlarm = false;
        if (data.MissionAlarm == true || data.QuestRewardCount > 0 || data.HousingGetCount > 0 
            || data.StampAlarm == true || data.DicCostumeNewData.Count > 0 || ManagerAIAnimal.HaveNewcomer() == true)
            bAlarm = true;

        if(ManagerUI._instance.diaryNewIcon.activeInHierarchy != bAlarm)
            ManagerUI._instance.SettingDiaryNewIcon(bAlarm);
    }

    public void SettingDiaryTap(UIDiaryTap[] diaryTaps)
    {
        if(arrayDiaryTap == null)
            arrayDiaryTap = diaryTaps;
        InitDiaryTapAlarmIcon(1);
        InitDiaryTapAlarmIcon(2);
        InitDiaryTapAlarmIcon(3);
        InitDiaryTapAlarmIcon(4);
    }
    
    public void SetDiaryTapAlarmIcon(int index, bool bAlarm)
    {
        if (arrayDiaryTap != null && arrayDiaryTap.Length > index)
        {
            if (arrayDiaryTap[index] != null)
                arrayDiaryTap[index].SettingAlarmIcon(bAlarm);
        }
    }

    public void InitDiaryTapAlarmIcon(int index)
    {
        if (arrayDiaryTap != null && arrayDiaryTap.Length > index)
        {
            if (arrayDiaryTap[index] == null)
                return;
            if (index == 1)
            {
                SetDiaryTapAlarmIcon(index, data.HousingGetCount > 0);
            }
            else if (index == 2)
            {
                SetDiaryTapAlarmIcon(index, data.StampAlarm);
            }
            else if (index == 3)
            {
                SetDiaryTapAlarmIcon(index, data.DicCostumeNewData.Count > 0);
            }
            else if (index == 4)
            {
                SetDiaryTapAlarmIcon(index, ManagerAIAnimal.HaveNewcomer());
            }
        }
    }

    public void SetttingDiaryTapNullData()
    {
        arrayDiaryTap = null;
    }

    public void SettingItemHousingProgressAlarmIcon(int index, UIItemDiaryProgress itemProgress)
    {
        int alarmCount = 0;
        data.DicHousingCateroryAlarmData.TryGetValue(index, out alarmCount);
        if (alarmCount <= 0)
        {
            itemProgress.InitCategoryAlarm(false);
        }
        else
        {
            string alarmText = alarmCount.ToString();
            if (alarmCount > 9)
                alarmText = "9+";
            itemProgress.InitCategoryAlarm(true, alarmText);
        }
    }

    public int GetItemHousingProgressAlarmCount(int index)
    {
        int alarmCount = 0;
        data.DicHousingCateroryAlarmData.TryGetValue(index, out alarmCount);
        return alarmCount;
    }

    #region QuestProgressPlayerPrefs
    public void CheckQuestProgressPlayerPrefs()
    {
        if (data.QuestPlayerPrefsKey == null)
            data.QuestPlayerPrefsKey = new Dictionary<int, int>();
        else
            data.QuestPlayerPrefsKey.Clear();
        
        if (data.DicQuestAlarmData == null)
        {
            data.DicQuestAlarmData = new Dictionary<int, QuestAlarmData>();
        }
        else
        {
            // 알람데이터 같은 경우, 스테이지를 바로 시작하거나 해서 아직 유저가 알람을 보지 않은 경우 알람을 다시 보여줘야하기 때문에 데이터 초기화 안함.
            // 데이터는 있는데 PlayerPrefs 에 키값이 없는 경우는 로그인 정보가 바뀌거나 해서 지워진 경우 이므로 데이터 초기화해줌.
            if (PlayerPrefs.HasKey("QuestProgress") == false)
            {
                RemoveAllQuestAlarmData();
            }
        }

        //playerPrefs에 저장된 퀘스트 키 정보를 읽어옴.
        this.ReadDiaryPlayerPrefsDictonary("QuestProgress", data.QuestPlayerPrefsKey);
    }

    public void SaveQuestProgressPlayerPrefs(bool isShowImmediately)
    {
        //유저 퀘스트 목록에 없어진 데이터들은 제거해줌.
        bool isRemoveData = IsRemoveQuestPlayerPrefsKey();

        // 현재 유저가 가지고 있는 퀘스트 중, 진행도가 달라졌거나 새로 추가된 퀘스트 key 리스트에 추가
        // '퀘스트 인덱스 : 키/ 퀘스트 진행도 : value 값'
        var enumerator_Q = ManagerData._instance._questGameData.GetEnumerator();
        while (enumerator_Q.MoveNext())
        {
            QuestGameData qData = enumerator_Q.Current.Value;
            if(qData.type == QuestType.chapter_Candy || qData.type == QuestType.chapter_Duck || qData.hidden != 0)
            {
                continue;
            }

            if(data.QuestPlayerPrefsKey.ContainsKey(qData.index))
            {
                int preValue = data.QuestPlayerPrefsKey[qData.index];

                //저장된 데이터의 값이 현재 퀘스트의 진행상황가 같다면 넘김.
                //다를경우, 현재 진행 상황을 덮어씌움.
                if (preValue == qData.prog1 || preValue >= qData.targetCount)
                {
                    continue;
                }
                data.QuestPlayerPrefsKey[qData.index] = qData.prog1;
                AddQuestAlarmData(qData.index, preValue, qData.prog1, false);
            }
            else
            {
                data.QuestPlayerPrefsKey.Add(qData.index, qData.prog1);
                AddQuestAlarmData(qData.index, 0, qData.prog1, true);
            }
        }

        //변경된 데이터 없는 경우 반환.
        if (isRemoveData == false && data.DicQuestAlarmData.Count <= 0 && PlayerPrefs.HasKey("QuestProgress"))
            return;

        //바뀐 데이터가 있다면, playerPref 값 다시 갱신.
        SaveDiaryPlayerPrefsDictionary("QuestProgress", data.QuestPlayerPrefsKey);

        if (data.DicQuestAlarmData.Count > 0)
        {
            data.DicQuestAlarmData = data.DicQuestAlarmData.OrderByDescending(i => i.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
            //바뀐 데이터 바로 보여주는 경우, 팝업 바로 띄움.
            if (isShowImmediately == true)
            {
                OpenQuestBanner();
            }
        }
    }

    private bool IsRemoveQuestPlayerPrefsKey()
    {
        bool isRemoveData = false;
        List<int> listRemoveKey = new List<int>();
        var enumerator_K = data.QuestPlayerPrefsKey.GetEnumerator();
        while (enumerator_K.MoveNext())
        {
            if (ManagerData._instance._questGameData.ContainsKey(enumerator_K.Current.Key) == false)
            {
                listRemoveKey.Add(enumerator_K.Current.Key);
            }
        }
        
        for (int i = 0; i < listRemoveKey.Count; i++)
        {
            if (data.QuestPlayerPrefsKey.ContainsKey(listRemoveKey[i]))
            {
                data.QuestPlayerPrefsKey.Remove(listRemoveKey[i]);
                isRemoveData = true;
            }
        }
        return isRemoveData;
    }

    private void RemoveQuestAlarmData()
    {
        List<int> listRemoveKey = new List<int>();
        var enumerator_K = data.DicQuestAlarmData.GetEnumerator();
        while (enumerator_K.MoveNext())
        {
            if (ManagerData._instance._questGameData.ContainsKey(enumerator_K.Current.Key) == false)
            {
                listRemoveKey.Add(enumerator_K.Current.Key);
            }
        }

        for (int i = 0; i < listRemoveKey.Count; i++)
        {
            if (data.DicQuestAlarmData.ContainsKey(listRemoveKey[i]))
            {
                data.DicQuestAlarmData.Remove(listRemoveKey[i]);
            }
        };
    }

    public void RemoveAllQuestAlarmData()
    {
        data.DicQuestAlarmData.Clear();
    }

    public void OpenQuestBanner()
    {
        //현재 유저 목록에 없는 알림 데이터 제거.
        RemoveQuestAlarmData();
        if (data.DicQuestAlarmData.Count > 0 && ManagerTutorial._instance == null)
        {
            StartCoroutine(WaitOpenQuestBanner());
        }
    }

    private IEnumerator WaitOpenQuestBanner()
    {
        yield return new WaitUntil(() => ManagerUI._instance._popupList.Count == 0);

        yield return new WaitWhile(() => ManagerCinemaBox.IsActive);

        yield return new WaitUntil(() => ManagerUI._instance.anchorBottomCenter.activeInHierarchy);

        ManagerUI._instance.OpenQuestBanner(data.DicQuestAlarmData);
        RemoveAllQuestAlarmData();
    }

    private void AddQuestAlarmData(int key, int preProgress, int curProgress, bool isNewQuest)
    {
        //키 없을 경우에는 알람 추가안함.
        if (!PlayerPrefs.HasKey("QuestProgress"))
            return;

        //이벤트 퀘스트 기간이 만료될 경우 등록된 알람 제거
        if (ManagerData._instance._questGameData[key].valueTime1 > 1 && ManagerData._instance._questGameData[key].valueTime1 < Global.GetTime())
        {
            if (data.DicQuestAlarmData.ContainsKey(key))
            {
                data.DicQuestAlarmData.Remove(key);
            }
            return;
        }

        //이전 playerPref 이 없으면 이거 안해야함.
        if (data.DicQuestAlarmData.ContainsKey(key))
        {
            if (data.DicQuestAlarmData[key].curProg1 != curProgress)
                data.DicQuestAlarmData[key].curProg1 = curProgress;
        }
        else
        {
            QuestAlarmData alarmData = new QuestAlarmData(preProgress, curProgress, isNewQuest);
            data.DicQuestAlarmData.Add(key, alarmData);
        }
    }

#endregion

    #region HosingPlayerPrefs
    public void CheckHousingNewIconPlayerPrefs()
    {
        //playerPrefs에 저장된 하우징 키 정보를 읽어옴.
        data.HousingPlayerPrefsKey = new List<string>();
        this.ReadDiaryPlayerPrefsList("Housing", data.HousingPlayerPrefsKey);

        if (data.DicProgressHousingNewData == null)
            data.DicProgressHousingNewData = new Dictionary<int, List<int>>();
        else
            data.DicProgressHousingNewData.Clear();

        //현재 하우징 키에 등록된 값이 없으면(전부 new Item이면), new 표시에서 제외시켜줌.
        if (data.HousingPlayerPrefsKey.Count == 0)
            return;
        
        int housingIdx = 0;
        if (data.ProgressHousingData.Count > 0)
            housingIdx = data.ProgressHousingData[0].housingIndex;

        // 현재 가지고 있는 목록들중에 새로 만들어진 모델들의 key 리스트에 추가. 
        // '하우징 인덱스 : 키/ 모델 인덱스 리스트 : value 값'.
        List<int> modelIndexList = new List<int>();
        for (int i = 0; i < data.ProgressHousingData.Count; i++)
        {
            //기존에 키를 가지고 있는 하우징인지 검사.
            string key = string.Format("{0}_{1}", data.ProgressHousingData[i].housingIndex, data.ProgressHousingData[i].modelIndex);
            if (CheckDiaryPlayerPrefsList(data.HousingPlayerPrefsKey, key) == false)
                continue;

            if ((housingIdx != data.ProgressHousingData[i].housingIndex))
            {
                if (modelIndexList.Count > 0)
                {
                    data.DicProgressHousingNewData.Add(housingIdx, modelIndexList);
                    if (!data.DicProgressHousingPreview.ContainsKey(housingIdx))
                        data.DicProgressHousingPreview.Add(housingIdx, true);
                }
                modelIndexList = new List<int>();
                housingIdx = data.ProgressHousingData[i].housingIndex;
            }
            modelIndexList.Add(data.ProgressHousingData[i].modelIndex);
        }
        //마지막 그룹 모델인덱스 추가.
        if(modelIndexList.Count > 0)
        {
            data.DicProgressHousingNewData.Add(housingIdx, modelIndexList);
            if (!data.DicProgressHousingPreview.ContainsKey(housingIdx))
                data.DicProgressHousingPreview.Add(housingIdx, true);
        }
    }

    public List<int> GetHousingNewIconPlayerPrefs(int housingIdx)
    {
        //키 저장이 안 된 상태라면, new 아이콘 안띄워줌.
        if (!PlayerPrefs.HasKey("Housing"))
            return null;
        if (data.DicProgressHousingNewData == null)
            return null;
        if (!data.DicProgressHousingNewData.ContainsKey(housingIdx))
            return null;

        List<int> newIconList = new List<int>();
        data.DicProgressHousingNewData.TryGetValue(housingIdx, out newIconList);
        return newIconList;
    }

    public bool CheckNewIconHousing(int housingIdx, int modelIdx)
    {
        List<int> modelList = new List<int>();
        modelList = GetHousingNewIconPlayerPrefs(housingIdx);
        if (modelList != null)
        {
            for (int i = 0; i < modelList.Count; i++)
            {
                if (modelList[i] == modelIdx)
                    return true;
            }
        }
        return false;
    }
   
    public void CheckLimitHousingProgressData()
    {
        //기간 한정 데코 중 기간만료돼서 갱신되는 데이터 있는 지 검사.
        bool bRefreshData = false;
        for (int i = 0; i < data.ProgressHousingData.Count; i++)
        {
            if (data.ProgressHousingData[i].expire_ts > 0)
            {
                if (data.ProgressHousingData[i].expire_ts < Global.GetTime())
                {
                    bRefreshData = true;
                    break;
                }
            }
        }

        //갱신되는 데이터가 있으면 데이터갱신, 알람 갱신.
        if (bRefreshData == true)
        {
            UpdateProgressHousingData();
        }
    }

    public void SaveHousingNewIconPlayerPrefs()
    {
        //playerPrefs에 저장된 하우징 키 정보를 읽어옴.
        if (data.HousingPlayerPrefsKey == null)
        {
            data.HousingPlayerPrefsKey = new List<string>();
            this.ReadDiaryPlayerPrefsList("Housing", data.HousingPlayerPrefsKey);
        }

        //새로운 데이터가 있을 떄에만 갱신.
        List<string> housingKeyList = new List<string>();
        bool bNew = false;
        for (int i = 0; i < data.ProgressHousingData.Count; i++)
        {
            string key = string.Format("{0}_{1}", data.ProgressHousingData[i].housingIndex, data.ProgressHousingData[i].modelIndex);
            if(bNew == false && CheckDiaryPlayerPrefsList(data.HousingPlayerPrefsKey, key) == true)
                bNew = true;
            housingKeyList.Add(key);
        }
        if (bNew == false && PlayerPrefs.HasKey("Housing"))
            return;

        //새로운 아이템이 있을 때, 키 추가해주고 확인하지않은 아이템이 들어있던 리스트는 초기화해줌.
        SaveDiaryPlayerPrefsList("Housing", housingKeyList);
        data.DicProgressHousingNewData.Clear();
        CheckHousingData();
    }
    #endregion

    #region CostumePlayerPrefs
    public void CheckCostumeNewIconPlayerPrefs()
    {
        //playerPrefs에 저장된 하우징 키 정보를 읽어옴.
        data.CostumePlayerPrefsKey = new List<string>();
        this.ReadDiaryPlayerPrefsList("Costume", data.CostumePlayerPrefsKey);

        data.DicCostumeNewData = new Dictionary<int, List<int>>();

        int charIdx = 0;
        if (data.CostumeData.Count > 0)
            charIdx = data.CostumeData[0].char_id;

        List<int> costumeIndexList = new List<int>();

        // 현재 가지고 있는 목록들중에 새로 만들어진 코스튬의 key 리스트에 추가. 
        // '캐릭터 인덱스 : 키/ 코스튬 인덱스 리스트 : value 값'.
        for (int i = 0; i < data.CostumeData.Count; i++)
        {
            //기존에 키를 가지고 있는 코스튬인지 검사.
            string key = string.Format("{0}_{1}", data.CostumeData[i].char_id, data.CostumeData[i].idx);
            if (data.CostumeData[i].idx == 0 || CheckDiaryPlayerPrefsList(data.CostumePlayerPrefsKey, key) == false)
                continue;

            if ((charIdx != data.CostumeData[i].char_id))
            {
                if (costumeIndexList.Count > 0)
                {
                    data.DicCostumeNewData.Add(charIdx, costumeIndexList);
                }
                costumeIndexList = new List<int>();
                charIdx = data.CostumeData[i].char_id;
            }
            costumeIndexList.Add(data.CostumeData[i].idx);
        }
        //마지막 그룹 모델인덱스 추가.
        if (costumeIndexList.Count > 0)
        {
            data.DicCostumeNewData.Add(charIdx, costumeIndexList);
        }
    }

    public List<int> GetCostumeNewIconPlayerPrefs(int charIdx)
    {
        //키 저장이 안 된 상태라면, new 아이콘 안띄워줌.
        if (!PlayerPrefs.HasKey("Costume"))
            return null;
        if (data.DicCostumeNewData == null)
            return null;
        if (!data.DicCostumeNewData.ContainsKey(charIdx))
            return null;

        List<int> newIconList = new List<int>();
        data.DicCostumeNewData.TryGetValue(charIdx, out newIconList);
        return newIconList;
    }

    public void SaveCostumeNewIconPlayerPrefs()
    {
        //playerPrefs에 저장된 하우징 키 정보를 읽어옴.
        if (data.CostumePlayerPrefsKey == null)
        {
            data.CostumePlayerPrefsKey = new List<string>();
            this.ReadDiaryPlayerPrefsList("Costume", data.CostumePlayerPrefsKey);
        }

        //새로운 데이터가 있을 떄에만 갱신.
        List<string> costumeKeyList = new List<string>();
        bool bNew = false;
        for (int i = 0; i < data.CostumeData.Count; i++)
        {
            string key = string.Format("{0}_{1}", data.CostumeData[i].char_id, data.CostumeData[i].idx);
            if (bNew == false && CheckDiaryPlayerPrefsList(data.HousingPlayerPrefsKey, key) == true)
                bNew = true;
            costumeKeyList.Add(key);
        }
        if (bNew == false && PlayerPrefs.HasKey("Costume"))
            return;

        //새로운 아이템이 있을 때, 키 추가해주고 확인하지않은 아이템이 들어있던 리스트는 초기화해줌.
        SaveDiaryPlayerPrefsList("Costume", costumeKeyList);
        data.DicCostumeNewData.Clear();
        SetBtnDiaryAlarm();
    }

    public bool IsEuquipCostume(CdnCostume costume)
    {
        var userCostume = ServerRepos.UserCostumes.Find((userCostumes) => userCostumes.is_equip == 1);
        int useCostumeIdx = 0;

        if (userCostume != null)
            useCostumeIdx = ServerRepos.UserCostumes.Find((userCostumes) => userCostumes.is_equip == 1).GetCostumeIndex();

        if (useCostumeIdx == costume.costume_id)
            return true;
        else
            return false;
    }

    public bool IsNewCostume(CdnCostume costume)
    {
        string key = string.Format("{0}_{1}", costume.char_id, costume.idx);
        List<string> PrefsKey = new List<string>();

        ReadDiaryPlayerPrefsList("Costume", PrefsKey);

        return CheckDiaryPlayerPrefsList(PrefsKey, key);
    }

    #endregion

    #region PlayerPrefs 관련
    //List 타입으로 관리
    private void ReadDiaryPlayerPrefsList(string prefsKey, List<string> jsonData)
    {
        if (PlayerPrefs.HasKey(prefsKey))
        {
            string jsonStr = PlayerPrefs.GetString(prefsKey);
            HousingIndexAndCostumeJsonData readData = JsonUtility.FromJson<HousingIndexAndCostumeJsonData>(jsonStr);
            readData.ConvertJsonData(readData, jsonData);
        }
    }

    private bool CheckDiaryPlayerPrefsList(List<string> keyList, string key)
    {
        for (int i = 0; i < keyList.Count; i++)
        {
            if (keyList[i] == key)
                return false;
        }
        return true;
    }

    private void SaveDiaryPlayerPrefsList(string prefsKey, List<string> keyList)
    {
        HousingIndexAndCostumeJsonData saveData = new HousingIndexAndCostumeJsonData(keyList);
        string jsonStr = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(prefsKey, jsonStr);
    }

    //Dictionary 타입으로 관리
    private void ReadDiaryPlayerPrefsDictonary<TKey, TValue>(string prefsKey, Dictionary<TKey, TValue> jsonData)
    {
        if (PlayerPrefs.HasKey(prefsKey))
        {
            string jsonStr = PlayerPrefs.GetString(prefsKey);
            DictionaryJsonData<TKey, TValue> readData = JsonUtility.FromJson<DictionaryJsonData<TKey, TValue>>(jsonStr);
            readData.ConvertJsonData(readData, jsonData);
        }
    }

    private void SaveDiaryPlayerPrefsDictionary<TKey, TValue>(string prefsKey, Dictionary<TKey, TValue> keyList)
    {
        DictionaryJsonData<TKey, TValue> saveData = new DictionaryJsonData<TKey, TValue>(keyList);
        string jsonStr = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(prefsKey, jsonStr);
    }
    #endregion
}
