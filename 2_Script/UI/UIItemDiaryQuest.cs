using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;
using PokoAddressable;

public class UIItemDiaryQuest : MonoBehaviour
{
    public GameObject questIconBox;
    public GameObject progressObject;
    public GameObject btnGetObject;
    public GameObject btnGetButton;

    public UIUrlTexture reward;
    public UISprite mainSprite;
    public UISprite activeButton;
    public UIUrlTexture questIcon;
    public UILabel rewardText;
    public UILabel[] rewardCount;
    public UILabel questLevel;
    public UILabel questInfo;
    public UILabel questTime;
    public UILabel getText;
    public UILabel progressStep;
    public UIProgressBar progressBar;

    [SerializeField]
    private UIPokoButton shortcutButton;

    //이벤트.
    public GameObject eventObject;
    public UIUrlTexture eventIcon;
    public UILabel eventInfo;

    [HideInInspector]
    public QuestGameData qData = null;

    private float questRemoveTime = 0.3f;

    [SerializeField]
    private UILabel questIndexLabel;

    public void InitBtnQuest(QuestGameData in_data)
    {
        qData = in_data;

        SetShowQuestIndex();
        InitPopupText();

        //퀘스트 시간 표시(제한 시간이 있다면 이벤트 퀘스트, 없으면 일반 퀘스트).
        //퀘스트 duration 이나 valueTime 이 있을 경우, 타이머 시작.
        if (qData.valueTime1 > 1)
        {
            SettingEventQuest();
            StartCoroutine(CoQuestTimer(qData.valueTime1));
        }
        else
        {
            SettingDefaultQuest();
            if (qData.duration > 0)
            {
                StartCoroutine(CoQuestTimer(qData.duration));
            }
            else
            {
                questTime.gameObject.SetActive(false);
            }
        }

        //퀘스트 진행 카운트 설정(진행 수는 타겟 수를 넘지 않게 표시됨).
        int progCount = qData.prog1;
        if (progCount > qData.targetCount)
        {
            progCount = qData.targetCount;
        }

        progressStep.text = string.Format("{0}/{1}", progCount, qData.targetCount);

        int width = reward.width;
        int height = reward.height;
        reward.SuccessEvent += () => { reward.width = width; reward.height = height; };
        
        if (qData.rewardType == RewardType.animalOverlapTicket)
        {
            var path = RewardHelper.GetOverlapAddressablePath(qData.rewardCount);
            reward.gameObject.AddressableAssetLoad<Texture2D>(path, (texture) =>
            {
                reward.enabled     = true;
                reward.mainTexture = texture;
            });
        }
        else
        {
            RewardHelper.SetTexture(reward, qData.rewardType, qData.rewardCount);
        }
        
        //프로그레스 바 설정.
        float progressOffset = 100f / qData.targetCount;
        progressBar.value = (progCount * progressOffset) * 0.01f;

        //보상 수 설정.
        if (RewardHelper.IsRewardTypeTimeLimit(qData.rewardType))
        {
            string count = $"{qData.rewardCount / 60}{Global._instance.GetString("time_3")}";
            rewardCount[0].text = count;
            rewardCount[1].text = count;
        }
        else if (RewardHelper.IsActiveRewardCount(qData.rewardType) == false || qData.rewardType == RewardType.none)
        {
            rewardCount[0].text = "";
            rewardCount[1].text = "";
            rewardCount[0].gameObject.SetActive(false);
            rewardCount[1].gameObject.SetActive(false);

            if (qData.state == QuestState.Completed)
            {
                reward.transform.localPosition = new Vector3(8f, 2f, 0f);
            }
        }
        else
        {
            string count = string.Format("x{0}", qData.rewardCount);
            rewardCount[0].text = count;
            rewardCount[1].text = count;
        }

        //Get 버튼 설정.
        if (qData.state == QuestState.Completed)
        {
            btnGetButton.SetActive(true);
            StartCoroutine(CoQuestActive());
        }

        //ShortcutButton 설정
        if (Mathf.Approximately(progressBar.value, 1.0f))
        {
            shortcutButton.gameObject.SetActive(false);
        }
        else if ((int)qData.type == 1000 || (int)qData.type == 1002)   // 1000. 사탕,   1002. 오리 퀘스트
        {
            customStageIndex = ManagerData._instance.chapterData[qData.level - 1]._stageIndex;
            shortcutButton.functionName = "OpenPopupStage_CustomFind";
            shortcutButton.gameObject.SetActive(true);
        }
        else if ((int)qData.type == 18)  // 18. 윙 사용
        {
            shortcutButton.functionName = "OpenPopup_Adventure";
            shortcutButton.gameObject.SetActive(true);
        }
        else if ((int)qData.type == 2)  // 2. 꽃 피우기 퀘스트
        {
            shortcutButton.functionName = "OpenPopupStage_FloewFind";
            
            var isButtonActive = ManagerUI.IsLobbyButtonActive;
            shortcutButton.gameObject.SetActive(isButtonActive);
        }
        else if((int)qData.type == 13)  // 13. 파란꽃 피우기 퀘스트
        {
            shortcutButton.functionName = "OpenPopupStage_BlueFlowerFind";
            shortcutButton.gameObject.SetActive(true);
        }
        else if((int)qData.type == 1 || (int)qData.type == 3 || (int)qData.type == 5 || // 1. 별 획득   3. 클로버 사용    5. 라인 폭탄 만들기
                (int)qData.type == 6 || (int)qData.type == 7 || (int)qData.type == 8 || // 6. 더블 폭탄 7. 레인보우 폭탄  8. 폭탄 조합
                (int)qData.type == 162) // 162. 마지막 에피소드 클리어
        {
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            if (myProfile.stage <= ManagerData._instance.maxStageCount)
                shortcutButton.functionName = "OpenPopupReadyLastStage";
            else
                shortcutButton.functionName = "OpenPopupStage";

            var isButtonActive = ManagerUI.IsLobbyButtonActive;
            shortcutButton.gameObject.SetActive(isButtonActive);
        }
        else if ((int)qData.type == 4)   // 4. 마을에서 재료 수집하기
        {
            shortcutButton.functionName = "MoveCameraMaterial";
            
            var isButtonActive = ManagerUI.IsLobbyButtonActive;
            shortcutButton.gameObject.SetActive(isButtonActive);
        }
        else
        {
            shortcutButton.gameObject.SetActive(false);
        }
    }

