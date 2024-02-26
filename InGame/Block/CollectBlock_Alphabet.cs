using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CollectBlock_Alphabet : MonoBehaviour
{
    [SerializeField]
    private UIBlockSprite sprite_alphabet;
    [SerializeField]
    private UIBlockUrlTexture urlTexture_alphabet;
    [SerializeField]
    private TweenRotation tween;

    private int alphabetIdx = 0;

    private bool isNormalAlphabet = true;

    //알파벳 인덱스 및 설정으로 초기화.
    public void InitCollectBlock(int alphabetIdx, int depth, bool isMakeStartBoard, bool isNormalAlphabet = true)
    {
        this.alphabetIdx = alphabetIdx;
        this.isNormalAlphabet = isNormalAlphabet;

        string blockName = (isNormalAlphabet == true) ?
            ManagerAlphabetEvent.instance.GetAlphabetSpriteName_N(alphabetIdx) :
            ManagerAlphabetEvent.alphabetIngame.GetAppearAlphabetSpriteName_S();

        SetCollectBlockImage(blockName, isMakeStartBoard);
        SetDepth(depth);
        InitPosAndScale();
        InitTween();
    }

    //다른 알파벳 블럭과 같은 설정으로 초기화.
    public void InitCollectBlock(CollectBlock_Alphabet originBlock)
    {
        InitCollectBlock(originBlock.alphabetIdx, originBlock.GetDepth(), false, originBlock.isNormalAlphabet);
    }

    private void InitPosAndScale()
    {
        transform.localPosition = new Vector3(25f, -15f, 0f);
        transform.localScale = Vector3.one * 0.9f;
    }

    private void InitTween()
    {
        bool isNewAlphabet = false;
        //현재 알파벳 획득 상태에 따라 트윈 활성화.
        if (isNormalAlphabet == true)
        {
            for (int i = 0; i < ManagerAlphabetEvent.alphabetIngame.listAlphabetData_N.Count; i++)
            {
                ManagerAlphabetEvent.AlphabetIngame.AlphabetData tempData = ManagerAlphabetEvent.alphabetIngame.listAlphabetData_N[i];
                if (tempData.index == alphabetIdx && tempData.getCount_All == 0)
                {
                    isNewAlphabet = true;
                    break;
                }
            }
        }
        else
        {
            if (ManagerAlphabetEvent.alphabetIngame.alphabetData_S.getCount_All == 0)
                isNewAlphabet = true;
        }

        tween.enabled = isNewAlphabet;
    }

    private void SetCollectBlockImage(string blockName, bool isMakeStartBoard)
    {
        sprite_alphabet.gameObject.SetActive(isNormalAlphabet);
        urlTexture_alphabet.gameObject.SetActive(!isNormalAlphabet);

        if (isNormalAlphabet == true)
        {
            sprite_alphabet.spriteName = blockName;
            sprite_alphabet.MakePixelPerfect();

            if (isMakeStartBoard == true)
                sprite_alphabet.customFill.blockRatio = (ManagerBlock.instance.stageInfo.reverseMove == 1) ? -1 : 1;
        }
        else
        {
            urlTexture_alphabet.LoadCDN(Global.gameImageDirectory, "IconEvent/", blockName);
            if (isMakeStartBoard == true)
                urlTexture_alphabet.customFill.blockRatio = (ManagerBlock.instance.stageInfo.reverseMove == 1) ? -1 : 1;
        }
    }

    public void SetCollectBlockImageColor(Color color)
    {
        if (isNormalAlphabet == true)
            sprite_alphabet.color = color;
        else
            urlTexture_alphabet.color = color;
    }

    private int GetDepth()
    {
        if (isNormalAlphabet == true)
            return sprite_alphabet.depth;
        else
            return urlTexture_alphabet.depth;
    }

    public void SetDepth(int depth)
    {
        if (isNormalAlphabet == true)
            sprite_alphabet.depth = depth;
        else
            urlTexture_alphabet.depth = depth;
    }

    public CustomFill GetCustomFill()
    {
        if (isNormalAlphabet == true)
            return sprite_alphabet.customFill;
        else
            return urlTexture_alphabet.customFill;
    }

    public void Action_Flash(Color targetColor, float actionTime, int loopCount, LoopType loopType)
    {
        if (isNormalAlphabet == true)
            DOTween.To(() => sprite_alphabet.color, x => sprite_alphabet.color = x, targetColor, actionTime).SetLoops(loopCount, loopType);
        else
            DOTween.To(() => urlTexture_alphabet.color, x => urlTexture_alphabet.color = x, targetColor, actionTime).SetLoops(loopCount, loopType);
    }

    public void GainCollectBlock()
    {
        ManagerAlphabetEvent.alphabetIngame.GainCollectItem_Alphabet(alphabetIdx, isNormalAlphabet, transform.position);
    }
}
