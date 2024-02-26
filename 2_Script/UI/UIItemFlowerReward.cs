using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;

public class UIItemFlowerReward : MonoBehaviour
{
    public UILabel[] epName;

    //프로그레스바.
    public UIProgressBar progressBar;
    public UILabel progressStep;

    //보상.
    public UISprite rewardSprite;
    public UIUrlTexture rewardTexture;
    public UILabel[] rewardCount;

    public GenericReward genericReward;

    //보상 버튼
    public GameObject btnReward;
    public UILabel[] buttonText;
    public UILabel rewardText;
    public UISprite getCheck;

    public UISprite itemBack;
    public UISprite sprFlower;

    //보상 내용.
    private RewardType type = RewardType.none;
    private int value = 0;

    FlowerRewardUIData item;

    int texLoadingRef = 0;
    bool needTexReload = false;
    

    private void Start()
    {
        InitItemText();
    }

    private void InitItemText()
    {
        string btnText = Global._instance.GetString("btn_28");
        for (int i = 0; i < buttonText.Length; i++)
        {
            buttonText[i].text = btnText;
        }
        rewardText.text = Global._instance.GetString("btn_29");
    }

    public void UpdateData(FlowerRewardUIData flowerRewardData)
    {
        item = flowerRewardData;
        InitEpName(item.Index + 1);
        InitProgress(item.blueFlowerCnt, item.stageCnt);
        
        type = item.rewardType;
        value = item.rewardValue;

        //GenericReward 사용으로 사용을 하지않게 됨.
        //InitReward();

        genericReward.countIncludeX = false;
        genericReward.SetReward(new Reward() { type = (int)type, value = value });

        bool bCollectFlower = item.blueFlowerCnt >= item.stageCnt;
        bool bGetReward = ManagerData._instance.IsGetRewardState(item.rewardState, item.flowerType);
        InitGetButton(bCollectFlower, bGetReward);

        SetFlowerSprite();
    }

    private void InitEpName(int chapterNum)
    {
        string nameText = string.Format("ep.{0}", chapterNum);
        for (int i = 0; i < epName.Length; i++)
        {
            epName[i].text = nameText;
        }
    }

    private void InitProgress(int flowerCount, int stageCnt)
    {  
        //프로그레스 바 설정.
        float progressOffset = 100f / stageCnt;
        progressBar.value = (flowerCount * progressOffset) * 0.01f;
        //프로그레스 카운트 설정.
        progressStep.text = string.Format("{0}/{1}", flowerCount, stageCnt);
    }

    //GenericReward 사용으로 사용을 하지않게 됨.
    private void InitReward()
    {
        InitRewardImage(type, value);
        InitRewardCount();
    }

    private void InitRewardImage(RewardType type, int value)
    {
        //모든 보상 이미지가 투명하게 설정했다가 텍스쳐 SettingEndCallBack이 성공하면 투명값을 원래대로 되돌림.
        rewardTexture.color = new Color(1, 1, 1, 0);

        if( this.texLoadingRef != 0)
        {
            // 뭔가 이미 로딩중이었으면 플래그만 켜두고 나중에 처리
            this.needTexReload = true;
            return;
        }

        rewardTexture.SuccessEvent += () =>
        { 
            OnRewardTextureLoadEnd(type);
            rewardTexture.color = new Color(1, 1, 1, 1);
        };

        //RewardHelper.SetRewardImage(new Reward() { type = (int)type, value = value }, rewardSprite, rewardTexture, null, 1.0f);

        genericReward.SetReward(new Reward() { type = (int)type, value = value });

        if( rewardSprite.enabled == false && rewardTexture.enabled == true)
        {
            // 스프라이트가 꺼져있고 텍스쳐가 켜져있으면 텍스쳐쪽을 사용하도록 처리됐다는 뜻이고
            // 로딩 레퍼런스를 하나 올려준다. bool을 사용하지 않고 ref를 사용하는 이유는 
            /// 만약 이미 로딩되어있었던 경우에, 위에서 바로 End 콜백이 불려버리기 때문임
            
            texLoadingRef++;
        }
        
        if (type == RewardType.none)
        {
            rewardSprite.gameObject.SetActive(false);
            buttonText[0].transform.localPosition = new Vector3(-7f, 3f, 0f);
        }
        else if (type == RewardType.cloverFreeTime)
        {
            rewardSprite.transform.localPosition = new Vector3(-20f, 15f, 0f);
        }

        rewardSprite.transform.localPosition = Vector3.zero;
    }

    void OnRewardTextureLoadEnd(RewardType type)
    {
        texLoadingRef--;
        
        if( needTexReload && type != item.rewardType)
        {
            // needTexReload는 이미 로딩되어있었던 경우에는 절대 true일 수 없음
            // 그래서 재귀처리되듯이 불릴 일은 없음

            this.InitRewardImage(item.rewardType, item.rewardValue);
        }
        needTexReload = false;
    }

    private void InitRewardCount()
    {
        if (type == RewardType.stamp || type == RewardType.toy
            || type == RewardType.costume || type == RewardType.housing
            || type == RewardType.animal || type == RewardType.gachaTicket)
        {
            for (int i = 0; i < rewardCount.Length; i++)
            {
                rewardCount[i].gameObject.SetActive(false);
            }
            return;
        }

        for (int i = 0; i < rewardCount.Length; i++)
        {
            rewardCount[i].text = value.ToString();
            if(rewardCount[i].gameObject.activeInHierarchy == false)
                rewardCount[i].gameObject.SetActive(true);
        }
    }

