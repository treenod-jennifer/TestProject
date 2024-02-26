using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class UIItemMoleCatch : MonoBehaviour
{
    public SkeletonAnimation spineMole;
    public UITexture clearIcon;
    public BoxCollider moleCollider;
    public int stageIndex = -1;

    //두더지 터치했을 때, 실행될 함수.
    private System.Action<int> onClickAction = null;

    public void InitItemMoleCatch(System.Action<int> action)
    {
        onClickAction = action;
    }

    private void OnClickBtnMole()
    {
        if(onClickAction != null)
            onClickAction(stageIndex);
    }
}
