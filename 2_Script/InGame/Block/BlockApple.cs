using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockApple : BlockBase
{
    public int AppleCount = 0;

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
        mainSprite.spriteName = "Apple_Plus" + AppleCount;

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
        _pangRemoveDelay = 0.3f;

        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        InGameEffectMaker.instance.MakeAppleEffect(_transform.position);
        //터지는 애니메이션
        //반칸내려가서 

        //턴으로 날아가는 효과
        ManagerSound.AudioPlayMany(AudioInGame.APPLE);
        DestroyBlockData();
        if (GameManager.instance.state == GameState.PLAY)
        {
            for (int i = 0; i < AppleCount; i++)
            {
                InGameEffectMaker.instance.MakeFlyApple(_transform.position, 1, 0.05f * i);
            }
            ManagerBlock.instance.AddScore(80);
            InGameEffectMaker.instance.MakeScore(_transform.position, 80);
        }
        else
        {
            //점수추가하기
            ManagerBlock.instance.AddScore(1000);
            InGameEffectMaker.instance.MakeScore(_transform.position, 1000);
        }

        //블럭 모으기 이벤트 처리.
        GetCollectEventBlock();

        StartCoroutine(CoPang());
    }

    IEnumerator CoPang()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * (ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer));
            timer += Global.deltaTimePuzzle * 3f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }
    }

    public override IEnumerator CoPangImmediately()
    {
        lifeCount = 0;
        
        Pang();

        yield return null;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_3;
    }
}
