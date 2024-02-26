using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Effect_BombPangField : ActiveHelperEffect
{
    [SerializeField] private UISprite spriteBombField;
    [SerializeField] private GameObject particleObj;

    protected override void OnEnable()
    {
        //폭탄 영역 알파 연출
        spriteBombField.alpha = 0;
        particleObj.SetActive(false);
    }

    protected override IEnumerator CoInActiveTimer()
    {
        DOTween.ToAlpha(() => spriteBombField.color, x => spriteBombField.color = x, 0.5f, ManagerBlock.instance.GetIngameTime(0.1f));

        //제거되는 타이밍까지 대기
        float waitTime = ManagerBlock.instance.GetIngameTime(destroyTime + destroyTimeOffset);
        yield return new WaitForSeconds(waitTime);
        particleObj.SetActive(true);

        //제거되는 연출 후 오브젝트 풀로 돌려줌
        DOTween.ToAlpha(() => spriteBombField.color, x => spriteBombField.color = x, 0f, ManagerBlock.instance.GetIngameTime(0.3f));
        yield return new WaitForSeconds(ManagerBlock.instance.GetIngameTime(0.5f));
        InGameObjectPoolManager.instance.ObjectPoolDeSpawn(effectType, this);
        endAction?.Invoke();
        activeRoutine = null;
    }
}
