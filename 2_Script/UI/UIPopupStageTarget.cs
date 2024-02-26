using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;

public class UIPopupStageTarget : UIPopupBase
{
    static public UIPopupStageTarget instance = null;
    //타겟 생성 위치
    public GameObject TargetRoot;
    public GameObject RankRoot;
    public UILabel labelTarget;

    #region 이벤트 안내 표시
    public GameObject EventInfoRoot;
    public UISprite EventInfoSprite;
    public UILabel[] EventInfoText;
    public UILabel EventInfoLabel;
    #endregion

    float popupTime = 1.3f;

    List<StageTarget> stageTargets = new List<StageTarget>();

    void Awake()
    {
        instance = this;
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopup(string targetText = "")
    {
        if (targetText == "")
        {
            InitPopup_TargetBlock();
        }
        else
        {
            InitPopup_TargetText(targetText);
        }
        InitEventInfoUI();
        StartCoroutine(CoClosePopup());
    }

    private void InitPopup_TargetBlock()
    {
        labelTarget.gameObject.SetActive(false);

        var enumerator = ManagerBlock.instance.dicCollectCount.GetEnumerator();
        while (enumerator.MoveNext())
        {
            TARGET_TYPE targetType = enumerator.Current.Key;
            if (enumerator.Current.Value != null)
            {
                string targetName = (targetType != TARGET_TYPE.COLORBLOCK) ?
                    string.Format("StageTarget_{0}", targetType) : "StageTarget";

                var e = enumerator.Current.Value.GetEnumerator();
                while (e.MoveNext())
                {
                    BlockColorType colorType = e.Current.Key;

                    StageTarget target = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIstageMission", TargetRoot).GetComponent<StageTarget>();
                    target.targetType = targetType;
                    target.targetColor = colorType;

                    //목표 수 표시
                    string collectCount = e.Current.Value.collectCount.ToString();
                    target.targetCount.text = collectCount;
                    target.targetCountShadow.text = collectCount;

                    //목표 이미지 설정
                    string targetColorName = (colorType != BlockColorType.NONE) ?
                        string.Format("{0}_{1}", targetName, colorType) : targetName;
                    target.targetSprite.spriteName = targetColorName;
                    ManagerUIAtlas.CheckAndApplyEventAtlas(target.targetSprite);
                    target.targetSprite.MakePixelPerfect();

                    stageTargets.Add(target);
                }
            }
        }

        float startPos = (1 - stageTargets.Count) * 48;
        for (int i = 0; i < stageTargets.Count; i++)
        {
            stageTargets[i].transform.localPosition = new Vector3(startPos + 96 * i, 0, 0);
        }
    }

    private void InitPopup_TargetText(string targetText = "")
    {
        labelTarget.gameObject.SetActive(true);
        labelTarget.text = targetText;
    }

    private void InitEventInfoUI()
    {
        if(Global.GameType == GameType.NORMAL
            && ManagerStageChallenge.instance != null
            && ManagerStageChallenge.instance.Stage == Global.stageIndex)
        {
            EventInfoRoot.gameObject.SetActive(true);

            foreach (var item in EventInfoText)
                item.text = Global._instance.GetString("p_msc_3");
            EventInfoLabel.text = string.Format("{0:n0}", ManagerStageChallenge.instance.TargetScore);
        }
        else
        {
            EventInfoRoot.gameObject.SetActive(false);
        }
    }

    private IEnumerator CoClosePopup()
    {
        yield return new WaitForSeconds(popupTime);
        ManagerUI._instance.ClosePopUpUI();
    }
    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        ManagerUI._instance.bTouchTopUI = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        _callbackEnd += callback;
        if (mainSprite != null)
        {
            mainSprite.transform.DOScale(Vector3.zero, _mainTime).SetEase(Ease.InBack);
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, _mainTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        }

        if (EventInfoRoot != null)
        {
            EventInfoRoot.transform.DOScale(Vector3.zero, _mainTime).SetEase(Ease.InBack);
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

    public override void OnClickBtnBack()
    {
        return;
    }

    new void OnDestroy()
    {
        base.OnDestroy();
        for (int i = 0; i < stageTargets.Count; i++)
        {
            Destroy(stageTargets[i].gameObject);
        }
        stageTargets.Clear();

        instance = null;
    }
}
