using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyRank : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform;
    public UISprite _UISprite;
    public UISprite _UISpriteShadow;

    [System.NonSerialized]
    public Vector3 startPos;

    [System.NonSerialized]
    public Vector3 endPos;

    float _timer = 0f;

    public BlockColorType targetColor = BlockColorType.NONE;

    void Awake() { _transform = transform; }

    public void SetSprite()
    {
        switch (targetColor)
        {
            case BlockColorType.A:
                _UISprite.spriteName = "blockRabbit";
                break;
            case BlockColorType.B:
                _UISprite.spriteName = "blockBear";
                break;
            case BlockColorType.C:
                _UISprite.spriteName = "blockKangaroo";
                break;
            case BlockColorType.D:
                _UISprite.spriteName = "blockTiger";
                break;
            case BlockColorType.E:
                _UISprite.spriteName = "blockWolf";               
                break;
        }

        _UISprite.MakePixelPerfect();
        _UISprite.color = new Color(1, 1, 1, 0.9f);
        _UISprite.cachedTransform.localScale = Vector3.one * 1.5f;

        _UISpriteShadow.spriteName = _UISprite.spriteName;
        _UISpriteShadow.color = new Color(0, 0, 0, 0.1f);
        _UISpriteShadow.MakePixelPerfect();
    }

    float speedRatio = 0f;
    float velocity = 0.08f;

    IEnumerator Start()
    {
        //거리측정
        //시간측정
        float _distance = Vector3.Distance(startPos, endPos);
        float _eTime =  1 + (_distance) *0.3f ;/// (78f * 1);

        _transform.position = startPos;
        yield return null;
        float rotZ = 0;

        while (_timer < _eTime)
        {
            _timer += Global.deltaTimePuzzle * 1.5f;

            float pX = Mathf.Lerp(startPos.x, endPos.x, _timer / _eTime);
           // float pY = startPos.y + (endPos.y - startPos.y) * ManagerBlock.instance._curveRankFly.Evaluate(_timer / _eTime);

            rotZ += 1 - ((_timer / _eTime) * (_timer / _eTime));

           // _transform.position = new Vector3(pX, pY, 0);
            _UISprite.transform.localEulerAngles = new Vector3(0, 0, rotZ*30f);
            _UISpriteShadow.transform.localEulerAngles = new Vector3(0, 0, rotZ * 30f);

            yield return null;
        }

        //사운드
        ManagerSound.AudioPlayMany(AudioInGame.GET_TARGET);

      //  ManagerBlock.instance.rankCollectCount++;
        //GameUIManager.instance.RefreshRankTargetCount();

        _UISprite.enabled = false;
        _UISpriteShadow.enabled = false;

        InGameEffectMaker.instance.RemoveEffect(gameObject);
    }
}
