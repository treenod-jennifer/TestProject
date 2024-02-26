using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using DG.Tweening;

public enum READY_ITEM_TYPE
{
    ADD_TURN,
    SCORE_UP,//ITEM1
    RANDOM_BOMB,//ITEM2
    LINE_BOMB,
    CIRCLE_BOMB,
    RAINBOW_BOMB,

    DOUBLE_ADD_TURN,
    DOUBLE_SCORE_UP,

    LOCK,
}

public enum READY_ITEM_STATE
{
    NONE,
    BOOSTING,
    TIMELIMIT,
}

public class ReadyItem : MonoBehaviour {

    public READY_ITEM_TYPE type
    {
        get;
        private set;
    } = READY_ITEM_TYPE.LOCK;

    private READY_ITEM_STATE state = READY_ITEM_STATE.NONE;

    private READY_ITEM_STATE ReadyItemState
    {
        get { return state; }
        set
        {
            if (state == value)
                return;

            state = value;
            normalRoot.SetActive(ReadyItemState == READY_ITEM_STATE.NONE);
            if (boostingRoot != null)
                boostingRoot.SetActive(ReadyItemState == READY_ITEM_STATE.BOOSTING);
            if (timeLimitRoot != null)
                timeLimitRoot.SetActive(ReadyItemState == READY_ITEM_STATE.TIMELIMIT);
        }
    }

    //일반 아이템 모드
    public GameObject normalRoot;
    //부스터 아이템 모드
    public GameObject boostingRoot;
    //시간제 아이템 모드
    public GameObject timeLimitRoot;

    public UISprite check1;
    public UISprite check2;
    public UISprite useCheck;
    public UISprite itemBG;
    public UISprite item;

    public UILabel costCountLabel;
    public UILabel costCountLabelShadow;
    public UISprite costTypeSprite;

    public UILabel freeCountLabel;
    public UILabel freeCountLabelShadow;

    public UISprite saleIcon;
    public UILabel AppleCountLabel;

    //시간제 아이템 남은 시간 표시 UI
    public UILabel limitTimeLabel;

    [System.NonSerialized]
    public Transform _transform;

    int index;

    //재화타입 1:코인, 2:다이아
    public int costType;
    //재화개수
    public int costCount;
    //레디아이템 닫힘
    bool isActiveItem = false;
    //무료 아이템 개수
    public int hasCount;

    private float saleIconYPos = 41f;

    //더블아이템인지
    private bool IsDoubleItem
    {
        get
        {
            if (type != READY_ITEM_TYPE.DOUBLE_ADD_TURN
                && type != READY_ITEM_TYPE.DOUBLE_SCORE_UP)
                return false;

            return true;
        }
    }

    //더블아이템으로 변환 될 수 있는지
    private bool IsCanDoubleItem
    {
        get
        {
            if (Global.GameInstance.CanUseReadyItem((int)GetDoubleType(type)) == false)
                return false;

            if (type != READY_ITEM_TYPE.ADD_TURN
                && type != READY_ITEM_TYPE.SCORE_UP)
                return false;

            return true;
        }
    }


    void Awake()
    {
        _transform = transform;
    }

    void Start()
    {
        //아이템종류받아오기
        //오픈여부받아오기
        //무료아이템갯수받아오기
        //재화종류와 숫자 받아오기
        //

    }

