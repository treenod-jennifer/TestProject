using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Spine;
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
    public SkeletonAnimation _adventureAnimaion;
    public SkeletonAnimation _colorBrushAnimation;
    
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
            _animaion.gameObject.SetActive(true);
            _animaion.initialSkinName = "2";
            _animaion.Initialize(true);
            _animaion.state.SetAnimation(0, "idle", true);
        }
        else if (type == GameItemType.CROSS_LINE)
        {
            _animaion.gameObject.SetActive(true);
            _animaion.initialSkinName = "1";
            _animaion.Initialize(true);
            _animaion.state.SetAnimation(0, "idle", true);

        }
        else if (type == GameItemType.RAINBOW_BOMB_HAMMER)
        {
            _animaion.gameObject.SetActive(true);
            _animaion.initialSkinName = "4";
            _animaion.Initialize(true);
            _animaion.state.SetAnimation(0, "idle", true);
        }
        else if (type == GameItemType.THREE_HAMMER)
        {
            _animaion.gameObject.SetActive(true);
            _animaion.initialSkinName = "3";
            _animaion.Initialize(true);
            _animaion.state.SetAnimation(0, "idle", true);
        }
        else if (type == GameItemType.COLOR_BRUSH)
        {
            _colorBrushAnimation.gameObject.SetActive(true);
            _colorBrushAnimation.Initialize(true);
            _colorBrushAnimation.state.SetAnimation(0, "idle", true);
        }
        else if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            _adventureAnimaion.gameObject.SetActive(true);
            _adventureAnimaion.initialSkinName = "1";
            _adventureAnimaion.Initialize(true);
            _adventureAnimaion.state.SetAnimation(0, "idle", true);
        }
        else if (type == GameItemType.SKILL_HAMMER)
        {
            _adventureAnimaion.gameObject.SetActive(true);
            _adventureAnimaion.initialSkinName = "2";
            _adventureAnimaion.Initialize(true);
            _adventureAnimaion.state.SetAnimation(0, "idle", true);
        }
        else if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
        {
            _adventureAnimaion.gameObject.SetActive(true);
            _adventureAnimaion.initialSkinName = "3";
            _adventureAnimaion.Initialize(true);
            _adventureAnimaion.state.SetAnimation(0, "idle", true);
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

        if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
        {
            ManagerBlock.instance.state = BlockManagrState.BEFORE_WAIT;            
        }
        else if (type == GameItemType.COLOR_BRUSH)
        {
            StartCoroutine(CoUseColorBrush(in_block));
            return;
        }
        
        StartCoroutine(CoUse(in_block));  
    }

    IEnumerator CoUse(BlockBase in_block)
    {
        _tPos.enabled = true;
        _tPos.from = transform.position;
        if (type == GameItemType.HAMMER || type == GameItemType.RAINBOW_BOMB_HAMMER || type == GameItemType.THREE_HAMMER || type == GameItemType.ADVENTURE_RAINBOW_BOMB)
            _tPos.to = in_block._transform.position + new Vector3(59, 21, 0) / 78 * Pick._startSize;
        else if (type == GameItemType.CROSS_LINE)
            _tPos.to = in_block._transform.position;// + new Vector3(59, 21, 0) / 78 * Pick._startSize;

        _tPos.worldSpace = true;
        _tPos.ResetToBeginning();
        _tPos.Play();

        if (LanguageUtility.IsShowBuyInfo)
        {
            _tScale.from = new Vector2(0.8f, 0.8f);
            _tScale.to = new Vector2(1.0f, 1.0f);
            _tScale.Play();
        }

        _tScale.Play();

        if (type == GameItemType.HAMMER || type == GameItemType.CROSS_LINE || type == GameItemType.RAINBOW_BOMB_HAMMER || type == GameItemType.THREE_HAMMER || type == GameItemType.ADVENTURE_RAINBOW_BOMB)
        {
            if (in_block.expectType != BlockBombType.NONE)
                in_block.expectType = BlockBombType.NONE;

            in_block.JumpBlock();
            in_block.mainSprite.depth = (int) GimmickDepth.FX_EFFECT;

            if (in_block.specialEventSprite != null)
            {
                in_block.specialEventSprite.depth = in_block.mainSprite.depth + 2;
            }

            if (in_block.collectBlock_Alphabet != null)
            {
                in_block.collectBlock_Alphabet.SetDepth(in_block.mainSprite.depth + 2);
            }

            in_block.inGameItemUse = true;

            if (type == GameItemType.RAINBOW_BOMB_HAMMER)
                in_block.rainbowBombHammerUse = true;
            //사운드
        }  

        yield return new WaitForSeconds(0.5f);

        SkeletonAnimation tempAnimation = _animaion;
        if ((int)type >= (int)GameItemType.HEAL_ONE_ANIMAL)
            tempAnimation = _adventureAnimaion;

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
        else if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            tempAnimation.state.SetAnimation(0, "start", false);
        }
        else if (type == GameItemType.SKILL_HAMMER)
        {
            tempAnimation.state.SetAnimation(0, "start", false);
        }
        else if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
        {
            tempAnimation.state.SetAnimation(0, "start", false);
        }

        if(tempAnimation.gameObject.activeSelf == true)
        {
            tempAnimation.skeleton.Time = 0f;

            while (tempAnimation.skeleton.Time < 0.75f)//0.46f
                yield return null;

            ManagerSound.AudioPlay(AudioInGame.CLICK_BLOCK);

            while (tempAnimation.skeleton.Time < 0.9f)//0.46f
                yield return null;

        }

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
        if(tempAnimation.gameObject.activeSelf == true)
        {
            while (tempAnimation.skeleton.Time < 0.6f)//0.72f
                yield return null;
        }


        if (in_block != null)
        {
            bool isPangByBomb = (type != GameItemType.HAMMER);
            in_block.BlockPang(BlockBase.uniqueIndexCount, BlockColorType.NONE, isPangByBomb);
             
            in_block._pangRemoveDelay = 0.6f;
            ManagerBlock.instance.state = BlockManagrState.BEFORE_WAIT;
        }
        
        HideAllBlock();

        //BlockBase.uniqueIndexCount++;
        yield return new WaitForSeconds(0.3f);

        if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
        {
            while (ManagerBlock.instance.checkBlockWait() == false)
                yield return null;

            AdventureManager.instance.ItemAction(); 
        }

        //보내기 아이템매니져에
        GameItemManager.instance.used = false;

        Destroy(gameObject);
        yield return null;
    }

    private IEnumerator CoUseColorBrush(BlockBase inBlock)
    {
        _tPos.enabled    = true;
        _tPos.from       = transform.position;
        _tPos.to         = inBlock._transform.position;
        _tPos.worldSpace = true;
        _tPos.ResetToBeginning();
        _tPos.Play();

        if (LanguageUtility.IsShowBuyInfo)
        {
            _tScale.from = new Vector2(0.8f, 0.8f);
            _tScale.to   = new Vector2(1.0f, 1.0f);
        }

        _tScale.Play();

        yield return new WaitForSeconds(0.5f);

        var isComplete    = false;
        var tempAnimation = _colorBrushAnimation;

        if (inBlock != null)
        {
            tempAnimation.state.Event    += ColorBrushAnimationEventListener;
            tempAnimation.state.Complete += ColorBrushAnimationCompleteListener;
            
            ManagerSound.AudioPlay(AudioInGame.INGAME_ITEM_COLOR_BRUSH);
            tempAnimation.state.SetAnimation(0, "start", false);
            yield return new WaitUntil(() => isComplete);

            BlockMatchManager.instance.SetBlockLink();
        }

        HideAllBlock();
        
        if (inBlock != null)
        {
            ManagerBlock.instance.state = BlockManagrState.BEFORE_WAIT;
        }

        yield return new WaitForSeconds(0.3f);

        //보내기 아이템매니져에
        GameItemManager.instance.used = false;

        Destroy(gameObject);
        yield return null;

        void ColorBrushAnimationEventListener(TrackEntry trackentry, Spine.Event e)
        {
            if (string.Equals(e.Data.Name, "pain_1_1"))
            {
                if (inBlock != null)
                {
                    inBlock.UseColorBrush(BlockDirection.LEFT);
                }
            }
            else if (string.Equals(e.Data.Name, "paint_1"))
            {
                if (inBlock != null)
                {
                    inBlock.UseColorBrush(BlockDirection.RIGHT);
                }
            }
            else if (string.Equals(e.Data.Name, "paint_2_1"))
            {
                if (inBlock != null)
                {
                    inBlock.UseColorBrush(BlockDirection.UP);
                }
            }
            else if (string.Equals(e.Data.Name, "paint_2"))
            {
                if (inBlock != null)
                {
                    inBlock.UseColorBrush(BlockDirection.DOWN);
                }
            }
        }

        void ColorBrushAnimationCompleteListener(TrackEntry trackentry)
        {
            if (trackentry.Animation.Name == "start")
            {
                isComplete = true;

                tempAnimation.state.Event    -= ColorBrushAnimationEventListener;
                tempAnimation.state.Complete -= ColorBrushAnimationCompleteListener;
            }
        }
    }
    
    public void UseAnimalItem(InGameAnimal tempAnimal = null)
    {
        if (type == GameItemType.HEAL_ONE_ANIMAL && tempAnimal == null)
            return;

        if (used) return;
        used = true;

        StartCoroutine(CoAdventureUse(tempAnimal));
    }

    IEnumerator CoAdventureUse(InGameAnimal tempAnimal)
    {
        _tPos.enabled = true;
        _tPos.from = transform.position;

        if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            _tPos.to = tempAnimal.transform.position;
        }
        else if (type == GameItemType.SKILL_HAMMER)
        {
            _tPos.to = tempAnimal.skillItem.transform.position;
        }

        _tPos.worldSpace = true;
        _tPos.ResetToBeginning();
        _tPos.Play();

        yield return new WaitForSeconds(0.5f);

        if (type == GameItemType.HEAL_ONE_ANIMAL || type == GameItemType.SKILL_HAMMER)
        {
            _adventureAnimaion.state.SetAnimation(0, "start", false);
        }

        _adventureAnimaion.skeleton.Time = 0f;

        while (_adventureAnimaion.skeleton.Time < 0.75f)//0.46f
            yield return null;

        if (type == GameItemType.HEAL_ONE_ANIMAL || type == GameItemType.SKILL_HAMMER)
            ManagerSound.AudioPlay(AudioInGame.CLICK_BLOCK);

        while (_adventureAnimaion.skeleton.Time < 0.9f)//0.46f
            yield return null;

        if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            tempAnimal.ReviveAnimal(100);
        }
        else if (type == GameItemType.SKILL_HAMMER)
        {
            tempAnimal.skillItem.AddSkillPoint(100);
        }

        while (_adventureAnimaion.skeleton.Time < 0.6f)//0.72f
            yield return null;

        yield return new WaitForSeconds(0.3f);
        
        GameItemManager.instance.used = false;
        ManagerBlock.instance.state = BlockManagrState.BEFORE_WAIT;

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
