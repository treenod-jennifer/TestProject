using System.Collections;
using UnityEngine;

public class TurnRelayEventAreaBase : HighlightAreaBase, IEventLobbyObject
{
    [Header("EventLobbyObject")]
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _jpStringData = null;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _exStringData = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _jpFileName;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _exFileName;

    [SerializeField] private string textFileName;

    public ObjectEvent _touchTarget;

    public override bool IsEventArea()
    {
        return true;
    }

    override protected void Awake()
    {
        InitSceneDatas();

        ManagerArea._instance.RegisterEventLobbyObject(this);

        base.Awake();
    }

    public void OnActionUIActive(bool in_active)
    {
        //if (_uiEvent != null)
        //    _uiEvent.gameObject.SetActive(in_active);
    }

	// Use this for initialization
	void Start () {     
        if (_touchTarget != null)
            _touchTarget._onTouch = (() => onTouch());

        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }
    }

    public override void EventAddHighlightBubble(bool IsAdd)
    {
        if(IsAdd)
            UIPopupTurnRelay_StageReady.playCallBack += HighlightBubbleSetting;
        else
            UIPopupTurnRelay_StageReady.playCallBack -= HighlightBubbleSetting;
    }

    protected override void HighlightBubbleSetting()
    {
        StartCoroutine(CoActiveBubble());
    }

    IEnumerator CoActiveBubble()
    {
        if(ManagerTurnRelay.GetEventState() != ManagerTurnRelay.EventState.RUNNING
            && ManagerTurnRelay.GetEventState() != ManagerTurnRelay.EventState.NEED_CREATE_PROFILE)
        {
            bubbleRoot.gameObject.SetActive(false);
            yield break;
        }

        if (ManagerTurnRelay.instance.EventAP > 0)
        {
            bubbleRoot.gameObject.SetActive(true);
            StartCoroutine(bubble.CoBubbleImageChange(objImages));
            yield break;
        }

        long remainingTime = Global.LeftTime(ManagerTurnRelay.instance.APRechargeTime);

        bubbleRoot.gameObject.SetActive(false);
        StopCoroutine(bubble.CoBubbleImageChange(objImages));

        while (remainingTime > 0)
        {
            remainingTime--;
            yield return new WaitForSeconds(1.0f);
        }
        
        if(ManagerTurnRelay.GetEventState()    != ManagerTurnRelay.EventState.RUNNING
           && ManagerTurnRelay.GetEventState() != ManagerTurnRelay.EventState.NEED_CREATE_PROFILE)
        {
            yield break;
        }

        bubbleRoot.gameObject.SetActive(true);
        StartCoroutine(bubble.CoBubbleImageChange(objImages));
    }

    void onTouch()
    {
        if (ManagerLobby._instance.IsLobbyComplete == false)
        {
            return;
        }
        
        var eventState = ManagerTurnRelay.GetEventState();

        bool participated = false;
        if(ServerRepos.UserTurnRelayEvent != null && ServerRepos.UserTurnRelayEvent.play >= 1)
            participated = true;

        //이벤트가 오픈되지 않은 경우는 반응 없음
        if (eventState == ManagerTurnRelay.EventState.NOT_STARTED ||
            (eventState > ManagerTurnRelay.EventState.RUNNING && participated == false) )
        {
            return;
        }

        //이벤트 오픈 시 처리
        ManagerTurnRelay.OpenTurnRelay();
    }

    public GameEventType GetEventType()
    {
        return GameEventType.TURN_RELAY;
    }

    public void Invalidate()
    {
    }

    public void TriggerSetting()
    {
        if (ManagerTurnRelay.IsActiveEvent())
        {
            int shopResVer = ServerContents.TurnRelayEvent.eventIndex;
            var eventState = ManagerTurnRelay.GetEventState();

            bool participated = false;
            if (ServerRepos.UserTurnRelayEvent != null && ServerRepos.UserTurnRelayEvent.play >= 1)
                participated = true;

            bool found = false;
            if (PlayerPrefs.HasKey(ManagerTurnRelay.TurnRelayResource.TURNRELAY_CUTSCENE_PLAYED))
            {
                int played = PlayerPrefs.GetInt(ManagerTurnRelay.TurnRelayResource.TURNRELAY_CUTSCENE_PLAYED);
                found = shopResVer == played;
            }

            if(ServerRepos.UserTurnRelayEvent?.play > 0)
            {
                found = true;
            }

            //이벤트가 오픈되지 않은 경우는 반응 없음
            if (eventState == ManagerTurnRelay.EventState.NOT_STARTED ||
                (eventState > ManagerTurnRelay.EventState.RUNNING && participated == false))
                found = false;

            if (found)
                _listSceneDatas[0].state = TypeSceneState.Active;
        }
    }
}
