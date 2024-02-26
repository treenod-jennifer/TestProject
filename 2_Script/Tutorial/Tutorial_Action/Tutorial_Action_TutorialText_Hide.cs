using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 말풍선 제거
/// </summary>
public class Tutorial_Action_TutorialText_Hide : Tutorial_Action
{
    public override void StartAction(System.Action endAction = null)
    {
        StartCoroutine(ManagerTutorial._instance._current.textBox.DestroyTextBox(0.3f));
        endAction.Invoke();
    }
}
