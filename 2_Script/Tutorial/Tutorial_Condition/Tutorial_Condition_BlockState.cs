using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 특정 블럭 상태가 될 때까지 대기하는 조건.
/// </summary>
public class Tutorial_Condition_BlockState : Tutorial_Condition
{
    public BlockManagrState blockState = BlockManagrState.WAIT;

    public void Init(BlockManagrState state)
    {
        this.blockState = state;
    }

    public override IEnumerator StartCondition(System.Action endAction = null)
    {
        yield return new WaitUntil(() => ManagerBlock.instance.state == blockState);
        endAction.Invoke();
    }
}
