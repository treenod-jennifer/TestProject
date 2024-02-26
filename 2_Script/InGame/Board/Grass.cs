using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : DecoBase, IGrass
{
    const string SPRITE_NAME = "Grass_";

    public UISprite BGSprite;

    public override void SetSprite()
    {
        uiSprite.spriteName = SPRITE_NAME + lifeCount;
        BGSprite.spriteName = SPRITE_NAME + lifeCount + "_BG";
        MakePixelPerfect(BGSprite);

        uiSprite.depth = (int)GimmickDepth.DECO_FIELD;
        BGSprite.depth = uiSprite.depth + 1;
        MakePixelPerfect(uiSprite);

        Board board = PosHelper.GetBoard(inX, inY);
        if (board != null && board.Block != null)
        {
            if (board.Block.IsThisBlockHasPlace() == true)
            {
                uiSprite.depth = (int)GimmickDepth.DECO_LAND;
                BGSprite.depth = (int)GimmickDepth.DECO_LAND;
            }
            else
            {
                uiSprite.depth = (int)GimmickDepth.DECO_FIELD;
                BGSprite.depth = (int)GimmickDepth.DECO_FIELD;
            }
        }
    }

    public override bool IsCoverStatue() //석상위에 깔리는 데코인지.
    {
        return true;
    }

    public void SetCrassPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if (pangIndex != uniquePang)
        {
            InGameEffectMaker.instance.MakeGrass(transform.position);
            //TODO 사운드추가
            ManagerSound.AudioPlayMany(AudioInGame.BREAK_WALK_BLOCK_1);//NET_PANG

            pangIndex = uniquePang;
            lifeCount--;
            //애니메이션
            //SetSprite();

            if (lifeCount <= 0)
            {
                board.RemoveDeco(this);
                //목표제거
                //아래에 석상있는지 체크후 석상작동
                board.CheckStatus();

                RemoveDeco();
            }
        }
    }

    public override void RemoveDeco()
    {
        ManagerBlock.instance.AddScore(80);
        InGameEffectMaker.instance.MakeScore(transform.position, 80);

        _listBoardDeco.Remove(this);
        ManagerBlock.instance.listObject.Remove(gameObject);
        Destroy(gameObject);
        Destroy(this);
    }

}
