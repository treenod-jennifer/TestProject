using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 손가락 이동
/// </summary>
public class Tutorial_Action_Finger_Move : Tutorial_Action
{
    private List<int> listKey = new List<int>();
    private List<Vector3> listTargetPos = new List<Vector3>();
    private float time = 0f;

    public Tutorial_Action_Finger_Move(List<Vector3> listPos, float time, List<int> listKey = null)
    {
        this.listTargetPos = listPos;
        this.time = time;
        if (listKey != null)
            this.listKey = listKey;
    }

    public override void StartAction(System.Action endAction = null)
    {
        if (listKey.Count == 0)
        {
            int posIdx = 0;
            var enumerator = ManagerTutorial._instance._current.dicFinger.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.transform.DOMove(listTargetPos[posIdx], time);
                posIdx++;
            }
        }
        else
        {
            for (int i = 0; i < listKey.Count; i++)
            {
                int key = listKey[i];
                if (ManagerTutorial._instance._current.dicFinger.ContainsKey(key) == true)
                {
                    ManagerTutorial._instance._current.dicFinger[key].transform.DOMove(listTargetPos[i], time);
                }
            }
        }
        endAction.Invoke();
    }
}
