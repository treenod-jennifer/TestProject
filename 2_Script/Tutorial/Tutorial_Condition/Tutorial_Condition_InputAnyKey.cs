using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 터치 입력이 들어올 떄 까지 대기하는 조건.
/// </summary>
public class Tutorial_Condition_InputAnyKey : Tutorial_Condition
{
    public float waitTime = 0f;

    public void Init(float time)
    {
        this.waitTime = time;
    }

    public override IEnumerator StartCondition(System.Action endAction = null)
    {
        yield return new WaitForSeconds(waitTime);
        yield return new WaitUntil(() => Input.anyKeyDown == true);
        endAction.Invoke();
    }
}
