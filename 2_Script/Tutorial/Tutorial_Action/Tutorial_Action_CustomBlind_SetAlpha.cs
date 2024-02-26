using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 커스텀 블라인드 투명도 설정
/// </summary>
public class Tutorial_Action_CustomBlind_SetAlpha : Tutorial_Action
{
    public float startAlpha = 0f;
    public float endAlpha = 1f;
    public float time = 0f;

    public override void StartAction(System.Action endAction = null)
    {
        DOTween.To((a) => ManagerTutorial._instance._current.blind.customBlind.SetAlphaCustomBlind(a), startAlpha, endAlpha, time);
        endAction.Invoke();
    }
}
