using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFire : BlockBase
{
    int fireCount = 0;
    bool getBlock = false;

    public override bool IsCanPang()
    {
        return true;
    }

    public override void UpdateSpriteByBlockType()
    { 
        if(fireCount > 0)
            mainSprite.spriteName = "BlockFire" + fireCount + "_0";
        else
            mainSprite.spriteName = "BlockFire" + fireCount; 

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
    }

    public override void UpdateBlock()
    {
        if (fireCount > 0)
        {
            int spriteNumber = ((int)(ManagerBlock.instance.BlockTime * 5f) % 2);
            mainSprite.spriteName = "BlockFire" + fireCount + "_" + spriteNumber;
        }

        _waitCount++;
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (mBombEffect) return;
        if (state == BlockState.PANG) return;
        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;
        Pang(splashColorType, true);
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        isSkipDistroy = true;

        fireCount++;

        if (fireCount > 2)
        {
            fireCount = 2;
            StartCoroutine(CoFinalPang());

            if (getBlock == false)
            {
                getBlock = true;
                //ManagerBlock.instance.pangBlockCount[(int)TARGET_TYPE.FIRE]++;
                //GameUIManager.instance.RefreshTarget(TARGET_TYPE.FIRE);

                DestroyBlockData();
            }
        }
        else
        {
            StartCoroutine(CoPang());
        }
        return;
    }

    IEnumerator CoPang()
    {
        //랜덤해서 날아가기      
        BlockBase lineBlock = PosHelper.GetRandomBlock();
        if (lineBlock != null)
        {
            //기존거 지우고 //바꾸기

        }

        yield return null;
    }

    IEnumerator CoFinalPang()
    {
        InGameEffectMaker.instance.MakeEffectPlant(_transform.position);

        float timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        UpdateSpriteByBlockType();

        if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0) != null
            && PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.BlockPang(pangIndex, colorType, true);

        if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0) != null
             && PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.BlockPang(pangIndex, colorType, true);

        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1) != null
             && PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.BlockPang(pangIndex, colorType, true);

        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1) != null
             && PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.BlockPang(pangIndex, colorType, true);




        if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 1) != null
    && PosHelper.GetBoardSreeen(indexX, indexY, 1, 1).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, 1, 1).Block.BlockPang(pangIndex, colorType, true);

        if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 1) != null
             && PosHelper.GetBoardSreeen(indexX, indexY, -1, 1).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, -1, 1).Block.BlockPang(pangIndex, colorType, true);

        if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 1) != null
             && PosHelper.GetBoardSreeen(indexX, indexY, -1, 1).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, -1, 1).Block.BlockPang(pangIndex, colorType, true);

        if (PosHelper.GetBoardSreeen(indexX, indexY, -1, -1) != null
             && PosHelper.GetBoardSreeen(indexX, indexY, -1, -1).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, -1, -1).Block.BlockPang(pangIndex, colorType, true);




        //BlockMaker.instance.MakeBombBlock(this, indexX, indexY, BlockBombType.CLEAR_BOMB, BlockColorType.NONE, false, true);

        /*
        float waitTimer = 0;
        List<BlockBase> listBlock = new List<BlockBase>();
        listBlock.Clear();

        for (int y = GameManager.MinScreenY; y < GameManager.MaxScreenY; y++)
        {
            for (int x = GameManager.MinScreenX; x < GameManager.MaxScreenX; x++)
            {
                BlockBase block = PosHelper.GetBlockScreen(x, y);
                if (block != null && block.IsNormalBlock() && block.state == BlockState.WAIT)
                {
                    listBlock.Add(block);
                }
            }
        }

        for(int i=0; i < 3; i++)
        {
            if (listBlock.Count == 0 && ManagerBlock.instance.checkBlockWait())
            {
                break;
            }
            else if (listBlock.Count > 0)
            {
                int rand = Random.Range(0, listBlock.Count);
                BlockBase block = listBlock[rand];
                listBlock.RemoveAt(rand);

                if (block != null)
                {
                    InGameEffectMaker.instance.MakeFlyBlock(BlockType.BLOCK_FIRE, block, _transform.localPosition, true);
                    //block.BlockPang();
                }
                yield return null;

                waitTimer = 0;
                while (waitTimer < 0.1f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }
        }
        */

        timer = 0f;
        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleIn.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 5f;
            yield return null;
        }

        MakePixelPerfect(mainSprite);
        mainSprite.transform.localScale = Vector3.one;

        state = BlockState.WAIT;
        yield return null;
    }
}
