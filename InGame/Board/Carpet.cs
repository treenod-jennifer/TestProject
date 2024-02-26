using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carpet : DecoBase
{
    [SerializeField]    GameObject effectRoot;
    [SerializeField]    TweenAlpha TwAlpha;

    public override void SetSprite()
    {
        uiSprite.depth = (int)GimmickDepth.DECO_LAND;
    }

    public void ShowCarpet(float delay)
    {
        uiSprite.alpha = 0;
        StartCoroutine(CoScaleAction(delay));
    }

    private IEnumerator CoScaleAction(float delay)
    {
        yield return new WaitForSeconds(delay);
        ManagerSound.AudioPlayMany(AudioInGame.WOOL_MAKE);
        effectRoot.SetActive(true);
        TwAlpha.enabled = true;
        InGameEffectMaker.instance.MakeCarpetEffect(transform.position);
    }

    public void CheckBoard(bool destroyGimik)
    {
        if (destroyGimik)
        {
            uiSprite.cachedTransform.localScale = Vector3.one;
        }
        else
        {
            if (PosHelper.GetBlock(inX, inY) != null &&
                (PosHelper.GetBlock(inX, inY).IsThisBlockHasPlace() == true))
            {
                uiSprite.cachedTransform.localScale = Vector3.one * 0.9f;
            }
            else
                uiSprite.cachedTransform.localScale = Vector3.one;
        }           
    }
}
