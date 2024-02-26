using System.Collections.Generic;
using UnityEngine;

public class UIPopupMinorLimit : UIPopupBase
{
    [SerializeField] private UILabel labelMonthlyPrice;
    [SerializeField] private UILabel labelInfo;

    public void InitPopup(double price)
    {
        labelMonthlyPrice.text = Global._instance.GetString("minor_limit_2").Replace("[0]", price.ToString());
        List<CdnBilledAmounts> dataList = ServerRepos.LoginCdn.minorProtection.billedAmounts;
        labelInfo.text = Global._instance.GetString($"minor_limit_4")
            .Replace("[0]", dataList[0].ageArray[1].ToString())
            .Replace("[1]", dataList[1].ageArray[0].ToString())
            .Replace("[2]", dataList[1].ageArray[1].ToString())
            .Replace("[3]", dataList[2].ageArray[0].ToString())
            .Replace("[n]", dataList[0].billedAmount.ToString())
            .Replace("[m]", dataList[1].billedAmount.ToString());
    }
}
