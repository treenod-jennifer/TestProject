using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TextboxTutorial : MonoBehaviour {
    [System.NonSerialized]
    public Transform _transform;
    public UILabel _text;
    public UITexture _bg;
    
    void Awake()
    {
        _transform = transform;
    }

    public void InitTextBox(bool bFlip, string in_message, float actionTime)
    {
        _bg.transform.localScale = new Vector3(0.3f, 1f, 1f);
        _text.text = in_message;

        int _nLineCharCount = (int)(_text.printedSize.x / _text.fontSize);

        //30f : 폰트 사이즈.
        float boxLength = 105f + (_nLineCharCount * 30f);
        //최대 크기 설정.
        if (boxLength > 515f)
            boxLength = 515f;
        _bg.width = (int)boxLength;

        if (bFlip == true)
        {
            _bg.flip = UIBasicSprite.Flip.Horizontally;
            _bg.pivot = UIWidget.Pivot.Right;
            _text.alignment = NGUIText.Alignment.Left;
            _text.transform.localPosition = new Vector3((_bg.width * -1) + 250f, 0f, 0f);
        }
        _bg.transform.DOScaleX(1f, actionTime);
        _bg.color = new Color(1f, 1f, 1f, 0f);
        DOTween.ToAlpha(() => _bg.color, x => _bg.color = x, 1f, actionTime);
    }

    public IEnumerator DestroyTextBox(float actionTime)
    {
        _bg.transform.DOScaleX(0f, actionTime);
        DOTween.ToAlpha(() => _bg.color, x => _bg.color = x, 0f, actionTime);

        yield return new WaitForSeconds(actionTime);
        Destroy(gameObject);
    }
}
