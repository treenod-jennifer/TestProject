using System;
using UnityEngine;

public class UIPopupMinorApprove : UIPopupBase
{
    [SerializeField] private GameObject objQuestion;
    [SerializeField] private GameObject objResult;

    private Action onComplete = null;
    private double price = 0;
    private int ageType = 0;
    private string currency = string.Empty;

    public void InitPopup(double price, int ageType, string currency, Action onComplete)
    {
        objQuestion.SetActive(true);
        objResult.SetActive(false);

        this.price = price;
        this.ageType = ageType;
        this.currency = currency;
        this.onComplete = onComplete;
    }

    private void OnClickYes()
    {
        if (bCanTouch == false)
            return;

        bCanTouch = false;
        ServerAPI.GetMinorProtection(currency, (resp) =>
        {
            if (resp.IsSuccess)
            {
                if (resp.isEventOn)
                {
                    if (Global._instance != null && Global._instance.showMinorPriceInfo)
                    {
                        NGUIDebug.Log($"monthlyAmount : {resp.monthlyAmount}");
                        NGUIDebug.Log($"price : {price}");
                        NGUIDebug.Log($"rate : {resp.rate}");
                        NGUIDebug.Log($"billedAmount : {ServerRepos.LoginCdn.minorProtection.billedAmounts[ageType - 1].billedAmount}");
                    }

                    if (resp.monthlyAmount + price * resp.rate <= ServerRepos.LoginCdn.minorProtection.billedAmounts[ageType - 1].billedAmount )
                        _callbackClose += () => { onComplete(); };
                    else
                        _callbackClose += () => { ManagerUI._instance.OpenPopup<UIPopupMinorLimit>().InitPopup(resp.monthlyAmount); };
                }
                else
                    _callbackClose += () =>
                    {
                        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
                        popup.InitSystemPopUp( Global._instance.GetString("p_t_4"), Global._instance.GetString("n_b_25"), false );
                    };
                ClosePopUp();
            }
        });
    }

    private void OnClickNo()
    {
        if (bCanTouch == false)
            return;
        
        objQuestion.SetActive(false);
        objResult.SetActive(true);
    }

    protected override void OnClickBtnClose()
    {
        if (bCanTouch == false)
            return;
        
        base.OnClickBtnClose();
    }
}