    #region 각종 초기화
    public void initItem(READY_ITEM_TYPE tempType, int tempCostCount, int count, int saleType, bool checkPay = true) //재화타입은 정해져 있음
    {
        if (boostingRoot != null)
            boostingRoot.gameObject.SetActive(false);

        if (timeLimitRoot != null)
            timeLimitRoot.gameObject.SetActive(false);

        check2.gameObject.SetActive(false);
        index = (int)tempType;
        costCount = tempCostCount;
        hasCount = count;
        type = tempType;

        if (Global.GameInstance.CanUseReadyItem((int)tempType) == false)
        {
            tempCostCount = 0;
        }

        // 세일, 가격, 무료개수 등 UI 세팅
        if (tempCostCount == 0)
        {
            //닫혀있는 경우
            isActiveItem = false;
            SetUI_Rock();
            
            //현재 아이템이 선택되어있는 설정일 경우, 해제 시켜줌.
            if (UIPopupReady.readyItemSelectCount[index] == 1)
            {
                UIPopupReady.readyItemSelectCount[index] = 0;
                string prefsName = Global.GameInstance.GetReadyItemSelectKey() + index;
                PlayerPrefs.SetInt(prefsName, 0);
            }
        }
        else if(IsCanDoubleItem == true
            && (UIPopupReady.readyItemSelectCount[(int)GetDoubleType(tempType)] == 1))
        {
            //더블 아이템이 활성화 되어 있는 상태
            ConvertDoubleItem(checkPay);
        }
        else
        {
            if (Global.GameInstance.GetItemCostType(ItemType.READY_ITEM) == RewardType.coin)
            {
                costType = 1;
            }
            else
            {
                if (tempType == READY_ITEM_TYPE.ADD_TURN ||
                    tempType == READY_ITEM_TYPE.SCORE_UP ||
                    tempType == READY_ITEM_TYPE.RANDOM_BOMB)
                {
                    costType = 1;
                }
                else if (tempType == READY_ITEM_TYPE.LINE_BOMB ||
                         tempType == READY_ITEM_TYPE.CIRCLE_BOMB ||
                         tempType == READY_ITEM_TYPE.RAINBOW_BOMB)
                {
                    costType = 2;
                }
            }

            isActiveItem = true;
            SetItemSprite();

            SetUI_Open();

            costCountLabel.text = tempCostCount.ToString();
            costCountLabelShadow.text = tempCostCount.ToString();

            if (count > 0)
            {
                bool isSelect = UIPopupReady.readyItemSelectCount[index] == 1;

                ShowFreeCount(hasCount, isSelect);

                SetUI_FreeItem(true);
            }
            else
            {
                SetUI_FreeItem(false);
            }

            switch (saleType)
            {
                case 1: // 일반 할인
                    saleIcon.gameObject.SetActive(true);
                    saleIcon.transform.localPosition
                        = new Vector3(saleIcon.transform.localPosition.x, saleIconYPos, saleIcon.transform.localPosition.z);
                    saleIcon.spriteName = "icon_sale_02";
                    saleIcon.MakePixelPerfect();
                    break;
                case 2: //더블할인
                    saleIcon.gameObject.SetActive(true);
                    saleIcon.transform.localPosition
                        = new Vector3(saleIcon.transform.localPosition.x, saleIconYPos + 3, saleIcon.transform.localPosition.z);
                    saleIcon.spriteName = "icon_powerSale";
                    saleIcon.MakePixelPerfect();
                    break;
                default: //할인 X
                    saleIcon.gameObject.SetActive(false);
                    break;
            }

            //미리 설정된 경우 확인
            SelectItem(checkPay);
        }
    }

    public void InitBoostingItem(READY_ITEM_TYPE tempType)
    {
        ReadyItemState = READY_ITEM_STATE.BOOSTING;
        index = (int)tempType;
        type = tempType;
        {
            isActiveItem = true;
            SetItemSprite();
            itemBG.spriteName = "ready_item_on_03";

            //현재 아이템이 선택되어있는 설정일 경우, 해제 시켜줌.
            if (UIPopupReady.readyItemSelectCount[index] == 1)
            {
                UIPopupReady.readyItemSelectCount[index] = 0;
                string prefsName = Global.GameInstance.GetReadyItemSelectKey() + index;
                PlayerPrefs.SetInt(prefsName, 0);
            }
        }
    }

