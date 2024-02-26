using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block2X2 : BlockBase
{
    public List<Board> blockBoardList = new List<Board>();

    public override bool IsPangAffectBoard()
    {
        return true;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsCanMove()
    {
        return false;
    }

    public void initBlock()
    {
        Vector3 centerPos = new Vector3();
        foreach (var tempBoard in blockBoardList)
        {
            centerPos += PosHelper.GetPosByIndex(tempBoard.indexX, tempBoard.indexY);
        }
        centerPos = centerPos * 0.25f;

        _transform.localPosition = centerPos;

        _transform.localScale = Vector3.one * 2f;
        UpdateSpriteByBlockType();
    }

    public override void UpdateSpriteByBlockType()
    {
        mainSprite.spriteName = GetSpriteName();
        SetMainSpriteDepth();
        //사이즈2배 //위치잡기
    }

    public override string GetSpriteName()
    {
        return "block" + GetColorTypeString();
    }

    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        //목표일때 날리기
        //슬라임날리기, 잡기돌날리기    


        foreach(var tempBoard in blockBoardList)
        {
            tempBoard.BoardPang(pangIndex, pangColorType);
        }

        //Set2x2SplashEffect(colorType, bombEffect);   //팡인덱스가 십만보다 크면 폭탄에 의한 폭발 //사이드이펙트주기     

    }

    public override void PangDestroyBoardData()
    {
        foreach (var tempBoard in blockBoardList)
        {
            tempBoard.Block = null;
            tempBoard.TempBlock = null;
        }

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);
    }

     int checkWaitFrame = 0;
    

    public override void UpdateBlock()
    {
        _stateTimer += Global.deltaTimePuzzle;

        switch (state)
        {
            case BlockState.WAIT:

                if(checkWaitFrame != Time.frameCount)
                {
                    checkWaitFrame = Time.frameCount;
                    _waitCount++;
                }

                break;
            case BlockState.PANG:

                if (!IsSkipPang && !isSkipDistroy)
                {
                    if (_stateTimer >= _pangRemoveDelay)
                    {
                        PangDestroyBoardData();
                    }

                    if (_stateTimer >= _pangAlphaDelay)
                    {
                        if (_pangGatherEffect != Vector3.zero)
                        {
                            float ratio = Mathf.Cos(ManagerBlock.PI90 * ((_stateTimer - _pangAlphaDelay) * 1.5f) / 0.4f);
                            _transform.localPosition = Vector3.Lerp(_stageBeforePos, _pangGatherEffect, ((_stateTimer - _pangAlphaDelay) * 1.5f) / 0.4f);

                            if ((_stateTimer - _pangAlphaDelay) * 1.5f / 0.4f > 0.5f)
                            {
                                mainSprite.color = new Color(1f, 1f, 1f,
                                    1.5f - (_stateTimer - _pangAlphaDelay) * 1.5f / 0.4f * 1.5f);
                            }

                            mainSprite.spriteName = GetSpriteName() + "_glow";
                            MakePixelPerfect(mainSprite);
                        }
                        else
                        {
                            float ratio = Mathf.Cos(Mathf.PI * 0.5f * (_stateTimer - _pangAlphaDelay) / 0.4f);
                            mainSprite.color = new Color(1f, 1f, 1f, ratio);
                        }
                    }
                }
                break;
        }
    }

    public void Set2x2SplashEffect(BlockColorType colorType = BlockColorType.NONE, bool takeEffectbomb = false)
    {
        if (PosHelper.GetBoardSreeen(indexX, indexY, 1, 0) != null
             && !PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).HasDecoHideBlock()
             && !PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).HasDecoCoverBlock()
             && PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, 1, 0).Block.SetSplashPang(pangIndex, colorType, takeEffectbomb);

        if (PosHelper.GetBoardSreeen(indexX, indexY, -1, 0) != null
             && !PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).HasDecoHideBlock()
             && !PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).HasDecoCoverBlock()
             && PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, -1, 0).Block.SetSplashPang(pangIndex, colorType, takeEffectbomb);

        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, 1) != null
             && !PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).HasDecoHideBlock() 
             && !PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).HasDecoCoverBlock()
             && PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, 0, 1).Block.SetSplashPang(pangIndex, colorType, takeEffectbomb);

        if (PosHelper.GetBoardSreeen(indexX, indexY, 0, -1) != null
             && !PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).HasDecoHideBlock() 
             && !PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).HasDecoCoverBlock()
             && PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block != null)
            PosHelper.GetBoardSreeen(indexX, indexY, 0, -1).Block.SetSplashPang(pangIndex, colorType, takeEffectbomb);
    }

}
