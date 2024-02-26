using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameItemType
{
    NONE,
    HAMMER, //1
    CROSS_LINE,
    THREE_HAMMER, //3
    RAINBOW_BOMB_HAMMER,
    HEAL_ONE_ANIMAL, //5
    SKILL_POINT_CHARGE,
    ADVENTURE_RAINBOW_BOMB, //7
}

public class GameItem : MonoBehaviour
{
    const string HAMMER_SPRITE_NAME = "itemHammer";
    const string CROSS_LINE_SPRITE_NAME = "itembombbox";
    const string RAINBOW_SPRITE_NAME = "item33RainbowHammer";
    const string THREE_HAMMER_SPRITE_NAME = "itemPowerHammer";

    const string HEAL_ONE_ANIMAL_SPRITE_NAME = "item_Resurrection";
    const string SKILL_POINT_CHARGE_SPRITE_NAME = "item_Skill_charge";
    const string ADVENTURE_RAINBOW_BOMB_SPRITE_NAME = "item_Rainbow_Hammer";
    
    [System.NonSerialized]
    public Transform _transform;
    List<UIWidget> _listWidget = new List<UIWidget>();

    public GameItemType type = GameItemType.HAMMER;

    public GameObject _saleIcon;

    public UISprite _spriteBg;
    public UISprite _spriteBody;
    public UISprite _spriteCountBg;
    public UILabel _textCount;
    public UISprite _textureSale;
    public UILabel _freeCount;
    public UILabel _freeCountShadow;

    public bool _useItem = false;
    public bool _freeItem = false;

    [System.NonSerialized]
    public int _price = 10;

    float _clickTime = 0f;

    int freeItemCount = 0;
    int jewelCount = 0;

    public void useItem()
    {
        _spriteBody.cachedTransform.localPosition = new Vector3(8,-6,0);
        _spriteBody.color = new Color(85f/256f, 85f / 256f, 85f / 256f, 165f/256f);
 
        _spriteCountBg.gameObject.SetActive(false);
        _textCount.gameObject.SetActive(false);
        _textureSale.gameObject.SetActive(false);
        _freeCount.gameObject.SetActive(false);
        _freeCountShadow.gameObject.SetActive(false);
        _saleIcon.SetActive(false);
        _useItem = false;
    }

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
            _freeCount.text = "+"+count.ToString();
            _freeCountShadow.text = "+" + count.ToString();
        }
        else
        {
            _freeCount.gameObject.SetActive(false);
            _freeCountShadow.gameObject.SetActive(false);
            _spriteCountBg.gameObject.SetActive(true);
            _textCount.gameObject.SetActive(true);
            _textCount.text = jewelCount.ToString();
        }
    }

    public void Init(GameItemType tempType, int count , int tempJewelCount, bool bSale)
    {
        type = tempType;
        jewelCount = tempJewelCount;

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
            case GameItemType.RAINBOW_BOMB_HAMMER:
                _spriteBody.spriteName = RAINBOW_SPRITE_NAME;
                break;
            case GameItemType.THREE_HAMMER:
                _spriteBody.spriteName = THREE_HAMMER_SPRITE_NAME;
                break;
            case GameItemType.HEAL_ONE_ANIMAL:
                _spriteBody.spriteName = HEAL_ONE_ANIMAL_SPRITE_NAME;
                break;
            case GameItemType.SKILL_POINT_CHARGE:
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
            if(freeItemCount > 0)
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
                _textCount.text = jewelCount.ToString();
            }
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

            //_saleIcon.SetActive(false);
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

    public void SetInGameItem()
    {

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

        if (GameManager.instance.state != GameState.PLAY)
            return;

        if (GameManager.gameMode == GameMode.NORMAL && GameManager.instance.moveCount == 0)
            return;

        if (ManagerBlock.instance.state != BlockManagrState.WAIT)
            return;

        if (!ManagerBlock.instance.checkBlockWait())
            return;

        if (GameItemManager.instance != null) return;

        //재화 비교 결제창 
        if(EditManager.instance != null)
        {

        }
        else if(freeItemCount > 0)
        {
            _freeCount.text = (freeItemCount-1).ToString();
        }
        else if (jewelCount > Global.jewel)
        {         
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            //popupSystem.SortOrderSetting();
            popupSystem.FunctionSetting(1, "OpenDiaShop", gameObject, true);
            Texture2D texture = Resources.Load("Message/jewel") as Texture2D;
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_5"), false, texture);
   
            return;
        }

        GameItemManager useItem = NGUITools.AddChild(GameUIManager.instance.AnchorBottom.gameObject, GameUIManager.instance.gameItemManagerObj).GetComponent<GameItemManager>();
        useItem.gameObject.transform.localScale = Vector3.one;
        useItem.gameObject.transform.localPosition = Vector3.zero;
        NGUITools.SetLayer(useItem.gameObject, LayerMask.NameToLayer("InGame"));
        NGUITools.MarkParentAsChanged(useItem.gameObject);
        useItem.SetType(type);
        useItem.useGameItem = this;
        
        useItem.CreateItem();   
    }
    
    //아이템이 오픈되어 있는지 확인.
    private bool IsOpenItem()
    {
        //if (Global.GameInstance.CanUseIngameItem(type) == false || jewelCount <= 0)
        if (jewelCount <= 0)
            return false;
        return true;
    }

    void OpenDiaShop()
    {
        ManagerUI._instance.OnClickDiamondShop();
    }

}
