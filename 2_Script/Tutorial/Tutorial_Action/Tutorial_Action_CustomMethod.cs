using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 커스텀 함수 실행
/// </summary>
public class Tutorial_Action_CustomMethod : Tutorial_Action
{
    //실행하고자 하는 액션
    public CustomMethodData customMethodData;

    private System.Action customAction = null;
    private System.Action endAction = null;

    public void Awake()
    {
        if (customMethodData.methodName != "")
            customAction = System.Delegate.CreateDelegate(typeof(System.Action), customMethodData.target, customMethodData.target.GetType().GetMethod(customMethodData.methodName)) as System.Action;
    }

    public override void StartAction(System.Action endAction = null)
    {
        if (customAction != null)
            customAction.Invoke();
        endAction.Invoke();
    }
}
