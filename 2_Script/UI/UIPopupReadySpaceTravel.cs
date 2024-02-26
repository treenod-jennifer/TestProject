using System.Collections;
using UnityEngine;

[IsReadyPopup]
public class UIPopupReadySpaceTravel : UIPopupReadyBase
{
    [SerializeField] private UISprite _readyUI;
    [SerializeField] private UISprite _spriteBg;
    [SerializeField] private UISprite _spritePlanet;
    [SerializeField] private AudioLobby _defaultSound;

    private ImportData_Live2D.ImportData _importData = new ImportData_Live2D.ImportData();

    public override void InitPopUp(StageMapData stageData)
    {
        base.InitPopUp(stageData);

        _spriteBg.atlas                  = ManagerSpaceTravel.instance._spaceTravelPackUI.UIAtlas;
        _spritePlanet.atlas              = ManagerSpaceTravel.instance._spaceTravelPackUI.UIAtlas;
        _readyUI.pivot                   = UIWidget.Pivot.Bottom;
        _readyUI.height                  = 980;
        _readyUI.transform.localPosition = new Vector2(0, LanguageUtility.IsShowBuyInfo ? -550 : _readyUI.transform.localPosition.y);
        readyBox.transform.localPosition = new Vector2(0, LanguageUtility.IsShowBuyInfo ? 515 : 575);
        _spritePlanet.spriteName         = ManagerSpaceTravel.instance.CurrentStage % 3 == 0 ? "ready_title_1" : "ready_title_0";

        lanpageButton.transform.localPosition = new Vector2(lanpageButton.transform.localPosition.x, LanguageUtility.IsShowBuyInfo ? -565f : lanpageButton.transform.localPosition.y);

        SetStageLabel();
    }

    protected override void SetBoniModel()
    {
        var modelNo = ManagerSpaceTravel.instance.ReadyCharIdx;
        _importData = ManagerCharacter._instance.GetLive2DCharacter(modelNo);

        var defaultPosition = new Vector3(221f, LanguageUtility.IsShowBuyInfo ? -290f : -330f, 0);
        var readyScale = new Vector3(-300f, 300f, 300f);
        var readyPosition = defaultPosition;

        if (modelNo > 0)
        {
            readyScale    = _importData.readyScale;
            readyPosition = defaultPosition + _importData.readyPostion;
        }

        boniLive2D = LAppModelProxy.MakeLive2D(mainSprite.gameObject, (TypeCharacterType) modelNo);
        boniLive2D.SetVectorScale(readyScale);
        boniLive2D.SetPosition(readyPosition);
        boniLive2D.SetSortOrder(uiPanel.sortingOrder + 1);
        boniLive2D.SetAnimation("Ready", true);
    }

    protected override void OnClickGoInGame()
    {
        if (!bCanTouch)
        {
            return;
        }

        if (!ManagerSpaceTravel.CheckStartable())
        {
            ManagerUI._instance.OpenPopupEventOver();
            return;
        }

        base.OnClickGoInGame();
    }

    protected override void SetStageLabel()
    {
        foreach (var stageText in stage) stageText.text = Global.GameInstance.GetStageText_ReadyPopup();

        var fontColor = Color.white;
        var lineColor = Color.white;

        var isRewardStage = ManagerSpaceTravel.instance.CurrentStage % 3 == 0;

        ColorUtility.TryParseHtmlString(isRewardStage ? "#FFF770" : "#FFFFFF", out fontColor);
        ColorUtility.TryParseHtmlString(isRewardStage ? "#F28900" : "#2A5D8B", out lineColor);

        stage[0].color       = fontColor;
        stage[0].effectColor = lineColor;
        stage[1].color       = lineColor;
        stage[1].effectColor = lineColor;

        stage[0].MakePixelPerfect();
        stage[1].MakePixelPerfect();
    }

    protected override string GetLive2DString(string key)
    {
        if (ManagerSpaceTravel.instance != null && ManagerSpaceTravel.instance.ReadyCharIdx > 0)
        {
            var eventKey = string.Format(key + "_{0}", ManagerSpaceTravel.instance.ReadyCharIdx);
            if (Global._instance.HasString(eventKey))
            {
                key = eventKey;
            }
        }

        return Global._instance.GetString(key);
    }
    
    public override AudioLobby GetReadyCharacterVoice()
    {
        if (ManagerSpaceTravel.instance.ReadyCharIdx == 0 || ManagerSpaceTravel.instance.ReadyCharIdx == (int) ManagerSpaceTravel.instance.baseCharacterType)
        {
            return _defaultSound;
        }

        return _importData.readySound;
    }
    
    protected override IEnumerator ShowCloverEffect(bool setLayerSort = false) => base.ShowCloverEffect(true);
}