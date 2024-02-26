using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHeartHome : BlockBase
{
    const BlockType exitType = BlockType.HEART;

    List<int> listHeartIndex = new List<int>();

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = "blockHeartHome";
        mainSprite.depth = (int)GimmickDepth.BLOCK_BASE + 1;
        mainSprite.MakePixelPerfect();
    }

    public override bool GetTargetBoard(BlockType blockType)
    {
        if (exitType == blockType) return true;
        return false;
    }

    public override bool isCanPangByRainbow()
    {
        return false;
    }

    public override bool IsCanMakeBombFieldEffect()
    {
        return false;
    }
    public void RemoveHeartIndex(int heartIndex)
    {
        listHeartIndex.Remove(heartIndex);
    }

    public void ClearListHeart()
    {
        listHeartIndex.Clear();
    }

    public void AddHeartIndex(int heartIndex)
    {
        if(listHeartIndex.Contains(heartIndex) == false)
            listHeartIndex.Add(heartIndex);
    }

    public int GetListHeartIndexCount()
    {
        return listHeartIndex.Count;
    }

    public bool HasListHeartIndex(int heartIndex)
    {
        if (listHeartIndex.Contains(heartIndex))
            return true;
        return false;
    }

    public void InitBlock()
    {
        ManagerBlock.instance.AddHeartHome(this);
    }

    public IEnumerator CoGetHeartEffect()
    {
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.2f));

        mainSprite.spriteName = "blockHeartHome1";
        mainSprite.depth = (int)GimmickDepth.BLOCK_BASE + 1;
        mainSprite.MakePixelPerfect();

        InGameEffectMaker.instance.MakeEffectBatteryGetEffect(_transform.position);
        ManagerSound.AudioPlay(AudioInGame.GET_STATUE);

        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.1f));

        ManagerBlock.instance.AddScore(1000);
        InGameEffectMaker.instance.MakeScore(_transform.position, 1000);

        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            if (GameUIManager.instance.listGameTarget[i].targetType == TARGET_TYPE.HEART)
            {
                InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.HEART);
                break;
            }
        }

        //하트 기믹 획득했으나 제거되지 않았을 때 이미지 되돌리기
        if (lifeCount > 0)
        {
            yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.1f));

            mainSprite.spriteName = "blockHeartHome";
            mainSprite.depth = (int)GimmickDepth.BLOCK_BASE + 1;
            mainSprite.MakePixelPerfect();
        }
    }

    public override IEnumerator CoPangImmediately()
    {
        IsSkipPang = true;
        isSkipDistroy = true;
        lifeCount = 0;

        yield return CoGetHeartEffect();


        DestroyBlockData();

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);

        yield return null;
    }
    public override void DestroyBlockData(int adType = -1)
    {
        ManagerBlock.instance.RemoveHeartHome(this);
        base.DestroyBlockData((int)adType);
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

    public override void SetRandomBoxHide(bool isHide)
    {
        SetSpriteEnabled(isHide);
    }

    public override void UpdateBlock()
    {
        base.UpdateBlock();
    }
}
