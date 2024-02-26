using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 일반 블라인드 투명도 설정
/// </summary>
public class Tutorial_Action_Blind_SetAlpha : Tutorial_Action
{
    public float startAlpha = 0f;
    public float endAlpha = 1f;
    public float time = 0f;

    public Tutorial_Action_Blind_SetAlpha(float sAlpha, float eAlpha, float time)
    {
        this.startAlpha = sAlpha;
        this.endAlpha = eAlpha;
        this.time = time;
    }

    public void Init(float sAlpha, float eAlpha, float time)
    {
        this.startAlpha = sAlpha;
        this.endAlpha = eAlpha;
        this.time = time;
    }

    public override void StartAction(System.Action endAction = null)
    {
        DOTween.To((a) => ManagerTutorial._instance._current.blind.SetAlpha(a), startAlpha, endAlpha, time);
        endAction.Invoke();
    }
}
