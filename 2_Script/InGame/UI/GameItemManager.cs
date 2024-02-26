using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Protocol;
using Newtonsoft.Json;

public class GameItemManager : MonoBehaviour
{
    public const int HAMMER_OPEN_STAGE = 8;
    public const int CROSS_LINE_OPEN_STAGE = 13;
    public const int RAINBOW_HAMMER_OPEN_STAGE = int.MaxValue;
    public const int HAMMER3X3_OPEN_STAGE = 36;

    [System.NonSerialized]
    public static GameItemManager instance = null;

    [System.NonSerialized]
    public Transform _transform;

    [System.NonSerialized]
    public GameItemType type = GameItemType.NONE;

    public GameObject flyGameItemObj;
    public UISprite _uiClipping;
    public UISprite _CenterClipping;
    public UILabel _textMessage;

    public GameItem useGameItem = null;

    [System.NonSerialized]
    public BlockBase _selectBlock;
    InGameAnimal selectAnimal;

    [System.NonSerialized]
    public bool used = false;

    [System.NonSerialized]
    public FlyGameItem _flyItem;

    public static int[] useCount = new int[7]{0,0,0,0,0,0,0};

    void Awake()
    {
        instance = this;
        _transform = transform;
        _selectBlock = null;
        used = false;
    }

    void Start()
    {
        _CenterClipping.gameObject.transform.parent = GameUIManager.instance.groundAnchor.transform;
        _CenterClipping.gameObject.transform.localScale = Vector3.one;
        _CenterClipping.gameObject.transform.localPosition = new Vector3(0, -GameManager.MOVE_Y * 78, 0);
        StartCoroutine(FadeOut());

        NGUITools.SetLayer(_CenterClipping.gameObject, LayerMask.NameToLayer("InGame"));
        NGUITools.MarkParentAsChanged(_CenterClipping.gameObject);

        GameUIManager.instance.ShowFlower(false);        
    }

    public void SetType(GameItemType temptype)
    {
        type = temptype;

        if (type == GameItemType.HEAL_ONE_ANIMAL ||
           type == GameItemType.SKILL_POINT_CHARGE)
        {

        }
        else
        {            
            ShowBlock();
        }
    }



    public void BtnCancel()
    {
        if (used) return;

        if (_CenterClipping != null)
            Destroy(_CenterClipping.gameObject);

        GameUIManager.instance.RefreshInGameItem();
        GameUIManager.instance.ShowFlower(true);

        Destroy(_flyItem.gameObject);
        Destroy(gameObject);
    }

    public void Close()
    {
        if (_CenterClipping != null)
            Destroy(_CenterClipping.gameObject);
        GameUIManager.instance.ShowFlower(true);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        HideAllBlock();
        GameUIManager.instance.ShowFlower(true);
        instance = null;
    }

    bool isFreeItem = false;
    public void UseGameItem(BlockBase tempBlock)
    {
        if (tempBlock == null) return;

        if (used) return;
        used = true;

        Board tempBoard = PosHelper.GetBoardSreeen(tempBlock.indexX, tempBlock.indexY);
        if (tempBoard == null) return;
        if (tempBoard.HasDecoHideBlock()) return;
        if (tempBoard.HasDecoCoverBlock()) return;

        selectBlock = tempBlock;
        
        if (EditManager.instance == null)
        {
        	/*
            //그로씨 머니 작성
            if (type != GameItemType.ADVENTURE_RAINBOW_BOMB)
            {
                int itemIndex = (int)type - 1;
                if (ServerRepos.UserItem.InGameItem(itemIndex) == 0)
                {
                    if ((int)ServerRepos.User.jewel > ServerRepos.LoginCdn.InGameItems[itemIndex])
                    {
                        usePJewel = ServerRepos.LoginCdn.InGameItems[itemIndex];
                    }
                    else if ((int)ServerRepos.User.jewel > 0)
                    {
                        usePJewel = (int)ServerRepos.User.jewel;
                        useFJewel = ServerRepos.LoginCdn.InGameItems[itemIndex] - (int)ServerRepos.User.jewel;
                    }
                    else
                    {
                        useFJewel = ServerRepos.LoginCdn.InGameItems[itemIndex];
                    }
                }
                else
                {
                    isFreeItem = true;
                }
            }
            else
            {
                int itemIndex = (int)type - 4;
                int itemInGameIdx = (int)type - 1;

                if (ServerRepos.UserItem.AdventureItem(itemIndex) == 0)
                {
                    if ((int)ServerRepos.User.jewel > ServerRepos.LoginCdn.InGameItems[itemIndex])
                    {
                        usePJewel = ServerRepos.LoginCdn.InGameItems[itemIndex];
                    }
                    else if ((int)ServerRepos.User.jewel > 0)
                    {
                        usePJewel = (int)ServerRepos.User.jewel;
                        useFJewel = ServerRepos.LoginCdn.InGameItems[itemIndex] - (int)ServerRepos.User.jewel;
                    }
                    else
                    {
                        useFJewel = ServerRepos.LoginCdn.InGameItems[itemIndex];
                    }
                }
                else
                {
                    isFreeItem = true;
                }
            }

            if (type == GameItemType.HAMMER) ServerAPI.UseInGameItem(6, recvGameItem);
            else if (type == GameItemType.CROSS_LINE) ServerAPI.UseInGameItem(7, recvGameItem);
            else if (type == GameItemType.THREE_HAMMER) ServerAPI.UseInGameItem(8, recvGameItem);//
            else if (type == GameItemType.RAINBOW_BOMB) ServerAPI.UseInGameItem(9, recvGameItem);//RAINBOW_BOMB
            else if (type == GameItemType.ADVENTURE_RAINBOW_BOMB) ServerAPI.AdventureUseInGameItem(13, recvGameItem);

            InGameEffectMaker.instance.MakeBombMakePangEffect(tempBlock._transform.position);
            */
        }
        else
        {
            isFreeItem = true;

            //BaseResp tempResp = new BaseResp();
            //tempResp.code = (int)ServerError.Success;
            //recvGameItem(tempResp);
            recvGameItem();
            InGameEffectMaker.instance.MakeBombMakePangEffect(tempBlock._transform.position);
        }
    }


