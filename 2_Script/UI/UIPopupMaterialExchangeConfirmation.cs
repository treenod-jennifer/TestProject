using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupMaterialExchangeConfirmation : UIPopupBase
{
    [SerializeField] private UIItemExchangeMaterial subGradeMaterial;
    [SerializeField] private UIItemExchangeMaterial upperGradeMaterial;
    [SerializeField] private UILabel subName;
    [SerializeField] private UILabel upperName;
    private int upperChangeCount = 1;
    private int subChangeCount;

    public void InitPopup(ServerUserMaterial normalMaterial, ServerUserMaterial upperGradeMaterial, int exchangeCount)
    {
        this.subGradeMaterial.InitMaterial(normalMaterial);
        this.upperGradeMaterial.InitMaterial(upperGradeMaterial);

        subChangeCount = UIPopupMaterialExchange.instance.GetSubCount(exchangeCount);
        upperChangeCount = exchangeCount;

        this.subGradeMaterial.SetCount(subChangeCount);
        this.upperGradeMaterial.SetCount(upperChangeCount);

        subName.text = Global._instance.GetString($"mt_{normalMaterial.index}");
        upperName.text = Global._instance.GetString($"mt_{upperGradeMaterial.index}");
    }

    public void ExchangeMaterial()
    {
        ServerAPI.ExchangeMaterial
        (
            subGradeMaterial.GetMaterial().index, 
            upperGradeMaterial.GetMaterial().index, 
            upperChangeCount, 
            (complete) => 
            {
                UIPopupMaterialExchange.instance.RePaint();
                ManagerUI._instance.OpenPopup<UIPopupMaterialAcquisition>
                (
                    (popup) => 
                    {
                        var materialData = new ServerUserMaterial() { index = upperGradeMaterial.GetMaterial().index, count = upperChangeCount };

                        popup.Init(materialData);
                        popup._callbackClose = () => ClosePopUp();

                        SendGrowthyLog_Item();
                    }
                );
                UIDiaryDeco._instance.Refresh();
            }
        );
    }

    private void SendGrowthyLog_Item()
    {
        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
        (
            rewardType: upperGradeMaterial.GetMaterial().index + (int)RewardType.material,
            rewardCount: upperChangeCount,
            moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_MATERIAL,
            itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_MATERIAL_RANKUP,
            QuestName: null
        );

        ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
        (
            rewardType: subGradeMaterial.GetMaterial().index + (int)RewardType.material,
            rewardCount: subChangeCount * -1,
            moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.U_BUY_ITEM_MATERIAL,
            itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.U_MATERIAL_RANKUP,
            QuestName: null
        );
    }
}
