using System;
using UnityEngine;
using DG.Tweening;

public class UIPopupDecoInformation : UIPopupBase
{
    public static UIPopupDecoInformation _instance = null;
    
    [SerializeField] private UITexture texDecoInfo;
    
    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
    
    private void Start()
    {
        texDecoInfo.mainTexture = ManagerAntiqueStore.instance.antiqueStoreResource.antiqueStorePack.TexDecoInfo;
    }
    
    public override void OpenPopUp(int depth)
    {
        //터치 관련 막음.
        bCanTouch = false;
        uiPanel.depth = depth;

        if (mainSprite != null)
        {
            mainSprite.transform.localScale = Vector3.one * 0.8f;
            mainSprite.transform.DOScale(Vector3.one, openTime).SetEase(ManagerUI._instance.popupScaleAnimation);
            mainSprite.color = new Color(mainSprite.color.r, mainSprite.color.g, mainSprite.color.b, 0f);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, openTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }

        StartCoroutine(CoPostOpenPopup());
    }

    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        ManagerUI._instance.bTouchTopUI = false;
        
        _callbackEnd += callback;
        if (mainSprite != null)
        {
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, _mainTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }

        //뒤에 깔린 검은 배경 알파 적용.
        StartCoroutine(CoAction(_mainTime, () =>
            PopUpCloseAlpha()
        ));

        //연출 끝난 후 해당 팝업 삭제.
        StartCoroutine(CoAction(_mainTime + 0.15f, () =>
        {
            ManagerUI._instance._popupList.Remove(this);
            Destroy(gameObject);
        }));
    }
}
