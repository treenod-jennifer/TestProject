using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using PokoAddressable;
using UnityEngine.AddressableAssets;

public class UIPopUpExpBallInfo : UIPopupBase
{
    public static UIPopUpExpBallInfo _instance = null;

    public UIPanel clippingPanel;
    public GameObject buttonOK;
    public GameObject spineRoot;
    public UITexture methodIcon_one;
    public UITexture methodIcon_two;
    public UITexture methodIcon_three;
    public UITexture methodIcon_four;
    public UITexture methodIcon_arrow;
    public UITexture maskIcon;
    
    [SerializeField] private AssetReference refTexture_01;
    [SerializeField] private AssetReference refTexture_02;
    [SerializeField] private AssetReference refTexture_03;
    [SerializeField] private AssetReference refTexture_04;
    [SerializeField] private AssetReference refTextureArrow;
    [SerializeField] private AssetReference refTextureBG;

    private LAppModelProxy spineBoni;

    public AnimationCurve methodAnimation;

    //튜토리얼에서 버튼 눌러졌는지 확인하는데 사용됨.
    [HideInInspector]
    public bool bClickBtnOK = false;

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
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, 0.1f).SetEase(ManagerUI._instance.popupAlphaAnimation);
        clippingPanel.depth = uiPanel.depth + 1;

        //스파인 세팅.
        spineBoni = LAppModelProxy.MakeLive2D(spineRoot, TypeCharacterType.Boni, "curiosity");
        spineBoni.SetAnimation("curiosity", true);
        spineBoni.SetScale(1f);

        //스파인 뎁스 설정
        spineBoni.SetSortOrder(4);

        //텍스쳐 로드.
        this.gameObject.AddressableAssetLoad<Texture>(refTexture_01,(x) => methodIcon_one.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTexture_02,(x) => methodIcon_two.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTexture_03,(x) => methodIcon_three.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTexture_04,(x) => methodIcon_four.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTextureArrow,(x) => methodIcon_arrow.mainTexture = x);
        this.gameObject.AddressableAssetLoad<Texture>(refTextureBG,(x) => maskIcon.mainTexture = x);

        //텍스쳐 움직임.
        Vector3 onePos = methodIcon_one.transform.localPosition;
        Vector3 twoPos = methodIcon_two.transform.localPosition;
        Vector3 threePos = methodIcon_three.transform.localPosition;
        Vector3 fourPos = methodIcon_four.transform.localPosition;

        methodIcon_one.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
        methodIcon_one.transform.localPosition = new Vector3(onePos.x, onePos.y - 20f, onePos.z);

        methodIcon_two.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
        methodIcon_two.transform.localPosition = new Vector3(twoPos.x, twoPos.y - 20f, twoPos.z);

        methodIcon_three.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
        methodIcon_three.transform.localPosition = new Vector3(threePos.x, threePos.y - 20f, threePos.z);

        methodIcon_four.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
        methodIcon_four.transform.localPosition = new Vector3(fourPos.x, fourPos.y - 20f, fourPos.z);

        bCanTouch = false;
        StartCoroutine(OpenAction(onePos.y, twoPos.y, threePos.y, fourPos.y));
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d 레이어 올려줌.
        if (layer != 10)
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

    IEnumerator OpenAction(float one, float two, float three, float four)
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

        methodIcon_four.transform.DOScale(Vector3.one, 0.3f).SetEase(methodAnimation);
        methodIcon_four.transform.DOLocalMoveY(four, 0.3f).SetEase(methodAnimation);
        yield return new WaitForSeconds(0.3f);

        bCanTouch = true;
        ManagerUI._instance.FocusCheck();
    }

    void OnClickBtnOK()
    {
        bClickBtnOK = true;

        ManagerUI._instance.ClosePopUpUI();
    }
}
