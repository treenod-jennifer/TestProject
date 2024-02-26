using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class GameItemManager : MonoBehaviour
{
    public const int HAMMER_OPEN_STAGE         = 8;
    public const int CROSS_LINE_OPEN_STAGE     = 17;
    public const int HAMMER3X3_OPEN_STAGE      = 39;
    public const int RAINBOW_HAMMER_OPEN_STAGE = 48;
    public const int COLOR_BRUSH_OPEN_STAGE    = 13;

    public const int SKILL_HAMMER_OPEN_CHAPTER = 1;
    public const int SKILL_HAMMER_OPEN_STAGE = 4;

    public const int UNLIMITED_ADVENTURE_ITEM_EVENT_OPEN_CHAPTER = 1;
    public const int UNLIMITED_ADVENTURE_ITEM_EVENT_OPEN_STAGE = 3;

    [System.NonSerialized]
    public static GameItemManager instance = null;

    [System.NonSerialized]
    public Transform _transform;

    [System.NonSerialized]
    public GameItemType type = GameItemType.NONE;

    public GameObject flyGameItemObj;
    public UISprite _uiClipping;
    public UISprite _CenterClipping;
    public UILabel _textMessage;

    //아이템 안내 버튼
    [SerializeField]
    private GameObject itemGuideButton;

    [SerializeField]
    private UILabel itemGuideText;

    public GameItem useGameItem = null;

    [System.NonSerialized]
    public BlockBase _selectBlock;
    InGameAnimal selectAnimal;

    [System.NonSerialized]
    public bool used = false;

    [System.NonSerialized]
    public FlyGameItem _flyItem;
    
    
    // 일본어 환경 / 기타 환경에서 위치, 활성화, 문구 등을 상이하게 설정하기 위한 오브젝트들 (일본 법률 개정 이슈)
    [SerializeField] private Transform root_BtnAnimPlay;
    [SerializeField] private Transform btn_PopupClose;
    [SerializeField] private UISprite btn_AnimPlay;
    [SerializeField] private Transform icon_AnimPlay;
    [SerializeField] private UILabel label_AnimPlay;
    [SerializeField] private UILabel label_MainInfo;
    [SerializeField] private UILabel label_BuyInfo;
    [SerializeField] private UIPanel panel_price;
    [SerializeField] private UISprite sprite_price;
    [SerializeField] private UILabel label_price;
    
    /// <summary>
    /// 0:해머, 1:크로스해머, 2:파워해머, 3:레인보우폭탄해머, 4:부활해머, 5:스킬해머, 6:레인보우해머, 7:컬러붓
    /// </summary>
    public static int[] useCount = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

    public static event System.Action OpenEvent;
    public static event System.Action CloseEvent;

    //레인보우 스틱 테스트 카운트 증가
    public event System.Action SuccessEvent;

    void Awake()
    {
        instance = this;
        _transform = transform;
        _selectBlock = null;
        used = false;

        OpenEvent?.Invoke();
    }

    void Start()
    {
        _CenterClipping.gameObject.transform.parent = GameUIManager.instance.groundAnchor.transform;
        _CenterClipping.gameObject.transform.localScale = Vector3.one;
        _CenterClipping.gameObject.transform.localPosition = new Vector3(0, -GameManager.MOVE_Y * 78, 0);
        _CenterClipping.depth = (int)GimmickDepth.INGAMEITEM;
        StartCoroutine(FadeOut());

        NGUITools.SetLayer(_CenterClipping.gameObject, LayerMask.NameToLayer("InGame"));
        NGUITools.MarkParentAsChanged(_CenterClipping.gameObject);

        GameUIManager.instance.ShowFlower(false);
        GameManager.instance.SetActive_HelpButtonUI(false);
    }

    public void SetType(GameItemType temptype)
    {
        type = temptype;

        if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            if (AdventureManager.instance != null)
            {
                AdventureManager.instance.HealItemGuideOn(true);
            }
        }
        else if(type == GameItemType.SKILL_HAMMER)
        {
            if (AdventureManager.instance != null)
            {
                AdventureManager.instance.SkillItemGuideOn(true);
            }
        }
        else
        {            
            ShowBlock();
        }
    }

    public void InitGameItemManager(GameItem gameItem)
    {
        //게임 아이템 설정
        useGameItem = gameItem;

        //설명이 필요한 아이템의 경우만 처리.
        if (useGameItem.IsItemType_HasIngameItemGuide() == true && ManagerTutorial._instance == null)
        {
            itemGuideButton.SetActive(true);
            itemGuideText.text = Global._instance.GetString("btn_52");
        }
        else
        {
            itemGuideButton.SetActive(false);
        }
        
        // 일본어 환경 / 기타 환경에 따라 오브젝트 위치값 등을 세팅
        if (LanguageUtility.IsShowBuyInfo)
        {
            label_BuyInfo.text = (GameManager.gameMode == GameMode.ADVENTURE)? Global._instance.GetString("buyinfo_advig_1") : Global._instance.GetString("buyinfo_ig_1");
            label_BuyInfo.gameObject.SetActive(true);
            label_MainInfo.alignment = NGUIText.Alignment.Left;
            label_MainInfo.width = 400;
            label_MainInfo.fontSize = 22;
            label_AnimPlay.width = 100;
            btn_AnimPlay.width = 146;
            btn_AnimPlay.height = 54;
            icon_AnimPlay.localPosition = new Vector2(-60f, 3f);
            label_AnimPlay.transform.localPosition = new Vector2(14f, 3f);
            root_BtnAnimPlay.localPosition = new Vector2(-290f, 100f);
            btn_PopupClose.localPosition = new Vector2(334f, 100f);
            label_MainInfo.transform.localPosition = new Vector2(0, 100f);
            SetItemPriceUI();
        }
        else
            label_BuyInfo.gameObject.SetActive(false);
    }

    public void BtnCancel()
    {
        if (used) return;

        if (_CenterClipping != null)
            Destroy(_CenterClipping.gameObject);

        GameUIManager.instance.RefreshInGameItem();
        GameUIManager.instance.ShowFlower(true);

        Destroy(_flyItem.gameObject);
        Destroy(gameObject);
    }

    public void Close()
    {
        if (_CenterClipping != null)
            Destroy(_CenterClipping.gameObject);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        HideAllBlock();
        GameUIManager.instance.ShowFlower(true);
        GameManager.instance.SetActive_HelpButtonUI(true);
        CloseAdventureGuide();

        CloseEvent?.Invoke();

        instance = null;
    }

    bool isFreeItem = false;
    public void UseGameItem(BlockBase tempBlock)
    {
        if (tempBlock == null) return;

        if (used) return;
        used = true;

        Board tempBoard = PosHelper.GetBoardSreeen(tempBlock.indexX, tempBlock.indexY);
        if (tempBoard == null) return;
        if (tempBoard.HasDecoHideBlock()) return;
        if (tempBoard.HasDecoCoverBlock()) return;
        selectBlock = tempBlock;

        if (EditManager.instance == null)
        {
            //그로씨 머니 작성
            if (type != GameItemType.ADVENTURE_RAINBOW_BOMB)
            {
                var itemIndex = (int)type - 1;
                var priceList = Global.GameInstance.GetItemCostList(ItemType.INGAME_ITEM);
                
                if(type == GameItemType.COLOR_BRUSH)
                {
                    itemIndex = 4;
                }
                
                if (ServerRepos.UserItem.InGameItem(itemIndex) == 0)
                {
                    if (Global.GameInstance.GetItemCostType(ItemType.INGAME_ITEM) == RewardType.coin)
                    {
                        if ((int)ServerRepos.User.coin >= priceList[itemIndex])
                        {
                            usePCoin = priceList[itemIndex];
                        }
                        else if ((int)ServerRepos.User.coin > 0)
                        {
                            usePCoin = (int)ServerRepos.User.coin;
                            useFCoin = priceList[itemIndex] - (int)ServerRepos.User.coin;
                        }
                        else
                        {
                            useFCoin = priceList[itemIndex];
                        }
                    }
                    else if (Global.GameInstance.GetItemCostType(ItemType.INGAME_ITEM) == RewardType.jewel)
                    {
                        if ((int)ServerRepos.User.jewel >= priceList[itemIndex])
                        {
                            usePJewel = priceList[itemIndex];
                        }
                        else if ((int)ServerRepos.User.jewel > 0)
                        {
                            usePJewel = (int)ServerRepos.User.jewel;
                            useFJewel = priceList[itemIndex] - (int)ServerRepos.User.jewel;
                        }
                        else
                        {
                            useFJewel = priceList[itemIndex];
                        }
                    }
                }
                else
                {
                    isFreeItem = true;
                }
            }
            else
            {
                int itemAdventureIndex = (int)type - 5;

                if (ServerRepos.UserItem.AdventureItem(itemAdventureIndex) == 0)
                {
                    if ((int)ServerRepos.User.jewel >= ServerRepos.LoginCdn.RenewalAdvIngameItems[itemAdventureIndex])
                    {
                        usePJewel = ServerRepos.LoginCdn.RenewalAdvIngameItems[itemAdventureIndex];
                    }
                    else if ((int)ServerRepos.User.jewel > 0)
                    {
                        usePJewel = (int)ServerRepos.User.jewel;
                        useFJewel = ServerRepos.LoginCdn.RenewalAdvIngameItems[itemAdventureIndex] - (int)ServerRepos.User.jewel;
                    }
                    else
                    {
                        useFJewel = ServerRepos.LoginCdn.RenewalAdvIngameItems[itemAdventureIndex];
                    }
                }
                else
                {
                    isFreeItem = true;
                }
            }

            if (type == GameItemType.HAMMER)
            {
                ServerAPI.UseInGameItem(6, recvGameItem);
            }
            else if (type == GameItemType.CROSS_LINE)
            {
                ServerAPI.UseInGameItem(7, recvGameItem);
            }
            else if (type == GameItemType.THREE_HAMMER)
            {
                ServerAPI.UseInGameItem(8, recvGameItem); //
            }
            else if (type == GameItemType.RAINBOW_BOMB_HAMMER)
            {
                ServerAPI.UseInGameItem(9, recvGameItem); //RAINBOW_BOMB
            }
            else if (type == GameItemType.COLOR_BRUSH)
            {
                ServerAPI.UseInGameItem(10, recvGameItem);
            }
            else if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
            {
                ServerAPI.AdventureUseInGameItem(13, recvGameItem);
            }

            InGameEffectMaker.instance.MakeBombMakePangEffect(tempBlock._transform.position);
        }
        else
        {
            isFreeItem = true;

            BaseResp tempResp = new BaseResp();
            tempResp.code = (int)ServerError.Success;
            recvGameItem(tempResp);

            InGameEffectMaker.instance.MakeBombMakePangEffect(tempBlock._transform.position);
        }
    }


    public void UseAnimalItem(InGameAnimal tempAnimal = null)
    {
        if (used) return;
        used = true;

        if (EditManager.instance == null)
        {
            int itemAdventureIndex = (int)type - 5;

            switch (type)
            {
                case GameItemType.HEAL_ONE_ANIMAL:
                    itemAdventureIndex = 0;
                    break;
                case GameItemType.SKILL_HAMMER:
                    itemAdventureIndex = 1;
                    break;
            }

            if(ServerRepos.UserItem.AdventureItem(itemAdventureIndex) == 0)
            {
                if ((int)ServerRepos.User.jewel >= ServerRepos.LoginCdn.RenewalAdvIngameItems[itemAdventureIndex])
                {
                    usePJewel = ServerRepos.LoginCdn.RenewalAdvIngameItems[itemAdventureIndex];
                }
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = ServerRepos.LoginCdn.RenewalAdvIngameItems[itemAdventureIndex] - (int)ServerRepos.User.jewel;
                }
                else
                {
                    useFJewel = ServerRepos.LoginCdn.RenewalAdvIngameItems[itemAdventureIndex];
                }
            }
            else
            {
                isFreeItem = true;
            }

            selectAnimal = tempAnimal;

            if (type == GameItemType.HEAL_ONE_ANIMAL)
            {
                ServerAPI.AdventureUseInGameItem(11, recvAdventureItem);
            }
            else if (type == GameItemType.SKILL_HAMMER)
            {
                ServerAPI.AdventureUseInGameItem(12, recvAdventureItem);
            }
        }
    }

    BlockBase selectBlock = null;
    int useFJewel = 0;
    int usePJewel = 0;
    int useFCoin = 0;
    int usePCoin = 0;

    void recvAdventureItem(BaseResp code)
    {
        if (code.IsSuccess)
        {
            StartCoroutine(FadeIn());

            if (useGameItem.type == GameItemType.HEAL_ONE_ANIMAL)
            {
                useCount[4]++;
            }
            else if (useGameItem.type == GameItemType.SKILL_HAMMER)
            {
                useCount[5]++;
            }

            useGameItem.RefreshItemUI();
            
            GameUIManager.instance.RefreshInGameItem();
            FlyGameItem.instance.UseAnimalItem(selectAnimal);
            ManagerSound.AudioPlay(AudioInGame.INGAME_ITEM_CLICK);

            Global.jewel = (int)GameData.Asset.AllJewel;
            Global.wing = (int)GameData.Asset.AllWing;
            Global.exp = (int)GameData.User.expBall;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();

            //그로씨
            if (isFreeItem == false)
            {
                var useReadyItemGet = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                        "InGameItem" + ((int)useGameItem.type).ToString(),
                        "InGameItem" + useGameItem.type.ToString(),
                        1,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                        Global.GameInstance.GetGrowthyGameMode().ToString()
                    );
                var GetDoc = JsonConvert.SerializeObject(useReadyItemGet);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", GetDoc);
            }

            var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                (
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                    "InGameItem" + ((int)useGameItem.type).ToString(),
                    "InGameItem" + useGameItem.type.ToString(),
                    -1,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM,
                    Global.GameInstance.GetGrowthyGameMode().ToString()
                );
            var doc = JsonConvert.SerializeObject(useReadyItem);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

            if (isFreeItem == false)
            {
                if (usePJewel > 0 || useFJewel > 0)
                {
                    var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                        (
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                        -usePJewel,
                        -useFJewel,
                        (int)(ServerRepos.User.jewel),
                        (int)(ServerRepos.User.fjewel),
                        "InGameItem" + useGameItem.type.ToString()
                        );

                    var docMoney = JsonConvert.SerializeObject(growthyMoney);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                }
            }

        }
        else
        {
            used = false;
            BtnCancel();
        }
    }

    void recvGameItem(BaseResp code)
    {
        if (code.IsSuccess)
        {
            StartCoroutine(FadeIn());

            if (useGameItem.type == GameItemType.HAMMER)
                useCount[0]++;
            if (useGameItem.type == GameItemType.CROSS_LINE)
                useCount[1]++;
            if (useGameItem.type == GameItemType.THREE_HAMMER)
                useCount[2]++;
            if (useGameItem.type == GameItemType.RAINBOW_BOMB_HAMMER)//RAINBOW_BOMB
                useCount[3]++;
            if (useGameItem.type == GameItemType.COLOR_BRUSH)
                useCount[7]++;
            if (useGameItem.type == GameItemType.ADVENTURE_RAINBOW_BOMB)//RAINBOW_BOMB
                useCount[6]++;

            useGameItem.RefreshItemUI();
            GameUIManager.instance.RefreshInGameItem();
            FlyGameItem.instance.UseItem(selectBlock);
            ManagerSound.AudioPlay(AudioInGame.INGAME_ITEM_CLICK);

            if (EditManager.instance == null)
            {
                Global.jewel = (int)GameData.Asset.AllJewel;
                Global.coin = (int)GameData.Asset.AllCoin;
                Global.wing = (int)GameData.Asset.AllWing;
                Global.exp = (int)GameData.User.expBall;
            }

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();

            if (EditManager.instance == null)
            {
                //그로씨
                if (isFreeItem == false)
                {
                    var useReadyItemGet = new ServiceSDK.GrowthyCustomLog_ITEM
                        (
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                            "InGameItem" + ((int)useGameItem.type).ToString(),
                            "InGameItem" + useGameItem.type.ToString(),
                            1,
                            ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BUY_ITEM,
                            Global.GameInstance.GetGrowthyGameMode().ToString()
                        );
                    var GetDoc = JsonConvert.SerializeObject(useReadyItemGet);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", GetDoc);
                }


                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                        "InGameItem" + ((int)useGameItem.type).ToString(),
                        "InGameItem" + useGameItem.type.ToString(),
                        -1,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_USE_ITEM,
                        Global.GameInstance.GetGrowthyGameMode().ToString()
                    );
                var doc = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

                if (isFreeItem == false)
                {
                    int itemIndex = (int)type - 1;
                    if (usePJewel > 0 || useFJewel > 0)
                    {
                        var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                            (
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                            -usePJewel,
                            -useFJewel,
                            (int)(ServerRepos.User.jewel),
                            (int)(ServerRepos.User.fjewel),
                            "InGameItem" + useGameItem.type.ToString()
                            );

                        var docMoney = JsonConvert.SerializeObject(growthyMoney);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                    }
                    else if (usePCoin > 0 || useFCoin > 0)
                    {
                        var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                        (
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                            ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_CONSUMPTION,
                            -usePCoin,
                            -useFCoin,
                            (int)(ServerRepos.User.coin),
                            (int)(ServerRepos.User.fcoin),
                            "InGameItem" + useGameItem.type.ToString()
                        );

                        var docMoney = JsonConvert.SerializeObject(growthyMoney);
                        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                    }
                }
            }
        }
        else
        {
            used = false;
            BtnCancel();
        }
    }


    public void CreateItem()
    {
        if(type == GameItemType.HAMMER)
        {
            _textMessage.text = Global._instance.GetString("s_i_1");
        }
        else if (type == GameItemType.CROSS_LINE )
        {
            _textMessage.text = Global._instance.GetString("s_i_2");
        }
        else if (type == GameItemType.THREE_HAMMER)
        {
            _textMessage.text = Global._instance.GetString("s_i_3");
        }
        else if (type == GameItemType.RAINBOW_BOMB_HAMMER)
        {
            _textMessage.text = Global._instance.GetString("s_i_4");
        }
        else if (type == GameItemType.COLOR_BRUSH)
        {
            _textMessage.text = Global._instance.GetString("s_i_5");
        }
        //TODO 모험모드 아이템 추가
        else if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            _textMessage.text = Global._instance.GetString("ad_i_1");
        }
        else if (type == GameItemType.SKILL_HAMMER)
        {
            _textMessage.text = Global._instance.GetString("ad_i_2");
        }
        else if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
        {
            _textMessage.text = Global._instance.GetString("ad_i_3");
        }
                
        CreateFlyItem();
    }

    void CreateFlyItem()
    {
        if (FlyGameItem.instance == null)
        {
            _flyItem = NGUITools.AddChild(GameUIManager.instance.AnchorBottom, flyGameItemObj).GetComponent<FlyGameItem>();

            if (LanguageUtility.IsShowBuyInfo)
            {
                _flyItem.transform.localPosition = new Vector3(220f, 60f, 0f);
                _flyItem.transform.localScale    = new Vector3(0.8f, 0.8f, 1f);
            }
            else
            {
                _flyItem.transform.localPosition = new Vector3(0, 140f, 0f);
            }

            _flyItem.type = type;
            
            if (panel_price.enabled)
            {
                var skelRenderer = _flyItem._adventureAnimaion.GetComponent<MeshRenderer>();
                panel_price.sortingOrder = skelRenderer.sortingOrder + 1;
            }
        }
    }

    void ShowBlock()
    {
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board back = PosHelper.GetBoardSreeen(j, i);
                if (back != null && back.Block != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock())
                {
                    if (back.Block.IsNormalBlock() && back.Block as NormalBlock
                        && (back.Block.blockDeco == null || back.Block.blockDeco.IsInterruptBlockSelect() == false))
                    {
                        back.Block.mainSprite.depth = (int)GimmickDepth.INGAMEITEM + 1;
                        if(back.Block.type == BlockType.NORMAL)
                        {
                            NormalBlock normalBlock = back.Block as NormalBlock;
                            if(normalBlock.toyBombSprite != null) normalBlock.toyBombSprite.depth = (int)GimmickDepth.INGAMEITEM + 2;
                        }

                        if (back.Block.specialEventSprite != null)
                        {
                            back.Block.specialEventSprite.depth = (int)GimmickDepth.INGAMEITEM + 3;
                        }
                        if (back.Block.collectBlock_Alphabet != null)
                        {
                            back.Block.collectBlock_Alphabet.SetDepth((int)GimmickDepth.INGAMEITEM + 3);
                        }
                    }
                }
            }
        }
    }

    void HideAllBlock()
    {
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board back = PosHelper.GetBoardSreeen(j, i);
                if (back != null && back.Block != null && !back.HasDecoCoverBlock() && !back.Block.isRainbowBomb)
                {
                    //back.Block.UpdateSpriteByBlockType();
                    back.Block.SetMainSpriteDepth();
                    if (back.Block.type == BlockType.NORMAL)
                    {
                        NormalBlock normalBlock = back.Block as NormalBlock;
                        if (normalBlock.toyBombSprite != null) normalBlock.toyBombSprite.depth = normalBlock.mainSprite.depth + 1;
                    }

                    if (back.Block.specialEventSprite != null)
                    {
                        back.Block.specialEventSprite.depth = back.Block.mainSprite.depth + 2;
                    }

                    if (back.Block.collectBlock_Alphabet != null)
                    {
                        back.Block.collectBlock_Alphabet.SetDepth(back.Block.mainSprite.depth + 2);
                    }
                }
            }
        }
    }

    IEnumerator FadeOut(float speed = 1.2f)
    {
        var color = _CenterClipping.color;
        color.a = 0.01f;
        _CenterClipping.color = color;

        float time = 0f;

        while (true)
        {
            if (time > 1f)
                break;

            time += Global.deltaTimePuzzle;
            _CenterClipping.color = new Color(1f, 1, 1f, Mathf.Lerp(0f, 0.4f, time * 4f));
            yield return null;
        }

        _CenterClipping.color = new Color(1f, 1f, 1f, 0.4f);
        yield return null;
    }

    IEnumerator FadeIn(float speed = 1.5f)
    {
        var color = _CenterClipping.color;
        float time = 0f;
        while (true)
        {
            if (time > 1f)
                break;

            time += Global.deltaTimePuzzle;
            _CenterClipping.color = new Color(1f, 1f, 1f, Mathf.Lerp(color.a, 0f, time * 4f));
            yield return null;
        }

        
        _CenterClipping.color = new Color(1f, 1f, 1f, 0f);
        yield return null;

        /*
        if (EditManager.instance != null)
            used = false;
        */

        while (used)
        {
            yield return new WaitForSeconds(0.1f);
        }
        GameUIManager.instance.ShowFlower(true);
        yield return null;
        Destroy(gameObject);
        yield return null;
    }

    void CloseAdventureGuide()
    {
        if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            AdventureManager.instance.HealItemGuideOn(false);
        }
        else if (type == GameItemType.SKILL_HAMMER)
        {
            AdventureManager.instance.SkillItemGuideOn(false);
        }
    }

    private void SetItemPriceUI()
    {
        int price = 0;
        List<int> itemPriceList = Global.GameInstance.GetItemCostList(ItemType.INGAME_ITEM);
        List<int> itemAdvPriceList = Global.GameInstance.GetItemCostList(ItemType.INGAME_ITEM_ADVENTURE);
        if (Global.GameInstance.GetItemCostType(ItemType.INGAME_ITEM) == RewardType.coin)
            sprite_price.spriteName = "icon_coin_shadow";

        switch (type)
        {
            case GameItemType.HAMMER when ServerRepos.UserItem.InGameItem(0) <= 0:
                price = itemPriceList[0];
                break;
            case GameItemType.CROSS_LINE when ServerRepos.UserItem.InGameItem(1) <= 0:
                price = itemPriceList[1];
                break;
            case GameItemType.THREE_HAMMER when ServerRepos.UserItem.InGameItem(2) <= 0:
                price = itemPriceList[2];
                break;
            case GameItemType.RAINBOW_BOMB_HAMMER when ServerRepos.UserItem.InGameItem(3) <= 0:
                price = itemPriceList[3];
                break;
            case GameItemType.COLOR_BRUSH when ServerRepos.UserItem.InGameItem(4) <= 0:
                price = itemPriceList[4];
                break;
            case GameItemType.HEAL_ONE_ANIMAL when ServerRepos.UserItem.AdventureItem(0) <= 0:
                price = itemAdvPriceList[0];
                break;
            case GameItemType.SKILL_HAMMER when ServerRepos.UserItem.AdventureItem(1) <= 0:
                price = itemAdvPriceList[1];
                break;
            case GameItemType.ADVENTURE_RAINBOW_BOMB when ServerRepos.UserItem.AdventureItem(2) <= 0:
                price = itemAdvPriceList[2];
                break;
        }

        if (price > 0)
        {
            panel_price.gameObject.SetActive(true);
            label_price.text = price.ToString();
        }
    }

    #region 인게임 아이템 가이드 관련
    private void OnClickBtn_IngameItemGuide()
    {
        OpenPopup_IngameItemGuide();
    }

    private void OpenPopup_IngameItemGuide()
    {
        string filePath = string.Format("Guide_IngameItem_{0}.png", (int)type);

        //인게임 아이템 가이드 팝업 호출
        ManagerUI._instance.OpenPopupShowAPNG(filePath, mainSpritePos: new Vector2(0f, -30f));
    }
    #endregion

    /// <summary>
    /// 인게임 아이템 사용 PLAYEND 로그 데이터 리턴
    /// </summary>
    public static List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM> GetPlayEndInGameItemLogData()
    {
        var itemList = new List<ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM>();
        for (var i = 0; i < 4; i++)
        {
            if (useCount[i] > 0)
            {
                var ingameItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
                {
                    L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                    L_IID = ((GameItemType)i + 1).ToString(),
                    L_CNT = useCount[i]
                };
                itemList.Add(ingameItem);
            }
        }
        if (useCount[7] > 0) // 컬러붓
        {
            var ingameItem = new ServiceSDK.GrowthyCustomLog_PLAYEND.CLASS_L_ITEM
            {
                L_CAT = ServiceSDK.GrowthyCustomLog_PLAYEND.Code_L_CAT.CONSUMPTION_INGAME.ToString(),
                L_IID = GameItemType.COLOR_BRUSH.ToString(),
                L_CNT = useCount[7]
            };
            itemList.Add(ingameItem);
        }

        return itemList;
    }
}
