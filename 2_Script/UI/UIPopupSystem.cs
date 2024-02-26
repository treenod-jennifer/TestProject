using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UIPopupSystem : UIPopupBase
{
    public static UIPopupSystem _instance = null;

    //폰트.
    public UIFont _lightFont;

    public UITexture Image;
    public UILabel textCenter;
    public UILabel textSide;
    public UILabel[] title;
    public GameObject closeButton;
    public GameObject[] button;

    //임시로 설정해놓음.
    public Texture2D systemTexture;

    // Calback을 사용할 경우를 위해
    public System.Action actionYesCallback;
    public System.Action actionNoCallback;
    
    [System.NonSerialized]
    public GameObject receiveObj1 = null;
    [System.NonSerialized]
    public GameObject receiveObj2 = null;
    [System.NonSerialized]
    public GameObject receiveObj3 = null;
    [System.NonSerialized]
    public string functionButton1 = null;
    [System.NonSerialized]
    public string functionButton2 = null;
    [System.NonSerialized]
    public string functionButton3 = null;

    //콜백 호출 시기(true: 창 종료된 후, false: 버튼 눌러졌을 때)
    [System.NonSerialized]
    public bool bCallBackClose1 = false;
    [System.NonSerialized]
    public bool bCallBackClose2 = false;
    [System.NonSerialized]
    public bool bCallBackClose3 = false;

    [SerializeField]
    private UILabel buttonText1;
    [SerializeField]
    private UILabel buttonShadowText1;
    [SerializeField]
    private UILabel buttonText2;
    [SerializeField]
    private UILabel buttonShadowText2;

    void Awake()
    {
        _instance = this;
    }
    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    public void InitServerSystemPopUp(string name, string text, bool useButtons, Texture2D texture = null, string buttonText = "", Method.FunctionVoid in_callback = null)
    {
        //버튼 세팅.
        if (useButtons == false)
        {
            button[0].transform.localPosition = new Vector3(0f, button[0].transform.localPosition.y, 0f);
            string btnText = buttonText;
            buttonText1.text = btnText;
            buttonShadowText1.text = btnText;
            button[1].gameObject.SetActive(false);
        }

        //팝업 메세지 위치.
        if (texture == null)
        {
            textCenter.gameObject.SetActive(true);
            textCenter.text = text;
        }
        else
        {
            Image.gameObject.SetActive(true);
            Image.mainTexture = texture;
            Image.MakePixelPerfect();
            Image.cachedTransform.localScale = Vector3.one * 1.25f; // 128로 하고 리소스 줄이기위해
            textSide.gameObject.SetActive(true);
            textSide.text = text;
        }

        for (int j = 0; j < title.Length; j++)
        {
            title[j].text = name;
        }
        _callbackEnd += in_callback;
    }

    public void TypeSetting(PopupType pType)
    {
        popupType = pType;
    }
    
    public void InitSystemPopUp(string name, string text, bool useButtons, Texture2D texture = null, Method.FunctionVoid in_callback = null)
    {
        //버튼 세팅.
        if (useButtons == false)
        {
            button[0].transform.localPosition = new Vector3(0f, button[0].transform.localPosition.y, 0f);
            string btnText = Global._instance.GetString("btn_1");
            buttonText1.text = btnText;
            buttonShadowText1.text = btnText;
            button[1].gameObject.SetActive(false);
        }

        //팝업 메세지 위치.
        if (texture == null)
        {
            textCenter.gameObject.SetActive(true);
            textCenter.text = text;
        }
        else
        {
            Image.gameObject.SetActive(true);
            Image.mainTexture = texture;
            Image.MakePixelPerfect();
            Image.cachedTransform.localScale = Vector3.one * 1.25f; // 128로 하고 리소스 줄이기위해
            textSide.gameObject.SetActive(true);
            textSide.text = text;
        }

        for (int j = 0; j < title.Length; j++)
        {
            title[j].text = name;
        }
        if (in_callback != null)
        {
            _callbackEnd += in_callback;
        }
    }

    public void SetImageSize(int w, int h)
    {
        Image.width = w;
        Image.height = h;
        Image.cachedTransform.localScale = Vector3.one * 1.25f; // 128로 하고 리소스 줄이기위해
    }

    public void UseOtherFont(bool bCenter = true)
    {
        if (bCenter == true)
        {
            textCenter.trueTypeFont = _lightFont.dynamicFont;
        }
        else
        {
            textSide.trueTypeFont = _lightFont.dynamicFont;
        }
    }

    public void HideCloseButton()
    {
        closeButton.SetActive(false);
    }

    public void FunctionSetting(int no, string funcName, GameObject receiveObject, bool bCallBackClose = false)
    {        
        switch (no)
        {
            case 1:
                functionButton1 = funcName;
                bCallBackClose1 = bCallBackClose;
                receiveObj1 = receiveObject;
                break;
            case 2:
                functionButton2 = funcName;
                bCallBackClose2 = bCallBackClose;
                receiveObj2 = receiveObject;
                break;
            case 3:
                functionButton3 = funcName;
                bCallBackClose3 = bCallBackClose;
                receiveObj3 = receiveObject;
                break;
        }
    }

    public void SortOrderSetting(bool bUse = true, int order = 0)
    {
        //uiPanel.useSortingOrder = true;
        //if (order != 0)
        //{
        //    uiPanel.sortingOrder = order;
        //}
    }

    protected override void OnClickBtnClose()
    {
        if (actionNoCallback != null) {
            actionNoCallback();
        }
        ClosePopUpUI();
    }

    void ClosePopUpUI() {
        actionYesCallback = null;
        actionNoCallback = null;
        ManagerUI._instance.ClosePopUpUI();
    }

    void OnClickBtnButton1()
    {
        if (actionYesCallback != null) {
            actionYesCallback();
        }
        
        if (functionButton1 != null)
        {
            if (bCallBackClose1 == true)
            {
                _callbackEnd += SettingCallBack1;
            }
            else
            {
                SettingCallBack1();
            }
        }
        ClosePopUpUI();
    }

    void SettingCallBack1()
    {
        receiveObj1.SendMessage(functionButton1, gameObject, SendMessageOptions.DontRequireReceiver);
    }

    void OnClickBtnButton2()
    {
        if (actionNoCallback != null) {
            actionNoCallback();
        }
        
        if (functionButton2 != null)
        {
            if (bCallBackClose2 == true)
            {
                _callbackEnd += SettingCallBack2;
            }
            else
            {
                SettingCallBack2();
            }
        }
        ClosePopUpUI();
    }

    void SettingCallBack2()
    {
        receiveObj2.SendMessage(functionButton2, gameObject, SendMessageOptions.DontRequireReceiver);
    }

    void OnClickBtnButton3()
    {
        if (actionYesCallback != null)
        {
            actionYesCallback();
        }

        if (functionButton3 != null)
        {
            if (bCallBackClose3 == true)
            {
                _callbackEnd += SettingCallBack3;
            }
            else
            {
                SettingCallBack3();
            }
        }
        ClosePopUpUI();
    }

    void SettingCallBack3()
    {
        receiveObj3.SendMessage(functionButton3, gameObject, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// 버튼 문구 셋팅
    /// </summary>
    /// <param name="no"></param>
    /// <param name="text"></param>
    public void SetButtonText(int no, string text)
    {
        switch (no)
        { 
            case 1:
                buttonText1.text = text;
                buttonShadowText1.text = text;
                break;
            case 2:
                buttonText2.text = text;
                buttonShadowText2.text = text;
                break;
        }
    }
}