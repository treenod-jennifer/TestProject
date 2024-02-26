using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomMethodData
{
    public Object target;
    public string methodName;
}

[System.Serializable]
public class FindBlockData
{
    public int startX = 0;
    public int startY = 0;
    public int endX = -1;
    public int endY = -1;
    public bool isCheckAllBlock = true;

    public List<BlockType> listBlockType = new List<BlockType>();
    public List<BlockColorType> listColorType = new List<BlockColorType>();
    public BlockBombType bombTYpe = BlockBombType.NONE;

    public List<BlockBase> GetMatchableBlockList()
    {
        List<BlockBase> listBlock = new List<BlockBase>();
        if (isCheckAllBlock == true)
        {
            foreach (var tempBlock in ManagerBlock.boards)
            {
                if (IsCanAddBLockLIst(tempBlock.Block) == true)
                    listBlock.Add(tempBlock.Block);
            }
        }
        else
        {
            if (endX == -1)
                endX = startX;
            if (endY == -1)
                endY = startY;

            int checkCnt_X = endX - startX;
            int checkCnt_Y = endY - startY;

            for (int inX = 0; inX <= checkCnt_X; inX++)
            {
                for (int intY = 0; intY <= checkCnt_Y; intY++)
                {
                    BlockBase block = ManagerBlock.boards[(startX + inX), (startY + intY)].Block;
                    if (IsCanAddBLockLIst(block) == true)
                        listBlock.Add(block);
                }
            }
        }
        return listBlock;
    }

    private bool IsCanAddBLockLIst(BlockBase block)
    {
        if (block == null)
            return false;

        if (listBlockType.Count > 0 && listBlockType.FindIndex(x => x == block.type) == -1)
            return false;

        if (listColorType.Count > 0 && listColorType.FindIndex(x => x == block.colorType) == -1)
            return false;

        if (block.bombType != bombTYpe)
            return false;

        return true;
    }
}

public class TutorialData_Wave : MonoBehaviour
{
    public GameObject conditionObj;
    public GameObject actionObj;

    public List<Tutorial_Condition> listCondition = new List<Tutorial_Condition>();
    public List<Tutorial_Action> listAction = new List<Tutorial_Action>();

    public void SetConditionList()
    {
        Tutorial_Condition[] conditions = conditionObj.GetComponents<Tutorial_Condition>();
        int childCnt = conditions.Length;
        for (int i = 0; i < childCnt; i++)
            listCondition.Add(conditions[i]);
    }

    public void SetActionList()
    {
        Tutorial_Action[] actions = actionObj.GetComponents<Tutorial_Action>();
        int childCnt = actions.Length;
        for (int i = 0; i < childCnt; i++)
            listAction.Add(actions[i]);

        listAction.Sort(delegate(Tutorial_Action actionA, Tutorial_Action actionB)
        {
            int typeA = (int)actionA.GetActionType();
            int typeB = (int)actionB.GetActionType();
            if (typeA < typeB) return -1;
            else if (typeA > typeB) return 1;
            else return 0;
        });
    }
}
