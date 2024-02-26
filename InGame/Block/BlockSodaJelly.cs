using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSodaJelly : BlockBase
{
    [SerializeField]
    private UISprite spriteSodaJelly;
    [SerializeField]
    private UISprite spritePlace;

    private int groupIndex;

    void Start()
    {
        mainSprite.spriteName = string.Format("sodaJelly_Base{0}", groupIndex);
        spritePlace.depth = (int)GimmickDepth.DECO_GROUND;
        MakePixelPerfect(spritePlace);
    }
    
    public override bool IsRemoveHideDeco_AtBlockPang()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsBlockType()
    {
        return false;
    }

    public override void UpdateBlock()
    {
        if (state == BlockState.WAIT)
            _waitCount++;
    }

    public override bool IsCanLink()
    {
        return false;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override bool IsThisBlockHasPlace()
    {
        return true;
    }

    public override bool IsCoverStatue() //석상위에 깔리는 블럭인지.
    {
        return true;
    }

    public override bool IsCanPangByPowerBomb()
    {
        return (lifeCount >= 2) ? true : false;
    }


    public override bool IsCanMakeBombFieldEffect()
    {
        return (lifeCount >= 2) ? true : false;
    }

    public override void UpdateSpriteByBlockType()
    {
        if (lifeCount < 1) return;
        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);

        switch (lifeCount)
        {
            case 1:
                spriteSodaJelly.spriteName = string.Format("sodaJelly{0}", groupIndex);
                spriteSodaJelly.enabled = true;
                spriteSodaJelly.depth = mainSprite.depth + 1;
                MakePixelPerfect(spriteSodaJelly);
                break;
            case 2:
                spriteSodaJelly.enabled = false;
                break;
        }
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;

        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;
        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (lifeCount <= 1)
            return;

        lifeCount--;
        if (lifeCount <= 1 && state != BlockState.PANG && ManagerBlock.instance.IsCanPangBlockSodaJelly(groupIndex) == true)
        {
            UpdateSpriteByBlockType();
            ManagerBlock.instance.PangBlockSodaJelly(groupIndex);
        }
        else
        {
            StartCoroutine(CoPang());
        }
    }

    IEnumerator CoPang()
    {
        //사운드 출력
        ManagerSound.AudioPlayMany(AudioInGame.SODAJELLY_1PANG);

        float timer = 0f;
        while (timer < 1f)
        {
            spriteSodaJelly.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        UpdateSpriteByBlockType();

        //이펙트
        InGameEffectMaker.instance.MakeEffectSodaJellyShineEffect(_transform.position, groupIndex);

        timer = 0f;
        while (timer < 1f)
        {
            spriteSodaJelly.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 3f;
            yield return null;
        }

        MakePixelPerfect(spriteSodaJelly);
        spriteSodaJelly.transform.localScale = Vector3.one;
    }

    IEnumerator CoPangFinal()
    {
        ManagerBlock.boards[indexX, indexY].CheckCarpetByPlant();

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        bool isInScreen = (back != null) ? true : false;

        if (isInScreen == true)
        {
            back.SetUnderBoard();
        }

        //스코어 출력
        int score = 500;
        ManagerBlock.instance.AddScore(score);
        if (isInScreen == true)
        {
            InGameEffectMaker.instance.MakeScore(transform.position, score);
        }

        //사운드 출력
        ManagerSound.AudioPlayMany(AudioInGame.SODAJELLY_DESTROY);

        bool makeEffect = false;
        float timer = 0f;
        while (timer < 1f && isInScreen == true)
        {
            if (timer > 0.5f && makeEffect == false)
            {
                //이펙트
                makeEffect = true;
                InGameEffectMaker.instance.MakeEffectSodaJellyHitEffect(_transform.position, groupIndex);
            }
            transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            yield return null;
        }

        if (back != null && back.BoardOnGrass.Count > 0)
        {
            foreach (var tempGrass in back.BoardOnGrass)
            {
                Grass grassA = tempGrass as Grass;
                grassA.uiSprite.depth = (int)GimmickDepth.DECO_FIELD;
                grassA.BGSprite.depth = grassA.uiSprite.depth + 1;
            }
        }
        PangDestroyBoardData();

        if (back != null)
            back.CheckStatus();
    }

    public void InitBlock(int group, int life)
    {
        lifeCount = life;
        groupIndex = group;
        ManagerBlock.instance.AddBlockSodaJellyInDictionary(groupIndex, this);
    }

    public bool IsCanDestroy()
    {
        if (lifeCount > 1)
            return false;
        return true;
    }

    public void PangBlockSodaJelly()
    {
        state = BlockState.PANG;
        StartCoroutine(CoPangFinal());
    }

    public override bool IsBlockRankExclusionTypeAtFireWork()
    {
        if (lifeCount <= 1)
            return true;
        else
            return false;
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        return FireWorkRank.RANK_1;
    }
}
