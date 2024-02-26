using System.Collections;
using UnityEngine;
using DG.Tweening;


/// <summary>
/// 레디 팝업에서 사용되는 버튼의 설정을 위한 스크립트
/// </summary>
public class UIItemReadyBtnStart : MonoBehaviour
{
    [SerializeField] private UISprite startBtnImage;

    #region 재화가 소모되는 타입의 버튼의 UI
    [SerializeField] private GameObject objRoot_Goods;
    [SerializeField] private GameObject objGoodsImageShadow;
    [SerializeField] private UILabel[] textStart_Goods;
    public UISprite goodsImage;
    
    //이펙트
    public GameObject _objEFfectUseClover;
    public GameObject _objEffectStartButton;
    public GameObject _objEffectButton;
    public GameObject _objRingGlow;
    #endregion
    
    #region AP가 소모되는 타입의 버튼 UI
    [SerializeField] private GameObject objAPRoot;
    [SerializeField] private UILabel[] textStart_CurrentAP;
    [SerializeField] private UILabel[] textStart_AllAP;
    #endregion

    [SerializeField] private StartType startType = StartType.CLOVER;
    [SerializeField] private BtnColorType colorType = BtnColorType.green;
    public StartType StartButtonType { get { return startType; } }
    
    /// <summary>
    /// 스타트 버튼을 누르기 위해 필요한 재화 타입
    /// </summary>
    public enum StartType
    {
        CLOVER = 0,
        AP,
    }
    
    public void SetButton()
    {
        objRoot_Goods.SetActive(startType == StartType.CLOVER);
        objAPRoot.SetActive(startType == StartType.AP);

        if (startType == StartType.CLOVER)
            SetFreeImage_Clover();
    }

    public void SetButtonText_AP(string currentAP, string allAP)
    {
        for (int i = 0; i < textStart_CurrentAP.Length; i++)
            textStart_CurrentAP[i].text = currentAP;

        for (int i = 0; i < textStart_AllAP.Length; i++)
            textStart_AllAP[i].text = allAP;
    }
    
    public void SetButton_Enable(bool isEnable)
    {
        //활성 상태에 따라 버튼 컬러 결정
        BtnColorType cType = (isEnable == true) ? colorType : BtnColorType.gray;
        SetButtonColor(cType);
    }

    public void SetButtonColor(BtnColorType cType)
    {
        var colorData = ManagerUI._instance.GetButtonColorData_BigButton(cType);
             startBtnImage.spriteName = colorData.Item1;
             
        switch (startType)
        {
            case StartType.CLOVER:
                SetButtonColor_Goods(colorData.Item2);
                break;
            case StartType.AP:
                SetButtonColor_AP(colorData.Item2);
                break;
        }
    }

    private void SetButtonColor_Goods(Color textColor)
    {
        textStart_Goods[0].effectColor= textColor;
        textStart_Goods[1].color= textColor;
        textStart_Goods[1].effectColor= textColor;
    }
    
    private void SetButtonColor_AP(Color textColor)
    {
        textStart_CurrentAP[0].effectColor= textColor;
        textStart_CurrentAP[1].color= textColor;
        textStart_CurrentAP[1].effectColor= textColor;
        
        textStart_AllAP[0].effectColor= textColor;
        textStart_AllAP[1].color= textColor;
        textStart_AllAP[1].effectColor= textColor;
    }

    #region 클로버 재화 관련 함수
    public void SetFreeImage_Clover()
    {
        if (GameData.RemainFreePlayTime() > 0)
        {   //무료 클로버일 때, 버튼 이미지 설정.
            SetCloverImage(true);
        }
    }
    
    public void SetCloverImage(bool bFree = true, bool bAction = false)
    {
        if (bFree == true)
        {
            goodsImage.spriteName = "icon_clover_infinity";
            goodsImage.width = 100;
            goodsImage.height = 100;
            objGoodsImageShadow.SetActive(false);
        }
        else
        {
            goodsImage.spriteName = "icon_clover";
            goodsImage.width = 72;
            goodsImage.height = 74;
            objGoodsImageShadow.SetActive(true);
        }

        if (bAction == true)
        {
            goodsImage.transform.localScale = Vector3.one * 0.5f;
            goodsImage.transform.DOScale(Vector3.one * 0.8f, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public void MakeFlyCloverEffect(GameObject parentObj, GameObject readyPopupObj)
    {
        //클로버날리기
        RingGlowEffect ringGlow = NGUITools.AddChild(ManagerUI._instance._CloverSprite.gameObject, _objRingGlow).GetComponent<RingGlowEffect>();
        ringGlow._effectScale = 0.45f;

        UIUseCloverEffect cloverEffect = NGUITools.AddChild(parentObj, _objEFfectUseClover).GetComponent<UIUseCloverEffect>();
        cloverEffect.targetObj = readyPopupObj;
        
        //클로버 사용(이벤트 무료 스테이지 거나, 무료 타임이 남았을 경우 투명 클로버).
        if (GameData.RemainFreePlayTime() > 0)
        {
            cloverEffect.Init(ManagerUI._instance._CloverSprite.transform.position, this.transform.position, true);
        }
        else
        {
            cloverEffect.Init(ManagerUI._instance._CloverSprite.transform.position, this.transform.position);
        }
    }

    public IEnumerator CoShowCloverEffect()
    {
        float showTimer = 0;
        float scaleRatio = 0.7f;
        float defaultScaleValue = goodsImage.cachedTransform.localScale.x;
        Vector3 startButtonPos = this.transform.localPosition;

        showTimer = 0;

        RingGlowEffect ringGlow = NGUITools.AddChild(this.transform.gameObject, _objRingGlow).GetComponent<RingGlowEffect>();
        ringGlow._effectScale = 0.9f;

        NGUITools.AddChild(this.transform.gameObject, _objEffectStartButton);
        _objEffectButton.SetActive(true);

        //이펙트 터질 때 버튼 움직임.
        ManagerSound.AudioPlay(AudioLobby.Button_01);
        while (showTimer < 0.8f)
        {
            showTimer += Global.deltaTimeLobby * 4f;

            if (showTimer < 0.5f)
            {
                scaleRatio = 0.7f + showTimer;
            }
            else
            {
                scaleRatio = 1.8f - showTimer;
            }

            this.transform.localPosition = startButtonPos * (1 + (1 - showTimer) * 0.04f);
            goodsImage.cachedTransform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
            _objEffectButton.transform.localScale = Vector3.one * scaleRatio * defaultScaleValue;
            yield return null;
        }

        _objEffectButton.SetActive(false);
        showTimer = 0;

        while (showTimer < 0.5f)
        {
            showTimer += Global.deltaTimeLobby;
            yield return null;
        }

        //인게임 씬로드.
        goodsImage.cachedTransform.localScale = Vector3.one * defaultScaleValue;
        yield return null;
    }
    
    #endregion
}
