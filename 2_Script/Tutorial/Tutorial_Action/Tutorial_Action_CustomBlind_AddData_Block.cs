using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 커스텀 블라인드 데이터 추가(블럭 위치 데이터로)
/// </summary>
public class Tutorial_Action_CustomBlind_AddData_Block : Tutorial_Action
{
    [System.Serializable]
    public class CustomBlindBlockData_Index
    {
        public int key = 0;
        public CustomBlindAreaData_Block_Index areaData = new CustomBlindAreaData_Block_Index();
    }
    public List<CustomBlindBlockData_Index> listDatas_index = new List<CustomBlindBlockData_Index>();

    //블럭의 위치가 아닌 보드의 위치를 구할 떄 사용.
    public bool isFindBoardPosition = false;

    [System.Serializable]
    public class CustomBlindBlockData_BlockType
    {
        public int key = 0;
        public CustomBlindAreaData_Block_BlockType findBlockData = new CustomBlindAreaData_Block_BlockType();
    }
    public List<CustomBlindBlockData_BlockType> listDatas_blockType = new List<CustomBlindBlockData_BlockType>();

    public override ActionType GetActionType()
    {
        return ActionType.DATA_SET;
    }

    public override void StartAction(System.Action endAction = null)
    {
        AddListCustomBlindDataByBlockPos();
        endAction.Invoke();
    }

    private void AddListCustomBlindDataByBlockPos()
    {
        #region 인덱스로 받아온 블럭의 블라인드 데이터 추가
        for (int i = 0; i < listDatas_index.Count; i++)
        {
            CustomBlindAreaData_Block_Index areaData = listDatas_index[i].areaData;

            Vector3? targetPos_1 = GetTargetPos(areaData.x1, areaData.y1);
            if (targetPos_1 == null)
                continue;

            List<CustomBlindData> listData = new List<CustomBlindData>();
            if ((areaData.x2 > -1 && areaData.y2 > -1))
            {
                Vector3? targetPos_2 = GetTargetPos(areaData.x2, areaData.y2);
                if (targetPos_2 == null)
                    continue;

                float centerPos_X = (targetPos_1.Value.x + targetPos_2.Value.x) * 0.5f;
                float centerPos_Y = (targetPos_1.Value.y + targetPos_2.Value.y) * 0.5f;
                Vector3 centerPos = new Vector3(centerPos_X + areaData.offsetX, centerPos_Y + areaData.offsetY, 0f);

                int touchSize_X = (areaData.x1 > areaData.x2) ? (areaData.x1 - areaData.x2 + 1) * areaData.sizeX : (areaData.x2 - areaData.x1 + 1) * areaData.sizeX;
                int touchSize_Y = (areaData.y1 > areaData.y2) ? (areaData.y1 - areaData.y2 + 1) * areaData.sizeY : (areaData.y2 - areaData.y1 + 1) * areaData.sizeY;

                listData.Add(new CustomBlindData(centerPos, touchSize_X, touchSize_Y));
            }
            else
            {
                Vector3 centerPos = new Vector3(targetPos_1.Value.x + areaData.offsetX, targetPos_1.Value.y + areaData.offsetY, 0f);
                listData.Add(new CustomBlindData(centerPos, areaData.sizeX, areaData.sizeY));
            }

            int key = listDatas_index[i].key;
            if (ManagerTutorial._instance._current.dicCustomBlindData.ContainsKey(key) == true)
                ManagerTutorial._instance._current.dicCustomBlindData[key] = new List<CustomBlindData>(listData);
            else
                ManagerTutorial._instance._current.dicCustomBlindData.Add(key, listData);
        }
        #endregion

        #region 블럭 타입으로 받아온 블럭의 블라인드 데이터 추가
        for (int i = 0; i < listDatas_blockType.Count; i++)
        {
            CustomBlindAreaData_Block_BlockType areaData = listDatas_blockType[i].findBlockData;
            List<BlockBase> listBlock = new List<BlockBase>(areaData.findBlockData.GetMatchableBlockList());
            List<CustomBlindData> listData = new List<CustomBlindData>();
            for (int j = 0; j < listBlock.Count; j++)
            {
                BlockBase tempBlock = listBlock[j];
                Vector3 centerPos = new Vector3(tempBlock.transform.localPosition.x + areaData.offsetX, tempBlock.transform.localPosition.y + areaData.offsetY, 0f);
                listData.Add(new CustomBlindData(centerPos, areaData.sizeX, areaData.sizeY));
            }

            int key = listDatas_blockType[i].key;
            if (ManagerTutorial._instance._current.dicCustomBlindData.ContainsKey(key) == true)
                ManagerTutorial._instance._current.dicCustomBlindData[key] = new List<CustomBlindData>(listData);
            else
                ManagerTutorial._instance._current.dicCustomBlindData.Add(key, listData);
        }

        #endregion
    }

    private Vector3? GetTargetPos(int inX, int inY)
    {
        Vector3? targetPos_1 = null;
        if (isFindBoardPosition == false)
        {
            BlockBase tempBlock_1 = PosHelper.GetBlock(inX, inY);
            if (tempBlock_1 != null)
                targetPos_1 = tempBlock_1.transform.localPosition;
        }
        else
        {
            targetPos_1 = PosHelper.GetPosByIndex(inX, inY);
        }
        return targetPos_1;
    }
}
