using UnityEngine;
using DG.Tweening;

public class UIPopUpReady_BingoEvent : UIPopupReadyBase
{
    [SerializeField] private UISprite backgroundUI;
    [SerializeField] private UISprite sprTitleUI;
    
    public override void InitPopUp(StageMapData stageData)
    {
        base.InitPopUp(stageData);
        
        //국가별 UI 변경
        SetReadyUI();
    }

    void SetReadyUI()
    {
        float bgPosY = LanguageUtility.IsShowBuyInfo ? 484f : 419f;
        float readyPosY = LanguageUtility.IsShowBuyInfo ? -560f : -560f + 55f;

        sprTitleUI.atlas = ManagerBingoEvent.bingoEventResource.bingoEventPack.AtlasOutgame;
        backgroundUI.transform.localPosition = new Vector3(0f, bgPosY, 0f);
        readyBox.transform.localPosition = new Vector3(0f, readyPosY, 0f);
    }

    private Vector3 defultPostion = new Vector3(221f, -290f, 0);
    private ImportData_Live2D.ImportData importData = new ImportData_Live2D.ImportData();
    
    protected override void SetBoniModel()
    {
        int modelNo = (int)ManagerBingoEvent.instance.live2dCharacter;
        importData = ManagerCharacter._instance.GetLive2DCharacter(modelNo);
        
        float posY = LanguageUtility.IsShowBuyInfo ? 0f : -70f;
        defultPostion += new Vector3(0f, posY, 0f);

        boniLive2D = LAppModelProxy.MakeLive2D(mainSprite.gameObject, (TypeCharacterType)modelNo);
        boniLive2D.SetVectorScale(importData.readyScale);
        boniLive2D.SetPosition(defultPostion + importData.readyPostion);
        boniLive2D.SetSortOrder(uiPanel.sortingOrder + 1);
        boniLive2D.SetAnimation("Ready", true);
    }
    
    public void ShowUseClover()
    {
        StartCoroutine(ShowCloverEffect());
    }

    public override AudioLobby GetReadyCharacterVoice()
    {
        return importData.readySound;
    }
    
    protected override string GetLive2DString(string key)
    {
        if (ManagerBingoEvent.instance.live2dCharacter != TypeCharacterType.ANIMAL_046)
        {
            //해당 키가 글로벌에 없을 경우, 기본 대사 출력해야함.
            string eventKey = string.Format(key + "_{0}", (int)ManagerBingoEvent.instance.live2dCharacter);
            if (Global._instance.HasString(eventKey) == true)
                key = eventKey;
        }
        
        return Global._instance.GetString(key);
    }
    
    public override void SettingBoniDialog()
    {
        dialogSequence = DOTween.Sequence();
        dialogBubble.text = GetLive2DString("p_sr_4");
    }
}
