using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STONE_TYPE
{
    NORMAL,     //0
    APPLE,      //1
    KEY,        //2
    LINE,       //3
    CIRCLE,     //4
    CANDY,               //5
    LINE_V,     //6
    LINE_H,     //7
    WATER,      //8

    HealPotion,     //9
    SkillPotion,    //10

    SPACESHIP,
}

public class BlockStone : BlockBase {

    public UISprite plantPlace_Sprite;

    public STONE_TYPE stoneType = STONE_TYPE.NORMAL;
    public int appleCount = 0;

    public override bool IsBlockType()
    {
        return false;
    }

    public override bool IsCanPang()
    {
        return true;
    }

    public override bool IsCanMove()
    {
        return false;
    }

    public override bool IsCoverStatue() //석상위에 깔리는 블럭인지.
    {
        return true;
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }

    public override bool IsThisBlockHasPlace()
    {
        return true;
    }

    void Start()
    {
        Board back = PosHelper.GetBoard(indexX, indexY);
        if (back.IsActiveBoard)
        {
            plantPlace_Sprite.spriteName = "plantPlace";
            plantPlace_Sprite.depth = (int)GimmickDepth.DECO_GROUND;
            MakePixelPerfect(plantPlace_Sprite);
        }
        else
        {
            Destroy(plantPlace_Sprite.gameObject);
        }
    }

