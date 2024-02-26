using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HighlightAreaBase : AreaBase
{
    [Header("HighlightEventObject")]
    [SerializeField] protected UIHighlightBubble bubble;
    [SerializeField] protected GameObject[] objImages;

    protected GameObject bubbleRoot;

    virtual protected void Awake()
    {
        bubbleRoot = bubble.GetComponentInChildren<Transform>().gameObject;
        StartCoroutine(CoLobbyCompletCheack());
        EventAddHighlightBubble(true);
    }

    virtual protected void OnDestroy()
    {
        EventAddHighlightBubble(false);
    }

    /// <summary>
    /// 각 이벤트의 말풍선 On & Off 기능 로직
    /// </summary>
    abstract protected void HighlightBubbleSetting();

    /// <summary>
    /// 말풍선 Event 등록
    /// </summary>
    /// <param name="IsAdd"></param>
    abstract public void EventAddHighlightBubble(bool IsAdd);

    public override void TriggerStart()
    {
        ManagerSound._instance.SetTimeBGM(sceneStartBgmOffset);

        if (sceneStartBgmOff)
            ManagerSound._instance?.PauseBGM();

        TriggerStart_Internal();
    }

    IEnumerator CoLobbyCompletCheack()
    {
        bubbleRoot.SetActive(false);

        yield return new WaitUntil(() => ManagerLobby._instance.IsLobbyComplete);

        HighlightBubbleSetting();
    }
}