using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;
using PokoAddressable;

public enum MessageSendType
{
    userRequestMessage, //클로버 요청 메세지
    userSendMessage,    //클로버 메세지(랭킹창/요청 응답에서 줄 수 있는 클로버)
    userSendItemMessage,//이벤트같은 조건에서 유저가 다른 유저에게 아이템을 주는 형식의 메세지
    systemMessage,      //시스템 메세지
    adMessage           //광고 메세지
}

public class UIItemMessage : MonoBehaviour
{
    [Header("ReceiveButton")]
    [SerializeField] private GameObject   receiveButton;
    [SerializeField] private UILabel[] buttonText;
    [SerializeField] private UILabel itemCount;

    [Header("SendButton")]
    [SerializeField] private GameObject   sendButton;
    [SerializeField] private UILabel[] buttonText_S;

    [Header("Profile")]
    [SerializeField] private GameObject potoRoot;
    [SerializeField] private UITexture photo;
    [SerializeField] private UIItemProfile profileItem;

    [Header("ADIcon")]
    [SerializeField] private GameObject adIcon;

    [Header("Etc")]
    [SerializeField] private UISprite         buttonImage;
    [SerializeField] private UISprite         buttonIcon;
    [SerializeField] private UISprite         buttonIconBg;
    [SerializeField] private UILabel          messageText;
    [SerializeField] private UILabel          leftTime;
    [SerializeField] private UIUrlTexture[]   buttonIcon_T;

    [Header("Plural Reward")] 
    [SerializeField] private UIPanel panelFlowBubble;
    [SerializeField] private GenericReward[] rewards;
    [SerializeField] private GameObject rootDefaultInfo;
    [SerializeField] private GameObject rootAdFlowBubble;
    [SerializeField] private UIPanel scrollView;
    [SerializeField] private UIReuseGrid_Move reuseGridMove;
    [SerializeField] private UILabel messageText2;
    private float scrollSpeed = 20;
    private const int ITEM_WIDTH = 45;
    private const int MAX_SHOW_ITEM_COUNT = 6;
    private Coroutine coPluralReward;

    [Header("SpecialDiaShop")]
    [SerializeField] private GameObject objMailMessage;
    [SerializeField] private GameObject objMailShop;
    [SerializeField] private UILabel labelMailShopLeftTime;

    private MessageData item;
    private MessageSendType type;
    private UserFriend uData = null;
    private AdManager.AdType adType;

    private bool bTimeStart = false;
    private Coroutine lastRoutine = null;

    public void UpdateData(MessageData ItemData)
    {
        item = ItemData as MessageData;
        if (item == null || gameObject.activeInHierarchy == false)
            return;

        bTimeStart = false;
        uData = null;
        if (lastRoutine != null)
        {
            StopCoroutine(lastRoutine);
        }
        photo.enabled = false;
        lastRoutine = null;

        //메세지 타입 설정 & 버튼 텍스트.
        SettingMessageType();

        //보상 이미지 세팅 & 보상 이름.
        string reward = SettingButtonIcon();

        // 메세지 내용 세팅.
        SettingMesageText(reward);

        //메세지 사진 세팅.
        SettingProfileImage();

        if (item.ts == 0)
        {
            leftTime.gameObject.SetActive(false);
        }
        else
        {
            leftTime.gameObject.SetActive(true);
            bTimeStart = true;
            leftTime.text = Global.GetTimeText_DDHHMM(item.ts);
            StartCoroutine(CoMessageTimer());
        }
    }

    void SettingMessageType()
    {
        objMailMessage.SetActive(true);
        objMailShop.SetActive(false);

        if (item.adType != AdManager.AdType.None)
        {
            SettingADItem(item.adType);
        }
        else if(item.type == RewardType.specialDiaShop)
        {
            objMailMessage.SetActive(false);
            objMailShop.SetActive(true);

            StartCoroutine(CoSpecialDiaEventEndTs());
        }
        else if (item.IsSystemMessage())
        {
            SettingReceiveItem(MessageSendType.systemMessage);
        }
        else
        {
            // 친구 정보 세팅
            SDKGameProfileManager._instance.TryGetPlayingFriend(item.userKey, out uData);
            {
                if (item.textKey > 0)
                {   //유저 아이템 메세지.
                    SettingReceiveItem(MessageSendType.userSendItemMessage);
                }
                else if (item.type == RewardType.none && item.value == 0)
                {   //클로버 요청 메세지.
                    SettingRequestItem();
                }
                else
                {   //클로버 전달 메세지.
                    SettingReceiveItem(MessageSendType.userSendMessage);
                }
            }
        }
    }

    IEnumerator CoSpecialDiaEventEndTs()
    {
        while (true)
        {
            labelMailShopLeftTime.text = GetTimeText_DDHHMM(ServerRepos.UserMailShop.endTs);

            if (Global.LeftTime(ServerRepos.UserMailShop.endTs) < 0) break;

            yield return new WaitForSeconds(1.0f);
        }

        labelMailShopLeftTime.text = "00: 00: 00";
    }

    void SettingProfileImage()

    {
        if (type == MessageSendType.systemMessage)
        {
            adIcon.SetActive(false);
            potoRoot.SetActive(true);
            profileItem.SetDefaultProfile("profile_1");
            photo.enabled = true;
        }
        else if (type == MessageSendType.adMessage)
        {
            potoRoot.SetActive(false);
            adIcon.SetActive(true);
        }
        else
        {
            adIcon.SetActive(false);
            potoRoot.SetActive(true);
            photo.enabled = true;
            if (uData != null)
            {
                //프로필 아이템 추가
                profileItem.SetProfile(uData);
            }
            else
            {
                profileItem.SetDefaultProfile("profile_2");
            }
        }
    }

    void SettingReceiveItem(MessageSendType sendType)
    {
        type = sendType;
        buttonImage.spriteName = "button_play";
        string text = Global._instance.GetString("btn_10");
        buttonText[0].text = text;
        buttonText[1].text = text;
        sendButton.SetActive(false);
        receiveButton.SetActive(true);
        
        rootDefaultInfo.SetActive(true);
        rootAdFlowBubble.SetActive(false);
    }

    void SettingRequestItem()
    {
        type = MessageSendType.userRequestMessage;
        buttonImage.spriteName = "button_002";
        string text = Global._instance.GetString("btn_7");
        buttonText_S[0].text = text;
        buttonText_S[1].text = text;
        receiveButton.SetActive(false);
        sendButton.SetActive(true);
        
        rootDefaultInfo.SetActive(true);
        rootAdFlowBubble.SetActive(false);
    }

    
    private void SettingADItem(AdManager.AdType adType)
    {
        type = MessageSendType.adMessage;
        this.adType = adType;
        buttonImage.spriteName = "button_play";
        string text = Global._instance.GetString("btn_41");
        buttonText[0].text = text;
        buttonText[1].text = text;
        receiveButton.SetActive(true);
        sendButton.SetActive(false);

        if (this.adType == AdManager.AdType.AD_1)
        {
            SetPluralReward(ServerContents.AdInfos[1].rewards);
        }
        else
        {
            rootDefaultInfo.SetActive(true);
            rootAdFlowBubble.SetActive(false);
        }
    }
    
