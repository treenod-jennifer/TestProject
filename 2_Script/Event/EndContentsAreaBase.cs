using UnityEngine;

public class EndContentsAreaBase : AreaBase, IEventLobbyObject
{
    [Header("EventLobbyObject")]
    [SerializeField] private string textFileName;
    public ObjectEvent _touchTarget;

    public override bool IsEventArea()
    {
        return true;
    }
    
    public GameEventType GetEventType()
    {
        return GameEventType.END_CONTENTS;
    }

    void Awake()
    {
        InitSceneDatas();
    }

    void Start () {     
        if (_touchTarget != null)
            _touchTarget._onTouch = (() => onTouch());
        
        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }
    }

    void onTouch()
    {
        if (ManagerEndContentsEvent.CheckStartable())
        {
            ManagerUI._instance.OpenPopupReadyEndContents();
        }
        else
        {
            EventLobbyObjectOption.NotEventPopup();
        }
    }

    #region 트리거 체크

    public override void TriggerStart()
    {
        ManagerSound._instance.SetTimeBGM(sceneStartBgmOffset);
        
        if (sceneStartBgmOff)
            ManagerSound._instance?.PauseBGM();

        TriggerStart_Internal();
    }

    public void TriggerSetting()
    {
        if (ManagerEndContentsEvent.CheckPlayStartCutScene() == false)
        {
            if (ManagerEndContentsEvent.instance.Status > 0)
                _listSceneDatas[0].state = TypeSceneState.Active;
            else 
                _listSceneDatas[0].state = TypeSceneState.Wait;

            TriggerStart();
        }
    }

    #endregion

    #region 구조상 필요하나 사용하지 않는 코드
    public void OnActionUIActive(bool in_active)
    {
        //if (_uiEvent != null)
        //    _uiEvent.gameObject.SetActive(in_active);
    }
    
    public void Invalidate()
    {
    }
    #endregion
}
