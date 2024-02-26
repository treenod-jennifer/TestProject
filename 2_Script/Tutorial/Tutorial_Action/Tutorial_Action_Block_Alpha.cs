using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 블럭 투명화
/// </summary>
public class Tutorial_Action_Block_Alpha : Tutorial_Action
{
    //찾고자 하는 블럭
    public bool isUseFindBlockData = true;
    public FindBlockData findBlockData = new FindBlockData();

    //블럭 리스트를 반환할 함수 데이터.
    public CustomMethodData methodData;
    public float actionTime = 1f;
    public float fromAlpha = 1f;
    public float toAlpha = 0.2f;

    private ManagerTutorial.GetListBlockBaseDelegate blockBaseDelegate = null;
    private List<BlockBase> listBlock = new List<BlockBase>();

    public void Awake()
    {
        if (methodData.target != null)
        {
            blockBaseDelegate = System.Delegate.CreateDelegate(typeof(ManagerTutorial.GetListBlockBaseDelegate), methodData.target, methodData.target.GetType().GetMethod(methodData.methodName)) as ManagerTutorial.GetListBlockBaseDelegate;
        }
    }

    public override void StartAction(System.Action endAction = null)
    {
        #region 블럭 데이터 등록
        listBlock.Clear();
        if (findBlockData != null && isUseFindBlockData == true)
        {
            listBlock.AddRange(findBlockData.GetMatchableBlockList());
        }

        if (blockBaseDelegate != null)
        {
            listBlock.AddRange(blockBaseDelegate());
        }
        #endregion

        //블럭 연출
        for (int i = 0; i < listBlock.Count; i++)
        {
            StartCoroutine(listBlock[i].CoSetBlockAlpha(fromAlpha, toAlpha, actionTime));
        }
        
        endAction.Invoke();
    }
}
