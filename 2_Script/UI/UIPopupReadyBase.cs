using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using DG.Tweening;
using Protocol;
using Newtonsoft.Json;
using UnityEngine.Networking;

/// <summary>
/// 별도의 레디 팝업을 설정할 때 사용하는 스크립트
/// UIPopupReady팝업에서 불필요한 기능을 제거한 스크립트
/// </summary>
[IsReadyPopup]
public class UIPopupReadyBase : UIPopupReady
{
    //게임 시작 버튼
    [SerializeField] protected UIItemReadyBtnStart startButton;
    
    protected override void SetStageLabel()
    {
        if (stage == null || stage.Length == 0)
            return;
        base.SetStageLabel();
    }

    public override void InitPopUp(StageMapData stageData)
    {
        stageIndex = Global.stageIndex;
        InitPopup_Description();
        InitSelectReadyItem();
        tempData = stageData;

        AddListTargetInfo(tempData.listTargetInfo);
        LoadTarget();
        
        SetReadyItem(tempData.gameMode == (int)GameMode.LAVA);
        SetHardStage();
        SetStageFlower();

        //스타트 버튼 설정
        InitPopupReadyAD();
        SetStartButton();

        //포코타 설정
        SetBoniModel();
        SettingBoniDialog();
        
        //Lan 페이지 설정
        if(lanpageButton != null)
        {
            lanpageButton.On($"LGPKV_bomb_random", Global._instance.GetString("p_sr_27"));
            
            if (LanguageUtility.IsShowBuyInfo)
            {
                lanpageButton.transform.localPosition 
                    = new Vector2(lanpageButton.transform.localPosition.x, lanpageButton.transform.localPosition.y - 20f);
            }
        }
    }

    /// <summary>
    /// 상품판매 법률 개정 UI
    /// </summary>
    private void InitPopup_Description()
    {
        objDescription.SetActive(LanguageUtility.IsShowBuyInfo);

        if (LanguageUtility.IsShowBuyInfo)
        {
            sprSelectItemBox.spriteName = "stage_bg_01";
            sprSelectItemBox.height = 315;
            objItemBoxLabel.SetActive(false);
            objSelectItemRoot.transform.localPosition += new Vector3(0f, 60f, 0f);
            objMaskPanel.transform.localPosition += new Vector3(0f, 60f, 0f);
        }
    }
    
    protected override void InitPopupReadyAD()
    {
        if (objSelectADRoot_AddTurn != null)
            base.InitPopupReadyAD();
    }
    
    /// <summary>
    /// 레디 아이템 선택 관련 설정
    /// </summary>
    private void InitSelectReadyItem()
    {
        //아이템선택상태 읽어오기
        if (PlayerPrefs.HasKey(Global.GameInstance.GetReadyItemSelectKey() + 0))
        {
            for (int i = 0; i < 8; i++)
            {
                if (Global.GameInstance.CanUseReadyItem(i) == false)
                {

                    if (PlayerPrefs.GetInt(Global.GameInstance.GetReadyItemSelectKey() + i) == 1
                        && (i == 6 || i == 7))
                    {
                        //더블레디 체크 설정 되었으나, 더블레디를 못쓰는 레디창인 경우, 단일 체크 설정 켜기
                        int singleIndex = (i == 6) ? 0 : 1;

                        if (Global.GameInstance.CanUseReadyItem(singleIndex) == true)
                        {
                            //더블체크 제거
                            string prefsName_Double = Global.GameInstance.GetReadyItemSelectKey() + i;
                            PlayerPrefs.SetInt(prefsName_Double, 0);
                            readyItemSelectCount[i] = PlayerPrefs.GetInt(prefsName_Double);

                            //단일체크 추가
                            string prefsName_Single = Global.GameInstance.GetReadyItemSelectKey() + singleIndex;
                            PlayerPrefs.SetInt(prefsName_Single, 1);
                            readyItemSelectCount[singleIndex] = PlayerPrefs.GetInt(prefsName_Single);
                        }
                    }

                    continue;
                }

                string prefsName = Global.GameInstance.GetReadyItemSelectKey() + i;
                readyItemSelectCount[i] = PlayerPrefs.GetInt(prefsName);
            }
        }

        //게임 타입에 따른 레디 아이템 선택 설정.
        Global.GameInstance.SetSelectReadyItem();
    }

    /// <summary>
    /// 포코타 레디 아이템 선택 보이스
    /// </summary>
    public override AudioLobby GetReadyCharacterVoice()
    {
        return AudioLobby.NO_SOUND;
    }

    protected new void OnDestroy()
    {
        _instance = null;
        listTargetInfo.Clear();
        base.OnDestroy();
    }

