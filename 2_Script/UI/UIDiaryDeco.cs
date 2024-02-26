using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class DecoLandIndexComparer : IComparer<DecoItemData>
{
    // -1 : a를 앞으로, 1: a를 뒤로, 0: 그대로.
    public int Compare(DecoItemData a, DecoItemData b)
    {
        //랜드 인덱스 정렬
        if (a.landIndex < b.landIndex)
            return -1;
        else if (a.landIndex > b.landIndex)
            return 1;
        else
        {
            if (a.housingIndex < b.housingIndex)
                return -1;
            else if (a.housingIndex > b.housingIndex)
                return 1;
            else
                return 0;
        }
    }
}

public class DecoItemData
{
    //메인 스크롤에서 만 사용 될 데이터
    public int housingIndex;
    public int modelIndex;
    public int setItemIndex;
    public int landIndex;

    //서브 스크롤에서 세팅 될 데코 리스트
    public List<PlusHousingModelData> listDecoItem = new List<PlusHousingModelData>();


    public void SetDecoItem(int idxHousing, int idxModel = 0)
    {
        housingIndex = idxHousing;
        modelIndex = idxModel;
    }
}

public class UIDiaryDeco : UIDiaryBase
{
    public static UIDiaryDeco _instance = null;
    [SerializeField] private UIPanel[] uiMainPanel;
    [SerializeField] private UIPanel[] uiSubPanel;
    [SerializeField] private UIPanel uiFrontPanel;
    [SerializeField] private UIPanel uiExpansionPanel;

    public enum SubScrollState
    {
        Idle,
        Expansion
    }

    public enum DecoTapType
    {
        Production,
        Install,
        SetItem,
        TapCount
    }
    //하우징 탭 연 시간 저장.
    private long openPopupTime = 0;

    [Header("ObjectLink")]
    [SerializeField] private UILabel materialCount;
    [SerializeField] private GameObject[] gridScrollRoot;
    [SerializeField] private UIReuseGrid_Generic[] subGridScrollRoot;
    [SerializeField] private UIScrollView[] mainScrollView;
    [SerializeField] private UILabel labelDecoItemEmpty;
    [SerializeField] private GameObject objMainItemRoot;

    [Header("DecoSubScroll")]
    [SerializeField] private UIPanel decoSubPanel;
    [SerializeField] private GameObject objDecoName;
    [SerializeField] private UILabel labelDecoName;
    [SerializeField] private float subScrollValue;
    [SerializeField] private float subScrollSpeed = 10f;
    [SerializeField] private float mainItemMoveSpeed = 15f;

    private float decoItemMinSize;
    private SortedDictionary<DecoTapType, List<int>> landCount = new SortedDictionary<DecoTapType, List<int>>();
    
    [Header("ParentChangeLink")]
    [SerializeField] private UIItemDeco_CopyDecoItem copyDecoItem;

    //Static
    public static SubScrollState state = SubScrollState.Idle;
    public static DecoTapType tapType = DecoTapType.Production;
    public static UIItemDeco selectDecoItem = null;
    public static GameObject changeParrentArray = null;

    //Deco List
    private List<DecoItemData> dataProduction = new List<DecoItemData>();
    private List<DecoItemData> dataInstall = new List<DecoItemData>();
    private List<DecoItemData> dataSetItem = new List<DecoItemData>(); //세트 아이템 리스트로 변경 필요

    HousingComparer housingComparer = new HousingComparer();
    CompleteHousingComparer completeHousingComparer = new CompleteHousingComparer();
    DecoLandIndexComparer landIndexComparer = new DecoLandIndexComparer();

