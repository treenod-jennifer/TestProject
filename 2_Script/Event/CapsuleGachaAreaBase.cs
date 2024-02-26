using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleGachaAreaBase : HighlightAreaBase, IEventLobbyObject
{
    public static CapsuleGachaAreaBase _instance = null;

    [Header("EventLobbyObject")]
    [SerializeField] private string textFileName;

    public ObjectEvent _touchTarget;

    public override bool IsEventArea()
    {
        return true;
    }

    override protected void Awake()
    {
        _instance = this;

        InitSceneDatas();
        base.Awake();
    }

    override protected void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        base.OnDestroy();
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
        
        string textName = string.Format(this.name +"_" + 1);

        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }
    }

    public override void EventAddHighlightBubble(bool IsAdd)
    {
        if(IsAdd)
            ServerRepos.eventTokenUpsert += HighlightBubbleSetting;
        else
            ServerRepos.eventTokenUpsert -= HighlightBubbleSetting;
    }

    protected override void HighlightBubbleSetting()
    {
        if (CapsuleGachaAreaBase._instance == null) return;

        bool isActiveBubble = false;

        //On Off
        if (ManagerCapsuleGachaEvent.CheckStartable())//이벤트 체크
        {
            if (ManagerCapsuleGachaEvent.IsFreeGacha())//무료 가차일 때는 무조건 생성
            {
                isActiveBubble = true;
            }
            else
            {
                if (ServerRepos.UserCapsuleGacha?.playCount > 0
                    && ManagerCapsuleGachaEvent.IsNoEnoughCapsuleToyToken(1) == false)
                {
                    isActiveBubble = true;
                }
            }
        }

        if (isActiveBubble)
        {
            bubbleRoot.gameObject.SetActive(true);
            StartCoroutine(bubble.CoBubbleImageChange(objImages));
        }
        else
        {
            bubbleRoot.gameObject.SetActive(false);
        }
    }

    void onTouch()
    {
        if (ManagerCapsuleGachaEvent.CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopupCapsuleGacha>();
        }
        else
        {
            EventLobbyObjectOption.NotEventPopup();
        }
    }

    public GameEventType GetEventType()
    {
        return GameEventType.CAPSULE_GACHA;
    }

    public void Invalidate()
    {
    }

    public void TriggerSetting()
    {
        if (ManagerCapsuleGachaEvent.CheckStartable())
        {
            if (ManagerCapsuleGachaEvent.CheckPlayStartCutScene())
            {
                _listSceneDatas[0].state = TypeSceneState.Active;
            }
        }
    }

}
