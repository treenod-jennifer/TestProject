using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Condition_WaitTime : Tutorial_Condition
{
    public float waitTime = 0.0f;

    public void Init(float time)
    {
        this.waitTime = time;
    }

    public override IEnumerator StartCondition(System.Action endAction = null)
    {
        yield return new WaitForSeconds(waitTime);
        endAction.Invoke();
    }
}