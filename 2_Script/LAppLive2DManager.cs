﻿using System.Collections;
using UnityEngine;

public class LAppLive2DManager : MonoBehaviour
{
    private LAppModelProxy modelA; //왼쪽
    private LAppModelProxy modelB; //오른쪽

    public static LAppLive2DManager _instance = null;

    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        _instance = this;
    }

    //라이브 투디 모델 생성 및 모션.
    public bool SetCharacterMotion(ChatData data)
    {
        bool bCharChange = false;

        if (data.character_0 == TypeCharacterType.None)
        {
            if (modelA != null)
            {
                HideCharacter(CHAT_SIDE.LEFT);
            }
        }
        else
        {
            //모델 없으면 모델 로드.
            if (modelA == null)
            {
                LoadModel(data.character_0, CHAT_SIDE.LEFT, data.characterMotion_0);
            }
            else
            {
                //현재 모델과 data의 모델이 같다면 모션과 표정 재생.
                if (IsSetCharacter(data.character_0, CHAT_SIDE.LEFT))
                {
                    ShowCharacterAction(CHAT_SIDE.LEFT, data.characterMotion_0);
                }
                //모델이 다르면 캐릭터 전환.
                else
                {
                    StartCoroutine(ChangeLive2DCharacter(data.character_0, CHAT_SIDE.LEFT, data.characterMotion_0));
                    bCharChange = true;
                }
            }
        }

        if (data.character_1 == TypeCharacterType.None)
        {
            if (modelB != null)
            {
                HideCharacter(CHAT_SIDE.RIGHT);
            }
        }
        else
        {
            if (modelB == null)
            {
                LoadModel(data.character_1, CHAT_SIDE.RIGHT, data.characterMotion_1);
            }
            else
            {
                //현재 모델과 data의 모델이 같다면 모션과 표정 재생.
                if (IsSetCharacter(data.character_1, CHAT_SIDE.RIGHT))
                {
                    ShowCharacterAction(CHAT_SIDE.RIGHT, data.characterMotion_1);
                }
                //모델이 다르면 캐릭터 전환.
                else
                {
                    StartCoroutine(ChangeLive2DCharacter(data.character_1, CHAT_SIDE.RIGHT, data.characterMotion_1));
                    bCharChange = true;
                }
            }
        }
        return bCharChange;
    }

    IEnumerator ChangeLive2DCharacter(TypeCharacterType character, CHAT_SIDE chatSide, string motion)
    {
        HideCharacter(chatSide);
        yield return new WaitForSeconds(0.3f);
        LoadModel(character, chatSide, motion);
    }

    public void HideCharacter()
    {
        HideCharacter(CHAT_SIDE.LEFT);
        HideCharacter(CHAT_SIDE.RIGHT);
    }

    //라이브 투디 모델 위치 이동 시 사용.
    public IEnumerator ChangeCharPosition(ChatData data)
    {
        //모델A 에서 모델 B로 이동.
        if (data.character_0 == TypeCharacterType.None)
        {
            //모델 A 다 들어갈 때 까지 대기.
            if (modelA != null)
            {
                HideCharacter(CHAT_SIDE.LEFT);
                yield return new WaitForSeconds(1.0f);
            }

            //모델 B에 아무것도 없다면 바로 생성, 있으면 들어가는 연출 후 생성.
            if (modelB == null)
            {
                LoadModel(data.character_1, CHAT_SIDE.RIGHT, data.characterMotion_1);
            }
            else
            {
                StartCoroutine(ChangeLive2DCharacter(data.character_1, CHAT_SIDE.RIGHT, data.characterMotion_1));
            }
        }

        //모델B 에서 모델 A로 이동.
        else
        {
            //모델 B 다 들어갈 때 까지 대기.
            if (modelB != null)
            {
                HideCharacter(CHAT_SIDE.RIGHT);
                yield return new WaitForSeconds(1.0f);
            }

            //모델 A에 아무것도 없다면 바로 생성, 있으면 들어가는 연출 후 생성.
            if (modelA == null)
            {
                LoadModel(data.character_0, CHAT_SIDE.LEFT, data.characterMotion_0);
            }
            else
            {
                StartCoroutine(ChangeLive2DCharacter(data.character_0, CHAT_SIDE.LEFT, data.characterMotion_0));
            }
        }
        yield return null;
    }

    void LoadModel(TypeCharacterType type, CHAT_SIDE chatSide, string motion)
    {
        LAppModelProxy model = LAppModelProxy.MakeLive2D(gameObject, type, "Appear");

        //모델 로드 후 타입설정, 등장 모션 설정.
        if (chatSide == CHAT_SIDE.LEFT)
        {
            model.transform.localPosition = new Vector3(12f, 0f, 0f);
            modelA = model;
        }
        else
        {
            model.transform.localPosition = new Vector3(-12f, 0f, 0f);
            model.transform.localScale = new Vector3(model.transform.localScale.x * -1, model.transform.localScale.y, model.transform.localScale.z);
            modelB = model;
        }

        StartCoroutine(CoAppear(model, motion));
    }

    IEnumerator CoAppear(LAppModelProxy model, string motion)
    {
        if (model.IsPlayingAnimation("Out"))
            yield break;

        yield return model.CoWaitEndAnimation("Appear");

        if (motion.Equals("") == true)
            motion = "Idle";
        model.SetAnimation(motion, GetLoopName(motion));
    }

    bool IsSetCharacter(TypeCharacterType pcType, CHAT_SIDE chat_side)
    {
        if (chat_side == CHAT_SIDE.LEFT && modelA.GetPcType() == pcType)
        {
            return true;
        }

        if (chat_side == CHAT_SIDE.RIGHT && modelB.GetPcType() == pcType)
        {
            return true;
        }

        return false;
    }
    
    //캐릭터숨기기 좌우구분.
    void HideCharacter(CHAT_SIDE chatSide)
    {
        if (chatSide == CHAT_SIDE.LEFT)
        {
            if (modelA != null)
            {
                modelA.SetHideModelFinishedMotion();
            }
        }

        if (chatSide == CHAT_SIDE.RIGHT)
        {
            if (modelB != null)
            {
                modelB.SetHideModelFinishedMotion();
            }
        }
    }

    void ShowCharacterAction(CHAT_SIDE chatSide, string actionName)
    {
        if (chatSide == CHAT_SIDE.LEFT && actionName != "")
        {   
            modelA.SetAnimation(actionName, GetLoopName(actionName));
        }

        if (chatSide == CHAT_SIDE.RIGHT && actionName != "")
        {
            modelB.SetAnimation(actionName, GetLoopName(actionName));
        }
    }

    private string GetLoopName(string motionName)
    {
        return motionName.Replace("_appear", "");
    }
}