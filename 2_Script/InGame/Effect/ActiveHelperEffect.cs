using System.Collections;
using UnityEngine;

public class ActiveHelperEffect : MonoBehaviour
{
    [SerializeField] protected float destroyTime = 5f;
    public EFFECT_TYPE effectType = EFFECT_TYPE.NONE;

    protected float destroyTimeOffset = 0f;

    //해당 이펙트가 제거/디스폰될 때 호출될 액션
    protected System.Action endAction = null;

    //연출 코루틴
    protected Coroutine activeRoutine = null;

    protected virtual void OnEnable()
    {
        activeRoutine = StartCoroutine(CoInActiveTimer());
    }

    /// <summary>
    /// 디스폰 액션 등록
    /// </summary>
    public void SetCallback(System.Action endAction)
    {
        this.endAction = endAction;
    }

    /// <summary>
    /// 이펙트 제거 시간을 변경시켜 이펙트를 실행할 때 호출되는 함수
    /// (ex. 연쇄폭탄 폭발시간)
    /// </summary>
    public void SetActiveEffect(float? dTime)
    {
        if (dTime != null)
            this.destroyTimeOffset = dTime.Value;
        else
            destroyTimeOffset = 0f;
        StartCoroutine(CoInActiveTimer());
    }

    protected virtual IEnumerator CoInActiveTimer()
    {
        float waitTime = ManagerBlock.instance.GetIngameTime(destroyTime + destroyTimeOffset);
        yield return new WaitForSeconds(waitTime);
        InGameObjectPoolManager.instance.ObjectPoolDeSpawn(effectType, this);
        endAction?.Invoke();
        activeRoutine = null;
    }

    private void OnDisable()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            InGameObjectPoolManager.instance.ObjectPoolDeSpawn(effectType, this);
            endAction?.Invoke();
            activeRoutine = null;
        }
    }
}
