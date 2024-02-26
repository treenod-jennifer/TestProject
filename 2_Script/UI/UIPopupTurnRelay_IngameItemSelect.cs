using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using PokoAddressable;

public class UIPopupTurnRelay_IngameItemSelect : UIPopupBase
{
    //캐릭터 스파인
    [SerializeField] private SkeletonAnimation spineItemSelect;

    //스파인 위에 올라갈 패널
    [SerializeField] UIPanel panel_upperSpine;

    //상단 패널 및 위젯
    [SerializeField] private UIWidget widget_topCenter;
    [SerializeField] private UIPanel panel_topCenter;
    [SerializeField] private UILabel labelWave_Current;
    [SerializeField] private UILabel labelWave_All;

    //포인트 및 이월 턴 정보
    [SerializeField] private GameObject effectEventPoint;
    [SerializeField] private UILabel labelEventPointCount;
    [SerializeField] private UILabel labelRemainTurnCount;
    private Color fontColor_bonusCount = new Color(1f, 230f / 255f, 18f / 255);
    private Color effectColor_bonusCount = new Color(152f / 255f, 82f / 255f, 5f / 255f);

    //말풍선
    [SerializeField] private UILabel labelItemSelect;
    [SerializeField] private UISprite spriteSelectItemBubble;

    //보너스 아이템
    [SerializeField] private UIItemTurnRelay_SelectItem[] selectItems;
    [SerializeField] private UILabel[] labelItemEventPointCount;

    //배경 텍스쳐
    [SerializeField] private UITexture textureBG;

    //아이템 선택했을 때 실행될 콜백함수
    private System.Action<ManagerTurnRelay.BONUSITEM_TYPE> selectAction = null;

    private int clearLayer = 1;

    private void Start()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            if ((float)Screen.height / (float)Screen.width > 2f || (float)Screen.width / (float)Screen.height > 2f)
            {
                widget_topCenter.topAnchor.absolute = -100;
                widget_topCenter.UpdateAnchors();
            }
        }
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        panel_upperSpine.depth = depth + 1;
        _callbackOpen += () => StartCoroutine(CoActionOpenPopup());
        InitTexture();
        InitInfo();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;

        clearLayer = layer;

        //클리어창 전에 팝업들이 sortOrder을 사용하지 않는 다면 live2d, spine만 레이어 올려줌.
        if (layer == 10)
        {
            spineItemSelect.GetComponent<MeshRenderer>().sortingOrder = clearLayer;
            clearLayer += 1;
        }
        else
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = clearLayer;
            spineItemSelect.GetComponent<MeshRenderer>().sortingOrder = clearLayer + 1;
            clearLayer += 2;
        }
        panel_upperSpine.useSortingOrder = true;
        panel_topCenter.useSortingOrder = true;
        panel_upperSpine.sortingOrder = clearLayer + 1;
        panel_topCenter.sortingOrder = clearLayer + 1;

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    private void InitTexture()
    { 
        gameObject.AddressableAssetLoad<Texture>("local_turn_relay/turnRelay_SelectItem_BG", texture =>
        {
            textureBG.mainTexture = texture;
            textureBG.width = 620;
            textureBG.height = 225;
        });
    }

    private void InitInfo()
    {
        labelEventPointCount.text = ManagerTurnRelay.turnRelayIngame.GetEventPoint_AtWave().Item1.ToString();

        //남은 턴의 표시(첫 웨이브에서는 0으로 표시)
        int remainTurn = ManagerTurnRelay.turnRelayIngame.RemainTurn;
        labelRemainTurnCount.text = (remainTurn == 0 || ManagerTurnRelay.turnRelayIngame.CurrentWave == 1) ?
            "0" : string.Format("+{0}", remainTurn);

        //현재 스테이지가 럭키 스테이지인 경우, UI 표시
        bool isLuckyStage = ManagerTurnRelay.turnRelayIngame.IsLuckyWave();
        effectEventPoint.SetActive(isLuckyStage);
        if (isLuckyStage == true)
        { 
            labelEventPointCount.color = fontColor_bonusCount;
            labelEventPointCount.effectColor = effectColor_bonusCount;
        }
    }

    private IEnumerator CoActionOpenPopup()
    {
        //뮈샤 사운드
        ManagerSound.AudioPlay(AudioLobby.m_misha_appear);

        //웨이브 표시
        DOTween.ToAlpha(() => labelWave_Current.color, x => labelWave_Current.color = x, 1f, 0.1f);
        DOTween.ToAlpha(() => labelWave_All.color, x => labelWave_All.color = x, 1f, 0.1f);

        //사운드 나오고 연출 나오는 동안 잠시 대기
        yield return new WaitForSeconds(0.5f);

        //인게임 아이템들 터치 활성화
        for (int i = 0; i < selectItems.Length; i++)
        {
            selectItems[i].SetIsCanTouchItem(true);
        }
    }

    public void InitPopup(System.Action<ManagerTurnRelay.BONUSITEM_TYPE> action)
    {
        InitWave();
        InitBubble();

        for (int i = 0; i < selectItems.Length; i++)
        {
            selectItems[i].InitItem(SelectBonusItem);
        }

        for (int i = 0; i < labelItemEventPointCount.Length; i++)
        {
            labelItemEventPointCount[i].text =
                ManagerTurnRelay.turnRelayIngame.GetBonusEventPoint_AtWave().ToString();
        }

        selectAction = action;
    }

    public void InitWave()
    {
        //웨이브 텍스트 설정
        labelWave_Current.text = string.Format("{0} {1}/", Global._instance.GetString("p_sc_5"), ManagerTurnRelay.turnRelayIngame.CurrentWave);
        labelWave_All.text = ManagerTurnRelay.instance.MaxWaveCount.ToString();

        //텍스트 알파값 설정
        labelWave_Current.alpha = 0;
        labelWave_All.alpha = 0;
    }

    public void InitBubble()
    {
        Vector2 labelSize = labelItemSelect.printedSize;
        spriteSelectItemBubble.width = Mathf.RoundToInt(labelSize.x + 40);
    }

    private void SelectBonusItem(ManagerTurnRelay.BONUSITEM_TYPE type)
    {
        ManagerTurnRelay.turnRelayIngame.SelectBonusItemType = type;
        ManagerTurnRelay.turnRelayIngame.listTotalBonusItem_Select[(int)type]++;
        switch (type)
        {
            case ManagerTurnRelay.BONUSITEM_TYPE.ADD_TURN:
                ManagerTurnRelay.turnRelayIngame.RemainTurn += 1;
                break;
            case ManagerTurnRelay.BONUSITEM_TYPE.LINE_BOMB:
            case ManagerTurnRelay.BONUSITEM_TYPE.CIRCLE_BOMB:
            case ManagerTurnRelay.BONUSITEM_TYPE.RAINBOW_BOMB:
                ManagerTurnRelay.turnRelayIngame.SetData_DicIngameItemGainCount(type);
                break;
            case ManagerTurnRelay.BONUSITEM_TYPE.EVENTPOINT:
                ManagerTurnRelay.turnRelayIngame.SetBonusEventPoint();
                break;
        }
        
        //인게임 아이템들 터치 비활성화
        for (int i = 0; i < selectItems.Length; i++)
        {
            selectItems[i].SetIsCanTouchItem(false);
        }

        //아이템 선택 콜백 실행
        selectAction?.Invoke(type);
        OnClickBtnClose();
    }

    /// <summary>
    /// 뒤로 가기 버튼으로 창 닫히지 않도록 설정
    /// </summary>
    public override void OnClickBtnBack()
    {
        return;
    }
}