    public override bool IsPangExtendable()
    {
        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null && back.HasDecoHideBlock() == true)
        {   //바위 상단을 장막 기믹이 막고 있으면 폭탄효과를 막지 않음
            return true;
        }
        return false;
    }

    public override void UpdateSpriteByBlockType()
    {
        switch (stoneType)
        {
            case STONE_TYPE.NORMAL:
            case STONE_TYPE.WATER:
            case STONE_TYPE.CANDY:
                mainSprite.spriteName = "blockStone" + lifeCount;
                break;
            case STONE_TYPE.APPLE:
                mainSprite.spriteName = "blockStone" + lifeCount + "_Apple";
                break;
            case STONE_TYPE.KEY:
                mainSprite.spriteName = "blockStone" + lifeCount + "_Key";
                break;
            case STONE_TYPE.LINE:
            case STONE_TYPE.LINE_V:
            case STONE_TYPE.LINE_H:
                mainSprite.spriteName = "blockStone" + lifeCount + "_Line";
                break;
            case STONE_TYPE.CIRCLE:
                mainSprite.spriteName = "blockStone" + lifeCount + "_Circle";
                break;

            case STONE_TYPE.HealPotion:
                mainSprite.spriteName = "blockStone" + lifeCount + "_HealPotion";
                break;
            case STONE_TYPE.SkillPotion:
                mainSprite.spriteName = "blockStone" + lifeCount + "_SkillPotion";
                break;
            case STONE_TYPE.SPACESHIP:
                mainSprite.spriteName = "blockStone" + lifeCount + "_Spider";
                break;
        }

        SetMainSpriteDepth();
        MakePixelPerfect(mainSprite);
    }

    public override void SetSplashPang(int uniqueIndex, BlockColorType splashColorType = BlockColorType.NONE, bool mBombEffect = false, int effectDir = 0, bool byRainbow = false)
    {
        if (lifeCount == 2) return;
        if (mBombEffect) return;
        if (state == BlockState.PANG) return;
        if (pangIndex == uniqueIndex) return;

        pangIndex = uniqueIndex;
        Pang(splashColorType, true);
    }
    
    public override void Pang(BlockColorType pangColorType = BlockColorType.NONE, bool PangByBomb = false)
    {
        if (!PangByBomb)
        {
            state = BlockState.WAIT;
            return;
        }

        lifeCount--;

        InGameEffectMaker.instance.MakeRockEffect(_transform.position);

        if (lifeCount <= 0)
        {
            _pangRemoveDelay = 0.3f;

            IsSkipPang = true;
            state = BlockState.PANG;
            StartCoroutine(CoPangFinal());
            ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG1);
        }
        else
        {
            state = BlockState.WAIT;
            StartCoroutine(CoPang());
            if (lifeCount > 1) ManagerSound.AudioPlay(AudioInGame.PLANT_PANG3);
            else ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG2);
        }
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
        mainSprite.transform.localScale = Vector3.one;
    }


    IEnumerator CoPangFinal()
    {
        //이펙트
        Board back = PosHelper.GetBoardSreeen(indexX, indexY);
        if (back != null) back.SetUnderBoard();

        ManagerBlock.boards[indexX, indexY].CheckCarpetByPlant();

        if (plantPlace_Sprite != null)
            Destroy(plantPlace_Sprite.gameObject);

        float timer = 0f;

        while (timer < 1f)
        {
            mainSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 4f;
            mainSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        switch (stoneType)
        {
            case STONE_TYPE.NORMAL:
                ManagerBlock.boards[indexX, indexY].Block = null;
                ManagerBlock.boards[indexX, indexY].TempBlock = null;
                break;
            case STONE_TYPE.APPLE:
                BlockBase blockApple = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.APPLE, BlockColorType.NONE, appleCount);
                ManagerBlock.boards[indexX, indexY].Block = blockApple;
                break;
            case STONE_TYPE.CANDY:
                BlockBase blockCandy = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.CANDY, BlockColorType.NONE);
                ManagerBlock.boards[indexX, indexY].Block = blockCandy;
                break;
            case STONE_TYPE.KEY:
                BlockBase blockKey = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.KEY, BlockColorType.NONE);
                ManagerBlock.boards[indexX, indexY].Block = blockKey;

                ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.KEY]--;
                ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.KEY]--;
                break;
            case STONE_TYPE.LINE:
                NormalBlock lineBlock = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
                ManagerBlock.boards[indexX, indexY].Block = lineBlock;
                int randomLine = GameManager.instance.GetIngameRandom(0, 2);
                lineBlock.bombType = randomLine == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H;
                lineBlock.makeLineBomb = true;
                lineBlock.InitBlock(type);
                break;
            case STONE_TYPE.LINE_V:
                NormalBlock lineBlockV = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
                ManagerBlock.boards[indexX, indexY].Block = lineBlockV;
                lineBlockV.bombType = BlockBombType.LINE_V;
                lineBlockV.makeLineBomb = true;
                lineBlockV.InitBlock(type);
                break;
            case STONE_TYPE.LINE_H:
                NormalBlock lineBlockH = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
                ManagerBlock.boards[indexX, indexY].Block = lineBlockH;
                lineBlockH.bombType =  BlockBombType.LINE_H;
                lineBlockH.makeLineBomb = true;
                lineBlockH.InitBlock(type);
                break;
            case STONE_TYPE.CIRCLE:
                NormalBlock bombBlock = (NormalBlock)BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.NORMAL, BlockColorType.NONE);
                ManagerBlock.boards[indexX, indexY].Block = bombBlock;
                bombBlock.bombType = BlockBombType.BOMB;
                bombBlock.makeLineBomb = true;
                bombBlock.InitBlock(type);
                break;
            case STONE_TYPE.WATER:
                DecoInfo decoWater = new DecoInfo();
                decoWater.BoardType = (int)BoardDecoType.WATER;
                DecoBase boardDeco = BoardDecoMaker.instance.MakeBoardDeco(ManagerBlock.boards[indexX, indexY], indexX, indexY, decoWater);
                ManagerBlock.instance.GetWater = true;
                ManagerSound.AudioPlayMany(AudioInGame.WATER_MAKE);
                break;

            case STONE_TYPE.HealPotion:
                BlockBase blockHealPotion = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.ADVENTURE_POTION_HEAL, BlockColorType.NONE, 1);
                ManagerBlock.boards[indexX, indexY].Block = blockHealPotion;
                ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ADVENTURE_POTION_HEAL]--;
                ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ADVENTURE_POTION_HEAL]--;
                break;
            case STONE_TYPE.SkillPotion:
                BlockBase blockSkillPotion = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.ADVENTURE_POTION_SKILL, BlockColorType.NONE, 1);
                ManagerBlock.boards[indexX, indexY].Block = blockSkillPotion;
                ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ADVENTURE_POTION_SKILL]--;
                ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ADVENTURE_POTION_SKILL]--;
                break;
            case STONE_TYPE.SPACESHIP:
                BlockBase blockSpaceship = BlockMaker.instance.MakeBlockBase(indexX, indexY, BlockType.SPACESHIP, BlockColorType.NONE);
                ManagerBlock.boards[indexX, indexY].Block = blockSpaceship;
                ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.SPACESHIP]--;
                ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.SPACESHIP]--;
                blockSpaceship.isMoveDownThisTurn = true;
                break;
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

        _listBlock.Remove(this);
        _listBlockTemp.Remove(this);
        BlockMaker.instance.RemoveBlock(this);

        //아래에 석상있는지 체크후 석상작동
        if (back != null)
            back.CheckStatus();

        yield return null;
    }

    public override IEnumerator CoPangImmediately()
    {
        lifeCount = 0;
        InGameEffectMaker.instance.MakeRockEffect(_transform.position);
        yield return CoPangFinal();
    }
}
