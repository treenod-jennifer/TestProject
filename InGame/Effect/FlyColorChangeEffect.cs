using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FlyColorChangeEffect : MonoBehaviour
{
    [SerializeField] private Color[] effectColorTint;
    [SerializeField] private Color[] trailColorTint;

    [SerializeField] private UISprite colorSprite;
    [SerializeField] private GameObject[] colorEffect;
    [SerializeField] private TrailRenderer trailRenderer;

    private Vector3 targetPos = Vector3.zero;
    private BlockColorType effectColor = BlockColorType.NONE;
    private GameObject hitColorObj = null;

    public void InitEffect(Vector3 tPos, BlockColorType colorType)
    {
        targetPos = tPos;
        effectColor = colorType;
        SetColor();
        StartCoroutine(CoActionEffect());
    }
    
    public void InitSimpleEffect(BlockColorType colorType)
    {
        effectColor = colorType;
        SetColor();
        StartCoroutine(CoActionSimpleEffect());
    }
    
    private void SetColor()
    {
        int colorIdx = (int)effectColor - 1;
        colorSprite.color = effectColorTint[colorIdx];
        colorSprite.gameObject.SetActive(true);
        hitColorObj = colorEffect[colorIdx];
        trailRenderer.startColor = trailColorTint[colorIdx];
        trailRenderer.endColor = trailColorTint[colorIdx];
    }

    private IEnumerator CoActionEffect()
    {
        float offsetX = (targetPos.x - transform.localPosition.x) * 0.5f + 10f;
        float offsetY = (targetPos.y - transform.localPosition.y) * 0.5f + 10f;

        Vector3 control_1 = new Vector3(transform.localPosition.x - offsetX, transform.localPosition.y + offsetY, 0f);
        Vector3 control_2 = new Vector3(targetPos.x - offsetX, targetPos.y + offsetY, 0f);
        Vector3[] pathWayPoints = new[] { targetPos, control_1, control_2 };

        //타겟위치까지 이펙트 이동
        float moveTime = ManagerBlock.instance.GetIngameTime(0.3f);
        transform.DOLocalPath(pathWayPoints, moveTime, PathType.CubicBezier, PathMode.Ignore);
        yield return new WaitForSeconds(moveTime);

        //사운드 재생
        ManagerSound.AudioPlayMany(AudioInGame.WATER_PANG);
        hitColorObj.SetActive(true);

        float actionTime = ManagerBlock.instance.GetIngameTime(0.3f);
        DOTween.ToAlpha(() => colorSprite.color, x => colorSprite.color = x, 0f, actionTime);
        colorSprite.transform.DOScale(Vector3.one * 3f, actionTime);
        yield return new WaitForSeconds(actionTime);

        actionTime = ManagerBlock.instance.GetIngameTime(0.3f);
        yield return new WaitForSeconds(actionTime);
        Destroy(gameObject);
    }
    
    private IEnumerator CoActionSimpleEffect()
    {
        hitColorObj.SetActive(true);

        var actionTime = ManagerBlock.instance.GetIngameTime(0.3f);
        DOTween.ToAlpha(() => colorSprite.color, x => colorSprite.color = x, 0f, actionTime);
        colorSprite.transform.DOScale(Vector3.one * 3f, actionTime);
        yield return new WaitForSeconds(actionTime);
        
        Destroy(gameObject);
    }
}
