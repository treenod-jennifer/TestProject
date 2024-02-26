using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class BlockSpaceShip : BlockBase
{
    [SerializeField]
    UISprite lineSprite;

    [SerializeField]
    UIBlockSprite spaceShipSprite;

    //우주선이 연속일 때, 아래 우주선도 따라 움직여야 하므로
    //현재 턴에 위로 움직였는지 아닌지
    public bool isMoveToUp = false;
    public BlockBase moveToUpBlock;

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanMakeBombFieldEffect()
    {
        //얼음이 설치되어 있는 상황이면 폭탄 영역 표시 가능
        if (IsCanCoverIce() == true && blockDeco != null)
            return true;
        else
            return false;
    }

    public override void SetSpriteEnabled(bool setEnabled)
    {
        spaceShipSprite.enabled = setEnabled;
    }

    public bool IsCanMatchBlock()
    {
        Board board = PosHelper.GetBoardSreeen(indexX, indexY);
        if (board != null && board.HasDecoCoverBlock() == false && board.IsGetTargetBoard(type)
           && (blockDeco == null || blockDeco.IsInterruptBlockSelect() == false)
           && board.IsNotDisturbBlock(BlockDirection.DOWN) == true)
        {
            return true;
        }

        return false;
    }

    public bool isCanMoveToUpSpaceShip()
    {
        //화면에 출력되는 기믹인지
        if (indexX < GameManager.MinScreenX || indexX >= GameManager.MaxScreenX || indexY < GameManager.MinScreenY || indexY >= GameManager.MaxScreenY)
        {
            return false;
        }

        //이벤트가 멈췄는지
        if (isStopEvent == true)
        {
            isStopEvent = false;
            return false;
        }

        //이번 턴에 아래로 흘렀는지
        if (isMoveDownThisTurn)
        {
            return false;
        }

        //얼음 기믹이 존재하는 지
        if (blockDeco != null && blockDeco.IsInterruptBlockSelect() == true)
        {
            return false;
        }

        //해당 방햑으로 움직일 수 있는지
        if (IsCanChangePosition(true, BlockDirection.UP) == false)
        {
            return false;
        }

        BlockBase upBlock = PosHelper.GetBlockScreen(indexX, indexY - 1);
        
        //교체할 윗기믹이 존재하는지, 교체 가능한 상태인지
        if (upBlock == null || upBlock.IsCanChangePosition(false) == false)
        {
            return false;
        }

        //교체할 윗기믹이 우주선인 경우
        if (upBlock.type == BlockType.SPACESHIP)
        {
            //윗우주선기믹이 얼음 1단계이면 교체 가능
            if (upBlock.blockDeco != null && upBlock.blockDeco.IsInterruptBlockMove() == false)
            {
                return true;
            }

            //윗우주선기믹이 올라가는 지
            BlockSpaceShip tempSpaceShip = upBlock as BlockSpaceShip;
            if (tempSpaceShip == null || tempSpaceShip.isMoveToUp == false)
            {
                return false;
            }
        }

        return true;
    }

    //교체될 맨 윗기믹값(moveToUpBlock) 설정
    public void setMoveToUpBlock()
    {
        if (isMoveToUp == false)
        {
            return;
        }

        BlockBase upBlock = PosHelper.GetBlockScreen(indexX, indexY - 1);

        moveToUpBlock = upBlock;

        //교체할 윗기믹이 우주선 기믹인 경우, 윗우주선기믹에서 값을 전달받음.
        if (moveToUpBlock.type == BlockType.SPACESHIP)
        {
            BlockSpaceShip tempSpaceShip = moveToUpBlock as BlockSpaceShip;

            if (tempSpaceShip.moveToUpBlock != null)
            {
                moveToUpBlock = tempSpaceShip.moveToUpBlock;
            }
        }
    }

    public IEnumerator MoveToUpSpaceShip(UnityAction callback) // 우주선 올라가는 코드
    {
        if (moveToUpBlock.blockDeco != null && moveToUpBlock.blockDeco.IsInterruptBlockSelect() == true)
        {
            spaceShipSprite.depth = moveToUpBlock.blockDeco.uiSprite.depth + 1;
        }
        else 
        {
            spaceShipSprite.depth = moveToUpBlock.mainSprite.depth + 10;
        }

        yield return CoLineDown();
        
        isMoveToUp = false;
        moveToUpBlock = null;

        callback();
        yield return null;
    }

    public IEnumerator CoLineDown()
    {
        spaceShipSprite.spriteName = "SpaceShip2";
        lineSprite.gameObject.SetActive(true);
        lineSprite.depth = spaceShipSprite.depth + 1;
        DOTween.To(() => lineSprite.height, x => lineSprite.height = x, 120, ManagerBlock.instance.GetIngameTime(0.3f));
        yield return null;

        ManagerSound.AudioPlayMany(AudioInGame.SPACESHIP_UP);

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.3f));

        moveToUpBlock.MoveToDirWithoutNull(0, 1);
        MoveToDirWithoutNull(0, -1);

        DOTween.To(() => lineSprite.height, x => lineSprite.height = x, 10, ManagerBlock.instance.GetIngameTime(0.5f));

        spaceShipSprite.spriteName = "SpaceShip1";
        lineSprite.gameObject.SetActive(false);
    }


    public override bool IsStopEvnetAtDestroyIce()
    {
        return true;
    }
    public override bool IsStopEvnetAtDestroyWater()
    {
        return true;
    }
    public override bool IsStopEvnetAtDestroyNet()
    {
        return true;
    }

    public override bool IsStopEventAtDestroyRandomBox()
    {
        return true;
    }
    
    public override bool IsStopEventAtMoveGround()
    {
        return true;
    }

    public override void UpdateSpriteByBlockType()
    {
        spaceShipSprite.depth = (int)GimmickDepth.BLOCK_BASE;
    }
    
    public override void SetStateWait()
    {
        if (isMoveDownThisTurn == false)
        {
            // 위로 교체 완료된 후 이미지 뎁스 수정
            UpdateSpriteByBlockType();
        }
    }

    public override void UpdateBlock()
    {
        MoveBlock();

        if (state == BlockState.WAIT)
        {
            if (IsCanMatchBlock())
            {
                state = BlockState.PANG;
                Pang();
            }
        }

        /*
        else if(state == BlockState.MOVE)
        {
            if (ManagerBlock.instance.isSpaceShipExitBlockHole == false && IsCanMatchBlock())
            {
                state = BlockState.PANG;
                Pang();
            }
        }
        */
    }


    public override void SpriteRatio(float ratio, float vertRatio = 0)
    {
        if (spaceShipSprite != null)
        {
            spaceShipSprite.customFill.verticalRatio = vertRatio;
            spaceShipSprite.customFill.blockRatio = ratio;
        }
    }

    public override void MakeDummyImage(Vector3 tartgetPos)
    {
        if (spaceShipSprite != null)
        {
            UIBlockSprite dummyMainSprite = NGUITools.AddChild(gameObject, BlockMaker.instance.blockDummySpriteObj).GetComponent<UIBlockSprite>();
            dummyMainSprite.spriteName = spaceShipSprite.spriteName;
            dummyMainSprite.depth = spaceShipSprite.depth;
            MakePixelPerfect(dummyMainSprite);
            dummyMainSprite.cachedTransform.localPosition = tartgetPos;
            dummySpriteList.Add(dummyMainSprite.customFill);
        }
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        ManagerSound.AudioPlay(AudioInGame.SPACESHIP_PANG);

        ManagerBlock.instance.AddScore(1000);
        InGameEffectMaker.instance.MakeScore(_transform.position, 1000);

        isSkipDistroy = true;
        DestroyBlockData();
        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            if (GameUIManager.instance.listGameTarget[i].targetType == TARGET_TYPE.SPACESHIP)
            {
                InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.SPACESHIP);
                break;
            }
        }

        ManagerBlock.instance.RemoveSpaceShip(this);
        PangDestroyBoardData();
    }

    public void InitBlock()
    {
        ManagerBlock.instance.AddSpaceShip(this);
        spaceShipSprite.spriteName = "SpaceShip1";
    }

    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return false;
        else
            return true;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (IsCanCoverIce() == true && blockDeco != null)
            return FireWorkRank.RANK_1;
        else
            return FireWorkRank.RANK_NONE;
    }

    public override IEnumerator CoFlashBlock_Color(int actionCount, float actionTIme, float waitTime)
    {
        float targetColorValue = 0.6f;
        Color targetColor = new Color(targetColorValue, targetColorValue, targetColorValue);

        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.To(() => spaceShipSprite.color, x => spaceShipSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            
            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        spaceShipSprite.color = Color.white;
        yield return null;
    }

    public override IEnumerator CoFlashBlock_Alpha(int actionCount, float actionTIme, float waitTime)
    {
        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.To((a) => spaceShipSprite.alpha = a, 1f, 0f, aTime).SetLoops(2, LoopType.Yoyo);

            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        spaceShipSprite.alpha = 1;
        yield return null;
    }
}