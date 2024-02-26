using UnityEngine;

[IsReadyPopup]
public class UIPopupReady_TreasureHunt : UIPopupReadyBase
{
    [SerializeField] private UISprite readyUI;
    [SerializeField] private UISprite titleUI;
    [SerializeField] private TextMeshGlobalString tmpTitle;
    [SerializeField] private AudioLobby defaultSound;
    
    private ImportData_Live2D.ImportData importData = new ImportData_Live2D.ImportData();
    private Vector3 defaultPosition = new Vector3(221f, -290f, 0);

    public override void InitPopUp(StageMapData stageData)
    {
        base.InitPopUp(stageData);

        titleUI.atlas = ManagerTreasureHunt.instance.treasureHuntPack.UIAtlas;
        readyUI.pivot = UIWidget.Pivot.Bottom;
        readyUI.height = 1000;
        readyUI.transform.localPosition = new Vector2(0, LanguageUtility.IsShowBuyInfo ? -550 : readyUI.transform.localPosition.y);
        readyBox.transform.localPosition = new Vector2(0, LanguageUtility.IsShowBuyInfo ? 515 : 575);
        lanpageButton.transform.localPosition = new Vector2(lanpageButton.transform.localPosition.x, LanguageUtility.IsShowBuyInfo ? -565f : lanpageButton.transform.localPosition.y);
        
        SetStageLabel();
        SetCoinEvent();
    }

    protected override void SetStageLabel()
    {
        string text = Global.GameInstance.GetStageText_ReadyPopup();
        tmpTitle.SetText(text);
    }

    protected override void SetBoniModel()
    {
        int modelNo = ManagerTreasureHunt.instance.ReadyCharIdx > 0 ? ManagerTreasureHunt.instance.ReadyCharIdx : (int)TypeCharacterType.Boni;
        importData = ManagerCharacter._instance.GetLive2DCharacter(modelNo);
        
        float posY = LanguageUtility.IsShowBuyInfo ? 0f : -40f;
        defaultPosition += new Vector3(0f, posY, 0f);

        var readyScale = new Vector3(-300f, 300f, 300f);
        var readyPosition = defaultPosition;
        
        if (modelNo > 0)
        {
            readyScale = importData.readyScale;
            readyPosition = defaultPosition + importData.readyPostion;
        }
        
        boniLive2D = LAppModelProxy.MakeLive2D(mainSprite.gameObject, (TypeCharacterType)modelNo);
        boniLive2D.SetVectorScale(readyScale);
        boniLive2D.SetPosition(readyPosition);
        boniLive2D.SetSortOrder(uiPanel.sortingOrder + 1);
        boniLive2D.SetAnimation("Ready", true);
    }
    
    protected override void OnClickGoInGame()
    {
        if (!bCanTouch) return;
        
        if (!ManagerTreasureHunt.CheckStartable())
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
        if (ManagerTreasureHunt.instance.ReadyCharIdx == 0)
            return defaultSound;
        return importData.readySound;
    }
    
    protected override string GetLive2DString(string key)
    {
        if (ManagerTreasureHunt.instance != null && ManagerTreasureHunt.instance.ReadyCharIdx > 0)
        {
            //해당 키가 글로벌에 없을 경우, 기본 대사 출력해야함.
            string eventKey = string.Format(key + "_{0}", ManagerTreasureHunt.instance.ReadyCharIdx);
            if (Global._instance.HasString(eventKey) == true)
                key = eventKey;
        }
        
        return Global._instance.GetString(key);
    }
}
