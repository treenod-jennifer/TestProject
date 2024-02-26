using UnityEngine;

public class LuckyRouletteObj : HighlightAreaBase, IEventLobbyObject
{
    [SerializeField] private string textFileName;
    public ObjectEvent _touchTarget;
    
    public override bool IsEventArea()
    {
        return true;
    }
    
    protected override void Awake()
    {
        InitSceneDatas();
        base.Awake();
    }
    
    private void Start () {     
        if (_touchTarget != null)
            _touchTarget._onTouch = (() => OnTouch());

        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }
    }
    
    public GameEventType GetEventType()
    {
        return GameEventType.LUCKY_ROULETTE;
    }

    public void Invalidate()
    {
    }
    
    private void OnTouch()
    {
        if (ManagerLobby._instance.IsLobbyComplete == false)
            return;
        if (ManagerLuckyRoulette.CheckStartable() == false)
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
        }
        else
            ManagerUI._instance.OpenPopupLuckyRoulette();
    }

    protected override void HighlightBubbleSetting()
    {
    }

    public override void EventAddHighlightBubble(bool IsAdd)
    {
    }

    public void TriggerSetting()
    {
        if (ManagerLuckyRoulette.instance == null)
            return;
        if(ManagerLuckyRoulette.CheckStartable())
        {
            if (ManagerLuckyRoulette.instance.IsLobbyObjActive(ServerContents.LuckyRoulette.vsn))
                _listSceneDatas[0].state = TypeSceneState.Active;
        }
    }
    
    //없으면 에러
    public void OnActionUIActive(bool in_active)
    {
    }
}
