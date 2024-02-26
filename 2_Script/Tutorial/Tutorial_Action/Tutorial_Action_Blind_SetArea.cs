using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일반 블라인드 영역 설정
/// </summary>
public class Tutorial_Action_Blind_SetArea : Tutorial_Action
{
    //블라인드 위치 설정
    public bool isChangeBlindCenterPos = true;
    public CustomMethodData methodData;
    public Vector3 offset_localPosition = Vector3.zero;
    
    //블라인드 사이즈 설정
    public Vector2 blindSize = Vector2.zero;
    public Vector2 touchSize = Vector2.zero;

    //블라인드 타입 설정
    public UIBasicSprite.Type blindType = UIBasicSprite.Type.Simple;

    //블라인드 터치 액션
    public CustomMethodData touchMethodData;

    //해당 시간 이후부터 터치영역 활성화가 됨
    public float touchTime = 0f;

    //블라인드 사이즈 연출 시간(0일 경우, 연출 없음)
    public float actionTime = 0f;

    private ManagerTutorial.GetGameObjectDelegate gameObjectDelegate = null;
    private System.Action touchAction = null;

    private System.Action endAction = null;

    public void Awake()
    {
        if (methodData.methodName != "")
            gameObjectDelegate = System.Delegate.CreateDelegate(typeof(ManagerTutorial.GetGameObjectDelegate), methodData.target, methodData.target.GetType().GetMethod(methodData.methodName)) as ManagerTutorial.GetGameObjectDelegate;

        if (touchMethodData.methodName != "")
            touchAction = System.Delegate.CreateDelegate(typeof(System.Action), touchMethodData.target, touchMethodData.target.GetType().GetMethod(touchMethodData.methodName)) as System.Action;
    }

    public Tutorial_Action_Blind_SetArea(Object targetObj, string methodName, Vector2 bSize, Vector2 tSize, UIBasicSprite.Type type = UIBasicSprite.Type.Simple)
    {
        this.methodData.target = targetObj;
        this.methodData.methodName = methodName;
        this.blindSize = bSize;
        this.touchSize = tSize;
        this.blindType = type;
    }

    public void Init(Object targetObj, string methodName, Vector2 bSize, Vector2 tSize, UIBasicSprite.Type type = UIBasicSprite.Type.Simple, string touchMethodName = "")
    {
        this.methodData.target = targetObj;
        this.methodData.methodName = methodName;
        this.blindSize = bSize;
        this.touchSize = tSize;
        this.blindType = type;

        if (touchMethodName != "")
        {
            this.touchMethodData.target = targetObj;
            this.touchMethodData.methodName = touchMethodName;
        }
    }

    public override void StartAction(System.Action endAction = null)
    {
        this.endAction = endAction;

        //블라인드 위치 설정
        if (isChangeBlindCenterPos == true)
        {
            ManagerTutorial._instance._current.blind.transform.position
                = (gameObjectDelegate == null) ? Vector3.zero : gameObjectDelegate().transform.position;
        }
        ManagerTutorial._instance._current.blind.transform.localPosition += offset_localPosition;

        //블라인드 사이즈 설정
        if (actionTime == 0f)
        {
            ManagerTutorial._instance._current.blind.SetSize((int)blindSize.x, (int)blindSize.y);
            if (touchAction != null)
                ManagerTutorial._instance._current.blind.SetSizeCollider_ClickFunc(Mathf.RoundToInt(blindSize.x), Mathf.RoundToInt(blindSize.y), () => touchAction());
        }
        else
        {
            StartCoroutine(BlindAni(ManagerTutorial._instance._current.blind._textureCenter.localSize, blindSize));
        }

        //블라인드 이미지 타입 설정
        ManagerTutorial._instance._current.blind._textureCenter.type = blindType;

        //블라인드 터치 영역 설정
        StartCoroutine(CoSetColliderArea());
    }

    //일정 시간 대기 후, 터치 영역 설정.
    private IEnumerator CoSetColliderArea()
    {
        yield return new WaitForSeconds(touchTime);
        ManagerTutorial._instance._current.blind.SetSizeCollider((int)touchSize.x, (int)touchSize.y);
        endAction.Invoke();
    }

    /// <summary>
    /// 블라인드 크기 변경 연출
    /// </summary>
    private IEnumerator BlindAni(Vector2 startSize, Vector2 endSize)
    {
        float totalTime = 0.0f;
        BlindTutorial blind = ManagerTutorial._instance._current.blind;
        while (true)
        {
            totalTime += Global.deltaTimeLobby;

            Vector2 size = Vector2.Lerp(startSize, endSize, totalTime / actionTime);
            blind.SetSize(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));

            if (totalTime > actionTime)
            {
                if (touchAction != null)
                    blind.SetSizeCollider_ClickFunc(Mathf.RoundToInt(endSize.x), Mathf.RoundToInt(endSize.y), () => touchAction());
                yield break;
            }

            yield return null;
        }
    }
}