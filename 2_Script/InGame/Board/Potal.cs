using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum POTAL_TYPE
{
    IN,
    OUT,
    NONE,
}

public class Potal : DecoBase {

    public UILabel label;

    public Potal exitPotal;
    public Board exitBoard = null;

    public int index;

    POTAL_TYPE mType = POTAL_TYPE.IN;
    public POTAL_TYPE type
    {
        get { return mType; }
        set
        {
            mType = value;

            if(value == POTAL_TYPE.IN)
            {
                uiSprite.spriteName = "potal" + index.ToString() + "_0";
                uiSprite.cachedTransform.localPosition = Vector3.down * 43f;
                uiSprite.cachedTransform.Rotate(new Vector3(0f, 0f, 180f));
                uiSprite.depth = (int)GimmickDepth.DECO_SURFACE;
                MakePixelPerfect(uiSprite);
                uiSprite.cachedTransform.localScale = Vector3.one * 1.1f;
            }
            else if (value == POTAL_TYPE.OUT)
            {
                uiSprite.spriteName = "potal" + index.ToString() + "_0";
                uiSprite.depth = (int)GimmickDepth.DECO_SURFACE;
                MakePixelPerfect(uiSprite);
                uiSprite.cachedTransform.localPosition = Vector3.up * 43f;
                uiSprite.cachedTransform.localScale = Vector3.one * 1.1f;
            }

            if (ManagerBlock.instance.stageInfo.reverseMove == 1)
            {
                transform.localEulerAngles = new Vector3(0, 0, 180);
            }
        }
    }

    public void SetPotal()
    {
        if(exitPotal != null)
        {
            exitBoard = PosHelper.GetBoard(exitPotal.inX, exitPotal.inY);
        }        
    }

    public bool IsNotExistDisturbDeco()
    {
        BlockDirection checkDirection = BlockDirection.DOWN;
        if (mType == POTAL_TYPE.IN)
        {
            if (ManagerBlock.instance.stageInfo.reverseMove == 1)
                checkDirection = BlockDirection.UP;
            else
                checkDirection = BlockDirection.DOWN;
        }
        else
        {
            if (ManagerBlock.instance.stageInfo.reverseMove == 1)
                checkDirection = BlockDirection.DOWN;
            else
                checkDirection = BlockDirection.UP;
        }
        return board.IsNotDisturbBlock(checkDirection);
    }

    public bool IsHasMovableBlock()
    {
        Board board = PosHelper.GetBoard(inX, inY);

        if (board != null && !board.HasDecoCoverBlock() && IsNotExistDisturbDeco())
        {
            if(board.Block != null && board.Block.IsCanMove() && !board.HasBlockDecoInterruptBlockMove())
            {
                return true;
            }
        }
        return false;
    }

    bool rightOn = false;

    void Update()
    {
        if(type == POTAL_TYPE.OUT)
        {
            if (board != null && board.Block != null && board.Block.state == BlockState.MOVE)
            {
                if (!rightOn)
                {
                    rightOn = true;
                    uiSprite.spriteName = "potal" + index.ToString() + "_1";
                    // uiSprite.depth = (int)GimmickDepth.DECO_CATCH;
                }
            }
            else
            {
                if (rightOn)
                {
                    rightOn = false;
                    uiSprite.spriteName = "potal" + index.ToString() + "_0";
                    // uiSprite.depth = (int)GimmickDepth.DECO_LAND;
                }
            }
        }
        else
        {
            if (exitBoard != null && exitBoard.Block != null && exitBoard.Block.state == BlockState.MOVE)
            {
                if (!rightOn)
                {
                    rightOn = true;
                    uiSprite.spriteName = "potal" + index.ToString() + "_1";
                    // uiSprite.depth = (int)GimmickDepth.DECO_CATCH;
                }
            }
            else
            {
                if (rightOn)
                {
                    rightOn = false;
                    uiSprite.spriteName = "potal" + index.ToString() + "_0";
                    // uiSprite.depth = (int)GimmickDepth.DECO_LAND;
                }
            }
        }

    }
}
