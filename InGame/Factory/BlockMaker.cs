using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMaker : MonoSingletonOnlyScene<BlockMaker>
{
    //블럭의 전체 생성확률
    public List<BlockColorType> listBlockProb = new List<BlockColorType>();
    public List<BlockColorType> listBlockProb2 = new List<BlockColorType>();
    public List<BlockColorType> listBlockProb3 = new List<BlockColorType>();

    //맵이 구성될 때 블럭 생성 확률
    public List<BlockColorType> listBlockProbMapMake = new List<BlockColorType>();

    //오브젝트풀
    public GameObject NormalBlockObj;
    public GameObject PlantBlockObj;
    public GameObject BombBlockObj;
    public GameObject KeyBlockObj;
    public GameObject AppleBlockObj;
    public GameObject GroundObj;
    public GameObject BoxObj;
    public GameObject block2X2Obj;
    public GameObject StoneObj;
    public GameObject plant2X2Obj;
    public GameObject IceAppleObj;
    public GameObject CandyObj;
    public GameObject BlackObj;
    public GameObject FireObj;
    public GameObject EventObj;
    public GameObject DynamiteObj;
    public GameObject EventStoneObj;
    public GameObject BlockCoinObj;
    public GameObject AdventurePotion_HealObj;
    public GameObject AdventurePotion_SkillObj;
    public GameObject BlockColorBigJewelObj;
    public GameObject ColorFlowerPotLittleObj;
    public GameObject FireWorkObj;
    public GameObject SodaJellyObj;
    public GameObject PeaObj;
    public GameObject PeaBossObj;
    public GameObject WorldRankItemObj;
    public GameObject EventGroundObj;
    public GameObject HeartObj;
    public GameObject HeartHomeObj;
    public GameObject PaintObj;
    public GameObject BreadObj;
    public GameObject WaterBombCountObj;
    public GameObject EndContentsItemObj;
    public GameObject CannonItemObj;

    public GameObject BlockLabelObj;                //라벨
    public GameObject BlockFrameSpriteObj;                //프레임 스프라이트

    //기타블럭관련 오브젝트
    public GameObject blockTempBombLabel;
    //블럭더미
    public GameObject blockDummySpriteObj;    
    public GameObject blockLinkerObject;         //링커    
    public GameObject blockGroundObject;        //블럭고정용

    //카펫 오브젝트
    public GameObject blockCarpetSpriteObj;

    //블럭코인
    public GameObject blockCoinObj;

    //스페셜이벤트
    public GameObject urlImageObj;

    //알파벳 이벤트
    public GameObject alphabetEventObj;

    //우주선
    public GameObject SpaceShipObj;

    public BlockBase MakeBlockBase(int indexX, int indexY, BlockType blockType, BlockColorType blockColorType, int count = 0, int index = 0, int subType = 0, List<DecoInfo> decoList = null)
    {
        BlockBase block;
        Board board = ManagerBlock.boards[indexX, indexY];

        switch (blockType)
        {
            case BlockType.BLOCK_DYNAMITE:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, DynamiteObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                BlockDynamite blockDynamite = block as BlockDynamite;
                blockDynamite.DynamiteCount = index;
                blockDynamite.CheckDynamiteCount();
                break;
            case BlockType.BLOCK_EVENT:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, EventObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.BLOCK_EVENT_STONE:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, EventStoneObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.FIRE_WORK:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, FireWorkObj).GetComponent<BlockBase>();
                if (count < 1)
                    count = 1;
                else if (count > 2)
                    count = 2;
                block.lifeCount = count;
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.ADVENTURE_POTION_HEAL:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, AdventurePotion_HealObj).GetComponent<BlockBase>();
                if (count == 0)
                    block.lifeCount = 2;
                else
                    block.lifeCount = count;
                break;
            case BlockType.ADVENTURE_POTION_SKILL:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, AdventurePotion_SkillObj).GetComponent<BlockBase>();
                if (count == 0)
                    block.lifeCount = 2;
                else
                    block.lifeCount = count;
                break;
            case BlockType.NORMAL:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, NormalBlockObj).GetComponent<BlockBase>();
                if (ManagerBlock.instance.stageInfo.isCanMakeBonusBombAtColor == false)
                {
                    NormalBlock blockNormal = block as NormalBlock;
                    blockNormal.isCanMakeBonusBombBlock = (blockColorType == BlockColorType.RANDOM) ? true : false;
                }
                break;
            case BlockType.BLOCK_FIRE:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, FireObj).GetComponent<BlockBase>();
                break;
            case BlockType.BLOCK_BLACK:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, BlackObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                break;
            case BlockType.PLANT2X2:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, plant2X2Obj).GetComponent<BlockBase>();
                block.lifeCount = count;
                Plant2X2 plant2X2 = block as Plant2X2;
                plant2X2.index = index;
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.ColorBigJewel:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, BlockColorBigJewelObj).GetComponent<BlockBase>();
                BlockColorBigJewel colorbigJewel = block as BlockColorBigJewel;
                colorbigJewel.index = index;
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.LITTLE_FLOWER_POT:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, ColorFlowerPotLittleObj).GetComponent<BlockBase>();
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.SODAJELLY:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, SodaJellyObj).GetComponent<BlockBase>();
                BlockSodaJelly blockSodaJelly = block as BlockSodaJelly;
                blockSodaJelly.InitBlock(index, count);
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.PEA:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, PeaObj).GetComponent<BlockBase>();
                BlockPea blockPea = block as BlockPea;
                blockPea.InitBlock(count);
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.PEA_BOSS:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, PeaBossObj).GetComponent<BlockBase>();
                BlockPeaBoss blockPeaBoss = block as BlockPeaBoss;
                blockPeaBoss.InitBlock(count);
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.WORLD_RANK_ITEM:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, WorldRankItemObj).GetComponent<BlockBase>();
                BlockWorldRankItem blockItem = block as BlockWorldRankItem;
                blockItem.InitBlock(count);
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.SPACESHIP:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, SpaceShipObj).GetComponent<BlockBase>();
                BlockSpaceShip blockSpaceship = block as BlockSpaceShip;
                blockSpaceship.InitBlock();
                break;
            case BlockType.GROUND:
            case BlockType.GROUND_JEWEL:
            case BlockType.GROUND_KEY:
            case BlockType.GROUND_BOMB:
            case BlockType.GROUND_ICE_APPLE:
            case BlockType.GROUND_APPLE:
            case BlockType.GROUND_BLOCKBLACK:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, GroundObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                BlockGround NormalGround = block as BlockGround;
                NormalGround.groundType = (GROUND_TYPE)subType;
                NormalGround.appleCount = index;

                if (blockType == BlockType.GROUND_JEWEL)
                {
                    block.lifeCount = count + 1;
                    NormalGround.groundType = GROUND_TYPE.JEWEL;
                }
                else if (blockType == BlockType.GROUND_KEY)
                {
                    NormalGround.groundType = GROUND_TYPE.KEY;
                }
                else if (blockType == BlockType.GROUND_BOMB)
                {
                    if (index == 1) NormalGround.groundType = GROUND_TYPE.CIRCLE;
                    if (index == 2) NormalGround.groundType = GROUND_TYPE.LINE_H;
                    if (index == 3) NormalGround.groundType = GROUND_TYPE.LINE_V;
                }
                else if (blockType == BlockType.GROUND_ICE_APPLE)
                {
                    NormalGround.groundType = GROUND_TYPE.ICE_APPLE;
                }
                else if (blockType == BlockType.GROUND_BLOCKBLACK)
                {
                    subType = (int)GROUND_TYPE.BlOCKBLACK;
                    NormalGround.groundType = GROUND_TYPE.BlOCKBLACK;
                }

                if (subType == (int)GROUND_TYPE.KEY) blockType = BlockType.GROUND_KEY;
                else if (subType == (int)GROUND_TYPE.JEWEL) blockType = BlockType.GROUND_JEWEL;
                else if (subType == (int)GROUND_TYPE.APPLE) blockType = BlockType.GROUND_APPLE;
                else if (subType == (int)GROUND_TYPE.ICE_APPLE) blockType = BlockType.GROUND_ICE_APPLE;
                else if (subType == (int)GROUND_TYPE.DUCK)
                {
                    bool checkGroundDuck1 = false;

                    if (EditManager.instance == null)
                    {
                        if (Global.GameType == GameType.NORMAL)
                        {
                            foreach (var item in ManagerData._instance._questGameData)
                            {
                                if (item.Value.level == GameManager.instance.currentChapter() + 1 && item.Value.type == QuestType.chapter_Duck)
                                {
                                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                                    {
                                        checkGroundDuck1 = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        checkGroundDuck1 = true;
                    }

                    if (checkGroundDuck1 == false)
                    {
                        NormalGround.groundType = (int)GROUND_TYPE.NORMAL;
                        subType = (int)GROUND_TYPE.NORMAL;
                    }
                }
                else if (subType == (int)GROUND_TYPE.CANDY)
                {
                    bool checkGroundCandy1 = false;
                    if (EditManager.instance == null)
                    {
                        if (Global.GameType == GameType.NORMAL)
                        {
                            foreach (var item in ManagerData._instance._questGameData)
                            {
                                if (item.Value.level == GameManager.instance.currentChapter() + 1 && item.Value.type == QuestType.chapter_Candy)
                                {
                                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                                    {
                                        checkGroundCandy1 = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        checkGroundCandy1 = true;
                    }

                    if(checkGroundCandy1 == false)
                    {
                        NormalGround.groundType = (int)GROUND_TYPE.NORMAL;
                        subType = (int)GROUND_TYPE.NORMAL;
                    }
                }
                else if (subType == (int)GROUND_TYPE.BlOCKBLACK) blockType = BlockType.GROUND_BLOCKBLACK;
                else if (subType == (int)GROUND_TYPE.CIRCLE || subType == (int)GROUND_TYPE.LINE_H || subType == (int)GROUND_TYPE.LINE_V || subType == (int)GROUND_TYPE.LINE)
                {
                    blockType = BlockType.GROUND_BOMB;
                }
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.BLOCK_EVENT_GROUND:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, EventGroundObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.BOX:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, BoxObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                break;

            case BlockType.STONE:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, StoneObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                BlockStone stoneBlock = block as BlockStone;

                if (subType == (int)STONE_TYPE.APPLE)stoneBlock.appleCount = index;
                if (subType == (int)STONE_TYPE.KEY)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.KEY]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.KEY]++;
                }
                CheckDownBlockOrDeco(board);

                if (EditManager.instance == null)
                {
                    bool checkMission = false;
                    if (subType == (int)STONE_TYPE.CANDY)
                    {
                        if (Global.GameType == GameType.NORMAL)
                        {
                            foreach (var item in ManagerData._instance._questGameData)
                            {
                                if (item.Value.level == GameManager.instance.currentChapter() + 1 && item.Value.type == QuestType.chapter_Candy)
                                {
                                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                                    {
                                        checkMission = true;
                                    }
                                }
                            }
                        }
                        if (checkMission == false)
                            subType = (int)STONE_TYPE.NORMAL;
                    }
                }
                stoneBlock.stoneType = (STONE_TYPE)subType;

                if (subType == (int)STONE_TYPE.HealPotion)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ADVENTURE_POTION_HEAL]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ADVENTURE_POTION_HEAL]++;
                }
                else if (subType == (int)STONE_TYPE.SkillPotion)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ADVENTURE_POTION_SKILL]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ADVENTURE_POTION_SKILL]++;
                }
                else if(subType == (int)STONE_TYPE.SPACESHIP)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.SPACESHIP]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.SPACESHIP]++;
                }
                else if (subType == (int)STONE_TYPE.CIRCLE)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.START_Bomb]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.START_Bomb]++;
                }
                else if (subType == (int)STONE_TYPE.LINE_H || subType == (int)STONE_TYPE.LINE_V || subType == (int)STONE_TYPE.LINE)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.START_Line]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.START_Line]++;
                }


                break;

            case BlockType.PLANT:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, PlantBlockObj).GetComponent<BlockBase>();
                block.lifeCount = count;
                BlockPlant plantNormal = block as BlockPlant;

                if(EditManager.instance == null)
                {
                    bool checkMission = false;
                    if (subType == (int)PLANT_TYPE.DUCK)
                    {
                        if (Global.GameType == GameType.NORMAL)
                        {
                            foreach (var item in ManagerData._instance._questGameData)
                            {
                                if (item.Value.level == GameManager.instance.currentChapter() + 1 && item.Value.type == QuestType.chapter_Duck)
                                {
                                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                                    {
                                        checkMission = true;
                                    }
                                }
                            }
                        }
                        if (checkMission == false)
                            subType = (int)PLANT_TYPE.NORMAL;
                    }
                    else if (subType == (int)PLANT_TYPE.CANDY)
                    {
                        if (Global.GameType == GameType.NORMAL)
                        {
                            foreach (var item in ManagerData._instance._questGameData)
                            {
                                if (item.Value.level == GameManager.instance.currentChapter() + 1 && item.Value.type == QuestType.chapter_Candy)
                                {
                                    if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                                    {
                                        checkMission = true;
                                    }
                                }
                            }
                        }
                        if (checkMission == false)
                            subType = (int)PLANT_TYPE.NORMAL;
                    }
                }
                CheckDownBlockOrDeco(board);

                plantNormal.plantType = (PLANT_TYPE)subType;
                plantNormal.AppleCount = index;
                if (subType == (int)PLANT_TYPE.KEY)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.KEY]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.KEY]++;
                }
                else if (subType == (int)PLANT_TYPE.HealPotion)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ADVENTURE_POTION_HEAL]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ADVENTURE_POTION_HEAL]++;
                }
                else if (subType == (int)PLANT_TYPE.SkillPotion)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ADVENTURE_POTION_SKILL]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ADVENTURE_POTION_SKILL]++;
                }
                else if (subType == (int)PLANT_TYPE.COIN_BAG)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.COIN_BAG]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.COIN_BAG]++;
                }
                else if (subType == (int)PLANT_TYPE.SPACESHIP)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.SPACESHIP]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.SPACESHIP]++;
                }
                else if (subType == (int)PLANT_TYPE.CIRCLE)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.START_Bomb]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.START_Bomb]++;
                }
                else if (subType == (int)PLANT_TYPE.LINE || subType == (int)PLANT_TYPE.LINE_H || subType == (int)PLANT_TYPE.LINE_V)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.START_Line]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.START_Line]++;
                }
                else if (subType == (int)PLANT_TYPE.APPLE)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.APPLE]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.APPLE]++;
                }
                else if (subType == (int)PLANT_TYPE.ICE_APPLE)
                {
                    ManagerBlock.instance.liveBlockTypeCount[(int)BlockType.ICE_APPLE]++;
                    ManagerBlock.instance.totalCreatBlockTypeCount[(int)BlockType.ICE_APPLE]++;
                }

                break;

            case BlockType.KEY:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, KeyBlockObj).GetComponent<BlockBase>();
                break;

            case BlockType.CANDY:
                bool checkCandy = false;
                if (EditManager.instance == null)
                {
                    if (Global.GameType == GameType.NORMAL)
                    {
                        foreach (var item in ManagerData._instance._questGameData)
                        {
                            if (item.Value.level == GameManager.instance.currentChapter() + 1 && item.Value.type == QuestType.chapter_Candy)
                            {
                                if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                                {
                                    checkCandy = true;
                                }
                            }
                        }                        
                    }
                }
                else
                {
                    checkCandy = true;
                }

                if (checkCandy)
                {
                    block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, CandyObj).GetComponent<BlockBase>();     
                }
                else
                {
                    if (ManagerBlock.hasDeco(decoList, decoType: new[] { BoardDecoType.WATER }))
                        return null;

                    block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, NormalBlockObj).GetComponent<BlockBase>();
                    blockType = BlockType.NORMAL;
                    blockColorType = BlockColorType.RANDOM;
                }
                break;

            case BlockType.DUCK:
                bool checkDuck = false;

                if (EditManager.instance == null)
                {
                    if (Global.GameType == GameType.NORMAL)
                    {
                        foreach (var item in ManagerData._instance._questGameData)
                        {
                            if (item.Value.level == GameManager.instance.currentChapter() + 1 && item.Value.type == QuestType.chapter_Duck)
                            {
                                if (ManagerData._instance._stageData[Global.stageIndex - 1]._missionClear == 0)
                                {
                                    checkDuck = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    checkDuck = true;
                }
                
                if (checkDuck)
                {
                    block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, CandyObj).GetComponent<BlockBase>();
                }
                else
                {
                    if (ManagerBlock.hasDeco(decoList, decoType: new[] { BoardDecoType.WATER }))
                        return null;

                    block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, NormalBlockObj).GetComponent<BlockBase>();
                    blockType = BlockType.NORMAL;
                    blockColorType = BlockColorType.RANDOM;
                }            
                break;

            case BlockType.APPLE:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, AppleBlockObj).GetComponent<BlockBase>();
                BlockApple blockApple = block as BlockApple;
                blockApple.AppleCount = count;
                break;
            case BlockType.ICE_APPLE:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, IceAppleObj).GetComponent<BlockBase>();
                break;

            case BlockType.BLOCK2X2:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, block2X2Obj).GetComponent<BlockBase>();
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.BLOCK_COIN:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, BlockCoinObj).GetComponent<BlockBase>();
                break;
            case BlockType.COIN_BAG:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, BlockCoinObj).GetComponent<BlockBase>();
                BlockCoin blockCoin = block as BlockCoin;
                blockCoin.isCoinBag = true;
                blockCoin.coinCount = count;
                break;
            case BlockType.HEART:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, HeartObj).GetComponent<BlockBase>();
                BlockHeart blockHeart = block as BlockHeart;
                blockHeart.InitBlock(index);
                break;
            case BlockType.HEART_HOME:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, HeartHomeObj).GetComponent<BlockBase>();
                BlockHeartHome blockHeartHome = block as BlockHeartHome;
                blockHeartHome.InitBlock();
                break;
            case BlockType.PAINT:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, PaintObj).GetComponent<BlockBase>();
                block.lifeCount = (count > 1)? 2 : 1;
                BlockPaint blockPaint = block as BlockPaint;
                ManagerBlock.instance.AddInkPaint(blockPaint);
                break;
            case BlockType.BREAD:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, BreadObj).GetComponent<BlockBase>();
                BlockBread blockBread = block as BlockBread;
                blockBread.InitBlock(index);
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.WATERBOMB:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, WaterBombCountObj).GetComponent<BlockBase>();
                block.lifeCount = (count > 1) ? 2 : 1;
                BlockWaterBomb blockWaterBomb = block as BlockWaterBomb;
                blockWaterBomb.timeCount = index;
                ManagerBlock.instance.AddWaterBomb(blockWaterBomb);
                break;
            case BlockType.ENDCONTENTS_ITEM:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, EndContentsItemObj).GetComponent<BlockBase>();
                BlockEndContentsItem blockEndContents = block as BlockEndContentsItem;
                blockEndContents.InitBlock(count);
                CheckDownBlockOrDeco(board);
                break;
            case BlockType.CANNON:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, CannonItemObj).GetComponent<BlockBase>();
                BlockCannon blockCannon = block as BlockCannon;
                blockCannon.targetBlockType = blockColorType == BlockColorType.RANDOM ? GetBlockRandomTypeAtMakeMap() : blockColorType;
                blockCannon.dirType = subType;
                blockCannon.blockCount = count;
                break;
            default:
                block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, NormalBlockObj).GetComponent<BlockBase>();
                break;
        }

        block.indexX = indexX;
        block.indexY = indexY;

        block.colorType = (blockColorType == BlockColorType.RANDOM) ? GetBlockRandomTypeAtMakeMap() : blockColorType;

        if (blockType == BlockType.PLANT)
        {
            if(blockColorType != BlockColorType.C && blockColorType != BlockColorType.E)
            {
                block.colorType = BlockColorType.NONE;
            }
        }

        if (blockType != BlockType.NORMAL &&
            blockType != BlockType.BLOCK2X2 &&
            blockType != BlockType.BOX &&
            blockType != BlockType.PLANT &&
            blockType != BlockType.BLOCK_DYNAMITE &&
            blockType != BlockType.LITTLE_FLOWER_POT &&
            blockType != BlockType.FIRE_WORK &&
            blockType != BlockType.HEART &&
            blockType != BlockType.PAINT
            )
        {
            block.colorType = BlockColorType.NONE;
        }

        block.type = blockType;
        if (blockType == BlockType.PAINT)
        {
            ManagerBlock.instance.AddCreateBlockColorCount(blockType, (int)block.colorType - 1);
        }

        block.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);

        BlockBase._listBlock.Add(block);
        ManagerBlock.instance.listObject.Add(block.gameObject);

        if (ManagerBlock.instance.CheckExcept_AdjustBlock(blockType, subType))
        {
            ManagerBlock.instance.totalCreatBlockTypeCount[(int)blockType]++;
            ManagerBlock.instance.liveBlockTypeCount[(int)blockType]++;
        }

        if (blockType == BlockType.NORMAL && blockColorType == BlockColorType.RANDOM && GameManager.instance.state == GameState.EDIT)
        {
            block.mainSprite.spriteName = "block_Random";
        }

        if (GameManager.instance.state == GameState.EDIT)
        {
            Vector3 labelTr = new Vector3(0f, 25f, 0f);
            if (blockType == BlockType.PLANT)
            {
                if (subType == (int)PLANT_TYPE.CANDY)
                {
                    MakeLabel(block.gameObject, "Candy", labelTr);
                }
                else if (subType == (int)PLANT_TYPE.WATER)
                {
                    labelTr = new Vector3(0f, -25f, 0f);
                    MakeLabel(block.gameObject, "W", labelTr);
                }
                else if (subType == (int)PLANT_TYPE.SPACESHIP)
                {
                    labelTr = new Vector3(0f, -25f, 0f);
                    MakeLabel(block.gameObject, "SS", labelTr);
                }
                else if (subType == (int)PLANT_TYPE.LINE || subType == (int)PLANT_TYPE.LINE_H || subType == (int)PLANT_TYPE.LINE_V || subType == (int)PLANT_TYPE.CIRCLE)
                {
                    string text = "";
                    if ((PLANT_TYPE)subType == PLANT_TYPE.LINE) text = "0";
                    else if ((PLANT_TYPE)subType == PLANT_TYPE.CIRCLE) text = "1";
                    else if ((PLANT_TYPE)subType == PLANT_TYPE.LINE_V) text = "2";
                    else if ((PLANT_TYPE)subType == PLANT_TYPE.LINE_H) text = "3";
                    MakeLabel(block.gameObject, text, labelTr);
                }
                else if (subType == (int)PLANT_TYPE.APPLE)
                {
                    MakeLabel(block.gameObject, index.ToString(), labelTr);
                }
                else if (subType == (int)PLANT_TYPE.COIN_BAG)
                {
                    MakeLabel(block.gameObject, index.ToString(), labelTr);
                }
            }
            else if (blockType == BlockType.STONE)
            {
                if (subType == (int)STONE_TYPE.CANDY)
                {
                    MakeLabel(block.gameObject, "Candy", labelTr);
                }
                else if (subType == (int)STONE_TYPE.WATER)
                {
                    labelTr = new Vector3(0f, -25f, 0f);
                    MakeLabel(block.gameObject, "W", labelTr);
                }
                else if (subType == (int)STONE_TYPE.SPACESHIP)
                {
                    labelTr = new Vector3(0f, -25f, 0f);
                    MakeLabel(block.gameObject, "SS", labelTr);
                }
                else if (subType == (int)STONE_TYPE.LINE || subType == (int)STONE_TYPE.LINE_H || subType == (int)STONE_TYPE.LINE_V || subType == (int)STONE_TYPE.CIRCLE)
                {
                    string text = "";
                    if ((STONE_TYPE)subType == STONE_TYPE.LINE) text = "0";
                    else if ((STONE_TYPE)subType == STONE_TYPE.CIRCLE) text = "1";
                    else if ((STONE_TYPE)subType == STONE_TYPE.LINE_V) text = "2";
                    else if ((STONE_TYPE)subType == STONE_TYPE.LINE_H) text = "3";

                    MakeLabel(block.gameObject, text, labelTr);
                }
                else if (subType == (int)STONE_TYPE.APPLE)
                {
                    MakeLabel(block.gameObject, index.ToString(), labelTr);
                }
            }
            else if (blockType == BlockType.GROUND)
            {
                if ((GROUND_TYPE)subType == GROUND_TYPE.APPLE)
                    MakeLabel(block.gameObject, index.ToString(), labelTr);
                else if (subType == (int)GROUND_TYPE.LINE || subType == (int)GROUND_TYPE.LINE_H || subType == (int)GROUND_TYPE.LINE_V || subType == (int)GROUND_TYPE.CIRCLE)
                {
                    string text = "0";
                    MakeLabel(block.gameObject, text, labelTr);
                }
                else if (subType == (int)GROUND_TYPE.CANDY)
                {
                    MakeLabel(block.gameObject, "Candy", labelTr);
                }
            }
            else if (blockType == BlockType.GROUND_BOMB)
            {
                string text = "";
                if ((GROUND_TYPE)subType == GROUND_TYPE.CIRCLE) text = "1";
                else if ((GROUND_TYPE)subType == GROUND_TYPE.LINE_V) text = "2";
                else if ((GROUND_TYPE)subType == GROUND_TYPE.LINE_H) text = "3";
                MakeLabel(block.gameObject, text, labelTr);
            }
            else if (blockType == BlockType.GROUND_APPLE)
            {
                MakeLabel(block.gameObject, index.ToString(), labelTr);
            }
            else if (blockType == BlockType.ColorBigJewel)
            {
                MakeLabel(block.gameObject, index.ToString(), labelTr);
            }
            else if (blockType == BlockType.WORLD_RANK_ITEM)
            {
                labelTr = new Vector3(0f, -23f, 0f);
                MakeLabel(block.gameObject, count.ToString(), labelTr, new Color(130f / 255f, 1f, 1f), new Color(50f / 255f, 50f / 255f, 150f / 255f));
            }
            else if (blockType == BlockType.HEART)
            {
                MakeLabel(block.gameObject, index.ToString(), labelTr);
            }
            else if (blockType == BlockType.LITTLE_FLOWER_POT || blockType == BlockType.PEA || blockType == BlockType.PEA_BOSS)
            {
                UpdateBlockCount(block);
            }
            else if (blockType == BlockType.SODAJELLY)
            {
                float rateColor = 0.8f;
                Color tmpColor = Color.white;

                switch (index)
                {
                    case 1:
                        tmpColor = Color.green * rateColor;
                        break;
                    case 2:
                        tmpColor = Color.yellow * rateColor;
                        break;
                    case 3:
                        tmpColor = Color.white * rateColor;
                        break;
                    case 4:
                        tmpColor = Color.blue * rateColor;
                        break;
                    case 5:
                        tmpColor = Color.red * rateColor;
                        break;
                }

                tmpColor.a = 1f;
                block.mainSprite.color = tmpColor;
            }
        }

        block.PostBlockMake();

        return block;
    }

    public BlockBomb MakeBombBlock(BlockBase tempBlock, int indexX, int indexY, BlockBombType blockType, BlockColorType blockColorType = BlockColorType.NONE, bool secendBomb = false, bool pangByNow = false, bool fixBombUniIndex = false, int pangIndex = -1)
    {
        BlockBomb._liveCount++;
        BlockBomb block = NGUITools.AddChild(GameUIManager.instance.groundAnchor, BombBlockObj).GetComponent<BlockBomb>();
        block.gameObject.transform.localPosition = PosHelper.GetPosByIndex(indexX, indexY);
        block.indexX = indexX;
        block.indexY = indexY;
        block.colorType = blockColorType;
        block.type = blockType;
        block._block = tempBlock;
        block.isSecendBomb = secendBomb;
        block.pangByNow = pangByNow;
        block.isFixBombUniIndex = fixBombUniIndex;
        block.isFixPangIndex = pangIndex;

        ManagerBlock.instance.listObject.Add(block.gameObject);

        if (block._block != null)
        block._block.PostBlockMake();

        return block;
    }

    public BlockBase MakeBlockByStartBoard(int indexX, int indexY)
    {
        Board tempBoard = PosHelper.GetBoard(indexX, indexY);

       foreach (var startInfo in ManagerBlock.instance.stageInfo.ListStartInfo)
       {
            //게임클리어시 노말블럭만 나오기
            if (GameManager.instance.state == GameState.GAMECLEAR ||
                GameManager.instance.state == GameState.GAMEOVER)
            {
                return MakeBlockBase(indexX, indexY, BlockType.NORMAL, GetBlockRandomTypeAtStart(tempBoard.GetListIgnoreColorType()));
            }

            if (startInfo.type == (int)BlockType.KEY
                && startInfo.probability > 0
                && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, startInfo.Probs[randCount]);// startInfo.lifeCount);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            else if (startInfo.type == (int)BlockType.GROUND_KEY
                && startInfo.probability > 0
                && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);

                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, startInfo.Probs[randCount]);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            else if (startInfo.type == (int)BlockType.GROUND_ICE_APPLE
                && startInfo.probability > 0
                && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);

                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    return MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, startInfo.Probs[randCount]);// startInfo.lifeCount);
                }
            }
            else if ((startInfo.type == (int)BlockType.BLOCK_DYNAMITE)
            && startInfo.probability > 0
            && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0 
            && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
            && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
            && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    int DynamiteTime = startInfo.timeCount[startInfo.Probs[randCount] - 1];

                    int randLineType = GameManager.instance.GetIngameRandom(0, 2);
                    BlockBombType dynamiteLine = BlockBombType.LINE_H;
                    if (randLineType == 0) dynamiteLine = BlockBombType.LINE_V;

                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;

                    //블럭 컬러 지정하는 부분.
                    List<BlockColorType> listIgnoreColor = tempBoard.GetListIgnoreColorType(BlockType.BLOCK_DYNAMITE);
                    BlockColorType dynamiteColor = GetBlockRandomTypeAtStart(listIgnoreColor);

                    int[] dynamiteColorCount = new int[6]{0,0,0,0,0,0};

                    //다이나마이트 블럭 컬러찾기
                    foreach(var tempDyablock in  BlockBase._listBlock)
                    {
                        if (tempDyablock is BlockDynamite)
                            dynamiteColorCount[(int)tempDyablock.colorType]++;
                    }

                    int lowCount = (int)dynamiteColor;
                    //출발에서 나오는 블럭이 정해져 있지 않은 상태이면, 현재 나온 다이너 마이트 중 색이 적은 걸로 변경.
                    if (listIgnoreColor.Count == 0)
                    {
                        //순차적으로
                        for (int i = 1; i < dynamiteColorCount.Length; i++)
                        {
                            if (dynamiteColorCount[lowCount] > dynamiteColorCount[i])
                            {
                                if (ManagerBlock.instance.stageInfo.probability[i - 1] > 0)
                                    lowCount = i;
                            }
                        }
                    }

                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, (BlockColorType)lowCount, startInfo.Probs[randCount], DynamiteTime, (int)dynamiteLine);// startInfo.lifeCount);
                    MakeIce(startInfo.ProbsICE, tempBlock);

                    return tempBlock;
                }
            }
            else if (startInfo.type == (int)BlockType.GROUND_BOMB
             && startInfo.probability > 0
             && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
             && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
             && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
             && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    int randBomb = GameManager.instance.GetIngameRandom(0, startInfo.ProbsSub.Count);
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    return MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, startInfo.Probs[randCount], startInfo.ProbsSub[randBomb]);
                }
            }
            else if (startInfo.type == (int)BlockType.GROUND_APPLE
               && startInfo.probability > 0
               && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
               && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
               && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
               && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
               )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    int randApple = GameManager.instance.GetIngameRandom(0, startInfo.ProbsSub.Count);
                    int appleCount = 1;
                    if (startInfo.ProbsSub[randApple] == 3)
                        appleCount = 5;
                    else if (startInfo.ProbsSub[randApple] == 2)
                        appleCount = 3;

                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    return MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, startInfo.Probs[randCount], appleCount, (int)GROUND_TYPE.APPLE);
                }
            }
            else if ((startInfo.type == (int)BlockType.START_Bomb || startInfo.type == (int)BlockType.START_Rainbow)
                    && startInfo.probability > 0
                    && (tempBoard.startBlockType2 & ((int)(1 << (startInfo.type-32)))) != 0
                    && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                    && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                    && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
                    )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE);

                    if (startInfo.type == (int)BlockType.START_Bomb)
                        tempBlock.bombType = BlockBombType.BOMB;
                    else if (startInfo.type == (int)BlockType.START_Rainbow)
                    {
                        tempBlock.bombType = BlockBombType.RAINBOW;
                        tempBlock.colorType = GetBlockRandomTypeAtStart(tempBoard.GetListIgnoreColorType());
                    }

                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;                 
                }
            }
            else if (startInfo.type == (int)BlockType.START_Line
                    && startInfo.probability > 0
                    && (tempBoard.startBlockType2 & ((int)(1 << (startInfo.type - 32)))) != 0
                    && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                    && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                    && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
                    )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE);

                    if(startInfo.Probs.Count > 0)
                    {
                        int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);

                        if (startInfo.Probs[randCount] == 1)
                            tempBlock.bombType = BlockBombType.LINE_H;
                        else
                            tempBlock.bombType = BlockBombType.LINE_V;

                    }
                    else
                    {
                        int randomLine = GameManager.instance.GetIngameRandom(0, 2);
                        tempBlock.bombType = randomLine == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H;
                    }
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            else if (startInfo.type == (int)BlockType.APPLE
                && startInfo.probability > 0
                && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
                )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    int appleCount = 1;

                    if (startInfo.Probs[randCount] == 3)
                        appleCount = 5;
                    else if (startInfo.Probs[randCount] == 2)
                        appleCount = 3;

                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, appleCount);

                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }

            //모험모드 아이템.
            else if (startInfo.type == (int)BlockType.ADVENTURE_POTION_HEAL
                && startInfo.probability > 0
                && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            else if (startInfo.type == (int)BlockType.ADVENTURE_POTION_SKILL
                && startInfo.probability > 0
                && (tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            else if ((startInfo.type == (int)BlockType.COIN_BAG)
                   && startInfo.probability > 0
                   && (tempBoard.startBlockType2 & ((int)(1 << (startInfo.type - 32)))) != 0
                   && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                   && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                   && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
                   )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    int coinCount = startInfo.timeCount[startInfo.Probs[randCount]-1];

                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, coinCount);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            else if ((startInfo.type == (int)BlockType.PEA || startInfo.type == (int)BlockType.PEA_BOSS)
                && startInfo.probability > 0
                && ((tempBoard.startBlockType2 & ((int)(1 << (startInfo.type - 32)))) != 0 || startInfo.type < 32)
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn)))
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;

                    BlockColorType currentColorType = BlockColorType.NONE;
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, currentColorType, startInfo.Probs[randCount] - 1);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            else if (startInfo.type == (int)BlockType.GROUND_BLOCKBLACK
                && startInfo.probability > 0
                && ((tempBoard.startBlockType2 & (int)(1 << (startInfo.type - 32)))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;

                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, startInfo.Probs[randCount]);
                    return tempBlock;
                }
            }
            else if (startInfo.type == (int)BlockType.PAINT
                && startInfo.probability > 0
                && ((tempBoard.startBlockType2 & (int)(1 << (startInfo.type - 32)))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    BlockColorType colorType = GetStartBlockColorType_Paint(tempBoard, startInfo.timeCount);
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, colorType, startInfo.Probs[randCount]);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            else if (startInfo.type == (int)BlockType.WATERBOMB
                && startInfo.probability > 0
                && ((tempBoard.startBlockType2 & (int)(1 << (startInfo.type - 32)))) != 0
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type]
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    int randCount = GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count);
                    int subCount = startInfo.ProbsSub[GameManager.instance.GetIngameRandom(0, startInfo.ProbsSub.Count)];
                    int timeCount = startInfo.timeCount[startInfo.Probs[randCount] - 1];
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, BlockColorType.NONE, startInfo.Probs[randCount], timeCount, subCount);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
            //기타타입들
            else if (startInfo.type != (int)BlockType.ICE 
                && startInfo.type != (int)BlockType.KEY
                && startInfo.type != (int)BlockType.GROUND_ICE_APPLE
                && startInfo.type != (int)BlockType.GROUND_BOMB
                && startInfo.type != (int)BlockType.GROUND_KEY
                && startInfo.type != (int)BlockType.ADVENTURE_POTION_SKILL
                && startInfo.type != (int)BlockType.ADVENTURE_POTION_HEAL
                && startInfo.type != (int)BlockType.PEA
                && startInfo.probability > 0
                && ((tempBoard.startBlockType & ((int)(1 << startInfo.type))) != 0 || startInfo.type >= 32)
                && ((tempBoard.startBlockType2 & ((int)(1 << (startInfo.type - 32)))) != 0 || startInfo.type < 32)
                && startInfo.max_display_Count > ManagerBlock.instance.liveBlockTypeCount[startInfo.type] 
                && (startInfo.max_stage_Count == 0 || startInfo.max_stage_Count > ManagerBlock.instance.totalCreatBlockTypeCount[startInfo.type])
                && ((startInfo.minTurn == 0) || (GameManager.instance.touchCount <= ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] - startInfo.minTurn))
            )
            {
                int rand = GameManager.instance.GetIngameRandom(0, 1000);
                if (rand <= startInfo.probability)
                {
                    ManagerBlock.instance.creatBlockTypeTurnCount[startInfo.type] = GameManager.instance.touchCount;
                    BlockColorType currentColorType = BlockColorType.NONE;

                    //설정된 단계값에서 랜덤한 값을 가져옴
                    int probValue = startInfo.Probs[GameManager.instance.GetIngameRandom(0, startInfo.Probs.Count)];

                    switch ((BlockType)startInfo.type)
                    {
                        case BlockType.FIRE_WORK:
                            currentColorType = GetBlockRandomTypeAtStart(tempBoard.GetListIgnoreColorType(BlockType.FIRE_WORK));
                            break;
                        case BlockType.GROUND_JEWEL: //보석의 경우, 3번째 칸의 설정이 1단계로 사용됨
                            if (probValue == 3)
                                probValue = 0;
                            break;

                    }
                    BlockBase tempBlock = MakeBlockBase(indexX, indexY, (BlockType)startInfo.type, currentColorType, probValue);
                    MakeIce(startInfo.ProbsICE, tempBlock);
                    return tempBlock;
                }
            }
        }

        //if (gimmickRemoveRatio == 1) return MakeBlockBase(indexX, indexY, BlockType.NORMAL, GetRandomType2());
        //if (gimmickRemoveRatio == 2) return MakeBlockBase(indexX, indexY, BlockType.NORMAL, GetRandomType3());

       if (GameManager.gameMode == GameMode.ADVENTURE && GameManager.adventureMode != AdventureMode.ORIGIN)
       {
           int downCount = 1;

           if (ManagerBlock.instance.comboCount >= ManagerBlock.instance.stageInfo.sameBlockRatio) //(ratioCount < ManagerBlock.instance.stageInfo.sameBlockRatio)
           {
               while (true)
               {
                   BlockBase downBlock = PosHelper.GetBlock(indexX, indexY + downCount);
                   Board downBoard = PosHelper.GetBoard(indexX, indexY + downCount);

                   if (downBoard == null)
                       break;

                   if (downBlock != null)
                   {
                        return MakeBlockBase(indexX, indexY, BlockType.NORMAL, GetBlockRandomTypeAtStart(tempBoard.GetListIgnoreColorType()));
                   }

                   downCount++;
               }
           }
       }

        return MakeBlockBase(indexX, indexY, BlockType.NORMAL, GetBlockRandomTypeAtStart(tempBoard.GetListIgnoreColorType()));
    }

    void MakeIce(List<int> ProbsICE, BlockBase tempBlock)
    {
        if (ProbsICE.Count == 0)
            return;

        if (tempBlock.IsCanCoverIce() == false)
            return;

        int iceCount = GameManager.instance.GetIngameRandom(0, ProbsICE.Count);
        DecoInfo boardInfo = new DecoInfo();
        boardInfo.BoardType = (int)BoardDecoType.ICE;
        boardInfo.count = ProbsICE[iceCount];

        float blockRatioY = 1;
        if (ManagerBlock.instance.stageInfo.reverseMove == 1)
            blockRatioY = 0.01f;

        BlockDeco boardDeco = BoardDecoMaker.instance.MakeBlockDeco(tempBlock, tempBlock.indexX, tempBlock.indexY, boardInfo);
        boardDeco.SetSprite();
        boardDeco.uiSprite.customFill.blockRatio = 1f * blockRatioY;
    }

    public void RemoveBlock(BlockBase blockBase)
    {
        ManagerBlock.instance.listObject.Remove(blockBase.gameObject);
        Destroy(blockBase.gameObject);
    }

    // 랜덤 컬러 구하기.
    public BlockColorType GetBlockRandomType(BlockColorType ignoreType = BlockColorType.NONE)
    {
        if (gimmickRemoveRatio == 1) return GetRandomType(2, ignoreType);
        if (gimmickRemoveRatio == 2) return GetRandomType(3, ignoreType);
        return GetRandomType(1, ignoreType);
    }

    // 출발에서 랜덤 컬러 구하기.
    public BlockColorType GetBlockRandomTypeAtStart(List<BlockColorType> listIgnoreColors, BlockColorType ignoreType = BlockColorType.NONE, bool isAdjustProb = true)
    {
        if (gimmickRemoveRatio == 1) return GetRandomTypeAtStart(2, listIgnoreColors, ignoreType, isAdjustProb);
        if (gimmickRemoveRatio == 2) return GetRandomTypeAtStart(3, listIgnoreColors, ignoreType, isAdjustProb);
        return GetRandomTypeAtStart(1, listIgnoreColors, ignoreType, isAdjustProb);
    }
    
    // 맵 구성 시(게임시작할 때/블럭 섞을때) 블럭 컬러 구하기.
    public BlockColorType GetBlockRandomTypeAtMakeMap(BlockColorType ignoreType = BlockColorType.NONE)
    {
        // 맵 구성 확률 사용 안할 경우, 기본확률로 블럭 생성.
        if (ManagerBlock.instance.stageInfo.isUseProbabilityMakMake == false)
        {
            return GetBlockRandomType(ignoreType);
        }

        if (ManagerBlock.instance.useSameColor)
        {
            if (ManagerBlock.instance.checkSameColor)
            {
                ManagerBlock.instance.SameColorCount++;

                if (ManagerBlock.instance.SameColorCount > ManagerBlock.instance.SameColorDiffcult)
                {
                    ManagerBlock.instance.checkSameColor = false;
                    ManagerBlock.instance.SameColorCount = 0;
                    return GetBlockRandomTypeAtMakeMap(ignoreType);
                }
                return ManagerBlock.instance.sameColor;
            }
        }

        List<BlockColorType> listColorProb =  new List<BlockColorType>();
        for (int i = 0; i < listBlockProbMapMake.Count; i++)
        {
            if (ignoreType != listBlockProbMapMake[i])
                listColorProb.Add(listBlockProbMapMake[i]);
        }

        //현재 등장할 수 있는 컬러 정보 없는 경우, 무시하는 타입 제외 랜덤으로 아무 블럭이나 출력.
        if (listColorProb.Count == 0)
        {
            while (true)
            {
                int randomType = GameManager.instance.GetIngameRandom(1, 6);
                if ((BlockColorType)randomType != ignoreType)
                {
                    return (BlockColorType)randomType;
                }
            }
        }
        else
        {
            int randomType = GameManager.instance.GetIngameRandom(0, listColorProb.Count);
            return listColorProb[randomType];
        }
    }

    //블럭 컬러 타입 랜덤으로 생성.
    public BlockColorType GetRandomType(int probIndex, BlockColorType ignoreType = BlockColorType.NONE)
    {
        if (ManagerBlock.instance.useSameColor)
        {
            if (ManagerBlock.instance.checkSameColor)
            {
                ManagerBlock.instance.SameColorCount++;

                if (ManagerBlock.instance.SameColorCount > ManagerBlock.instance.SameColorDiffcult) //ManagerBlock.instance.stageInfo.SameBlockColorByStart)                {
                {
                    ManagerBlock.instance.checkSameColor = false;
                    ManagerBlock.instance.SameColorCount = 0;
                    return GetRandomType(probIndex, ignoreType);
                }
                return ManagerBlock.instance.sameColor;
            }
        }

        List<BlockColorType> listColorProb = null;
        listColorProb = GetListByProb(probIndex, ignoreType);

        //현재 등장할 수 있는 컬러 정보 없는 경우, 무시하는 타입 제외 랜덤으로 아무 블럭이나 출력.
        if (listColorProb == null || listColorProb.Count == 0)
        {
            while (true)
            {
                int randomType = GameManager.instance.GetIngameRandom(1, 6);
                if ((BlockColorType)randomType != ignoreType)
                {
                    return (BlockColorType)randomType;
                }
            }
        }
        else
        {
            int randomType = GameManager.instance.GetIngameRandom(0, listColorProb.Count);
            return listColorProb[randomType];
        }
    }

    // 출발에서 블럭 컬러 타입 랜덤으로 생성.
    public BlockColorType GetRandomTypeAtStart(int probIndex, List<BlockColorType> listStartIgnoreColors, BlockColorType ignoreType = BlockColorType.NONE, bool isAdjustProb = true)
    {
        // 생성하지 않을 컬러 목록 재생성.
        List<BlockColorType> listIgnoreColors = new List<BlockColorType>(listStartIgnoreColors);
        if (ignoreType != BlockColorType.NONE && listIgnoreColors.FindIndex(x => x == ignoreType) == -1)
        {
            listIgnoreColors.Add(ignoreType);
        }

        // 현재 출발에서 나올 수 있는 컬러 리스트.
        List<BlockColorType> listColorProb = null;
        listColorProb = GetListByProbAtStart(probIndex, listIgnoreColors);

        // 확률 조정이 들어간다면, 확률조정 코드 동작
        if (isAdjustProb == true)
        {
            //블럭확률 조정된 블럭이 현재 출발에서 나올 수 없는 컬러일 경우, 확률 적용 안됨.
            if (ManagerBlock.instance.useSameColor)
            {
                if (ManagerBlock.instance.checkSameColor)
                {
                    ManagerBlock.instance.SameColorCount++;
                    if (ManagerBlock.instance.SameColorCount > ManagerBlock.instance.SameColorDiffcult)
                    {
                        ManagerBlock.instance.checkSameColor = false;
                        ManagerBlock.instance.SameColorCount = 0;
                        return GetRandomTypeAtStart(probIndex, listStartIgnoreColors, ignoreType);
                    }

                    //등장할 수 있는 블럭이 없는 경우, 현재 블럭조정된 컬러가 무시하는 타입이 아니면 해당타입으로 블럭 생성.
                    if (listColorProb == null || listColorProb.Count == 0)
                    {
                        if (ignoreType != ManagerBlock.instance.sameColor)
                        {
                            Debug.Log(" [1-1]. 무시하는 컬러가 아님 : " + ManagerBlock.instance.sameColor);
                            return ManagerBlock.instance.sameColor;
                        }
                        else
                        {
                            Debug.Log(" [1-2]. 무시하는 컬러임 : " + ManagerBlock.instance.sameColor);
                        }
                    }
                    else
                    {
                        if (listColorProb.FindIndex(x => x == ManagerBlock.instance.sameColor) > -1)
                        {
                            Debug.Log(" [2-1]. 블럭 확률 조정 컬러 : " + ManagerBlock.instance.sameColor);
                            return ManagerBlock.instance.sameColor;
                        }
                        else
                        {
                            Debug.Log(" [2-2]. 현재 출발에서 나올 수 있는 컬러가 아님 : " + ManagerBlock.instance.sameColor);
                        }
                    }
                }
            }
        }

        //현재 등장할 수 있는 컬러 정보 없는 경우, 무시하는 타입 제외 랜덤으로 아무 블럭이나 출력.
        if (listColorProb == null || listColorProb.Count == 0)
        {
            while (true)
            {
                int randomType = GameManager.instance.GetIngameRandom(1, 6);
                if ((BlockColorType)randomType != ignoreType)
                {
                    return (BlockColorType)randomType;
                }
            }
        }
        else
        {
            int randomType = GameManager.instance.GetIngameRandom(0, listColorProb.Count);
            return listColorProb[randomType];
        }
    }

    private List<BlockColorType> GetListByProb(int probIndex, BlockColorType listIgnoreColor)
    {
        // 생성 안하는 컬러 제외한 확률 리스트 생성 후 반환.
        List<BlockColorType> listBlockColors = new List<BlockColorType>();;
        switch (probIndex)
        {
            case 1:
                for (int i = 0; i < listBlockProb.Count; i++)
                {
                    if (listIgnoreColor != listBlockProb[i])
                        listBlockColors.Add(listBlockProb[i]);
                }
                break;
            case 2:
                for (int i = 0; i < listBlockProb2.Count; i++)
                {
                    if (listIgnoreColor != listBlockProb2[i])
                        listBlockColors.Add(listBlockProb2[i]);
                }
                break;
            case 3:
                for (int i = 0; i < listBlockProb3.Count; i++)
                {
                    if (listIgnoreColor != listBlockProb3[i])
                        listBlockColors.Add(listBlockProb3[i]);
                }
                break;
        }

        if (listBlockColors.Count > 0)
            return listBlockColors;

        // 생성될 수 있는 컬러가 없으면 null 반환.
        return null;
    }

    private List<BlockColorType> GetListByProbAtStart(int probIndex, List<BlockColorType> listIgnoreColors)
    {
        // 생성 안하는 컬러가 없으면 확률 그대로 반환.
        if (listIgnoreColors.Count == 0)
        {
            switch (probIndex)
            {
                case 1:
                    return listBlockProb;
                case 2:
                    return listBlockProb2;
                default:
                    return listBlockProb3;
            }
        }

        // 생성 안하는 컬러 제외한 확률 리스트 생성 후 반환.
        List<BlockColorType> listBlockColors = new List<BlockColorType>();
        switch (probIndex)
        {
            case 1:
                for (int i = 0; i < listBlockProb.Count; i++)
                {
                    if (listIgnoreColors.FindIndex(x => x == listBlockProb[i]) == -1)
                        listBlockColors.Add(listBlockProb[i]);
                }
                break;
            case 2:
                for (int i = 0; i < listBlockProb2.Count; i++)
                {
                    if (listIgnoreColors.FindIndex(x => x == listBlockProb2[i]) == -1)
                        listBlockColors.Add(listBlockProb2[i]);
                }
                break;
            case 3:
                for (int i = 0; i < listBlockProb3.Count; i++)
                {
                    if (listIgnoreColors.FindIndex(x => x == listBlockProb3[i]) == -1)
                        listBlockColors.Add(listBlockProb3[i]);
                }
                break;
        }
        
        if (listBlockColors.Count > 0)
            return listBlockColors;

        // 생성될 수 있는 컬러가 없으면 null 반환.
        return null;
    }

    public BlockColorType GetRandomType2(BlockColorType ignoreType = BlockColorType.NONE)
    {
        if (ManagerBlock.instance.useSameColor)
        {
            if (ManagerBlock.instance.checkSameColor)
            {
                ManagerBlock.instance.SameColorCount++;

                if (ManagerBlock.instance.SameColorCount > ManagerBlock.instance.SameColorDiffcult) //ManagerBlock.instance.stageInfo.SameBlockColorByStart)                {
                {
                    ManagerBlock.instance.checkSameColor = false;
                    ManagerBlock.instance.SameColorCount = 0;
                    return GetRandomType2();
                }
                return ManagerBlock.instance.sameColor;
            }
        }

        while (true)
        {
            int randomType = GameManager.instance.GetIngameRandom(0, listBlockProb2.Count);

            if (listBlockProb2[randomType] != ignoreType)
            {
                return listBlockProb2[randomType];
            }
        }
    }

    public BlockColorType GetRandomType3(BlockColorType ignoreType = BlockColorType.NONE)
    {
        if (ManagerBlock.instance.useSameColor)
        {
            if (ManagerBlock.instance.checkSameColor)
            {
                ManagerBlock.instance.SameColorCount++;

                if (ManagerBlock.instance.SameColorCount > ManagerBlock.instance.SameColorDiffcult) //ManagerBlock.instance.stageInfo.SameBlockColorByStart)                {
                {
                    ManagerBlock.instance.checkSameColor = false;
                    ManagerBlock.instance.SameColorCount = 0;
                    return GetRandomType3(ignoreType);
                }
                return ManagerBlock.instance.sameColor;
            }
        }

        while (true)
        {
            int randomType = GameManager.instance.GetIngameRandom(0, listBlockProb3.Count);

            if (listBlockProb3[randomType] != ignoreType)
            {
                return listBlockProb3[randomType];
            }
        }
    }

    public void SetBlockProbability()
    {
        listBlockProb.Clear();

        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[0]; p++)
            listBlockProb.Add(BlockColorType.A);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[1]; p++)
            listBlockProb.Add(BlockColorType.B);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[2]; p++)
            listBlockProb.Add(BlockColorType.C);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[3]; p++)
            listBlockProb.Add(BlockColorType.D);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[4]; p++)
            listBlockProb.Add(BlockColorType.E);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[5]; p++)
            listBlockProb.Add(BlockColorType.F);
    }

    public void SetBlockProbability2()
    {
        listBlockProb2.Clear();

        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[0]; p++)
            listBlockProb2.Add(BlockColorType.A);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[1]; p++)
            listBlockProb2.Add(BlockColorType.B);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[2]; p++)
            listBlockProb2.Add(BlockColorType.C);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[3]; p++)
            listBlockProb2.Add(BlockColorType.D);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[4]; p++)
            listBlockProb2.Add(BlockColorType.E);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[5]; p++)
            listBlockProb2.Add(BlockColorType.F);

        listBlockProb3.Clear();

        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[0]; p++)
            listBlockProb3.Add(BlockColorType.A);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[1]; p++)
            listBlockProb3.Add(BlockColorType.B);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[2]; p++)
            listBlockProb3.Add(BlockColorType.C);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[3]; p++)
            listBlockProb3.Add(BlockColorType.D);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[4]; p++)
            listBlockProb3.Add(BlockColorType.E);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability[5]; p++)
            listBlockProb3.Add(BlockColorType.F);


        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[0]; p++)
            listBlockProb3.Add(BlockColorType.A);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[1]; p++)
            listBlockProb3.Add(BlockColorType.B);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[2]; p++)
            listBlockProb3.Add(BlockColorType.C);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[3]; p++)
            listBlockProb3.Add(BlockColorType.D);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[4]; p++)
            listBlockProb3.Add(BlockColorType.E);
        for (int p = 0; p < ManagerBlock.instance.stageInfo.probability2[5]; p++)
            listBlockProb3.Add(BlockColorType.F);

    }

    public void SetBlockProbabilityMapMake()
    {
        listBlockProbMapMake.Clear();

        int[] probMapMake = ManagerBlock.instance.stageInfo.probabilityMapMake == null ? new int[] { 10, 10, 10, 10, 10, 0 } : ManagerBlock.instance.stageInfo.probabilityMapMake;

        for (int p = 0; p < probMapMake[0]; p++)
            listBlockProbMapMake.Add(BlockColorType.A);
        for (int p = 0; p < probMapMake[1]; p++)
            listBlockProbMapMake.Add(BlockColorType.B);
        for (int p = 0; p < probMapMake[2]; p++)
            listBlockProbMapMake.Add(BlockColorType.C);
        for (int p = 0; p < probMapMake[3]; p++)
            listBlockProbMapMake.Add(BlockColorType.D);
        for (int p = 0; p < probMapMake[4]; p++)
            listBlockProbMapMake.Add(BlockColorType.E);
        for (int p = 0; p < probMapMake[5]; p++)
            listBlockProbMapMake.Add(BlockColorType.F);
    }

    #region 확률2
    //남은 고정기믹 갯수나 남은 턴에 따라 어느 확률을 사용할 지.
    public int gimmickRemoveRatio = 0;

    public void UseProb2()
    {
        if (ManagerBlock.instance.stageInfo.useProbability2 == 0)
        {
            gimmickRemoveRatio = 0;
            return;
        }
       
        if(GameManager.gameMode != GameMode.COIN)
        {
            UseProb2ByGimmick();
        }
        else
        {
            UseProb2ByTurn();
        }
    }

    //기믹 카운트로 확률2 사용
    private void UseProb2ByGimmick()
    {
        int boardCount = 0;
        int originGimmickCount = 0;
        float gimmickCount = 0;

        //양쪽 보드가 모두 가지는 기믹 카운트.
        int sideGimmickCount = 0;

        foreach (BlockInfo blockInfo in ManagerBlock.instance.stageInfo.ListBlock)
        {
            if (blockInfo.isActiveBoard == 1) boardCount++;

            if (blockInfo.type == (int)BlockType.PLANT ||
                blockInfo.type == (int)BlockType.STONE ||
                blockInfo.type == (int)BlockType.ColorBigJewel ||
                blockInfo.type == (int)BlockType.LITTLE_FLOWER_POT ||
                blockInfo.type == (int)BlockType.SODAJELLY ||
                blockInfo.type == (int)BlockType.HEART ||
                blockInfo.type == (int)BlockType.HEART_HOME ||
                blockInfo.type == (int)BlockType.BREAD ||
                blockInfo.type == (int)BlockType.CANNON)
                originGimmickCount++;

            foreach (DecoInfo boardInfo in blockInfo.ListDeco)
            {
                if (boardInfo.BoardType == (int)BoardDecoType.NET) originGimmickCount++;
                if (boardInfo.BoardType == (int)BoardDecoType.WATER) originGimmickCount++;
                if (boardInfo.BoardType == (int)BoardDecoType.GRASS) originGimmickCount++;
                if (boardInfo.BoardType == (int)BoardDecoType.FENCEBLOCK) originGimmickCount++;
                if (boardInfo.BoardType == (int)BoardDecoType.GRASSFENCEBLOCK) originGimmickCount++;
                if (boardInfo.BoardType == (int)BoardDecoType.RANDOM_BOX) originGimmickCount++;
                if (boardInfo.BoardType == (int)BoardDecoType.CLOVER) originGimmickCount++;
            }
        }

        foreach (var block in BlockBase._listBlock)
        {
            if (block.type == BlockType.PLANT
                || block.type == BlockType.STONE
                || block.type == BlockType.LITTLE_FLOWER_POT
                || block.type == BlockType.SODAJELLY
                || block.type == BlockType.HEART
                || block.type == BlockType.HEART_HOME
                || block.type == BlockType.BREAD
                || block.type == BlockType.CANNON)
                gimmickCount++;

            if (block.type == BlockType.ColorBigJewel)
                gimmickCount += 4;
        }

        foreach (var board in ManagerBlock.boards)
        {
            if (board.BoardOnHide.Count > 0) gimmickCount += board.BoardOnHide.Count;
            if (board.BoardOnNet.Count > 0) gimmickCount += board.BoardOnNet.Count;
            if (board.BoardOnGrass.Count > 0) gimmickCount++;

            // 터지는 방해블럭은 기믹 카운트 올려줌.
            foreach (var disturb in board.BoardOnDisturbs)
            {
                if (disturb.IsCanPang() == true)
                {
                    sideGimmickCount++;
                }
            }
        }
        if (sideGimmickCount > 2)
            gimmickCount += (int)(sideGimmickCount / 2);

        int ratio = (int)((float)gimmickCount / (float)originGimmickCount * 100);
        if (ratio > 50)
        {
            gimmickRemoveRatio = 0;
        }
        else if (ratio > 25)
        {
            gimmickRemoveRatio = 2;
        }
        else
        {
            gimmickRemoveRatio = 1;
        }
    }

    //남은 턴으로 확률2 사용
    private void UseProb2ByTurn()
    {
        if (GameManager.instance.moveCount > 10)
        {
            gimmickRemoveRatio = 0;
        }
        else
        {
            gimmickRemoveRatio = 1;
        }
    }
    #endregion

    #region 등장 가능한 컬러 계산 관련
    private BlockColorType GetStartBlockColorType_Paint(Board tempBoard, int[] colorCount)
    {
        //생성 가능한 개수가 남아있는 컬러 리스트를 저장
        List<BlockColorType> listExistColor = new List<BlockColorType>();
        for (int i = 0; i < colorCount.Length; i++)
        {
            if (ManagerBlock.instance.IsLimitOver_BlockColorCreate(BlockType.PAINT, i, colorCount[i]) == false)
            {
                listExistColor.Add((BlockColorType)(i + 1));
            }
        }

        //해당 출발에서 생성 가능한 컬러면서 생성가능한 카운트가 남아있는 컬러들만 등록해줌
        List<BlockColorType> listCanMakeColor = new List<BlockColorType>();
        List<BlockColorType> listStartColor = tempBoard.GetListCanMakeColorType(BlockType.PAINT);
        if (listExistColor.Count > 0)
        {
            for (int i = 0; i < listStartColor.Count; i++)
            {
                BlockColorType checkColor = listStartColor[i];
                if (listExistColor.FindIndex(x => x == checkColor) != -1)
                    listCanMakeColor.Add(checkColor);
            }
        }
        
        if (listCanMakeColor.Count > 0)
        {   //생성 가능한 컬러가 있다면 해당 컬러 중 랜덤으로 반환
            return listCanMakeColor[GameManager.instance.GetIngameRandom(0, listCanMakeColor.Count)];
        }
        else
        {   //생성 가능한 컬러가 없다면, 출발 확률에 따라 반환
            return GetBlockRandomTypeAtStart(tempBoard.GetListIgnoreColorType(BlockType.PAINT), isAdjustProb: false);
        }
    }
    #endregion

    public GameObject MakeBlockCoin(GameObject parentObj)
    {
        GameObject obj = NGUITools.AddChild(parentObj, blockCoinObj);
        return obj;
    }

    //현재 맵에 있는 해당 블럭의 카운트를 업데이트 할 때 사용
    private void UpdateBlockCount(BlockBase block)
    {
        if (EditManager.instance == null)
            return;
        int count = EditManager.instance.GetDicBlockCount(block.type);
        Vector3 labelPos = new Vector3(20f, -20f, 0f);
        MakeLabel(block.gameObject, count.ToString(), labelPos);
    }

    //현재 만들어진 블럭 아래에 석판, 석상, 잔디 등이 있는지 검사하는 함수.
    private void CheckDownBlockOrDeco(Board board)
    {
        if (GameManager.instance.state != GameState.EDIT)
            return;

		 if (board == null)
            return;
        for (int i = 0; i < board.DecoOnBoard.Count; i++)
        {   
            Crack crack = board.DecoOnBoard[i] as Crack;
            Grass grass = board.DecoOnBoard[i] as Grass;
            Carpet carpet = board.DecoOnBoard[i] as Carpet;
            if (crack as Crack || grass != null)
            {
                if (board.DecoOnBoard[i].GetComponentInChildren<UILabel>() == null)
                {
                    Vector3 tr = Vector3.zero;
                    string text = "";
                    if (crack != null)
                        text = "C";
                    else if (grass != null)
                    {
                        text = "G";
                        tr = new Vector3(25f, 0f, 0f);
                    }
                    if (carpet != null)
                        text = "C.p";
                    MakeLabel(board.DecoOnBoard[i].gameObject, text, tr);
                }
            }
        }
    }

    private void MakeLabel(GameObject obj, string text, Vector3 tr)
    {
        UILabel uiLabel = NGUITools.AddChild(obj.gameObject, BlockMaker.instance.BlockLabelObj).GetComponent<UILabel>();
        uiLabel.depth = (int)GimmickDepth.UI_LABEL;
        uiLabel.effectStyle = UILabel.Effect.Outline8;
        uiLabel.effectDistance = new Vector2(2, 2);
        uiLabel.effectColor = new Color(124f / 255f, 40f / 255f, 40f / 255f);
        uiLabel.color = new Color(1f, 208f / 255f, 56f / 255f);
        uiLabel.text = text;
        uiLabel.transform.localPosition = tr;
    }

    private void MakeLabel(GameObject obj, string text, Vector3 tr, Color textColor, Color effectColor)
    {
        UILabel uiLabel = NGUITools.AddChild(obj.gameObject, BlockMaker.instance.BlockLabelObj).GetComponent<UILabel>();
        uiLabel.depth = (int)GimmickDepth.UI_LABEL;
        uiLabel.effectStyle = UILabel.Effect.Outline8;
        uiLabel.effectDistance = new Vector2(2, 2);
        uiLabel.effectColor = effectColor;
        uiLabel.color = textColor;
        uiLabel.text = text;
        uiLabel.transform.localPosition = tr;
    }
}