    public void UseAnimalItem(InGameAnimal tempAnimal = null)
    {
        if (used) return;
        used = true;
        /*
        if (EditManager.instance == null)
        {
            int itemIndex = (int)type - 4;
            int itemInGameIdx = (int)type - 1;

            if (ServerRepos.UserItem.AdventureItem(itemIndex) == 0)
            {
                if ((int)ServerRepos.User.jewel > ServerRepos.LoginCdn.InGameItems[itemInGameIdx])
                {
                    usePJewel = ServerRepos.LoginCdn.InGameItems[itemInGameIdx];
                }
                else if ((int)ServerRepos.User.jewel > 0)
                {
                    usePJewel = (int)ServerRepos.User.jewel;
                    useFJewel = ServerRepos.LoginCdn.InGameItems[itemInGameIdx] - (int)ServerRepos.User.jewel;
                }
                else
                {
                    useFJewel = ServerRepos.LoginCdn.InGameItems[itemInGameIdx];
                }
            }
            else
            {
                isFreeItem = true;
            }

            selectAnimal = tempAnimal;

            if (type == GameItemType.HEAL_ONE_ANIMAL) ServerAPI.AdventureUseInGameItem(11, recvAdventureItem);
            else if (type == GameItemType.SKILL_POINT_CHARGE) ServerAPI.AdventureUseInGameItem(12, recvAdventureItem);
        }*/
    }


    BlockBase selectBlock = null;
    int useFJewel = 0;
    int usePJewel = 0;

    /*
    void recvAdventureItem(BaseResp code)
    {
        if (code.IsSuccess)
        {
            StartCoroutine(FadeIn());

            if (useGameItem.type == GameItemType.HEAL_ONE_ANIMAL)
            {
                useCount[4]++;
                //selectAnimal.RecoverAnimal(100);
            }
            else if (useGameItem.type == GameItemType.SKILL_POINT_CHARGE)
            {
                useCount[5]++;
                AdventureManager.instance.ChargeAllSkillPoint();
            }

            useGameItem.useItem();
            
            GameUIManager.instance.RefreshInGameItem();
            FlyGameItem.instance.UseAnimalItem(selectAnimal);
            ManagerSound.AudioPlay(AudioInGame.INGAME_ITEM_CLICK);

            Global.jewel = (int)GameData.Asset.AllJewel;
            Global.wing = (int)GameData.Asset.AllWing;
            Global.exp = (int)GameData.User.expBall;

            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();
        }
        else
        {
            used = false;
            BtnCancel();
        }
    }

*/
    
