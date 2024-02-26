using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemFlyEffect : MonoBehaviour
{
    [SerializeField] private Transform _transform;
    [SerializeField] private UISprite spriteImage;
    [SerializeField] private UITexture textureImage;
    [SerializeField] private ParticleSystem effectRing;
    [SerializeField] private ParticleSystem effectParticle;
    [SerializeField] private Renderer rendererEffectRing;
    [SerializeField] private Renderer rendererEffectParticle;

    private System.Action endAction = null;

    #region 이미지 설정 관련
    public void SetSpriteImage(string spriteName, int depth, float size = -1f, NGUIAtlas atlas = null)
    {
        //이미지 활성화
        spriteImage.gameObject.SetActive(true);

        //아틀라스 설정
        if (atlas != null)
            spriteImage.atlas = atlas;

        //이미지 설정
        spriteImage.spriteName = spriteName;

        //뎁스 설정
        spriteImage.depth = depth;

        //사이즈 설정
        spriteImage.MakePixelPerfect();
        if (size > -1)
            spriteImage.transform.localScale *= size;
    }

    public void SetTextureImage(Texture2D texture, int depth, int width = -1, int height = -1)
    {
        //이미지 활성화
        textureImage.gameObject.SetActive(true);

        //이미지 설정
        textureImage.mainTexture = texture;

        //사이즈 설정
        textureImage.MakePixelPerfect();
        if (width > -1)
            textureImage.width = width;
        if (height > -1)
            textureImage.height = height;

        //뎁스 설정
        textureImage.depth = depth;
    }
    #endregion

    #region 연출

    public void InitFlyEffect(Vector3 trTargetPos, float actionTime, int sortOrder = -1, System.Action endCallBack = null)
    {
        effectRing.gameObject.SetActive(false);
        if (sortOrder != -1)
        {
            rendererEffectRing.sortingOrder = sortOrder;
            rendererEffectParticle.sortingOrder = sortOrder;
        }

        if (endCallBack != null)
            endAction = endCallBack;

        StartCoroutine(CoActionMoveEffect(trTargetPos, actionTime));
    }

    public IEnumerator CoActionMoveEffect(Vector3 trTargetPos, float actionTime)
    {
        _transform.DOMove(trTargetPos, actionTime);
        yield return new WaitForSeconds(actionTime);

        if(spriteImage.gameObject.activeInHierarchy == true)
            DOTween.ToAlpha(() => spriteImage.color, x => spriteImage.color = x, 0f, 0.2f);

        if (textureImage.gameObject.activeInHierarchy == true)
            DOTween.ToAlpha(() => textureImage.color, x => textureImage.color = x, 0f, 0.2f);

        effectRing.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);
        endAction?.Invoke();

        yield return new WaitForSeconds(0.3f);
        this.gameObject.SetActive(false);
    }
    #endregion
}
