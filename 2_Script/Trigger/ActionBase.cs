using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBase : MonoBehaviour
{   
    public float _delay = 0f;

    [System.NonSerialized]
    public TypeTriggerState _stateType = TypeTriggerState.None;
    [System.NonSerialized]
    public bool bAction = false;
    [System.NonSerialized]
    public bool bActionFinish = false;

    [System.NonSerialized]
    public float timer = 0f;
    [System.NonSerialized]
    public bool bWaitActionFinishOnOff = false;
 

    public virtual void DoAction() { }

    public virtual void AbortAction() { }
    
}
