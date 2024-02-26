using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FenceBlock : DecoBase, IDisturb
{
    public BlockDirection direction = BlockDirection.NONE;
    public Board boardB;

    #region 폭탄 영역 관련
    [SerializeField]private UISprite effectBombFieldSprite;
    private Coroutine bombFieldRoutine = null;
    #endregion

    public void SetDisturbPang(int uniquePang = 0, BlockBombType bombType = BlockBombType.NONE, bool bombEffect = false)
    {
    }

    public override bool IsCanPangBlockBoardIndex(int indexX, int indexY, BlockDirection bombDirection)
    {
        if ((direction == BlockDirection.DOWN || direction == BlockDirection.UP)
        && (bombDirection == BlockDirection.RIGHT || bombDirection == BlockDirection.LEFT))
        {
            return true;
        }
        else if ((direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
            && (bombDirection == BlockDirection.DOWN || bombDirection == BlockDirection.UP))
        {
            return true;
        }

        if (bombDirection == direction)
        {
            if (inX == indexX && inY == indexY)
                return true;
            else
                return false;
        }
        else
        {
            if (bombDirection == BlockDirection.NONE)
            {
                if (inX == indexX && inY == indexY)
                    return true;
            }
            if (direction == BlockDirection.LEFT)
            {
                if (indexX < inX) return true;
            }
            else if (direction == BlockDirection.RIGHT)
            {
                if (indexX > inX) return true;
            }
            else if (direction == BlockDirection.UP)
            {
                if (indexY < inY) return true;
            }
            else if (direction == BlockDirection.DOWN)
            {
                if (indexY > inY) return true;
            }
        }

        return false;
    }

    public override bool SetDirectionPang(Board checkBoard = null, int bombX = 0, int bombY = 0, int uniquePang = 0, BlockDirection bombDirection = BlockDirection.NONE, List<BombAreaInfo> infoList = null, bool bNoneUseDirection = false)
    {
        if (pangIndex != uniquePang && lifeCount > 0)
        {
            if (bNoneUseDirection == false)
            {
                if (((direction == BlockDirection.DOWN || direction == BlockDirection.UP)
                && (bombDirection == BlockDirection.RIGHT || bombDirection == BlockDirection.LEFT))
                || ((direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
                && (bombDirection == BlockDirection.DOWN || bombDirection == BlockDirection.UP)))
                {
                    return false;
                }
            }

            //블럭 팡을 막는 블럭이 현재 칸에 존재하는 경우, 방해블럭이 터질 수 있는 지 검사.
            if (checkBoard != null && CanPangFenceBlock(checkBoard, bombX, bombY, bombDirection) == false)
                return false;

            //폭탄 범위에 걸쳐지면 터지지 않음.
            if (IsBombArea(infoList, bombX, bombY) == true)
                return false;

            InGameEffectMaker.instance.MakeRockEffect(this.transform.position);
            pangIndex = uniquePang;
            lifeCount--;
            ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG1);
            if (lifeCount <= 0)
            {
                StartCoroutine(CoPangFinal());
            }
            else
            {
                StartCoroutine(CoPang());
            }
            return true;
        }
        return false;
    }

    public override void MakeFieldEffect()
    {
        if (bombFieldRoutine != null)
            return;

        effectBombFieldSprite.depth = (int)GimmickDepth.DECO_DISTURB + 1;
        effectBombFieldSprite.alpha = 0;
        bombFieldRoutine = StartCoroutine(CoMakeFieldEffect());
    }

    private IEnumerator CoMakeFieldEffect()
    {
        DOTween.ToAlpha(() => effectBombFieldSprite.color, x => effectBombFieldSprite.color = x, 0.5f, ManagerBlock.instance.GetIngameTime(0.01f));
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.5f));
        DOTween.ToAlpha(() => effectBombFieldSprite.color, x => effectBombFieldSprite.color = x, 0f, ManagerBlock.instance.GetIngameTime(0.3f));
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.3f));
        bombFieldRoutine = null;
    }

    public override bool IsDisturbDeco_ByBomb(Board checkBoard = null, int bombX = 0, int bombY = 0, int uniquePang = 0, BlockDirection bombDirection = BlockDirection.NONE, List<BombAreaInfo> infoList = null, bool bNoneUseDirection = false)
    {
        if (pangIndex != uniquePang)
        {
            if (bNoneUseDirection == false)
            {
                if (((direction == BlockDirection.DOWN || direction == BlockDirection.UP)
                && (bombDirection == BlockDirection.RIGHT || bombDirection == BlockDirection.LEFT))
                || ((direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
                && (bombDirection == BlockDirection.DOWN || bombDirection == BlockDirection.UP)))
                {
                    return false;
                }
            }

            if (checkBoard != null && CanPangFenceBlock(checkBoard, bombX, bombY, bombDirection) == false)
                return false;

            //폭탄 범위에 걸쳐지면 터지지 않음.
            if (IsBombArea(infoList, bombX, bombY) == true)
                return false;

            //이미 life가 0인 상태라면 터지지 않음
            if (lifeCount >= 1)
                return true;
            else
                return false;
        }
        return false;
    }

    public bool IsLinkable()
    {
        return false;
    }

    public bool IsDisturbMove()
    {
        return true;
    }

    public bool IsDisturbBomb()
    {
        return true;
    }

    public bool IsCanPang()
    {
        return true;
    }

    //현재 보드의 해당 방향으로 방해블럭이 존재하는지(true : 존재함, false : 존재하지 않음).
    public bool IsDisturbBoardDirection(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0)
    {
        return IsDisturbBoard(blockDirection, indexX, indexY);
    }

    //현재 보드의 해당 방향으로 풀울타리, 돌울타리가 존재하는지(true : 존재함, false : 존재하지 않음).
    public bool IsDisturbBoard(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0)
    {
        if ((direction == BlockDirection.DOWN || direction == BlockDirection.UP)
        && (blockDirection == BlockDirection.RIGHT || blockDirection == BlockDirection.LEFT))
        {
            return false;
        }
        else if ((direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
            && (blockDirection == BlockDirection.DOWN || blockDirection == BlockDirection.UP))
        {
            return false;
        }

        if (blockDirection == direction)
        {
            if (inX == indexX && inY == indexY)
                return true;
            else
                return false;
        }
        else
        {
            if (direction == BlockDirection.LEFT)
            {
                if (indexX < inX) return true;
            }
            else if (direction == BlockDirection.RIGHT)
            {
                if (indexX > inX) return true;
            }
            else if (direction == BlockDirection.UP)
            {
                if (indexY < inY) return true;
            }
            else if (direction == BlockDirection.DOWN)
            {
                if (indexY > inY) return true;
            }
        }
        return false;
    }

    //해당 보드의 방향으로 물 생성이 막혀있는지(true : 물 생성 못함, false : 물 생성 가능).
    public bool IsDisturbMoveWater(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0)
    {
        return IsDisturbBoard(blockDirection, indexX, indexY);
    }

    //현재 보드의 방향으로 스플레시가 막혀있는지(true : 스플레시 불가능, false : 스플레시 가능).
    public bool IsDisturbSplashBoard(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0)
    {
        return IsDisturbBoard(blockDirection, indexX, indexY);
    }

    public override void RemoveDeco()
    {
        if (bombFieldRoutine != null)
            StopCoroutine(bombFieldRoutine);
        boardB.RemoveDeco(this);
        base.RemoveDeco();
    }

    public override bool IsPangDirectionExtendable(BlockDirection bombDirection, int indexX, int indexY)
    {
        //Debug.Log("boardIndexX :" + indexX + " / boardIndexY :" +indexY + " / inX : " + inX + " , inY : " + inY
        //    + " / direction : " + direction);

        if ((direction == BlockDirection.DOWN || direction == BlockDirection.UP)
        && (bombDirection == BlockDirection.RIGHT || bombDirection == BlockDirection.LEFT))
        {
            return true;
        }
        else if ((direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
            && (bombDirection == BlockDirection.DOWN || bombDirection == BlockDirection.UP))
        {
            return true;
        }

        if (direction == BlockDirection.LEFT && bombDirection != BlockDirection.LEFT)
        {
            if (indexX < inX) return false;
        }
        else if (direction == BlockDirection.RIGHT && bombDirection != BlockDirection.RIGHT)
        {
            if (indexX > inX) return false;
        }
        else if (direction == BlockDirection.UP && bombDirection != BlockDirection.UP)
        {
            if (indexY < inY) return false;
        }
        else if (direction == BlockDirection.DOWN && bombDirection != BlockDirection.DOWN)
        {
            if (indexY > inY) return false;
        }
        return false;
    }

    public override void SetSprite()
    {
        if (direction == BlockDirection.DOWN || direction == BlockDirection.UP)
        {
            uiSprite.spriteName = "block_H";
            uiSprite.depth = (int)GimmickDepth.DECO_DISTURB;
            MakePixelPerfect(uiSprite);
        }
        else
        {
            uiSprite.spriteName = "block_V";
            uiSprite.depth = (int)GimmickDepth.DECO_DISTURB;
            MakePixelPerfect(uiSprite);
            effectBombFieldSprite.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }

    //현재 방해블럭이 터질 수 있는 지 검사.
    private bool CanPangFenceBlock(Board checkBoard, int bombX, int bombY, BlockDirection bombDirection)
    {
        //현재 팡을 방해하는 오브젝트가 없다면 그냥 넘어감.
        if (checkBoard.Block == null || (checkBoard.Block != null && checkBoard.Block.IsPangExtendable() == true))
            return true;

        if (((direction == BlockDirection.DOWN || direction == BlockDirection.UP)
              && (bombDirection == BlockDirection.RIGHT || bombDirection == BlockDirection.LEFT))
              || ((direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
              && (bombDirection == BlockDirection.DOWN || bombDirection == BlockDirection.UP)))
        {
            return true;
        }

        BlockDirection checkBombDir = bombDirection;
        //블럭 방향 none일때, 방향 얻어오기.
        if (bombDirection == BlockDirection.NONE)
            checkBombDir = GetBombDirectionAtNoneDirection(bombX, bombY);

        return CheckCanPang(checkBoard, checkBombDir);
    }

    private bool CheckCanPang(Board checkBoard, BlockDirection bombDirection)
    {
        if (bombDirection == direction)
        {   // 폭발 방향과 방해블럭의 방향이 같을 때, 현재 블럭팡을 방해하는 오브젝트와 방해블럭이 같은 칸에 위치하면 안터짐.
            if (direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
            {
                if (checkBoard.indexX == inX)
                    return false;
            }
            else
            {
                if (checkBoard.indexY == inY)
                    return false;
            }
        }
        else
        {    // 폭발 방향과 방해블럭의 방향이 다를 때, 현재 블럭팡을 방해하는 오브젝트와 방해블럭이 다른 칸에 위치하면 안터짐.
            if (direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
            {
                if (checkBoard.indexX != inX)
                    return false;
            }
            else
            {
                if (checkBoard.indexY != inY)
                    return false;
            }
        }
        return true;
    }

    //현재 방해블럭이 폭발범위 내에 있는지 검사하는 함수.
    private bool IsBombArea(List<BombAreaInfo> infoList, int bombX, int bombY)
    {
        if (infoList == null)
            return false;

        //폭탄 범위에 걸쳐지면 터지지 않음.
        for (int i = 0; i < infoList.Count; i++)
        {
            for (int j = 0; j < infoList[i].areaDirList.Count; j++)
            {
                BlockDirection checkDir = infoList[i].areaDirList[j];

                if (checkDir == direction && infoList[i].indexX == inX && infoList[i].indexY == inY)
                    return true;

                switch (direction)
                {
                    case BlockDirection.UP:
                        if (checkDir == BlockDirection.DOWN && infoList[i].indexX == inX && infoList[i].indexY == inY - 1)
                            return true;
                        break;
                    case BlockDirection.DOWN:
                        if (checkDir == BlockDirection.UP && infoList[i].indexX == inX && infoList[i].indexY == (inY + 1))
                            return true;
                        break;
                    case BlockDirection.RIGHT:
                        if (checkDir == BlockDirection.LEFT && infoList[i].indexY == inY && infoList[i].indexX == inX + 1)
                            return true;
                        break;
                    case BlockDirection.LEFT:
                        if (checkDir == BlockDirection.RIGHT && infoList[i].indexY == inY && infoList[i].indexX == inX - 1)
                            return true;
                        break;
                }
            }
        }
        return false;
    }
    
    //폭발 방향찾기.
    private BlockDirection GetBombDirectionAtNoneDirection(int bombX, int bombY)
    {
        BlockDirection checkBombDir = BlockDirection.NONE;
        if (direction == BlockDirection.RIGHT || direction == BlockDirection.LEFT)
        {   //블럭이 터진 x 인덱스가 현재 방해블럭 x 보다 작다면 왼쪽에서 터진것이므로 폭발방향은 오른쪽.
            //크다면 오른쪽에서 터진것이므로 폭발방향은 왼쪽.
            if (bombX < inX)
                checkBombDir = BlockDirection.RIGHT;
            else
                checkBombDir = BlockDirection.LEFT;
        }
        else
        {   //블럭이 터진 y 인덱스가 현재 방해블럭 y 보다 작다면 위쪽에서 터진것이므로 폭발방향은 아래쪽.
            //크다면 아래쪽에서 터진것이므로 폭발방향은 위쪽.
            if (bombY < inY)
                checkBombDir = BlockDirection.DOWN;
            else
                checkBombDir = BlockDirection.UP;
        }
        return checkBombDir;
    }
}
