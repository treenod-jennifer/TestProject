using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum TreasureType
{
    None,
    Normal,
    Animal,
    Event
}

public class InGameTreasure : MonoBehaviour
{
    public UISprite treasureIcon;
    public UISprite treasureShadow;

    private bool isActive = false;

    public void SetTreasureImage(TreasureType treasureType = TreasureType.None)
    {
        treasureIcon.spriteName = string.Format("icon_treasureBox_{0}_01", treasureType.ToString());
        treasureIcon.MakePixelPerfect();
    }

    public IEnumerator CoDropAction()
    {
        treasureIcon.color = new Color(treasureIcon.color.r, treasureIcon.color.g, treasureIcon.color.b, 0f);
        DOTween.ToAlpha(() => treasureIcon.color, x => treasureIcon.color = x, 1f, 0.3f).SetEase(Ease.OutBack);

        treasureIcon.transform.localScale = Vector3.one * 0.2f;
        treasureIcon.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        
        treasureIcon.transform.DOLocalMoveY(50f, 0.15f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(0.15f);
        treasureIcon.transform.DOLocalMoveY(0f, 0.2f).SetEase(Ease.OutBack);

        isActive = true;
    }

    public void GetAction()
    {
        gameObject.SetActive(true);
        StartCoroutine(CoGetAction());
    }

    private IEnumerator CoGetAction()
    {
        while (!isActive)
            yield return new WaitForSeconds(0.1f);

        DOTween.ToAlpha(() => treasureIcon.color, x => treasureIcon.color = x, 0f, 0.3f).SetEase(Ease.OutBack);
        DOTween.ToAlpha(() => treasureShadow.color, x => treasureShadow.color = x, 0f, 0.3f).SetEase(Ease.OutBack);
        InGameEffectMaker.instance.MakeFlyMakeAdventureTreasure(transform.position, treasureIcon.spriteName);

        isActive = false;
    }
}