    public void InitTimeLimitItem(READY_ITEM_TYPE tempType)
    {
        ReadyItemState = READY_ITEM_STATE.TIMELIMIT;
        index = (int)tempType;
        type = tempType;

        isActiveItem = true;
        SetItemSprite();
        itemBG.spriteName = "ready_item_on_03";

        //남은 시간 표시
        StartCoroutine(CoStartTimeLimit());

        //현재 아이템이 선택되어있는 설정일 경우, 해제 시켜줌.
        if (UIPopupReady.readyItemSelectCount[index] == 1)
        {
            UIPopupReady.readyItemSelectCount[index] = 0;
            string prefsName = Global.GameInstance.GetReadyItemSelectKey() + index;
            PlayerPrefs.SetInt(prefsName, 0);
        }
    }

    public void InitDoubleItem(READY_ITEM_TYPE doubleType, int tempCostCount, int saleType, bool checkPay)
    {
        //기본 값 설정
        index = (int)doubleType;
        type = doubleType;
        costCount = tempCostCount;
        hasCount = 0;
        costType = 1;
        isActiveItem = true;
        SetItemSprite();

        //아이템 UI
        SetUI_Open();
        itemBG.spriteName = "ready_item_on_03";
        check2.gameObject.SetActive(true);
        check2.spriteName = "ready_button_01_on";

        //가격 UI
        SetUI_FreeItem(false);
        costCountLabel.text = tempCostCount.ToString();
        costCountLabelShadow.text = tempCostCount.ToString();

        switch (saleType)
        {
            case 1: // 일반 할인
                saleIcon.gameObject.SetActive(true);
                saleIcon.transform.localPosition
                    = new Vector3(saleIcon.transform.localPosition.x, saleIconYPos, saleIcon.transform.localPosition.z);
                saleIcon.spriteName = "icon_sale_02";
                saleIcon.MakePixelPerfect();
                break;
            case 2: //더블할인
                saleIcon.gameObject.SetActive(true);
                saleIcon.transform.localPosition
                    = new Vector3(saleIcon.transform.localPosition.x, saleIconYPos + 3, saleIcon.transform.localPosition.z);
                saleIcon.spriteName = "icon_powerSale";
                saleIcon.MakePixelPerfect();
                break;
            default: //할인 X
                saleIcon.gameObject.SetActive(false);
                break;
        }

        //재화 계산 후 활성화/비활성화 확인
        SelectItem(checkPay);
    }

    #endregion

    #region UI 세팅 모음
    public void SetUI_Rock()
    {
        item.spriteName = "ready_item_rock";
        item.MakePixelPerfect();
        itemBG.spriteName = "ready_item_off";
        itemBG.MakePixelPerfect();

        //아이템클로즈
        freeCountLabel.gameObject.SetActive(false);
        freeCountLabelShadow.gameObject.SetActive(false);

        check1.gameObject.SetActive(false);
        check2.gameObject.SetActive(false);
        useCheck.gameObject.SetActive(false);
        costCountLabel.gameObject.SetActive(false);
        costCountLabelShadow.gameObject.SetActive(false);
        costTypeSprite.gameObject.SetActive(false);
        saleIcon.gameObject.SetActive(false);

        if (AppleCountLabel != null)
            AppleCountLabel.gameObject.SetActive(false);
    }

    public void SetUI_Open()
    {
        check2.gameObject.SetActive(false);
        costCountLabel.gameObject.SetActive(true);
        costCountLabelShadow.gameObject.SetActive(true);
        costTypeSprite.gameObject.SetActive(true);

        //가격 타입에 따라 UI 변경
        if (costType == 1)
        {
            check1.gameObject.SetActive(true);
            useCheck.gameObject.SetActive(false);
            costTypeSprite.spriteName = "icon_coin_shadow";
        }
        else if (costType == 2)
        {
            check1.gameObject.SetActive(false);
            check2.gameObject.SetActive(false);
            useCheck.gameObject.SetActive(true);
            costTypeSprite.spriteName = "icon_diamond_shadow";
        }
        if(IsCanDoubleItem == true)
        {
            //더블 체크 표시
            check2.gameObject.SetActive(true);
            check2.spriteName = "ready_button_01_off";
        }

        itemBG.spriteName = "ready_item_on_02";
        useCheck.spriteName = "ready_button_01_off";
        check1.spriteName = "ready_button_01_off";
        check2.spriteName = "ready_button_01_off";

        costTypeSprite.MakePixelPerfect();
    }

