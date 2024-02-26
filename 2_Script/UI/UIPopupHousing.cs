using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Protocol;
using Newtonsoft.Json;

public class UIPopupHousing : UIPopupBase
{
    public static UIPopupHousing _instance = null;

    public GameObject _objItemHousing;
    public GameObject btnClose;
    public UIScrollView scrollView;

    public AnimationCurve _housingBoxAnimationCurve;
    public AnimationCurve selectButtonAnimation;

    private ActionObjectHousing currentObject = null;
    private int defaultModelIndex = 0;  //초기 하우징 인덱스.
    private int currentSelectIndex = 0; //유저가 누른 인덱스.
    private int selectButtonIndex = 1;  //현재 선택 버튼이 떠있는 하우징 인덱스.
    //private List<UIItemHousing> itemHousingList = new List<UIItemHousing>();
    
    private const int _nAnimationSpeedRatio = 5;
    private const int BTN_SPACE_XSIZE = 115;  //Deco버튼 X 간격.
    private const int BTN_SPACE_YPOS = 0;   //Deco버튼 Y 위치.

    private bool bFree = true;

    //화면 위치 관련.
    private Vector3 screenPos = Vector3.zero;
    private Camera guiCam;

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
                Character character = ManagerLobby._instance.GetCharacter(TypeCharacterType.Boni);
                character._ai.PlayAnimation(false, "haughty", WrapMode.Loop, 0f, 1f);
                
