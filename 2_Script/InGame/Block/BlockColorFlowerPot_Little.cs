using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockColorFlowerPot_Little : BlockBase
{
    public int index;
    [SerializeField]
    private UISprite spriteFlower;

    public override bool IsDigTarget()
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
        if(state == BlockState.WAIT)
            _waitCount++;
    }

    public override bool IsCanLink()
    {
        return false;
    }

    public override bool isCanPangByRainbow()
    {
        return false;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override bool IsCoverStatue() //석상위에 깔리는 블럭인지.
    {
        return true;
    }

    public override void UpdateSpriteByBlockType()
    {
        SetMainSpriteDepth();
        spriteFlower.depth = mainSprite.depth + 1;
        spriteFlower.spriteName = "flower_01_" + ManagerBlock.instance.GetColorTypeString(colorType);
        MakePixelPerfect(spriteFlower);
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;

        if (lifeCount <= 0 || state == BlockState.PANG)
            return;

        if (byRainbow == false && pangIndex == uniqueIndex)
            return;

        if (splashColorType == BlockColorType.NONE)
            return;

        if (colorType != splashColorType)
            return;   //컬러매치

        pangIndex = uniqueIndex;
        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (state != BlockState.PANG)
        {
            state = BlockState.PANG;
            StartCoroutine(CoPangFinal());

            for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
            {
                if (GameUIManager.instance.listGameTarget[i].targetType == TARGET_TYPE.FLOWER_POT)
                {
                    InGameEffectMaker.instance.MakeFlowerBedFlower(_transform.position);
                    break;
                }
            }
        }
    }

    IEnumerator CoPangFinal(bool isPangImmediately = false, bool isChangePang = false)
    {
        ManagerBlock.boards[indexX, indexY].CheckCarpetByPlant();

        if(isPangImmediately == false) yield return CoPangAction();

        InGameEffectMaker.instance.MakeFlyTarget(transform.position, TARGET_TYPE.FLOWER_POT, BlockColorType.NONE, colorType);
        ManagerBlock.instance.AddScore(500);
        InGameEffectMaker.instance.MakeScore(transform.position, 500);
        ManagerSound.AudioPlayMany(AudioInGame.FLOWERPOT_DESTROY);

        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        float timer = 0f;
        while (timer < 1f)
        {
            transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            spriteFlower.color = new Color(1f, 1f, 1f, 1f - timer);
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

        if (isChangePang == false) PangDestroyBoardData();
        else PangDestroyBoardDataWithoutNull();

        if (back != null)
            back.CheckStatus();
    }

    public IEnumerator CoPangAction()
    {
        float timer = 0f;
        while (timer < 1f)
        {
            spriteFlower.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        spriteFlower.spriteName = "flower_02_" + ManagerBlock.instance.GetColorTypeString(colorType);
        MakePixelPerfect(spriteFlower);

        bool makeEffect = false;
        timer = 0f;
        while (timer < 1f)
        {
            spriteFlower.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;

            if (timer > 0.5f && makeEffect == false)
            {
                makeEffect = true;
                InGameEffectMaker.instance.MakeLittleFlowerBedPang(_transform.position);
            }
            yield return null;
        }
    }

    public override IEnumerator CoPangImmediately()
    {
        lifeCount = 0;
        yield return CoPangFinal(true);
    }

    public override FireWorkRank GetDefaultBlockRankAtFireWork()
    {
        if (ManagerBlock.instance.HasAchievedCollectTarget(TARGET_TYPE.FLOWER_POT))
            return FireWorkRank.RANK_1;
        else
            return FireWorkRank.RANK_NONE;
    }
}
