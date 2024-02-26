using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBox : BlockBase
{
    public UILabel countLabel;

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanLink()
    {
        return false;
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = "blockBox" + lifeCount;// + GetColorTypeString();

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);

        if(countLabel !=null)
        Destroy(countLabel.gameObject);

//        countLabel.text = lifeCount.ToString();
  //      countLabel.depth = mainSprite.depth + 1;
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (lifeCount <= 0) return;

        lifeCount--;
        InGameEffectMaker.instance.MakePotEffect(_transform.position, lifeCount);

        if (lifeCount <= 0)
        {
            _pangRemoveDelay = 0.3f;

            state = BlockState.PANG;
            IsSkipPang = true;
            StartCoroutine(CoPangFinal());
            ManagerSound.AudioPlayMany(AudioInGame.BREAK_1_JAR);
        }
        else
        {      
            IsSkipPang = true;
            state = BlockState.PANG;       
            //state = BlockState.WAIT;
            StartCoroutine(CoPang());
            if (lifeCount > 1)
            {
                ManagerSound.AudioPlayMany(AudioInGame.BREAK_3_JAR);
            }
            else
            {
                ManagerSound.AudioPlayMany(AudioInGame.BREAK_2_JAR);
            }
        }
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;
        if (state == BlockState.PANG) return;
        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;

        if (blockDeco != null)
        {
            if (bombEffect) return;

            if (blockDeco.DecoPang(uniqueIndex, splashColorType))
                return;
        }

        Pang(splashColorType);
    }

    IEnumerator CoPang()
    {
        //이펙트
        float timer = 0f;
        while (timer < 0.9f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            yield return null;

            if (timer > 0.3f && state == BlockState.PANG)
            {
                //state = BlockState.WAIT;
                if (targetPos != Vector3.zero && Vector3.Distance(_transform.localPosition, targetPos) > 15f)
                {
                    state = BlockState.MOVE;
                }
                else
                {
                    state = BlockState.WAIT;
                }
                IsSkipPang = false;
            }
        }
        UpdateSpriteByBlockType();
        yield return null;

        timer = 0.1f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            yield return null;
        }
        mainSprite.transform.localScale = Vector3.one;
    }


    IEnumerator CoPangFinal(bool isChangePang = false)
    {
        //이펙트
        InGameEffectMaker.instance.MakePotEffect(_transform.position, 0);

        ManagerBlock.instance.AddScore(500);
        InGameEffectMaker.instance.MakeScore(_transform.position, 500);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        float timer = 0f;

        while (timer < 1f)
        {
            _transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.SHOVEL);
        DestroyBlockData();
        GameUIManager.instance.RefreshTarget(TARGET_TYPE.SHOVEL);
        if(isChangePang == false) PangDestroyBoardData();
        else PangDestroyBoardDataWithoutNull();

        yield return null;
    }

    public override IEnumerator CoPangImmediately()
    {
        lifeCount = 0;
        yield return CoPangFinal(true);
        yield return null;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        //목표 검사
        if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.SHOVEL))
            return FireWorkRank.RANK_1;
        else
            return FireWorkRank.RANK_3;
    }

    public override bool IsBlockCheckKeyTypeAtFireWork()
    {
        return true;
    }
}
