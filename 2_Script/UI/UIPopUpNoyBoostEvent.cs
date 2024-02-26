using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

public class UIPopUpNoyBoostEvent : UIPopupBase
{
    public static UIPopUpNoyBoostEvent _instance = null;
    [SerializeField] private List<UISprite> spriteList;
    [SerializeField] private List<UISprite> starSpriteList;
    [SerializeField] private List<UILabel> starLabelList;
    [SerializeField] private List<UILabel> stepLabelList;
    [SerializeField] private List<UILabel> nowStepLabel;
    [SerializeField] private List<UILabel> endStepLabel;
    [SerializeField] private UIProgressBar stepProgress;
    [SerializeField] private UILabel descriptionLabel;
    [SerializeField] private UILabel endTsLabel;
    [SerializeField] private UIPanel spineRootPanel;
    [SerializeField] private UIPanel uiRootPanel;
    [SerializeField] private Transform subTitleText;

    private SkeletonAnimation noySpine = null;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        ManagerNoyBoostEvent.instance.UnLoadNoyBoostResource(ManagerNoyBoostEvent.PrefabType.UI);
        foreach (var spr in spriteList)
            spr.atlas = null;
        Destroy(noySpine);
        noySpine = null;
        
        if (_instance == this)
            _instance = null;
        base.OnDestroy();
    }
    
    public override void SettingSortOrder(int layer)
    {
        int startLayer = 10;
        if (layer >= 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            startLayer = layer;
        }
        
        //노이 스파인의 레이어 세팅
        spineRootPanel.sortingOrder = startLayer;
        spineRootPanel.depth = uiPanel.depth + 1;
        
        //노이 스파인 상단 패널 레이어 세팅
        uiRootPanel.sortingOrder = startLayer + 1;
        uiRootPanel.depth = uiPanel.depth + 2;
        
        // 레이어 정렬값 세팅
        sortOrderCount += 2;
        panelCount += 2;
    }

    public void InitPopup()
    {
        // 번들 리소스 설정
        SetBundleResource();
        
        // 게이지 바 설정
        SetBoostProgress();
        
        // 기타 타이머 및 텍스트 설정
        EndTsTimer.Run(endTsLabel, ManagerNoyBoostEvent.instance.EndTs);
        descriptionLabel.text = Global._instance.GetString("p_noyboost_4").Replace("[n]", ManagerNoyBoostEvent.instance.StartStage.ToString()).Replace("[m]", ManagerNoyBoostEvent.instance.EndStage.ToString());
        
        if (!LanguageUtility.IsShowBuyInfo)
            subTitleText.localPosition = new Vector2(subTitleText.localPosition.x, 13f);
    }

    private void SetBundleResource()
    {
        if (ManagerNoyBoostEvent.instance.NoyBoostPackUI == null)
            return;
        
        // 아틀라스 할당
        foreach (var spr in spriteList)
            spr.atlas = ManagerNoyBoostEvent.instance.NoyBoostPackUI.UIAtlas;
        
        // 캐릭터 스파인 생성
        if (noySpine == null)
            noySpine = Instantiate(ManagerNoyBoostEvent.instance.NoyBoostPackUI.NoySpineObj, spineRootPanel.transform).GetComponentInChildren<SkeletonAnimation>();
        noySpine.AnimationName = "idle";
        noySpine.transform.localScale = Vector3.one * 100f;
        noySpine.transform.localPosition = Vector3.zero;
    }

    private void SetBoostProgress()
    {
        List<int> comboList = ManagerNoyBoostEvent.instance.ComboList;
        int comboStep = ManagerNoyBoostEvent.instance.ComboStep;
        
        // comboStep이 comboList에 포함된 경우 딱 맞아떨어지는 값 설정 (구간에 포함될 필요가 없기 때문)
        if (comboStep == 0 || comboList.Contains(comboStep))
            stepProgress.value = 0.25f * ManagerNoyBoostEvent.instance.GetBoostStep();
        // comboStep이 부스팅 1~2, 2~3, 3~4 구간에 포함될 경우 해당 구간 내 퍼센테이지 계산 후 4로 분할
        else
        {
            if (comboStep < comboList[0])
                stepProgress.value = 0.25f + (float) comboStep / comboList[0] / 4;
            else if (comboStep < comboList[1])
                stepProgress.value = 0.50f + ((float) comboStep - comboList[0]) / (comboList[1] - comboList[0]) / 4;
            else if (comboStep < comboList[2])
                stepProgress.value = 0.75f + ((float) comboStep - comboList[1]) / (comboList[2] - comboList[1]) / 4;
            else
                stepProgress.value = 1;
        }

        // 단계 별똥별 이미지 설정
        int boostStep = ManagerNoyBoostEvent.instance.GetBoostStep() - 1;
        starSpriteList[boostStep].spriteName = "ev_boosting_skill_icon_on";
        starSpriteList[boostStep].transform.localScale = Vector3.one * 1.1f;
        starLabelList[boostStep].color = GetHexCodeColor("#ffe952");
        starLabelList[boostStep].effectColor = GetHexCodeColor("#d76c15");
        
        // 단계 텍스트 설정
        for (int i = 0; i < stepLabelList.Count; i++)
        {
            stepLabelList[i].text = Global._instance.GetString("p_noyboost_3").Replace("[n]", (i + 1).ToString());
            starLabelList[i].text = "x" + (i + 1);
        }
        foreach (var str in nowStepLabel)
            str.text = ManagerNoyBoostEvent.instance.GetBoostStep().ToString();
        foreach (var str in endStepLabel)
            str.text = "/" + stepLabelList.Count;
    }

    private Color GetHexCodeColor(string code)
    {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(code, out color);

        return color;
    }

    #region 버튼 이벤트

    private void OnClickStartButton()
    {
        if (ManagerNoyBoostEvent.CheckStartable())
            AsyncStartButton().Forget();
        else
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
        }
    }

    private async UniTask AsyncStartButton()
    {
        ManagerUI._instance.ClosePopUpUI();
        await UniTask.WaitForSeconds(0.2f);
        if (GameData.User.stage < ManagerNoyBoostEvent.instance.EndStage)
            ManagerUI._instance.OpenPopupReadyLastStage(false);
        else
            ManagerUI._instance.OpenPopupReadyIndexStage(ManagerNoyBoostEvent.instance.EndStage);
    }
    
    #endregion
}
