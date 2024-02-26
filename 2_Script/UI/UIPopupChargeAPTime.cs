using System;
using UnityEngine;

public class UIPopupChargeAPTime : UIPopupBase
{
    public static UIPopupChargeAPTime _instance = null;

    //공통 UI
    [SerializeField] private UILabel label_Title;
    [SerializeField] private UILabel label_Title_S;
    [SerializeField] private UILabel label_MainText;
    [SerializeField] private UILabel label_Description;
    [SerializeField] private UILabel label_BuyInfo;
    
    //첫 번째 버튼
    [SerializeField] private GameObject sprite_Button1;
    [SerializeField] private UISprite sprite_Currency1;
    [SerializeField] private UILabel label_Price1;
    [SerializeField] private UILabel label_Price1_S;
    
    //두 번째 버튼
    [SerializeField] private GameObject sprite_Button2;
    [SerializeField] private UISprite sprite_Currency2;
    [SerializeField] private UILabel label_Price2;
    [SerializeField] private UILabel label_Price2_S;

    private Action okAction1 = null;
    private Action okAction2 = null;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    // 팝업 초기화 함수
    public void InitPopUp(string str_title, string str_main, string str_buyinfo = null)
    {
        label_Title.text = str_title;
        label_Title_S.text = str_title;
        label_MainText.text = str_main;

        if (string.IsNullOrEmpty(str_buyinfo))
            label_BuyInfo.gameObject.SetActive(false);
        else
            label_BuyInfo.text = str_buyinfo;
    }
    
    // 버튼 세팅 함수
    public void InitButton(string spriteName1, string price1, Action action1, string spriteName2 = null, string price2 = null, Action action2 = null)
    {
        sprite_Currency1.spriteName = spriteName1;
        label_Price1.text = price1;
        label_Price1_S.text = price1;
        if (string.IsNullOrEmpty(spriteName2) || string.IsNullOrEmpty(price2))
        {
            sprite_Button1.transform.position = new Vector2(0, sprite_Button1.transform.position.y);
            sprite_Button2.SetActive(false);
        }
        else
        {
            sprite_Currency2.spriteName = spriteName2;
            label_Price2.text = price2;
        }
        okAction1 = action1;
        okAction2 = action2;
    }

    private void OnClickButtonAction1()
    {
        _callbackClose += () => okAction1();
        ManagerUI._instance.ClosePopUpUI();
    }

    private void OnClickButtonAction2()
    {
        _callbackClose += () => okAction2();
        ManagerUI._instance.ClosePopUpUI();
    }
    
    protected override void OnClickBtnClose()
    {
        ManagerUI._instance.ClosePopUpUI();
    }
}
