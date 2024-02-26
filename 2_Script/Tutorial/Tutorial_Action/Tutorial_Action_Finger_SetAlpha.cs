using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 손가락 투명도 설정
/// </summary>
public class Tutorial_Action_Finger_SetAlpha : Tutorial_Action
{
    private List<int> listKey = new List<int>();
    private float startAlpha = 0f;
    private float endAlpha = 1f;
    private float time = 0f;

    public Tutorial_Action_Finger_SetAlpha(float sAlpha, float eAlpha, float time, List<int> listKey = null)
    {
        this.startAlpha = sAlpha;
        this.endAlpha = eAlpha;
        this.time = time;
        if (listKey != null)
            this.listKey = listKey;
    }

    public override void StartAction(System.Action endAction = null)
    {
        if (listKey.Count == 0)
        {
            var enumerator = ManagerTutorial._instance._current.dicFinger.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UITexture texture = enumerator.Current.Value;
                SetAlpha(texture);
            }
        }
        else
        {
            for (int i = 0; i < listKey.Count; i++)
            {
                int key = listKey[i];
                if (ManagerTutorial._instance._current.dicFinger.ContainsKey(key) == true)
                {
                    UITexture texture = ManagerTutorial._instance._current.dicFinger[key];
                    SetAlpha(texture);
                }
            }
        }
        endAction.Invoke();
    }

    private void SetAlpha(UITexture texture)
    {
        texture.color = new Color(1f, 1f, 1f, startAlpha);
        DOTween.ToAlpha(() => texture.color, x => texture.color = x, endAlpha, time);
    }
}
