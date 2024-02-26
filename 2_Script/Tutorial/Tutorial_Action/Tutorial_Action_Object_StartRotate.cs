using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 오브젝트를 회전
/// </summary>
public class Tutorial_Action_Object_StartRotate : Tutorial_Action
{
    //딕셔너리에서 가져올 오브젝트 키
    public int objectKey = 0;
    //딕셔너리에서 가져올 시퀀스 키
    public int sequenceKey = 0;

    public override void StartAction(System.Action endAction = null)
    {
        List<GameObject> listObj = new List<GameObject>();
        if (ManagerTutorial._instance._current.dicGameObject.ContainsKey(objectKey) == true)
        {
            listObj = ManagerTutorial._instance._current.dicGameObject[objectKey];
        }

        List<Sequence> listSequence = new List<Sequence>();
        for (int i = 0; i < listObj.Count; i++)
        {
            listObj[i].transform.rotation = Quaternion.Euler(0f, 0f, 15f);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(listObj[i].transform.DORotate(new Vector3(0f, 0f, -15f), 0.5f));
            sequence.SetLoops(-1, LoopType.Yoyo);
            listSequence.Add(sequence);
        }
        ManagerTutorial._instance._current.dicTweenSequence.Add(sequenceKey, listSequence);
        endAction.Invoke();
    }
}