    public IEnumerator CoQuestTimer(long timer)
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;

            questTime.text = Global.GetTimeText_DDHHMM(timer, true);

            if (Global.LeftTime(timer) <= 0)
            {
                break;
            }
            yield return null;
        }
    }

    private void SetShowQuestIndex()
    {
        //옵션에서 미션 번호 켜면, 해당 미션 인덱스 보이도록 해줌
        if (Global._instance.isShowIndex == true)
        {
            questIndexLabel.gameObject.SetActive(true);
            questIndexLabel.text = qData.index.ToString();
        }
        else
        {
            questIndexLabel.gameObject.SetActive(false);
        }
    }

    private void InitPopupText()
    {
        getText.text = Global._instance.GetString("btn_28");
        rewardText.text = Global._instance.GetString("btn_29");
    }

    private void SettingDefaultQuest()
    {
        eventObject.SetActive(false);
        questIconBox.SetActive(true);
        questInfo.gameObject.SetActive(true);

        //이미지 로드.
        string fileName = string.Format("q_{0}", (int)qData.type);
        questIcon.LoadCDN(Global.gameImageDirectory, "IconQuest/", fileName);

        //텍스트 세팅.
        questInfo.text = Global._instance.GetString(qData.info);
        if ((int)qData.type >= 1000)
        {
            questLevel.text = string.Format("ep.{0}", qData.level);
        }
        else
        {
            questLevel.text = string.Format("Lv.{0}", qData.level);
        }
    }

    private void SettingEventQuest()
    {
        eventObject.SetActive(true);
        questIconBox.SetActive(false);
        questInfo.gameObject.SetActive(false);

        //이미지 로드.
        string fileName = string.Format("e_{0}", (int)qData.type);
        eventIcon.LoadCDN(Global.gameImageDirectory, "IconQuest/", fileName);

        //텍스트 세팅.
        if ((int)qData.type == 162)
        {
            int lastEpisode = ManagerData.GetFinalChapter();
            eventInfo.text = Global._instance.GetString(qData.info).Replace("[n]", lastEpisode.ToString());
        }
        else
        {
            eventInfo.text = Global._instance.GetString(qData.info);
        }
    }

    private IEnumerator CoQuestActive()
    {
        float color = 70f / 255f;
        activeButton.enabled = true;
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;

            //터치 불가 상태일 때는 멈춤.
            if (UIDiaryMission._instance != null && UIDiaryMission._instance.bClearButton == true)
                break;

            float ratio = (0.2f + Mathf.Cos(Time.time * 10f) * 0.1f);
            activeButton.color = new Color(color, color, color, ratio);
            yield return null;
        }
        yield return null;
        activeButton.enabled = false;
    }

    void OnClickBtnReward()
    {
        if (qData.state != QuestState.Completed)
            return;


        if (!CheckbCanTouch())
            return;
        SetbCanTouch(false);

        if (qData.rewardType == RewardType.boxSmall || qData.rewardType == RewardType.boxMiddle || qData.rewardType == RewardType.boxBig)
        {
            if (ObjectGiftbox._giftboxList.Count >= 2)
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();

                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_2"), false, () => { SetbCanTouch(true); });
                popupSystem.SetResourceImage("Message/tired");
                return;
            }
        }

        ServerAPI.GetQuestGetReward(qData.index, recvQuestGetReward);
    }

    void recvQuestGetReward(UserQuestsGetRewardResp resp)
    {
        if (resp.IsSuccess)
        {
            //서버 통신 성공 시 :DestroyQuest() -> UIDiaryMission.DoBtnQuest() 
            // -> UIDiaryMission.DoQuestAction() 에서 MoveBtnQuestPos() 후 터치 가능상태로 변경.
            //그로씨            
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    (int)qData.rewardType,
                    (int)qData.rewardCount,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_QUEST_REWARD,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_QUEST_REWARD,
                    "q_" + qData.index.ToString()
                    );  

                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.QUEST,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.HOBBY_TIME,
                    "q_" + qData.index.ToString(),
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
                var doc = JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
            }
            if ( UIDiaryMission._instance != null )
                UIDiaryMission._instance.bClearButton = true;

            if (resp.userGiftBoxes != null && resp.userGiftBoxes.Count > 0)
            {
                ManagerLobby._instance.ReMakeGiftbox();

                for (var i = 0; i < resp.userGiftBoxes.Count; i++)
                {
                    LocalNotification.GiftBoxNotification(resp.userGiftBoxes[i].index, (int)resp.userGiftBoxes[i].openTimer - (int)GameData.GetTime());
                }
            }

            Global.star = (int)GameData.Asset.Star;
            Global.clover = (int)GameData.Asset.AllClover;
            Global.coin = (int)GameData.Asset.AllCoin;
            Global.jewel = (int)GameData.Asset.AllJewel;
            Global.wing = (int)GameData.Asset.AllWing;
            Global.exp = (int)GameData.User.expBall;
            Global.flower = (int)GameData.User.flower;
            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();
            MaterialData.SetUserData();
            if (resp.userHousing != null)
            {
                PlusHousingModelData.SetUserData();
            }

            DestroyQuest();

            if( resp.toy != null)
            {
                PokoyuraData.SetUserData();
                ManagerLobby._instance.ReMakePokoyura();
            }

            Method.FunctionVoid func = null;
            //스티커 획득 최초플레이확인(보상 타입이 스티커이고 퀘스트 인덱스 2인 경우)
            if (qData.rewardType == RewardType.stamp && qData.index == 2)
            {
                func = CallBackStartStickerTutorial;
            }
            else if( resp.levelUp )
            {
                func = CallBackOpenPopupRankup;

                if (resp.toy != null && resp.toy.Count > 0)
                {
                    rankupToyIndex = resp.toy[0].index;
                }
                else
                {
                    rankupToyIndex = 0;
                }
                //포코유라 업데이트.
                ManagerUI._instance.SettingRankingPokoYura();
            }
            else if( qData.rewardType == RewardType.housing)
            { 
                func = CallBackOpenPopupHousing;
                if (resp.userHousing != null)
                {
                    housingItemIndex = resp.userHousing.index * 10000;
                    housingItemIndex += resp.userHousing.modelIndex;

                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        (int)RewardType.housing,
                        housingItemIndex,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.NULL,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_QUEST_REWARD,
                        null
                        );
                }
            }
            else if (qData.rewardType == RewardType.costume)
            {
                func = ManagerUI._instance.ClosePopupAndOpenPopupCostume;
                UIDiaryController._instance.UpdateCostumeData();
            }
            else if ( qData.rewardType == RewardType.animal )
            {
                ManagerAdventure.UserDataAnimal getAnimal = new ManagerAdventure.UserDataAnimal()
                {
                    animalIdx = resp.userAnimal.animalId,
                    exp = resp.userAnimal.exp,
                    gettime = 0,
                    grade = resp.userAnimal.grade,
                    level = resp.userAnimal.level,
                    overlap = resp.userAnimal.Overlap
                };

                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    (int)RewardType.animal,
                    resp.userAnimal.animalId,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.NULL,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_QUEST_REWARD,
                    null
                    );  

                func = () =>
                {
                    ManagerAdventure.OnInit((b) =>
                    {
                        var newAnimal = ManagerAdventure.User.GetAnimalInstance(getAnimal);
                        ManagerAdventure.User.SyncFromServer_Animal();
                        ManagerAIAnimal.Sync();

                        UIPopupAdventureSummonAction summonPopup = ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, newAnimal, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);
                    });
                };
            }
            else if (qData.rewardType >= RewardType.material)
            {
                UIDiaryController._instance.UpdateProgressHousingData();
            }

            //다이어리 알림 갱신.
            UIDiaryController._instance.OnClickBtnGetQuest(qData);
            UIDiaryController._instance.SaveQuestProgressPlayerPrefs(false);
            
            //튜토리얼 시에는 메세지 안뜸.
            if (ManagerTutorial._instance != null)
                return;

            if (qData.rewardType == RewardType.animalOverlapTicket)
            {
                var appliedRewardSet = new AppliedRewardSet()
                {
                    mailReceived = resp.mailReward
                };
                
                ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, appliedRewardSet);
            }
            else
            {
                string text = SettingGetRewardText(RewardHelper.GetRewardName(qData.rewardType, qData.rewardCount));
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                popupSystem.SetButtonText(0, Global._instance.GetString("btn_1"));
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false, func);
                // SetImage를 사용할 경우 퀘스트 아이템이 사라지며 이미지가 깨지는 이슈가 있어 RewardHelper 사용
                popupSystem.SetImage_UseRewardHelper(qData.rewardType, qData.rewardCount);
                ManagerSound.AudioPlay(AudioInGame.CONTINUE);
            }
            
            if ( UIDiaryMission._instance != null )
                UIDiaryMission._instance.bClearButton = false;
        }
    }

    int housingItemIndex = -1;
    void CallBackOpenPopupHousing()
    {
        ManagerUI._instance.ClosePopupAndOpenPopupHousing(housingItemIndex, 2);
    }

    int rankupToyIndex = 0;
    void CallBackOpenPopupRankup()
    {
        ManagerUI._instance.OpenPopupRankUp(ServerLogics.UserLevelWithFlower(), rankupToyIndex);
    }

    void CallBackStartStickerTutorial()
    {
        ManagerTutorial.PlayTutorial(TutorialType.TutorialGetSticker);
    }

    string SettingGetRewardText(string rewardText)
    {
        //퀘스트 보상 받음.
        string text = Global._instance.GetString("n_q_1");

        text = text.Replace("[1]", rewardText);
        
        //수량 표기 여부에 따라 설정.
        if (RewardHelper.IsActiveRewardCount(qData.rewardType))
        {
            text = text.Replace("[n]", rewardCount[0].text);
        }
        else
        {
            text = text.Replace("[n]", "");
        }

        return text;
    }

    void DestroyQuest()
    {
        if (UIDiaryMission._instance != null)
            UIDiaryMission._instance.DoBtnQuest(qData.index, questRemoveTime);
        else if (UIPopupEventQuest._instance != null)
            UIPopupEventQuest._instance.DoBtnQuest(qData.index, questRemoveTime);
        QuestGameData.SetUserData();
    }

    public void OpenPopupReadyLastStage()
    {
        if (CheckbCanTouch() == false)
            return;

        ManagerUI._instance.OpenPopupReadyLastStage();
    }

    public void OpenPopupStage()
    {
        if (CheckbCanTouch() == false)
            return;

        ManagerUI._instance.OpenPopupStage();
    }
    public void OpenPopupStage_FloewFind()
    {
        if (CheckbCanTouch() == false)
            return;

        ManagerUI._instance.OpenPopupStage(UIReuseGrid_Stage.ScrollMode.FlowerFind);
    }
    public void OpenPopupStage_BlueFlowerFind()
    {
        if (CheckbCanTouch() == false)
            return;

        ManagerUI._instance.OpenPopupStage(UIReuseGrid_Stage.ScrollMode.BlueFlowerFind);
    }

    private int customStageIndex = 0;
    public void OpenPopupStage_CustomFind()
    {
        if (CheckbCanTouch() == false)
            return;

        ManagerUI._instance.OpenPopupStage(customStageIndex);
    }

    public void OpenPopup_Adventure()
    {
        if (CheckbCanTouch() == false)
            return;

        ManagerUI._instance.OpenPopupStageAdventure();
    }

    public void MoveCameraMaterial()
    {
        if (CheckbCanTouch() == false)
            return;

        if (ManagerLobby.landIndex != 0)
        {
            ManagerLobby._instance.MoveMainLand(() =>
            {
                ManagerLobby._instance.MoveCameraMaterial();
            });

            return;
        }

        ManagerLobby._instance.MoveCameraMaterial();
    }

    private bool CheckbCanTouch()
    {
        if (UIPopupDiary._instance != null)
            return UIPopupDiary._instance.bCanTouch;
        else if (UIPopupEventQuest._instance != null)
            return UIPopupEventQuest._instance.bCanTouch;
        else
            return true;
    }

    private void SetbCanTouch(bool bCanTouch)
    {
        if (UIPopupDiary._instance != null)
            UIPopupDiary._instance.bCanTouch = bCanTouch;
        else if (UIPopupEventQuest._instance != null)
            UIPopupEventQuest._instance.bCanTouch = bCanTouch;
    }
}
