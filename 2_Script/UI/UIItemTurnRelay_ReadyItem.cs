using System.Collections;
using System.Reflection.Emit;
using UnityEngine;

public class UIItemTurnRelay_ReadyItem : MonoBehaviour
{
    private const string bgName_itemOn = "ready_item_on_01";
    private const string bgName_itemOff = "ready_item_on_02";
    private const string bgName_itemTime = "ready_item_on_03";

    private const string checkName_itemOn = "ready_button_01_on";
    private const string checkName_itemOff = "ready_button_01_off";

    public enum READYITEM_COSTTYPE
    {
        COIN,
        JEWEL,
    }

    public enum READYITEM_SALETYPE
    {
        NONE,
        SALE_NORMAL,
        SALE_POWER,
    }

    //아이템 타입
    public READY_ITEM_TYPE itemType = READY_ITEM_TYPE.ADD_TURN;
    
    //원래 아이템 타입
    public READY_ITEM_TYPE originItemType = READY_ITEM_TYPE.ADD_TURN;

    //아이템 구매 재화 타입
    public READYITEM_COSTTYPE costType = READYITEM_COSTTYPE.COIN;

    //아이템 상태
    private READY_ITEM_STATE state = READY_ITEM_STATE.NONE;
    public READY_ITEM_STATE ReadyItemState
    {
        get { return state; }
        set 
        {
            if (state == value)
                return;

            state = value;
            normalRoot.SetActive(ReadyItemState == READY_ITEM_STATE.NONE);
            if (timeLimitRoot != null)
                timeLimitRoot.SetActive(ReadyItemState == READY_ITEM_STATE.TIMELIMIT);
        }
    }
    
    //아이템 세일 타입
    [SerializeField] private READYITEM_SALETYPE saleType = READYITEM_SALETYPE.NONE;

    //아이템 인덱스
    public int index = 0;

    #region 일반 아이템 모드
    [SerializeField] private GameObject normalRoot;
    
    //세일 UI
    [SerializeField] private UISprite spriteSaleIcon;

    //아이템 수 루트
    [SerializeField] private GameObject itemCountRoot;

    //가지고 있는 아이템 수
    [SerializeField] private UILabel[] labelItemCount;

    //가격 루트
    [SerializeField] private GameObject costRoot;

    //아이템 가격
    [SerializeField] private UILabel[] labelCost;
    #endregion

    #region 시간제 아이템 모드
    [SerializeField] private GameObject timeLimitRoot;

    //남은 시간 UI
    [SerializeField] private UILabel limitTimeLabel;
    #endregion

    //아이템 배경 UI
    [SerializeField] private UISprite spriteBG;

    //아이템 선택 UI
    [SerializeField] private UISprite spriteCheck;

    //아이템 선택 콜백
    private System.Action<READY_ITEM_TYPE, READY_ITEM_STATE, int, bool> itemSelectAction = null;

    //아이템 선택 여부
    private bool isSelect = false;

    public bool IsSelectItem
    {
        set { isSelect = value; }
    }

    //아이템 소유 개수
    private int hasCount = 0;

    //아이템 소유 개수
    private int costCount = 0;

    //사과갯수
    [SerializeField] private UILabel[] AppleCountLabel;

    //더블 아이템 선택 UI
    [SerializeField] private UISprite spriteCheck2;

    /// <summary>
    /// 더블 레디 아이템을 사용할 수 있는지 검사 
    /// </summary>
    private bool IsCanUseDoubleItem(READY_ITEM_TYPE doubleType)
    {
        if (Global.GameInstance.CanUseDoubleReadyItem() == false)
            return false;
        
        if (Global.GameInstance.CanUseReadyItem((int)doubleType) == false)
            return false;

        return true;
    }
    
    //아이템 초기화
    public void InitItem(System.Action<READY_ITEM_TYPE, READY_ITEM_STATE, int, bool> selectAction)
    {
        IsSelectItem = false;
        itemSelectAction = selectAction;
        originItemType = itemType;
        index = (int)itemType;
        
        //코스트 타입 초기화
        InitCostType();
     
        //아이템 데이터 및 UI 갱신
        RefreshItem();
    }
    
    /// <summary>
    /// 아이템 타입 초기화
    /// </summary>
    public void SetItemType(READY_ITEM_TYPE changeType)
    {
        itemType = changeType;
        index = (int)itemType;
    }

