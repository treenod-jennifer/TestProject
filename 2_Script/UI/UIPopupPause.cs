using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;


public class UIPopupPause : UIPopupBase
{
    static public UIPopupPause _instance = null;

    public UILabel[] title;
    public UILabel stageText;

    public GameObject btnClose;
    public GameObject btnContinue;
    public GameObject btnMap;
    public GameObject btnRetry;

    public UISprite spriteContinue;
    public UISprite spriteMap;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Start()
    {
        ManagerSound._instance.PauseBGM();

        if (Global._systemLanguage == CurrLang.eJap)
        {
            stageText.text = string.Format("ステージ {0}", Global.stageIndex);    //UIPopupReady.stageIndex
        } 
        else
        {
            stageText.text = string.Format("STAGE {0}", Global.stageIndex);
        }
        stageText.MakePixelPerfect();

        if (Global.eventIndex > 0)
        {
            SettingPauseButtons();
        }
    }

    void SettingPauseButtons()
    {
        mainSprite.height = 480;
        title[0].transform.localPosition = new Vector3(-2f, 177f, 0f);
        btnClose.transform.localPosition = new Vector3(330f, 183f, 0f);
        btnContinue.transform.localPosition = new Vector3(0f, 32f, 0f);
        btnMap.transform.localPosition = new Vector3(0f, -128f, 0f);
        stageText.transform.localPosition = new Vector3(0f, 275f, 0f);

        spriteContinue.height = 130;
        spriteMap.height = 130;

        btnRetry.gameObject.SetActive(false);
    }


    void OnClickBtnMenu()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        //if (Global.eventIndex > 0)
        //{
        //    ManagerLobby._eventStageFail = true;
        //    UIPopupReady.eventStageFail = true;
        //}

        //팝업 다 지우고 로비로 이동.
        ManagerSound._instance.StopBGM();
        //SceneLoading.MakeSceneLoading("Lobby");
        
        //그로씨
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

            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD growthyStageType = Global.eventIndex == 0 ? ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.NORMAL : ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT;

            var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
                (
                ManagerData._instance.userData.userID,
                (ManagerData._instance.userData.stage - 1).ToString(),
                GameManager.instance.GrowthyAfterStage.ToString(),
                UIPopupReady.stageIndex.ToString(),//Global.stageIndex.ToString(),  
                growthyStageType,
                ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.DRAW,
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
                Global.eventIndex
                );

            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);*/
        }



        var usedItems = new List<int>();
        usedItems.Add(UIPopupReady.readyItemUseCount[0].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[1].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[2].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[3].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[4].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[5].Value);
        usedItems.Add(GameItemManager.useCount[0]);
        usedItems.Add(GameItemManager.useCount[1]);
        usedItems.Add(GameItemManager.useCount[2]);
        usedItems.Add(GameItemManager.useCount[3]);
        usedItems.Add(GameManager.instance.useContinueCount);

        var remainTarget = ManagerBlock.instance.GetListRemainTarget();
    }


void OnClickBtnRestart()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        ManagerUI._instance.CoShowUI(0.1f, true, TypeShowUI.eTopUI);

        //그로씨
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

            ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD growthyStageType = Global.eventIndex == 0 ? ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.NORMAL : ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.EVENT;

            var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
                (
                ManagerData._instance.userData.userID,
                (ManagerData._instance.userData.stage - 1).ToString(),
                GameManager.instance.GrowthyAfterStage.ToString(),
                UIPopupReady.stageIndex.ToString(),//Global.stageIndex.ToString(),  
                growthyStageType,
                ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.DRAW,
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
                Global.eventIndex
                );

            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);*/
        }

        var usedItems = new List<int>();
        usedItems.Add(UIPopupReady.readyItemUseCount[0].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[1].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[2].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[3].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[4].Value);
        usedItems.Add(UIPopupReady.readyItemUseCount[5].Value);
        usedItems.Add(GameItemManager.useCount[0]);
        usedItems.Add(GameItemManager.useCount[1]);
        usedItems.Add(GameItemManager.useCount[2]);
        usedItems.Add(GameItemManager.useCount[3]);
        usedItems.Add(GameManager.instance.useContinueCount);
    }



    void OnTouch()
    {
        if(this.gameObject != null)
            this.bCanTouch = true;
    }

    void CoShowTopUIFalse()
    {
        ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eTopUI);
    }

    IEnumerator CoRestartGame()
    {
        string stageKey = "pp" + Global.stageIndex + ".xml";
        string stageName = Global.GetHashfromText(stageKey) + ".xml";

        if (Global.eventIndex > 0)
            stageName = "E" + Global.eventIndex +"_" +Global.stageIndex + ".xml";

        WWW www = new WWW(Global.FileUri + Global.StageDirectory + stageName);
        yield return www;

        if (www.error == null)
        {
            StringReader reader = new StringReader(www.text);
            var serializer = new XmlSerializer(typeof(StageMapData));
            StageMapData tempData = serializer.Deserialize(reader) as StageMapData;

            ManagerUI._instance.OpenPopupReady(tempData);

            yield return null;
        }
    }

    public void ClosePopUp()
    {
        OnClickBtnClose();
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;
        ManagerSound._instance.PlayBGM();
        ManagerUI._instance.ClosePopUpUI();
    }
}
