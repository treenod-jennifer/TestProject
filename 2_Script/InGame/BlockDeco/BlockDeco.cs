using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDeco : MonoBehaviour {

    public int inX;
    public int inY;

    public UIBlockSprite uiSprite;

    public BlockBase parentBlock;
    public static List<BlockDeco> _listBlockDeco = new List<BlockDeco>();


    public int pangIndex = -1;

    int mLifeCount = 1;
    public int lifeCount
    {
        get { return mLifeCount; }
        set
        {
            mLifeCount = value;
            SetSprite();
        }
    }

    public virtual void SetSprite()
    {

    }

    public virtual void SidePang(int tempPangIndex = 0, BlockColorType pangColorType = BlockColorType.NONE, bool bombEffect = false)
    {

    }


    public virtual bool DecoPang(int tempPangIndex = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        return IsInterruptBlockSelect();
    }

    public virtual bool IsInterruptBlockSelect()    //블럭선택을 막는지
    {
        return false;
    }

    public virtual bool IsInterruptBlockPang()  //블럭의 폭발을 막는지 //게임클리어시 남은폭탄 처리할때 막음
    {
        return false;
    }

    public virtual bool IsInterruptBlockEvent() //블럭 이벤트를 막는지(카운트 감소 등)
    {
        return false;
    }

    public virtual bool IsInterruptBlockMove()
    {
        return false;
    }
    
    public virtual void SetMainSpriteDepth()
    {
        uiSprite.depth = (int)GimmickDepth.DECO_CATCH;
    }
}
