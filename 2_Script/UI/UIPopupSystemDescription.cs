using UnityEngine;
using PokoAddressable;

public class UIPopupSystemDescription : UIPopupBase
{
    private const string ADDRESSABLE_PATH = "local_message/";
    
    public static UIPopupSystemDescription _instance = null;

    // Button.
    [SerializeField] private UISprite buttonIconImage1;
    [SerializeField] private UILabel buttonText1;
    [SerializeField] private UILabel buttonShadowText1;
    [SerializeField] private UISprite buttonImage1;
    [SerializeField] private UILabel buttonText2;
    [SerializeField] private UILabel buttonShadowText2;
    [SerializeField] private UISprite buttonImage2;
    // Message.
    [SerializeField] private UILabel centerMessageLabel;
    [SerializeField] private UILabel sideMessageLabel;
    // Image.
    [SerializeField] private UITexture image;
    // Description.
    [SerializeField] private UILabel descriptionLabel;
    
    [SerializeField] private UIFont lightFont;
    [SerializeField] private UILabel[] titleLabels;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject[] buttonObjects;
    
    [SerializeField] private UIItemLanpageButton lanpageButton;
    
    [SerializeField] private NGUIFont fontLilit;
    
    #region Callback
    // Callback을 사용할 경우를 위해.
    public System.Action actionYesCallback;// Close시에도 호출됨.
    public System.Action actionNoCallback;

    private System.Action actionBtnCallback1;
    private System.Action actionBtnCallback2;
    private System.Action actionBtnCallback3;
    
    // 콜백 호출 시기(true: 창 종료된 후, false: 버튼 눌러졌을 때).
    private bool bCallBackClose1 = false;
    private bool bCallBackClose2 = false;
    private bool bCallBackClose3 = false;
    #endregion

    void Awake()
    {
        _instance = this;
    }
    
    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    
    public void InitSystemPopUp(string title, string message, bool useButtons, Method.FunctionVoid endCallback = null, bool isError = false, PackagePriceType price_type = PackagePriceType.None, string price_value = "")
    {
        if (isError)
            _callbackOpen += () => ClickBlocker.Make(0.1f);
        
        //버튼 세팅.
        if (useButtons == false)
        {
            buttonObjects[0].transform.localPosition = new Vector3(0f, buttonObjects[0].transform.localPosition.y, 0f);
            string btnText = price_value.Equals("") ? Global._instance.GetString("btn_1") : price_value;
            buttonText1.text = btnText;
            buttonShadowText1.text = btnText;
            buttonObjects[1].gameObject.SetActive(false);

            //결제 팝업인 경우  버튼에 결제 타입 아이콘과 가격을 표시
            if (price_type != PackagePriceType.None)
            {
                buttonText1.spacingX = 0;
                buttonShadowText1.spacingX = 0;
                
                if (price_type != PackagePriceType.Cash)
                {
                    buttonText1.font = fontLilit;
                    buttonShadowText1.font = fontLilit;
                    
                    Vector2 text1Position =
                        new Vector2(buttonText1.transform.localPosition.x + 20, buttonText1.transform.localPosition.y);
                    buttonText1.transform.localPosition = text1Position;

                    buttonIconImage1.spriteName = GetPriceSpriteName(price_type);
                    buttonIconImage1.gameObject.SetActive(true);
                    buttonIconImage1.transform.localPosition = new Vector2(buttonText1.transform.localPosition.x - 38f - (buttonText1.text.Length * 10), buttonIconImage1.transform.localPosition.y);
                }
            }
        }

        //팝업 메세지 위치.
        centerMessageLabel.text = message;
        sideMessageLabel.text = message;
        SetImage(image.mainTexture as Texture2D);

        foreach (var t in this.titleLabels)
        {
            t.text = title;
        }
        if (endCallback != null)
        {
            _callbackEnd += endCallback;
        }
        
        //패키지 가격 타입에 해당하는 이미지명을 가져오기 위한 함수
        string GetPriceSpriteName(PackagePriceType type)
        {
            switch (type)
            {
                case PackagePriceType.Coin:
                    return "icon_coin";
                case PackagePriceType.Jewel:
                    return "icon_gem";
            }
            
            return "";
        }
    }

    public void ShowBuyInfo(string key)
    {
        if (LanguageUtility.IsShowBuyInfo && !string.IsNullOrEmpty(key))
        {
            descriptionLabel.text = Global._instance.GetString(key);
            descriptionLabel.gameObject.SetActive(true);
            
            UpdateDescriptionLabelPosition();
        }
    }
    
