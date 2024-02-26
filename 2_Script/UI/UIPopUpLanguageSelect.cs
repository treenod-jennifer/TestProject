using System.Collections;
using System.Collections.Generic;
using ServiceSDK;
using UnityEngine;

public class UIPopUpLanguageSelect : UIPopupBase
{
    [SerializeField] private UILabel[] labelTitle;
    [SerializeField] private UILabel labelPopupInformation;
    [SerializeField] private UILabel[] labelOK;

    [SerializeField] private GameObject objCloseButton;

    [SerializeField] private GameObject languageBtn;
    [SerializeField] private Transform gridLanguageRoot;
    [SerializeField] private UITable tableLanguage;
    
    public static SystemLanguage selectLanguage = SystemLanguage.Unknown;

    private bool IsSavedLanguage;

    public void InitPopup(bool IsSavedLanguage)
    {
        this.IsSavedLanguage = IsSavedLanguage;

        selectLanguage = LanguageUtility.SystemLanguage;

        SetLabelGlobalKey();

        if (IsSavedLanguage) //저장된 언어가 있을 경우 ex) 옵션창에서 접근
            objCloseButton.SetActive(true);
        else                 //저장된 언어가 없을 경우 ex) 로딩화면에서 접근
            objCloseButton.SetActive(false);

        for (int i = 0; i < LanguageUtility.GetLanguageList().Count; i++)
        {
            UIItemLanguageSelectButton itemRoot = NGUITools.AddChild(gridLanguageRoot, languageBtn).GetComponent<UIItemLanguageSelectButton>();

            itemRoot.UpdataData(LanguageUtility.GetLanguageList()[i]);
        }

        gridLanguageRoot.GetComponent<UIGrid>().Reposition();
        tableLanguage.Reposition();
    }

    private void SetLabelGlobalKey()
    {
        labelTitle.SetText(Global._instance.GetString("p_t_12"));
        labelPopupInformation.text = Global._instance.GetString("l_s_1");
        labelOK.SetText(Global._instance.GetString("btn_1"));
    }

    public void OnClickOKBtn()
    {
        if (LanguageUtility.SystemLanguage != selectLanguage)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.SortOrderSetting();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("l_s_2"), false);
            popupSystem._callbackEnd += () => Global.ReBoot();
        }
        else
        {
            base.OnClickBtnClose();
        }

        LanguageUtility.SystemLanguage = selectLanguage;
        if (ServiceSDKManager.instance != null)
        {
            ServiceSDKManager.instance.SetTridentLanuage();
        }
    }

    public void SetLanguageCheck()
    {
        var uiItemLanguage = gridLanguageRoot.GetComponentsInChildren<UIItemLanguageSelectButton>();

        for (int i = 0; i < uiItemLanguage.Length; i++)
        {
            uiItemLanguage[i].ChangeCheck();
        }
    }

    protected override void OnClickBtnClose()
    {
        if(IsSavedLanguage) // 옵션창에서 접근 할 때 만 허용
            base.OnClickBtnClose();
    }
}