    private void InitGetButton(bool bCollectFlower, bool bGet)
    {
        rewardText.gameObject.SetActive(!bCollectFlower);
        if (bCollectFlower == false)
        {
            btnReward.gameObject.SetActive(false);
            getCheck.gameObject.SetActive(false);
            itemBack.enabled = false;
        }
        else
        {
            btnReward.gameObject.SetActive(!bGet);
            getCheck.gameObject.SetActive(bGet);
            itemBack.enabled = bGet;
        }
    }

    private void OnClickBtnGet()
    {
        switch (item.flowerType)
        {
            case 3:
                ServerAPI.GetBlueFlowerReward((item.Index + 1), 1, recvGetFlowerReward);
                break;
            case 4:
                ServerAPI.GetBlueFlowerReward((item.Index + 1), 2, recvGetFlowerReward);
                break;
            case 5:
                ServerAPI.GetBlueFlowerReward((item.Index + 1), 3, recvGetFlowerReward);
                break;
            default:
                break;
        }
    }

    void recvGetFlowerReward(GetBlueFlowerRewardResp resp)
    {
        if (resp.IsSuccess)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
            (
                rewardType: (int)item.rewardType,
                rewardCount: item.rewardValue,
                moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EP_REWARD,
                itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EP_REWARD,
                QuestName: $"EP_{item.Index + 1}_FLOWER_{item.flowerType-2}"
            );

            item.rewardState = resp.chapter.isGetBlueFlowerReward;

            ManagerAdventure.UserDataAnimal getAnimal = null;
            if (resp.userAnimal != null)
            {
                getAnimal = new ManagerAdventure.UserDataAnimal()
                {
                    animalIdx = resp.userAnimal.animalId,
                    exp = resp.userAnimal.exp,
                    gettime = 0,
                    grade = resp.userAnimal.grade,
                    level = resp.userAnimal.level,
                    overlap = resp.userAnimal.Overlap
                };
            }
            OpenPopupRewardGet(resp.clearReward, getAnimal);
            UpdateGetData();
        }
    }

    private void OpenPopupRewardGet(AppliedRewardSet rewardSet, ManagerAdventure.UserDataAnimal getAnimal = null)
    {
        //보상 텍스트 설정
        string textMessage = Global._instance.GetString("n_s_22");
        textMessage = textMessage.Replace("[n]", (item.Index + 1).ToString());
        textMessage = textMessage.Replace("[1]", Global._instance.GetString($"item_flw_{item.flowerType - 2}"));

        ManagerUI._instance.OpenPopupGetRewardAlarm
            (textMessage,
            () =>
            {
                if (getAnimal != null)
                {
                    ManagerAdventure.OnInit((b) =>
                    {
                        var newAnimal = ManagerAdventure.User.GetAnimalInstance(getAnimal);
                        ManagerAdventure.User.SyncFromServer_Animal();
                        ManagerAIAnimal.Sync();

                        UIPopupAdventureSummonAction summonPopup = ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, newAnimal, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);
                    });
                }
            },
            rewardSet);
    }

    private void UpdateGetData()
    {
        ManagerData._instance.RefreshFlowerRewardData();
        
        if (UIPopupStage._instance != null)
        {
            if (ManagerData._instance.listGetFlowersReward.ContainsKey(ScoreFlowerType.FLOWER_WHITE) == false)
                UIPopupStage._instance.InitWhiteFlowerAlarm(false);

            if (ManagerData._instance.listGetFlowersReward.ContainsKey(ScoreFlowerType.FLOWER_BLUE) == false)
                UIPopupStage._instance.InitBlueFlowerAlarm(false);

            if (ManagerData._instance.listGetFlowersReward.ContainsKey(ScoreFlowerType.FLOWER_RED) == false)
                UIPopupStage._instance.InitRedFlowerAlarm(false);

            if (ManagerData._instance.listGetFlowersReward.Count == 0)
            {
                //UI 갱신.
                ManagerUI._instance.SettingStageNewIcon(false);
            }
        }
        InitGetButton(true, true);
        UpdateReward();
    }

    private void UpdateReward()
    {
        //받는 보상이 선물상자 일 경우, 로비에 생성.
        if (item.rewardType == RewardType.boxSmall || item.rewardType == RewardType.boxMiddle || item.rewardType == RewardType.boxBig)
        {
            
            ManagerLobby._instance.ReMakeGiftbox();
        }
        else if (item.rewardType > RewardType.material)
        {
            UIDiaryController._instance.UpdateProgressHousingData();
        }
        else if (item.rewardType == RewardType.stamp)
        {
            UIDiaryController._instance.UpdateStampData();
        }
        else if (item.rewardType == RewardType.toy)
        {
            PokoyuraData.SetUserData();
            ManagerLobby._instance.ReMakePokoyura();
        }
        else if (item.rewardType == RewardType.housing)
        {
            PlusHousingModelData.SetUserData();
        }
        else if (item.rewardType == RewardType.costume)
        {
            UIDiaryController._instance.UpdateCostumeData();
        }
        else
        {   //재화 업데이트.
            Global.star = (int)GameData.User.Star;
            Global.clover = (int)(GameData.User.AllClover);
            Global.coin = (int)(GameData.User.AllCoin);
            Global.jewel = (int)(GameData.User.AllJewel);
            Global.flower = (int)GameData.User.flower;
            Global.exp = (int)GameData.User.expBall;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();
        }
    }

    private void SetFlowerSprite()
    {
        if(item.flowerType == 5)
        {
            sprFlower.spriteName = "icon_redflower_stroke_red";
        }
        else if(item.flowerType == 4)
        {
            sprFlower.spriteName = "icon_blueflower_stroke_blue";
        }
        else
        {
            sprFlower.spriteName = "icon_flower_stroke_white";
        }
    }
}