    public void SetUI_FreeItem(bool free)
    {
        freeCountLabel.gameObject.SetActive(free);
        freeCountLabelShadow.gameObject.SetActive(free);

        costCountLabel.gameObject.SetActive(!free);
        costCountLabelShadow.gameObject.SetActive(!free);
        costTypeSprite.gameObject.SetActive(!free);
    }
    #endregion

    //아이템 활성화, 비활성화 설정
    public void SelectItem(bool checkPay = true)
    {
        if (UIPopupReady.readyItemSelectCount[index] == 1)
        {
            if (hasCount > 0)
            {
                if (checkPay) ShowFreeCount(hasCount, true);

                UIPopupReady.readyItemSelectCount[index] = 1;
                onItem();
            }
            else
            {
                //재화 비교 결제창
                if (costType == 1)
                {
                    if (costCount > Global.coin)
                    {
                        UIPopupReady.readyItemSelectCount[index] = 0;
                        offItem();
                    }
                    else
                    {
                        if (checkPay)
                            ManagerUI._instance.SetActionCoin(ManagerUI.CurrencyType.READY_ITEM, -costCount);
                        ManagerUI._instance.UpdateUI();
                        onItem();
                    }
                }
                else if (costType == 2)
                {
                    if (costCount > Global.jewel)
                    {
                        UIPopupReady.readyItemSelectCount[index] = 0;
                        offItem();
                    }
                    else
                    {
                        if (checkPay)
                            ManagerUI._instance.SetActionJewel(ManagerUI.CurrencyType.READY_ITEM, -costCount);
                        ManagerUI._instance.UpdateUI();
                        onItem();
                    }
                }
            }
        }
    }

