using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum GameItemType
{
    NONE,
    HAMMER, //1
    CROSS_LINE,
    THREE_HAMMER, //3
    RAINBOW_BOMB_HAMMER,
    HEAL_ONE_ANIMAL, //5
    SKILL_HAMMER,
    ADVENTURE_RAINBOW_BOMB, //7
    COLOR_BRUSH,
}

public class GameItem : MonoBehaviour
{
    private const string HAMMER_SPRITE_NAME       = "itemHammer";
    private const string CROSS_LINE_SPRITE_NAME   = "itembombbox";
    private const string RAINBOW_SPRITE_NAME      = "item33RainbowHammer";
    private const string THREE_HAMMER_SPRITE_NAME = "itemPowerHammer";
    private const string COLOR_BRUSH_SPRITE_NAME  = "item_color_brush";

    const string HEAL_ONE_ANIMAL_SPRITE_NAME = "item_Resurrection";
    const string SKILL_POINT_CHARGE_SPRITE_NAME = "item_Skill_charge";
    const string ADVENTURE_RAINBOW_BOMB_SPRITE_NAME = "item_Rainbow_Hammer";
    
    [System.NonSerialized]
    public Transform _transform;
    List<UIWidget> _listWidget = new List<UIWidget>();

    public GameItemType type = GameItemType.HAMMER;

    //가격이나 세일 관련된 오브젝트를 가지고 있음.
    [SerializeField]
    private GameObject costRoot;

    [SerializeField]
    private GameObject alarmObj;

    [SerializeField]
    private GameObject itemLightObj;

    public GameObject _saleIcon;

    public UISprite _spriteBg;
    public UISprite _spriteBody;
    public UISprite _spriteCountBg;
    public UILabel _textCount;
    public UISprite _textureSale;
    public UILabel _freeCount;
    public UILabel _freeCountShadow;

    //현재 아이템이 활성화 되어 있는지.
    public bool _useItem = false;

    public bool _freeItem = false;

    [System.NonSerialized]
    public int _price = 10;

    float _clickTime = 0f;

    int freeItemCount = 0;
    int currencyCount = 0;
    private bool isCoin = false;

    /// <summary>
    /// 인게임 아이템 UI 초기화
    /// </summary>
    public void Init(GameItemType tempType, int count , int tempCurrencyCount, bool bSale, bool isCoin)
    {
        type = tempType;
        currencyCount = tempCurrencyCount;
        itemLightObj.SetActive(false);
        this.isCoin = isCoin;

        //아이템이 오픈되지 않은 상황에서는 비활성화 시키고 처리 종료.
        if (EditManager.instance == null && IsOpenItem() == false)
        {
            _spriteBg.spriteName = "button_skill_lock";
            _spriteBg.MakePixelPerfect();
            _spriteBody.gameObject.SetActive(false);
            _spriteCountBg.gameObject.SetActive(false);
            _textCount.gameObject.SetActive(false);
            _textureSale.gameObject.SetActive(false);
            _freeCount.gameObject.SetActive(false);
            _freeCountShadow.gameObject.SetActive(false);
            _saleIcon.SetActive(false);
            _useItem = false;
            SetAlarmIcon_IngameItemGuide(false);

            return;
        }

        _useItem = true;

        _spriteBg.spriteName = "button_skill_bg";
        _spriteBg.MakePixelPerfect();

        _textureSale.gameObject.SetActive(false);
        _textureSale.enabled = false;
        _spriteCountBg.enabled = true;
        _spriteBody.enabled = true;
        _textCount.enabled = true;
        GetComponent<Collider>().enabled = true;

        freeItemCount = count;

        if (isCoin)
        {
            _spriteCountBg.spriteName = "icon_coin_shadow";
            _textCount.pivot = UIWidget.Pivot.Right;
            _textCount.transform.localPosition = new Vector2(54f, -55f);
            _textCount.fontSize = 26;
        }

        foreach (UIWidget wi in _listWidget)
        {
            wi.color = Color.white;
        }

        switch( tempType )
        {
            case GameItemType.HAMMER:
                _spriteBody.spriteName = HAMMER_SPRITE_NAME;
                break;
            case GameItemType.CROSS_LINE:
                _spriteBody.spriteName = CROSS_LINE_SPRITE_NAME;
                break;
            case GameItemType.THREE_HAMMER:
                _spriteBody.spriteName = THREE_HAMMER_SPRITE_NAME;
                break;
            case GameItemType.RAINBOW_BOMB_HAMMER:
                _spriteBody.spriteName = RAINBOW_SPRITE_NAME;
                break;
            case GameItemType.COLOR_BRUSH:
                _spriteBody.spriteName = COLOR_BRUSH_SPRITE_NAME;
                break;
            case GameItemType.HEAL_ONE_ANIMAL:
                _spriteBody.spriteName = HEAL_ONE_ANIMAL_SPRITE_NAME;
                break;
            case GameItemType.SKILL_HAMMER:
                _spriteBody.spriteName = SKILL_POINT_CHARGE_SPRITE_NAME;
                break;
            case GameItemType.ADVENTURE_RAINBOW_BOMB:
                _spriteBody.spriteName = ADVENTURE_RAINBOW_BOMB_SPRITE_NAME;
                break;
        }
        
        _spriteBody.MakePixelPerfect();
        _spriteBody.gameObject.transform.localScale = Vector3.one * 0.9f;

        if (tempType != GameItemType.NONE)
        {
            RefreshCount(freeItemCount);
        }

        if (bSale == true)
        {
            _textureSale.gameObject.SetActive(true);
            _textureSale.enabled = true;

            _saleIcon.SetActive(true);
        }
        else
        {
            _textureSale.gameObject.SetActive(false);
            _textureSale.enabled = false;
        }

        //가이드 알림 표시(인게임 아이템 튜토리얼이 등장하지 않을 때만 가이드 알림 표시)
        if (IsItemType_HasIngameItemGuide() == true && IsShowIngameItemTutorial_AtCurrentStage() == false)
            SetAlarmIcon_IngameItemGuide_CheckPlayerPrefs();
        else
            SetAlarmIcon_IngameItemGuide(false);
    }

