using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


[System.Serializable]
public class ActionObjectChangeList
{
    public ObjectBase _objectFrom;
    public ObjectBase _objectTo;
    public float _delay = 0f;
    public float _duration = 1f;


    [System.NonSerialized]
    public bool play = false;
    [System.NonSerialized]
    public bool playFinish = false;
    [System.NonSerialized]
    public Vector3 localPos = Vector3.zero;
    [System.NonSerialized]
    public Vector3 localScale = Vector3.zero;
    [System.NonSerialized]
    public Quaternion localRotation = Quaternion.identity;

    [System.NonSerialized]
    public float timer = 0f;
} 


public class ActionObjectChange : ActionBase
{
    public bool _conditionWaitOn = false;
    public List<ActionObjectChangeList> _listObject = new List<ActionObjectChangeList>();
    public AnimationCurve _spawnPositionH = new AnimationCurve();
    public AnimationCurve _spawnScaleY = new AnimationCurve();
    public AnimationCurve _spawnScaleX = new AnimationCurve();
    public AnimationCurve _spawnRotation = new AnimationCurve();
    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionObjectChange _ DoAction");
      
        StartCoroutine(CoDoAction());

    }
    IEnumerator CoDoAction()
    {

        while (true)
        {
            bool allFinish = true;

            for (int i = 0; i < _listObject.Count; i++)
            {

                if (!_listObject[i].play)
                {
                    _listObject[i].timer += Global.deltaTimeLobby;

                    if (_listObject[i].timer >= _listObject[i]._delay)
                    {
                        if (_listObject[i]._objectTo != null)
                        {
                            _listObject[i]._objectTo.gameObject.SetActive(true);
                            _listObject[i]._objectTo.SetAlpha(0f);
                        }

                        if (_spawnPositionH.length > 0)
                        {
                            if (_listObject[i]._objectTo != null)
                            {
                                _listObject[i].localPos = _listObject[i]._objectTo._transformModel.localPosition;
                                _listObject[i]._objectTo._transformModel.localPosition = _listObject[i].localPos + Vector3.up * _spawnPositionH.Evaluate(0f);
                            }
                            else
                            {
                                _listObject[i].localPos = _listObject[i]._objectFrom._transformModel.localPosition;
                                _listObject[i]._objectFrom._transformModel.localPosition = _listObject[i].localPos + Vector3.up * _spawnPositionH.Evaluate(0f);
                            }
                        }
                        if (_spawnScaleY.length > 0 || _spawnScaleX.length > 0)
                        {
                            if (_listObject[i]._objectTo != null)
                            {
                                _listObject[i].localScale = _listObject[i]._objectTo._transformModel.localScale;
                                _listObject[i]._objectTo._transformModel.localScale = _listObject[i].localScale + new Vector3(_spawnScaleX.Evaluate(0f), _spawnScaleY.Evaluate(0f), 0f);
                            }
                            else
                            {
                                _listObject[i].localScale = _listObject[i]._objectFrom._transformModel.localScale;
                                _listObject[i]._objectFrom._transformModel.localScale = _listObject[i].localScale + new Vector3(_spawnScaleX.Evaluate(0f), _spawnScaleY.Evaluate(0f), 0f);
                            }
                        }
                        if (_spawnRotation.length > 0)
                        {
                            
                            if (_listObject[i]._objectTo != null)
                            {
                                _listObject[i].localRotation = _listObject[i]._objectTo._transformModel.localRotation;
                                _listObject[i]._objectTo._transformModel.localRotation = _listObject[i].localRotation * Quaternion.Euler(0f, 0f, _spawnRotation.Evaluate(0f) * 10f);
                            }
                            else
                            {
                                _listObject[i].localRotation = _listObject[i]._objectFrom._transformModel.localRotation;
                                _listObject[i]._objectFrom._transformModel.localRotation = _listObject[i].localRotation * Quaternion.Euler(0f, 0f, _spawnRotation.Evaluate(0f) * 10f);
                            }
                        }

                        _listObject[i].play = true;
                        _listObject[i].timer = 0f;
                    }
                    allFinish = false;
                }
                else
                {
                    if (!_listObject[i].playFinish)
                    {
                        _listObject[i].timer += Global.deltaTimeLobby;


                        float ratio =  Mathf.Clamp01(_listObject[i].timer / _listObject[i]._duration);

                        if (_listObject[i]._objectFrom == null)
                        {
                            _listObject[i]._objectTo.SetAlpha(Mathf.Clamp01(ratio * 3f));

                            if (_spawnPositionH.length > 0)
                                _listObject[i]._objectTo._transformModel.localPosition = _listObject[i].localPos + Vector3.up * _spawnPositionH.Evaluate(ratio);
                            if (_spawnScaleY.length > 0 || _spawnScaleX.length > 0)
                                _listObject[i]._objectTo._transformModel.localScale = _listObject[i].localScale + new Vector3(_spawnScaleX.Evaluate(ratio), _spawnScaleY.Evaluate(ratio), 0f);
                            if (_spawnRotation.length > 0)
                                _listObject[i]._objectTo._transformModel.localRotation = _listObject[i].localRotation * Quaternion.Euler(0f, 0f, _spawnRotation.Evaluate(ratio) * 10f);

                            if (_listObject[i].timer >= _listObject[i]._duration)
                            {
                                _listObject[i].playFinish = true;
                            }
                        }
                        else if (_listObject[i]._objectTo == null)
                        {
                            _listObject[i]._objectFrom.SetAlpha(Mathf.Clamp01(1.2f - ratio * 1.5f));

                            if (_spawnPositionH.length > 0)
                                _listObject[i]._objectFrom._transformModel.localPosition = _listObject[i].localPos + Vector3.up * _spawnPositionH.Evaluate(ratio);
                            if (_spawnScaleY.length > 0 || _spawnScaleX.length > 0)
                                _listObject[i]._objectFrom._transformModel.localScale = _listObject[i].localScale + new Vector3(_spawnScaleX.Evaluate(ratio), _spawnScaleY.Evaluate(ratio), 0f);
                            if (_spawnRotation.length > 0)
                                _listObject[i]._objectFrom._transformModel.localRotation = _listObject[i].localRotation * Quaternion.Euler(0f, 0f, _spawnRotation.Evaluate(ratio) * 10f);

                            if (_listObject[i].timer >= _listObject[i]._duration)
                            {
                                _listObject[i].playFinish = true;
                                _listObject[i]._objectFrom.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            _listObject[i]._objectTo.SetAlpha(Mathf.Clamp01(ratio * 3f));
                            _listObject[i]._objectFrom.SetAlpha(Mathf.Clamp01(1f - ratio * 1.5f));

                            if (_spawnPositionH.length > 0)
                                _listObject[i]._objectTo._transformModel.localPosition = _listObject[i].localPos + Vector3.up * _spawnPositionH.Evaluate(ratio);
                            if (_spawnScaleY.length > 0 || _spawnScaleX.length > 0)
                                _listObject[i]._objectTo._transformModel.localScale = _listObject[i].localScale + new Vector3(_spawnScaleX.Evaluate(ratio), _spawnScaleY.Evaluate(ratio), 0f);
                            if (_spawnRotation.length > 0)
                                _listObject[i]._objectTo._transformModel.localRotation = _listObject[i].localRotation * Quaternion.Euler(0f, 0f, _spawnRotation.Evaluate(ratio) * 10f);

                            if (_listObject[i].timer >= _listObject[i]._duration)
                            {
                                _listObject[i].playFinish = true;
                                _listObject[i]._objectFrom.gameObject.SetActive(false);
                            }
                        }
                        allFinish = false;
                    }
                }
            }

            if (allFinish)
            {
                bActionFinish = true;
                break;
            }
            yield return null;
        }
    }
}
