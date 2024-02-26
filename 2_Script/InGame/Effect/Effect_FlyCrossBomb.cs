using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Effect_FlyCrossBomb : MonoBehaviour
{
    [SerializeField]
    private GameObject effectRoot;

    [SerializeField]
    private GameObject targetObj;

    private Board targetBoard = null;
    private BlockBase targetBlock = null;
    private Vector3 targetPos = Vector3.zero;

    public void Init(Vector3 startPos, Board tBoard)
    {
        this.transform.position = startPos;
        targetBoard = tBoard;
        targetBlock = tBoard.Block;

        effectRoot.SetActive(true);
        targetObj.SetActive(true);

        //블럭이 있으면, 블럭기준으로 목표지점 설정(1x1크기를 넘는 기믹때문에), 없으면 보드 기준으로 설정
        if (targetBlock != null)
        {
            effectRoot.transform.parent = targetBlock.transform;
            targetObj.transform.parent = targetBlock.transform;
            targetPos = (targetBlock.mainSprite != null) ? targetBlock.mainSprite.transform.localPosition : targetBlock.transform.localPosition;
        }
        else
        {
            effectRoot.transform.parent = GameUIManager.instance.groundAnchor.transform;
            targetObj.transform.parent = GameUIManager.instance.groundAnchor.transform;
            targetPos = PosHelper.GetPosByIndex(targetBoard.indexX, targetBoard.indexY);
        }
        targetObj.transform.localPosition = targetPos;


        StartCoroutine(CoStart());
    }

    public IEnumerator CoStart()
    {
        BlockBomb._liveCount++;

        //현재 타겟으로 잡은 블럭이나 보드의 상태를 변경시켜줌.
        if (targetBlock != null)
            targetBlock.isRainbowBomb = true;
        else if (targetBoard.BoardOnNet.Count > 0)
            targetBoard.isFireWorkBoard = true;

        //거리에 따라 움직이는 시간 설정
        float sqrLen = (targetPos - effectRoot.transform.localPosition).sqrMagnitude;
        float moveTime = (0.8f + (sqrLen * 0.001f));
        if (moveTime > 1.0f)
            moveTime = 1.0f;

        //블럭움직임 시간과 맞추기.
        moveTime = ManagerBlock.instance.GetIngameTime(moveTime);

        //타겟 블럭으로 이동하는 연출
        effectRoot.transform.DOLocalMove(targetPos, moveTime).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(moveTime);

        //이동 후, 폭탄 터짐.
        effectRoot.transform.parent = this.transform;
        targetObj.transform.parent = this.transform;
        BlockBomb._bombUniqueIndex++;

        BlockMaker.instance.MakeBombBlock(targetBlock, targetBoard.indexX, targetBoard.indexY, BlockBombType.BLACK_BOMB, BlockColorType.NONE, false);
        //타겟이 되는 블럭이나 보드는 개별로 터뜨려줌.
        if (targetBlock != null)
        {
            targetBlock._pangRemoveDelay = 0.1f;
            targetBlock.IsSkipPang = false;
            targetBlock.isRainbowBomb = false;
            targetBlock.BlockPang(BlockBomb._bombUniqueIndex, BlockColorType.NONE, true);
        }
        else
        {
            targetBoard.isFireWorkBoard = false;
            
            if (targetBoard.BoardOnHide.Count > 0)
            {
                targetBoard.HasDecoHideBlock(true, BlockBomb._bombUniqueIndex);
            }
            else if (targetBoard.BoardOnNet.Count > 0)
            {
                targetBoard.HasDecoCoverBlock(true, BlockBomb._bombUniqueIndex);   
            }
        }

        //터진 후 이펙트 제거.
        BlockBomb._liveCount--;
        Destroy(gameObject);
    }
}
