using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupMinorCheck : UIPopupBase
{
    [SerializeField] private List<UISprite> objBgList;
    [SerializeField] private List<GameObject> objCheckList;
    [SerializeField] private List<GameObject> objOutLineList;
    [SerializeField] private List<UILabel> labelTextList;
    [SerializeField] private UILabel labelInfoText;
    [SerializeField] private GameObject objGrayButton;
    [SerializeField] private GameObject objGreenButton;
    [SerializeField] private Color bgColor;

    private int selectIndex = -1;
    private Action onComplete = null;
    private double price = 0;
    private string currency = string.Empty;
    
    public void InitPopup(double price, string currency, Action onComplete) 
    {
        InitCheckBoxList();
        InitButtonTextList();
        this.onComplete = onComplete;
        this.price = price;
        this.currency = currency;
    }

    private void InitButtonTextList()
    {
        List<CdnBilledAmounts> dataList = ServerRepos.LoginCdn.minorProtection.billedAmounts;
        List<string> textList = new List<string>();
        textList.Add(Global._instance.GetString($"minor_age_1").Replace("[0]", dataList[0].ageArray[1].ToString()));
        textList.Add(Global._instance.GetString($"minor_age_2").Replace("[0]", dataList[1].ageArray[0].ToString()).Replace("[1]", dataList[1].ageArray[1].ToString()));
        textList.Add(Global._instance.GetString($"minor_age_3").Replace("[0]", dataList[2].ageArray[0].ToString()));
        
        for (int i = 0; i < labelTextList.Count; i++)
            labelTextList[i].text = textList[i].Replace("[n]", dataList[i].billedAmount.ToString());
        labelInfoText.text = Global._instance.GetString("minor_check_4").Replace("[0]", dataList[2].ageArray[0].ToString());
    }

    private void InitCheckBoxList(int index = -1)
    {
        for (int i = 0; i < 3; i++)
        {
            objBgList[i].color = Color.white;
            objCheckList[i].SetActive(false);
            objOutLineList[i].SetActive(false);
        }
            
        if (index >= 0)
        {
            objBgList[index].color = bgColor;
            objCheckList[index].SetActive(true);
            objOutLineList[index].SetActive(true);
            selectIndex = index;
        }
        
        if (selectIndex < 0 || selectIndex > 2)
        {
            objGreenButton.SetActive(false);
            objGrayButton.SetActive(true);
        }
        else
        {
            objGreenButton.SetActive(true);
            objGrayButton.SetActive(false);
        }
    }

    private void OnClickCheck1()
    {
        if (bCanTouch == false)
            return;
        InitCheckBoxList(0);
    }
    
    private void OnClickCheck2()
    {
        if (bCanTouch == false)
            return;
        InitCheckBoxList(1);
    }
    
    private void OnClickCheck3()
    {
        if (bCanTouch == false)
            return;
        InitCheckBoxList(2);
    }

    private void OnClickConfirm()
    {
        if (bCanTouch == false)
            return;
        
        if (selectIndex < 0 || selectIndex > 2)
            return;

        bCanTouch = false;
        ServerAPI.SetMinorProtection(selectIndex + 1, (resp) =>
        {
            if (resp.IsSuccess)
            {
                if (resp.isEventOn)
                {
                    if (selectIndex == 2)
                        _callbackClose += () => onComplete();
                    else
                        _callbackClose += () => { ManagerUI._instance.OpenPopup<UIPopupMinorApprove>().InitPopup(price, selectIndex + 1, currency, onComplete); };
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
}
