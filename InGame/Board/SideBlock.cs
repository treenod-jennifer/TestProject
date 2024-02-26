using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideBlock : DecoBase, IDisturb
{
    public BlockDirection direction = BlockDirection.NONE;

    public void SetDisturbPang(int uniquePang = 0, BlockBombType bombType = BlockBombType.NONE, bool bombEffect = false)
    {

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
        return false;
    }

    public bool IsCanPang()
    {
        return false;
    }

    //현재 보드의 해당 방향으로 방해블럭이 존재하는지(true : 존재함, false : 존재하지 않음).
    public bool IsDisturbBoardDirection(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0)
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

    //현재 보드의 해당 방향으로 풀울타리, 돌울타리가 존재하는지(true : 존재함, false : 존재하지 않음).
    public bool IsDisturbBoard(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0)
    {
        return false;
    }

    //해당 보드의 방향으로 물 생성이 막혀있는지(true : 물 생성 못함, false : 물 생성 가능).
    public bool IsDisturbMoveWater(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0)
    {
        return false;
    }

    //현재 보드의 방향으로 스플레시가 막혀있는지(true : 스플레시 불가능, false : 스플레시 가능).
    public bool IsDisturbSplashBoard(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0)
    {
        return false;
    }

    public Board boardB;


    public override void RemoveDeco()
    {
        boardB.BoardOnDisturbs.Remove(this);
        base.RemoveDeco();
    }

    public override void SetSprite()
    {
        if (direction == BlockDirection.DOWN || direction == BlockDirection.UP)
        {
            uiSprite.spriteName = "block_H";
            uiSprite.depth = (int)GimmickDepth.DECO_SURFACE;
            MakePixelPerfect(uiSprite);
        }
        else
        {
            uiSprite.spriteName = "block_V";
            uiSprite.depth = (int)GimmickDepth.DECO_SURFACE;
            MakePixelPerfect(uiSprite);
        }
    }
}
