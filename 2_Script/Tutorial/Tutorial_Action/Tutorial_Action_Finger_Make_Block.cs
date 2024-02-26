using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 손가락 생성(블럭 위치로 손가락 위치 설정)
/// </summary>
public class Tutorial_Action_Finger_Make_Block : Tutorial_Action
{
    [System.Serializable]
    public class TutorialFingerData_Block_Index
    {
        public int indexX = 0;
        public int indexY = 0;
        public TutorialFingerData fingerData = new TutorialFingerData();
    }
    //손가락 데이터
    public List<TutorialFingerData_Block_Index> listFingerData_Index = new List<TutorialFingerData_Block_Index>();

    [System.Serializable]
    public class TutorialFingerData_Block_BlockType
    {
        public FindBlockData findBlockData;
        public TutorialFingerData fingerData = new TutorialFingerData();
    }
    //손가락 데이터
    public List<TutorialFingerData_Block_BlockType> listFingerData_BlockType = new List<TutorialFingerData_Block_BlockType>();

    private List<UITexture> listActionFinger = new List<UITexture>();
    private int depth = 100;

    public override ActionType GetActionType()
    {
        return ActionType.DATA_SET;
    }

    public override void StartAction(System.Action endAction = null)
    {
        #region 인덱스 데이터로 손가락 생성
        for (int i = 0; i < listFingerData_Index.Count; i++)
        {
            TutorialFingerData fingerData = listFingerData_Index[i].fingerData;
            BlockBase tempBlock = PosHelper.GetBlock(listFingerData_Index[i].indexX, listFingerData_Index[i].indexY);
            MakeFinger(tempBlock, fingerData);
        }
        #endregion

        #region 블럭 데이터로 손가락 생성
        for (int i = 0; i < listFingerData_BlockType.Count; i++)
        {
            TutorialFingerData fingerData = listFingerData_BlockType[i].fingerData;
            List<BlockBase> listBlock = new List<BlockBase>(listFingerData_BlockType[i].findBlockData.GetMatchableBlockList());
            if (listBlock.Count > 0)
                MakeFinger(listBlock[0], fingerData);
        }
        #endregion
        
        StartCoroutine(CoActionFinger());
        endAction.Invoke();
    }

    private void MakeFinger(BlockBase tempBlock, TutorialFingerData fingerData)
    {
        if (tempBlock == null)
            return;

        GameObject fingerObj = NGUITools.AddChild(this.gameObject, ManagerTutorial._instance._spriteFinger.gameObject);
        UITexture finger = fingerObj.GetComponent<UITexture>();
        if (finger != null)
        {
            finger.depth = depth;
            finger.mainTexture = ManagerTutorial._instance._textureFingerNormal;
            finger.transform.position = (tempBlock == null) ? Vector3.zero : tempBlock.transform.position;
            finger.transform.localPosition += fingerData.offset_localPos;
            finger.transform.rotation = Quaternion.Euler(0f, 0f, fingerData.rotation);
            DOTween.ToAlpha(() => finger.color, x => finger.color = x, 1f, 0.2f);
            ManagerTutorial._instance._current.dicFinger.Add(fingerData.key, finger);
            depth--;

            if (fingerData.isTouchAction == true)
                listActionFinger.Add(finger);
        }
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
