using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Reflection;
using PokoAddressable;
using UnityEngine.AddressableAssets;

public class UIPopUpMethod : UIPopupBase
{
    public static UIPopUpMethod _instance = null;

    public UIPanel clippingPanel;
    public GameObject buttonOK;
    public GameObject spineRoot;
    public UITexture methodIcon_one;
    public UITexture methodIcon_two;
    public UITexture methodIcon_three;
    public UITexture methodButtonIcon;
    public UITexture maskIcon;

    public UILabel[] title;
    public UILabel[] mothodeText_one;
    public UILabel[] mothodeText_two;
    public UILabel[] mothodeText_three;
    public UILabel[] iconText_one;
    public UILabel[] buttonText;
    
    [SerializeField] private AssetReference refTexture_01;
    [SerializeField] private AssetReference refTexture_02;
    [SerializeField] private AssetReference refTexture_03;
    [SerializeField] private AssetReference refTexture_icon_01;
    [SerializeField] private AssetReference refTextureBG;

    private LAppModelProxy spineBoni;

    public AnimationCurve methodAnimation;

    //튜토리얼에서 버튼 눌러졌는지 확인하는데 사용됨.
    [HideInInspector] public bool bClickBtnOK = false;
    private bool isMissionFlow = false;

    private ResourceBox box;

    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }


    public override void OpenPopUp(int _depth)
    {
        bCanTouch = false;
        popupType = PopupType.method;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = _depth;
        mainSprite.transform.localScale = Vector3.one * 0.2f;
        mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
        mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, 0.1f)
            .SetEase(ManagerUI._instance.popupAlphaAnimation);
        clippingPanel.depth = uiPanel.depth + 1;

        //스파인 세팅
        spineBoni = LAppModelProxy.MakeLive2D(spineRoot, TypeCharacterType.Boni, "curiosity");
        spineBoni.SetAnimation("curiosity", true);
        spineBoni.SetScale(1f);

        //스파인 뎁스 설정
        spineBoni.SetSortOrder(4);

        //텍스쳐 로드.
        this.gameObject.AddressableAssetLoad<Texture>(refTexture_01,(x) => methodIcon_one.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTexture_02,(x) => methodIcon_two.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTexture_03,(x) => methodIcon_three.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTexture_icon_01,(x) => methodButtonIcon.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTextureBG,(x) => maskIcon.mainTexture = x);
        
        //텍스쳐 움직임.
        Vector3 onePos = methodIcon_one.transform.localPosition;
        Vector3 twoPos = methodIcon_two.transform.localPosition;
        Vector3 threePos = methodIcon_three.transform.localPosition;

        methodIcon_one.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
        methodIcon_one.transform.localPosition = new Vector3(onePos.x, onePos.y - 20f, onePos.z);

        methodIcon_two.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
        methodIcon_two.transform.localPosition = new Vector3(twoPos.x, twoPos.y - 20f, twoPos.z);

        methodIcon_three.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
        methodIcon_three.transform.localPosition = new Vector3(threePos.x, threePos.y - 20f, threePos.z);

        bCanTouch = false;
        StartCoroutine(OpenAction(onePos.y, twoPos.y, threePos.y));
    }

    public void InitData(bool isMissionFlow = false)
    {
        this.isMissionFlow = isMissionFlow;

        //텍스트 세팅.
        string titleText = Global._instance.GetString("p_met_1");
        string oneText = Global._instance.GetString("p_met_2");
        string twoText = Global._instance.GetString("p_met_3");
        string threeText = Global._instance.GetString("p_met_4");
        string playText = Global._instance.GetString("btn_3");
        string btnText = isMissionFlow ? Global._instance.GetString("btn_3") : Global._instance.GetString("btn_1");

        for (int i = 0; i < 2; i++)
        {
            title[i].text = titleText;
            mothodeText_one[i].text = oneText;
            mothodeText_two[i].text = twoText;
            mothodeText_three[i].text = threeText;
            iconText_one[i].text = playText;
            buttonText[i].text = btnText;
        }
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 레이어 올려줌.
        if (layer != 10 && spineBoni != null)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }
        spineBoni.SetSortOrder(layer);

        clippingPanel.useSortingOrder = true;
        clippingPanel.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public GameObject GetButton()
    {
        return buttonOK.gameObject;
    }

    IEnumerator OpenAction(float one, float two, float three)
    {
        yield return new WaitForSeconds(openTime);

        methodIcon_one.transform.DOScale(Vector3.one, 0.3f).SetEase(methodAnimation);
        methodIcon_one.transform.DOLocalMoveY(one, 0.3f).SetEase(methodAnimation);
        yield return new WaitForSeconds(0.15f);

        methodIcon_two.transform.DOScale(Vector3.one, 0.3f).SetEase(methodAnimation);
        methodIcon_two.transform.DOLocalMoveY(two, 0.3f).SetEase(methodAnimation);
        yield return new WaitForSeconds(0.15f);

        methodIcon_three.transform.DOScale(Vector3.one, 0.3f).SetEase(methodAnimation);
        methodIcon_three.transform.DOLocalMoveY(three, 0.3f).SetEase(methodAnimation);
        yield return new WaitForSeconds(0.3f);
        bCanTouch = true;
        ManagerUI._instance.FocusCheck();
    }

    void OnClickBtnOK()
    {
        bClickBtnOK = true;

        if (isMissionFlow)
        {
            if (ManagerUI._instance._btnComingSoon.activeInHierarchy == true)
                OpenComingSoon();
            else
            {
                //레디창오픈여부확인
                if (IsReadyCheck())
                    this._callbackClose += ManagerUI._instance.OpenPopupReadyLastStage;
            }
        }
        ManagerUI._instance.ClosePopUpUI();
    }

    void OpenComingSoon()
    {
        //커밍순 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_15"), false);
        popupSystem.SetResourceImage("Message/soon");
    }

    void TestMethod()
    {
        var type = GetType();
        foreach (var i in type.GetCustomAttributes())
        {
            Debug.Log(i.GetType().Name);
        }
    }

    private bool IsReadyCheck()
    {
        foreach (var popup in ManagerUI._instance._popupList)
        {
            var type = popup.GetType();
            if( type.GetCustomAttribute<IsReadyPopupAttribute>() != null )
            {
                return false;
            }
        }
        return true;
    }

}
