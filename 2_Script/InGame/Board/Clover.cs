using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clover : DecoBase, IHide
{
    [SerializeField] private List<UISprite> cloverLeaf;
    [SerializeField] private UISprite cloverDepthLeaf;

    public bool isPang = false;

    public bool IsRainbowBomb { get; set; } = false;

    [System.Serializable] 
    private class IdleActionData
    {
        public List<TweenScale> leafScale;
        public List<TweenRotation> leafRotation;

        public void SetActive()
        {
            for (int i = 0; i < leafScale.Count; i++)
            {
                if (leafScale[i].enabled) return;
                
                leafScale[i].PlayForward();
                leafScale[i].ResetToBeginning();
                
                leafRotation[i].PlayForward();
                leafRotation[i].ResetToBeginning();
            }
        }
    }

    [SerializeField] private IdleActionData idleAction;
    
    public override void SetSprite()
    {
        uiSprite.depth = (int)GimmickDepth.DECO_HIDE;    //inY * ManagerBlock.BLOCK_SRPRITE_DEPTH_COUNT + ManagerBlock.BLOCK_SRPRITE_MIN + ManagerBlock.NET_RPRITE_MIN;

        cloverDepthLeaf.depth = uiSprite.depth + 1;
        
        for (int i = 0; i < cloverLeaf.Count; i++)
        {
            cloverLeaf[i].depth = cloverDepthLeaf.depth + (i + 1);
        }

        this.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        if (GameManager.instance.state == GameState.EDIT)
        {
            uiSprite.alpha = 0.4f;
            return;
        }
    }

    public void SetBlockSpriteEnable()
    {
        if (GameManager.instance.state == GameState.EDIT)
        {
            uiSprite.alpha = 0.4f;
            return;
        }
        
        if (board.Block != null)
        {
            board.Block.SetCloverHide(false);
        }
        
        foreach (var item in board.DecoOnBoard)
            if (item is Water)
                item.SetSpriteEnabled(false);
    }

    public override bool IsCoverCarpet()
    {
        return true;
    }
    
    public override bool IsCoverStatue()
    {
        return true;
    }
    
    public bool IsHideDeco()
    {
        return true;
    }

    public override bool IsCanFill()
    {
        return true;
    }
    
    public override bool IsWarpBlock()
    {
        return true;
    }

    public override bool IsInterruptBlockEvent()
    {
        return true;
    }

    public override bool IsInterruptBlockColorChange()
    {
        return true;
    }

    public override bool IsDisturbPangByRainbowBomb()
    {
        return false;
    }

    public override bool EventAction()
    {
        if (board.Block != null)
        {
            board.Block.SetCloverHide(false);

            if (board.Block.IsStopEventAtOutClover())
                board.Block.isStopEvent = true;
        }
        
        return false;
    }

    public override bool SetSplashPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE, bool bombEffect = false)
    {
        if (bombEffect) return true;

        if (lifeCount <= 0) return true;

        if(pangIndex != uniquePang)
        {
            pangIndex = uniquePang;
            lifeCount--;

            StartCoroutine(CoPangCloverFinal());
        }

        return true;
    }
    
    public void SetHideDecoPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if(lifeCount <= 0) return;
        
        if (pangIndex != uniquePang)
        {
            pangIndex = uniquePang;
            lifeCount--; 

            StartCoroutine(CoPangCloverFinal());
        }
    }
    
    //연출
    public IEnumerator CoPangCloverFinal()
    {
        isPang = true;
        float timer = 0f;

        var currentBlock = board.Block;
        var decoOnBoard = board.DecoOnBoard;

        if (currentBlock != null)
            currentBlock.SetHideBeforeCloverPang();
        // 클로버보다 크기가 큰 기믹은 미리 활성화 (물)
        if (decoOnBoard != null)
        {
            foreach (var item in decoOnBoard)
                if (item is Water)
                    item.SetSpriteEnabled(true);
        }

        InGameEffectMaker.instance.MakeCloverEffect(transform.position);
        ManagerSound.AudioPlayMany(AudioInGame.PLANT_PANG3);
        
        ManagerBlock.instance.AddScore(30);
        InGameEffectMaker.instance.MakeScore(transform.position, 30, 0.25f);

        while (timer < 1f)
        {
            uiSprite.transform.localScale = Vector3.one * ManagerBlock.instance._curveTreeScaleOut.Evaluate(timer);
            timer += Global.deltaTimePuzzle * 3f;
            uiSprite.color = new Color(1f, 1f, 1f, 1f - timer);
            yield return null;
        }

        if (board.DecoOnBoard != null)
        {
            Water water = null;
            
            foreach (var deco in board.DecoOnBoard)
            {
                if (water == null) water = deco as Water;

                if (water != null)
                {
                    ManagerBlock.instance.GetWater = true;
                    ManagerSound.AudioPlayMany(AudioInGame.WATER_MAKE);
                }
            }   
        }

        board.RemoveDeco(this);

        //목표제거
        //아래에 석상있는지 체크후 석상작동
        board.CheckStatus();

        //클로버 안 기믹 동작 시작
        if (currentBlock != null)
        {
            //클로버 안에 있는 블럭들을 hide 시킨다면 필요.
            currentBlock.SetCloverHide(true);

            //클로버가 사라지고 난 이후 필요한 처리 ex) 컬러화단
            currentBlock.HideDecoRemoveAction();
            
            if (currentBlock.IsStopEventAtOutClover())
                currentBlock.isStopEvent = true;
        }

        isPang = false;
        ManagerBlock.instance.listClover.Remove(this);
        RemoveDeco();
        
        yield return null;
    }

    IEnumerator CoIdleAction()
    {
        yield return new WaitForSeconds(Random.Range(0, 5));

        while (this.gameObject)
        {
            idleAction.SetActive();
            
            yield return new WaitForSeconds(Random.Range(10, 15));
        }
        
    }

    public override void Init()
    {
        StartCoroutine(CoIdleAction());
    }

    public override IEnumerator CoFlashDeco_Color(int actionCount, float actionTIme, float waitTime)
    {
        float targetColorValue = 0.6f;
        Color targetColor = new Color(targetColorValue, targetColorValue, targetColorValue);

        float aTime = (actionTIme / actionCount) * 0.5f;

        for (int i = 0; i < actionCount; i++)
        {
            DOTween.To(() => uiSprite.color, x => uiSprite.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            DOTween.To(() => cloverLeaf[0].color, x => cloverLeaf[0].color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            DOTween.To(() => cloverLeaf[1].color, x => cloverLeaf[1].color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            DOTween.To(() => cloverLeaf[2].color, x => cloverLeaf[2].color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            DOTween.To(() => cloverDepthLeaf.color, x => cloverDepthLeaf.color = x, targetColor, aTime).SetLoops(2, LoopType.Yoyo);
            float waitTIme = ((i + 1) < actionCount) ? (aTime * 2) + waitTime : (aTime * 2);
            yield return new WaitForSeconds(waitTIme);
        }

        uiSprite.color = Color.white;
        cloverLeaf[0].color = Color.white;
        cloverLeaf[1].color = Color.white;
        cloverLeaf[2].color = Color.white;
        cloverDepthLeaf.color = Color.white;
        yield return null;
    }
}

