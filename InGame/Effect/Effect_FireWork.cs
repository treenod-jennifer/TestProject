using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Effect_FireWork : MonoBehaviour
{
    public BlockBase targetBlock = null;
    public Board targetBoard = null;
    public Vector3 targetPos = Vector3.zero;
    public bool HasCarpet = false;

    public UISprite fireSprite;

    [SerializeField] 
    private Effect_FireWork_Target targetSprite;

    [SerializeField] 
    private GameObject hitEffect;

    //출발대기 시간
    public float waitTime = 0f;

    //제자리 움직임
    float moveDistance = 50f;
    float moveSpeed = 2f;
    IEnumerator Start()
    {
        fireSprite.depth = (int)GimmickDepth.FX_EFFECTBASE;
        if (targetSprite.GetComponent<UISprite>() != null)
            targetSprite.GetComponent<UISprite>().depth = (int)GimmickDepth.FX_EFFECTBASE;
        targetSprite.gameObject.SetActive(true);

        if (targetBlock != null && targetBlock.size == BlockSizeType.BIG) // 2x2 기믹일 때 예외 처리
        {
            if (targetBlock == null || targetBoard.HasDecoHideBlock())
            {
                targetSprite.gameObject.transform.parent = GameUIManager.instance.groundAnchor.transform;
                targetSprite.gameObject.transform.localPosition = targetPos;
            }
            else
            {
                targetSprite.gameObject.transform.parent = targetBlock.transform;
                targetSprite.gameObject.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            if (targetBlock == null)
            {
                targetSprite.gameObject.transform.parent = GameUIManager.instance.groundAnchor.transform;
                targetSprite.gameObject.transform.localPosition = targetPos;
            }
            else
            {
                targetSprite.gameObject.transform.parent = targetBlock.transform;
                targetSprite.gameObject.transform.localPosition = Vector3.zero;
            }
        }
        

        float distance = Vector3.Distance(transform.localPosition, targetPos);
        int rotDir = transform.localPosition.x < targetPos.x ? 1 : -1;
        fireSprite.cachedTransform.localPosition = new Vector3(-rotDir * 16, 25f, 0);

        ManagerSound.AudioPlay(AudioInGame.BEE_GROWUP);

        //빙글빙글돌기
        //최소거리일때 출발
        float MinDistance = Vector3.Distance(transform.localPosition, targetPos);
        Vector3 beforePosition = transform.localPosition;
        yield return null;
        
        float timer = 0f;
        while (true)
        {
            timer += Global.deltaTimePuzzle;

            float addX = Mathf.Sin(Mathf.PI * timer * 2 * moveSpeed) * moveDistance;
            float addY = Mathf.Cos(Mathf.PI * timer * 2 * moveSpeed) * moveDistance;
            transform.localPosition = beforePosition + Vector3.up * addX + Vector3.right * addY;

            if (timer >= waitTime + 0.7f)
            {
                float currentDistance = Vector3.Distance(transform.localPosition, targetPos);
                if (currentDistance <= MinDistance || timer > 1.0f)
                    break;
            }
            yield return null;
        }

        //블럭 위치에 따라 벌 회전
        fireSprite.cachedTransform.localScale = new Vector3(fireSprite.cachedTransform.localScale.x * -rotDir, fireSprite.cachedTransform.localScale.y, fireSprite.cachedTransform.localScale.z);

        float dis = Vector3.Distance(transform.localPosition, targetPos);
        float moveTime = (0.03f + (dis * 0.001f));
        if (moveTime > 0.5f)
            moveTime = 0.5f;

        //블럭움직임 시간과 맞추기.
        moveTime = ManagerBlock.instance.GetIngameTime(moveTime);

        float offsetX = (targetPos.x - transform.localPosition.x) * 0.5f + 10f;
        float offsetY = (targetPos.y - transform.localPosition.y) * 0.5f + 10f;

        Vector3 control_1 = new Vector3(transform.localPosition.x - offsetX, transform.localPosition.y + offsetY, 0f);
        Vector3 control_2 = new Vector3(targetPos.x - offsetX, targetPos.y + offsetY, 0f);
        Vector3[] pathWayPoints = new[] { targetPos, control_1, control_2 };

        transform.DOLocalPath(pathWayPoints, moveTime, PathType.CubicBezier, PathMode.Ignore);
        yield return new WaitForSeconds(moveTime);

        if (targetSprite != null)
            targetSprite.EffectEnd();

        if (targetBlock != null && targetBlock.size == BlockSizeType.BIG) // 1x1 보다 큰 블럭일 경우 예외 처리
        {
            if (targetBlock != null && targetBoard.HasDecoHideBlock() == false)
            {
                if (HasCarpet
                    && targetBoard.HasDecoCoverBlock() == false
                    && targetBlock.IsCanMakeCarpet())
                {
                    Board tempBoard = PosHelper.GetBoard(targetBlock.indexX, targetBlock.indexY);
                    tempBoard.MakeCarpet(0f);
                }
   
                BlockBomb._bombUniqueIndex++;
                //targetBlock._pangRemoveDelay = 0.1f;
                //targetBlock.IsSkipPang = false;
                targetBlock.isRainbowBomb = false;
                targetBlock.BlockPang(BlockBomb._bombUniqueIndex, BlockColorType.NONE, true);
   
                //점수추가
                if (targetBlock.IsNormalBlock())
                {
                    InGameEffectMaker.instance.MakeScore(targetBlock._transform.position, 80);
                    ManagerBlock.instance.AddScore(80);
                }
   
            }
            else if (targetBoard != null)
            {
                if (targetBoard.BoardOnHide.Count > 0)
                {
                    BlockBomb._bombUniqueIndex++;
                    targetBoard.isFireWorkBoard = false;
                    targetBoard.HasDecoHideBlock(true, BlockBomb._bombUniqueIndex);
                }
                else if (targetBoard.BoardOnNet.Count > 0)
                {
                    BlockBomb._bombUniqueIndex++;
                    targetBoard.isFireWorkBoard = false;
                    targetBoard.HasDecoCoverBlock(true, BlockBomb._bombUniqueIndex);
                }
                else
                {
                    targetBoard.isFireWorkBoard = false;
                }
            } 
        }
        else
        {
            if (targetBlock != null)
            {
                if (HasCarpet
                    && targetBoard.HasDecoCoverBlock() == false
                    && targetBlock.IsCanMakeCarpet())
                {
                    Board tempBoard = PosHelper.GetBoard(targetBlock.indexX, targetBlock.indexY);
                    tempBoard.MakeCarpet(0f);
                }

                BlockBomb._bombUniqueIndex++;
                //targetBlock._pangRemoveDelay = 0.1f;
                //targetBlock.IsSkipPang = false;
                targetBlock.isRainbowBomb = false;
                targetBlock.BlockPang(BlockBomb._bombUniqueIndex, BlockColorType.NONE, true);

                //점수추가
                if (targetBlock.IsNormalBlock())
                {
                    InGameEffectMaker.instance.MakeScore(targetBlock._transform.position, 80);
                    ManagerBlock.instance.AddScore(80);
                }

            }
            else if (targetBoard != null)
            {
                if (targetBoard.BoardOnHide.Count > 0)
                {
                    BlockBomb._bombUniqueIndex++;
                    targetBoard.isFireWorkBoard = false;
                    targetBoard.HasDecoHideBlock(true, BlockBomb._bombUniqueIndex);
                }
                else if (targetBoard.BoardOnNet.Count > 0)
                {
                    BlockBomb._bombUniqueIndex++;
                    targetBoard.isFireWorkBoard = false;
                    targetBoard.HasDecoCoverBlock(true, BlockBomb._bombUniqueIndex);
                }
                else
                {
                    targetBoard.isFireWorkBoard = false;
                }
            }
        }
        

        BlockBomb._liveCount--;

        ManagerSound.AudioPlay(AudioInGame.BEE_HIT);
        hitEffect.SetActive(true);

        timer = 0f;
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;

            float TimeRatio = (1 - (timer / 0.5f)) * (1 - (timer / 0.5f));
            float AddRatio = Mathf.Sin(timer * Mathf.PI * 10) * TimeRatio;

            fireSprite.cachedTransform.localScale = new Vector3(-rotDir * 1.5f, 1.5f + AddRatio * 0.4f, 1.5f);
            fireSprite.cachedTransform.localPosition = new Vector3(-rotDir * 16, 25f + AddRatio * 10f, 0);

            if (timer > 0.3f)
                fireSprite.alpha = 1 - (timer - 0.3f) * 5;

            yield return null;
        }

        Destroy(gameObject);
    }
}
