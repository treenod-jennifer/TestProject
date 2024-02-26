using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 한 웨이브 끝난 뒤, 사용한 데이터들 리셋 처리(블라인드 터치 영역, 말풍선)
/// </summary>
public class Tutorial_Action_Wave_Reset : Tutorial_Action
{
    public override ActionType GetActionType()
    {
        return ActionType.DATA_REMOVE;
    }

    public override void StartAction(System.Action endAction = null)
    {
        if (ManagerTutorial._instance._current.blind.defaultBlindRoot.activeInHierarchy == true)
        {
            ManagerTutorial._instance._current.blind.SetSizeCollider(0, 0);
        }
        else
        {
            ManagerTutorial._instance._current.blind.customBlind.ResetTouchData();
        }
        StartCoroutine(ManagerTutorial._instance._current.textBox.DestroyTextBox(0.3f));
        endAction.Invoke();
    }
}
