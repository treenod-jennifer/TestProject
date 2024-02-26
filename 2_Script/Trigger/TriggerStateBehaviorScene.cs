using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TypeBehaviorMode
{
    None,
    MissionWait,
    MissionActive,
    MissionFinish,

}

public class TriggerStateBehaviorScene : TriggerState
{
    //public TypeBehaviorMode _mode = TypeBehaviorMode.None;
    public bool _onWait = false;
    public bool _onActive = false;
    public bool _onFinish = false;


    public int _skipMissionStartIndex = 0;
    public float _spawnRatio = 1f;
    public List<ObjectBase> _touchObject = new List<ObjectBase>();

    [System.NonSerialized]
    public int _missionIndex = 0;

    [SerializeField]
    int _missionIndex_allowTapMin = 0;
    [SerializeField]
    int _missionIndex_allowTapMax = 0;

    public float activeSize = 20f;

    void Awake()
    {
        _type = TypeTriggerState.BehaviorScene;
        for (int i = 0; i < _conditions.Count; i++)
        {
            _conditions[i]._stateType = _type;
            _conditions[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < _touchObject.Count; i++)
            _touchObject[i]._callbackTapList.Add(this);

        // 터치메세지형이면 자동 비해비어 패스
        //if (_missionIndex > 0 && _touchObject.Count == 0)
        if (_touchObject.Count == 0)
        {
            if (LobbyBehavior._instance != null)
                LobbyBehavior._instance._behaviorScene.Add(this);
        }
    }
	

    public override void OnTap(ObjectBase in_obj )
    {
        bool play = false;
        //Debug.Log(_missionIndex + "  " + in_obj.name);
        if (_onWait == false && _onActive == false && _onFinish == false)
            return;

        if (_missionIndex > 0)
        {
            if (ManagerData._instance._missionData[_missionIndex].state == TypeMissionState.Inactive)
                if (_onWait)
                    play = true;

            if (ManagerData._instance._missionData[_missionIndex].state == TypeMissionState.Clear)
                if (_onFinish)
                    play = true;

            if (ManagerData._instance._missionData[_missionIndex].state == TypeMissionState.Active)
                if (_onActive)
                    play = true;
        }

        if (_missionIndex==0)
            play = true;

        if (play)
        {
            LobbyBehavior._instance.CancleBehavior();

            LobbyBehavior._instance.PlayExternalBehavior(this);
            //Reset();
            //gameObject.SetActive(true);
            //StartCondition();
        }

        
       // Debug.Log(gameObject.name);
    }

    public override bool CanExecuteTapAction()
    {
        if(_missionIndex_allowTapMin == _missionIndex_allowTapMax && _missionIndex_allowTapMin == 0)
        {
            return true;
        }

        if (_missionIndex_allowTapMin != 0 && ManagerData._instance._missionData[_missionIndex_allowTapMin].state != TypeMissionState.Clear)
            return false;

        if (_missionIndex_allowTapMax != 0 && ManagerData._instance._missionData[_missionIndex_allowTapMax].state == TypeMissionState.Clear)
            return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, activeSize);
    }
}
