using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLanguageSelectButton : MonoBehaviour
{
    [SerializeField] private GameObject objCheck;
    [SerializeField] private UISprite sprBG;
    [SerializeField] private UILabel labelLanguage;

    [SerializeField] private Color[] colorBGSelect;
    [SerializeField] private Color[] colorLabelSelect;

    private SystemLanguage mySystemLanguage;

    public void UpdataData(SystemLanguage language)
    {
        mySystemLanguage = language;
        labelLanguage.text = Global._instance.GetString($"btn_lc_{LanguageUtility.LanguageToCountryCode(mySystemLanguage)}");

        ChangeCheck();
    }

    private void OnClickLanguageSelect()
    {
        UIPopUpLanguageSelect.selectLanguage = mySystemLanguage;

        GetComponentInParent<UIPopUpLanguageSelect>().SetLanguageCheck();
    }

    public void ChangeCheck()
    {
        if(UIPopUpLanguageSelect.selectLanguage == mySystemLanguage)
        {
            objCheck.SetActive(true);
            sprBG.color = colorBGSelect[0];
            labelLanguage.color = colorLabelSelect[0];
        }
        else
        {
            objCheck.SetActive(false);
            sprBG.color = colorBGSelect[1];
            labelLanguage.color = colorLabelSelect[1];
        }
    }
}