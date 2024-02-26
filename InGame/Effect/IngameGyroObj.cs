using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class IngameGyroObj : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody2D rigidBody;
    [SerializeField]
    protected Transform mainSpriteTr;
    [SerializeField]
    protected Transform shadowTr;

    [SerializeField]
    public UISprite mainSprite;
    [SerializeField]
    public UISprite shadowSprite;

    //속도
    [SerializeField]
    protected float speed = 80f;

    //그림자 위치 오프셋
    [SerializeField]
    protected Vector3 shadowPosOffset = new Vector3(-2.5f, -30.0f, 0f);

    protected Vector3 force = Vector3.zero;    
    protected float randPower = 0f;

    void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        randPower = Random.Range(0.8f, 1f);
    }

    void FixedUpdate()
    {
        force = Time.deltaTime * Vector3.left * -Input.acceleration.x * speed * randPower;
    }

    void Update()
    {
        rigidBody.AddForce(force);

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rigidBody.AddForce(Time.deltaTime * Vector3.left * speed * randPower);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rigidBody.AddForce(Time.deltaTime * Vector3.right * speed * randPower);
        }

        shadowTr.localPosition = new Vector3(mainSpriteTr.localPosition.x + shadowPosOffset.x, mainSpriteTr.localPosition.y + shadowPosOffset.y, shadowTr.localPosition.z);
    }

    public IEnumerator CoAutoDestroy(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        float alphaTime = 0.3f;
        SpriteAlphaAction(alphaTime);
        yield return new WaitForSeconds(alphaTime);

        Destroy(gameObject);
    }

    protected void SpriteAlphaAction(float alphaTime)
    {
        if (mainSprite != null)
            DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0, alphaTime);

        if (shadowSprite != null)
            DOTween.ToAlpha(() => shadowSprite.color, x => shadowSprite.color = x, 0, alphaTime);
    }
}
