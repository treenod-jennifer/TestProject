using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Action_Deco_Flash : Tutorial_Action
{
    //데코 리스트를 반환할 함수 데이터.
    public CustomMethodData methodData;
    public int flashCount = 2;
    public float actionTime = 1f;
    public float waitTime = 0.2f;
    public bool isFlashAlpha = false;

    private ManagerTutorial.GetListDecoBaseDelegate decoBaseDelegate = null;

    public void Awake()
    {
        if (methodData.target != null)
        {
            decoBaseDelegate = System.Delegate.CreateDelegate(typeof(ManagerTutorial.GetListDecoBaseDelegate), methodData.target, methodData.target.GetType().GetMethod(methodData.methodName)) as ManagerTutorial.GetListDecoBaseDelegate;
        }
    }

    public override void StartAction(System.Action endAction = null)
    {
        List<DecoBase> listDeco = new List<DecoBase>();

        #region 데코 데이터 등록
        if (decoBaseDelegate != null)
        {
            listDeco = new List<DecoBase>(decoBaseDelegate());
        }
        #endregion

        //블럭 연출
        for (int i = 0; i < listDeco.Count; i++)
        {
            if (isFlashAlpha == false)
                StartCoroutine(listDeco[i].CoFlashDeco_Color(flashCount, actionTime, waitTime));
            else
                StartCoroutine(listDeco[i].CoFlashDeco_Alpha(flashCount, actionTime, waitTime));
        }

        endAction.Invoke();
    }
}
