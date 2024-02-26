using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Rendering;

public class LAppDefine
{
    //live2D 프리팹 읽어올 path 지정.
    //POKOPOKO
    public const string PATH_LIVE2D_MODEL = "Bundle/ChatCharacter/";
    public const string PATH_LIVE2D_MODEL_EXTENSION = "/model.model.json";
    public const string PATH_LIVE2D_MODEL_PREFAB = ".prefab";
}

[ExecuteInEditMode]
public class LAppModelProxy : MonoBehaviour
{
    //포코퍼즐정보
    private TypeCharacterType pcType = TypeCharacterType.None;
    //포코퍼즐_모델 애니메이션 관련.
    public Animator _animator = null;
    public CubismRenderController _CubismRender;

    public void LoadModel(TypeCharacterType setPcType)
    {
        pcType = setPcType;
    }

    public void SetPcType(TypeCharacterType setPcType)
    {
        pcType = setPcType;
    }

    public void SetPosition(Vector3 tempPos)
    {
        transform.localPosition = tempPos;
    }

    public void SetScale(float tempScale)
    {
        transform.localScale = Vector3.one * tempScale;
    }

    public void SetVectorScale(Vector3 tempPo)
    {
        transform.localScale = tempPo;
    }

    public void SetModel(Vector3 tempPos, float tempScale)
    { 
        transform.localScale = Vector3.one * tempScale;
        transform.localPosition = tempPos;
    }

    public void setAnimation(bool bUseCroseFade, string aniname, float fadetime = 0.2f)
    {
        if (bUseCroseFade)
            _animator.CrossFade(aniname, fadetime);
        else
            _animator.Play(aniname);
    }

    private bool IsHideCharacter = false;

    public void SetHideModelFinishedMotion()
    {
        if (IsHideCharacter)
            return;
        IsHideCharacter = true;

        StartCoroutine(CoHide());
    }

    IEnumerator CoHide()
    {
        setAnimation(false, "Out");    //모션.
        if (pcType == TypeCharacterType.Boni)
        {
            setAnimation(false, "Ex_out");    //모션.
        }
        else
        {   
            setAnimation(false, "Ex_base");    //모션.
        }
        while (true)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Out") && 
                _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {   
                Destroy(gameObject);
                break;
            }
            yield return null;
        }
        yield return null;
    }

    public TypeCharacterType GetPcType()
    {
        return pcType;
    }
}