using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStateFinish : TriggerState
{

    void Awake()
    {
        _type = TypeTriggerState.Finish;
        for (int i = 0; i < _conditions.Count; i++)
            _conditions[i]._stateType = _type;
    }
}
