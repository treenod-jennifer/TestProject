using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crack : DecoBase , ICrack
{
    const string SPRITE_NAME = "crack0";
    
    public BlockColorType colorType = BlockColorType.NONE;
    public int crackIndex = 0;
    
    public override void SetSprite()
    {
        uiSprite.spriteName = string.Format("{0}{1}", SPRITE_NAME, lifeCount);
        uiSprite.depth = (int)GimmickDepth.DECO_AREA;
        MakePixelPerfect(uiSprite);
    }

    public void SetCrackPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE)
    {
        if (pangIndex != uniquePang)
        {
            if (colorType != BlockColorType.NONE)
            {
                if (colorType == pangColorType)
                {
                    pangIndex = uniquePang;
                    lifeCount--;
                    //애니메이션
                    InGameEffectMaker.instance.MakeCrackParticle(transform.position);
                    ManagerSound.AudioPlayMany(AudioInGame.CRACK_PANG);

                }
            }
            else
            {
                pangIndex = uniquePang;
                lifeCount--;
                //애니메이션
                InGameEffectMaker.instance.MakeCrackParticle(transform.position);
                ManagerSound.AudioPlayMany(AudioInGame.CRACK_PANG);
            }
        }

        if (lifeCount <= 0)
        {
            //목표제거
            RemoveDeco();
        }
    }

    public override void RemoveDeco()
    {
        InGameEffectMaker.instance.MakeScore(transform.position, 500, 0.25f);
        ManagerBlock.instance.AddScore(500);

        ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.CRACK);
        GameUIManager.instance.RefreshTarget(TARGET_TYPE.CRACK);

        ManagerBlock.instance.listCrack.Remove(this);
        base.RemoveDeco();
    }

    public override bool IsTarget_LavaMode()
    {
        return true;
    }

    bool OnWarring = false;

    public void OnLavaWarring(bool isWarring = true)
    {
        if (isWarring == true && OnWarring == false)
        {
            OnWarring = true;
            StartCoroutine(CoWarring());
        }
        else if (isWarring == false && OnWarring == true)
        {
            OnWarring = false;
        }
    }

    IEnumerator CoWarring()
    {
        while (true)
        {
            if (OnWarring == false)
                break;

            uiSprite.color = Color.white * (0.8f + Mathf.Sin(Time.time * 25f) * 0.2f);
            yield return null;
        }

        uiSprite.color = Color.white;
        yield return null;
    }
}
