using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 오브젝트들 회전 정지
/// </summary>
public class Tutorial_Action_Object_StopRotate : Tutorial_Action
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

        if (ManagerTutorial._instance._current.dicTweenSequence.ContainsKey(sequenceKey) == false)
            return;

        List<Sequence> listSequence = ManagerTutorial._instance._current.dicTweenSequence[sequenceKey];
        for (int i = 0; i < listSequence.Count; i++)
        {
            listSequence[i].Kill();
        }
        ManagerTutorial._instance._current.dicTweenSequence.Remove(sequenceKey);

        for (int i = 0; i < listObj.Count; i++)
        {
            listObj[i].transform.rotation = Quaternion.identity;
        }
        endAction.Invoke();
    }
}
