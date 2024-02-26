using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UIPopupFail : UIPopupBase
{
    public static UIPopupFail _instance = null;

    public UIIngameResultChat resultChat;
    
    public UILabel _labelStage;
    public UILabel failText;
    public GameObject _trTarget;
    public GameObject[] failButton;

    List<StageTarget> stageTargets = new List<StageTarget>();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 만 레이어 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }
        if (UIPopupContinue._instance != null && UIPopupContinue._instance.boniLive2D != null)
        {
            UIPopupContinue._instance.boniLive2D._CubismRender.SortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUp()
    {
        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eTopUI);


        if (Global._systemLanguage == CurrLang.eJap)
        {
            _labelStage.text = string.Format("ステージ {0}", Global.stageIndex);
        }
        else
        {
            _labelStage.text = string.Format("STAGE {0}", Global.stageIndex);
        }
        _labelStage.MakePixelPerfect();

        if (Global.eventIndex > 0)
        {
            SettingFailButton();
            failText.text = Global._instance.GetString("n_ev_3");
        }
        else
        {
            failText.text = Global._instance.GetString("p_sf_4");
        }

        SetTarget();  //목표 설정.


        //Growthy 그로씨        
        {/*
            List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM> itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
            for (int i = 0; i < 6; i++)
            {
                if (UIPopupReady.readyItemUseCount[i].Value > 0)
                {
                    var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                    {
                        L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_LOBBY.ToString(),
                        L_IID = ((READY_ITEM_TYPE)i).ToString(),
                        L_CNT = UIPopupReady.readyItemUseCount[i].Value
                    };
                    itemList.Add(readyItem);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (GameItemManager.useCount[i] > 0)
                {
                    var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                    {
                        L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                        L_IID = ((GameItemType)i + 1).ToString(),
                        L_CNT = GameItemManager.useCount[i]
                    };
                    itemList.Add(readyItem);
                }
            }
            if (GameManager.instance.useContinueCount > 0)
            {
                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                    L_IID = "Continue",
                    L_CNT = GameManager.instance.useContinueCount
                };
                itemList.Add(readyItem);
            }
            var docItem = JsonConvert.SerializeObject(itemList);

          
            var remainTarget = new List<int>();
            for (int j = 0; j < ManagerBlock.instance.collectCount.Length; j++)
            {
                if (j == 0)
                {
                    if (ManagerBlock.instance.crackTotalCount == 0)
                    {
                        remainTarget.Add(0);
                    }
                    else
                    {
                        int count = ManagerBlock.instance.crackTotalCount - ManagerBlock.instance.pangBlockCount[0];
                        remainTarget.Add(count);
                    }
                }
                else
                {
                    if (ManagerBlock.instance.collectCount[j] == 0)
                    {
                        remainTarget.Add(0);
                    }
                    else
                    {
                        int count = ManagerBlock.instance.collectCount[j] - ManagerBlock.instance.pangBlockCount[j];
                        remainTarget.Add(count);
                    }
                }
            }

            for (int j = 0; j < ManagerBlock.instance.collectColorCount.Length; j++)
            {
                if (ManagerBlock.instance.collectColorCount[j] == 0)
                {
                    remainTarget.Add(0);
                }
                else
                {
                    int count = ManagerBlock.instance.collectColorCount[j] - ManagerBlock.instance.pangBlockColorCount[j];
                    remainTarget.Add(count);
                }
            }
            var docRemainGoals = JsonConvert.SerializeObject(remainTarget);


            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD growthyStageType = Global.eventIndex == 0 ? ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.NORMAL : ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT;

            var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
                (
                ManagerData._instance.userData.userID,
                (ManagerData._instance.userData.stage - 1).ToString(),
                GameManager.instance.GrowthyAfterStage.ToString(),
                UIPopupReady.stageIndex.ToString(),//Global.stageIndex.ToString(),               
                growthyStageType,
                ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.LOSE,
                0,
                ManagerBlock.instance.score,
                0,
                (long)(Time.time - GameManager.instance.playTime),
                GameManager.instance.firstPlay,
                GameManager.instance.useContinueCount > 0,
                GameManager.instance.leftMoveCount,   //남은턴 다시계산
                docItem,
                0,
                false,
                false,
                null,
                Global.eventIndex,
                docRemainGoals
                );

            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);

            //Debug.Log("목표 =" + docRemainGoals);*/
        }

        /*
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (UIPopupReady.stageIndex >= 2)
            {
                InitResultChat();
            }
        }
        resultChat.SetCollider(false);
        */

        //_resultChatUI = NGUITools.AddChild(ManagerGameUI.instanse._objChatRoot, _objChatRoot).GetComponent<ResultChatUI>();
        //_resultChatUI._trResultChat.localScale = Vector3.one;
        //_resultChatUI.InitChat("ingame_failed_boni", (_uiPanel.depth + _nPanelCount));
        //_stageData = ReadyPopUpUI.tempData;
        //SetPosition();  //목표 위치 설정.
        //GlobalGameManager.instance._nInGameResult = 0;
        //SetCharacter();

        ManagerSound.AudioPlay(AudioInGame.STAGE_FAIL2);
        ManagerSound._instance.PauseBGM();
    }

    public void SetTarget()
    {
        var enumerator = ManagerBlock.instance.dicCollectCount.GetEnumerator();
        while (enumerator.MoveNext())
        {
            TARGET_TYPE targetType = enumerator.Current.Key;
            if (enumerator.Current.Value != null)
            {
                string targetName = (targetType != TARGET_TYPE.COLORBLOCK) ?
                    string.Format("StageTarget_{0}", targetType) : "StageTarget";

                var e = enumerator.Current.Value.GetEnumerator();
                while (e.MoveNext())
                {
                    BlockColorType colorType = e.Current.Key;

                    StageTarget target = NGUITools.AddChild(_trTarget, ManagerUI._instance._objInGameTarget).GetComponent<StageTarget>();
                    target.targetType = targetType;
                    target.targetColor = colorType;

                    //목표 수 표시
                    int remainCount = e.Current.Value.collectCount - e.Current.Value.pangCount;
                    if (remainCount <= 0)
                    {
                        target.targetCount.gameObject.SetActive(false);
                        target.targetCountShadow.gameObject.SetActive(false);
                        target.checkSprite.gameObject.SetActive(true);
                        target.checkSprite.depth = 5;
                    }
                    else
                    {
                        string remainCountText = remainCount.ToString();
                        target.targetCount.text = remainCountText;
                        target.targetCountShadow.text = remainCountText;
                    }

                    //목표 이미지 설정
                    string targetColorName = (colorType != BlockColorType.NONE) ?
                        string.Format("{0}_{1}", targetName, colorType) : targetName;
                    target.targetSprite.spriteName = targetColorName;
                    target.targetSprite.MakePixelPerfect();

                    stageTargets.Add(target);
                }
            }
        }

        float startPos = (1 - stageTargets.Count) * 48;
        for (int i = 0; i < stageTargets.Count; i++)
        {
            stageTargets[i].transform.localPosition = new Vector3(startPos + 96 * i, -45, 0);
        }    
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeSceneLoading("Lobby");
    }

    public void SettingFailButton()
    {
        failButton[0].transform.localPosition = new Vector3(-10f, failButton[0].transform.localPosition.y, 0f);
        failButton[1].SetActive(false);
    }

    void OnClickBtnRetry()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        //레디창.
        ManagerUI._instance.OpenPopupReadyStageCallBack(OnTouch);
    }

    void OnTouch()
    {
        this.bCanTouch = true;
    }
}
