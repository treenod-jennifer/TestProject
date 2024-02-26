using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TriggerStateBehaviorArea : TriggerState
{
#if UNITY_EDITOR
    [Header("Comment", order = 1000)]
    [MultiLineString(100.0f, order = 1000)]
    public string comment = "";
#endif
    public int startMissionIndex = 0;
    public int endMissionIndex = 0;
    public float activeSize = 20f;

    void Awake()
    {
        _type = TypeTriggerState.BehaviorArea;
        for (int i = 0; i < _conditions.Count; i++)
        {
            _conditions[i]._stateType = _type;
            _conditions[i].gameObject.SetActive(false);
        }

        if (LobbyBehavior._instance != null)
            LobbyBehavior._instance._behaviorArea.Add(this);
        gameObject.SetActive(false);


        //_type = TypeTriggerState.Wait;
        //for (int i = 0; i < _conditions.Count; i++)
          //  _conditions[i]._stateType = _type;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, activeSize);
    }
}
