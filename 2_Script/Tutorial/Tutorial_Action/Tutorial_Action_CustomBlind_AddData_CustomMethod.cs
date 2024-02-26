using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 커스텀 블라인드 데이터 추가(블라인드 영역 데이터를 들고오는 함수로)
/// </summary>
public class Tutorial_Action_CustomBlind_AddData_CustomMethod : Tutorial_Action
{
    //블라인드 데이터를 딕셔너리에 저장할 때 사용할 키 값
    public int key = 0;
    
    //데이터 가지고 오는 함수
    public CustomMethodData methodData;

    private ManagerTutorial.GetCustomBlindDataDelegate blindDataDelegate = null;

    public override ActionType GetActionType()
    {
        return ActionType.DATA_SET;
    }

    public void Awake()
    {
        if (methodData.methodName != "")
            blindDataDelegate = System.Delegate.CreateDelegate(typeof(ManagerTutorial.GetCustomBlindDataDelegate), methodData.target, methodData.target.GetType().GetMethod(methodData.methodName)) as ManagerTutorial.GetCustomBlindDataDelegate;
    }

    public override void StartAction(System.Action endAction = null)
    {
        if (ManagerTutorial._instance._current.dicCustomBlindData.ContainsKey(key) == true)
            ManagerTutorial._instance._current.dicCustomBlindData[key] = new List<CustomBlindData>(blindDataDelegate());
        else
            ManagerTutorial._instance._current.dicCustomBlindData.Add(key, new List<CustomBlindData>(blindDataDelegate()));
        endAction.Invoke();
    }
}
