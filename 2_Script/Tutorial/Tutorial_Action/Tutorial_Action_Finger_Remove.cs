using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 손가락 제거
/// </summary>
public class Tutorial_Action_Finger_Remove : Tutorial_Action
{
    public List<int> listKey = new List<int>();

    public override ActionType GetActionType()
    {
        return ActionType.DATA_REMOVE;
    }

    public override void StartAction(System.Action endAction = null)
    {
        StartCoroutine(CoRemoveFinger());
        endAction.Invoke();
    }

    private IEnumerator CoRemoveFinger()
    {
        int removeCount = listKey.Count;
        int endCount = 0;
        if (listKey.Count == 0)
        {
            removeCount = ManagerTutorial._instance._current.dicFinger.Count;
            var enumerator = ManagerTutorial._instance._current.dicFinger.GetEnumerator();
            while (enumerator.MoveNext())
            {
                UITexture texture = enumerator.Current.Value;
                StartCoroutine(CoSetAlpha(texture, () => endCount++));
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
                    StartCoroutine(CoSetAlpha(texture, () => endCount++));
                }
            }
        }
        yield return new WaitUntil(() => (endCount >= removeCount));
        RemoveFinger();
    }

    private IEnumerator CoSetAlpha(UITexture texture, System.Action action)
    {
        DOTween.ToAlpha(() => texture.color, x => texture.color = x, 0, 0.2f);
        yield return new WaitForSeconds(0.2f);
        action.Invoke();
    }

    private void RemoveFinger()
    {
        if (listKey.Count == 0)
        {
            var enumerator = ManagerTutorial._instance._current.dicFinger.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Destroy(enumerator.Current.Value.gameObject);
            }
            ManagerTutorial._instance._current.dicFinger.Clear();
        }
        else
        {
            for (int i = 0; i < listKey.Count; i++)
            {
                int key = listKey[i];
                if (ManagerTutorial._instance._current.dicFinger.ContainsKey(key) == true)
                {
                    Destroy(ManagerTutorial._instance._current.dicFinger[key].gameObject);
                    ManagerTutorial._instance._current.dicFinger.Remove(key);
                }
            }
        }
    }
}
