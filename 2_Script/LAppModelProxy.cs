using UnityEngine;
using System.Collections;
using Spine.Unity;

[ExecuteInEditMode]
public class LAppModelProxy : MonoBehaviour
{
    //포코퍼즐정보
    private TypeCharacterType pcType = TypeCharacterType.None;

    //스파인 모델
    public SkeletonAnimation _skeletonAnim;

    //스파인 렌더러
    private MeshRenderer _skelRenderer = null;

    public static LAppModelProxy MakeLive2D(GameObject parent, TypeCharacterType type, string aniName = null, bool isLoopAnimation = false)
    {
        var charData = ManagerCharacter._instance.GetLive2DCharacter((int)type);
        LAppModelProxy spineChar = NGUITools.AddChild(parent, charData.obj).GetComponent<LAppModelProxy>();
        spineChar.pcType = type;
        spineChar.transform.localScale = new Vector3(charData.defaultScale, Mathf.Abs(charData.defaultScale), Mathf.Abs(charData.defaultScale));
        if (aniName != null)
        {
            spineChar.SetRenderer(false);
            spineChar.SetAnimation(aniName, isLoopAnimation);
            spineChar.StartCoroutine(spineChar.CoSetRenderer(true));
        }
        return spineChar;
    }

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

    public void SetAnimation(string aniName, bool isLoop)
    {
        if (IsExistAnimation(aniName) == false)
            return;

        _skeletonAnim.state.SetAnimation(0, aniName, isLoop);
    }

    public void SetAnimation(string startName, string loopName)
    {
        if (IsExistAnimation(startName) == false)
            return;

        _skeletonAnim.state.SetAnimation(0, startName, false);

        if (IsExistAnimation(loopName) == false)
            return;

        if (loopName != null)
            _skeletonAnim.state.AddAnimation(0, loopName, true, 0f);
    }

    private bool IsExistAnimation(string aniName)
    {
        bool isExistAni = (_skeletonAnim.skeleton.Data.Animations.FindIndex(x => x.Name.Equals(aniName)) != -1);
        if (isExistAni == false)
        {
            Debug.LogError("{0} 애니메이션이 없습니다. : " + aniName);
        }
        return isExistAni;
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
        SetAnimation("Out", false);
        yield return CoWaitEndAnimation("Out");
        Destroy(gameObject);
    }

    public TypeCharacterType GetPcType()
    {
        return pcType;
    }

    public void SetSortOrder(int depth)
    {
        if (_skelRenderer == null)
            _skelRenderer = _skeletonAnim.GetComponent<MeshRenderer>();

        if (_skelRenderer != null)
            _skelRenderer.sortingOrder = depth;
    }

    /// <summary>
    /// 해당 애니메이션이 재생중인지 검사
    /// </summary>
    public bool IsPlayingAnimation(string aniName)
    {
        int fIdx = _skeletonAnim.skeleton.Data.Animations.FindIndex(x => x.Name.Equals(aniName));
        if (fIdx > -1 && _skeletonAnim.AnimationState.Tracks.Count > 0)
        {
            Spine.TrackEntry currentTrack = _skeletonAnim.state.GetCurrent(0);
            if (currentTrack.IsComplete == false || currentTrack.Loop == true)
                return _skeletonAnim.state.GetCurrent(0).Animation.Name.Equals(aniName);
        }
        return false;
    }

    /// <summary>
    /// 현재 재생중인 애니메이션의 이름을 가져옴
    /// </summary>
    public string GetPlayingAniName()
    {
        if (_skeletonAnim.AnimationState.Tracks.Count > 0)
        {
            if (_skeletonAnim.state.GetCurrent(0).IsComplete == false || _skeletonAnim.state.GetCurrent(0).Loop == true)
                return _skeletonAnim.AnimationState.GetCurrent(0).Animation.Name;
        }
        return "";
    }

    /// <summary>
    /// 해당 애니메이션이 끝날 때 까지 대기.
    /// </summary>
    public IEnumerator CoWaitEndAnimation(string aniName)
    {
        Spine.Animation animation = _skeletonAnim.skeleton.Data.FindAnimation(aniName);
        while (true)
        {
            if (animation == null || _skeletonAnim.AnimationState.Tracks.Count == 0 ||
                (_skeletonAnim.state.GetCurrent(0).IsComplete == true))
                yield break;
            yield return null;
        }
    }

    /// <summary>
    /// 캐릭터를 한 프레임 뒤에 표시하고 싶을 때 사용
    /// (캐릭터가 등장 시, 깜빡이는 현상을 방지)
    /// </summary>
    public IEnumerator CoSetRenderer(bool value)
    {
        yield return null;
        SetRenderer(value);
    }

    public void SetRenderer(bool value)
    {
        if (_skelRenderer == null)
            _skelRenderer = _skeletonAnim.GetComponent<MeshRenderer>();

        _skelRenderer.enabled = value;
    }
}