    private void SetPluralReward(params Reward[] rewards)
    {
        panelFlowBubble.depth = UIPopupMailBox._instance.uiPanel.depth + UIPopupMailBox._instance.panelCount +
                                UIPopupMailBox._instance.sortOrderCount;
        
        rootDefaultInfo.SetActive(false);
        rootAdFlowBubble.SetActive(true);
        //아이템 데이터 설정
        bool isUseScroll = rewards.Length > MAX_SHOW_ITEM_COUNT; 
        if (isUseScroll == true)
        {   //획득할 수 있는 보상의 수가 보여지는 ui 보다 많을 때, 스크롤 표시
            reuseGridMove.InItGrid(rewards.Length, (go, index) =>
            {
                if (rewards.Length > index)
                    go.GetComponent<GenericReward>().SetReward(rewards[index]);
            });
            scrollView.clipping = UIDrawCall.Clipping.SoftClip;
        }
        else
        {   //스크롤이 불필요하다면 보여지는 보상만 설정
            for (int i = 0; i < this.rewards.Length; i++)
            {
                this.rewards[i].gameObject.SetActive(i < rewards.Length);
                if (rewards.Length > i)
                    this.rewards[i].SetReward(rewards[i]);
            }
            scrollView.clipping = UIDrawCall.Clipping.None;
        }

        //그리드 위치 설정
        float itemSpace = (ITEM_WIDTH * 0.5f);
        Vector3 reuseGridPos = reuseGridMove.transform.localPosition;
        reuseGridPos.x = (isUseScroll == true)
            ? (itemSpace * -(MAX_SHOW_ITEM_COUNT - 1))
            : (itemSpace * -(rewards.Length - 1));
        reuseGridMove.transform.localPosition = reuseGridPos;
        
        //아이템 간격 설정
        reuseGridMove.itemSize = ITEM_WIDTH;

        //스크롤 사용될 때, 스크롤 연출 출력
        if (isUseScroll == true)
        {
            if(coPluralReward != null)
                StopCoroutine(coPluralReward);
            
            coPluralReward = StartCoroutine(CoPluralRewardMove());
        }
    }
    
    private IEnumerator CoPluralRewardMove()
    {
        yield return new WaitForSeconds(0.3f);
        
        while (this.gameObject.activeInHierarchy == true)
        {
            scrollView.transform.localPosition += -Vector3.right * Time.deltaTime * scrollSpeed;
            scrollView.clipOffset += Vector2.right * Time.deltaTime * scrollSpeed;
            yield return null;
        }
    }

