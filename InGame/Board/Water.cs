using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : DecoBase, INet //IDisturb,
{
    public UISprite BGSprite;
    public UISprite CoverSprite;
    public GameObject rootObj;

    int startDir = 0;

    public override void SetSprite()
    {
        SetMainSpriteDepth();
    }

    public override void SetMainSpriteDepth()
    {
        uiSprite.depth = (int)GimmickDepth.DECO_UNDER + 1;
        BGSprite.depth = (int)GimmickDepth.DECO_UNDER;
        CoverSprite.depth = (int)GimmickDepth.BLOCK_LAND;
    }

    public void SetDir(int dir)
    {
        MakePixelPerfect(uiSprite);
        MakePixelPerfect(BGSprite);
        MakePixelPerfect(CoverSprite);

        uiSprite.color = new Color(1, 1, 1, 0);
        BGSprite.color = new Color(1, 1, 1, 0);
        CoverSprite.color = new Color(1, 1, 1, 0);

        SetMainSpriteDepth();

        StartCoroutine(MakeWater(dir));
    }

    IEnumerator MakeWater(int moveDirection = 0)
    {
        Vector3 startPos = Vector3.zero;

        if(moveDirection == 1)
        {
            startPos = new Vector3(0, ManagerBlock.BLOCK_SIZE, 0);
        }
        else if (moveDirection == 3)
        {
            startPos = new Vector3(0, -ManagerBlock.BLOCK_SIZE, 0);
        }
        else if (moveDirection == 2)
        {
            startPos = new Vector3(-ManagerBlock.BLOCK_SIZE,0, 0);
        }
        else if (moveDirection == 4)
        {
            startPos = new Vector3(ManagerBlock.BLOCK_SIZE,0, 0);
        }

        //편집 모드는 물 애니메이션없이 실행
        if (GameManager.instance.state == GameState.EDIT)
        {
            uiSprite.color = new Color(1, 1, 1, 1);
            BGSprite.color = new Color(1, 1, 1, 1);
            CoverSprite.color = new Color(1, 1, 1, 1);
            rootObj.transform.localPosition = Vector3.zero;
            yield break;
        }

        rootObj.transform.localPosition = startPos;
        yield return null;      


        float timer = 0f;
        while (timer < 1f)
        {
            uiSprite.color = new Color(1,1,1, timer);
            BGSprite.color = new Color(1, 1, 1, timer);
            CoverSprite.color = new Color(1, 1, 1, timer);

            rootObj.transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, (1 - Mathf.Cos(timer * Mathf.PI)) * 0.25f + 0.5f);//   (1-Mathf.Cos(timer*Mathf.PI))*0.5f);   //Mathf.Sin(timer * ManagerBlock.PI90)
            timer += Global.deltaTimePuzzle * 3f;
            yield return null;
        }

        rootObj.transform.localPosition = Vector3.zero;
        yield return null;
    }

    public override void SetSpriteEnabled(bool setEnabled)
    {
        uiSprite.enabled = setEnabled;
        BGSprite.enabled = setEnabled;
        CoverSprite.enabled = setEnabled;
    }

    public void SetNetPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if (pangIndex != uniquePang)
        {
            pangIndex = uniquePang;
            lifeCount--;

            if (lifeCount <= 0)
            {
                ManagerBlock.instance.GetWater = true;
                pang = true;
               // ManagerBlock.instance.AddScore(80);
               // InGameEffectMaker.instance.MakeScore(transform.position, 80);
                StartCoroutine(CoPangWaterFinal());
                ManagerSound.AudioPlayMany(AudioInGame.WATER_PANG);
                InGameEffectMaker.instance.MakeWaterPangEffect(transform.position);

                if (board.Block != null)
                {
                    board.Block._transform.localPosition = PosHelper.GetPosByIndex(inX, inY);
                    if (board.Block.IsStopEvnetAtDestroyWater())
                        board.Block.isStopEvent = true;
                }
            }
            else
            {
                InGameEffectMaker.instance.MakeRope(transform.position);
                StartCoroutine(CoPang());
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


            if (lifeCount <= 0)
            {
                ManagerBlock.instance.GetWater = true;
                pang = true;
                //ManagerBlock.instance.AddScore(80);
                //InGameEffectMaker.instance.MakeScore(transform.position, 80);
                StartCoroutine(CoPangWaterFinal());
                ManagerSound.AudioPlayMany(AudioInGame.WATER_PANG);
                InGameEffectMaker.instance.MakeWaterPangEffect(transform.position);

                if (board.Block != null)
                {
                    board.Block._transform.localPosition = PosHelper.GetPosByIndex(inX, inY);
                    if (board.Block.IsStopEvnetAtDestroyWater())
                        board.Block.isStopEvent = true;
                }
            }
            else
            {
                //InGameEffectMaker.instance.MakeRope(transform.position);
                StartCoroutine(CoPang());
            }
        }

        return true;
    }

    /*
    public bool IsLinkable()
    {
        return false;
    }
    

    public bool IsDisturbMove()
    {
        return true;
    }
    */

    public bool IsNetDeco()
    {
        return true;
    }

    public override bool IsWarpBlock()
    {
        return true;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override void RemoveDeco()
    {
        ManagerBlock.instance.listWater.Remove(this);
        base.RemoveDeco();        
    }

    public void SetPang()
    {

    }

    bool pang = false;

    public void Update()
    {
        if (board.Block != null && pang == false)
        {
            board.Block._transform.localPosition = PosHelper.GetPosByIndex(inX, inY) + new Vector3(0, 2.5f, 0) * Mathf.Sin((ManagerBlock.instance.BlockTime) * ManagerBlock.PI90);              //((Mathf.Sin((ManagerBlock.instance.BlockTime )*2.5f)+1));//ManagerBlock.instance.BlockTime + inX + inY
            if (board.Block.bombType != BlockBombType.NONE) board.Block._transform.localPosition += new Vector3(0, 2f, 0);

        }
    }

    IEnumerator CoPangWaterFinal()
    {
        float timer = 0f;

        while (timer < 1f)
        {
            rootObj.transform.localScale = Vector3.one * ( 1 + ( ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer) - 1)*0.5f);
            timer += Global.deltaTimePuzzle * 3f;

            uiSprite.color = new Color(1, 1, 1, 1-timer);
            BGSprite.color = new Color(1, 1, 1, 1-timer);
            CoverSprite.color = new Color(1, 1, 1, 1-timer);
            yield return null;
        }

        RemoveDeco();
        yield return null;
    }

}
