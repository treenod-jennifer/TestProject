using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;





public class ActionTransformAnimation : ActionBase
{
    public bool _conditionWaitOn = false;
    public TypeCharacterType _targetCharacter = TypeCharacterType.None;
    public List<ObjectBase> _targetObject = new List<ObjectBase>();
    public bool _editLocal = false;
    Character character;

    Animation animaitonData = null;
    public int _staticSource = 0;

    float aniSpdMultiplier = 1f;

    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        //Debug.Log("ActionTransformAnimation _ DoAction");

        character = ManagerLobby._instance.GetCharacter(_targetCharacter);
        animaitonData = GetComponent<Animation>();
        

        if (animaitonData != null)
        {
            if (_staticSource > 0)
                animaitonData.clip = ManagerLobby._instance._staticAnimationObj[_staticSource - 1];
            StartCoroutine(CoDoAction());
        }
    }

    private void Update()
    {
        if( aniSpdMultiplier != Global.timeScaleLobby && animaitonData != null)
        {
            aniSpdMultiplier = Global.timeScaleLobby;

            foreach (AnimationState state in animaitonData)
            {
                state.speed = Global.timeScaleLobby;
            }
        }
    }


    IEnumerator CoDoAction()
    {
        aniSpdMultiplier = Global.timeScaleLobby;

        foreach (AnimationState state in animaitonData)
        {
            state.speed = Global.timeScaleLobby;
        }

        animaitonData.Play();
        Transform _transform = transform;

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
                //_targetObject[i]._transform.rotation = _transform.rotation;
            }
            if (character != null)
            {
                if (_editLocal)
                    character._transform.position = obCposition + _transform.position;
                else
                    character._transform.position = _transform.position;
                character._model.transform.localScale = _transform.localScale;
                //character._transform.rotation = _transform.rotation;
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
    }
}
