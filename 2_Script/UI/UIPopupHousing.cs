using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;
using System;

public class UIPopupHousing : UIPopupBase
{
    public static UIPopupHousing _instance = null;

    public GameObject _objItemHousing;
    public GameObject btnClose;
    public UIScrollView scrollView;

    public AnimationCurve _housingBoxAnimationCurve;
    public AnimationCurve selectButtonAnimation;

    //private ActionObjectHousing currentObject = null;
    private int housingIdx = 0;
    private int defaultModelIndex = 0;  //초기 하우징 인덱스.
    private int currentSelectIndex = 0; //유저가 누른 인덱스.
    private int selectButtonIndex = 1;  //현재 선택 버튼이 떠있는 하우징 인덱스.
    private List<UIItemHousing> itemHousingList = new List<UIItemHousing>();
    
    private const int _nAnimationSpeedRatio = 5;
    private const int BTN_SPACE_XSIZE = 115;  //Deco버튼 X 간격.
    private const int BTN_SPACE_YPOS = 0;   //Deco버튼 Y 위치.

    [NonSerialized]
    bool trackHousing = false;

    private bool bFree = true;

    //화면 위치 관련.
    private Vector3 screenPos = Vector3.zero;
    private Camera guiCam;

    Vector3 focusPos = Vector3.zero;
    Vector3 focusOffset = Vector3.zero;

    bool _newHousingMake = false;
    int _newHousingIndex = 0;
    bool _cancle = false;

    void Awake()
    {
        _instance = this;
    }