                if (currentObject != null)
                {
                    if ( currentObject._firstGetTextSpecial.ContainsKey(lastSelectedModelIndex) )
                    {
                        var textList = currentObject._firstGetTextSpecial[lastSelectedModelIndex];

                        int index = Random.Range(0, textList.Count);
                        var lobbyChat = UILobbyChat.MakeLobbyChat(character._transform, Global._instance.GetString(textList[index]), 2.5f);
                        lobbyChat.heightOffset = character.GetBubbleHeightOffset();
                    }
                    else if (currentObject._firstGetBoniTextList.Count > 0)
                    {
                        int index = Random.Range(0, currentObject._firstGetBoniTextList.Count);
                        var lobbyChat = UILobbyChat.MakeLobbyChat(character._transform, currentObject.GetString(currentObject._firstGetBoniTextList[index]), 2.5f);
                        lobbyChat.heightOffset = character.GetBubbleHeightOffset();
                    }
                }
                    
                
            }
        }
        //Debug.Log("defaultModelIndex " + defaultModelIndex + " currentSelectIndex " + currentSelectIndex);
        //Debug.Log(" _newHousingMake " + _newHousingMake + " _newHousingIndex " + _newHousingIndex);

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

    public void InitHousing(ActionObjectHousing Obj, int selectIndex, bool bSelectAction,  Method.FunctionVoid in_callback = null)
    {
        guiCam = NGUITools.FindCameraForLayer(gameObject.layer);
        currentObject = Obj;
        
        //초기 인덱스 받아오기.
        defaultModelIndex = Obj._modelIndex;
        if (defaultModelIndex == 0 && Obj._housingIndex > 0)
        {
            for (int i = 0; i < ManagerData._instance._housingSelectData.Count; i++)
            {
                if (ManagerData._instance._housingSelectData[i].index == Obj._housingIndex)
                {
                    defaultModelIndex = ManagerData._instance._housingSelectData[i].selectModel;
                    break;
                }
            }
            //defaultModelIndex = ManagerData._instance._housingSelectData[Obj._housingIndex - 1].selectModel;
        }
        
        //하우징 버튼 생성.
        MakeBtnHousingItem(Obj._finishObject, Obj._housingIndex);
        /*
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
                if( ServerContents.Housings[itemHousingList[i].housingData.housingIndex][itemHousingList[i].housingData.modelIndex].housingOpenType == 1)
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
        */
        _callbackEnd = in_callback;

        if (currentObject._finishObject.Count>0)
            CameraController._instance.MoveToPosition(currentObject._finishObject[0].transform.position,0.5f);
        else
            CameraController._instance.MoveToPosition(currentObject.transform.position, 0.5f);
        /*
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

                // 현재 하우징에서 열린 아이템 수에 따라 스크롤 패널 위치 설정.
                if (itemHousingList.Count > 3)
                {
                    scrollView.transform.localPosition = new Vector3(30f, scrollView.gameObject.transform.localPosition.y, 0f);
                    scrollView.panel.clipOffset = new Vector2(-30f, 0);
                }
                else
                {
                    scrollView.transform.localPosition = new Vector3(60f, scrollView.gameObject.transform.localPosition.y, 0f);
                    scrollView.panel.clipOffset = new Vector2(-60f, 0);
                }

                //아이템 위치 변경 및 연출 때 이동해야할 offset 값 반환.
                float xOffset = CheckItmePosition(selectIndex);
                StartCoroutine(CoAction(0.6f, () =>
                {
                    bCanTouch = false;
                    //현재 선택 버튼 변경 & 스크롤을 선택버튼 위치로 이동.
                    ChangeSelectButton(selectIndex + 1);
                    //하우징 버튼 생성연출 & 뒤에 아이템들 밀어지는 연출.
                    ActionPosition(selectIndex, xOffset);
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
        }*/
    }

    float CheckItmePosition(int selectIndex)
    {
        float xOffset = 0f;
        /*   int itemCount = itemHousingList.Count;

           //생성될 아이템이 중앙 쯤에 위치하면 뒤에 있는 아이템들 위치를 한칸씩 땡겨줌.
           //(연출 후 뒤로 밀어주기 위해).
           if (selectIndex != (itemCount - 1))
           {
               if (itemCount == 2)
               {
                   xOffset = 120f;
               }
               else if (itemCount == 3)
               {
                   xOffset = 116f;
               }
               else if (itemCount >= 4)
               {
                   xOffset = 112f;
               }

               for (int i = (selectIndex + 1); i < itemCount ; i++)
               {
                //   Vector3 pos = itemHousingList[i].transform.localPosition;
                //  itemHousingList[i].transform.localPosition = new Vector3(pos.x - xOffset, pos.y, pos.z);
               }
           }*/
        return xOffset;
    }

    void ActionPosition(int selectIndex, float xOffset)
    {/*
        StartCoroutine(itemHousingList[selectIndex].PlusHousingAction());
        for (int i = (selectIndex + 1); i < itemHousingList.Count; i++)
        {
            itemHousingList[i].MoveBtnPosition(xOffset);
        }*/
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
   //     PlusHousingModelData data = itemHousingList[selectButtonIndex - 1].housingData;
      //  PlusHousingModelData data = itemHousingList[currentSelectIndex - 1].housingData;
        //Debug.Log("하우징 selectButtonIndex  " + selectButtonIndex + " itemHousingList.Count " + itemHousingList.Count);

        //Debug.Log("하우징 data.active  " + data.active + " bFree " + bFree);

        // 이미 구매한 아이템이거나, 아무것도 구매하지 않은경우.
      //  if (data.active == 1 || bFree == true)
        {

            //Debug.Log("하우징 currentSelectIndex  " + currentSelectIndex + " modelIndex " + itemHousingList[currentSelectIndex - 1].housingData.modelIndex);

            //모델 데이터 변경.
          //  ChangeModel(itemHousingList[currentSelectIndex - 1].housingData.modelIndex);
         //   SelectButtonSetting();
        }
    }

    public bool CheckCanPurchase()
    {
       /* PlusHousingModelData data = itemHousingList[currentSelectIndex - 1].housingData;
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
        }*/
        return true;
    }

    //현재 마지막으로 선택한 하우징 아이템 구매.
    public void PurchaseHousingItem()
    {
     //   PlusHousingModelData data = itemHousingList[currentSelectIndex - 1].housingData;
        /*
        if (bFree == true && ServerContents.Housings[data.housingIndex][data.modelIndex].housingOpenType == 1)
        {
            StartCoroutine(itemHousingList[(currentSelectIndex - 1)].CoChangeButton(true, () => { ManagerUI._instance.ClosePopUpUI(); }));
        }
        else if (data.active == 1)
        {
            SelectButtonSetting();
        }*/
    }

    private void ChangeSelectButton(int selectIndex)
    {
        //현재 선택 버튼 변경.
        currentSelectIndex = selectIndex;

        //버튼 위치로 이동.
        float xPos = 0f;
        Vector3 pos = Vector3.zero;
        if (selectIndex > 3)
        {
            xPos = -54f - ((selectIndex - 4) * 110f);
        }
        pos = new Vector3(xPos, scrollView.gameObject.transform.localPosition.y, 0f);
        SpringPanel.Begin(scrollView.gameObject, pos, 8);
    }

    private void ChangeModel(int modelIndex)
    {
        currentObject._modelIndex = modelIndex;
        currentObject.RefreshModel(false);
    }

    private void SelectButtonSetting()
    {   /*
        itemHousingList[(selectButtonIndex - 1)].housingBox.spriteName = "housing_box_02";

        if (bFree == false && selectButtonIndex != currentSelectIndex)
        {
            itemHousingList[(selectButtonIndex - 1)].housingSelectIcon.gameObject.SetActive(false);
        }
        selectButtonIndex = currentSelectIndex;*/
    }

    private void OnClickBtnConfirm()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        /*
        PlusHousingModelData data = itemHousingList[selectButtonIndex - 1].housingData;
        //연출 상태에서 'v'버튼 누르면 현재 선택된 오브젝트를 구매.
        if (ServerContents.Housings[data.housingIndex][data.modelIndex].housingOpenType == 1 && bFree == true)
        {
            ServerAPI.HousingFreeSelect(data.housingIndex, data.modelIndex, recvHousingFreeSelect);
        }
        else
        {
            if (data.costCoin > 0)
            {
                if ((int)ServerRepos.User.coin > data.costCoin) usePJewel = data.costCoin;
                else if ((int)ServerRepos.User.coin > 0)
                {
                    usePJewel = (int)ServerRepos.User.coin;
                    useFJewel = data.costCoin - (int)ServerRepos.User.coin;
                }
                else useFJewel = data.costCoin;
            }
            else if (data.costJewel > 0)
            {
                if ((int)ServerRepos.User.jewel > data.costJewel) usePJewel = data.costJewel;
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = data.costJewel - (int)ServerRepos.User.jewel;
                }
                else useFJewel = data.costJewel;
            }            

            ServerAPI.HousingChange(data.housingIndex, data.modelIndex, recvHousingChange);
        }*/
    }
    /*
    void recvHousingFreeSelect(HousingFreeSelectResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** HousingFreeSelect ok count:" + resp.selectedItem);
            PlusHousingModelData.SetUserData();
            HousingUserData.SetUserData();
            ManagerSound.AudioPlay(AudioLobby.UseClover);
            PurchaseHousingItem();

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
        }
    }
*/
    int useFJewel = 0;
    int usePJewel = 0;
