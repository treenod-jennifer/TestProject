using UnityEngine;
using PokoAddressable;

public class UIPopupSystem : UIPopupBase
{
    private const string ADDRESSABLE_PATH = "local_message/";
    
    public static UIPopupSystem _instance = null;

    //폰트.
    public UIFont _lightFont;

    public UITexture Image;
    public UIUrlTexture urlImage;
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

    public System.Action actionBtnCallback1;
    public System.Action actionBtnCallback2;
    public System.Action actionBtnCallback3;
    public System.Action actionBtnCallback4;

    [System.NonSerialized]
    public GameObject receiveObj1 = null;
    [System.NonSerialized]
    public GameObject receiveObj2 = null;
    [System.NonSerialized]
    public GameObject receiveObj3 = null;
    [System.NonSerialized]
    public GameObject receiveObj4 = null;
    [System.NonSerialized]
    public string functionButton1 = null;
    [System.NonSerialized]
    public string functionButton2 = null;
    [System.NonSerialized]
    public string functionButton3 = null;
    [System.NonSerialized]
    public string functionButton4 = null;

    //콜백 호출 시기(true: 창 종료된 후, false: 버튼 눌러졌을 때)
    [System.NonSerialized]
    public bool bCallBackClose1 = false;
    [System.NonSerialized]
    public bool bCallBackClose2 = false;
    [System.NonSerialized]
    public bool bCallBackClose3 = false;
    [System.NonSerialized]
    public bool bCallBackClose4 = false;

    [SerializeField]
    private UILabel buttonText1;
    [SerializeField]
    private UILabel buttonShadowText1;
    [SerializeField]
    private UISprite buttonImage1;
    [SerializeField]
    private UILabel buttonText2;
    [SerializeField]
    private UILabel buttonShadowText2;
    [SerializeField]
    private UISprite buttonImage2;

    //스테이지 플레이 아이콘
    [SerializeField]
    private GameObject stagePlayIcon1;
    [SerializeField]
    private GameObject stagePlayIcon2;

    [SerializeField] UIPokoButton lanLinkButton;
    string lanLinkAddress;
    string lanLinkSubject;
    
    [System.NonSerialized]
    public  bool useBlockClick = false; // 최초 클릭 후 클릭 처리를 막을지 여부.
    private bool clicked       = false; // 클릭을 했는지 여부.

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if(box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    void Awake()
    {
        _instance = this;
    }
    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    public void InitServerSystemPopUp(string name, string text, bool useButtons, string buttonText = "", Method.FunctionVoid in_callback = null, bool isError = false)
    {
        if (isError)
            _callbackOpen += () => ClickBlocker.Make(0.1f);

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
        textCenter.text = text;
        textSide.text = text;
        SetImage(Image.mainTexture as Texture2D);

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
    
    public void InitSystemPopUp(string name, string text, bool useButtons, Method.FunctionVoid in_callback = null, bool isError = false)
    {
        if (isError)
            _callbackOpen += () => ClickBlocker.Make(0.1f);

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
        textCenter.text = text;
        textSide.text = text;
        SetImage(Image.mainTexture as Texture2D);

        for (int j = 0; j < title.Length; j++)
        {
            title[j].text = name;
        }
        if (in_callback != null)
        {
            _callbackEnd += in_callback;
        }
    }

    public void SetResourceImage(string path, Texture2D failTexture = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            SetImage(failTexture);
            return;
        }

        if (path.Contains("Message/"))
            path = ADDRESSABLE_PATH + path.Substring(path.IndexOf("/") + 1);
        
        gameObject.AddressableAssetLoad<Texture2D>(path, (texture) => SetImage(texture), () => SetImage(failTexture));
    }

    public void SetStremingImage(string path)
    {
        Box.LoadStreaming(path, (Texture2D texture) => SetImage(texture));
    }

    public void SetImage(Texture2D texture)
    {
        if (texture == null)
        {
            Image.gameObject.SetActive(false);
            textSide.gameObject.SetActive(false);
            textCenter.gameObject.SetActive(true);
        }
        else
        {
            Image.gameObject.SetActive(true);
            textSide.gameObject.SetActive(true);
            textCenter.gameObject.SetActive(false);

            Image.mainTexture = texture;
            Image.MakePixelPerfect();
            Image.cachedTransform.localScale = Vector3.one * 1.25f; // 128로 하고 리소스 줄이기위해
        }
    }

    /// <summary>
    /// RewardType을 직접 받아와 해당 타입의 보상을 직접 시스템 팝업에 세팅해주는 함수
    /// </summary>
    public void SetImage_UseRewardHelper(RewardType type, int value = 0)
    {
        Image.gameObject.SetActive(true);
        textSide.gameObject.SetActive(true);
        textCenter.gameObject.SetActive(false);

        RewardHelper.SetTexture(urlImage, type, value);
        Image.MakePixelPerfect();
        Image.cachedTransform.localScale = Vector3.one * 1.25f; // 128로 하고 리소스 줄이기위해
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

    public void SetFontSize (int size, bool bCenter = true)
    {
        if (bCenter == true)
        {
            textCenter.fontSize = size;
        }
        else
        {
            textSide.fontSize = size;
        }
    }

    public void HideCloseButton()
    {
        closeButton.SetActive(false);
    }

    //버튼 동작 시, SendMessage형태로 특정 함수를 호출할 때 사용
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
            case 4:
                functionButton4 = funcName;
                bCallBackClose4 = bCallBackClose;
                receiveObj4 = receiveObject;
                break;
        }
    }

