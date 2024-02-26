using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum eDialogState
{
    None,
    Move,
}

public class UIDialog : MonoBehaviour
{
    public UILabel _labelChat;
    public UISprite _sprDialogBox;
    
    private eDialogState _eDialogState;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private bool _isRemove;

    private void Start()
    {
        _eDialogState = eDialogState.None;
        _startPos = Vector3.zero;
        _targetPos = Vector3.zero;
        _isRemove = false;
    }

    public void MoveDialog(int _spaceSize)
    {
        if (_eDialogState == eDialogState.Move)
            transform.localPosition = _targetPos;

        _eDialogState = eDialogState.Move;
        transform.DOShakeScale(0.3f, 0.1f, 10, 0f);
        _startPos = transform.localPosition;
        _targetPos = _startPos + Vector3.up * _spaceSize * -1f;
        StartCoroutine(CoMove());
    }

    public void RemoveDialog(bool bImmediately = false)
    {
        if (bImmediately == false)
            StartCoroutine(CoReMove());
        else
            ImmediatelyRemove();
    }

    private IEnumerator CoMove()
    {
        float ElapsedTime = 0.0f;
        float TotalTime = 0.5f;
        while (ElapsedTime < TotalTime)
        {
            ElapsedTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPos, (ElapsedTime / TotalTime));
            yield return null;
        }
        _eDialogState = eDialogState.None;
        transform.localPosition = _targetPos;

        if (_isRemove)
        {
            _isRemove = false;
            DialogRemoveSetting();
            this.gameObject.Recycle();
        }
    }

    private IEnumerator CoReMove()
    {
        if (_eDialogState == eDialogState.Move)
            transform.localPosition = _targetPos;

        float ElapsedTime = 0.0f;
        float TotalTime = 0.3f;
        float targetX = Screen.width * 2 * transform.localScale.x * -1;
        Vector3 _target = new Vector3(targetX, transform.localPosition.y, transform.localPosition.z);
        while (ElapsedTime < TotalTime)
        {
            ElapsedTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _target, (ElapsedTime / TotalTime));
            yield return null;
        }
        DialogRemoveSetting();
        this.gameObject.Recycle();
    }

    private void ImmediatelyRemove()
    {
        DialogRemoveSetting();
        this.gameObject.Recycle();
    }

    private void DialogRemoveSetting()
    {
        _eDialogState = eDialogState.None;
    }
}
