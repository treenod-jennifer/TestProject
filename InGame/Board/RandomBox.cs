using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RandomBox : DecoBase, INet
{
    const string SPRITE_NAME = "RandomBox";

    public bool IsNetDeco()
    {
        return true;
    }
    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override bool IsWarpBlock()
    {
        return true;
    }

    public override bool IsInterruptBlockEvent()
    {
        return true;
    }

    public override bool IsInterruptBlockColorChange()
    {
        return true;
    }

    public override bool IsDisturbPangByRainbowBomb()
    {
        return false;
    }

    public void SetBlockSpriteEnable()
    {
        if (GameManager.instance.state == GameState.EDIT)
        {
            uiSprite.alpha = 0.4f;
            return;
        }

        if (board.Block != null)
        {
            board.Block.SetRandomBoxHide(false);
        }

        for (int i = 0; i < board.DecoOnBoard.Count; i++)
        {
            Carpet carpet = board.DecoOnBoard[i] as Carpet;
            Water water = board.DecoOnBoard[i] as Water;

            if (carpet != null)
            {
                carpet.SetSpriteEnabled(false);
            }

            if (water != null)
            {
                water.SetSpriteEnabled(false);
            }
        }
    }

    public override void SetSprite()
    {
        if (lifeCount <= 0) return;

        if (GameManager.instance.state == GameState.EDIT)
            uiSprite.spriteName = string.Format("{0}{1}_{2}", SPRITE_NAME, 0, lifeCount);
        else
            uiSprite.spriteName = string.Format("{0}{1}_{2}", SPRITE_NAME, 1, lifeCount);

        uiSprite.cachedTransform.localScale = Vector3.one * 0.75f;
        SetMainSpriteDepth();
    }

    public void SetNetPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if (pangIndex != uniquePang)
        {
            pangIndex = uniquePang;
            lifeCount--;

            if (lifeCount <= 0)
            {
                StartCoroutine(CoPangRandomBoxFinal());
            }
        }
    }

    public override bool SetSplashPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE, bool bombEffect = false)
    {
        if (bombEffect) return true;

        if (pangIndex != uniquePang)
        {
            pangIndex = uniquePang;
            lifeCount--;

            if (lifeCount > 0)
            {
                StartCoroutine(PangRandomBox());
            }
            else
            {
                StartCoroutine(CoPangRandomBoxFinal());
            }
        }

        return true;
    }

    IEnumerator PangRandomBox()
    {
        yield return CoScaleAction();
    }

    IEnumerator CoPangRandomBoxFinal()
    {
        //박스 스케일 조정
        float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
        uiSprite.transform.DOScale(1.0f, actionTime).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(actionTime);

        //박스 안 기믹 동작 시작
        if (board.Block != null)
        {
            board.Block.SetRandomBoxHide(true);

            if (board.Block.IsStopEventAtDestroyRandomBox())
                board.Block.isStopEvent = true;
        }
        Carpet carpet = null;
        Water water = null;
        for (int i = 0; i < board.DecoOnBoard.Count; i++)
        {
            if(carpet == null) carpet = board.DecoOnBoard[i] as Carpet;
            if(water == null) water = board.DecoOnBoard[i] as Water;

            if (carpet != null)
            {
                carpet.SetSpriteEnabled(true);
            }

            if (water != null)
            {
                water.SetSpriteEnabled(true);
                ManagerBlock.instance.GetWater = true;
            }
        }

        //박스 이미지 변경 및 투명도 조절
        uiSprite.spriteName = string.Format("{0}{1}_{2}", SPRITE_NAME, 1, 3);
        uiSprite.color = new Color(uiSprite.color.r, uiSprite.color.g, uiSprite.color.b, 0.5f);

        //박스 안 기믹에서 나가는 연출
        ManagerBlock.instance.AddScore(80);
        InGameEffectMaker.instance.MakeScore(transform.position, 80);

        if (board.Block != null)
        {
            InGameEffectMaker.instance.MakeEffectRandomBoxOpen(transform.position);

            board.Block.ExitByRandomBox(() => { uiSprite.color = new Color(uiSprite.color.r, uiSprite.color.g, uiSprite.color.b, 0f); });

            actionTime = ManagerBlock.instance.GetIngameTime(0.08f) * 2;
            yield return new WaitForSeconds(actionTime);
        }
        else
        {
            //내부 블럭 없는 경우 연기 이펙트
            if(water == null && carpet == null)
                InGameEffectMaker.instance.MakeEffectRandomBoxDust(transform.position);
            else
                InGameEffectMaker.instance.MakeEffectRandomBoxOpen(transform.position);

            actionTime = ManagerBlock.instance.GetIngameTime(0.08f);
            yield return new WaitForSeconds(actionTime);
        }

        RemoveDeco();
        
        yield return null;
    }

    IEnumerator CoScaleAction()
    {
        float actionTime = ManagerBlock.instance.GetIngameTime(0.2f);
        uiSprite.transform.DOScale(1.0f, actionTime).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(actionTime);

        actionTime = ManagerBlock.instance.GetIngameTime(0.4f);
        uiSprite.transform.DOScale(0.75f, actionTime).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(actionTime);
    }

    public override void SetMainSpriteDepth()
    {
        uiSprite.depth = (int)GimmickDepth.DECO_COVER;
    }
}
