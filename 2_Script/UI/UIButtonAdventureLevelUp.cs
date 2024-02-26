using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;

public class UIButtonAdventureLevelUp : MonoBehaviour {
    [System.NonSerialized] public bool bCanTouch_Trigger = true;
    public bool bCanTouch
    {
        set
        {
            bCanTouch_Trigger = value;
        }
        get
        {
            return bCanTouch_Trigger;
        }
    }

    [SerializeField] private UIPokoButton button;
    [SerializeField] private UILabel priceCoin;
    [SerializeField] private UILabel priceExpBall;
    [SerializeField] private Color onColor_In;
    [SerializeField] private Color onColor_Out;
    [SerializeField] private Color offColor_In;
    [SerializeField] private Color offColor_Out;
    [SerializeField] private GameObject max_On;
    [SerializeField] private GameObject max_Off;
    [SerializeField] private GameObject btn_Level;
    [SerializeField] private GameObject btn_OK;
    [SerializeField] private UIItemAdventureAnimalInfo animalInfo;

    private ManagerAdventure.AnimalInstance aData;

    private bool activeCoin
    {
        set
        {
            priceCoin.color = value ? onColor_In : offColor_In;
            priceCoin.effectColor = value ? onColor_Out : offColor_Out;
        }

        get
        {
            if (priceCoin.color == onColor_In)
                return true;
            else
                return false;
        }
    }

    private bool activeExpBall
    {
        set
        {
            priceExpBall.color = value ? onColor_In : offColor_In;
            priceExpBall.effectColor = value ? onColor_Out : offColor_Out;
        }

        get
        {
            if (priceExpBall.color == onColor_In)
                return true;
            else
                return false;
        }
    }

    private bool maxLevel
    {
        set
        {
            if(max_On != null && max_Off != null)
            {
                max_On.SetActive(value);
                max_Off.SetActive(!value);
                button.functionName = "";
            }
        }
    }

    public int AnimalIdx
    {
        get { return aData.idx; }
    }

    public void ResetButton()
    {
        StartCoroutine(CoResetButton());
    }

    private IEnumerator CoResetButton()
    {
        yield return new WaitUntil(() => animalInfo.IsInit);

        aData = ManagerAdventure.User.GetAnimalInstance(animalInfo.AnimalIdx);

        int maxLevel = ManagerAdventure.ManagerAnimalInfo.GetMaxLevel(animalInfo.AnimalIdx);

        if (aData.level == maxLevel)
        {
            this.maxLevel = true;
        }
        else if(aData.level == 0)
        {
            btn_Level.SetActive(false);
            btn_OK.SetActive(true);
            button.functionName = "OnClickBtnClose";
        }
        else
        {
            this.maxLevel = false;

            var expInfo = ManagerAdventure.ManagerAnimalInfo.GetExpTable(aData.grade)[aData.level];
            int needValue = expInfo.endExp - aData.totalExp;

            activeCoin = (Global.coin >= needValue);
            activeExpBall = (Global.exp >= needValue);

            if (!activeCoin)
                button.functionName = "OnClickCoinShop";
            else if (activeCoin && activeExpBall)
                button.functionName = "OnClickLevelUp";
            else    //경험치볼만 없는 경우
                button.functionName = "OnLackExpBall";

            priceExpBall.text = needValue.ToString();
            priceCoin.text = needValue.ToString();
        }
    }

    public void OnClickLevelUp()
    {
        if (!bCanTouch)
            return;

        animalInfo.LevelUpWait();

        var expInfo = ManagerAdventure.ManagerAnimalInfo.GetExpTable(aData.grade)[aData.level];
        int needValue = expInfo.endExp - aData.totalExp;
        var useCoin = Global._instance.UseCoin(needValue);

        ManagerAdventure.User.AnimalLevelup(aData.idx, 
            (idx) => 
            {   
                ManagerUI._instance.UpdateUI();

                animalInfo.isLevelUpLoaded = true;
                //animalInfo.ChangeAnimal_Ani(ManagerAdventure.User.GetAnimalInstance(idx));

                //그로씨
                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_LEVELUP_CHARACTER,
                    -useCoin.usePMoney,
                    -useCoin.useFMoney,
                    (int)(ServerRepos.User.coin),
                    (int)(ServerRepos.User.fcoin),
                    null
                    );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
                

                ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                    (int)RewardType.expBall,
                    -needValue,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_LEVELUP_CHARACTER,
                    ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LEVEL_UP_REWARD,
                    null
                    );

            }
        );
    }

    public void OnClickCoinShop()
    {
        if (!bCanTouch)
            return;

        ManagerUI._instance.LackCoinsPopUp();
    }

    public void OnLackExpBall()
    {
        if (!bCanTouch)
            return;

        if (UIPopupSystem._instance != null) return;

        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_adv_1"), false);
        popupSystem.SetResourceImage("Message/adven_exp_icon");
    }
}