    /// <summary>
    /// 아이템 UI 갱신
    /// </summary>
    public void RefreshItemUI(bool isCanUseItem = false, bool isAction = true)
    {
        //오픈되어 있지 않은 아이템이면 UI 갱신하지 않음.
        if (IsOpenItem() == false)
            return;

        if (Global.GameType == GameType.ADVENTURE || Global.GameType == GameType.ADVENTURE_EVENT)
        {
            //탐험 인게임 아이템 무제한 사용 이벤트가 오픈되어 있는 상태인지 검사
            if (Global.GameInstance.GetProp(GameTypeProp.APPLY_UNLIMITED_INGAMEITEM) == true
                && ServerRepos.LoginCdn.unlimitedAdvItemsEventEndTs > ServerRepos.GameStartTs)
            {
                //참여 조건인 1-3 스테이지 이상인지 검사
                if (!(Global.stageIndex   < GameItemManager.UNLIMITED_ADVENTURE_ITEM_EVENT_OPEN_STAGE &&
                      Global.chapterIndex == GameItemManager.UNLIMITED_ADVENTURE_ITEM_EVENT_OPEN_CHAPTER))
                {
                    isAction     = false;
                    isCanUseItem = true;
                }
            }
        }
        else
        { 
            //인게임 아이템 무제한 사용 이벤트 오픈되어 있는 상태인지 검사
            if (Global.GameInstance.GetProp(GameTypeProp.APPLY_UNLIMITED_INGAMEITEM) == true
                && ServerRepos.LoginCdn.unlimitedItemsEventEndTs > ServerRepos.GameStartTs)
            {
                isAction     = false;
                isCanUseItem = true;
            }

        }
        
        bool isUsedItem = !_useItem;

        //아이템 활성화 상태 갱신.
        _useItem = isCanUseItem;

        //아이템을 이미 사용한 경우와 아닌 경우 설정.
        if (isCanUseItem == true)
        {
            //이미 사용했었던 아이템만 갱신
            if (isUsedItem == true)
            {
                //인게임 아이템 오픈 연출
                StartCoroutine(CoRefreshItemAction(isAction));
            }
        }
        else
        {
            _spriteBody.cachedTransform.localPosition = new Vector3(8, -6, 0);
            _spriteBody.color = new Color(85f / 256f, 85f / 256f, 85f / 256f, 165f / 256f);
            costRoot.SetActive(isCanUseItem);
        }
    }

    private IEnumerator CoRefreshItemAction(bool isAction = true)
    {
        if (isAction == true)
        {
            yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.5f));

