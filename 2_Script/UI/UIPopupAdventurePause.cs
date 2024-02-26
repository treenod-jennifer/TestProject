using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;

public class UIPopupAdventurePause : UIPopupBase
{
    [SerializeField] private UILabel stage;
    [SerializeField] private GenericReward[] rewards;

    //public override void OpenPopUp(int depth)
    //{
    //    InitData();
    //    base.OpenPopUp(depth);
    //}

    public void InitPopup(string stageData, Reward[] rewardsData)
    {
        stage.text = stageData;

        /*Test
        rewardsData = new Reward[2];
        rewardsData[0] = new Reward();
        rewardsData[0].type = (int)RewardType.coin;
        rewardsData[0].value = 11;
        rewardsData[1] = new Reward();
        rewardsData[1].type = (int)RewardType.clover;
        rewardsData[1].value = 22;
        */

        for(int i = 0; i<rewards.Length; i++)
        {
            if (rewardsData != null && i < rewardsData.Length)
            {
                rewards[i].SetReward(rewardsData[i]);
                rewards[i].gameObject.SetActive(true);
            }
            else
            {
                rewards[i].gameObject.SetActive(false);
            }
        }
    }

    void OnClickBtnMenu()
    {
        if (bCanTouch == false)
            return;
        bCanTouch = false;

        var arg = new AdventureGameCancelReq()
        {
            type = (int)Global.GameType,
            eventIdx = Global.eventIndex,
            stage = Global.stageIndex,
            chapter = Global.chapterIndex,
            seed = ServerRepos.IngameSeed,
        };

        ServerAPI.AdventureGameCancel(arg, RecvAdventureGameCancel);
    }

    private void RecvAdventureGameCancel(AdventureGameCancelResp resp)
    {
        if (resp.IsSuccess)
        {
            ManagerUI._instance.CoShowUI(0f, false, TypeShowUI.eAll);
            ManagerSound._instance.StopBGM();
            SceneLoading.MakeSceneLoading("Lobby");

            UIPopupStageAdventure.startChapter = Global.chapterIndex;

            // Growthy 그로씨
            // 사용한 인게임 아이템
            var itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
            for (var i = 4; i < 7; i++)
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
            // 컨티뉴 횟수
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

            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            var playEnd = new ServiceSDK.GrowthyCustomLog_PLAYEND
            (
                myProfile.userID,
                (myProfile.stage - 1).ToString(),
                GameManager.instance.GrowthyAfterStage.ToString(),
                Global.GameInstance.GetGrowthyStageIndex(),
                (Global.GameType == GameType.ADVENTURE_EVENT)
                    ? ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE_EVENT
                    : ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_GMOD.ADVENTURE,
                ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_PWIN.DRAW,
                0,
                0,
                0,
                (long)(Time.time - GameManager.instance.playTime),
                GameManager.instance.firstPlay,
                GameManager.instance.useContinueCount > 0,
                0, //남은턴 다시계산
                docItem,
                0,
                false,
                false,
                null,
                Global.GameInstance.GetGrowthy_PLAYEND_L_NUM3(),
                null,
                "N",
                boostLevel: "0",
                firstFlowerLevel: "N",
                usedTurn: GameManager.instance.useMoveCount,
                continueReconfirm: GameManager.instance.continueReconfirmCount,
                detailInfo: null
            );

            // 사용동물
            var animalList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL>();
            for (var i = 0; i < 3; i++)
            {
                var animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);

                var readyItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ANIMAL
                {
                    L_CID = animalData.idx,
                    L_LEV = animalData.level,
                    L_CNT = animalData.overlap
                };
                animalList.Add(readyItem);
            }
            var docAnimal = JsonConvert.SerializeObject(animalList);
            playEnd.L_CHAR = docAnimal;

            var doc = JsonConvert.SerializeObject(playEnd);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("PLAYEND", doc);
        }
    }
}
