using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyGameItem : MonoBehaviour
{
    public static FlyGameItem instance = null;
    public static int liveCount = 0;
    public GameItemType type = GameItemType.NONE;

    public UISprite _sprite;
    public List<BlockBase> listBlock = new List<BlockBase>();
    public TweenPosition _tPos;
    public TweenScale _tScale;
    public SkeletonAnimation _animaion;

    Vector3 offset = new Vector3(60f, 18f, 0f);

    void Awake()
    {
        instance = this;
        liveCount++;
    }

    IEnumerator Start()
    {
        _sprite.depth = 14;

        BlockBase.uniqueIndexCount++;
        used = false;

        if (type == GameItemType.HAMMER)
        {
            _sprite.enabled = true;
            _sprite.spriteName = "itemHammer";
        }
        else if (type == GameItemType.CROSS_LINE)
        {
            _sprite.enabled = true;
            _sprite.spriteName = "itembombbox";

        }
        else if (type == GameItemType.RAINBOW_BOMB_HAMMER)
        {
            _sprite.enabled = true;
            _sprite.spriteName = "item33RainbowHammer";
        }
        else if (type == GameItemType.THREE_HAMMER)
        {
            _sprite.enabled = true;
            _sprite.spriteName = "itemPowerHammer";
        }

        yield return new WaitForSeconds(0.5f);

        yield return null;
    }

    [System.NonSerialized]
    public bool used = false;
    bool endAnimation = false;

    public void SpineEndEvent(Spine.AnimationState state, int trackIndex)
    {
        string name = state.GetCurrent(trackIndex).Animation.Name;

        if (name == "Hammer_02" || name == "bombox_02" || name == "stick_02")
        {
            endAnimation = true;
        }

    }

    void OnDestroy()
    {
        if (GameItemManager.instance != null)
        {
            GameItemManager.instance.Close();
        }

        liveCount--;
        instance = null;
    }

    public void UseItem(BlockBase in_block)
    {
        if (in_block == null)
            return;

        if (used) return;

        used = true;
       // if (type == GameItemType.HAMMER)
        {
            StartCoroutine(CoUse(in_block));
        }
    }

    IEnumerator CoUse(BlockBase in_block)
    {
        _tPos.enabled = true;
        _tPos.from = transform.position;
        if (type == GameItemType.HAMMER || type == GameItemType.RAINBOW_BOMB_HAMMER || type == GameItemType.THREE_HAMMER)
            _tPos.to = in_block._transform.position + new Vector3(59, 21, 0) / 78 * Pick._startSize;
        else if (type == GameItemType.CROSS_LINE)
            _tPos.to = in_block._transform.position;// + new Vector3(59, 21, 0) / 78 * Pick._startSize;

        _tPos.worldSpace = true;
        _tPos.ResetToBeginning();
        _tPos.Play();

        if (type == GameItemType.HAMMER || type == GameItemType.CROSS_LINE || type == GameItemType.RAINBOW_BOMB_HAMMER || type == GameItemType.THREE_HAMMER)
        {
            if (in_block.expectType != BlockBombType.NONE)
                in_block.expectType = BlockBombType.NONE;

            in_block.JumpBlock();
            in_block.mainSprite.depth = (int) GimmickDepth.FX_EFFECT;

            if (in_block.specialEventSprite != null)
            {
                in_block.specialEventSprite.depth = in_block.mainSprite.depth + 2;
            }

            in_block.inGameItemUse = true;

            if (type == GameItemType.RAINBOW_BOMB_HAMMER)
                in_block.rainbowBombHammerUse = true;
            //사운드
        }  

        yield return new WaitForSeconds(0.5f);

        /*
        SkeletonAnimation tempAnimation = _animaion;
        if (type == GameItemType.HAMMER)
        {
            tempAnimation.state.SetAnimation(0, "start", false);
        }
        else if (type == GameItemType.CROSS_LINE)
        {
            tempAnimation.state.SetAnimation(0, "start", false);
        }
        else if (type == GameItemType.THREE_HAMMER)
        {
            tempAnimation.state.SetAnimation(0, "start", false);
        }
        else if (type == GameItemType.RAINBOW_BOMB_HAMMER)
        {
            tempAnimation.state.SetAnimation(0, "start", false);
        }
        else if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
        {
            _animaion.state.SetAnimation(0, "start", false);
        }
        _animaion.skeleton.Time = 0f;

        while (_animaion.skeleton.Time < 0.75f)//0.46f
            yield return null;

        ManagerSound.AudioPlay(AudioInGame.CLICK_BLOCK);

        while (_animaion.skeleton.Time < 0.9f)//0.46f
            yield return null;

        yield return new WaitForSeconds(0.75f);
        ManagerSound.AudioPlay(AudioInGame.CLICK_BLOCK);
        yield return new WaitForSeconds(0.9f);
        */

        if (in_block != null)
        {
            if (type == GameItemType.CROSS_LINE)
            {
                in_block.bombType = BlockBombType.CROSS_LINE;
            }
            else if (type == GameItemType.RAINBOW_BOMB_HAMMER)
            {
                in_block.bombType = BlockBombType.RAINBOW_X_BOMB_WITHSELF;
                in_block.rainbowColorType = in_block.colorType;
                in_block.isSkipDistroy = true;
                in_block._pangAlphaDelay = 0.3f;
                in_block._pangRemoveDelay = 1f;
            }
            else if (type == GameItemType.THREE_HAMMER)
            {
                in_block.bombType = BlockBombType.POWER_BOMB;
            }
            else if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
            {
                in_block.bombType = BlockBombType.RAINBOW;
                in_block.rainbowColorType = in_block.colorType;
            }
        }
        
        //while (_animaion.skeleton.Time < 0.6f)//0.72f
        //    yield return null;
        yield return new WaitForSeconds(0.6f);


        if (in_block != null)
        {
            //in_block.BlockPang(BlockBase.uniqueIndexCount);//
            bool isPangByBomb = (type != GameItemType.HAMMER);
            in_block.BlockPang(BlockBase.uniqueIndexCount, BlockColorType.NONE, isPangByBomb);
            in_block._pangRemoveDelay = 0.6f;

            ManagerBlock.instance.state = BlockManagrState.BEFORE_WAIT;
        }

        HideAllBlock();
    
        //BlockBase.uniqueIndexCount++;
        yield return new WaitForSeconds(0.3f);

        //보내기 아이템매니져에
        GameItemManager.instance.used = false;

        Destroy(gameObject);
        yield return null;
    }
    
    void HideAllBlock()
    {
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board back = PosHelper.GetBoardSreeen(j, i);
                if (back != null && back.Block != null && !back.HasDecoCoverBlock())
                {
                    //back.Block.UpdateSpriteByBlockType();
                    back.Block.SetMainSpriteDepth();
                    if (back.Block.type == BlockType.NORMAL)
                    {
                        NormalBlock normalBlock = back.Block as NormalBlock;
                        // if (normalBlock.toyBombSprite != null) normalBlock.toyBombSprite.depth = normalBlock.indexY * ManagerBlock.BLOCK_SRPRITE_DEPTH_COUNT + ManagerBlock.BLOCK_SRPRITE_MIN + 1;
                    }

                    if (back.Block.specialEventSprite != null)
                    {
                        back.Block.specialEventSprite.depth = back.Block.mainSprite.depth + 2;
                    }
                }
            }
        }
    }

}
