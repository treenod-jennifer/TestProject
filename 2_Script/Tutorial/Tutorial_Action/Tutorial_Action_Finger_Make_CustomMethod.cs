using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class TutorialFingerData
{
    public int key = 0;
    public Vector3 offset_localPos = Vector3.zero;
    public float rotation = 0f;
    public bool isTouchAction = true;
}

/// <summary>
/// 손가락 생성(유저가 설정한 함수에서 위치값을 가져옴)
/// </summary>
public class Tutorial_Action_Finger_Make_CustomMethod : Tutorial_Action
{
    //손가락 위치값을 설정하는 함수 데이터.
    public CustomMethodData methodData;
    public List<TutorialFingerData> listFingerData = new List<TutorialFingerData>();

    private ManagerTutorial.GetListGameObjectDelegate gameObjectDelegate = null;
    private List<UITexture> listActionFinger = new List<UITexture>();

    public override ActionType GetActionType()
    {
        return ActionType.DATA_SET;
    }

    public void Awake()
    {
        gameObjectDelegate = System.Delegate.CreateDelegate(typeof(ManagerTutorial.GetListGameObjectDelegate), methodData.target, methodData.target.GetType().GetMethod(methodData.methodName)) as ManagerTutorial.GetListGameObjectDelegate;
    }

    public override void StartAction(System.Action endAction = null)
    {
        List<GameObject> listTargetObj = new List<GameObject>(gameObjectDelegate());
        int depth = 100;
        for (int i = 0; i < listFingerData.Count; i++)
        {
            TutorialFingerData data = listFingerData[i];
            GameObject fingerObj = NGUITools.AddChild(this.gameObject, ManagerTutorial._instance._spriteFinger.gameObject);
            UITexture finger = fingerObj.GetComponent<UITexture>();
            if (finger != null)
            {
                finger.depth = depth;
                finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
                finger.transform.position = (listTargetObj.Count > i) ? listTargetObj[i].transform.position : Vector3.zero;
                finger.transform.localPosition += data.offset_localPos;
                finger.transform.rotation = Quaternion.Euler(0f, 0f, data.rotation);
                DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
                ManagerTutorial._instance._current.dicFinger.Add(data.key, finger);
                depth--;

                if (data.isTouchAction == true)
                    listActionFinger.Add(finger);
            }
        }
        StartCoroutine(CoActionFinger());
        endAction.Invoke();
    }

    private IEnumerator CoActionFinger()
    {
        bool bPush = false;
        float fTimer = 0.0f;
        while (fTimer < 0.1f)
        {
            //손가락 이미지 전환.
            fTimer += Global.deltaTime * 1.0f;
            for (int i = 0; i < listActionFinger.Count; i++)
            {
                if (bPush == true)
                {
                    listActionFinger[i].mainTexture = ManagerTutorial._instance._textureFingerNormal;
                    bPush = false;
                }
                else
                {
                    listActionFinger[i].mainTexture = ManagerTutorial._instance._textureFingerPush;
                    bPush = true;
                }
                fTimer = 0.0f;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}
