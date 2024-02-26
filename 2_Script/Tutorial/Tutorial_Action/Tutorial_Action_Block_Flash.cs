using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 블럭 깜빡깜빡
/// </summary>
public class Tutorial_Action_Block_Flash : Tutorial_Action
{
    //찾고자 하는 블럭
    public bool isUseFindBlockData = true;
    public FindBlockData findBlockData = new FindBlockData();

    //블럭 리스트를 반환할 함수 데이터.
    public CustomMethodData methodData;
    public int flashCount = 2;
    public float actionTime = 1f;
    public float waitTime = 0.2f;
    public bool isFlashAlpha = false;

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
            if (isFlashAlpha == false)
                StartCoroutine(listBlock[i].CoFlashBlock_Color(flashCount, actionTime, waitTime));
            else
                StartCoroutine(listBlock[i].CoFlashBlock_Alpha(flashCount, actionTime, waitTime));
        }
        
        endAction.Invoke();
    }
}
