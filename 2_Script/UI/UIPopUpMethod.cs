using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIPopUpMethod : UIPopupBase
{
    public static UIPopUpMethod _instance = null;

    public UIPanel clippingPanel;
    public GameObject buttonOK;
    public GameObject live2dRoot;
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

    public AnimationCurve methodAnimation;

    private LAppModelProxy boniLive2D;

    //튜토리얼에서 버튼 눌러졌는지 확인하는데 사용됨.
    [HideInInspector]
    public bool bClickBtnOK = false;

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
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, 0.1f).SetEase(ManagerUI._instance.popupAlphaAnimation);
        clippingPanel.depth = uiPanel.depth + 1;

        boniLive2D = NGUITools.AddChild(live2dRoot, ManagerCharacter._instance._live2dObjects[(int)TypeCharacterType.Boni].obj).GetComponent<LAppModelProxy>();
        boniLive2D._CubismRender.SortingOrder = 4;
        boniLive2D.setAnimation(false, "Ready_in");
        boniLive2D.setAnimation(false, "curiosity");
        
        //텍스쳐 로드.
        methodIcon_one.mainTexture = Resources.Load("UI/method_icon_01") as Texture2D;
        methodIcon_two.mainTexture = Resources.Load("UI/method_icon_02") as Texture2D;
        methodIcon_three.mainTexture = Resources.Load("UI/method_icon_03") as Texture2D;
        methodButtonIcon.mainTexture = Resources.Load("UI/method_icon_01_01") as Texture2D;
        maskIcon.mainTexture = Resources.Load("UI/method_bg_01") as Texture2D;

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

        //텍스트 세팅.
        string titleText    = Global._instance.GetString("p_met_1");
        string oneText      = Global._instance.GetString("p_met_2");
        string twoText      = Global._instance.GetString("p_met_3");
        string threeText    = Global._instance.GetString("p_met_4");
        string playText     = Global._instance.GetString("btn_19");
        string btnText      = Global._instance.GetString("btn_1");

        for (int i = 0; i < 2; i++)
        {
            title[i].text               = titleText;
            mothodeText_one[i].text     = oneText;
            mothodeText_two[i].text     = twoText;
            mothodeText_three[i].text   = threeText;
            iconText_one[i].text        = playText;
            buttonText[i].text          = btnText;
        }

        bCanTouch = false;
        StartCoroutine(OpenAction(onePos.y, twoPos.y, threePos.y));
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 레이어 올려줌.
        if (layer != 10 && boniLive2D != null)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }
        boniLive2D._CubismRender.SortingOrder = layer;
        clippingPanel.useSortingOrder = true;
        clippingPanel.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public GameObject GetButton()
    {
        return buttonOK.gameObject;
    }

    public override void ClosePopUp(float _startTime = 0.3f, Method.FunctionVoid callback = null)
    {
        boniLive2D.setAnimation(false, "Ready_out");
        base.ClosePopUp();
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
    }

    void OnClickBtnOK()
    {
        bClickBtnOK = true;

        if (ManagerUI._instance._btnComingSoon.activeInHierarchy == true)
        {
            OpenComingSoon();
        }
        else
        {
            //레디창오픈여부확인
            bool isOpenReady = true;
            int pCount = ManagerUI._instance._popupList.Count;
            foreach (var popup in ManagerUI._instance._popupList)
            {
                if (popup.name == "UIPopUpReady")
                {
                    isOpenReady = false;
                    break;
                }
            }

            if (isOpenReady)
            {
                this._callbackClose += ManagerUI._instance.OpenPopupReadyLastStage;
            }
            ManagerUI._instance.ClosePopUpUI();
        }
    }

    void OpenComingSoon()
    {
        //커밍순 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        Texture2D texture = Resources.Load("Message/soon") as Texture2D;
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_15"), false, texture);
    }
}