    /// <summary>
    /// 아이템 선택상태를 해제상태로 변경 후, 아이템 UI를 재갱신시키는 함수
    /// 외부에서 레디 아이템을 수령했을 때, 해당 아이템 카운트나 UI를 갱신시켜주기 위해 사용
    /// </summary>
    public void UnSelectAndRefreshItem()
    {
        if (isSelect == true)
        {
            isSelect = false;
            SetUIItem_UnSelect();
        }

        RefreshItem();
    }

    /// <summary>
    /// 아이템 갱신
    /// </summary>
    public void RefreshItem()
    {
        //세일 타입 초기화
        InitSaleType();
        
        int originIdx = (int)originItemType;
        if (ServerRepos.UserItemFreeTime.CheckReadyItemFree(originIdx) == true 
            && Global.GameInstance.CanUseReadyItem(originIdx) == true)
        {   //시간제 아이템 적용되어 있는지 확인
            InitTimeLimitItem();
            itemSelectAction?.Invoke(itemType, ReadyItemState, index, isSelect);
        }
        else
        {   //시간제 아이템 적용되어 있지 않다면 아이템 타입에 따른 설정
            if (Global._instance.CheckReadyItemType_DoubleItem(itemType) == false)
                InitNormalTypeReadyItem();
            else
                InitDoubleTypeReadyItem();
        }
    }

    private void InitCostType()
    {
        switch (originItemType)
        {
            case READY_ITEM_TYPE.ADD_TURN:
            case READY_ITEM_TYPE.RANDOM_BOMB:
            case READY_ITEM_TYPE.DOUBLE_ADD_TURN:
                costType = READYITEM_COSTTYPE.COIN;
                break;
            default:
                costType = READYITEM_COSTTYPE.JEWEL;
                break;
        }
    }

    private void InitSaleType()
    {
        int sType = (ServerRepos.LoginCdn.ReadyItemSale == 1) ? 1 : 0;

        if (itemType == READY_ITEM_TYPE.DOUBLE_ADD_TURN)
        {
            int doubleIndex = UIPopupReady.SERVER_DOUBLEREADY_INDEX;
            if (ServerRepos.LoginCdn.DoubleReadyItemSaleLevel != null)
                sType = ServerRepos.LoginCdn.DoubleReadyItemSaleLevel[doubleIndex];
        }
        else if (itemType == READY_ITEM_TYPE.DOUBLE_SCORE_UP)
        {
            int doubleIndex = UIPopupReady.SERVER_DOUBLEREADY_INDEX + 1;
            if (ServerRepos.LoginCdn.DoubleReadyItemSaleLevel != null)
                sType = ServerRepos.LoginCdn.DoubleReadyItemSaleLevel[doubleIndex];
        }
        else
        {
            //신버전 할인 데이터 있으면 해당 세일 타입 들고옴.
            if (ServerRepos.LoginCdn.ReadyItemSaleLevel != null)
                sType = ServerRepos.LoginCdn.ReadyItemSaleLevel[index];
        }

        saleType = (READYITEM_SALETYPE)sType;
    }

    private void InitNormalTypeReadyItem()
    {
        if (timeLimitRoot != null)
            timeLimitRoot.gameObject.SetActive(false);        

        //배경 이미지 컬러 및 체크 이미지 디폴트 설정
        spriteBG.spriteName = bgName_itemOff;
        spriteCheck.spriteName = checkName_itemOff;
        
        READY_ITEM_TYPE doubleType = Global._instance.GetDoubleReadyItemType(originItemType);
        if (IsCanUseDoubleItem(doubleType) == true)
        {   //더블 아이템이 활성화 상태에 따라 더블 레디 설정
            spriteCheck2.gameObject.SetActive(true);
            spriteCheck2.spriteName = checkName_itemOff;
        }
        else
        {
            if (spriteCheck2 != null)
                spriteCheck2.gameObject.SetActive(false);
        }
        
        if (itemType == READY_ITEM_TYPE.ADD_TURN)
        {
            for (int i = 0; i < AppleCountLabel.Length; i++)
            {
                AppleCountLabel[i].text = "3";
                AppleCountLabel[i].gameObject.SetActive(true);
            }
        }

        //카운트 및 가격 UI 설정
        hasCount = ServerRepos.UserItem.ReadyItem(index);
        costCount = ServerRepos.LoginCdn.ReadyItems[index];
        if (hasCount > 0)
        {
            costRoot.SetActive(false);
            itemCountRoot.SetActive(true);
            SetHasCount(hasCount);
        }
        else
        {
            costRoot.SetActive(true);
            string itemCostText = costCount.ToString();
            for (int i = 0; i < labelCost.Length; i++)
            {
                labelCost[i].text = itemCostText;
            }

            itemCountRoot.SetActive(false);
        }

        //세일 아이콘 설정
        switch (saleType)
        {
            case READYITEM_SALETYPE.SALE_NORMAL: // 일반 할인
                spriteSaleIcon.gameObject.SetActive(true);
                spriteSaleIcon.spriteName = "icon_sale_02";
                spriteSaleIcon.MakePixelPerfect();
                break;
            case READYITEM_SALETYPE.SALE_POWER: //더블할인
                spriteSaleIcon.gameObject.SetActive(true);
                spriteSaleIcon.transform.localPosition += new Vector3(0f, 3f, 0f);
                spriteSaleIcon.spriteName = "icon_powerSale";
                spriteSaleIcon.MakePixelPerfect();
                break;
            default: //할인 X
                spriteSaleIcon.gameObject.SetActive(false);
                break;
        }
    }