    //더블 아이템 변환
    public void ConvertDoubleItem(bool checkPay = true)
    {
        if (Global.GameInstance.CanUseReadyItem((int)GetDoubleType(type)) == false)
            return;

        READY_ITEM_TYPE swapItemType = GetDoubleType(type);
        int swapItemIndex = (int)swapItemType;

        //변환
        switch (swapItemType)
        {
            case READY_ITEM_TYPE.ADD_TURN:
            case READY_ITEM_TYPE.SCORE_UP:
                initItem(swapItemType, ServerRepos.LoginCdn.ReadyItems[swapItemIndex], ServerRepos.UserItem.ReadyItem(swapItemIndex), UIPopupReady._instance.GetReadyItemSaleType(swapItemType), checkPay);
                break;
            case READY_ITEM_TYPE.DOUBLE_ADD_TURN:
                InitDoubleItem(swapItemType, ServerRepos.LoginCdn.DoubleReadyItems[UIPopupReady.SERVER_DOUBLEREADY_INDEX], UIPopupReady._instance.GetReadyItemSaleType(swapItemType), checkPay);
                break;
            case READY_ITEM_TYPE.DOUBLE_SCORE_UP:
                InitDoubleItem(swapItemType, ServerRepos.LoginCdn.DoubleReadyItems[UIPopupReady.SERVER_DOUBLEREADY_INDEX + 1], UIPopupReady._instance.GetReadyItemSaleType(swapItemType), checkPay);
                break;
            default:
                break;
        }
    }

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
        UIPopupReady._instance.InitTimeLimitTypeReadyItem(index, type, false);
    }

    private void SetItemSprite()
    {
        switch (type)
        {
            case READY_ITEM_TYPE.ADD_TURN:
                if (AppleCountLabel != null)
                {
                    AppleCountLabel.text = "3";
                    AppleCountLabel.gameObject.SetActive(true);
                }
                item.spriteName = "item_apple";
                break;
            case READY_ITEM_TYPE.SCORE_UP:
                item.spriteName = "item_scoreUp";
                break;
            case READY_ITEM_TYPE.RANDOM_BOMB:
                item.spriteName = "item_random_bomb";
                break;
            case READY_ITEM_TYPE.LINE_BOMB:
                item.spriteName = "item_line_bomb";
                break;
            case READY_ITEM_TYPE.CIRCLE_BOMB:
                item.spriteName = "item_bomb";
                break;
            case READY_ITEM_TYPE.RAINBOW_BOMB:
                item.spriteName = "item_rainbow";
                break;
            case READY_ITEM_TYPE.DOUBLE_ADD_TURN:
                if (AppleCountLabel != null)
                {
                    AppleCountLabel.text = "6";
                    AppleCountLabel.gameObject.SetActive(true);
                }
                item.spriteName = "item_apple";
                break;
            case READY_ITEM_TYPE.DOUBLE_SCORE_UP:
                item.spriteName = "item_scoreUp20";
                break;
            default:
                item.spriteName = "ready_item_rock";
                break;
        }
        item.MakePixelPerfect();
    }

    public void offItem()
    {
        if (!isActiveItem) return;

        if (costType == 1)
        {
            check2.spriteName = "ready_button_01_off";
            check1.spriteName = "ready_button_01_off";
        }
        else if (costType == 2)
        {
            useCheck.spriteName = "ready_button_01_off";
        }
        itemBG.spriteName = "ready_item_on_02";

        string prefsName = Global.GameInstance.GetReadyItemSelectKey() + index;
        PlayerPrefs.SetInt(prefsName, 0);
    }

    public void onItem()
    {
        if (costType == 1)
        {
            if (IsDoubleItem)
                check2.spriteName = "ready_button_01_on";
            check1.spriteName = "ready_button_01_on";
        }
        else if (costType == 2)
        {
            useCheck.spriteName = "ready_button_01_on";
        }

        if (IsDoubleItem)
            itemBG.spriteName = "ready_item_on_03";
        else
            itemBG.spriteName = "ready_item_on_01";

        string prefsName = Global.GameInstance.GetReadyItemSelectKey() + index;
        PlayerPrefs.SetInt(prefsName, 1);

        if(IsDoubleItem == true)
        {
            // 더블 체크 값 교정
            string prefsName_Double = Global.GameInstance.GetReadyItemSelectKey() + (int)GetDoubleType(type);
            PlayerPrefs.SetInt(prefsName_Double, 0);
        }
    }

    public void ShowFreeCount(int count, bool isSelect)
    {
        int test = isSelect ? 1 : 0;

        freeCountLabel.text = "+" + (count - test).ToString();
        freeCountLabelShadow.text = "+" + (count - test).ToString();
    }

    void UseItem()
    {
        if (!isActiveItem) return;
        if (UIPopupReady._instance.bStartGame) return;

        if (ReadyItemState != READY_ITEM_STATE.NONE)
            return;

        //더블아이템류 상태 변경
        if(IsDoubleItem)
        {
            //더블 -> 비활성화
            CostUI(false);
            offItem();

            ConvertDoubleItem(false);
        }
        else if(IsCanDoubleItem &&
             (UIPopupReady.readyItemSelectCount[index] == 1))
        {
            //기본 -> 더블

            //기본 레디 제거
            CostUI(false);
            offItem();

            //변환
            ConvertDoubleItem(false);
            if (CostCheck())
            {
                //더블 레디 활성화
                CostUI(true);
                onItem();
                ReadyItemBoniAni();
            }
            else
            {
                //코인 부족 : 비활성화 변환
                ConvertDoubleItem(false);
            }
        }
        else
        {
            if (UIPopupReady.readyItemSelectCount[index] == 0)
            {
                //비활성화 -> 활성화
                if(CostCheck())
                {
                    CostUI(true);
                    onItem();
                    ReadyItemBoniAni();
                }
            }
            else
            {
                //활성화 -> 비활성화
                CostUI(false);
                offItem();
            }
        }
    }

    private void CostUI(bool isSelect)
    {
        int dialogIndex = index; 
        if (IsCanDoubleItem)
        {
            dialogIndex += 8;
        }
        
        UIPopupReady._instance.MakeBoniDialog(isSelect == true ? dialogIndex : -1);
        UIPopupReady.readyItemSelectCount[index] = isSelect == true ? 1 : 0;

        if (hasCount > 0)
        {
            ShowFreeCount(hasCount, isSelect);
        }
        else
        {
            if (costType == 1)
            {
                ManagerUI._instance.SetActionCoin(ManagerUI.CurrencyType.READY_ITEM, isSelect == true ? -costCount : costCount);
            }
            if (costType == 2)
            {
                ManagerUI._instance.SetActionJewel(ManagerUI.CurrencyType.READY_ITEM, isSelect == true ? -costCount : costCount);
            }

            ManagerUI._instance.UpdateUI();
        }
    }

    private bool CostCheck()
    {
        if (hasCount > 0)
        {
            return true;
        }

        if (costType == 1)
        {
            if (costCount > Global.coin)
            {
                ManagerUI._instance.LackCoinsPopUp();
                return false;
            }
        }
        else if (costType == 2)
        {
            if (costCount > Global.jewel)
            {
                ManagerUI._instance.LackDiamondsPopUp();
                return false;
            }
        }

        return true;
    }

    private void ReadyItemBoniAni()
    {
        string itemSelectAni = UIPopupReady._instance.GetItemAnimationName();
        string readyLoopAni = UIPopupReady._instance.GetReadyLoopAnimationName();
        if (Global.GameInstance.GetProp(GameTypeProp.USE_READY_CHARACTER))
        {
            AudioLobby voice = UIPopupReady._instance.GetReadyCharacterVoice();
            if (voice != AudioLobby.NO_SOUND)
            {
                ManagerSound.AudioPlay(voice);
            }
        }
        else
        {
            ManagerSound.AudioPlay(AudioLobby.m_boni_tubi);
        }

        if (UIPopupReady._instance.boniLive2D.IsPlayingAnimation(itemSelectAni) == false)
            UIPopupReady._instance.boniLive2D.SetAnimation(itemSelectAni, readyLoopAni);
    }

    public bool IsSelectItem()
    {
        return UIPopupReady.readyItemSelectCount[index] == 1;
    }

    //더블 레디 아이템 타입 <-> 기본 레디 아이템 타입
    public READY_ITEM_TYPE GetDoubleType(READY_ITEM_TYPE tempType)
    {
        switch(tempType)
        {
            case READY_ITEM_TYPE.ADD_TURN:
                return READY_ITEM_TYPE.DOUBLE_ADD_TURN;
            case READY_ITEM_TYPE.DOUBLE_ADD_TURN:
                return READY_ITEM_TYPE.ADD_TURN;
            case READY_ITEM_TYPE.SCORE_UP:
                return READY_ITEM_TYPE.DOUBLE_SCORE_UP;
            case READY_ITEM_TYPE.DOUBLE_SCORE_UP:
                return READY_ITEM_TYPE.SCORE_UP;
        }

        return READY_ITEM_TYPE.LOCK;
    }

    public virtual IEnumerator CoFlashItem_Alpha(int actionCount, float actionTIme)
    {
        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        DOTween.ToAlpha(() => item.color, x => item.color = x, 0, aTime).SetLoops(count, LoopType.Yoyo);
        yield return new WaitForSeconds(actionTIme);

        item.color = new Color(item.color.r, item.color.g, item.color.b, 1f);
    }
}
