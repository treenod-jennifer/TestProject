using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionWaitEventObjectLink : ActionBase
{

    public ObjectEvent _waitObject = null;
    public int _eventType = 1;

    

    public override void DoAction()
    {
        if (ManagerArea._instance != null && _waitObject != null)
            ManagerArea._instance._eventWaitObject.Add(_eventType, _waitObject);
    }
}
