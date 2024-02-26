using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerHousing : MonoBehaviour {
    static ManagerHousing instance = null;

    public SortedDictionary<int, IHousingTrigger> _objectHousing = new SortedDictionary<int, IHousingTrigger>();

    public List<HousingUserData> _housingSelectData = new List<HousingUserData>();
    public Dictionary<string, PlusHousingModelData> _housingGameData = new Dictionary<string, PlusHousingModelData>();
    Dictionary<int, HousingInfo> housingInfo = new Dictionary<int, HousingInfo>();

    [System.Serializable]
    internal class HousingData
    {
        int modelIdx;

        PlusHousingModelDataType getMethod; // 획득경로

        List<GameObject> objectList = new List<GameObject>();
    }

    public class HousingInfo
    {
        public int landIndex;
        public int firstOpenMission = 999999;
        public Dictionary<int, int> openMission = new Dictionary<int, int>();    // modelIndex, missionIndex
    }
    
    private void Awake()
    {
        instance = this;
    }
    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    static public void OnReboot()
    {
        if( instance != null )
        {
            instance._housingSelectData.Clear();
            instance._housingGameData.Clear();
            instance._objectHousing.Clear();
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    static public void OnLobbyDestroyed()
    {
        if( instance != null )
            instance._objectHousing.Clear();

    }

    static public int GetSelectedHousingModelIdx(int housingIdx)
    {
        if (housingIdx > 0)
        {
            var selectedHousing = instance._housingSelectData.Find(x => x.index == housingIdx);
            if (selectedHousing != null)
                return selectedHousing.selectModel;
        }
        return 0;
    }

    static public bool HaveSelected(int housingIdx)
    {
        return instance._housingSelectData.Exists(x => x.index == housingIdx);
    }

    static public void RegisterHousing(IHousingTrigger housing)
    {
        if (instance._objectHousing.ContainsKey(housing.GetHousingIdx()))
            instance._objectHousing.Remove(housing.GetHousingIdx());

        instance._objectHousing.Add(housing.GetHousingIdx(), housing);
    }

    static public bool IsHousingActivated(int housingIdx)
    {
        if( instance._objectHousing.ContainsKey(housingIdx) )
        {
            return instance._objectHousing[housingIdx].IsHousingActivated();
        }

        return false;
    }

    static public bool IsMovableHousing(int housingIdx)
    {
        if (instance._objectHousing.ContainsKey(housingIdx))
        {
            return instance._objectHousing[housingIdx].IsMovableHousing();
        }

        return false;
    }

    static public PlusHousingModelData GetHousingModel(int housingIndex, int modelIndex)
    {
        string hKey = housingIndex + "_" + modelIndex;
        PlusHousingModelData ret = null;

        instance._housingGameData.TryGetValue(hKey, out ret);
        return ret;
    }

    static public List<PlusHousingModelData> GetHousingModels(int housingIndex)
    {
        List<PlusHousingModelData> ret = new List<PlusHousingModelData>();
        foreach (var item in instance._housingGameData)
        {
            if (item.Value.housingIndex == housingIndex)
                ret.Add(item.Value);

        }
        return ret;
    }

    static public Dictionary<string, PlusHousingModelData>.ValueCollection GetHousingList()
    {
        return instance._housingGameData.Values;
    }

    static public List<int> GetHousingIndexList()
    {
        List<int> listHousingIndex = new List<int>();

        foreach(var housingItem in instance._housingGameData)
        {
            if (listHousingIndex.Contains(housingItem.Value.housingIndex) == false)
            {
                listHousingIndex.Add(housingItem.Value.housingIndex);
            }
        }

        listHousingIndex.Sort();

        return listHousingIndex;
    }

    static public void GetHousingProgress(List<PlusHousingModelData> inProgress, List<PlusHousingModelData> completed)
    {
        var enumerator = instance._housingGameData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            PlusHousingModelData hData = enumerator.Current.Value;
            if (hData.type == PlusHousingModelDataType.byMission)
            {
                //continue;
            }

            if (hData.active == 0)
            {
                var missionIndex = hData.openMission;
                if (!ManagerData._instance._missionData.ContainsKey(missionIndex))
                {
                    continue;
                }

                if (hData.type == PlusHousingModelDataType.byProgress)
                {
                    // 일반 하우징의 경우, 해당 하우징의 미션 까지 완료되지 않은 상태이면 표시 안 함.
                    if (hData.expire_ts == 0)
                    {
                        if (ManagerData._instance._missionData[missionIndex].state != TypeMissionState.Clear)
                            continue;
                    }
                    else
                    {
                        // 이벤트 하우징의 경우, 일정 미션 이상을 진행한 상태이고 시간이 다 끝나지 않은 상태면 추가.
                        // 이벤트 하우징은 해당 하우징의 미션 까지 완료되지 않은 상태라도 표시하고 get 하는 부분에서 경고팝업만 띄움.

                        if (GameData.User.missionCnt > ManagerLobby._missionThreshold_eventHousingOpen
                            && hData.expire_ts >= Global.GetTime())
                        {
                            
                        }
                        else
                        {
                            continue;
                        }
                    }
                    inProgress.Add(hData);
                }

                if(hData.type == PlusHousingModelDataType.byMission &&
                    ManagerData._instance._missionData[missionIndex].state == TypeMissionState.Clear)
                {
                    completed.Add(hData);
                }
            }
            else
            {
                completed.Add(hData);
            }
        }
    }

    static public bool IsHaveEventHousingList(List<PlusHousingModelData> listHousing)
    {
        for(int i = 0; i < listHousing.Count; i++)
        {
            if (listHousing[i].expire_ts == 0 || listHousing[i].expire_ts < Global.GetTime())
                continue;
            else
                return true;
        }

        return false;
    }

    static public int GetUnfinishedEventItemCount()
    {
        int eventItemCount = 0;
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventHousingOpen)
            return eventItemCount;

        var enumerator = instance._housingGameData.GetEnumerator();
        while (enumerator.MoveNext())
        {
            PlusHousingModelData hData = enumerator.Current.Value;
            if (hData.type != PlusHousingModelDataType.byProgress)
                continue;

            if (hData.active == 0 && hData.openMission != 0)
            {
                var missionIndex = hData.openMission;
                if (!ManagerData._instance._missionData.ContainsKey(missionIndex))
                {
                    continue;
                }

                if (hData.type == PlusHousingModelDataType.byProgress && hData.expire_ts >= Global.GetTime())
                {
                    eventItemCount++;
                }
            }
        }

        return eventItemCount;
    }

    static public void SyncUserHousingsActiveState()
    {
        foreach (var item in ServerRepos.UserHousingItems)
        {
            PlusHousingModelData data = null;
            if (instance._housingGameData.TryGetValue(item.index + "_" + item.modelIndex, out data))
            {
                data.active = item.active;
            }
        }
    }

    static public void  SyncUserSelectedHousing()
    {
        foreach (var item in ServerRepos.UserHousingSelected)
        {
            int findIndex = -1;
            for (int i = 0; i < instance._housingSelectData.Count; i++)
            {
                if (instance._housingSelectData[i].index == item.index)
                    findIndex = i;
            }

            if (findIndex == -1)
            {
                HousingUserData data = new HousingUserData();
                data.index = item.index;
                data.selectModel = item.selectModel;
                instance._housingSelectData.Add(data);
            }
            else
            {
                instance._housingSelectData[findIndex].index = item.index;
                instance._housingSelectData[findIndex].selectModel = item.selectModel;
            }
        }
    }

    static public void SyncUserHousing()
    {
        instance._housingGameData.Clear();

        foreach (var item1 in ServerContents.Housings)
        {
            foreach (var i in item1.Value)
            {
                PlusHousingModelData housing = new PlusHousingModelData();
                housing.housingIndex = i.Value.housingIndex;
                housing.modelIndex = i.Value.modelIndex;
                housing.expire_ts = i.Value.expireTs;
                housing.costCoin = i.Value.costCoin;
                housing.costJewel = i.Value.costJewel;
                housing.type = (PlusHousingModelDataType)i.Value.housingOpenType;
                housing.material = new List<HousingMaterialData>();

                for (var idx = 0; idx < i.Value.materials.Count; idx++)
                {
                    HousingMaterialData material = new HousingMaterialData();
                    material._index = i.Value.materials[idx].index;
                    material._count = i.Value.materials[idx].count;
                    housing.material.Add(material);
                }

                housing.openMission = GetHousingOpenMission(i.Value.housingIndex, i.Value.modelIndex);

                instance._housingGameData.Add(item1.Key + "_" + i.Key, housing);
            }

        }
    }

    

    static public void BuildInfo()
    {
        var landMap = ServerContents.Day.CreateAreaToLandMap();

        Dictionary<int, HousingInfo> hi = new Dictionary<int, HousingInfo>();
        
        foreach (var housingProgress in ServerContents.HousingProgresses)
        {
            var missionIdx = housingProgress.Key;
            foreach (var progress in housingProgress.Value)
            {
                if(!hi.ContainsKey(progress.spawnIndex) )
                    hi[progress.spawnIndex] = new HousingInfo();
                
                hi[progress.spawnIndex].openMission[progress.spawnModel] = missionIdx;
                if( hi[progress.spawnIndex].firstOpenMission > missionIdx )
                {
                    hi[progress.spawnIndex].firstOpenMission = missionIdx;
                }
                if( ServerContents.Housings.ContainsKey(progress.spawnIndex))
                {
                    hi[progress.spawnIndex].landIndex = ServerContents.Housings[progress.spawnIndex][progress.spawnModel].landIndex;
                }
            }
        }

        

        instance.housingInfo = hi;
    }

    static public int GetHousingOpenMission(int housingIdx, int modelIndex = 0) // modelIndex = 0 인 경우, 하우징 위치 자체가 최초로 열리는 미션번호를 리턴함
    {
        HousingInfo hInfo = null;
        if( instance.housingInfo.TryGetValue(housingIdx, out hInfo))
        {
            if( modelIndex == 0 )
                return hInfo.firstOpenMission;
            else 
                return hInfo.openMission.ContainsKey(modelIndex) ? hInfo.openMission[modelIndex] : 0;
        }
        return 0;
    }

    static public int GetHousingLandIndex(int housingIdx, int modelIndex = 0)
    {
        HousingInfo hInfo = null;
        if (instance.housingInfo.TryGetValue(housingIdx, out hInfo))
        {
            return hInfo.landIndex;
        }
        return 0;
    }

    static public void OnBuyNewHousing(int housingIdx)
    {
        instance._objectHousing[housingIdx].BuyNewHousing();
    }

    static public void RequestLoadSelectedModels(List<ManagerAssetLoader.BundleRequest> bundleLoadList, string loadFailedBundleString )
    {
        foreach (var item in instance._housingSelectData)
        {
            if (item.selectModel > 0)
            {
                var itemKey = item.index + "_" + item.selectModel;

                if( instance._housingGameData.ContainsKey(itemKey) == false)
                {
                    Debug.LogWarning("housing not found in _housingGameData : " + itemKey);
                    loadFailedBundleString += string.Format("no housing in list: {0}\n", itemKey);
                        continue;
                }

                PlusHousingModelData model = instance._housingGameData[itemKey];
                
                if (model.type == PlusHousingModelDataType.byMission)
                    continue;

                if (model.active > 0)
                {
                    string name = "h_" + item.index + "_" + item.selectModel;
                    if (!ManagerLobby._assetBankHousing.ContainsKey(name))
                    {
                        ManagerAssetLoader.BundleRequest bundleReq = new ManagerAssetLoader.BundleRequest()
                        {
                            uri = name,
                            successCallback = (assetBundle) => {
                                GameObject obj = assetBundle.LoadAsset<GameObject>(name.Replace("h_", ""));
                                ManagerLobby._assetBankHousing.Add(name, obj);
                            },
                            failCallback = (r) =>
                            {
                                Debug.LogWarning("Download Asset Bundle(Chapter) Error");
                                loadFailedBundleString += name + "\n";
                            }
                        };
                        bundleLoadList.Add(bundleReq);
                    }
                }
            }
        }

    }

    internal static IHousingTrigger FindHousing(int housingIdx)
    {
        if (instance._objectHousing.ContainsKey(housingIdx))
            return instance._objectHousing[housingIdx] as IHousingTrigger;
        return null;
    }

    static public void MakeHousingInstance(int housingIdx, int modelIdx, System.Action completeCallback)
    {
        // 데이타를 읽었으면 그냥 선택, 읽은적이 없다면 로딩 후 선택
        string name = "h_" + housingIdx + "_" + modelIdx;

        if (ManagerLobby._assetBankHousing.ContainsKey(name))
        {
#if  UNITY_EDITOR
            string path = "Assets/5_OutResource/housing/housing_" + housingIdx + "_" + modelIdx + "/" + housingIdx + "_" + modelIdx + ".prefab";

            GameObject Obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            instance._objectHousing[housingIdx].MakeObject(modelIdx, Obj);

#else
            GameObject obj = ManagerLobby._assetBankHousing[name];
            instance._objectHousing[housingIdx].MakeObject(modelIdx, obj);
#endif
            completeCallback();
        }
        else
            instance.StartCoroutine(instance.CoLoadModel(housingIdx, modelIdx, completeCallback));
    }

    internal static void SelectModel(int housingIdx, int modelIndex)
    {
        var housing = instance._objectHousing[housingIdx];
        housing.SelectModel(modelIndex);

    }

    internal static void MakeHousingChat(int housingIdx, int modelIdx)
    {
        var housing = instance._objectHousing[housingIdx];
        housing.MakeHousingChat(modelIdx);
    }

    internal static Vector3 GetHousingFocusPosition(int housingIdx)
    {
        var housing = instance._objectHousing[housingIdx];
        return housing.GetHousingFocusPosition();
    }

    internal static bool IsVaildHousing(int housingIdx)
    {
        return ServerContents.Housings.ContainsKey(housingIdx) && ServerContents.Housings[housingIdx].ContainsKey(1);
    }

    IEnumerator CoLoadModel(int housingIdx, int modelIdx, System.Action completeCallback)
    {
        if (Global.LoadFromInternal)
        {

#if UNITY_EDITOR
            string path = "Assets/5_OutResource/housing/housing_" + housingIdx + "_" + modelIdx + "/" + housingIdx + "_" + modelIdx + ".prefab";
            GameObject Obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            instance._objectHousing[housingIdx].MakeObject(modelIdx, Obj);
#endif
        }
        else
        {
            NetworkLoading.MakeNetworkLoading(0.5f);
            string name = "h_" + housingIdx + "_" + modelIdx;

            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(name);
            while (e.MoveNext())
                yield return e.Current;
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    GameObject obj = assetBundle.LoadAsset<GameObject>(name.Replace("h_", ""));
                    yield return null;

                    ManagerLobby._assetBankHousing.Add(name, obj);
                    instance._objectHousing[housingIdx].MakeObject(modelIdx, obj);
                }
            }

            NetworkLoading.EndNetworkLoading();
        }


        completeCallback();
        yield return null;
    }
}
