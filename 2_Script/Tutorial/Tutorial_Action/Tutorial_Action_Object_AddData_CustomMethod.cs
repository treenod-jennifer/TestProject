using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Action_Object_AddData_CustomMethod : Tutorial_Action
{
    //딕셔너리에 오브젝트를 저장할 키
    public int key = 0;
    //오브젝트 리스트를 가져올 함수
    public CustomMethodData methodData;

    private ManagerTutorial.GetListGameObjectDelegate gameObjectsDelegate = null;

    public override ActionType GetActionType()
    {
        return ActionType.DATA_SET;
    }

    public void Awake()
    {
        if (methodData.methodName != "")
            gameObjectsDelegate = System.Delegate.CreateDelegate(typeof(ManagerTutorial.GetListGameObjectDelegate), methodData.target, methodData.target.GetType().GetMethod(methodData.methodName)) as ManagerTutorial.GetListGameObjectDelegate;
    }

    public override void StartAction(System.Action endAction = null)
    {
        if (ManagerTutorial._instance._current.dicGameObject.ContainsKey(key) == true)
            ManagerTutorial._instance._current.dicGameObject[key] = new List<GameObject>(gameObjectsDelegate());
        else
            ManagerTutorial._instance._current.dicGameObject.Add(key, gameObjectsDelegate());
        endAction.Invoke();
    }
}