/*
    void recvHousingChange(HousingChangeResp resp)
    {
        if (resp.IsSuccess)
        {
            //Debug.Log("** HousingChange ok count:" + resp.changed.Count);
            ManagerSound.AudioPlay(AudioLobby.UseClover);
            HousingUserData.SetUserData();
            ManagerUI._instance.ClosePopUpUI();
            
            //그로씨
            PlusHousingModelData data = itemHousingList[selectButtonIndex - 1].housingData;
            string fileName = string.Format("h_{0}_{1}", data.housingIndex, data.modelIndex);
            string ItemName = string.Format("DECO-{0}-{1}", data.housingIndex, data.modelIndex);


            var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                      (
                         ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.DECO,
                          ItemName,//data.housingIndex.ToString(),
                          fileName,//data.modelIndex.ToString(),//housingName,
                          1,
                          ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                          fileName
                      );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
            
            //니     public int costCoin = 1;    // 코인이나 보석 둘중하나... 즉시 획득 비용    public int costJewel = 2;
            
            if(data.costCoin > 0)
            {
                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_DECO,
                    -usePJewel,
                    -useFJewel,
                    (int)(ServerRepos.User.coin),
                    (int)(ServerRepos.User.fcoin),
                    fileName
                    );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if (data.costJewel > 0)
            {
                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_DECO,
                    -usePJewel,
                    -useFJewel,
                    (int)(ServerRepos.User.jewel),
                    (int)(ServerRepos.User.fjewel),
                    fileName
                    );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            

        }
    }
    */
    private void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        _cancle = true;

        //원래 설정돼있던 index로 돌려줘야 함.
        currentSelectIndex = defaultModelIndex;

        /*
        if (currentObject != null)
        {
            if (defaultModelIndex == 0 && 
                ServerContents.Housings.ContainsKey(currentObject._housingIndex) && ServerContents.Housings[currentObject._housingIndex].ContainsKey(1)) 
            {   // 이 하우징에서 선택이 한번도 된 적 없을 때
                bool foundSelected = false;
                for (int i = 0; i < ServerRepos.UserHousingSelected.Count; i++)
                {
                    if (ServerRepos.UserHousingSelected[i].index == currentObject._housingIndex)
                    {
                        foundSelected = true;
                    }
                }

                if( !foundSelected )
                {
                    defaultModelIndex = 1;
                    currentSelectIndex = 1;
                }
            }

            if (!currentObject._haveIndex)
            {
                for (int i = 0; i < ServerRepos.UserHousingSelected.Count; i++)
                {
                    if (ServerRepos.UserHousingSelected[i].index == currentObject._housingIndex)
                    {
                        currentObject._haveIndex = true;
                        break;
                    }
                }

                var d = this.itemHousingList[currentSelectIndex - 1].housingData;
                if ( d.active == 0 && ServerContents.Housings[d.housingIndex][d.modelIndex].housingOpenType == 1)
                    currentObject._haveIndex = true;
                else if (!currentObject._haveIndex)
                    currentSelectIndex = 0;
            }
        }
        */

        ChangeModel(currentSelectIndex);
        ManagerUI._instance.ClosePopUpUI();
    }

    //데코 버튼들 생성& 위치설정 해주는 함수.
    private void MakeBtnHousingItem(List<ObjectBase> objectInfo, int objIndex)
    {/*
        itemHousingList.Clear();
        int itemIndex = 1;

        bool freeHousingFound = false;
        bool noActiveFreeHousing = true;
        
        foreach (var item in ManagerData._instance._housingGameData)
        { 
            //하우징 데이터 중 현재 모델의 아이템만 추가.
            if (objIndex == item.Value.housingIndex)
            {
                if (item.Value.type != PlusHousingModelDataType.byMission && item.Value.active == 0)
                    continue;

                int modelIndex = item.Value.modelIndex;
                itemHousingList.Add(NGUITools.AddChild(scrollView.gameObject, _objItemHousing).GetComponent<UIItemHousing>());
                itemHousingList[itemIndex - 1].SetDecoInfo(itemIndex, item.Value);

                if( ServerContents.Housings[objIndex][modelIndex].housingOpenType == 1)
                {
                    freeHousingFound = true;
                    if (item.Value.active == 1 && noActiveFreeHousing)
                        noActiveFreeHousing = false;
                }

                //구매 여부에 따라 버튼 모양 세팅.
                if (item.Value.active == 1)
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
                itemIndex ++;
            }
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
        SetDecoObjPosition();*/
    }

    public void SetDecoObjPosition()
    {
        /*
        HousingData housingData = LobbyScene.instance._housingGameData.GetHousingData(currentObject._nHousingGameDataIndex);
        int nCount = housingData._listPurchaseIndex.Count;
        int _halfCount = (int)(_nHousingObjectCount * 0.5f);

        int _nPurIndex = _halfCount;               // 구매한 인덱스.
        int _nDeInex = (_halfCount - nCount);   // 구매하지 않은 인덱스.
        float _fPur_StartXPos = 0;  //구매한 데코의 초기위치.
        float _fDef_StartXPos = 0;  //구매하지 않은 데코의 초기위치.

        for (int i = 0; i < _nHousingObjectCount; i++)
        {
            _fPur_StartXPos = -BTN_SPACE_XSIZE * _nPurIndex;
            _fDef_StartXPos = -BTN_SPACE_XSIZE * _nDeInex;
            if (_nHousingObjectCount % 2 == 0)
            {
                _fPur_StartXPos += 135f;
                _fDef_StartXPos += 135f;
            }

            //구매한 데코 목록들은 앞에 배치.
            if (_listDecoBtn[i]._bPurchase == true)
            {
                _listDecoBtn[i].transform.localPosition = new Vector3(_fPur_StartXPos, BTN_SPACE_YPOS, 0);
                _nPurIndex -= 1;
            }
            else
            {
                _listDecoBtn[i].transform.localPosition = new Vector3(_fDef_StartXPos, BTN_SPACE_YPOS, 0);
                _nDeInex -= 1;
            }
        }
        }

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
        }*/
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
     /*
        mainSprite.transform.localScale = Vector3.zero;
        for (int i = 0; i < itemHousingList.Count; i++)
        {
            itemHousingList[i].Recycle();
        }

        if(_newHousingIndex< itemHousingList.Count)
            lastSelectedModelIndex = itemHousingList[_newHousingIndex].housingData.modelIndex;

        itemHousingList.Clear();*/
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
        if (currentObject._finishObject.Count > 0)
            screenPos = guiCam.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(currentObject._finishObject[0].transform.position));
        else
            screenPos = guiCam.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(currentObject.transform.position));

        
        screenPos.z = 0f;
        mainSprite.transform.position = screenPos;
        mainSprite.transform.localPosition -= Vector3.up * GetUIOffSet();
    }
}
