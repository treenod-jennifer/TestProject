using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Net : DecoBase, INet
{
    public static int NetCount = 0;

    const string SPRITE_NAME = "netPlants";

    public override void SetSprite()
    {
        uiSprite.spriteName = SPRITE_NAME + lifeCount;
        uiSprite.depth = (int)GimmickDepth.DECO_CATCH;    //inY * ManagerBlock.BLOCK_SRPRITE_DEPTH_COUNT + ManagerBlock.BLOCK_SRPRITE_MIN + ManagerBlock.NET_RPRITE_MIN;
        MakePixelPerfect(uiSprite);
    }

    public bool IsNetDeco()
    {
        return true;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override void RemoveDeco()
    {
        base.RemoveDeco();
    }

    public override IEnumerator CoPangFinal()
    {
        InGameEffectMaker.instance.MakeRope(transform.position);
        StartCoroutine(CoStartShake());

        ManagerBlock.instance.AddScore(80);
        InGameEffectMaker.instance.MakeScore(transform.position, 80);

        StartCoroutine(CoPangNetFinal());
        ManagerSound.AudioPlayMany(AudioInGame.NET_PANG);

        if (board.Block != null)
        {
            if (board.Block.IsStopEvnetAtDestroyNet())
                board.Block.isStopEvent = true;
        }

        yield return null;
    }

    public bool isPang = false;

    public void SetNetPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if (pangIndex != uniquePang)
        {
            pangIndex = uniquePang;
            lifeCount--;
            InGameEffectMaker.instance.MakeRope(transform.position);
            StartCoroutine(CoStartShake());
            //StartCoroutine(CoPang());   

            if (lifeCount <= 0 && isPang == false)
            {
                isPang = true;
                NetCount++;

                ManagerBlock.instance.AddScore(80);
                InGameEffectMaker.instance.MakeScore(transform.position, 80);

                StartCoroutine(CoPangNetFinal());
                //InGameEffectMaker.instance.MakeRope(transform.position);
                ManagerSound.AudioPlayMany(AudioInGame.NET_PANG);

                if (board.Block != null)
                {
                    if (board.Block.IsStopEvnetAtDestroyNet())
                        board.Block.isStopEvent = true;
                }
            }
        }
    }

    public IEnumerator CoPangNetFinal()
    {
        float timer = 0f;

        while (timer < 1f)
        {
            uiSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            uiSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        NetCount--;

        board.RemoveDeco(this);
        RemoveDeco();
        yield return null;
    }




    float netTimer = 0;

    IEnumerator CoStartShake()
    {

        netTimer = 0;
        while (netTimer < 4f)
        {
            netTimer += Global.deltaTimePuzzle*4;

            uiSprite.customFill.ropeRatio = 0.35f * Mathf.Cos(2* netTimer * 2 * ManagerBlock.PI90) / Mathf.Exp(netTimer);
            yield return null;
        }
        netTimer = 0;

    }

}