    //버튼 동작 시, Callback형태로 특정 함수를 호출할 때 사용
    public void SetCallbackSetting(int no, System.Action callback, bool bCallBackClose = false)
    {
        switch (no)
        {
            case 1:
                bCallBackClose1 = bCallBackClose;
                actionBtnCallback1 = callback;
                break;
            case 2:
                bCallBackClose2 = bCallBackClose;
                actionBtnCallback2 = callback;
                break;
            case 3:
                bCallBackClose3 = bCallBackClose;
                actionBtnCallback3 = callback;
                break;
            case 4:
                bCallBackClose4 = bCallBackClose;
                actionBtnCallback4 = callback;
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

    public override void OnClickBtnBack()
    {
        OnClickBtnButton4();
        base.OnClickBtnBack();
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
        
        if (ManagerUI._instance != null)
        {
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    void OnClickBtnButton1()
    {
        if (CanClickButton() == false)
        {
            return;
        }

        if (actionYesCallback != null) {
            actionYesCallback();
        }

        if (actionBtnCallback1 != null)
        {
            if (bCallBackClose1 == true)
                _callbackEnd += () => actionBtnCallback1();
            else
                actionBtnCallback1.Invoke();
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
        if (CanClickButton() == false)
        {
            return;
        }
        
        if (actionNoCallback != null) {
            actionNoCallback();
        }

        if (actionBtnCallback2 != null)
        {
            if (bCallBackClose2 == true)
                _callbackEnd += () => actionBtnCallback2();
            else
                actionBtnCallback2.Invoke();
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
        if (CanClickButton() == false)
        {
            return;
        }
        
        if (actionYesCallback != null)
        {
            actionYesCallback();
        }

        if (actionBtnCallback3 != null)
        {
            if (bCallBackClose3 == true)
                _callbackEnd += () => actionBtnCallback3();
            else
                actionBtnCallback3.Invoke();
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
    
    void OnClickBtnButton4()
    {
        if (CanClickButton() == false)
        {
            return;
        }
        
        if (actionBtnCallback4 != null)
        {
            if (bCallBackClose4 == true)
                _callbackEnd += () => actionBtnCallback4();
            else
                actionBtnCallback4.Invoke();
        }

        if (functionButton4 != null)
        {
            if (bCallBackClose4 == true)
            {
                _callbackEnd += SettingCallBack4;
            }
            else
            {
                SettingCallBack4();
            }
        }
    }

    void SettingCallBack4()
    {
        receiveObj4.SendMessage(functionButton4, gameObject, SendMessageOptions.DontRequireReceiver);
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

    public void SetButtonTextSpacing(int no, int spacing)
    {
        switch (no)
        {
            case 1:
                buttonText1.spacingX = spacing;
                buttonShadowText1.spacingX = spacing;
                break;
            case 2:
                buttonText2.spacingX = spacing;
                buttonShadowText2.spacingX = spacing;
                break;
        }
    }

    public void SetButtonColor(int no, BtnColorType btnColorType)
    {
        var colorData = ManagerUI._instance.GetButtonColorData_BigButton(btnColorType);

        switch (no)
        {
            case 1:
                buttonImage1.spriteName = colorData.Item1;
                buttonText1.effectColor = colorData.Item2;
                buttonShadowText1.color = colorData.Item2;
                buttonShadowText1.effectColor = colorData.Item2;
                break;
            case 2:
                buttonImage2.spriteName = colorData.Item1;
                buttonText2.effectColor = colorData.Item2;
                buttonShadowText2.color = colorData.Item2;
                buttonShadowText2.effectColor = colorData.Item2;
                break;
        }
    }

    public void SetLANLinkButton(string linkAddr, string linkSubject)
    {
        this.lanLinkButton.gameObject.SetActive(true);
        this.lanLinkAddress = linkAddr;
        this.lanLinkSubject = linkSubject;
        this.lanLinkButton.SetLabel(linkSubject);
    }

    void OnClickLANInfo()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, lanLinkAddress, lanLinkSubject);
    }

    public void SetButtonStagePlay(int no)
    {
        string btnString = Global._instance.GetString("btn_3");
        SetButtonText(no, btnString);
        SetButtonTextSpacing(no, -3);
        switch (no)
        {
            case 1:
                stagePlayIcon1.SetActive(true);
                buttonText1.transform.localPosition = new Vector3(17f, buttonText1.transform.localPosition.y, buttonText1.transform.localPosition.z);
                break;
            case 2:
                stagePlayIcon1.SetActive(true);
                buttonText2.transform.localPosition = new Vector3(17f, buttonText2.transform.localPosition.y, buttonText2.transform.localPosition.z);
                break;
        }
    }

    /// UIPokoButton에서 동작하는 ClickDelay는 로비, 인게임 씬에서만 동작하기 때문에 그 외 씬에서 시스템 팝업을 사용 할 경우 다중 클릭 방지 처리 필요.
    private bool CanClickButton()
    {
        if (useBlockClick == false)
        {
            return true;
        }

        if (clicked == false)
        {
            clicked = true;
            return true;
        }
        else
        {
            return false;
        }
    }
}