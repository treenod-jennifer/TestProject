using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCondition : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Comment", order = 1000)]
    [MultiLineString(100.0f, order = 1000)]
    public string comment = "";
#endif
    [System.NonSerialized]
    public TypeTriggerState _stateType = TypeTriggerState.None;

    public float _delay = 0f;
    public List<ActionBase> _actions = new List<ActionBase>();
    public TriggerCondition _nextCondition = null;
    protected bool _ready = false;

    [System.NonSerialized]
    public bool _end = false;
    float timer = 0f;
    virtual public void Start()
    {
        _end = false;
        _ready = true;
    }
    virtual public bool IsPlaying()
    {

        return false;
    }

    virtual public void Reset()
    {
        Start();
        timer = 0f;
        _end = false;
        for (int i = 0; i < _actions.Count; i++)
        {
            ActionBase  action = _actions[i];
            action.AbortAction();
            action.timer = 0f;
            action.bAction = false;
            action.bActionFinish = false;
        }
    }

    // Update is called once per frame
    virtual public void Update()
    {
        if (!_ready)
            return;

        // 컨디션 딜레만큼 기다렸다 액션들 딜레이만큼 기다렸다 각각 실행
        if (timer >= _delay)
        {
            //Debug.Log("dddddddd " + timer + "     " + _actions.Count);
            bool endCondition = true;

            for (int i = 0; i < _actions.Count; i++)
            {
                ActionBase  action = _actions[i];
                if (action != null)
                {
                    if (!action.bAction)
                    {
                        if (action.timer >= action._delay && action.ActionStartPrecheck() )
                        {
                            //Debug.LogFormat("DoAction({0}) : {1} ", Time.frameCount, action.GetType().ToString());

                            action._stateType = _stateType;
                            action.DoAction();
                            action.bAction = true;
                        }
                        action.timer += Global.deltaTimeLobby;
                        endCondition = false;
                    }
                    else
                    {
                        if (action.bWaitActionFinishOnOff)
                            if (!action.bActionFinish)
                                endCondition = false;
                    }
                }
            }
            
            if (endCondition)
            {
                //Debug.LogFormat("EndCondition({0})", Time.frameCount);
                ManagerLobby._instance.FlushRemoveReservedCharacters();

                _end = true;
                if (_nextCondition != null)
                {
                    if (!_nextCondition.gameObject.active)
                    {
                        //Debug.Log("On   " + _nextCondition.name);
                        _nextCondition.gameObject.SetActive(true);
                    }
                }

                // 마지막 컨디션 끝
                if (_nextCondition == null)
                {

                }

                enabled = false;
            }
        }

        timer += Global.deltaTimeLobby;
    }
}
