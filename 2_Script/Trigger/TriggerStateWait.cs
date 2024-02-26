using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStateWait : TriggerState
{

    void Awake()
    {
        _type = TypeTriggerState.Wait;
        for (int i = 0; i < _conditions.Count; i++)
            _conditions[i]._stateType = _type;
    }
}
