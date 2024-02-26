using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Protocol;

public enum READY_ITEM_TYPE
{
    ADD_TURN,
    SCORE_UP,//ITEM1,
    ITEM2,
    LINE_BOMB,
    CIRCLE_BOMB,
    RAINBOW_BOMB,

    LOCK,
}


public class ReadyItem : MonoBehaviour {

    public READY_ITEM_TYPE type = READY_ITEM_TYPE.LOCK;

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

    public GameObject saleIcon;
    public GameObject AppleCountLabel;

    [System.NonSerialized]
    public Transform _transform;

    int index;
    public int costType;
    public int costCount;
    bool isActiveItem=false;

    public int hasCount;

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

    public void initItem(READY_ITEM_TYPE tempType, int tempCostCount, int count, bool bSale) //재화타입은 정해져 있음
    {
        check2.gameObject.SetActive(false);
        index = (int)tempType;
        costCount = tempCostCount;
        hasCount = count;
        type = tempType;


        if (tempType == READY_ITEM_TYPE.ADD_TURN)
        {
            if(Global.eventIndex > 0)
            {

            }
            else if(Global.stageIndex < UIPopupReady.OPEN_ADD_TURN_STAGE)
            {
                tempCostCount = 0;
            }
        }
        
        else if (tempType == READY_ITEM_TYPE.SCORE_UP)
        {
            //tempCostCount = 0;            
            if (Global.eventIndex > 0)
            {
                tempCostCount = 0;
            }
            else if (Global.stageIndex < UIPopupReady.OPEN_SCORE_UP_STAGE)
            {
                tempCostCount = 0;
            }            
            //type = READY_ITEM_TYPE.LOCK;
        }
        else if (tempType == READY_ITEM_TYPE.ITEM2)
        {
            tempCostCount = 0;
            /*
            if (Global.eventIndex > 0)
            {

            }
            else if (Global.stageIndex < UIPopupReady.OPEN_ITEM2_STAGE)
            {
                tempCostCount = 0;
            }
            */
            type = READY_ITEM_TYPE.LOCK;
        }
        
        else if (tempType == READY_ITEM_TYPE.LINE_BOMB)
        {
            if (Global.eventIndex > 0)
            {

            }
            else if (Global.stageIndex < UIPopupReady.OPEN_LINE_BOMB_STAGE)
            {
                tempCostCount = 0;
            }
        }
        else if (tempType == READY_ITEM_TYPE.CIRCLE_BOMB)
        {
            if (Global.eventIndex > 0)
            {

            }
            else if (Global.stageIndex < UIPopupReady.OPEN_CIRCLE_BOMB_STAGE)
            {
                tempCostCount = 0;
            }
        }
        else if (tempType == READY_ITEM_TYPE.RAINBOW_BOMB)
        {
            if (Global.eventIndex > 0)
            {

            }
            else if (Global.stageIndex < UIPopupReady.OPEN_RAINBOW_BOMB_STAGE)
            {
                tempCostCount = 0;
            }
        }



        if (tempCostCount == 0)
        {
            isActiveItem = false;

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
            AppleCountLabel.SetActive(false);
        }
        else
        {
            if(tempType == READY_ITEM_TYPE.ADD_TURN ||
                tempType == READY_ITEM_TYPE.SCORE_UP ||
                tempType == READY_ITEM_TYPE.ITEM2)
            {
                costType = 1;
            }
            else if (tempType == READY_ITEM_TYPE.LINE_BOMB ||
                    tempType == READY_ITEM_TYPE.CIRCLE_BOMB ||
                    tempType == READY_ITEM_TYPE.RAINBOW_BOMB)
            {
                costType = 2;
            }

            isActiveItem = true;

            if (tempType == READY_ITEM_TYPE.ADD_TURN)
            {
                if (AppleCountLabel != null)AppleCountLabel.SetActive(true);
                item.spriteName = "item_apple";
            }
            else if (tempType == READY_ITEM_TYPE.SCORE_UP)
                item.spriteName = "item_scoreUp";
            else if (tempType == READY_ITEM_TYPE.LINE_BOMB)
                item.spriteName = "item_line_bomb";
            else if (tempType == READY_ITEM_TYPE.CIRCLE_BOMB)
                item.spriteName = "item_bomb";
            else if (tempType == READY_ITEM_TYPE.RAINBOW_BOMB)
                item.spriteName = "item_rainbow";
            else
                item.spriteName = "ready_item_rock";

            item.MakePixelPerfect();

            check2.gameObject.SetActive(false);
            costCountLabel.gameObject.SetActive(true);
            costCountLabelShadow.gameObject.SetActive(true);
            costTypeSprite.gameObject.SetActive(true);

            if (costType == 1)
            {
                check1.gameObject.SetActive(true);
                useCheck.gameObject.SetActive(false);
                costTypeSprite.spriteName = "icon_coin_shadow";
            }
            else if (costType == 2)
            {
                check1.gameObject.SetActive(false);
                useCheck.gameObject.SetActive(true);
                costTypeSprite.spriteName = "icon_diamond_shadow";
            }

            itemBG.spriteName = "ready_item_on_02";
            useCheck.spriteName = "ready_button_02_off";
            check1.spriteName = "ready_button_01_off";

            costTypeSprite.MakePixelPerfect();

            
            costCountLabel.text = tempCostCount.ToString();
            costCountLabelShadow.text = tempCostCount.ToString();

            if(count > 0)
            {
                freeCountLabel.gameObject.SetActive(true);
                freeCountLabelShadow.gameObject.SetActive(true);

                freeCountLabel.text = "+" + hasCount.ToString();
                freeCountLabelShadow.text = "+" + hasCount.ToString();

                /*
                costCountLabel.text = hasCount.ToString();
                costCountLabel.fontSize = 40;
                costCountLabelShadow.text = hasCount.ToString();
                costCountLabelShadow.fontSize = 40;
                costCountLabel.cachedTransform.localPosition = new Vector3(40.8f, -34.9f, 0);
                costCountLabelShadow.cachedTransform.localPosition = new Vector3(3.1f, -3.1f, 0);
                */
                costCountLabel.gameObject.SetActive(false);
                costCountLabelShadow.gameObject.SetActive(false);
                costTypeSprite.gameObject.SetActive(false);
            }
            else
            {
                freeCountLabel.gameObject.SetActive(false);
                freeCountLabelShadow.gameObject.SetActive(false);

                costCountLabel.gameObject.SetActive(true);
                costCountLabelShadow.gameObject.SetActive(true);
                costTypeSprite.gameObject.SetActive(true);
            }
            
            //세일 여부에 따라 세일 아이콘 세팅.
            if (bSale == true)
            {
                saleIcon.gameObject.SetActive(true);
            }
            else
            {
                saleIcon.gameObject.SetActive(false);
            }

            if (UIPopupReady.readyItemSelectCount[index] == 1)
            {
                if (hasCount > 0)
                {
                    ShowFreeCount(hasCount - 1);

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
                            Global.coin -= costCount;
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
                            Global.jewel -= costCount;
                            ManagerUI._instance.UpdateUI();
                            onItem();
                        }
                    }
                }
            }
            //오픈여부 선택여부
        }
    }


    public void offItem()
    {
        if (!isActiveItem) return;

        if (costType == 1)
        {
            check1.spriteName = "ready_button_01_off";
        }
        else if (costType == 2)
        {
            useCheck.spriteName = "ready_button_02_off";
        }
        itemBG.spriteName = "ready_item_on_02";

        string prefsName = "readyItemSelect" + index;
        PlayerPrefs.SetInt(prefsName, 0);
    }

    public void onItem()
    {
        if (costType == 1)
        {
            check1.spriteName = "ready_button_01_on";
        }
        else if (costType == 2)
        {
            useCheck.spriteName = "ready_button_02_on";
        }
        itemBG.spriteName = "ready_item_on_01";

        string prefsName = "readyItemSelect" + index;
        PlayerPrefs.SetInt(prefsName, 1);
    }

    public void ShowFreeCount(int count)
    {
        freeCountLabel.text = "+" + count.ToString();
        freeCountLabelShadow.text = "+" + count.ToString();
    }


    void UseItem()
    {
        if (!isActiveItem) return;
        if (UIPopupReady._instance.bStartGame) return;
        
        if (UIPopupReady.readyItemSelectCount[index] == 0)
        {
            if (index == 3)
            {
                if (UIPopupReady.readyItemSelectCount[4] == 1)
                {
                    if (UIPopupReady._instance.listReadyItem[4].hasCount > 0)
                    {
                        UIPopupReady._instance.listReadyItem[4].ShowFreeCount(UIPopupReady._instance.listReadyItem[4].hasCount);
                    }
                    else
                    {
                        Global.jewel += UIPopupReady._instance.listReadyItem[4].costCount;
                        ManagerUI._instance.UpdateUI();
                    }
                    UIPopupReady.readyItemSelectCount[4] = 0;
                    UIPopupReady._instance.listReadyItem[4].offItem();
                }

                if (UIPopupReady.readyItemSelectCount[5] == 1)
                {
                    if (UIPopupReady._instance.listReadyItem[5].hasCount > 0)
                    {
                        UIPopupReady._instance.listReadyItem[5].ShowFreeCount(UIPopupReady._instance.listReadyItem[5].hasCount);
                    }
                    else
                    {
                        Global.jewel += UIPopupReady._instance.listReadyItem[5].costCount;
                        ManagerUI._instance.UpdateUI();
                    }
                    UIPopupReady.readyItemSelectCount[5] = 0;
                    UIPopupReady._instance.listReadyItem[5].offItem();
                }
            }
            else if (index == 4)
            {
                if (UIPopupReady.readyItemSelectCount[3] == 1)
                {
                    if (UIPopupReady._instance.listReadyItem[3].hasCount > 0)
                    {
                        UIPopupReady._instance.listReadyItem[3].ShowFreeCount(UIPopupReady._instance.listReadyItem[3].hasCount);
                    }
                    else
                    {
                        Global.jewel += UIPopupReady._instance.listReadyItem[3].costCount;
                        ManagerUI._instance.UpdateUI();
                    }
                    UIPopupReady.readyItemSelectCount[3] = 0;
                    UIPopupReady._instance.listReadyItem[3].offItem();
                }

                if (UIPopupReady.readyItemSelectCount[5] == 1)
                {
                    if (UIPopupReady._instance.listReadyItem[5].hasCount > 0)
                    {
                        UIPopupReady._instance.listReadyItem[5].ShowFreeCount(UIPopupReady._instance.listReadyItem[5].hasCount);
                    }
                    else
                    {
                        Global.jewel += UIPopupReady._instance.listReadyItem[5].costCount;
                        ManagerUI._instance.UpdateUI();
                    }
                    UIPopupReady.readyItemSelectCount[5] = 0;
                    UIPopupReady._instance.listReadyItem[5].offItem();
                }
            }
            else if (index == 5)
            {
                if (UIPopupReady.readyItemSelectCount[3] == 1)
                {
                    if (UIPopupReady._instance.listReadyItem[3].hasCount > 0)
                    {
                        UIPopupReady._instance.listReadyItem[3].ShowFreeCount(UIPopupReady._instance.listReadyItem[3].hasCount);
                    }
                    else
                    {
                        Global.jewel += UIPopupReady._instance.listReadyItem[3].costCount;
                        ManagerUI._instance.UpdateUI();
                    }
                    UIPopupReady.readyItemSelectCount[3] = 0;
                    UIPopupReady._instance.listReadyItem[3].offItem();
                }

                if (UIPopupReady.readyItemSelectCount[4] == 1)
                {
                    if (UIPopupReady._instance.listReadyItem[4].hasCount > 0)
                    {
                        UIPopupReady._instance.listReadyItem[4].ShowFreeCount(UIPopupReady._instance.listReadyItem[4].hasCount);
                    }
                    else
                    {
                        Global.jewel += UIPopupReady._instance.listReadyItem[4].costCount;
                        ManagerUI._instance.UpdateUI();
                    }
                    UIPopupReady.readyItemSelectCount[4] = 0;
                    UIPopupReady._instance.listReadyItem[4].offItem();
                }
            }

            if (hasCount > 0)
            {
                ShowFreeCount(hasCount - 1);
            }
            else
            {
                //재화 비교 결제창
                if (costType == 1)
                {
                    if (costCount > Global.coin)
                    {
                        ManagerUI._instance.LackCoinsPopUp();
                        return;
                    }
                    Global.coin -= costCount;

                }
                else if (costType == 2)
                {


                    if (costCount > Global.jewel)
                    {
                        ManagerUI._instance.LackDiamondsPopUp();
                        return;
                    }
                    Global.jewel -= costCount;
                }
                ManagerUI._instance.UpdateUI();
            }

            UIPopupReady._instance.MakeBoniDialog(index);
            UIPopupReady.readyItemSelectCount[index] = 1;

            onItem();

            if (UIPopupReady._instance.boniLive2D._animator.GetCurrentAnimatorStateInfo(0).IsName("InGame_Item_Select") == false ||
                 UIPopupReady._instance.boniLive2D._animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.567f)
            {
                if (Global.eventIndex > 0)
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
            }
            UIPopupReady._instance.boniLive2D.setAnimation(false, "InGame_Item_Select");
        }
        else
        {
            UIPopupReady._instance.MakeBoniDialog(-1);
            UIPopupReady.readyItemSelectCount[index] = 0;


            if (hasCount > 0)
            {
                freeCountLabel.text = "+" + hasCount.ToString();
                freeCountLabelShadow.text = "+" + hasCount.ToString();
            }
            else
            {
                if (costType == 1)
                {
                    Global.coin += costCount;
                }
                if (costType == 2)
                {
                    Global.jewel += costCount;
                }

                ManagerUI._instance.UpdateUI();
            }

            offItem();
        }
    }

}
