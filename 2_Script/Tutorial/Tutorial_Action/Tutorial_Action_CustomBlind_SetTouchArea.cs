using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 커스텀 블라인드 터치 영역 설정
/// </summary>
public class Tutorial_Action_CustomBlind_SetTouchArea : Tutorial_Action
{
    //터치 영역 데이터
    public int key = 0;

    //블라인드 터치 액션
    public CustomMethodData touchMethodData;

    //해당 시간 이후부터 터치영역 활성화가 됨
    public float touchTime = 0f;

    private System.Action touchAction = null;
    private System.Action endAction = null;

    public override ActionType GetActionType()
    {
        return ActionType.DATA_SET;
    }

    public void Awake()
    {
        if (touchMethodData.methodName != "")
            touchAction = System.Delegate.CreateDelegate(typeof(System.Action), touchMethodData.target, touchMethodData.target.GetType().GetMethod(touchMethodData.methodName)) as System.Action;
    }

    public override void StartAction(System.Action endAction = null)
    {  
        this.endAction = endAction;
        StartCoroutine(CoSetTouchArea());
    }

    //일정 시간 대기 후, 터치 영역 설정.
    private IEnumerator CoSetTouchArea()
    {
        yield return new WaitForSeconds(touchTime);
        if (ManagerTutorial._instance._current.dicCustomBlindData.ContainsKey(key) == true)
        {
            List<CustomBlindData> listData = ManagerTutorial._instance._current.dicCustomBlindData[key];
            ManagerTutorial._instance._current.blind.customBlind.SetTouchData(listData, touchAction);
        }
        endAction.Invoke();
    }
}
