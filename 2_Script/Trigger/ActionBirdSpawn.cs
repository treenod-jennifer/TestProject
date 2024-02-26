﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionBirdSpawn : ActionBase
{
    public bool _conditionWaitOn = false;
    public TypeCharacterDir _direction = TypeCharacterDir.None;
    public string _animationName = "appear";
   // public float _animationSpeed = 1f;
    public Transform _startPosition;
    public Transform _endPosition;
    public Vector3 _startOffset = Vector3.zero;
    public Vector3 _endOffset = Vector3.zero; 


    Character character;
  //  Color shadowColor;
    public void Awake()
    {
        bWaitActionFinishOnOff = _conditionWaitOn;
    }
    public override void DoAction()
    {
        //base.DoAction();
        character = ManagerLobby._instance.GetCharacter(TypeCharacterType.BlueBird);


        if (_startPosition != null)
            character._transform.position = _startPosition.position + _startOffset;

        if (_direction != TypeCharacterDir.None)
            character._ai.BodyTurn(_direction);
        character._startMotion = _animationName;

     //   shadowColor = character._rendererShadow.material.GetColor("_Color");
        character._rendererShadow.material.SetColor("_Color", new Color(1f, 1f, 1f, 0f));
        StartCoroutine(CoDoAction());
    }

    IEnumerator CoDoAction()
    {
        yield return null;
        float timer = 0f;
        float length = character._animation[_animationName].length;
        while (true)
        {
            timer += Global.deltaTimeLobby;
            character._rendererShadow.material.SetColor("_Color", new Color(1f, 1f, 1f, Mathf.Clamp01(timer / length)));

            if (!character._animation.isPlaying)
                break;
            yield return null;
        }
        character._rendererShadow.material.SetColor("_Color", Color.white);
        character.StartPath(_endPosition.position + new Vector3(_endOffset.x, 0f, _endOffset.z), null);
        yield return null;
        while (true)
        {
            if (character._ai.GetStateID() == AIStateID.eIdle)
            {
                bActionFinish = true;
                break;
            }
            yield return null;
        }

        bActionFinish = true;
    }
    void OnDrawGizmosSelected()
    {
        if (_startPosition != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_startPosition.position + _startOffset, new Vector3(0.8f, 1.4f, 0.8f));
        }
        if (_endPosition != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(_endPosition.position + _endOffset, new Vector3(0.8f, 1.4f, 0.8f));

            if (_startPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_endPosition.position + _endOffset, _startPosition.position + _startOffset);
            }
        }
    }
}