            //아이템 스케일 액션
            float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
            _spriteBody.transform.DOScale(0.5f, actionTime);
            yield return new WaitForSeconds(actionTime);
        }

        //인게임 아이템 가격 표시.
        GameUIManager.instance.RefreshInGameItem();

        //인게임 아이템 알림 설정.
        SetAlarmIcon_IngameItemGuide_CheckPlayerPrefs();

        //가격 관련 UI 표시.
        costRoot.SetActive(true);

        if (isAction == true)
        {
            float actionTime = ManagerBlock.instance.GetIngameTime(0.5f);

            //빛 돌리기
            itemLightObj.SetActive(true);
            itemLightObj.transform.DORotate(new Vector3(0f, 0f, 200f), actionTime);

            //아이템 스케일 액션
            _spriteBody.transform.DOScale(0.9f, actionTime).SetEase(Ease.OutBack);

            //아이템 컬러 액션
            DOTween.To(() => _spriteBody.color, x => _spriteBody.color = x, Color.white, ManagerBlock.instance.GetIngameTime(0.3f));

            yield return new WaitForSeconds(actionTime);

            itemLightObj.transform.rotation = Quaternion.identity;
            itemLightObj.SetActive(false);
        }
        _spriteBody.color = Color.white;
    }

    /// <summary>
    /// 아이템 남은 카운트/ 가격 표시 갱신.
    /// </summary>
    public void RefreshCount(int count)
    {
        if (_useItem == false)
            
            return;

        freeItemCount = count;
        
        if (count > 0)
        {
            _freeCount.gameObject.SetActive(true);
            _freeCountShadow.gameObject.SetActive(true);
            _spriteCountBg.gameObject.SetActive(false);
            _textCount.gameObject.SetActive(false);
            _freeCount.text = "+" + count.ToString();
            _freeCountShadow.text = "+" + count.ToString();
        }
        else
        {
            _freeCount.gameObject.SetActive(false);
            _freeCountShadow.gameObject.SetActive(false);
            _spriteCountBg.gameObject.SetActive(true);
            _textCount.gameObject.SetActive(true);
            _textCount.text = currencyCount.ToString();
        }
    }

    void Awake()
    {
        _transform = transform;
    }

    void Start()
    {
        _clickTime = Time.time;
        MakeWidgetList(gameObject);
    }

    public void MakeWidgetList(GameObject in_obj)
    {
        UIWidget widget = in_obj.GetComponent<UIWidget>();
        if (widget != null)
            _listWidget.Add(widget);

        for (int i = 0; i < in_obj.transform.childCount; i++)
            MakeWidgetList(in_obj.transform.GetChild(i).gameObject);
    }

    int GetItemPrice(GameItemType tempType)
    {
        return 0;
    }

    void SetDefaultItemUI()
    {
        _freeItem = false;
    }

    void SetFreeItemUI()
    {
        _freeItem = true;
    }

    void BtnUse()
    {
        if (_useItem == false) return;

        if (type == GameItemType.NONE) return;

        if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            if (AdventureManager.instance == null) return;
            //if (AdventureManager.instance.IsExistDeathAnimal() == false)
            //    return;
        }

        if (GameManager.instance.state != GameState.PLAY)
            return;

        if (GameManager.gameMode == GameMode.NORMAL && GameManager.instance.moveCount == 0)
            return;

        if (ManagerBlock.instance.state != BlockManagrState.WAIT)
            return;

        if (!ManagerBlock.instance.checkBlockWait())
            return;

        if (GameItemManager.instance != null)
            return;

        //해당 아이템의 설명을 본 적이 없다면, 강제로 설명 팝업 호출 시켜줌.
        if (IsItemType_HasIngameItemGuide() == true && ManagerTutorial._instance == null)
        {
            string key = string.Format("Check_IngameItem_Guide_{0}", (int)type);
            if (PlayerPrefs.HasKey(key) == false)
            {
                PlayerPrefs.SetInt(key, 1);
                SetAlarmIcon_IngameItemGuide(false);
                UIPopupShowAPNG apngPopup = OpenPopup_IngameItemGuide();
                apngPopup._callbackClose += () => UseGameItem();
                return;
            }
        }

        UseGameItem();
    }

    private void UseGameItem()
    {
        //재화 비교 결제창 
        if (EditManager.instance != null)
        {

        }
        else if (freeItemCount > 0)
        {
            _freeCount.text = (freeItemCount - 1).ToString();
        }
        else
        {
            if (isCoin)
            {
                if (currencyCount > Global.coin)
                {
                    ManagerUI._instance.LackCoinsPopUp(true);
                    return;
                }
            }
            else
            {
                if (currencyCount > Global.jewel)
                {
                    ManagerUI._instance.LackDiamondsPopUp();
                    return;
                }
            }
        }

        GameItemManager useItem = NGUITools.AddChild(GameUIManager.instance.AnchorBottom.gameObject, GameUIManager.instance.gameItemManagerObj).GetComponent<GameItemManager>();
        useItem.gameObject.transform.localScale = Vector3.one;
        useItem.gameObject.transform.localPosition = new Vector3(0, 30f, 0);
        NGUITools.SetLayer(useItem.gameObject, LayerMask.NameToLayer("InGame"));
        NGUITools.MarkParentAsChanged(useItem.gameObject);

        useItem.SetType(type);
        useItem.InitGameItemManager(this);
        useItem.CreateItem();
    }

    //아이템이 오픈되어 있는지 확인.
    private bool IsOpenItem()
    {
        if (Global.GameInstance.CanUseIngameItem(type) == false || currencyCount <= 0)
            return false;
        return true;
    }

    //현재 스테이지에서 인게임 아이템 튜토리얼을 보여주는 지 검사.
    public bool IsShowIngameItemTutorial_AtCurrentStage()
    {
        //통상 모드가 아니라면 인게임 아이템 튜토리얼이 뜨지 않으니 검사 종료.
        if (Global.GameType != GameType.NORMAL)
            return false;

        //해당 아이템 타입에 따라 오픈되는 스테이지를 받아옴.
        int stage = 0;
        switch (type)
        {
            case GameItemType.HAMMER:
                stage = GameItemManager.HAMMER_OPEN_STAGE;
                break;
            case GameItemType.CROSS_LINE:
                stage = GameItemManager.CROSS_LINE_OPEN_STAGE;
                break;
            case GameItemType.THREE_HAMMER:
                stage = GameItemManager.HAMMER3X3_OPEN_STAGE;
                break;
            case GameItemType.RAINBOW_BOMB_HAMMER:
                stage = GameItemManager.RAINBOW_HAMMER_OPEN_STAGE;
                break;
            case GameItemType.COLOR_BRUSH:
                stage = GameItemManager.COLOR_BRUSH_OPEN_STAGE;
                break;
            default:
                return false;
        }

        //현재 스테이지가 아이템이 오픈되는 스테이지가 아닐 경우 검사 종료.
        if (Global.stageIndex != stage)
            return false;

        //현재 스테이지에서 튜토리얼이 뜰 수 있는지 검사해서 값 반환.
        return GameManager.instance.IsCanPlayTutorialStage();
    }

    #region 인게임 아이템 가이드 관련

    //인게임 아이템 가이드를 띄우는 아이템 타입인지 검사
    public bool IsItemType_HasIngameItemGuide()
    {
        if (type == GameItemType.HAMMER || type == GameItemType.CROSS_LINE || type == GameItemType.THREE_HAMMER || type == GameItemType.RAINBOW_BOMB_HAMMER || type == GameItemType.COLOR_BRUSH)
            return true;
        else
            return false;
    }

    //PlyaerPrefs의 데이터를 이용해 가이드 알림 표시.
    public void SetAlarmIcon_IngameItemGuide_CheckPlayerPrefs()
    {
        string key = string.Format("Check_IngameItem_Guide_{0}", (int)type);

        bool isActive = false;
        if (IsItemType_HasIngameItemGuide())
            isActive = PlayerPrefs.HasKey(key) == false;
        else
            isActive = false;

        SetAlarmIcon_IngameItemGuide(isActive);
    }

    //가이드 알림 표시 설정.
    public void SetAlarmIcon_IngameItemGuide(bool isActive)
    {
        alarmObj.SetActive(isActive);
    }

    private UIPopupShowAPNG OpenPopup_IngameItemGuide()
    {
        string filePath = string.Format("Guide_IngameItem_{0}.png", (int)type);

        //인게임 아이템 가이드 팝업 호출
        return ManagerUI._instance.OpenPopupShowAPNG(filePath, mainSpritePos: new Vector2(0f, -30f));
    }
    #endregion
}
