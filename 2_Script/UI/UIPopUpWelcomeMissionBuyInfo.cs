using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpWelcomeMissionBuyInfo : UIPopupBase
{
    [SerializeField] private UILabel[] labelTitle;
    [SerializeField] private UILabel labelGuidance;
    [SerializeField] private UILabel[] labelCashs;
    [SerializeField] private GameObject objDescription;
    
    private System.Action btnAction = null;

    public void InitData(string price, string typeKey, System.Action btnAction)
    {
        labelCashs.SetText(price);
        labelTitle.SetText(Global._instance.GetString($"p_{typeKey}_i_1"));
        labelGuidance.text = Global._instance.GetString($"p_{typeKey}_i_2");
        this.btnAction = btnAction;
        
        //상품판매 법률 개정 표기
        objDescription.SetActive(LanguageUtility.IsShowBuyInfo);
    }

    public void OnClickPurchaseBtn()
    {
        ClosePopUp(0.3f, () => { btnAction(); });
    }
}
