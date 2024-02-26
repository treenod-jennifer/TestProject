using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일반 블라인드 활성화 여부
/// </summary>
public class Tutorial_Action_Blind_SetActive : Tutorial_Action
{
    public bool isActive = true;
    public int depth = 10;

    public Tutorial_Action_Blind_SetActive(bool active, int depth)
    {
        this.isActive = active;
        this.depth = depth;
    }

    public override void StartAction(System.Action endAction = null)
    {
        if (depth == 0) depth = 10;
        ManagerTutorial._instance._current.blind.SetActiveDefulatBlind(isActive, depth);
        endAction.Invoke();
    }
}
