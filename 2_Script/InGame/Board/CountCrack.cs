using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CountCrack : Crack, ICrack
{
    private string spriteName = "countCrack_";

    public void InitCountCrack(int indexX, int indexY, Board tempBoard, int index)
    {
        inX = indexX;
        inY = indexY;
        board = tempBoard;
        crackIndex = index;
        ManagerBlock.instance.AddCountCrackDictionary(crackIndex, this);
    }

    public override void SetSprite()
    {
        string name = string.Format("{0}{1}", spriteName, crackIndex);
        uiSprite.spriteName = name;
        uiSprite.depth = (int)GimmickDepth.DECO_AREA;
        MakePixelPerfect(uiSprite);
    }

    public new void SetCrackPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        //현재 활성화 된 단계석판이 아니라면 터지지 않음.
        if (ManagerBlock.instance.activeCountCrackIdx != crackIndex)
            return;

        if (pangIndex != uniquePang)
        {
            pangIndex = uniquePang;
            lifeCount--;
            //애니메이션
            InGameEffectMaker.instance.MakeEffectCountCrackPang(transform.position, crackIndex);
            ManagerSound.AudioPlayMany(AudioInGame.CRACK_PANG);
        }

        if (lifeCount <= 0)
        {
            //목표제거
            RemoveDeco();
        }
    }

    public override void RemoveDeco()
    {
        ManagerBlock.instance.AddScore(500);
        InGameEffectMaker.instance.MakeScore(transform.position, 500, 0.25f);

        TARGET_TYPE targetType = ManagerBlock.instance.GetCollectTypeCountCrackByInteger(crackIndex);
        ManagerBlock.instance.UpdateCollectTarget_PangCount(targetType);
        GameUIManager.instance.RefreshTarget(targetType);

        ManagerBlock.instance.listCrack.Remove(this);
        base.RemoveDeco();
    }

    public void ActiveCountCrack()
    {
        this.gameObject.SetActive(true);
        board.AddDeco(this);
        ManagerBlock.instance.listCrack.Add(this);
        DecoBase._listBoardDeco.Add(this);
    }

    public IEnumerator CoAppearAction(System.Action action = null)
    {
        ActiveCountCrack();
        float actionTime = ManagerBlock.instance.GetIngameTime(0.3f);
        uiSprite.transform.localScale = Vector3.zero;
        uiSprite.transform.DOScale(1.0f, actionTime).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(actionTime);
        action.Invoke();
    }
}
