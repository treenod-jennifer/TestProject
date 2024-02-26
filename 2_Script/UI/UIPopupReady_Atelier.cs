using UnityEngine;

[IsReadyPopup]
public class UIPopupReady_Atelier : UIPopupReadyBase
{
    [SerializeField] private UISprite   _readyUI;
    [SerializeField] private UISprite   _titleUI;
    [SerializeField] private AudioLobby _defaultSound;
    
    private ImportData_Live2D.ImportData _importData      = new ImportData_Live2D.ImportData();
    private Vector3                      _defaultPosition = new Vector3(221f, -290f, 0);

    protected new void OnDestroy()
    {
        _titleUI.atlas = null;
        _instance      = null;
        listTargetInfo.Clear();
        base.OnDestroy();
    }

    public override void InitPopUp(StageMapData stageData)
    {
        base.InitPopUp(stageData);

        _titleUI.atlas                   = ManagerAtelier.instance._atelierPack.AtlasUI;
        _readyUI.pivot                   = UIWidget.Pivot.Bottom;
        _readyUI.height                  = 1000;
        _readyUI.transform.localPosition = new Vector2(0, LanguageUtility.IsShowBuyInfo ? -550 : _readyUI.transform.localPosition.y);
        readyBox.transform.localPosition = new Vector2(0, LanguageUtility.IsShowBuyInfo ? 515 : 575);
        lanpageButton.transform.localPosition =
            new Vector2(lanpageButton.transform.localPosition.x, LanguageUtility.IsShowBuyInfo ? -565f : lanpageButton.transform.localPosition.y);

        SetStageLabel();
    }

    protected override void SetBoniModel()
    {
        var modelNo = ManagerAtelier.instance._live2dIndex;
        _importData = ManagerCharacter._instance.GetLive2DCharacter(modelNo);

        var posY = LanguageUtility.IsShowBuyInfo ? 0f : -40f;
        _defaultPosition += new Vector3(0f, posY, 0f);

        var readyScale    = new Vector3(-300f, 300f, 300f);
        var readyPosition = _defaultPosition;

        if (modelNo > 0)
        {
            readyScale    = _importData.readyScale;
            readyPosition = _defaultPosition + _importData.readyPostion;
        }

        boniLive2D = LAppModelProxy.MakeLive2D(mainSprite.gameObject, (TypeCharacterType)modelNo);
        boniLive2D.SetVectorScale(readyScale);
        boniLive2D.SetPosition(readyPosition);
        boniLive2D.SetSortOrder(uiPanel.sortingOrder + 1);
        boniLive2D.SetAnimation("Ready", true);
    }
    
    public new void ShowUseClover() => StartCoroutine(ShowCloverEffect(true));

    protected override void OnClickGoInGame()
    {
        if (!bCanTouch)
        {
            return;
        }

        if (!ManagerAtelier.CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
            return;
        }

        base.OnClickGoInGame();
    }

    public override AudioLobby GetReadyCharacterVoice()
    {
        if (ManagerAtelier.instance._live2dIndex == 0)
        {
            return _defaultSound;
        }

        return _importData.readySound;
    }
    
    protected override string GetLive2DString(string key)
    {
        if (ManagerAtelier.instance != null && ManagerAtelier.instance._live2dIndex > 0)
        {
            //해당 키가 글로벌에 없을 경우, 기본 대사 출력해야함.
            var eventKey = string.Format(key + "_{0}", ManagerAtelier.instance._live2dIndex);
            if (Global._instance.HasString(eventKey) == true)
            {
                key = eventKey;
            }
        }

        return Global._instance.GetString(key);
    }
}