using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;





public class ActionTransformAnimationBySource : ActionBase
{
    public bool _conditionWaitOn = false;
    public Transform _sourceObject = null;
    public TypeCharacterType _targetCharacter = TypeCharacterType.None;
    public List<ObjectBase> _targetObject = new List<ObjectBase>();
    public bool _editLocal = false;
    Character character;

    Animation animaitonData = null;
    public int _staticSource = 0;
    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
        _sourceObject.gameObject.SetActive(false);
    }
    public override void DoAction()
    {
        //Debug.Log("ActionTransformAnimationBySource _ DoAction");

        character = ManagerLobby._instance.GetCharacter(_targetCharacter);
        animaitonData = _sourceObject.GetComponent<Animation>();
        if (animaitonData != null)
        {
            if (_staticSource > 0)
                animaitonData.clip = ManagerLobby._instance._staticAnimationObj[_staticSource - 1];

            StartCoroutine(CoDoAction());
        }
    }

    IEnumerator CoDoAction()
    {
        _sourceObject.gameObject.SetActive(true);
        animaitonData.Play();
        Transform _transform = _sourceObject;

        float timer = 0f;


        Vector3 chScale = Vector3.one;
        if (character != null)
            chScale = character._model.transform.localScale;
        List<Vector3> obScale = new List<Vector3>();
        List<Vector3> obPosition = new List<Vector3>();
        Vector3 obCposition = character._transform.position;

        for (int i = 0; i < _targetObject.Count; i++)
        {
            obScale.Add(_targetObject[i]._transformModel.localScale);
            obPosition.Add(_targetObject[i]._transformModel.localPosition);
        }

        while (true)
        {

            for (int i = 0; i < _targetObject.Count; i++)
            {
                if (_editLocal)
                    _targetObject[i]._transform.localPosition = obPosition[i] + _transform.localPosition;
                else
                    _targetObject[i]._transform.localPosition = _transform.localPosition;
                _targetObject[i]._transformModel.localScale = _transform.localScale;
                _targetObject[i]._transform.rotation = _transform.rotation;
            }
            if (character != null)
            {
                if (_editLocal)
                    character._transform.position = obCposition + _transform.position;
                else
                    character._transform.position = _transform.position;

                character._model.transform.localScale = _transform.localScale;
                character._transform.rotation = _transform.rotation;
            }

            if (!animaitonData.isPlaying)
            {
                for (int i = 0; i < _targetObject.Count; i++)
                    _targetObject[i]._transformModel.localScale = obScale[i];
                if (character != null)
                    character._model.transform.localScale = chScale;

                bActionFinish = true;
                break;
            }

            yield return null;
        }
        _sourceObject.gameObject.SetActive(false);
    }
}
