using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public enum TypeActionTriggerEnd
{
    SceneEnd,
    DayEnd,
    BehaviorEnd,
}

public class ActionTriggerEnd : ActionBase
{

    public TypeActionTriggerEnd _endType = TypeActionTriggerEnd.SceneEnd;
    public TriggerStateBehaviorScene _playSmallBehavior = null;

    public override void DoAction()
    {
        //base.DoAction();
        //Debug.Log("ActionTriggerEnd _ DoAction");

        StartCoroutine(CoDoAction());
        
    }

    public IEnumerator CoDoAction()
    {
        yield return new WaitForSeconds(0.3f);

        if (_endType == TypeActionTriggerEnd.SceneEnd)
            ManagerLobby._instance.ChageState(TypeLobbyState.Wait);
        else if (_endType == TypeActionTriggerEnd.DayEnd)
            ManagerLobby._instance.EndDay();
        else if (_endType == TypeActionTriggerEnd.BehaviorEnd)
            LobbyBehavior._instance.EndBehavior();

        if (_playSmallBehavior != null)
            LobbyBehavior._instance.PlayBehavior(_playSmallBehavior);

        yield break;
    }

}