    protected override void SetBoniModel()
    {
        int modelNo = (int)TypeCharacterType.Boni;

        float posY = LanguageUtility.IsShowBuyInfo ? -290f : -320f - 40f;

        boniLive2D = LAppModelProxy.MakeLive2D(mainSprite.gameObject, (TypeCharacterType)modelNo);

        bool flip = ManagerCharacter._instance._live2dObjects[modelNo].defaultScale < 0.0f;
        // 보니를 300사이즈로 만드는 게 기준이므로, 다른캐릭터로 할 때는 보니 크기에 비례해서 스케일 조정해줘야
        float scaleRatio = Mathf.Abs(ManagerCharacter._instance._live2dObjects[modelNo].defaultScale /
                                     ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.Boni]
                                         .defaultScale);
        boniLive2D.SetVectorScale(new Vector3(flip ? -300f : 300f, 300f, 300f) * scaleRatio);
        boniLive2D.SetPosition(new Vector3(221f, posY, 0f));

        boniLive2D.SetSortOrder(uiPanel.sortingOrder + 1);
        SettingLive2DAnimation();
    }

    public override void SetHardStage()
    {
        return;
    }

    protected override void SetStageFlower()
    {
        //꽃 이미지 표시하지 않는 타입인지 검사.
        if (Global.GameInstance.GetProp(GameTypeProp.FLOWER_ON_READY) == false)
            return;
    }
    
    protected virtual IEnumerator ShowCloverEffect()
    {
        yield return CoShowCloverEffect();
    }

    protected IEnumerator CoShowCloverEffect()
    {
        //클로버 연출 종료될 때까지 대기
        yield return startButton.CoShowCloverEffect();
        StartCoroutine(CoGoIngame());
    }

    protected virtual IEnumerator CoGoIngame()
    {  
        //인게임 진입 연출 종료될때까지 대기
        while (_recvGameStart_end == false)
        {
            yield return null;
        }

        touchButton = false;
        ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeSceneLoading("InGame");
        ManagerUI._instance.bTouchTopUI = true;
        yield return null;
    }

    protected override void OnClickGoInGame()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        if (startButton.StartButtonType == UIItemReadyBtnStart.StartType.CLOVER)
        {
            OnClickGoInGame_Clover(Global.GetTime());
        }
        else
        {
            OnClickGoInGame_Custom(Global.GetTime());
        }
    }

    private void OnClickGoInGame_Clover(long startTime)
    {
        if ((Global.clover > 0 || GameData.RemainFreePlayTime() > 0) && touchButton == false)
        {
            if (UIPopupPause._instance != null && UIPopupPause._instance.preservedRestart != null)
            {
                StartCoroutine(CoRestart(startTime));
            }
            else StartCoroutine(CoStartGame(startTime));
        }
        else if (Global.clover <= 0 && touchButton == false)
        {
            ManagerUI._instance.LackCloverProcess();
            bCanTouch = true;
        }
    }

    protected virtual void OnClickGoInGame_Custom(long startTime)
    {
        if (UIPopupPause._instance != null && UIPopupPause._instance.preservedRestart != null)
        {   //일시정지 팝업에서 게임시작 눌렀다면 재시작 처리
            StartCoroutine(CoRestart(startTime));
        }
        else
        {   //게임 시작
            bStartGame = true;
            StartGame(startTime);
        }
    }

    protected override void StartGame(long startTime)
    {
        ManagerSound.AudioPlay(AudioLobby.UseClover);
        
        if (startButton.StartButtonType == UIItemReadyBtnStart.StartType.CLOVER)
        {   //클로버 이펙트가 날아간 뒤, ShowUseClover 함수를 호출해 인게임 진입
            StartEffect_Clover();
        }
        else
        {   //서버 통신 완료될 때까지 대기했다가 게임 진입
            StartCoroutine(CoGoIngame());
        }

        #region 레디 아이템 사용 여부 저장
        getReadyItemCount = new int[]
        {
                ServerRepos.UserItem.ReadyItem(0),
                ServerRepos.UserItem.ReadyItem(1),
                ServerRepos.UserItem.ReadyItem(2),
                ServerRepos.UserItem.ReadyItem(3),
                ServerRepos.UserItem.ReadyItem(4),
                ServerRepos.UserItem.ReadyItem(5)
        };

        for (int i = 0; i < 8; i++)
        {
            string prefsName = Global.GameInstance.GetReadyItemSelectKey() + i;
            PlayerPrefs.SetInt(prefsName, readyItemSelectCount[i]);
        }

        for (int i = 0; i < 8; i++)
        {
            readyItemUseCount[i] = new EncValue();
            readyItemUseCount[i].Value = readyItemSelectCount[i];

            if (readyItemSelectCount[i] > 0 && Global.GameInstance.CanUseReadyItem(i) == false)
            {
                readyItemUseCount[i].Value = 0;
            }
        }

        if (tempData.gameMode == (int)GameMode.LAVA)
        {
            readyItemUseCount[0].Value = 0;
            readyItemUseCount[6].Value = 0;
        }

        for (int i = 0; i < 8; i++)
        {
            if (readyItemUseCount[i].Value > 0)
            {
                if (i < 3)
                {
                    //사과, 스코어업
                    if (getReadyItemCount[i] < readyItemUseCount[i].Value)
                    {
                        if (ServerRepos.LoginCdn.ReadyItems[i] < (int)(GameData.User.coin))
                        {
                            payCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                            freeCoin[i] = 0;
                        }
                        else if (ServerRepos.LoginCdn.ReadyItems[i] > (int)(GameData.User.coin) && (int)(GameData.User.coin) > 0)
                        {
                            payCoin[i] = (int)(GameData.User.coin);
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i] - (int)(GameData.User.coin);
                        }
                        else
                        {
                            payCoin[i] = 0;
                            freeCoin[i] = ServerRepos.LoginCdn.ReadyItems[i];
                        }
                    }
                }
                else if (Global.GameInstance.CanUseDoubleReadyItem() == true && (i == 6 || i == 7))
                {
                    int doubleIndex = SERVER_DOUBLEREADY_INDEX;
                    if (i == 7) doubleIndex++;

                    //더블 아이템 사과, 스코어업
                    if (ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] < (int)(GameData.User.coin))
                    {
                        payCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex];
                        freeCoin[i] = 0;
                    }
                    else if (ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] > (int)(GameData.User.coin) && (int)(GameData.User.coin) > 0)
                    {
                        payCoin[i] = (int)(GameData.User.coin);
                        freeCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex] - (int)(GameData.User.coin);
                    }
                    else
                    {
                        payCoin[i] = 0;
                        freeCoin[i] = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex];
                    }
                }
                else
                {
                    if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(i) == false && getReadyItemCount[i] < readyItemUseCount[i].Value)
                    {
                        int userCurrency = (int) (GameData.User.jewel);
                        int itemPrice = Global.GameInstance.GetItemCostList(ItemType.READY_ITEM)[i];
                        
                        if (Global.GameInstance.GetItemCostType(ItemType.READY_ITEM) == RewardType.coin)
                            userCurrency = (int) (GameData.User.coin);
                        
                        if (itemPrice < userCurrency)
                        {
                            payCoin[i] = itemPrice;
                            freeCoin[i] = 0;
                        }
                        else if (itemPrice > (int)(GameData.User.jewel) && userCurrency > 0)
                        {
                            payCoin[i] = userCurrency;
                            freeCoin[i] = itemPrice - userCurrency;
                        }
                        else
                        {
                            payCoin[i] = 0;
                            freeCoin[i] = itemPrice;
                        }
                    }
                }
            }
        }

        beforeFCoin = (int)(GameData.User.fcoin);
        beforeCoin = (int)(GameData.User.coin);
        beforeFDia = (int)(GameData.User.fjewel);
        beforeDia = (int)(GameData.User.jewel);

        // 0~5 레디아이템, 15~16 더블레디아이템 //6~14 인게임아이템, 컨티뉴횟수 등 : 서버측 동기화요청으로 0으로 설정
        var useItems = new int[] {
                    readyItemUseCount[0].Value,
                    readyItemUseCount[1].Value,
                    readyItemUseCount[2].Value,
                    readyItemUseCount[3].Value,
                    readyItemUseCount[4].Value,
                    readyItemUseCount[5].Value,
                    0, //6
                    0, //7
                    0, //8
                    0, //9
                    0, //10
                    0, //11
                    0, //12
                    0, //13
                    0, //14
                    readyItemUseCount[6].Value,
                    readyItemUseCount[7].Value };

        //유저의 레디 아이템 소모 없이 사용가능한 아이템 리스트 초기화
        for (int i = 0; i < 6; i++)
        {
            readyItemAutoUse[i] = new EncValue();
            readyItemAutoUse[i].Value = 0;
        }

        //시간제 레디 아이템 사용되어 있으면 무료로 사용 가능한 아이템 true 로 설정.
        for (int i = 0; i < readyItemTimeLimit.Length; i++)
        {
            if (readyItemTimeLimit[i] == true)
                readyItemAutoUse[i].Value = 1;
        }
        #endregion

        #region 광고 데이터 추가
        var listADItems = new List<int>();
        int itemCount = (int)GameBaseReq.AD_GameItem.AD_GAMEITEM_COUNT;
        for (int i = 0; i < itemCount; i++)
            listADItems.Add(0);

        if (isCanWatchAD_AddTurn == true && isSelectAD_AddTurn == true && addTurnCount_ByAD.Value > 0)
        {
            listADItems[(int)GameBaseReq.AD_GameItem.AD_ADD_ITEM] = 1;
        }
        #endregion

        int tempGameMode = (int)Global.GameType;

        var req = new GameStartReq()
        {
            ts = startTime,
            type = tempGameMode,
            stage = Global.stageIndex,
            eventIdx = Global.eventIndex,
            chapter = Global.GameInstance.GetChapterIdx(),
            items = useItems,
            adItems = listADItems,
        };

        ServerRepos.GameStartTs = startTime;    // 임시작업

        QuestGameData.SetUserData();

        Global.GameInstance.GameStart(req, recvGameStart, onFailStart);
    }

    private void StartEffect_Clover()
    {
        // UI 정지 시키고 .. 클로버 날리기
        touchButton = true;
        startButton.MakeFlyCloverEffect(clippingPanel.gameObject, this.gameObject);
            
        //클로버 사용하는 경우 클로버 감소.
        if (GameData.RemainFreePlayTime() <= 0)
        {
            Global.clover--;
            ManagerUI._instance.UpdateUI();
        }
    }

    protected override void recvGameStart(BaseResp code)
    {
        if (code.IsSuccess)
        {
            //게임 시작 설정
            ManagerLobby._stageStart = true;

            //스테이지 시작 버튼을 누를 때 레디아이템 보정 재화값 초기화
            ManagerUI._instance.InitActionCurrency();

            Global.clover = (int)GameData.Asset.AllClover;
            Global.coin = (int)GameData.Asset.AllCoin;
            Global.jewel = (int)GameData.Asset.AllJewel;
            Global.wing = (int)GameData.Asset.AllWing;
            Global.exp = (int)GameData.User.expBall;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();

            stageIndex = Global.stageIndex;
            QuestGameData.SetUserData();

            if (ManagerCoinStashEvent.CheckStartable())
                ManagerCoinStashEvent.currentCoinMultiplierState = ServerRepos.UserCoinStash.multiplier;

            _recvGameStart_end = true;

            //그로시 전송
            if (startButton.StartButtonType == UIItemReadyBtnStart.StartType.CLOVER)
            {
                SendGrowthy_UseClover();
            }

            SendGrowthy_UseReadyItem();
        }
    }
    
    private void SendGrowthy_UseClover()
    {
        if (GameData.RemainFreePlayTime() <= 0)
        {
            var useClover = new ServiceSDK.GrowthyCustomLog_Money
            (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_GAME_PLAY,
                0, //-usePCoin,
                -1, //-useFCoin,
                0,//(int)(ServerRepos.User.clover),
                (int)(ServerRepos.User.AllClover),//(int)(ServerRepos.User.fclover)
                mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
            );
            var cloverDoc = JsonConvert.SerializeObject(useClover);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", cloverDoc);
        }
        else
        {
            var useClover = new ServiceSDK.GrowthyCustomLog_Money
            (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_GAME_PLAY_FREE,
                0, //-usePCoin,
                0, //-useFCoin,
                0,//(int)(ServerRepos.User.clover),
                (int)(ServerRepos.User.AllClover),//(int)(ServerRepos.User.fclover)
                mrsn_DTL: Global.GameInstance.GetGrowthyGameMode().ToString()
            );
            var cloverDoc = JsonConvert.SerializeObject(useClover);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", cloverDoc);
        }
    }

    private void SendGrowthy_UseReadyItem()
    {
        for (int i = 0; i < 8; i++)
        {
            //RewardType 의 더블 아이템 인덱스 계산
            string rewardTypestring_ReadyItem = ((i <= 5)
                ? ((RewardType)((int)RewardType.readyItem1 + i)).ToString()
                : ((RewardType)((int)RewardType.readyItem7 + (i - 6))).ToString());

            if (readyItemUseCount[i].Value > 0)
            {
                if (listReadyItem[i].type == READY_ITEM_TYPE.LOCK) continue;

                // 1번, 2번, 6번, 7번 아이템이거나 보물찾기 게임 타입이라면 코인 그로시 전송
                bool isCoinItem = i < 3 || i == 6 || i == 7 || Global.GameType == GameType.TREASURE_HUNT;

                if (isCoinItem)
                {
                    if (payCoin[i] > 0 || freeCoin[i] > 0)
                    {
                        beforeCoin -= payCoin[i];
                        beforeFCoin -= freeCoin[i];

                        var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                        (
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                            -payCoin[i],
                            -freeCoin[i],
                            beforeCoin,
                            beforeFCoin,
                            "ReadyItem" + ((READY_ITEM_TYPE)i).ToString()
                        );
                        var docItem = JsonConvert.SerializeObject(playEnd);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docItem);

                        var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                            rewardTypestring_ReadyItem,
                            "ReadyItem" + listReadyItem[i].type.ToString(),
                            readyItemUseCount[i].Value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                            Global.GameInstance.GetGrowthyGameMode().ToString()
                        );
                        var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                    }
                }
                else
                {
                    if (payCoin[i] > 0 || freeCoin[i] > 0)
                    {
                        beforeDia -= payCoin[i];
                        beforeFDia -= freeCoin[i];

                        var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                        (
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                            -payCoin[i],
                            -freeCoin[i],
                            beforeDia,
                            beforeFDia,
                            "ReadyItem" + ((READY_ITEM_TYPE)i).ToString()
                        );
                        var docItem = JsonConvert.SerializeObject(playEnd);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docItem);

                        var useReadyItem1 = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                            rewardTypestring_ReadyItem,
                            "ReadyItem" + listReadyItem[i].type.ToString(),
                            readyItemUseCount[i].Value,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                            Global.GameInstance.GetGrowthyGameMode().ToString()
                        );
                        var doc1 = JsonConvert.SerializeObject(useReadyItem1);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc1);
                    }
                }

                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                    rewardTypestring_ReadyItem,
                    "ReadyItem" + listReadyItem[i].type.ToString(),
                    -readyItemUseCount[i].Value,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM,
                    Global.GameInstance.GetGrowthyGameMode().ToString()
                );
                var doc = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
            }
            else if (readyItemUseCount[i].Value == 0)
            {
                // 무료로 사용된 케이스 체크
                bool usedFree = false;
                if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(i))
                {
                    usedFree = true;
                }

                if (usedFree)
                {
                    var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                        rewardTypestring_ReadyItem,
                        "ReadyItem" + listReadyItem[i].type.ToString(),
                        0,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_FREE_ITEM,
                        Global.GameInstance.GetGrowthyGameMode().ToString()
                    );
                    var doc = JsonConvert.SerializeObject(useReadyItem);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
                }
            }
        }
    }

    //스테이지 정보 다시부르기
    protected override IEnumerator CoCheckStageDataBeforeOpenPopUpReady()
    {
        string stageName = Global.GameInstance.GetStageFilename();
        yield return ManagerUI._instance.CoCheckStageData(stageName);

        using (var www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName))
        {
            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                StringReader reader = new StringReader(www.downloadHandler.text);
                var serializer = new XmlSerializer(typeof(StageMapData));
                tempData = serializer.Deserialize(reader) as StageMapData;
            }
            else
            {
                NetworkLoading.EndNetworkLoading();
            }
        }
        yield return null;

        AddListTargetInfo(tempData.listTargetInfo);

        //레디 창 목표 아틀라스 재 조합.
        yield return ManagerUI._instance.CoMakeTargetAtlas(tempData.collectCount, tempData.collectColorCount, listTargetInfo);

        LoadTarget();
        SetStageFlower();
        SetStageLabel();
        SetHardStage();
        
        //연출 후 터치가능.
        StartCoroutine(OnTouch(0.5f));
        yield return null;
    }

    #region 게임 시작버튼 설정
    /// <summary>
    /// 시작 버튼 이미지 설정
    /// </summary>
    protected override void SetStartButton()
    {
        startButton.SetButton();
        
        //광고 선택 상태 확인
        if (sprStartADIcon_AddTurn != null)
        {
            sprStartADIcon_AddTurn.gameObject.SetActive(isSelectAD_AddTurn);
            if (isSelectAD_AddTurn == true)
            {
                InitSelectADTypeBtnStart();
            }
        }
    }

    protected override void InitSelectADTypeBtnStart()
    {
        Vector3 originScale = sprStartADIcon_AddTurn.transform.localScale;
        sprStartADIcon_AddTurn.transform.localScale = Vector3.zero;
        sprStartADIcon_AddTurn.transform.DOScale(originScale, 0.1f).SetEase(Ease.InOutBack);

        //버튼 및 스타트 텍스트 컬러 설정
        startButton.SetButtonColor(BtnColorType.yellow);
    }
    #endregion
}