    string SettingButtonIcon()
    {
        bool bSprite = true;
        bool bSpriteAutoPerfect = true;
        bool bCount = true;
        string reward = "";
        buttonIconBg.gameObject.SetActive(false);
        if (type == MessageSendType.userRequestMessage)
        {
            buttonIcon.spriteName = "icon_clover_stroke_green";
            buttonIcon.MakePixelPerfect();
        }
        else if(type == MessageSendType.adMessage && item.type == RewardType.none)
        {
            buttonIcon.spriteName = "icon_randombox_close";
            buttonIcon.MakePixelPerfect();
            reward = Global._instance.GetString("item_22");
            bCount = false;
        }
        else
        {
            if (item.type == RewardType.none)
            {
                buttonIcon.gameObject.SetActive(false);
                buttonText[0].transform.localPosition = new Vector3(-7f, 3f, 0f);
            }
            //기본 재화.
            else if (item.type == RewardType.clover)
            {
                buttonIcon.spriteName = "icon_clover_stroke_green";
                reward = Global._instance.GetString("item_1");
            }
            else if (item.type == RewardType.jewel)
            {
                buttonIcon.spriteName = "icon_diamond_stroke_blue";
                reward = Global._instance.GetString("item_2");
            }
            else if (item.type == RewardType.star)
            {
                buttonIcon.spriteName = "icon_star_stroke_pink";
                reward = Global._instance.GetString("item_3");
            }
            else if (item.type == RewardType.coin)
            {
                buttonIcon.spriteName = "icon_coin_stroke_yellow";
                reward = Global._instance.GetString("item_4");
            }
            else if (item.type == RewardType.cloverFreeTime)
            {
                buttonIcon.spriteName = "icon_cloverTime_stroke_green";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localPosition = new Vector3(-20f, 15f, 0f);
                reward = Global._instance.GetString("item_7");
                reward.Replace("[n]", item.value.ToString());
                itemCount.enabled = true;
                itemCount.text = $"{item.value / 60}{Global._instance.GetString("time_3")}";

                buttonIcon.enabled = true;
                buttonIcon_T[0].enabled = false;
                buttonIcon_T[1].enabled = false;
                return reward;
            }
            else if (item.type == RewardType.flower)
            {
                buttonIcon.spriteName = "stage_icon_level_03";
                reward = Global._instance.GetString("item_10");
            }
            //선물상자.
            else if (item.type == RewardType.boxSmall)
            {
                buttonIcon.spriteName = "icon_giftbox_blueStroke_01";
                reward = Global._instance.GetString("item_g_1");
                bCount = false;
            }
            else if (item.type == RewardType.boxMiddle)
            {
                buttonIcon.spriteName = "icon_giftbox_blueStroke_02";
                reward = Global._instance.GetString("item_g_2");
                bCount = false;
            }
            else if (item.type == RewardType.boxBig)
            {
                buttonIcon.spriteName = "icon_giftbox_blueStroke_03";
                reward = Global._instance.GetString("item_g_3");
                bCount = false;
            }
            //레디 아이템.
            else if (item.type == RewardType.readyItem1)
            {
                buttonIcon.spriteName = "icon_apple_stroke";
                reward = Global._instance.GetString("item_i_1");
            }
            else if (item.type == RewardType.readyItem2)
            {
                buttonIcon.spriteName = "icon_scoreUp_stroke";
                reward = Global._instance.GetString("item_i_2");
            }
            else if (item.type == RewardType.readyItem3)
            {
                buttonIcon.spriteName = "icon_random_bomb_stroke";
                reward = Global._instance.GetString("item_i_3");
            }
            else if (item.type == RewardType.readyItem4)
            {
                buttonIcon.spriteName = "icon_line_bomb_stroke";
                reward = Global._instance.GetString("item_i_4");
            }
            else if (item.type == RewardType.readyItem5)
            {
                buttonIcon.spriteName = "icon_bomb_stroke";
                reward = Global._instance.GetString("item_i_5");
            }
            else if (item.type == RewardType.readyItem6)
            {
                buttonIcon.spriteName = "icon_rainbow_stroke";
                reward = Global._instance.GetString("item_i_6");
            }
            else if (item.type == RewardType.readyItem7)
            {
                buttonIcon.spriteName = "";
                reward = Global._instance.GetString("item_i_7");
            }
            else if (item.type == RewardType.readyItem8)
            {
                buttonIcon.spriteName = "";
                reward = Global._instance.GetString("item_i_8");
            }
            else if (item.type == RewardType.readyItem3_Time)
            {
                buttonIcon.spriteName = "icon_time_random_bomb_stroke";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localPosition = new Vector3(-20f, 15f, 0f);
                reward = Global._instance.GetString("item_i_15");
                reward.Replace("[n]", item.value.ToString());
                itemCount.enabled = true;
                itemCount.text = $"{item.value / 60}{Global._instance.GetString("time_3")}";

                buttonIcon.enabled = true;
                buttonIcon_T[0].enabled = false;
                buttonIcon_T[1].enabled = false;
                return reward;
            }
            else if (item.type == RewardType.readyItem4_Time)
            {
                buttonIcon.spriteName = "icon_time_line_bomb_stroke";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localPosition = new Vector3(-20f, 15f, 0f);
                reward = Global._instance.GetString("item_i_11");
                reward.Replace("[n]", item.value.ToString());
                itemCount.enabled = true;
                itemCount.text = $"{item.value / 60}{Global._instance.GetString("time_3")}";

                buttonIcon.enabled = true;
                buttonIcon_T[0].enabled = false;
                buttonIcon_T[1].enabled = false;
                return reward;
            }
            else if (item.type == RewardType.readyItem5_Time)
            {
                buttonIcon.spriteName = "icon_time_bomb_stroke";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localPosition = new Vector3(-20f, 15f, 0f);
                reward = Global._instance.GetString("item_i_12");
                reward.Replace("[n]", item.value.ToString());
                itemCount.enabled = true;
                itemCount.text = $"{item.value / 60}{Global._instance.GetString("time_3")}";

                buttonIcon.enabled = true;
                buttonIcon_T[0].enabled = false;
                buttonIcon_T[1].enabled = false;
                return reward;
            }
            else if (item.type == RewardType.readyItem6_Time)
            {
                buttonIcon.spriteName = "icon_time_rainbow_stroke";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localPosition = new Vector3(-20f, 15f, 0f);
                reward = Global._instance.GetString("item_i_13");
                reward.Replace("[n]", item.value.ToString());
                itemCount.enabled = true;
                itemCount.text = $"{item.value / 60}{Global._instance.GetString("time_3")}";

                buttonIcon.enabled = true;
                buttonIcon_T[0].enabled = false;
                buttonIcon_T[1].enabled = false;
                return reward;
            }
            else if (item.type == RewardType.readyItemBomb_Time)
            {
                buttonIcon.spriteName = "icon_time_allBomb_stroke";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localPosition = new Vector3(-20f, 15f, 0f);
                reward = Global._instance.GetString("item_i_14");
                reward.Replace("[n]", item.value.ToString());
                itemCount.enabled = true;
                itemCount.text = $"{item.value / 60}{Global._instance.GetString("time_3")}";

                buttonIcon.enabled = true;
                buttonIcon_T[0].enabled = false;
                buttonIcon_T[1].enabled = false;
                return reward;
            }

            //모험모드 전용 아이템
            else if (item.type == RewardType.revivalAndHeal)
            {
                buttonIcon.spriteName = "item_Resurrection";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localScale = Vector3.one * 0.55f;
                reward = Global._instance.GetString("item_ad_1");

                bSpriteAutoPerfect = false;
            }
            else if (item.type == RewardType.skillCharge)
            {
                buttonIcon.spriteName = "item_Skill_charge";
                buttonIcon.MakePixelPerfect();
                reward = Global._instance.GetString("item_ad_2");

                bSpriteAutoPerfect = false;
            }
            else if (item.type == RewardType.rainbowHammer)
            {
                buttonIcon.spriteName = "item_Rainbow_Hammer";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localScale = Vector3.one * 0.55f;
                reward = Global._instance.GetString("item_ad_3");

                bSpriteAutoPerfect = false;
            }

            //인게임아이템.
            else if (item.type == RewardType.ingameItem1)
            {
                buttonIcon.spriteName = "icon_hammer_stroke";
                reward = Global._instance.GetString("item_i_7");
            }
            else if (item.type == RewardType.ingameItem2)
            {
                buttonIcon.spriteName = "icon_line_hammer_stroke";
                reward = Global._instance.GetString("item_i_8");
            }
            else if (item.type == RewardType.ingameItem3)
            {
                buttonIcon.spriteName = "icon_power_hammer_stroke";
                reward = Global._instance.GetString("item_i_9");
            }
            else if (item.type == RewardType.ingameItem4)
            {
                buttonIcon.spriteName = "icon_rainbow_bomb_hammer_stroke";
                reward = Global._instance.GetString("item_i_10");
            }
            else if (item.type == RewardType.ingameItem5)
            {
                buttonIcon.spriteName = "icon_color_brush";
                reward                = Global._instance.GetString("item_i_16");
            }
            else if (item.type == RewardType.stamp)
            {   //다이어리 스티커.
                gameObject.AddressableAssetLoad<Texture2D>("local_message/stamps", (texture) =>
                {
                    buttonIcon_T[0].mainTexture = texture;
                    buttonIcon_T[1].mainTexture = texture;
                });
                reward = Global._instance.GetString("item_5");
                bCount = false;
                bSprite = false;
            }
            else if (item.type == RewardType.toy)
            {   //포코유라.
                string fileName = string.Format("y_i_{0}", item.value);
                buttonIcon_T[0].SuccessEvent += SettingStrokeButtonIcon;
                buttonIcon_T[0].SettingTextureScale(70, 70);
                buttonIcon_T[0].LoadCDN(Global.gameImageDirectory, "Pokoyura/", fileName);
                reward = Global._instance.GetString("item_8");
                bCount = false;
                bSprite = false;
            }
            else if (item.type == RewardType.costume)
            {   //코스튬.
                gameObject.AddressableAssetLoad<Texture2D>("local_message/costume_stroke", (texture) =>
                {
                    buttonIcon_T[0].mainTexture = texture;
                    buttonIcon_T[1].mainTexture = texture;
                });
                reward = Global._instance.GetString("item_8");
                bCount = false;
                bSprite = false;
            }
            else if (item.type == RewardType.housing)
            {
                int housingIdx = (int)(item.value / 10000);
                int modelIdx = (int)(item.value % 10000);

                string fileName = string.Format("{0}_{1}", housingIdx, modelIdx);
                buttonIcon_T[0].SuccessEvent += SettingStrokeButtonIcon;
                buttonIcon_T[0].SettingTextureScale(70, 70);
                buttonIcon_T[0].LoadCDN(Global.gameImageDirectory, "IconHousing/", fileName);

                reward = Global._instance.GetString(string.Format("h_{0}_{1}", housingIdx, modelIdx));
                bCount = false;
                bSprite = false;

            }
            else if (item.type == RewardType.wing)
            {
                buttonIcon.spriteName = "adven_wing_icon";
                reward = Global._instance.GetString("item_11");
            }
            else if (item.type == RewardType.wingFreetime)
            {
                buttonIcon.spriteName = "adven_wing_time_icon";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localPosition = new Vector3(-20f, 15f, 0f);
                reward = Global._instance.GetString("item_20");
                itemCount.enabled = true;
                itemCount.text = $"{item.value / 60}{Global._instance.GetString("time_3")}";

                buttonIcon.enabled = true;
                buttonIcon_T[0].enabled = false;
                buttonIcon_T[1].enabled = false;
                return reward;
            }
            else if (item.type == RewardType.wingExtend)
            {
                buttonIcon.spriteName = "adven_wing_icon";
                reward = Global._instance.GetString("wing_up_Item");
            }
            else if (item.type == RewardType.expBall)
            {
                buttonIcon.spriteName = "adven_exp_icon";
                reward = Global._instance.GetString("item_12");
            }
            else if (item.type == RewardType.gachaTicket)
            {
                string fileName = string.Format("gacha_t_{0:D4}", (int)item.value);
                buttonIcon_T[0].SuccessEvent += SettingStrokeButtonIcon;
                buttonIcon_T[0].SettingTextureScale(70, 70);
                buttonIcon_T[0].LoadCDN(Global.adventureDirectory, "IconEvent/", fileName);

                reward = Global._instance.GetString("item_13");
                bSprite = false;
                bCount = false;
            }
            else if (item.type == RewardType.animal)
            {
                string fileName = ManagerAdventure.GetAnimalProfileFilename(item.value, 0);
                buttonIconBg.gameObject.SetActive(true);
                buttonIcon_T[1].enabled = false;
                buttonIcon_T[0].SuccessEvent += SettingStrokeButtonIconDisable;
                buttonIcon_T[0].SettingTextureScale(50, 50);
                buttonIcon_T[0].LoadCDN(Global.adventureDirectory, "Animal/", fileName);
                bCount = false;
                reward = Global._instance.GetString("item_14");
                //reward = ManagerAdventure.instance.GetString(string.Format("n_a{0}", item.type));
                bSprite = false;
            }
            //중첩아이템 이미지 추가
            else if(item.type == RewardType.animalOverlapTicket)
            {
                buttonIcon.spriteName = $"item_overlap_{item.value}";
                reward = Global._instance.GetString($"item_ot_{item.value}");
                bCount = false;
            }
            //랭킹토큰 아이템
            else if(item.type == RewardType.rankToken)
            {
                buttonIcon.spriteName = $"worldrank_rankToken";
                reward = Global._instance.GetString($"item_23");
            }
            //캡슐토이토큰 아이템
            else if(item.type == RewardType.capsuleGachaToken)
            {
                buttonIcon.spriteName = $"icon_capsuleToy_Token";
                buttonIcon.MakePixelPerfect();
                buttonIcon.transform.localScale = Vector3.one;
                reward = Global._instance.GetString($"p_ct_4");

                bSpriteAutoPerfect = false;
            }
            else if (item.type == RewardType.endContentsToken)
            {
                buttonIcon.spriteName = $"icon_endContents_Token";
                reward = Global._instance.GetString($"ec_col_101");
            }
            else
            {   //재료.
                int matNum = ((int)item.type % 1000);
                string fileName = string.Format("mt_{0}", matNum);
                buttonIcon_T[0].SuccessEvent += SettingStrokeButtonIcon;
                buttonIcon_T[0].SettingTextureScale(70, 70);
                buttonIcon_T[0].LoadCDN(Global.gameImageDirectory, "IconMaterial/", fileName);
                reward = Global._instance.GetString("item_9");
                bSprite = false;
            }
        }

        //스프라이트 / 텍스트에 따른 처리.
        if (bSprite == true)
        {
            if(bSpriteAutoPerfect)
                buttonIcon.MakePixelPerfect();

            buttonIcon.enabled = true;
            buttonIcon_T[0].enabled = false;
            buttonIcon_T[1].enabled = false;
        }
        else
        {
            buttonIcon.enabled = false;
            buttonIcon_T[0].enabled = true;
            buttonIcon_T[1].enabled = true;
        }

        //카운트.
        if (bCount == true)
        {
            itemCount.enabled = true;
            itemCount.text = string.Format("x{0}", item.value);
        }
        else
        {
            buttonIcon_T[0].transform.localPosition = new Vector3(0f, buttonIcon_T[0].transform.localPosition.y, 0f);
            itemCount.enabled = false;
        }

        if(item.type == RewardType.animalOverlapTicket)
            buttonIcon.transform.localPosition = new Vector3(-5f, 24f, 0f);
        else
            buttonIcon.transform.localPosition = new Vector3(-5f, 15f, 0f);

        return reward;
    }

