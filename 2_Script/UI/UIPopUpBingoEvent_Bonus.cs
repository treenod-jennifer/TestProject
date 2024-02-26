using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UIPopUpBingoEvent_Bonus : UIPopupBase
{
    public static UIPopUpBingoEvent_Bonus _instance = null;
    
    [SerializeField] private UILabel[] labelsTitle;
    [SerializeField] private UILabel labelText;
    [SerializeField] private GenericReward reward;
    [SerializeField] private UIGrid grid;
    [SerializeField] private GameObject objADButton;

    private int slotIndex;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public void InitData(int slotIndex, bool IsSpecial)
    {
        this.slotIndex = slotIndex;

        int bonusIndex = ManagerBingoEvent.instance.GetSlotState(slotIndex); 
        
        labelsTitle.SetText(Global._instance.GetString($"bge_bonus_{1 + (IsSpecial ? 2 : 0)}"));
        labelText.text = Global._instance.GetString($"bge_bonus_{2 + (IsSpecial ? 2 : 0)}");
        
        reward.SetReward( ManagerBingoEvent.instance.GetBonusReward(bonusIndex));
        
        if(IsSpecial) 
            objADButton.SetActive(true);
        else 
            objADButton.SetActive(false);
        
        grid.Reposition();
    }

    void OnClickBonus()
    {
        if (bCanTouch == false) return;
        bCanTouch = false;
        
        ServerAPI.BingoEventGetBonus(slotIndex, (resp) =>
        {
            if (resp.IsSuccess)
            {
                //그로시
                {
                    int bonusIndex = ManagerBingoEvent.instance.GetSlotState(slotIndex);
                    bool isSpecial = ManagerBingoEvent.instance.IsSpecialBonus(bonusIndex);
                    
                    var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                    (
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.BINGO_REWARD,
                        $"BINGO{(isSpecial ? "_SPECIAL" : "")}_BONUS_REWARD",
                        ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                    );
                    
                    var doc = JsonConvert.SerializeObject(achieve);
                    ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                    
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
                    (
                        rewardType: ManagerBingoEvent.instance.GetBonusReward(100).type,
                        rewardCount: ManagerBingoEvent.instance.GetBonusReward(100).value,
                        moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_BINGO_BONUS_REWARD,
                        itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BINGO_BONUS_REWARD,
                        QuestName: null
                    );
                }
                
                ClosePopUp();

                ManagerBingoEvent.instance.SetCompleteEvent();
                
                if (ManagerBingoEvent.instance.CheckComplete() == false)
                {
                    ManagerBingoEvent.instance.SyncFromServerUserData();   
                }

                ManagerBingoEvent.instance.isSlotOpen = true;
                
                UIPopupBingoEvent_Board._instance.SetBoard();
                BingoTopUIUpdate();
                
                if (ManagerUI._instance != null)
                    ManagerUI._instance.SyncTopUIAssets();
            }
            else
            {
                bCanTouch = true;
            }
        });
    }

    void OnClickADBonus()
    {
        if (bCanTouch == false) return;
        bCanTouch = false;
        
        AdManager.ShowAD_ReqAdBingoEvent(AdManager.AdType.AD_16, slotIndex, (isSuccess) =>
        {
            if (!isSuccess)
            {
                bCanTouch = true;
                return;
            }
            else
            {
                ClosePopUp();
                BingoTopUIUpdate();
            }
        });
    }

    void BingoTopUIUpdate()
    {
        if(ManagerBingoEvent.instance.isBingoClear) UIPopupBingoEvent_Board._instance.SetBonusStageClearUpdate();
        
        UIItemBingoEventBoard.LineClearEvent +=
            () =>
            {
                UIPopupBingoEvent_Board._instance.SetBonusStageClearUpdate();
                UIItemBingoEventBoard.LineClearEvent = null;
            };
    }
}
