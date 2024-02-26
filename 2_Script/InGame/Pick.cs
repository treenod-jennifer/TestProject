using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pick : MonoSingletonOnlyScene<Pick>
{
    [SerializeField] Transform aForPick;
    [SerializeField] Transform bForPick;
    [SerializeField] BoxCollider boxCollider;

    public Vector3 _startPos;
    public static float _startSize = 0f;

    float blockInterval;
    float rateBlockInterval;
    float space;
    float rateX;
    float rateY;
    float scaleX;
    float scaleY;
    float offsetX;
    float offsetY;

    float pickTimer = 0;

    public void InitPick()
    {
        aForPick.localPosition = PosHelper.GetPosByIndex(0, 0);
        bForPick.localPosition = PosHelper.GetPosByIndex(1, 0);
        _startPos = aForPick.position;
        _startSize = bForPick.position.x - aForPick.position.x;
        pickTimer = 0;

        if (GameManager.gameMode == GameMode.ADVENTURE)
        {
            boxCollider.center = new Vector3(0, -190, 0);
            boxCollider.size = new Vector3(720, 620, 1);
        }

    }

    void OnDrag(Vector2 delta)
    {
        if (GameItemManager.instance != null) return;// && GameItemManager.instance.used == false) return;
        if (GameManager.instance.state != GameState.PLAY)return;
        if (ManagerTutorial._instance != null) return;
        if (GameManager.gameMode == GameMode.NORMAL && GameManager.instance.moveCount == 0)return;
        if (GameManager.gameMode == GameMode.DIG && GameManager.instance.moveCount == 0)return;
        if (ManagerBlock.instance.isPlayContinueAction == true) return;
        if (ManagerBlock.instance.waitBlockToFinishMoving && ManagerBlock.instance.state != BlockManagrState.WAIT)return;
        if (ManagerBlock.instance.stageInfo.waitState == 1 && ManagerBlock.instance.state != BlockManagrState.WAIT)return;
        if (ManagerBlock.instance.BlockTime - pickTimer < ManagerBlock.WAIT_TIME_FOR_NEXT_TOUCH)return;
        if (ManagerBlock.instance.movePanelPause)return;

        Pos blockIndex = GetBlockIndex();
        BlockBase selectBlock = GetBlock(blockIndex.x, blockIndex.y);

        if (selectBlock != null && selectBlock.IsSelectable() && !ManagerBlock.instance.isTouchDownBlock)
        {
           BlockMatchManager.instance.SetSelectBlockEffect(selectBlock);
        }
        else
        {
            BlockMatchManager.instance.SetSelectBlockEffect(null);
        }
    }

    BlockBase firstBlock = null;
    bool doubleClick = false;

    void OnPress(bool isPressed)
    {
        if (GameManager.instance.IsCanTouch == false) return;
        if (GameManager.instance.state != GameState.PLAY) return;
        if (GameManager.gameMode == GameMode.NORMAL && GameManager.instance.moveCount == 0) return;
        if (GameManager.gameMode == GameMode.DIG && GameManager.instance.moveCount == 0)  return;
        if (ManagerBlock.instance.isPlayContinueAction == true) return;
        if (ManagerBlock.instance.waitBlockToFinishMoving && ManagerBlock.instance.state != BlockManagrState.WAIT) return;
        if (ManagerBlock.instance.stageInfo.waitState == 1 && ManagerBlock.instance.state != BlockManagrState.WAIT) return;  
        if (ManagerBlock.instance.BlockTime - pickTimer < ManagerBlock.WAIT_TIME_FOR_NEXT_TOUCH)return;
        if (ManagerBlock.instance.movePanelPause) return;
        if (GameItemManager.instance != null && GameItemManager.instance.used == true) return;

        if (isPressed)
        {
            Pos blockIndex = GetBlockIndex();
            BlockBase selectBlock = GetBlock(blockIndex.x, blockIndex.y);
            Board selectBoard = PosHelper.GetBoard(blockIndex.x, blockIndex.y);

            if (GameItemManager.instance != null && GameItemManager.instance.used == false)
            {
                if (GameItemManager.instance.type == GameItemType.HEAL_ONE_ANIMAL ||
                    GameItemManager.instance.type == GameItemType.SKILL_POINT_CHARGE)
                    return;                        


                if (selectBlock != null && 
                    selectBlock.IsNormalBlock() && 
                    selectBoard != null && 
                    selectBoard.HasDecoHideBlock() == false && 
                    selectBoard.HasDecoCoverBlock() == false &&
                    selectBlock is NormalBlock  &&
                    (selectBlock.blockDeco == null || selectBlock.blockDeco.IsInterruptBlockSelect() == false)
                    )
                {
                    GameItemManager.instance.UseGameItem(selectBlock);
                }
                return;
            }

            if (Input.touchCount > 1)
            {
                doubleClick = true;
                return;
            }


            if (selectBlock != null && selectBlock.IsSelectable()) //
            {
                if (BlockMatchManager.instance.checkBlockToyBlastMatch(selectBlock))
                {
                    if (ManagerBlock.instance.isTouchDownBlock)
                    {
                        GameManager.instance.RemoveTurn();
                        BlockMatchManager.instance.CheckMatchPangBlock(selectBlock);
                        pickTimer = ManagerBlock.instance.BlockTime;
                    }
                    else
                    {
                        firstBlock = selectBlock;
                        BlockMatchManager.instance.SetSelectBlockEffect(selectBlock);
                    }
                }
                else
                {
                    if (ManagerBlock.instance.isTouchDownBlock)
                    {
                        selectBlock.DontMatchBlockSelect();
                    }
                    else
                    {
                        selectBlock.DontMatchBlockSelect();
                    }
                }
            }
        }
        else
        {
            BlockMatchManager.instance.SetSelectBlockEffect(null);

            Pos blockIndex = GetBlockIndex();
            BlockBase selectBlock = GetBlock(blockIndex.x, blockIndex.y);

            if (GameItemManager.instance != null )return;   
            if(ManagerTutorial._instance != null && selectBlock != firstBlock)return;


            if (Input.touchCount > 1) return;
            if (doubleClick)
            {
                doubleClick = false;
                return;
            }

            
            if (selectBlock != null && selectBlock.IsSelectable() && !ManagerBlock.instance.isTouchDownBlock)
            {
                if (BlockMatchManager.instance.checkBlockToyBlastMatch(selectBlock))
                {
                    if (GameManager.gameMode == GameMode.ADVENTURE)
                    {
                        ManagerBlock.instance.creatBlock = false;
                        AdventureManager.instance.ChangePos(selectBlock); 
                    }


                    GameManager.instance.RemoveTurn();
                    BlockMatchManager.instance.CheckMatchPangBlock(selectBlock);
                    pickTimer = ManagerBlock.instance.BlockTime;

                    if (ManagerBlock.instance.useSameColor)
                    {
                        int randomCount = GameManager.instance.GetIngameRandom(0, 100);
                        if (randomCount >= 45)
                        {
                            ManagerBlock.instance.SameColorCount = 0;
                            ManagerBlock.instance.sameColor = BlockMaker.instance.GetBlockRandomType();
                            ManagerBlock.instance.checkSameColor = true;
                        }
                        else
                        {
                            ManagerBlock.instance.checkSameColor = false;
                        }
                    }
                }
                else
                {
                    selectBlock.DontMatchBlockSelect();
                }
            }
        }      

    }


    Pos GetBlockIndex()
    {
        Vector3 mPos = Global._touchPos;// Input.mousePosition;
        Vector3 pos = GameUIManager.instance.groundMoveTransform.GetComponent<Camera>().ScreenToWorldPoint(mPos) - _startPos;


        int indexX = (int)((pos.x + _startSize * 0.5f) / _startSize);
        int indexY = -(int)((pos.y + _startSize * 0.5f) / _startSize) + 1;

        indexY += GameManager.MOVE_Y;

        if (GameManager.gameMode == GameMode.LAVA)
            indexY -= ManagerBlock.instance.stageInfo.Extend_Y;


        return new Pos(indexX, indexY);
    }

    BlockBase GetBlock(int in_offsetX, int in_offsetY)
    {
        if (PosHelper.GetBoardSreeen(in_offsetX, in_offsetY) != null)
        {
            if (PosHelper.GetBoardSreeen(in_offsetX, in_offsetY).Block != null)
            {
                return PosHelper.GetBoardSreeen(in_offsetX, in_offsetY).Block;
            }
        }
        return null;
    }
}
