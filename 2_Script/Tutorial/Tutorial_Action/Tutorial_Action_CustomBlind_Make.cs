using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 커스텀 블라인드 생성
/// </summary>
public class Tutorial_Action_CustomBlind_Make : Tutorial_Action
{
    public int width = 2000;
    public int height = 3000;
    public int textureSize = 512;

    public override ActionType GetActionType()
    {
        return ActionType.DATA_SET;
    }

    public override void StartAction(System.Action endAction = null)
    {
        ManagerTutorial._instance._current.blind.MakeCustomBlindTexture(width, height, textureSize);
        endAction.Invoke();
    }
}
