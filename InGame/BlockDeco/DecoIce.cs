using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoIce : BlockDeco
{
    const string SpriteName = "DecoIce";

    public override void SetSprite()
    {
        uiSprite.spriteName = SpriteName + lifeCount.ToString();
        SetMainSpriteDepth();
    }

    public override bool IsInterruptBlockSelect()    //블럭선택을 막는지
    {
        return true;
    }

    public override void SidePang(int tempPangIndex = 0, BlockColorType pangColorType = BlockColorType.NONE, bool bombEffect = false)
    {
        if (bombEffect) return;

        DecoPang(tempPangIndex, pangColorType);
    }

    public override bool DecoPang(int tempPangIndex = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if (lifeCount == 0) return false;

        if (pangIndex == tempPangIndex)return true;

        //추후컬러타입추가
        pangIndex = tempPangIndex;
        lifeCount--;

        InGameEffectMaker.instance.MakeICeEffect(transform.position);

        if (lifeCount > 0)
        {
            StartCoroutine(PangIce());

            // 고정 얼음을 화면 수에 포함시키지 않는 설정이 켜져있으면,고정얼음이 얼음 1단계가 됐을 때 해당 얼음을 화면수에 포함시킴.
            if (ManagerBlock.instance.stageInfo.isFixedIceAdjust == 1 && lifeCount == 1)
            {
                ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ICE]++;
            }
        }
        else
        {
            if (parentBlock != null)
            {
                parentBlock.pangIndex = tempPangIndex;
                if (parentBlock.IsStopEvnetAtDestroyIce())
                    parentBlock.isStopEvent = true;
            }
            StartCoroutine(PangIceFinal());
        }

        return true;
    }

    IEnumerator PangIce()
    {
        float timer = 0f;

        while (timer < 0.1f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }
    }

    IEnumerator PangIceFinal()
    {
        ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.ICE);
        ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ICE]--;
        GameUIManager.instance.RefreshTarget(TARGET_TYPE.ICE);

        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }
     
        //제거
        parentBlock.blockDeco = null;
        //카운트 있는 블럭들의 경우 카운트 액션 실행
        if (parentBlock.type == BlockType.BLOCK_DYNAMITE || parentBlock.type == BlockType.WATERBOMB)
            parentBlock.UpdateSpriteByBlockType();
        Destroy(gameObject);
        yield return null;
    }

    public override bool IsInterruptBlockEvent() //시간카운트줄이기를 막는지?
    {
        return true;
    }

    public override bool IsInterruptBlockMove()
    {
        if (lifeCount >= 2)
            return true;

        return false;
    }

    public override bool IsInterruptBlockPang()  //블럭의 폭발을 막는지
    {
        return true;
    }

    public override void SetMainSpriteDepth()
    {
        uiSprite.depth = (int)GimmickDepth.DECO_CATCH;
    }
}