    void recvGameItem()//BaseResp code)
    {
        //if (code.IsSuccess)
        {
            //used = false;
            StartCoroutine(FadeIn());

            if (useGameItem.type == GameItemType.HAMMER)            
                useCount[0]++;
            if (useGameItem.type == GameItemType.CROSS_LINE)
                useCount[1]++;
            if (useGameItem.type == GameItemType.THREE_HAMMER)
                useCount[2]++;
            if (useGameItem.type == GameItemType.RAINBOW_BOMB_HAMMER)//RAINBOW_BOMB
                useCount[3]++;
            if (useGameItem.type == GameItemType.ADVENTURE_RAINBOW_BOMB)//RAINBOW_BOMB
                useCount[6]++;

            //useGameItem.useItem();
            GameUIManager.instance.RefreshInGameItem();
            FlyGameItem.instance.UseItem(selectBlock);
            ManagerSound.AudioPlay(AudioInGame.INGAME_ITEM_CLICK);


            if (ManagerUI._instance != null)
                ManagerUI._instance.UpdateUI();
        }
        //else
        //{
        //    used = false;
        //    BtnCancel();
        //}
    }

    public void CreateItem()
    {
        if(type == GameItemType.HAMMER)
        {
            _textMessage.text = Global._instance.GetString("s_i_1");
        }
        else if (type == GameItemType.CROSS_LINE )
        {
            _textMessage.text = Global._instance.GetString("s_i_2");
        }
        else if (type == GameItemType.THREE_HAMMER)
        {
            _textMessage.text = Global._instance.GetString("s_i_3");
        }
        else if (type == GameItemType.RAINBOW_BOMB_HAMMER)
        {
            _textMessage.text = Global._instance.GetString("s_i_4");
        }
        //TODO 모험모드 아이템 추가
        else if (type == GameItemType.HEAL_ONE_ANIMAL)
        {
            _textMessage.text = Global._instance.GetString("ad_i_1");
        }
        else if (type == GameItemType.SKILL_POINT_CHARGE)
        {
            _textMessage.text = Global._instance.GetString("ad_i_2");
            UseAnimalItem();
        }
        else if (type == GameItemType.ADVENTURE_RAINBOW_BOMB)
        {
            _textMessage.text = Global._instance.GetString("ad_i_3");
        }
                
        CreateFlyItem();
    }

    void CreateFlyItem()
    {
        if (FlyGameItem.instance == null)
        {
            _flyItem = NGUITools.AddChild(GameUIManager.instance.AnchorBottom.gameObject, flyGameItemObj).GetComponent<FlyGameItem>();
            _flyItem.transform.localPosition = new Vector3(0,140,0);
            _flyItem.type = type;
        }
    }


    void ShowBlock()
    {
        for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
        {
            for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
            {
                Board back = PosHelper.GetBoardSreeen(j, i);
                if (back != null && back.Block != null && !back.HasDecoHideBlock() && !back.HasDecoCoverBlock())
                {
                    if (back.Block.IsNormalBlock() && back.Block as NormalBlock
                        && (back.Block.blockDeco == null || back.Block.blockDeco.IsInterruptBlockSelect() == false))
                    {
                        back.Block.mainSprite.depth = (int)GimmickDepth.INGAMEITEM + 1;
                        if (back.Block.type == BlockType.NORMAL)
                        {
                            NormalBlock normalBlock = back.Block as NormalBlock;
                            if(normalBlock.toyBombSprite != null) normalBlock.toyBombSprite.depth = (int)GimmickDepth.INGAMEITEM + 2;
                        }

                        if (back.Block.specialEventSprite != null)
                        {
                            back.Block.specialEventSprite.depth = (int)GimmickDepth.INGAMEITEM + 3;
                        }
                    }
                }
            }
        }
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

    IEnumerator FadeOut(float speed = 1.2f)
    {
        var color = _CenterClipping.color;
        color.a = 0.01f;
        _CenterClipping.color = color;

        float time = 0f;

        while (true)
        {
            if (time > 1f)
                break;

            time += Global.deltaTimePuzzle;
            _CenterClipping.color = new Color(1f, 1, 1f, Mathf.Lerp(0f, 0.4f, time * 4f));
            yield return null;
        }

        _CenterClipping.color = new Color(1f, 1f, 1f, 0.4f);
        yield return null;
    }

    IEnumerator FadeIn(float speed = 1.5f)
    {
        var color = _CenterClipping.color;
        float time = 0f;
        while (true)
        {
            if (time > 1f)
                break;

            time += Global.deltaTimePuzzle;
            _CenterClipping.color = new Color(1f, 1f, 1f, Mathf.Lerp(color.a, 0f, time * 4f));
            yield return null;
        }

        
        _CenterClipping.color = new Color(1f, 1f, 1f, 0f);
        yield return null;

        /*
        if (EditManager.instance != null)
            used = false;
        */

        while (used)
        {
            yield return new WaitForSeconds(0.1f);
        }
        GameUIManager.instance.ShowFlower(true);
        yield return null;
        Destroy(gameObject);
        yield return null;
    }


}