    public int FrontPanelDepth
    {
        get
        {
            return uiFrontPanel.depth;
        }
    }

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
    }

    private void Start()
    {
        decoItemMinSize = decoSubPanel.GetViewSize().y;
        openPopupTime = Global.GetTime();

        InitStaticData();
        InitProgressItem();
        InitMaterialCount();
        EmptyDecoItem();

        SetScrollData(tapType);
    }

    #region Init
    //depth 설정
    public void InitPanel(int depth)
    {
        //메인 스크롤 depth
        for (int i = 0; i < uiMainPanel.Length; i++)
        {
            uiMainPanel[i].depth = depth;
        }

        //확장되는 패널 depth
        uiExpansionPanel.depth = depth + 1;

        //서브 스크롤 depth
        for (int i = 0; i < uiSubPanel.Length; i++)
        {
            uiSubPanel[i].depth = depth + 2;
        }

        //Front 패널 depth
        uiFrontPanel.depth = depth + 3;
    }

    private void InitStaticData()
    {
        state = SubScrollState.Idle;
        tapType = DecoTapType.Production;
        selectDecoItem = null;
        changeParrentArray = null;
    }
    private void InitProgressItem()
    {
        //기간한정 데코 검사.
        UIDiaryController._instance.CheckLimitHousingProgressData() ;

        UIPopupDiary._instance.SettingEventTap(1, ManagerHousing.GetUnfinishedEventItemCount() > 0);

        //제작 설치 세트 탭의 아이템 세팅
        InitDicDecoItem();
    }

    private void InitMaterialCount()
    {
        List<ServerUserMaterial> materialDataList = ServerRepos.UserMaterials;
        int dataCount = materialDataList.Count;
        int count = 0;
        for (int i = 0; i < dataCount; i++)
        {
            if (ManagerData._instance._materialSpawnProgress.ContainsKey(materialDataList[i].index) == true)
            {
                //기간이 지난 한정 재료의 수는 카운트 하지않음.
                if (ManagerData._instance._materialSpawnProgress[materialDataList[i].index] != 0
                    && ManagerData._instance._materialSpawnProgress[materialDataList[i].index] < openPopupTime)
                    continue;
            }
            count += ServerRepos.UserMaterials[i].count;
        }
        materialCount.text = count.ToString();
    }

    void InitDicDecoItem()
    {
        var listHousingIndex = ManagerHousing.GetHousingIndexList();
        var dicHousingItem = ManagerHousing.GetHousingList();

        List<PlusHousingModelData> listProduction = new List<PlusHousingModelData>();
        List<PlusHousingModelData> listInstall = new List<PlusHousingModelData>();

        ManagerHousing.GetHousingProgress(listProduction, listInstall);


        //제작 하우징 아이템 List 세팅
        for (int i = 0; i < listHousingIndex.Count; i++)
        {
            List<PlusHousingModelData> listDecoData = new List<PlusHousingModelData>();

            DecoItemData AddData = new DecoItemData();

            for (int idx = 0; idx < listProduction.Count; idx++)
            {
                if (listHousingIndex[i] == listProduction[idx].housingIndex)
                {
                    listDecoData.Add(listProduction[idx]);
                }
            }

            if (listDecoData.Count > 0)
            {
                AddData.SetDecoItem(listHousingIndex[i]);
                AddData.landIndex = ManagerHousing.GetHousingLandIndex(listDecoData[0].housingIndex);

                listDecoData.Sort(housingComparer);

                AddData.listDecoItem.AddRange(listDecoData);

                dataProduction.Add(AddData);
            }
        }

        //설치 하우징 아이템 List 세팅
        for (int i = 0; i < listHousingIndex.Count; i++)
        {
            List<PlusHousingModelData> listDecoData = new List<PlusHousingModelData>();

            DecoItemData AddData = new DecoItemData();

            for (int idx = 0; idx < listInstall.Count; idx++)
            {
                if (listHousingIndex[i] == listInstall[idx].housingIndex)
                {
                    listDecoData.Add(listInstall[idx]);
                }
            }

            if (listDecoData.Count > 0)
            {
                AddData.SetDecoItem(listHousingIndex[i], ManagerHousing.GetSelectedHousingModelIdx(listHousingIndex[i]));
                AddData.landIndex = ManagerHousing.GetHousingLandIndex(listDecoData[0].housingIndex);

                listDecoData.Sort(completeHousingComparer);

                AddData.listDecoItem.AddRange(listDecoData);

                dataInstall.Add(AddData);
            }
        }

        //세트 하우징 아이템 List 세팅
        for (int setIdx = 1; setIdx < HousingSetInfoUtility.GetSetIndexList().Count + 1; setIdx++)
        {
            List<PlusHousingModelData> listDecoData = new List<PlusHousingModelData>();

            DecoItemData AddData = new DecoItemData();

            foreach (var housingItem in dicHousingItem)
            {
                if (HousingSetInfoUtility.TryGetSetIndex(housingItem.housingIndex, housingItem.modelIndex, out int[] setIndexArray))
                {
                    for (int i = 0; i < setIndexArray.Length; i++)
                    {
                        if (setIdx == setIndexArray[i])
                        {
                            listDecoData.Add(housingItem);
                        }
                    }
                }
            }

            if (listDecoData.Count > 0 && listDecoData.Exists((deco) => deco.active == 1))
            {
                AddData.setItemIndex = setIdx;
                AddData.landIndex = ManagerHousing.GetHousingLandIndex(listDecoData[0].housingIndex);
                AddData.listDecoItem.AddRange(listDecoData);

                dataSetItem.Add(AddData);
            }
        }
    }
    
    #endregion

    void SetScrollData(DecoTapType tapType)
    {
        var LandRoot = GetLandDecoItem(GetDecoTapDictionary(tapType));
        float itemPosY = 0;

        int index = 0;

        foreach (var item in LandRoot)
        {
            if (item.Key > 99 && ServerRepos.User.missionCnt < 7) continue;

            UIItemDeco_MainScroll_ItemRoot itemRoot
                = NGUITools.AddChild(gridScrollRoot[(int)tapType], objMainItemRoot).GetComponent<UIItemDeco_MainScroll_ItemRoot>();

            itemRoot.UpdateData(item.Key, item.Value, index);
            itemRoot.SetPosition(itemPosY);

            itemPosY = itemPosY + GetNextItemPosition(item.Value.Count);

            index++;

            if (landCount.ContainsKey(tapType))
                landCount[tapType].Add(item.Key);
            else
            {
                landCount.Add(tapType, new List<int>());
                landCount[tapType].Add(item.Key);
            }
                
        }
    }

    private const float landNameHight = 60;
    private const float interval_yPos = 15;
    private float GetNextItemPosition(int decoListCount)
    {
        float yPos = landNameHight + /*간격*/interval_yPos + (160 * (((decoListCount - 1) / 4) + 1));

        return yPos;
    }

    SortedDictionary<int, List<DecoItemData>> GetLandDecoItem(List<DecoItemData> allDecoItemData)
    {
        SortedDictionary<int, List<DecoItemData>> test = new SortedDictionary<int, List<DecoItemData>>();
        int landIndex = 0;

        allDecoItemData.Sort(landIndexComparer);

        for (int i = 0; i < allDecoItemData.Count; i++)
        {
            if (landIndex != allDecoItemData[i].landIndex) landIndex = allDecoItemData[i].landIndex;

            if(test.ContainsKey(landIndex))
            {
                test[landIndex].Add(allDecoItemData[i]);
            }
            else
            {
                test.Add(landIndex, new List<DecoItemData>() { allDecoItemData[i] });
            }
        }

        return test;
    }

    //메인 스크롤에 나올 아이템이 없으면 나올 문구 설정
    void EmptyDecoItem()
    {
        if (GetDecoTapDictionary(tapType).Count == 0)
        {
            string emptyDecoItem = "";

            switch (tapType)
            {
                case DecoTapType.Production:
                    emptyDecoItem = Global._instance.GetString("p_e_5");
                    break;
                case DecoTapType.Install:
                    emptyDecoItem = Global._instance.GetString("p_e_6");
                    break;
                case DecoTapType.SetItem:
                    emptyDecoItem = Global._instance.GetString("p_e_14");
                    break;
            }

            labelDecoItemEmpty.gameObject.SetActive(true);
            mainScrollView[(int)tapType].gameObject.SetActive(false);

            labelDecoItemEmpty.text = emptyDecoItem;
        }
        else
        {
            labelDecoItemEmpty.gameObject.SetActive(false);
            mainScrollView[(int)tapType].gameObject.SetActive(true);
        }
    }

    public void OnClickEvent(UIItemDeco selectItem, bool IsOtherBtnClick)
    {

        switch(state)
        {
            case SubScrollState.Idle: //서브 스크롤이 접히는 상태
                {
                    //뎁스를 원래 자리로 (같은 아이템 클릭)
                    ChangParent(false);

                    //선택된 버튼 해제
                    selectDecoItem = null;

                    //서브 스크롤 접는 기능
                    StartCoroutine(CoDecoItemMenuTransforming(true));
                    mainScrollView[(int)tapType].enabled = true;
                    mainScrollView[(int)tapType].RestrictWithinBounds(false, false, true);
                }
                break;
            case SubScrollState.Expansion:
                {
                    if(IsOtherBtnClick) //서브 스크롤이 펼쳐져 있는 상태에서 다른 아이템을 클릭 할 때
                    {
                        //데이터 세팅
                        selectDecoItem = selectItem;
                        ChangParent(true);
                        DataSetting(selectItem.decoItem.listDecoItem);
                        return;
                    }
                    else //서브 스크롤이 펼쳐지는 상태
                    {
                        //데이터 세팅
                        selectDecoItem = selectItem;
                        DataSetting(selectItem.decoItem.listDecoItem);

                        changeParrentArray = selectItem.GetComponentInParent<UIGrid>().gameObject;

                        //서브 스크롤 펼치는 기능
                        StartCoroutine(CoDecoArrayMove(selectItem));
                        StartCoroutine(CoDecoItemMenuTransforming(false));
                    }
                }
                break;
        }
    }

    public void Refresh()
    {
        InitMaterialCount();
        
        if (selectDecoItem != null)
        {
            OnClickEvent(selectDecoItem, true);
        }
    }

    public void ChangParent(bool IsParent)
    {
        copyDecoItem.InitCopyDecoItem();

        if (IsParent)
        {
            selectDecoItem.gameObject.SetActive(false);
            copyDecoItem.SetDecoDataPosition(selectDecoItem.transform.position, selectDecoItem.decoItem);
        }
        else
        {
            selectDecoItem.gameObject.SetActive(true);
        }
    }

    public void DataSetting(List<PlusHousingModelData> listDecoItem)
    {
        int tapIndex = (int)tapType;
        
        //데코 네임 세팅
        objDecoName.SetActive(tapType != DecoTapType.SetItem);
        labelDecoName.text = Global._instance.GetString($"h_{listDecoItem[0].housingIndex}");

        //SubScroll Data Setting
        switch (tapType)
        {
            case DecoTapType.Production:
                {
                    subGridScrollRoot[tapIndex].InItGrid(listDecoItem.Count, (go, index) =>
                    {
                        go.GetComponent<UIItemDeco_Sub_Production>().UpdateData(listDecoItem[index]);

                        subGridScrollRoot[tapIndex].GetComponentInParent<UIScrollView>().panel.Refresh();
                    });
                }
                break;
            case DecoTapType.Install:
                {
                    subGridScrollRoot[tapIndex].InItGrid(Mathf.CeilToInt((float)listDecoItem.Count * 0.25f), (go, index) =>
                    {
                        var listSubDecoItem = go.GetComponentsInChildren<UIItemDeco_Sub_Install>(true);

                        for(int i = 0; i < listSubDecoItem.Length; i++)
                        {
                            listSubDecoItem[i].gameObject.SetActive(true);

                            if (listDecoItem.Count - 1 < (index * 4) + i)
                            {
                                listSubDecoItem[i].gameObject.SetActive(false);
                                continue;
                            }

                            listSubDecoItem[i].UpdateData(listDecoItem[(index * 4) + i]);
                        }

                        subGridScrollRoot[tapIndex].GetComponentInParent<UIScrollView>().panel.Refresh();
                    });
                }
                break;
            case DecoTapType.SetItem:
                {
                    subGridScrollRoot[tapIndex].InItGrid(Mathf.CeilToInt((float)listDecoItem.Count * 0.25f), (go, index) =>
                    {
                        var listSubDecoItem = go.GetComponentsInChildren<UIItemDeco_Sub_SetItem>(true);

                        for (int i = 0; i < listSubDecoItem.Length; i++)
                        {
                            listSubDecoItem[i].gameObject.SetActive(true);

                            if (listDecoItem.Count - 1 < (index * 4) + i)
                            {
                                listSubDecoItem[i].gameObject.SetActive(false);
                                continue;
                            }

                            listSubDecoItem[i].UpdateData(listDecoItem[(index * 4) + i]);
                        }

                        subGridScrollRoot[tapIndex].GetComponentInParent<UIScrollView>().panel.Refresh();
                    });
                }
                break;
        }

        decoSubPanel.Invalidate(true);
        subGridScrollRoot[tapIndex].GetComponentInParent<UIScrollView>().ResetPosition();
    }

    private IEnumerator CoDecoItemMenuTransforming(bool isExpansion)
    {
        float totalTime = 0.0f;

        float startHegiht = decoSubPanel.GetViewSize().y;
        float endHeight = decoItemMinSize + (subScrollValue * (isExpansion == true ? 0 : 1));

        while (totalTime < 1.0f)
        {
            totalTime += Time.unscaledDeltaTime * subScrollSpeed;
    
            float height = Mathf.Lerp(startHegiht, endHeight, totalTime);
            decoSubPanel.SetRect(0.0f, (height - decoItemMinSize) * 0.5f, decoSubPanel.width, height);

            decoSubPanel.Invalidate(true);

            yield return null;
        }

        mainScrollView[(int)tapType].verticalScrollBar.enabled = isExpansion;
    }

    private IEnumerator CoDecoArrayMove(UIItemDeco selectItem)
    {
        float totalTime = 0.0f;

        var decoListPanel = mainScrollView[(int)tapType].GetComponent<UIPanel>();

        var arrayGrid = mainScrollView[(int)tapType].GetComponentInChildren<UIGrid>();

        float startHegiht = Mathf.RoundToInt(decoListPanel.transform.localPosition.y);

        float endHeight = landNameHight + ((landNameHight + interval_yPos) * (landCount[tapType].IndexOf(selectItem.decoItem.landIndex))) + arrayGrid.cellHeight * GetMainDecoItemIndex(selectItem.decoItem, landCount[tapType][0]);

        while (totalTime < 1.0f)
        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                            
            totalTime += Time.unscaledDeltaTime * mainItemMoveSpeed;

            float height = Mathf.Lerp(startHegiht, endHeight, totalTime);

            decoListPanel.transform.localPosition = new Vector3(decoListPanel.transform.localPosition.x, height, decoListPanel.transform.localPosition.z);
            decoListPanel.clipOffset = new Vector2(decoListPanel.clipOffset.x, height * -1f);

            mainScrollView[(int)tapType].UpdateScrollbars();

            yield return null;
        }

        mainScrollView[(int)tapType].enabled = false;
        //뎁스 옮기기
        ChangParent(true);

    }

    public void TapEvent(int clickTapType)
    {
        StartCoroutine(CoClickTapEvent());
        tapType = (DecoTapType)clickTapType;

        if(gridScrollRoot[(int)tapType].transform.childCount == 0)
        {
            //오브젝트 생성
            SetScrollData(tapType);
        }

        EmptyDecoItem();
    }

    public IEnumerator CoClickTapEvent()
    {
        if (state == SubScrollState.Expansion)
        {
            //뎁스를 원래 자리로 (다른 탭 클릭)
            ChangParent(false);

            selectDecoItem.pActiveButton(false);
            mainScrollView[(int)tapType].enabled = true;
            mainScrollView[(int)tapType].RestrictWithinBounds(false, false, true);

            state = SubScrollState.Idle;

            yield return CoDecoItemMenuTransforming(true);

            selectDecoItem = null;
        }
    }

    public List<DecoItemData> GetDecoTapDictionary(DecoTapType tapType)
    {
        switch(tapType)
        {
            case DecoTapType.Production:
                return dataProduction;
            case DecoTapType.Install:
                return dataInstall;
            case DecoTapType.SetItem:
                return dataSetItem;
            default:
                return null;
        }
    }

    public int GetMainDecoItemIndex(DecoItemData selectDecoData, int firstLandIndex)
    {
        var listDecoItem = GetDecoTapDictionary(tapType);

        var tapTypeIndex = tapType != DecoTapType.SetItem ? selectDecoData.housingIndex : selectDecoData.setItemIndex;

        int indexCount = 0;
        int Index = 0;
        int landIndex = firstLandIndex;

        listDecoItem.Sort(landIndexComparer);

        for (int i = 0; i < (int)listDecoItem.Count; i++)
        {
            if (tapTypeIndex == 0) return 0;

            if(listDecoItem[i].landIndex != landIndex)
            {
                indexCount = 1;
                landIndex = listDecoItem[i].landIndex;
                Index++;
            }
            else
            {
                indexCount++;

                if (indexCount > 4)
                {
                    indexCount = 1;
                    Index++;
                }
            }

            if (listDecoItem[i] == selectDecoData)
            { 
                return Index; 
            }

        }

        return -1;
    }

    public void OpenProductionDecoItemBtn(int housingIndex)
    {
        List<UIItemDeco> listDecoItem = new List<UIItemDeco>();

        listDecoItem.AddRange(gridScrollRoot[(int)tapType].GetComponentsInChildren<UIItemDeco>());

        var targetDecoItem = listDecoItem.Find((x) => x.decoItem.housingIndex == housingIndex);

        targetDecoItem.OnClickDecoItem();
    }

    public UIItemDeco_Sub_Production GetDecoItme(int housingIndex, int modelIndex)
    {
        List<UIItemDeco_Sub_Production> listSubDecoItem = new List<UIItemDeco_Sub_Production>();

        listSubDecoItem.AddRange(subGridScrollRoot[(int)tapType].GetComponentsInChildren<UIItemDeco_Sub_Production>());

        return listSubDecoItem.Find((x) => x.decoItem.housingIndex == housingIndex && x.decoItem.modelIndex == modelIndex);
    }

    void OnClickBtnMaterial()
    {
        if (UIPopupDiary._instance.bCanTouch == false)
            return;
        ManagerUI._instance.OpenPopupMaterial(openPopupTime);
    }

    private void OnClickBtnOpenPopupMaterialExchange()
    {
        if (UIPopupDiary._instance.bCanTouch == false)
        {
            return;
        }
        ManagerUI._instance.OpenPopup<UIPopupMaterialExchange>();
    }

    public UIScrollView GetScrollView()
    {
        return mainScrollView[(int)tapType];
    }

    #region 모두 설치 버튼 기능
    void OnClickAllInstall()
    {
        List<PlusHousingModelData> listDeco = new List<PlusHousingModelData>();

        //현재 세트 아이템의 랜드 인덱스
        int setItemLandIndex = 0;
        
        for (int i = 0; i < selectDecoItem.decoItem.listDecoItem.Count; i++)
        {
            int selectedModelIdx = ManagerHousing.GetSelectedHousingModelIdx(selectDecoItem.decoItem.listDecoItem[i].housingIndex);

            if (selectDecoItem.decoItem.listDecoItem[i].active == 1 && selectDecoItem.decoItem.listDecoItem[i].modelIndex != selectedModelIdx)
            {
                listDeco.Add(selectDecoItem.decoItem.listDecoItem[i]);
            }
        }

        if(listDeco.Count == 0)
        {
            UIPopupDiary._instance.ClosePopUp();
            return;
        }

        foreach (var deco in listDeco)
        {
            if (deco.openMission != 0)
            {
                int missionIndex = deco.openMission;
                if (ManagerData._instance._missionData[missionIndex].state != TypeMissionState.Clear)
                {
                    HousingCantOpenPopup();
                    return;
                }
            }
        }

        setItemLandIndex = ManagerHousing.GetHousingLandIndex(listDeco[0].housingIndex);

        List<ServerAPI.HousingPair> setList = new List<ServerAPI.HousingPair>();
        for (int i = 0; i < listDeco.Count; i++)
        {
            setList.Add(new ServerAPI.HousingPair() { index = listDeco[i].housingIndex, model = listDeco[i].modelIndex });
        }

        //랜드가 다를 경우 처리
        if(ManagerLobby.landIndex != setItemLandIndex)
        {
            ManagerLobby._instance.MoveLand(setItemLandIndex, () => ServerAPI.HousingSetChange(setList, recvSetDecoChange));
        }
        else
        {
            ServerAPI.HousingSetChange(setList, recvSetDecoChange);
        }
        
    }
    
    void HousingCantOpenPopup()
    {
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_73"), false);
        popupSystem.SetResourceImage("Message/tired");
    }

    void recvSetDecoChange(HousingChangeResp resp)
    {
        if (resp.IsSuccess)
        {
            for( int i = 0; i < resp.changed.Count; ++i)
            {
                int housingIndex = resp.changed[i].index;
                int selectModelIndex = resp.changed[i].selectModel;

                ManagerHousing.MakeHousingInstance(housingIndex, selectModelIndex, () =>
                {
                    ManagerHousing.SelectModel(housingIndex, selectModelIndex);
                });

                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.HOUSING,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.CHANGE_HOUSING,
                    string.Format("HOUSING_{0}_{1}", housingIndex, selectModelIndex),
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
                var d = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
            }
            
            HousingUserData.SetUserData();
            ManagerUI._instance.ClosePopUpUI();

            Vector3 focusPos = new Vector3();

            if (HousingSetInfoUtility.TryGetLandCamPostion(resp.changed[0].index, resp.changed[0].selectModel, out Vector3 camPos))
                focusPos = camPos;

            CameraController._instance.MoveToPosition(focusPos, 0.5f);
        }
    }

    #endregion
}