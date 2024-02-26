using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoleCatchEvent : AreaBase, IEventLobbyObject
{
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

    void Awake()
    {
        InitSceneDatas();
    }

    public void OnActionUIActive(bool in_active)
    {
        //if (_uiEvent != null)
        //    _uiEvent.gameObject.SetActive(in_active);
    }

    public override void TriggerStart()
    {
        ManagerSound._instance.SetTimeBGM(sceneStartBgmOffset);
        
        if (sceneStartBgmOff)
            ManagerSound._instance?.PauseBGM();

        TriggerStart_Internal();
    }

	// Use this for initialization
	void Start () {     
        if (_touchTarget != null)
            _touchTarget._onTouch = (() => onTouch());
        
        string textName = string.Format(this.name +"_" + ManagerMoleCatch.GetResourceIndex());

        if (!string.IsNullOrEmpty(textFileName))
        {
            StringHelper.LoadStringFromCDN(textFileName, Global._instance._stringData);
        }
    }

    void onTouch()
    {
        ManagerUI._instance.OpenPopupMoleCatch();
    }

    public GameEventType GetEventType()
    {
        return GameEventType.MOLE_CATCH;
    }

    public void Invalidate()
    {
    }

    public void TriggerSetting()
    {
        if (ManagerMoleCatch.CheckPlayStartCutScene() == false)
            _listSceneDatas[0].state = TypeSceneState.Active;
    }
}
