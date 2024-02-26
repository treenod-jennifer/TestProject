using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lava : DecoBase
{
    const string SPRITE_NAME = "ingame_lava_splash";
    const string SPRITE_BG_NAME = "ingame_lava_splash_frame";

    public bool gameOverLava = false;

    private void Start()
    {
        SetSprite();
    }

    public override bool IsCanFill()
    {
        return false;
    }
    public override bool IsCanFlow()
    {
        return false;
    }

    public UISprite BGSprite;

    public void RemoveLava()
    {
        board.lava = null;
        RemoveDeco();
    }
    

    public override void SetSprite()
    {
        uiSprite.spriteName = SPRITE_NAME;
        uiSprite.depth = (int)GimmickDepth.DECO_DISTROY + 1;
        MakePixelPerfect(uiSprite);

        BGSprite.spriteName = SPRITE_BG_NAME;
        BGSprite.depth = (int)GimmickDepth.DECO_DISTROY;
        MakePixelPerfect(BGSprite);
    }

    public void MakeLava()
    {
        //위치설정
        if (PosHelper.GetBoard(inX, inY, 0, 1) != null 
            && PosHelper.GetBoard(inX, inY, 0, 1).lava != null)
            gameObject.transform.localPosition = PosHelper.GetPosByIndex(inX, inY + 1);
        else if (PosHelper.GetBoard(inX, inY, 1, 0) != null 
            && PosHelper.GetBoard(inX, inY, 1, 0).lava != null)
            gameObject.transform.localPosition = PosHelper.GetPosByIndex(inX+1, inY);
        else if (PosHelper.GetBoard(inX, inY, -1, 0) != null 
            && PosHelper.GetBoard(inX, inY,-1, 0).lava != null)
            gameObject.transform.localPosition = PosHelper.GetPosByIndex(inX-1, inY);
        
        ManagerSound.AudioPlay(AudioInGame.LAVA_LOG);
        ManagerBlock.instance.makeLaveCount++;
        StartCoroutine(CoMoveLava());
    }

    IEnumerator CoMoveLava()
    {
        Vector3 targetPos = PosHelper.GetPosByIndex(inX, inY);
        yield return null;

        while (Vector3.Distance(gameObject.transform.localPosition, targetPos) > 1f)
        {            
            if(Vector3.Distance(gameObject.transform.localPosition, targetPos) < 39f)
            {
                //목표블럭이있는지?
                /*
                if (PosHelper.GetBoardSreeen(inX,inY) != null  && PosHelper.GetBoardSreeen(inX, inY).BoardOnCrack.Count != 0)
                {
                    GameManager.instance.StageFail();
                    gameOverLava = true;
                    yield break;
                }
                */
                if (gameOverLava)
                {
                    //GameManager.instance.StageFail();
                    yield break;
                }

            }
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, targetPos, Global.deltaTimePuzzle * 350f);
            yield return null;
        }

        gameObject.transform.localPosition = PosHelper.GetPosByIndex(inX, inY);

        //위에 석판 경고
        Board tempBoard = PosHelper.GetBoard(inX, inY, 0, -1);
        if (tempBoard != null && tempBoard.BoardOnCrack.Count != 0)
        {
            foreach (var tempDeco in tempBoard.BoardOnCrack)
            {
                Crack tempCrack = tempDeco as Crack;
                tempCrack.OnLavaWarring();
            }
        }

        yield return null;
    }

    
    public void SetIceLave(bool enable)
    {
        if (enable)
        {
            uiSprite.spriteName = "ingame_lava_splash_Ice";
            BGSprite.spriteName = "ingame_lava_splash_frame_Ice";
            uiSprite.type = UIBasicSprite.Type.Simple;
            uiSprite.customFill.lavaRatioV = 0;
        }
        else
        {
            uiSprite.spriteName = "ingame_lava_splash";
            BGSprite.spriteName = "ingame_lava_splash_frame";
            uiSprite.color = Color.white;
            BGSprite.color = Color.white;

            uiSprite.blockSpriteType = CustomFill.BlockSpriteType.Lava;
        }
    }

    public override IEnumerator CoFlashDeco_Color(int actionCount, float actionTIme, float waitTime)
    {
        float targetColorValue = 0.6f;
        Color targetColor = new Color(targetColorValue, targetColorValue, targetColorValue);

        float aTime = (actionTIme / actionCount) * 0.5f;
        int count = (actionCount * 2);

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.To(() => uiSprite.color, x => uiSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            DOTween.To(() => BGSprite.color, x => BGSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);

            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        uiSprite.color = Color.white;
        BGSprite.color = Color.white;
        yield return null;
    }
}