    public void InitTimeLimitItem()
    {
        ReadyItemState = READY_ITEM_STATE.TIMELIMIT;

        spriteBG.spriteName = bgName_itemTime;

        timeLimitRoot.SetActive(true);

        //남은 시간 표시
        StartCoroutine(CoStartTimeLimit());
    }

    #region 시간제 설정
    private IEnumerator CoStartTimeLimit()
    {   
        while (true)
        {
            long limitTime = ServerRepos.UserItemFreeTime.GetItemFreetime(index);
            if (Global.LeftTime(limitTime) > 0)
            {
                limitTimeLabel.text = Global.GetTimeText_MMSS(limitTime);
            }
            else
            {
                limitTimeLabel.text = "00 : 00";
                SetTimeOverItem();
                yield break;
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    //시간제한 끝났을 때 처리
    private void SetTimeOverItem()
    {
        ReadyItemState = READY_ITEM_STATE.NONE;
        InitNormalTypeReadyItem();
    }
    #endregion

    #region 더블 아이템 설정
    private void InitDoubleTypeReadyItem()
    {
        if (timeLimitRoot != null)
            timeLimitRoot.gameObject.SetActive(false);
        
        //배경 이미지 컬러 및 체크 이미지 디폴트 설정
        spriteBG.spriteName = bgName_itemTime;
        spriteCheck.spriteName = checkName_itemOn;
        spriteCheck2.gameObject.SetActive(true);
        spriteCheck2.spriteName = checkName_itemOn;
        
        //더블레디 아이템은 소유가능한 아이템이 아니기 때문에 카운트를 0으로 설정
        hasCount = 0;

        //효과 카운트 UI 설정
        if (itemType == READY_ITEM_TYPE.DOUBLE_ADD_TURN)
        {
            for (int i = 0; i < AppleCountLabel.Length; i++)
            {
                AppleCountLabel[i].text = "6";
                AppleCountLabel[i].gameObject.SetActive(true);
            }
        }

        //가격 설정
        costRoot.SetActive(true);
        
        int doubleIndex = UIPopupReady.SERVER_DOUBLEREADY_INDEX; //더블 사과만 가능
        costCount = ServerRepos.LoginCdn.DoubleReadyItems[doubleIndex];
        string itemCostText = costCount.ToString();
        for (int i = 0; i < labelCost.Length; i++)
        {
            labelCost[i].text = itemCostText;
        }
        itemCountRoot.SetActive(false);

        //세일 아이콘 설정
        switch (saleType)
        {
            case READYITEM_SALETYPE.SALE_NORMAL: // 일반 할인
                spriteSaleIcon.gameObject.SetActive(true);
                spriteSaleIcon.spriteName = "icon_sale_02";
                spriteSaleIcon.MakePixelPerfect();
                break;
            case READYITEM_SALETYPE.SALE_POWER: //더블할인
                spriteSaleIcon.gameObject.SetActive(true);
                spriteSaleIcon.transform.localPosition += new Vector3(0f, 3f, 0f);
                spriteSaleIcon.spriteName = "icon_powerSale";
                spriteSaleIcon.MakePixelPerfect();
                break;
            default: //할인 X
                spriteSaleIcon.gameObject.SetActive(false);
                break;
        }
    }
    #endregion

    /// <summary>
    /// 아이템 선택 상태 변경
    /// </summary>
    public void SetSelectItem(READY_ITEM_TYPE convertType)
    {
        bool isChangeItemType = (itemType != convertType);
        if (isChangeItemType == true)
        {   //아이템 타입이 변화했다면 아이템 타입 및 UI 설정
            SetItemType(convertType);
            InitSaleType();

            //일반/더블 레디에 따라 아이템 UI 변경
            if (Global._instance.CheckReadyItemType_DoubleItem(convertType) == false)
                InitNormalTypeReadyItem();
            else
                InitDoubleTypeReadyItem();
        }

        SetItemUI();
    }

    private void OnClickBtnItem()
    {
        //일반 단계가 아닐때는 선택되지 않음
        if (ReadyItemState != READY_ITEM_STATE.NONE)
            return;

        bool isDoubleItem = Global._instance.CheckReadyItemType_DoubleItem(itemType);
        READY_ITEM_TYPE doubleType = Global._instance.GetDoubleReadyItemType(originItemType);

        if (isDoubleItem == false && IsCanUseDoubleItem(doubleType) && isSelect)
        {   //더블 아이템으로 변경가능하다면, 기본 -> 더블 상태로 변경
            //아이템 선택상태 해제
            SetUIItem_UnSelect();
            itemSelectAction?.Invoke(itemType, ReadyItemState, index, false);

            //더블 레디 상태로 변경 및 UI 갱신
            IsSelectItem = true;
            SetSelectItem(doubleType);
            SetItemUI_Select();
            
            if(isSelect == false)
            {   //재화 부족으로 취소되었다면, 비활성화
                itemSelectAction?.Invoke(itemType, ReadyItemState, index, isSelect);
                SetSelectItem(originItemType);
            }
        }
        else if (isDoubleItem == true && isSelect)
        {   //더블 아이템을 한번 더 선택한다면, 더블 -> 비활성화 상태로 변경
            //아이템 선택상태 해제
            IsSelectItem = false;
            SetUIItem_UnSelect();
            itemSelectAction?.Invoke(itemType, ReadyItemState, index, false);
            
            //일반 레디 상태로 변경 및 UI 갱신
            SetSelectItem(originItemType);
        }
        else
        {
            //선택상태 반전
            IsSelectItem = !isSelect;
            
            //선택 상태에 따라 재화 증가/감소
            if (isSelect == true)
                SetItemUI_Select();
            else
                SetUIItem_UnSelect();
            
            //선택 상태에 따라 UI 설정
            SetItemUI();
        }
    }

    //아이템 선택 상태에 따라 UI 설정
    private void SetItemUI()
    {
        //일반/더블 레디 아이템에 따라 배경 컬러 변경 
        if (Global._instance.CheckReadyItemType_DoubleItem(itemType) == true)
            spriteBG.spriteName = (isSelect == false) ? bgName_itemOff : bgName_itemTime;
        else
            spriteBG.spriteName = (isSelect == false) ? bgName_itemOff : bgName_itemOn;
        
        //체크 UI 이미지 설정
        spriteCheck.spriteName = (isSelect == false) ? checkName_itemOff : checkName_itemOn;

        //아이템 선택 콜백 호출
        itemSelectAction?.Invoke(itemType, ReadyItemState, index, isSelect);
    }

    public void SetItemUI_Select()
    {
        if (hasCount > 0)
        {
            SetHasCount(hasCount - 1);
        }
        else
        {
            switch (costType)
            {
                case READYITEM_COSTTYPE.COIN:
                    if (costCount <= Global.coin)
                    {
                        ManagerUI._instance.SetActionCoin(ManagerUI.CurrencyType.READY_ITEM, -costCount);
                    }
                    else
                    {
                        ManagerUI._instance.LackCoinsPopUp();
                        IsSelectItem = false;
                        return;
                    }
                    break;

                case READYITEM_COSTTYPE.JEWEL:
                    if (costCount <= Global.jewel)
                    {
                        ManagerUI._instance.SetActionJewel(ManagerUI.CurrencyType.READY_ITEM, -costCount);
                    }
                    else
                    {
                        ManagerUI._instance.LackDiamondsPopUp();
                        IsSelectItem = false;
                        return;
                    }
                    break;
            }
            ManagerUI._instance.UpdateUI();
        }
    }

    private void SetUIItem_UnSelect()
    {
        if (hasCount > 0)
        {
            SetHasCount(hasCount);
        }
        else
        {
            switch (costType)
            {
                case READYITEM_COSTTYPE.COIN:
                    ManagerUI._instance.SetActionCoin(ManagerUI.CurrencyType.READY_ITEM, costCount);
                    break;

                case READYITEM_COSTTYPE.JEWEL:
                    ManagerUI._instance.SetActionJewel(ManagerUI.CurrencyType.READY_ITEM, costCount);
                    break;
            }
        }
        ManagerUI._instance.UpdateUI();
    }

    public void SetHasCount(int count)
    {
        string hasCountText = string.Format("+{0}", count);
        for (int i = 0; i < labelItemCount.Length; i++)
        {
            labelItemCount[i].text = hasCountText;
        }
    }
}
