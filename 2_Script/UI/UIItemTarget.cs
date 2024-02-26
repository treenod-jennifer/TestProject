using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemTarget : MonoBehaviour
{
    public UISprite _sprTarget;
    public UISprite _sprCheck;
    public UILabel[] _labelTargetCnt;

    private Color READY_TARGET_COLOR = new Color(42f / 255f, 93f / 255f, 139f / 255f);
    private Color FAIL_TARGET_COLOR = new Color(255f / 255f, 54f / 255f, 54f / 255f);

    public void InitTargetUI(UIAtlas _UIatlas, string _sprName)
    {
        transform.localScale = Vector3.one;
        _sprCheck.enabled = false;
        _sprTarget.GetComponent<UISprite>().atlas = _UIatlas;
        _sprTarget.spriteName = _sprName;
        _sprTarget.MakePixelPerfect();
    }

    public void SetTargetCount(string _labelCnt)
    {
        for (int i = 0; i < _labelTargetCnt.Length; i++)
        {
            _labelTargetCnt[i].enabled = true;
            _labelTargetCnt[i].text = _labelCnt;
            _labelTargetCnt[i].MakePixelPerfect();
        }
    }

    public void SetPos(float _xPos)
    {
        transform.localPosition = new Vector3(_xPos, 0, 0);
    }

    public void ReadyPopUpTarget()
    {
        _labelTargetCnt[0].color = READY_TARGET_COLOR;
        for (int i = 0; i < _labelTargetCnt.Length; i++)
        {
            _labelTargetCnt[i].fontSize = 40;
            _labelTargetCnt[i].MakePixelPerfect();
        }
    }

    public void FailPopUpTarget()
    {
        _labelTargetCnt[0].color = FAIL_TARGET_COLOR;
        for (int i = 0; i < _labelTargetCnt.Length; i++)
        {
            _labelTargetCnt[i].fontSize = 30;
            _labelTargetCnt[i].MakePixelPerfect();
        }
    }

    public void SetCheck()
    {
        transform.localScale = Vector3.one;
        _sprCheck.enabled = true;
        for (int i = 0; i < _labelTargetCnt.Length; i++)
        {
            _labelTargetCnt[i].enabled = false;
        }
    }
}