    void SettingStrokeButtonIcon()
    {
        if (buttonIcon_T[1] == null)
            return;
        buttonIcon_T[1].mainTexture = buttonIcon_T[0].mainTexture;
        buttonIcon_T[1].width = 70;
        buttonIcon_T[1].height = 70;
    }

    void SettingStrokeButtonIconDisable()
    {
        if (buttonIcon_T[1] == null)
            return;
        buttonIcon_T[1].mainTexture = buttonIcon_T[0].mainTexture;
        buttonIcon_T[1].width = buttonIcon_T[0].width;
        buttonIcon_T[1].height = buttonIcon_T[0].height;
        buttonIcon_T[1].enabled = false;
    }

    void SettingMesageText(string reward)
    {
        if (type == MessageSendType.systemMessage)
        {
            if (item.textKey > -1)
            {
                string key = string.Format("msg_s_{0}", item.textKey);
                messageText.text = Global._instance.GetString(key).Replace("[1]", reward);
                messageText.text = messageText.text.Replace("[n]", item.value.ToString());
            }
            else if (item.text != "")
            {
                messageText.text = item.text;
            }
        }
        else if (type == MessageSendType.adMessage)
        {
            if(adType == AdManager.AdType.AD_1)
                messageText2.text = Global._instance.GetString("msg_ads_2");
            else
                messageText.text = Global._instance.GetString("msg_ads_1").Replace("[1]", reward);
        }
        else
        {
            string userName = Global._instance.GetString("user_1");
            if (uData != null)
            {
                userName = Global.ClipString(uData.DefaultName, 10);
            }

            //메세지 타입에 따라 메세지 결정.
            switch (type)
            {
                case MessageSendType.userRequestMessage:
                    messageText.text = Global._instance.GetString("msg_u_2").Replace("[u]", userName);
                    break;
                case MessageSendType.userSendMessage:
                    messageText.text = Global._instance.GetString("msg_u_1").Replace("[u]", userName);
                    break;
                case MessageSendType.userSendItemMessage:
                    string key = string.Format("msg_s_{0}", item.textKey);
                    string message = Global._instance.GetString(key).Replace("[1]", reward);
                    message = message.Replace("[n]", item.value.ToString());
                    messageText.text = message.Replace("[u]", userName);
                    break;
            }
        }
    }

    void OnClickBtnMailShop()
    {
        ServerAPI.MailShopGoods(ServerContents.MailShop.vsn, (MailShopGoodsResp resp) =>
        {
            ManagerUI._instance.OpenPopup<UIPopupSpecialDiaShop>((popup) => popup.InitData(resp.mailShopGoods));
        });
    }

    void OnClickBtnMessage()
    {  
        //터치 가능 조건 검사.
        if (UIPopupMailBox._instance.bCanTouch == false)
            return;
        //터치막음.
        UIPopupMailBox._instance.bCanTouch = false;

        if(type == MessageSendType.adMessage)
        {
            if (adType == AdManager.AdType.AD_1 ||
                adType == AdManager.AdType.AD_2)
            {
                AdManager.ShowAD_RequestAdReward(adType, (isSuccess, reward) =>
                {
                    UIPopupMailBox._instance.bCanTouch = true;
                    UIPopupMailBox._instance.SetMailBox();
                });
            }
            else
            {
                ManagerUI._instance.OpenPopup<UIPopupADView>
                (
                    (popup) =>
                    {
                        popup.SetRequestAdReward(adType, ServerContents.AdInfos[(int)adType].rewards);
                        popup._callbackClose += () =>
                        {
                            UIPopupMailBox._instance.bCanTouch = true;
                            UIPopupMailBox._instance.SetMailBox();
                        };
                    }
                );
            }

        }
        else
        {
            ReceiveMessage();
        }
    }

    void ReceiveMessage()
    {
        if( item.ts != 0 )
        {
            long lTime = Global.LeftTime(item.ts);
            if( lTime <= 0 )
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_34"), false, TouchOn);
                popupSystem.SetResourceImage("Message/tired");
                return;
            }
        }