    int lastSelectedModelIndex = 0;
    new void OnDestroy()
    {
        if (_newHousingMake && _cancle == false)
        {
            if (selectButtonIndex == _newHousingIndex + 1)
            {
                ManagerHousing.MakeHousingChat(housingIdx, lastSelectedModelIndex);
            }
        }
        if( _instance == this )
            _instance = null;
        base.OnDestroy();
    }
    public override void OpenPopUp(int _depth)
    {
        bCanTouch = false;
        popupType = PopupType.housing;
        uiPanel.depth = _depth;
        StartCoroutine(OpenHousingUI());
        scrollView.panel.depth = uiPanel.depth + 1;

        StartCoroutine(CoPostOpenPopup());
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        scrollView.panel.useSortingOrder = true;
        scrollView.panel.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitHousing(int housingIdx, int selectIndex, bool bSelectAction, Vector3 focusPos, Method.FunctionVoid in_callback = null)
    {
        guiCam = NGUITools.FindCameraForLayer(gameObject.layer);

        this.focusPos = ManagerHousing.IsMovableHousing(housingIdx) ? ManagerHousing.GetHousingFocusPosition(housingIdx) : focusPos;
        focusOffset = focusPos - ManagerHousing.GetHousingFocusPosition(housingIdx);
        this.housingIdx = housingIdx;
        //초기 인덱스 받아오기.
        defaultModelIndex = ManagerHousing.GetSelectedHousingModelIdx(housingIdx);
        if (defaultModelIndex == 0)
            defaultModelIndex = 1;
        
        //하우징 버튼 생성.
        MakeBtnHousingItem(housingIdx);

        //현재 초기 모델 인덱스에 해당하는 버튼 찾아서 인덱스 저장.
        for (int i = 0; i < itemHousingList.Count; i++)
        {
            if (itemHousingList[i].housingData.modelIndex == defaultModelIndex)
            {
                currentSelectIndex = i + 1;
                selectButtonIndex = i + 1;
                break;
            }
        }
        itemHousingList[(selectButtonIndex - 1)].housingBox.spriteName = "housing_box_01";

        //아무것도 구매하지 않은 상태라면 X버튼 비활성화, 버튼 상태 변경.
        if (bFree == true)
        {
            int activeHousingCount = 0;
            for (int i = 0; i < itemHousingList.Count; i++)
            {
                if( ServerContents.Housings[itemHousingList[i].housingData.housingIndex][itemHousingList[i].housingData.modelIndex].housingOpenType == (int)PlusHousingModelDataType.byMission)
                    itemHousingList[i].FreeSetting();

                if (itemHousingList[i].housingData.active == 1)
                    activeHousingCount++;
            }
            if( activeHousingCount == 0)
                btnClose.SetActive(false);

            if ( (selectButtonIndex -1) < itemHousingList.Count)
            {
                var d = itemHousingList[(selectButtonIndex - 1)];
                if( d.housingData.active == 1)
                {
                    d.housingSelectIcon.SetActive(true);
                }
            }
        }
        else
        {
            itemHousingList[(selectButtonIndex - 1)].housingSelectIcon.SetActive(true);
        }

        _callbackEnd = in_callback;

        
        CameraController._instance.MoveToPosition(focusPos, 0.5f);
        
        StartCoroutine(CoAction(0.5f, () => { trackHousing = true;}));

        //storage 창에서 하우징 열렸을 경우 연출.
        if (selectIndex > -1)
        {
            for (int i = 0; i < itemHousingList.Count; i++)
            {
                //현재 찾고자 하는 모델번호 검사 후, 해당 모델을 들고있는 버튼 인덱스를 저장.
                if (itemHousingList[i].housingData.modelIndex == selectIndex)
                {
                    selectIndex = i;
                    break;
                }
            }

            //버튼 생성되는 연출 나오게 할건지.
            if (bSelectAction == true)
            {
                _newHousingMake = true;
                _newHousingIndex = selectIndex;

                itemHousingList[selectIndex].transform.localScale = Vector3.zero;

                //아이템 위치 변경 및 연출 때 이동해야할 offset 값 반환.
                StartCoroutine(CoAction(0.1f, () =>
                {
                    bCanTouch = false;
                    //현재 선택 버튼 변경.
                    ChangeSelectButton(selectIndex + 1);
                    //하우징 버튼 생성연출.
                    StartCoroutine(itemHousingList[selectIndex].PlusHousingAction());
                    //사운드 출력.
                    ManagerSound.AudioPlay(AudioLobby.Housing_Select);
                }));
            }
            else
            {
                //현재 선택 버튼 변경.
                ChangeSelectButton(selectIndex + 1);
                itemHousingList[selectIndex].ModelSetting();
            }
        }
    }

    //현재 선택한 오브젝트 정보 저장.
    public bool SelectObj(int itemIndex, PlusHousingModelData hData)
    {
        if (bCanTouch == false)
            return false;
        if (selectButtonIndex == itemIndex)
            return false;
        currentSelectIndex = itemIndex;
        return true;
    }

    //구매여부나 게임모드에 따라 모델, 버튼 이미지 변경. 
    public void CheckAndChangeModel()
    {
        if (itemHousingList == null || itemHousingList.Count < 1) return;

        PlusHousingModelData data = itemHousingList[currentSelectIndex - 1].housingData;
        
        // 이미 구매한 아이템이거나, 아무것도 구매하지 않은경우.
        if (data.active == 1 || bFree == true)
        {
            //모델 데이터 변경.
            ChangeModel(itemHousingList[currentSelectIndex - 1].housingData.modelIndex);
            SelectButtonSetting();
        }
    }

    public bool CheckCanPurchase()
    {
        PlusHousingModelData data = itemHousingList[currentSelectIndex - 1].housingData;
        // 구매할 재화가 없는 경우는 구매실패 팝업 띄움.
        if (data.costCoin > Global.coin || data.costJewel > Global.jewel)
        {
            if (data.costCoin > 0)
            {
                ManagerUI._instance.LackCoinsPopUp();
            }
            else
            {
                ManagerUI._instance.LackDiamondsPopUp();
            }
            return false;
        }
        return true;
    }

    //현재 마지막으로 선택한 하우징 아이템 구매.
    public void PurchaseHousingItem()
    {
        PlusHousingModelData data = itemHousingList[currentSelectIndex - 1].housingData;
        
        if (bFree == true && ServerContents.Housings[data.housingIndex][data.modelIndex].housingOpenType == (int)PlusHousingModelDataType.byMission)
        {
            StartCoroutine(itemHousingList[(currentSelectIndex - 1)].CoChangeButton(true, () => { ManagerUI._instance.ClosePopUpUI(); }));
        }
        else if (data.active == 1)
        {
            SelectButtonSetting();
        }
    }

    private void ChangeSelectButton(int selectIndex)
    {
        //현재 선택 버튼 변경.
        currentSelectIndex = selectIndex;

        //버튼 위치로 이동.
        float yPos = scrollView.gameObject.transform.localPosition.y;
        Vector3 pos = Vector3.zero;
        if (selectIndex > 8)
        {
            int reviseNum = selectIndex % 4 == 0 ? 2 : 1;
            
            yPos += -42.5f + (((selectIndex / 4) - reviseNum) * 120f);
        }
        pos = new Vector3(scrollView.gameObject.transform.localPosition.x, yPos, 0f);
        SpringPanel.Begin(scrollView.gameObject, pos, 8);
    }

    private void ChangeModel(int modelIndex)
    {
        ManagerHousing.SelectModel(this.housingIdx, modelIndex);
        
    }

    private void SelectButtonSetting()
    {   
        itemHousingList[(selectButtonIndex - 1)].housingBox.spriteName = "housing_box_02";

        if (selectButtonIndex != currentSelectIndex &&
            (selectButtonIndex - 1) >= 0 &&
            (selectButtonIndex -1) < itemHousingList.Count)
        {
            itemHousingList[(selectButtonIndex - 1)].housingSelectIcon.gameObject.SetActive(false);
        }
        selectButtonIndex = currentSelectIndex;
    }

    private void OnClickBtnConfirm()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        PlusHousingModelData data = itemHousingList[selectButtonIndex - 1].housingData;
        //연출 상태에서 'v'버튼 누르면 현재 선택된 오브젝트를 구매.
        if (ServerContents.Housings[data.housingIndex][data.modelIndex].housingOpenType == (int)PlusHousingModelDataType.byMission && bFree == true)
        {
            ServerAPI.HousingFreeSelect(data.housingIndex, data.modelIndex, recvHousingFreeSelect);
        }
        else
        {
            if (data.costCoin > 0)
            {
                if ((int)ServerRepos.User.coin >= data.costCoin) usePJewel = data.costCoin;
                else if ((int)ServerRepos.User.coin > 0)
                {
                    usePJewel = (int)ServerRepos.User.coin;
                    useFJewel = data.costCoin - (int)ServerRepos.User.coin;
                }
                else useFJewel = data.costCoin;
            }
            else if (data.costJewel > 0)
            {
                if ((int)ServerRepos.User.jewel >= data.costJewel) usePJewel = data.costJewel;
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = data.costJewel - (int)ServerRepos.User.jewel;
                }
                else useFJewel = data.costJewel;
            }            

            ServerAPI.HousingChange(data.housingIndex, data.modelIndex, recvHousingChange);
        }
    }

    void recvHousingFreeSelect(HousingFreeSelectResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** HousingFreeSelect ok count:" + resp.selectedItem);
            PlusHousingModelData.SetUserData();
            HousingUserData.SetUserData();
            ManagerSound.AudioPlay(AudioLobby.UseClover);
            PurchaseHousingItem();
            CameraController._instance.AbortCameraTarget();

            //그로씨
            PlusHousingModelData data = itemHousingList[selectButtonIndex - 1].housingData;
            string fileName = string.Format("h_{0}_{1}", data.housingIndex, data.modelIndex);
            string ItemName = string.Format("DECO-{0}-{1}", data.housingIndex, data.modelIndex);


            var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                      (
                         ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.DECO,
                          ItemName,//data.housingIndex.ToString(),
                          fileName,
                          1,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_QUEST_REWARD,
                          "m" + UIItemDiaryMission.GrowthyMisisonIndex
                      );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.HOUSING,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.CHANGE_HOUSING,
                    string.Format("HOUSING_{0}_{1}", data.housingIndex, data.modelIndex),
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        }
    }

    int useFJewel = 0;
    int usePJewel = 0;

    void recvHousingChange(HousingChangeResp resp, int housingIndex, int modelIndex)
    {
        if (resp.IsSuccess)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.HOUSING,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.CHANGE_HOUSING,
                    string.Format("HOUSING_{0}_{1}", housingIndex, modelIndex),
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);

            ManagerSound.AudioPlay(AudioLobby.UseClover);
            HousingUserData.SetUserData();
            ManagerUI._instance.ClosePopUpUI();

            CameraController._instance.AbortCameraTarget();
        }
    }

    public override void OnClickBtnBack()
    {
        if (bCanTouch == false || !btnClose.activeSelf)
            return;
        OnClickBtnClose();
    }

    private void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        _cancle = true;

        //원래 설정돼있던 index로 돌려줘야 함.
        currentSelectIndex = defaultModelIndex;

        CameraController._instance.AbortCameraTarget();
        int modelIdx = ManagerHousing.GetSelectedHousingModelIdx(housingIdx);
        if (modelIdx == 0)
            modelIdx = 1;

        ChangeModel(modelIdx);
        ManagerUI._instance.ClosePopUpUI();
    }

    List<PlusHousingModelData> SelectedHousingSort()
    {
        var NonSelectedHousingList = ManagerHousing.GetHousingModels(housingIdx);

        //선택된 하우징이 없을 때 예외 처리
        if (!NonSelectedHousingList.Contains(ManagerHousing.GetHousingModel(housingIdx, ManagerHousing.GetSelectedHousingModelIdx(housingIdx))))
            return NonSelectedHousingList;

        NonSelectedHousingList.RemoveAt(ManagerHousing.GetSelectedHousingModelIdx(housingIdx) - 1);

        NonSelectedHousingList.Add(ManagerHousing.GetHousingModel(housingIdx, ManagerHousing.GetSelectedHousingModelIdx(housingIdx)));

        return NonSelectedHousingList;
    }

    //데코 버튼들 생성& 위치설정 해주는 함수.
    private void MakeBtnHousingItem( int housingIdx)
    {
        itemHousingList.Clear();
        int itemIndex = 1;

        bool freeHousingFound = false;
        bool noActiveFreeHousing = true;
        bool errChk_nothingFound = true;

        var hList = SelectedHousingSort();

        for(int i = hList.Count - 1; i >=  0; --i)
        {
            var housingModel = hList[i];
            errChk_nothingFound = false;

            if (housingModel.type != PlusHousingModelDataType.byMission && housingModel.active == 0)
                continue;

            int modelIndex = housingModel.modelIndex;
            itemHousingList.Add(NGUITools.AddChild(scrollView.GetComponentInChildren<UIGrid>().gameObject, _objItemHousing).GetComponent<UIItemHousing>());
            itemHousingList[itemIndex - 1].SetDecoInfo(itemIndex, housingModel);

            if (ServerContents.Housings[housingIdx][modelIndex].housingOpenType == (int)(PlusHousingModelDataType.byMission))
            {
                freeHousingFound = true;
                if (housingModel.active == 1 && noActiveFreeHousing)
                    noActiveFreeHousing = false;
            }

            //구매 여부에 따라 버튼 모양 세팅.
            if (housingModel.active == 1)
            {
                itemHousingList[itemIndex - 1].PurchaseSetting();
                if (modelIndex == defaultModelIndex)
                {
                    SelectButtonSetting();
                }
                bFree = false;
            }
            else
            {
                itemHousingList[itemIndex - 1].DefaultSetting();
            }
            itemIndex++;
        }

        if( errChk_nothingFound )
        {
            Debug.LogError("DC에 이 하우징의 정보가 없습니다");
        }
        else if( itemHousingList.Count == 0)
        {
            Debug.LogError("조건에 맞는 하우징이 하나도 없습니다");
        }

        if(freeHousingFound && noActiveFreeHousing)
        {
            bFree = true;
        }


        //선택할 수 있는 하우징이 1개 이하이면 x 버튼 비활성화.
        if (itemHousingList.Count <= 1)
        {
            btnClose.SetActive(false);
        }
        SetDecoObjPosition();
    }

    public void SetDecoObjPosition()
    {
        int itemCount = itemHousingList.Count;
        for (int i = 0; i < itemCount; i++)
        {
            float startXPos = 0f;
            if (itemCount == 2)
            {
                startXPos = -60f + (120f * i);
            }
            else if (itemCount == 3)
            {
                startXPos = -116f + (116f * i);
            }
            else if(itemCount >= 4)
            {
                startXPos = -140f + (112f * i);
            }

            itemHousingList[i].transform.localPosition = new Vector3(startXPos, BTN_SPACE_YPOS, 0);
        }
    }

    public override void ClosePopUp(float _startTime = 0.3f, Method.FunctionVoid callback = null)
    {
        StartCoroutine(CoCloseHousingUI());
    }

    IEnumerator CoCloseHousingUI(bool useGrowthy = false)
    {
        if (useGrowthy)
        {
            yield return new WaitForSeconds(0.5f);
        }

        float animationTimer = 1;
        float ratio;
        while (animationTimer > 0)
        {
            ratio = _housingBoxAnimationCurve.Evaluate(animationTimer);
            mainSprite.gameObject.transform.localScale = Vector3.one * ratio;
            animationTimer -= Time.deltaTime * _nAnimationSpeedRatio;
            yield return null;
        }
        ImmediatelyCloseHousingUI();
    }

    void ImmediatelyCloseHousingUI()
    {
        if (_callbackEnd != null)
            _callbackEnd();
     
        mainSprite.transform.localScale = Vector3.zero;
        for (int i = 0; i < itemHousingList.Count; i++)
        {
            itemHousingList[i].Recycle();
        }

        if(_newHousingIndex< itemHousingList.Count)
            lastSelectedModelIndex = itemHousingList[_newHousingIndex].housingData.modelIndex;

        itemHousingList.Clear();
        Destroy(gameObject);
    }
  
    IEnumerator OpenHousingUI()
    {
        float animationTimer = 0;
        float ratio;
        while (animationTimer < 1)
        {
            ratio = _housingBoxAnimationCurve.Evaluate(animationTimer);
            mainSprite.gameObject.transform.localScale = Vector3.one * ratio;
            animationTimer += Time.deltaTime * _nAnimationSpeedRatio;
            yield return null;
        }
        mainSprite.gameObject.transform.localScale = Vector3.one;
        bCanTouch = true;
    }

    private float GetUIOffSet()
    {
        float ratio = Camera.main.fieldOfView;
        return ratio;
    }
    private void LateUpdate()
    {
        var housingPos = ManagerHousing.IsMovableHousing(housingIdx) ? ManagerHousing.GetHousingFocusPosition(housingIdx) + focusOffset : focusPos;
        screenPos = guiCam.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(housingPos));

        screenPos.z = 0f;
        mainSprite.transform.position = screenPos;
        mainSprite.transform.localPosition -= Vector3.up * GetUIOffSet();

        if (ManagerHousing.IsMovableHousing(housingIdx) && trackHousing && CameraController._instance._rigidSkipTimer <= 0.0f)
        {
            var p = Vector3.Lerp(CameraController._instance.GetCenterWorldPos(), housingPos, 0.5f);
            //CameraController._instance.MoveToPosition(housingPos, 1.5f);
            CameraController._instance.SetCameraPosition(p);
        }

    }

    public void PurchaseHousingItem(int housingIndex, int modelIndex)
    {
        for(int i = 0; i < itemHousingList.Count; i++)
        {
            if(itemHousingList[i].housingData.housingIndex == housingIndex && itemHousingList[i].housingData.modelIndex == modelIndex)
            {
                itemHousingList[i].OnClickDecoBtn();
            }
        }
    }
}