    public void ShowLanPageButton(string lanpageURL, string title)
    {
        lanpageButton.gameObject.SetActive(true);
        lanpageButton.On(lanpageURL, title);
        
        UpdateDescriptionLabelPosition();
    }

    private void UpdateDescriptionLabelPosition()
    {
        if (descriptionLabel.isActiveAndEnabled && lanpageButton.isActiveAndEnabled)
        {
            descriptionLabel.transform.localPosition = new Vector3(0f, -285f, 0f);
        }
    }

    public void HideCloseButton()
    {
        closeButton.SetActive(false);
    }

    private void ClosePopupUI() {
        actionYesCallback = null;
        actionNoCallback = null;
        ManagerUI._instance.ClosePopUpUI();
    }
    
    #region Image
    /// <summary>
    /// 이미지 추가(Resources 폴더)
    /// </summary>
    public void SetResourceImage(string path, Texture2D failTexture = null)
    {
        // 의도적으로 path에 빈 값을 넣고 failTexture을 사용할 경우 아래 조건문에서 처리 (path가 존재하는 데 못 불러올 경우 AddressableAssetLoader에서 onFailed 처리)
        if (string.IsNullOrEmpty(path))
        {
            SetImage(failTexture);
            return;
        }
        
        if (path.Contains("Message/"))
            path = ADDRESSABLE_PATH + path.Substring(path.IndexOf("/") + 1);
        
        gameObject.AddressableAssetLoad<Texture2D>(path, (texture) => SetImage(texture), () => SetImage(failTexture));
    }

    public void SetImage(Texture2D texture)
    {
        if (texture == null)
        {
            image.gameObject.SetActive(false);
            sideMessageLabel.gameObject.SetActive(false);
            centerMessageLabel.gameObject.SetActive(true);
        }
        else
        {
            image.gameObject.SetActive(true);
            sideMessageLabel.gameObject.SetActive(true);
            centerMessageLabel.gameObject.SetActive(false);

            image.mainTexture = texture;
            image.MakePixelPerfect();
            image.cachedTransform.localScale = Vector3.one * 1.25f; // 128로 하고 리소스 줄이기위해
        }
    }

    public void SetImageSize(int w, int h)
    {
        image.width = w;
        image.height = h;
        image.cachedTransform.localScale = Vector3.one * 1.25f; // 128로 하고 리소스 줄이기위해
    }
    #endregion

    #region Font
    public void UseOtherFont(bool bCenter = true)
    {
        if (bCenter == true)
        {
            centerMessageLabel.trueTypeFont = lightFont.dynamicFont;
        }
        else
        {
            sideMessageLabel.trueTypeFont = lightFont.dynamicFont;
        }
    }

    public void SetFontSize (int size, bool bCenter = true)
    {
        if (bCenter == true)
        {
            centerMessageLabel.fontSize = size;
        }
        else
        {
            sideMessageLabel.fontSize = size;
        }
    }
    #endregion
    
    #region Callback
    /// <summary>
    /// 버튼 동작 시 Callback형태로 특정 함수를 호출할 때 사용
    /// </summary>
    public void SetCallbackSetting(int buttonType, System.Action callback, bool bCallBackClose = false)
    {
        switch (buttonType)
        {
            //Yes
            case 1:
                bCallBackClose1 = bCallBackClose;
                actionBtnCallback1 = callback;
                break;
            //No
            case 2:
                bCallBackClose2 = bCallBackClose;
                actionBtnCallback2 = callback;
                break;
            //Close
            case 3:
                bCallBackClose3 = bCallBackClose;
                actionBtnCallback3 = callback;
                break;
        }
    }
    #endregion
    
    #region Button Click
    protected override void OnClickBtnClose()
    {
        if (actionNoCallback != null) {
            actionNoCallback();
        }
        ClosePopupUI();
    }
    
    private void OnClickBtnButton1()
    {
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

        ClosePopupUI();
    }
    
    private void OnClickBtnButton2()
    {
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
        
        ClosePopupUI();
    }
    
    private void OnClickBtnButton3()
    {
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

        ClosePopupUI();
    }
    #endregion

    #region Button Settings
    /// <summary>
    /// 버튼 문구 셋팅.
    /// </summary>
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

    /// <summary>
    /// 버튼 텍스트 간격 수정.
    /// </summary>
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

    /// <summary>
    /// 버튼 색 변경.
    /// </summary>
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
    #endregion
}