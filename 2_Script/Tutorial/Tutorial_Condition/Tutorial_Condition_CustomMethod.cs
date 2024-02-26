using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직접 설정한 함수 내에서 다음으로 진행하는 지 결정.
/// </summary>
public class Tutorial_Condition_CustomMethod : Tutorial_Condition
{
    public Object target;
    public string method;


    private ManagerTutorial.GetBoolDelegate boolDelegate = null;

    public void Awake()
    {
        boolDelegate = ManagerTutorial.GetBoolDelegate.CreateDelegate(typeof(ManagerTutorial.GetBoolDelegate), target, target.GetType().GetMethod(method)) as ManagerTutorial.GetBoolDelegate;
    }

    public override IEnumerator StartCondition(System.Action endAction)
    {
        yield return new WaitUntil(()=> boolDelegate() == true );
        endAction.Invoke();
    }
}
