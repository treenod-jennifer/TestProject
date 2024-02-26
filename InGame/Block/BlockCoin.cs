using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCoin : BlockBase
{
    public UILabel labelCoinCount;
    public bool isCoinBag = false;
    public int coinCount = 1;

    public override bool IsCanMove()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return true;
    }
    public override void SetSpriteEnabled(bool setEnabled)
    {
        mainSprite.enabled = setEnabled;
        labelCoinCount.enabled = setEnabled;
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = "BlockCoin";

        if (isCoinBag)
        {
            string number = "3";
            if (coinCount < 10)
                number = "1";
            else if(10 <= coinCount && coinCount < 20)
                number = "2";
            mainSprite.spriteName = string.Format("BlockCoinBag_0{0}", number);
        }
        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
        InitCoinCountText();
    }


    private void InitCoinCountText()
    {
        if (isCoinBag)
        {
            labelCoinCount.gameObject.SetActive(true);
            labelCoinCount.text = coinCount.ToString();
            labelCoinCount.depth = mainSprite.depth + 1;
        }
        else
        {
            labelCoinCount.gameObject.SetActive(false);
        }
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool bombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (bombEffect) return;
        if (blockDeco != null)
        {
            if (blockDeco.DecoPang(uniqueIndex, splashColorType))
                return;
        }
        if (state == BlockState.PANG) return;
        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;
        state = BlockState.PANG;
        _pangRemoveDelay = 0.3f;

        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        InGameEffectMaker.instance.MakeAppleEffect(_transform.position);
        //터지는 애니메이션
        //턴으로 날아가는 효과
        ManagerSound.AudioPlayMany(AudioInGame.APPLE);

        if(isCoinBag)
        {
            for (int i = 0; i < coinCount; i++)
            {
                InGameEffectMaker.instance.MakeFlyCoin(transform.position, 1, i * 0.02f);
            }
        }
        else
        {
            InGameEffectMaker.instance.MakeFlyCoin(transform.position, 1, 0f);
            InGameEffectMaker.instance.MakeFlyCoin(transform.position, 1, 0.1f);

            //점수추가하기
            ManagerBlock.instance.AddScore(1000);
            InGameEffectMaker.instance.MakeScore(_transform.position, 1000);
        }
        DestroyBlockData();
        StartCoroutine(CoPang());
    }

    IEnumerator CoPang()
    {
        float timer = 0f;

        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * 1.3f * (ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer));
            timer += Global.deltaTimePuzzle * 3f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }
    }

    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        return true;
    }
}
