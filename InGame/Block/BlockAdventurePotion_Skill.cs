using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAdventurePotion_Skill : BlockBase
{
    const string SpriteName = "potion_Skill";

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
        if (lifeCount <= 1)
            mainSprite.spriteName = SpriteName + "_On";
        else
            mainSprite.spriteName = SpriteName + "_Off";

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
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
        _pangRemoveDelay = 0.3f;

        Pang();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        lifeCount--;

        if (lifeCount <= 0 && state != BlockState.PANG)
        {
            state = BlockState.PANG;
            StartCoroutine(CoPangFinal());
            ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG1);
        }
        else
        {
            StartCoroutine(CoPang());
            ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG2);
        }
    }

    IEnumerator CoPang()
    {
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

        MakePixelPerfect(mainSprite);
        mainSprite.transform.localScale = Vector3.one;
    }

    IEnumerator CoPangFinal()
    { 
        //이펙트.
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);
        
        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        //if (ManagerAdventure.instance != null)
        {
            Debug.Log("스킬포션 획득");
        }
        DestroyBlockData();

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
    }
}
