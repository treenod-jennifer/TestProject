using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 종료
/// </summary>
public class Tutorial_Action_Tutorial_End : Tutorial_Action
{
    public override void StartAction(System.Action endAction = null)
    {   
        StartCoroutine(CoEndTutorial());
        endAction.Invoke();
    }

    private IEnumerator CoEndTutorial()
    {
        //파랑새 나가는 연출.
        ManagerTutorial._instance.SetBirdAnimation("T_idle_turn");

        //파랑새 도는 애니메이션.
        yield return ManagerTutorial._instance.birdLive2D.CoWaitEndAnimation("T_idle_turn");

        //방향 전환 애니메이션 끝나면 파랑새 나감.
        ManagerTutorial._instance.BlueBirdTurn();
        ManagerTutorial._instance.SetBirdAnimation("T_idle_loop", true);
        
        StartCoroutine(ManagerTutorial._instance.OutBlueBird(0.3f));
        yield return new WaitForSeconds(0.3f);

        //오브젝트 제거.
        Destroy(ManagerTutorial._instance.gameObject);
    }
}
