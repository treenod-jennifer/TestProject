using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCandy : BlockBase
{
    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override void UpdateSpriteByBlockType()
    {
        if(type == BlockType.CANDY)
        {
            mainSprite.spriteName = "BlockCandy";
        }
        else if (type == BlockType.DUCK)
        {
            mainSprite.spriteName = "blockDuck";
        }

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool bombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (blockDeco != null)
        {
            if (bombEffect) return;

            if (blockDeco.DecoPang(uniqueIndex, splashColorType))
                return;
        }
        
        if (pangIndex == uniqueIndex) return;
        if (state == BlockState.PANG) return;

        pangIndex = uniqueIndex;

        state = BlockState.PANG;
        //_pangRemoveDelay = 0.5f;

        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        InGameEffectMaker.instance.MakeDuckEffect(_transform.position);
        _pangRemoveDelay = 0.5f;
        //턴으로 날아가는 효과        

        GameManager.instance.clearMission = 1;
        if (type == BlockType.CANDY)
        {
            GameManager.instance.getCandy = 1;
            ManagerSound.AudioPlay(AudioInGame.GET_CANDY);
        }
        else if (type == BlockType.DUCK)
        { 

            GameManager.instance.getDuck = 1;
            ManagerSound.AudioPlay(AudioInGame.GET_DUCK);
        }

        StartCoroutine(CoPang());
        GameUIManager.instance.GetChapterMissionSprite();
    }

    IEnumerator CoPang()
    {
        float timer = 0f;

        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one *( ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer) );
            timer += Global.deltaTimePuzzle * 2.5f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        yield return null;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_4;
    }
}
