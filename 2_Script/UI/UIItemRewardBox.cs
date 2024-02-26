using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIItemRewardBox : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("box")]
    [SerializeField] protected UISprite box;
    [SerializeField] private GenericReward reward;

    [Header("Session_1")]
    [SerializeField] private AnimationCurve b_sizeController_1;
    [SerializeField] private AnimationCurve b_angleController_1;

    [Header("Session_2")]
    [SerializeField] private AnimationCurve b_sizeController_2;

    [Header("Session_3")]
    [SerializeField] private AnimationCurve r_sizeController_2;
    [SerializeField] private AnimationCurve r_heightController_2;

    protected abstract string CloseBoxName { get; }
    protected abstract string OpenBoxName { get; }
    protected abstract Transform BoxRoot { get; }

    public void SetReward(Reward reward)
    {
        this.reward.SetReward(reward);
    }

    public IEnumerator CoGetReward(System.Func<IEnumerator> openEvent = null)
    {
        if (!gameObject.activeSelf)
            yield break;

        #region Session_1
        box.spriteName = CloseBoxName;
        box.MakePixelPerfect();
        box.alpha = 1.0f;
        reward.gameObject.SetActive(false);

        float totalTime = 0.0f;
        float endTime = Mathf.Max
        (
            b_sizeController_1.keys[b_sizeController_1.length - 1].time,
            b_angleController_1.keys[b_angleController_1.length - 1].time
        );

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimeNoScale;

            BoxRoot.localScale = Vector3.one * b_sizeController_1.Evaluate(totalTime);
            BoxRoot.localEulerAngles = Vector3.forward * b_angleController_1.Evaluate(totalTime);

            yield return null;
        }
        #endregion

        #region Session_2
        box.spriteName = OpenBoxName;
        box.MakePixelPerfect();
        totalTime = 0.0f;
        endTime = b_sizeController_2.keys[b_sizeController_2.length - 1].time;

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimeNoScale;

            BoxRoot.localScale = Vector3.one * b_sizeController_2.Evaluate(totalTime);

            yield return null;
        }
        #endregion

        #region Session_3
        yield return new WaitForSeconds(0.1f);

        if (openEvent != null)
            yield return openEvent();

        StartCoroutine(CoShowReward());

        yield return new WaitForSeconds(0.1f);

        totalTime = 0.0f;
        endTime = 0.1f;

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimeNoScale;

            box.alpha = Mathf.Lerp(1.0f, 0.0f, totalTime / endTime);

            yield return null;
        }
        #endregion
    }

    public IEnumerator CoShowReward()
    {
        float totalTime = 0.0f;
        float endTime = Mathf.Max
        (
            r_sizeController_2.keys[r_sizeController_2.length - 1].time,
            r_heightController_2.keys[r_heightController_2.length - 1].time
        );

        reward.gameObject.SetActive(true);

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimeNoScale;

            reward.transform.localScale = Vector3.one * r_sizeController_2.Evaluate(totalTime);
            reward.transform.localPosition = Vector3.up * r_heightController_2.Evaluate(totalTime);

            yield return null;
        }
    }
}