        // 클로버 획득 상한처리
        if (item.type == RewardType.clover && GameData.Asset.AllClover >= ServerRepos.LoginCdn.CloverMax )
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
            var msg = Global._instance.GetString("n_s_51");
            if (msg.Contains("[n]"))
            {
                msg = msg.Replace("[n]", ServerRepos.LoginCdn.CloverMax.ToString());
            }
            else
            {
                msg = msg.Replace("100", ServerRepos.LoginCdn.CloverMax.ToString());
            }

            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), msg, false, TouchOn);
            popupSystem.SetResourceImage("Message/error");
            return;
        }
        
        // 날개 획득 상한 처리
        if (item.type == RewardType.wing && GameData.Asset.AllWing >= ServerRepos.LoginCdn.WingMax )
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
            var msg = Global._instance.GetString("n_m_12");
            if (msg.Contains("[n]"))
            {
                msg = msg.Replace("[n]", ServerRepos.LoginCdn.WingMax.ToString());
            }
            else
            {
                msg = msg.Replace("50", ServerRepos.LoginCdn.WingMax.ToString());
            }

            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), msg, false, TouchOn);
            popupSystem.SetResourceImage("Message/error");
            return;
        }

        if (item.type == RewardType.boxSmall || item.type == RewardType.boxMiddle || item.type == RewardType.boxBig)
        {
            
            if (ServerRepos.GiftBoxes.Count >= 2)
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_2"), false, TouchOn);
                popupSystem.SetResourceImage("Message/tired");
                return;
            }
        }

        // 탐험모드 날개, 인게임 아이템, 경험치 구슬 예외처리 (Day 2 전 수령 불가)
        if (item.type == RewardType.wing || item.type == RewardType.wingFreetime || item.type == RewardType.revivalAndHeal || item.type == RewardType.skillCharge || item.type == RewardType.rainbowHammer || item.type == RewardType.expBall)
        {
            if (!ManagerAdventure.CheckStartable())
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_adv_3"), false);
                popupSystem.SetResourceImage("Message/tired");
                popupSystem.SortOrderSetting();
                popupSystem.SetCallbackSetting(1, OnTouch, true);
                popupSystem.SetCallbackSetting(3, OnTouch, true);
                popupSystem.SetCallbackSetting(4, OnTouch, true);
                return;
            }
        }

        //친구에게서 온 메세지고 uData있을 경우에 & 쿨타임이 가지 않은 상태일 경우에만 질문 시스템팝업.
        if (type == MessageSendType.userSendMessage && uData != null)
        {
            if(uData.CloverCoolTime == 0 || Global.LeftTime(uData.CloverCoolTime) <= 0)
            {
                //보상처리 (필요여부 확인 필요) - 친구 클로버 받았을 때 다시 보내는 팝업.
                //"친구에게서 클로버가 왔고 감사의 마음을 담아 클로버를 보내겠습니까? (라인메세지 전송)"
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();

                var gameFriends = SDKGameProfileManager._instance.GetGameFriendsKey();
                bool isGameFriend = gameFriends.Contains(uData._userKey);

                string message = Global._instance.GetString(isGameFriend ? "n_m_10" : "n_m_5");

                popupSystem.FunctionSetting(1, "ReceiveMail", gameObject);
                popupSystem.FunctionSetting(3, "OnTouch", gameObject, true);
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, TouchOn);
                popupSystem.SetResourceImage("Message/clover");
                return;
            }
        }

        if( item.type == RewardType.gachaTicket )
        {
            // 탐험모드 오픈 전 예외처리
            if (!ManagerAdventure.CheckStartable())
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_tk_4"), false);
                popupSystem.SetResourceImage("Message/tired");
                popupSystem.SortOrderSetting();
                popupSystem.SetCallbackSetting(1, OnTouch, true);
                popupSystem.SetCallbackSetting(3, OnTouch, true);
                popupSystem.SetCallbackSetting(4, OnTouch, true);
                return;
            }
            ServerAPI.AdventureCanGacha(item.value, (AdventureCanGachaResp resp) 
                => {
                    if(resp.IsSuccess)
                    {
                        ManagerAdventure.OnInit(
                                (bool b) =>
                                {
                                    //보상처리 - 가챠 티켓
                                    UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
                                    string message = Global._instance.GetString("n_tk_5");
                                    popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, true, TouchOn);
                                    popupSystem.SetResourceImage(RewardHelper.GetRewardTextureResourcePath(item.type), buttonIcon_T[0].mainTexture as Texture2D);

                                    popupSystem.SetButtonText(1, Global._instance.GetString("btn_2"));
                                    popupSystem.SetButtonTextSpacing(1, -2);
                                    popupSystem.SetButtonText(2, Global._instance.GetString("btn_1"));
                                    popupSystem.SetButtonTextSpacing(2, 2);

                                    popupSystem.SetCallbackSetting(1, OnTouch, true);
                                    if (resp.canGacha)
                                        popupSystem.SetCallbackSetting(2, ReceiveMail);
                                    else
                                        popupSystem.SetCallbackSetting(2, GachaTicketToOverlapTicket);
                                    popupSystem.SetCallbackSetting(3, OnTouch, true);
                                    popupSystem.ShowLanPageButton(ManagerAdventure.GetGachaLanpageKey(item.value), Global._instance.GetString("p_frd_l_20"));
                                    popupSystem.ShowBuyInfo("buyinfo_tk_1");
                                });
                    }
                    else
                    {
                        TouchOn();
                    }
                    
                });

            return;
        }
        else if (item.type == RewardType.animal )
        {
            // 탐험모드 오픈 전 예외처리
            if (!ManagerAdventure.CheckStartable())
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_adv_4"), false);
                popupSystem.SetResourceImage("Message/tired");
                popupSystem.SortOrderSetting();
                popupSystem.SetCallbackSetting(1, OnTouch, true);
                popupSystem.SetCallbackSetting(3, OnTouch, true);
                popupSystem.SetCallbackSetting(4, OnTouch, true);
                return;
            }

            ManagerAdventure.OnInit((b) =>
            {
                var existAnimal = ManagerAdventure.User.GetAnimalInstance(item.value);
                var animalData = ManagerAdventure.Animal.GetAnimal(item.value);
                if ( existAnimal != null && animalData != null)
                {
                    if( existAnimal.overlap < animalData.maxOverlap )
                    {
                        if (LanguageUtility.IsShowBuyInfo)
                        {
                            // 법률 관련 문구 필요 : 
                            UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
                            string message = Global._instance.GetString("n_tk_8");
                            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, true, TouchOn);
                            popupSystem.SetResourceImage(RewardHelper.GetRewardTextureResourcePath(item.type), buttonIcon_T[0].mainTexture as Texture2D);

                            popupSystem.SetButtonText(1, Global._instance.GetString("btn_2"));
                            popupSystem.SetButtonTextSpacing(1, -2);
                            popupSystem.SetButtonText(2, Global._instance.GetString("btn_1"));
                            popupSystem.SetButtonTextSpacing(2, 2);

                            popupSystem.SetCallbackSetting(1, OnTouch, true);
                            popupSystem.SetCallbackSetting(2, ReceiveMail);
                            popupSystem.SetCallbackSetting(3, OnTouch, true);
                            popupSystem.ShowBuyInfo("buyinfo_tk_1");
                        }
                        else
                            ReceiveMail();

                    }
                    else
                    {
                        UIPopupSystemDescription popupSystem = ManagerUI._instance.OpenPopup<UIPopupSystemDescription>();
                        string message = Global._instance.GetString("n_tk_8");
                        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, true, TouchOn);
                        popupSystem.SetResourceImage(RewardHelper.GetRewardTextureResourcePath(item.type), buttonIcon_T[0].mainTexture as Texture2D);

                        popupSystem.SetButtonText(1, Global._instance.GetString("btn_2"));
                        popupSystem.SetButtonTextSpacing(1, -2);
                        popupSystem.SetButtonText(2, Global._instance.GetString("btn_1"));
                        popupSystem.SetButtonTextSpacing(2, 2);

                        popupSystem.SetCallbackSetting(1, OnTouch, true);
                        popupSystem.SetCallbackSetting(2, AnimalToOverlapTicket);
                        popupSystem.SetCallbackSetting(3, OnTouch, true);
                        popupSystem.ShowBuyInfo("buyinfo_tk_1");
                    }
                }
                
                }
            );

            return;
        }
        else if (item.type == RewardType.animalOverlapTicket )
        {
            if (!ManagerAdventure.CheckStartable())
            {
                //보상처리 - 중첩권
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();

                string message = Global._instance.GetString("n_adv_3");
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, TouchOn);
                popupSystem.SetResourceImage(RewardHelper.GetOverlapTextureResourcePath(item.value));
            }
            else
            {
                ReceiveMailOverlapOpenPopup();
            }
            
            return;
        }
        else if( item.type == RewardType.wingFreetime)
        {
            if(!ManagerAdventure.CheckStartable())
            {
                //보상처리 - 시간제 날개
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                string message = Global._instance.GetString("n_s_44");
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, TouchOn);

                return;
            }
        }
        else if(item.type == RewardType.rankToken)
        {
            int maxRankTokenAmount = 3000;

            if(ManagerWorldRanking.userData == null) return;

            if(ManagerWorldRanking.userData.GetRankToken() >= maxRankTokenAmount)
            {
                //보상처리 - 랭킹 토큰
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                string message = Global._instance.GetString("n_wrk_3");
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, TouchOn);

                return;
            }
        }
        else if(item.type == RewardType.endContentsToken)
        {
            if(ManagerEndContentsEvent.GetPokoCoin() >= ManagerEndContentsEvent.GetMaxPokoCoin())
            {
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                string message = Global._instance.GetString("n_ec_3");
                popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, TouchOn);

                return;
            }
        }

        ReceiveMail();
    }

    void ExchangeGachaTicket(System.Action<int> successCallback = null)
    {
        ServerAPI.ExchangeGachaToAnimalTicket((int)item.index, (r) =>
        {
            if (r.IsSuccess)
            {
                if( r.output > 0)
                {
                    //보상처리 - 가챠 티켓
                    UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                    string message = Global._instance.GetString("n_tk_7");
                    message = message.Replace("[n]", r.output.ToString());
                    //Texture texture = RewardHelper.GetTexture(RewardType.animalOverlapTicket,  buttonIcon_T[0].mainTexture);
                    popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false, null);

                    UIPopupMailBox._instance.OnRefreshReceived(r);

                    successCallback?.Invoke(r.output);
                }
                else
                {
                    ReceiveMail();
                }
            }
        });
    }

    private void GachaTicketToOverlapTicket()
    {
        ExchangeGachaTicket
        (
            (ticketGrade) => 
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                (
                    (int)RewardType.animalOverlapTicket,
                    ticketGrade,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.NULL,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_OVERLAP_TICKET_CHANGE,
                    null
                );
            }
        );
    }

    private void AnimalToOverlapTicket()
    {
        ExchangeGachaTicket
        (
            (ticketGrade) =>
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                (
                    (int)RewardType.animalOverlapTicket,
                    ticketGrade,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.NULL,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ANIMAL_TO_OVERLAP_TICKET,
                    null
                );
            }
        );
    }

    private void ReceiveMailOverlapOpenPopup()
    {
        var popup = ManagerUI._instance.OpenPopup<UIPopUpAdventureAnimalSelectOverlap>((UIPopUpAdventureAnimalSelectOverlap p) =>
        {
            p.InitTarget(UIPopupStageAdventureAnimal.PopupMode.OverLapMode, item.value);
            p.onSelectOK += delegate (int animalIndex) {
                ServerAPI.ReceiveMail((int)item.index, animalIndex.ToString(), recvMessageReceive);
            };

        }
        );
    }

    private void OpenPopup_OverlapMax(UserReceiveMailResp resp, RewardType itemType)
    {
        //보상처리 - 중첩권
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.InitSystemPopUp(
            name:           Global._instance.GetString("p_t_4"),
            text:           (itemType == RewardType.animal) ? Global._instance.GetString("n_s_36") : Global._instance.GetString("n_tk_6"),
            useButtons:     false,
            in_callback:    () => { TouchOn(); GetItemUpdate(resp); }
        );
        popupSystem.SetResourceImage("Message/coin");
        popupSystem.SortOrderSetting();
    }

    void OnTouch()
    {
        UIPopupMailBox._instance.bCanTouch = true;
    }

    void ReceiveMail()
    {
        ServerAPI.ReceiveMail((int)item.index, recvMessageReceive);
    }

    Texture GetMessageTexture(Texture tex)
    {
        if (buttonIcon_T[0].enabled)
            return tex;
        else
            return null;
    }

    private bool IsLineFriendReturnSendClover()
    {
        //유저 데이터가 없을 때 예외처리
        if (uData == null) return false;

        //라인 친구에게 클로버를 받았지만 이미 친구에게 클로버를 보냈을 때는 예외 처리
        if (uData.CloverCoolTime != 0 && Global.LeftTime(uData.CloverCoolTime) > 0) return false;
        
        var gameFriends = SDKGameProfileManager._instance.GetGameFriendsKey();

        return !gameFriends.Contains(uData._userKey);

    }
    
    void recvMessageReceive(UserReceiveMailResp resp)
    {
        if (resp.IsSuccess)
        {
            #region 그로씨
            //그로씨     
            bool isSendGrowthy = true;
            string questName = null;

            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_OPERATOR_REWARD;
            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PRESENT_BOX;

            if (!item.IsSystemMessage())
            {
                mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD;
                rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SOCIAL_REWARD;

                if (item.type == RewardType.gachaTicket)
                {
                    isSendGrowthy = false;
                    LogGachaTicket(resp);
                }

                else if (item.textKey == 42) //깨우기 이벤트 - 깨우기 메시지 보상
                {
                    SendGrowthyLog_Social_RESPOND_WAKEUP();
                }
                else if (item.textKey == 43) //깨우기 이벤트 - 친구가 깨우기 메시지를 받아 받는 보상
                {
                    SendGrowthyLog_Social_RECEIVE_WAKEUP_REWARD();
                }
                else if (item.textKey == 66) // 친구에 의해 온 에코피 보상 공유 아이템
                {
                    rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_POKOFLOWER_SHARE;
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_POKOFLOWER_SHARE;                    
                }
            }
            else
            {
                if (item.textKey == 1 || item.CheckSystemMailFlag("evt_login"))
                {
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ATTENDANCE_REWARD;
                    rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_LOGIN;
                }
                else if (item.textKey == 5 || item.CheckSystemMailFlag("sysrew_invite"))
                {
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_SOCIAL_REWARD;
                    rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_SOCIAL_REWARD;
                }
                else if (item.textKey == 13 || item.textKey == 15 || item.CheckSystemMailFlag("sysrew_allclear"))
                {
                    rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_FLOWER;
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_QUEST_REWARD;
                }
                else if (item.textKey == 14 )
                {                    
                    rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_RANKING_REWARD;
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_RANKING_REWARD;
                }
                else if (item.textKey == 22 || item.CheckSystemMailFlag("sysrew_adv")) //탐험모드 보상키일때 수정
                {
                    if (item.type == RewardType.boxSmall || item.type == RewardType.boxMiddle || item.type == RewardType.boxBig)
                    {
                        isSendGrowthy = false;
                    }
                }
                else if (item.textKey == 31 || item.CheckSystemMailFlag("evt_mole")) //두더지모드 보상 - 팝업에서 그로시 전부 보냄
                {
                    isSendGrowthy = false;
                }
                else if (item.textKey == 45 || item.CheckSystemMailFlag("evt_adv")) //이벤트 탐험모드 보상
                {
                    isSendGrowthy = false;
                }
                else if (item.textKey == 54 || item.textKey == 56 || item.textKey == 57 || item.CheckSystemMailFlag("evt_worldrank"))
                {
                    // 황금나뭇잎 구매, 월드랭킹 보상, 보상 받을 때 이미 그로시 남겼음
                    isSendGrowthy = false;
                }
                else if (item.textKey == 66 || item.CheckSystemMailFlag("evt_pokoflower")) // 친구에 의해 온 에코피 보상 공유 아이템
                {
                    rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_POKOFLOWER_SHARE;
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_POKOFLOWER_SHARE;
                }
                else if (item.CheckSystemMailFlag("sysrew_cbu")) // cbu 보상 수령
                {
                    rsnType = ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_CBU_SUPPLY;
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_CBU_SUPPLY;
                }
                else if (item.type == RewardType.housing)
                {
                    isSendGrowthy = false;
                }
                else if (item.CheckSystemMailFlag("/offerwall/reward") ||
                         item.CheckSystemMailFlag("/offerwall_tapjoy/reward")) // 오퍼월 보상 수령
                {
                    mrsnType = ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_OFFERWALL_REWARD;
                }
                else if (
                        item.CheckSystemMailFlag("evt_eventstage") ||
                        item.CheckSystemMailFlag("sysrew_pkg") ||
                        item.CheckSystemMailFlag("sysrew_xchganimalticket") ||
                        item.CheckSystemMailFlag("sysrew_quest") ||
                        item.CheckSystemMailFlag("sysrew_epflower") ||
                        item.CheckSystemMailFlag("sysrew_chapmission") ||
                        item.CheckSystemMailFlag("sysrew_ad") ||
                        item.CheckSystemMailFlag("sysrew_wrshop") ||
                        item.CheckSystemMailFlag("evt_alphabet") || 
                        item.CheckSystemMailFlag("evt_pokoflower") ||
                        item.CheckSystemMailFlag("evt_stagerank") || 
                        item.CheckSystemMailFlag("evt_turn_relay") ||
                        item.CheckSystemMailFlag("evt_capsuleGacha") ||
                        item.CheckSystemMailFlag("evt_welcomeMission") ||
                        item.CheckSystemMailFlag("evt_welcomeBackMission") ||
                        item.CheckSystemMailFlag("evt_renewal_login_bonus_reward") ||
                        item.CheckSystemMailFlag("decocollection/reward") ||
                        item.CheckSystemMailFlag("sysrew_ecshop") ||
                        item.CheckSystemMailFlag("sysrew_bingo_line") ||
                        item.CheckSystemMailFlag("sysrew_treasure_hunt") ||
                        item.CheckSystemMailFlag("evt_antique_store_bonus") ||
                        item.CheckSystemMailFlag("evt_pass") || // 패스 상품 : 탐험(evt_pass_adventure), 권리형(evt_pass_premium)
                        item.CheckSystemMailFlag("evt_single_round") || // 수문장 크루그 보상
                        item.CheckSystemMailFlag("evt_criminal_clear_reward") ||
                        item.CheckSystemMailFlag("evt_criminal_all_clear_reward") ||
                        item.CheckSystemMailFlag("lucky_roulette") ||
                        item.CheckSystemMailFlag("evt_group_ranking") ||
                        item.CheckSystemMailFlag("space_travel_event") ||
                        item.CheckSystemMailFlag("evt_atelier")
                        )
                {
                    isSendGrowthy = false;
                }
                else if( item.CheckSystemMailFlag("logged"))    // 차후 어떤 이유로든지간에 로그가 남았을 때
                {
                    isSendGrowthy = false;
                }

                if (item.type == RewardType.gachaTicket)
                {
                    isSendGrowthy = false;
                    LogGachaTicket(resp);
                }
            }

            if (isSendGrowthy)
            {
                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    (int)item.type,
                    item.value,
                    mrsnType,
                    rsnType,
                    questName
                    );  
            }

            #endregion 그로씨

            //"~ 를 받았어!" 팝업의 On/Off 여부
            bool overlapMaxPopup = false;

            //Debug.Log("** Message Receive ok index :" + resp.receiveIdx);
            string systemTitle = Global._instance.GetString("p_t_4");
            //클로버 요청 메일에 대한 응답 후.
            if (type == MessageSendType.userRequestMessage)
            {
                //"클로버를 보냈어!" 팝업.
                UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                string message = Global._instance.GetString("n_m_2");
                if (uData != null)
                {
                    string userName = Global.ClipString(uData.DefaultName, 10);
                    message = message.Replace("[u]", userName);
                }
                else
                {
                    message = message.Replace("[u]", Global._instance.GetString("user_1"));
                }
                popupSystem.InitSystemPopUp(systemTitle, message, false, TouchOn);
                popupSystem.SetResourceImage("Message/ok");

                //uData있을 경우에만 라인 메세지 전달.
                if (uData != null)
                {
                    SendReplyToFriend();
                }

                //그로씨(친구에게 클로버 보내기)
                if (item.IsFriendMessage())
                {
                    if (SDKGameProfileManager._instance.TryGetPlayingFriend(item.userKey, out UserFriend user))
                    {
                        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

                        var inviteFriend = new ServiceSDK.GrowthyCustomLog_Social
                                        (
                                           myProfile.stage.ToString(),
                                           ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.SEND_CLOVAR,
                                           user._userKey,
                                           user.GetTridentProfile() != null ? "" : "GAMEFRIEND"
                                        );
                        var doc = JsonConvert.SerializeObject(inviteFriend);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);

                    }
                }
            }
            //메일 받았을 경우.
            else
            {
                Method.FunctionVoid func = TouchOn;

                if (item.type == RewardType.gachaTicket || item.type == RewardType.animal || item.type == RewardType.animalOverlapTicket)
                {
                    if (resp.userAdvAnimal == null)
                    {
                        OpenPopup_OverlapMax(resp, item.type);
                        overlapMaxPopup = true;
                    }
                    else
                    {
                        ManagerAdventure.UserDataAnimal getAnimal = new ManagerAdventure.UserDataAnimal()
                        {
                            animalIdx = resp.userAdvAnimal.animalId,
                            exp = resp.userAdvAnimal.exp,
                            gettime = 0,
                            grade = resp.userAdvAnimal.grade,
                            level = resp.userAdvAnimal.level,
                            overlap = resp.userAdvAnimal.Overlap
                        };

                        var newAnimalInstance = ManagerAdventure.User.GetAnimalInstance(getAnimal);
                        func = ()
                            => {
                                TouchOn();
                                ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, newAnimalInstance, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);

                                ManagerAdventure.User.SyncFromServer_Animal();
                                ManagerAIAnimal.Sync();

                                //중첩티켓으로 동물을 획득한 경우 그로씨
                                if(item.type == RewardType.animalOverlapTicket)
                                {
                                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                                    (
                                        (int)RewardType.animal,
                                        newAnimalInstance.idx, 
                                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.NULL, 
                                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_OVERLAP_TICKET,
                                        null
                                    );
                                }
                            };
                    }
                }

                if (resp.levelUp)
                {
                    func = CallBackOpenPopupRankup;

                    if (resp.userToys != null && resp.userToys.Count > 0)
                    {
                        // 여기로 온 건 꽃을 받아서 포코유라를 받은 경우인데, 
                        // 이 경우 혹시 이 값이 들어있으면 즉시 포코유라가 받아지지 않는 문제가 있으므로 초기화해줌
                        ManagerLobby._activeGetPokoyura = 0;
                        rankupToyIndex = resp.userToys[0].index;
                    }
                    else
                    {
                        rankupToyIndex = 0;
                    }
                    //포코유라 업데이트.
                    ManagerUI._instance.SettingRankingPokoYura();
                }

                //"~ 를 받았어!" 팝업.
                if (!overlapMaxPopup)
                {
                    //즉시 적용 가능한 보상 타입인지 검사
                    bool isCanApplyImmediately = (item.type == RewardType.housing);

                    //버튼 2개
                    bool useButtons = isCanApplyImmediately;

                    //보상처리 - 아이템을 받았어!
                    UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                    string message = Global._instance.GetString(IsLineFriendReturnSendClover() ? "n_m_13" : "n_m_1");
                    popupSystem.InitSystemPopUp(systemTitle, message, useButtons, func);

                    string textPath = RewardHelper.GetRewardTextureResourcePath(item.type);
                    if (string.IsNullOrEmpty(textPath))
                    {
                        popupSystem.SetImage((Texture2D)GetMessageTexture(buttonIcon_T[0].mainTexture));
                    }
                    else
                    {
                        popupSystem.SetResourceImage(textPath);
                    }

                    if (UIPopupMailBox._instance != null)
                        UIPopupMailBox._instance.allowEndPopup = false;

                    //보상 타입에 따른 시스템 팝업 버튼 설정
                    switch (item.type)
                    {
                        case RewardType.toy:
                            popupSystem.SetImageSize(128, 128);
                            break;
                        case RewardType.costume:
                            popupSystem.SetImageSize(128, 128);
                            popupSystem.FunctionSetting(1, "ClosePopupAndOpenPopupCostume", ManagerUI._instance.gameObject, true);
                            UIDiaryController._instance.UpdateCostumeData();
                            break;
                        case RewardType.housing:
                            popupSystem.SetButtonText(2, Global._instance.GetString("btn_79"));
                            //팝업이 닫히기 전, 메일함 item 이 갱신되며 value 값이 변경되기 때문에 캐싱해서 사용
                            int value = item.value; 
                            popupSystem.SetCallbackSetting(2, () => GetHousing(value), true);
                            break;
                    }

                    if (item.type == RewardType.toy || item.type == RewardType.costume)
                    {
                      
                        if (item.type == RewardType.costume)
                        {
                            
                        }
                        else if (item.type == RewardType.toy && ManagerLobby.landIndex == 0)
                        {
                            bool autoHangPokoyura = false;

                            if (PlayerPrefs.HasKey("PokoyuraDeploy") == false)
                            {
                                if (ManagerData._instance._pokoyuraData.Count < ManagerLobby._instance._spawnPokogoroPosition.Length)
                                {
                                    autoHangPokoyura = true;
                                }
                            }

                            if (autoHangPokoyura)
                            {
                                ManagerLobby._activeGetPokoyura = item.value;

                                popupSystem.FunctionSetting(1, "ClosePopupAndStartNewPokoyuraScene", ManagerUI._instance.gameObject, true);
                                popupSystem.FunctionSetting(3, "ClosePopupAndStartNewPokoyuraScene", ManagerUI._instance.gameObject, true);
                            }
                            else
                            {
                                ManagerLobby._activeGetPokoyura = 0;
                                PokoyuraData.SetUserData();

                                popupSystem.FunctionSetting(1, "ClosePopupAndOpenPopupPokoyuraSelect", ManagerUI._instance.gameObject, true);
                                popupSystem.FunctionSetting(3, "ClosePopupAndOpenPopupPokoyuraSelect", ManagerUI._instance.gameObject, true);
                            }
                        }
                    }

                    //받은 아이템들 업데이트
                    GetItemUpdate(resp);
                }

                //친구에게서 온 메세지고 uData있을 경우에 & 쿨타임이 가지 않은 상태일 경우에만 클로버 쿨탐갱신 & 라인 메세지 전달.
                if (type == MessageSendType.userSendMessage && uData != null && item.type == RewardType.clover)
                {
                    if(uData.CloverCoolTime == 0 || Global.LeftTime(uData.CloverCoolTime) <= 0)
                    {
                        Global._instance.UpdateCloverCoolTime(uData._userKey);
                        SendReplyToFriend();
                    }
                }

                //그로씨(친구가 보낸 클로버 받기)
                if (item.textKey == 0 && item.IsFriendMessage() )
                {
                    if (SDKGameProfileManager._instance.TryGetPlayingFriend(item.userKey, out UserFriend user))
                    {
                        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

                        var inviteFriend = new ServiceSDK.GrowthyCustomLog_Social
                                            (
                                               myProfile.stage.ToString(),
                                               ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.RECEIVE_CLOVAR,
                                               user._userKey
                                            );
                        var doc = JsonConvert.SerializeObject(inviteFriend);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);
                    }
                }
            }

            UIPopupMailBox._instance.SetTabMailCount(-1);
            UIPopupMailBox._instance.SetMailBox();
        }
        else
        {
            TouchOn();
        }
    }

    // 받은 아이템들 업데이트
    private void GetItemUpdate(UserReceiveMailResp resp)
    {
        //받는 보상이 선물상자 일 경우, 로비에 생성.
        if (item.type == RewardType.boxSmall || item.type == RewardType.boxMiddle || item.type == RewardType.boxBig)
        {
            ManagerLobby._instance.ReMakeGiftbox();
        }
        else if (item.type > RewardType.material)
        {
            UIDiaryController._instance.UpdateProgressHousingData();
        }
        else if (item.type == RewardType.stamp)
        {
            UIDiaryController._instance.UpdateStampData();
        }

        if (resp.userToys != null && resp.userToys.Count > 0 && ManagerLobby._activeGetPokoyura == 0)
        {
            PokoyuraData.SetUserData();
            ManagerLobby._instance.ReMakePokoyura();
        }

        //재화 업데이트.
        Global.star = (int)GameData.User.Star;
        Global.clover = (int)(GameData.User.AllClover);
        Global.coin = (int)(GameData.User.AllCoin);
        Global.jewel = (int)(GameData.User.AllJewel);
        Global.flower = (int)GameData.User.flower;
        Global.wing = (int)GameData.Asset.AllWing;
        Global.exp = (int)GameData.User.expBall;

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        if (resp.userHousing != null)
        {
            PlusHousingModelData.SetUserData();
        }
    }

    private void GetHousing(int value)
    {
        int housingIdx = (int)(value / 10000);
        int modelIdx = (int)(value % 10000);

        //현재 오픈되지 않은 하우징이면 경고 팝업 출력
        int missionIdx = ManagerHousing.GetHousingModel(housingIdx, modelIdx).openMission;
        if (missionIdx != 0 && ManagerData._instance._missionData[missionIdx].state != TypeMissionState.Clear)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_16"), false);
            popupSystem.SetResourceImage("Message/tired");
            return;
        }
        ManagerUI._instance.ClosePoupAndOpenLobbyHousing(housingIdx, modelIdx);
    }

    int housingItemIndex = -1;
    void CallBackOpenPopupHousing()
    {
        TouchOn();
        ManagerUI._instance.ClosePopupAndOpenPopupHousing(housingItemIndex, 2);
    }

    int rankupToyIndex = 0;
    void CallBackOpenPopupRankup()
    {
        TouchOn();
        ManagerUI._instance.OpenPopupRankUp(ServerLogics.UserLevelWithFlower(), rankupToyIndex);
    }



    void SendReplyToFriend()
    {
        string userkey = null;

        if (SDKGameProfileManager._instance.TryGetPlayingFriend(item.userKey, out UserFriend user))
        {
            string userPKey = user._userKey;
            if (user.GetTridentProfile() != null)
            {
                userkey = userPKey;
            }
        }

        var lineTemplateId = UIItemSendClover.GetCloverSendLineTemplateId();
        ManagerData.SendLineMessage(userkey, lineTemplateId);
    }

    public IEnumerator CoMessageTimer()
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false || bTimeStart == false)
                break;
            long lTime = Global.LeftTime(item.ts);
            leftTime.text = GetTimeText_DDHHMM(item.ts);

            if (lTime <= 0)
            {
                leftTime.effectColor = new Color32(0x7a, 0x7a, 0x7a, 0xFF);
                break;
            }
            else
            {
                leftTime.effectColor = new Color32(0x8C, 0xAA, 0xBD, 0xFF);
            }
            yield return null;
        }
    }

    string GetTimeText_DDHHMM(long in_time, bool bLeftTime = true)
    {
        long t = in_time;
        if (bLeftTime == true)
        {
            t = Global.LeftTime(t);
        }
        if (t < 0)
            t = 0;

        string text = "";

        int day = (int)(t / (3600 * 24));
        if (day > 0)
        {
            text += day + "Day ";
        }

        text += string.Format("{0:D2}", (int)((t / 3600) % 24)) + ": ";//"時間 ";
        text += string.Format("{0:D2}", (int)((t / 60) % 60)) + ": ";//"分 ";
        text += string.Format("{0:D2}", (int)(t % 60));//+ "秒";
        return text;
    }

    void TouchOn()
    {
        //터치 가능.
        if (UIPopupMailBox._instance != null)
        {
            UIPopupMailBox._instance.bCanTouch = true;
        }
    }

    private void LogGachaTicket(UserReceiveMailResp resp)
    {
        if (resp.userAdvAnimal != null)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                (int)RewardType.gachaTicket,
                item.value,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_USE_GACHA,
                ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.NULL,
                null
            );

            var rewardTicket = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.ANIMAL,
                     "Animal_" + resp.userAdvAnimal.animalId,
                     "Animal",
                     1,
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_GACHA_TICKET,
                     $"Gacha_{item.value}"
                  );
            var DocTicket = JsonConvert.SerializeObject(rewardTicket);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", DocTicket);
        }
        else//가챠로 얻은 수 있는 모든 동물이 최대 중첩 시 3000코인으로 지급
        {
            var rewardCoin = new ServiceSDK.GrowthyCustomLog_Money
            (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_GACHA,
                0,
                3000,
                (int)(ServerRepos.User.coin),
                (int)(ServerRepos.User.fcoin)
            );
            var docCoin = JsonConvert.SerializeObject(rewardCoin);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docCoin);
        }
    }

    private void SendGrowthyLog_Social_RESPOND_WAKEUP()
    {
        if(SDKGameProfileManager._instance.TryGetPlayingFriend(item.userKey, out UserFriend user))
        {
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            var social = new ServiceSDK.GrowthyCustomLog_Social
            (
                lastStage: myProfile.stage.ToString(),
                socialType: ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.RESPOND_WAKEUP,
                friendUid: user._userKey
            );

            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(social);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", jsonData);
        }        
    }

    private void SendGrowthyLog_Social_RECEIVE_WAKEUP_REWARD()
    {
        if (SDKGameProfileManager._instance.TryGetPlayingFriend(item.userKey, out UserFriend user))
        {
            var myProfile = SDKGameProfileManager._instance.GetMyProfile();

            var social = new ServiceSDK.GrowthyCustomLog_Social
            (
                lastStage: myProfile.stage.ToString(),
                socialType: ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.RECEIVE_WAKEUP_REWARD,
                friendUid: user._userKey
            );

            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(social);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", jsonData);
        }
    }
}
