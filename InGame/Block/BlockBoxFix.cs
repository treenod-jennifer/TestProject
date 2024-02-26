using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBoxFix : BlockBase
{
    public UILabel countLabel;
    public UISprite ground;

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsCanLink()
    {
        return false;
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = "blockBox" + GetColorTypeString();

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
        countLabel.text = lifeCount.ToString();
        countLabel.depth = mainSprite.depth + 1;
        ground.depth = mainSprite.depth + 1;
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (colorType != BlockColorType.NONE &&
            colorType != pangColorType &&
            !PangByBomb)
        {
            state = BlockState.WAIT;
            return;
        }

        lifeCount--;

        if (lifeCount <= 0)
        {
            _pangRemoveDelay = 0.3f;

            state = BlockState.PANG;
            StartCoroutine(CoPangFinal());
            ManagerSound.AudioPlay(AudioInGame.PLANT_PANG1);

            Destroy(countLabel.gameObject);
        }
        else
        {
            state = BlockState.WAIT;
            StartCoroutine(CoPang());
            if (lifeCount > 1) ManagerSound.AudioPlay(AudioInGame.PLANT_PANG3);
            else ManagerSound.AudioPlay(AudioInGame.PLANT_PANG2);
        }
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;

        if (state == BlockState.PANG) return;

        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;
        Pang(splashColorType);
    }

    IEnumerator CoPang()
    {
        //이펙트

        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
        UpdateSpriteByBlockType();

        timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }
    }


    IEnumerator CoPangFinal()
    {
        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        float timer = 0f;

        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }
    }
}
