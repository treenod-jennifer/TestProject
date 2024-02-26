using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTsTimer : MonoBehaviour
{
    private UILabel target;
    
    private long endTs;

    private System.Func<long, string> setTextFunc = null;

    private System.Action timeOutAction = null;

    private Coroutine timerCoroutine;

    public static EndTsTimer Run(UILabel target, long endTs, System.Func<long, string> overrideTextFunc = null, System.Action timeOutAction = null)
    {
        var timer = target.GetComponent<EndTsTimer>();

        if(timer == null)
        {
            timer = target.gameObject.AddComponent<EndTsTimer>();
        }

        timer.StopTimer();
        timer.target = target;
        timer.endTs = endTs;
        timer.setTextFunc = overrideTextFunc ?? DefaultTextFunc;
        timer.timeOutAction = timeOutAction;
        timer.StartTimer();

        return timer;
    }

    public static string DefaultTextFunc(long endTs)
    {
        return Global.GetLeftTimeText(endTs);
    }

    public static EndTsTimer Stop(UILabel target)
    {
        var timer = target.GetComponent<EndTsTimer>();

        if (timer != null)
        {
            timer.StopTimer();
        }

        return timer;
    }

    private void StartTimer()
    {
        if (target == null) return;

        StopTimer();

        if (gameObject.activeInHierarchy)
        {
            timerCoroutine = StartCoroutine(Timer());
        }
    }

    private void StopTimer()
    {
        if(timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }

    private IEnumerator Timer()
    {
        while (true)
        {
            target.text = setTextFunc(endTs);

            if (Global.LeftTime(endTs) <= 0) break;
            
            yield return new WaitForSeconds(1.0f);
        }

        timerCoroutine = null;
        timeOutAction?.Invoke();
    }

    private void OnEnable()
    {
        StartTimer();
    }

    private void OnDisable()
    {
        StopTimer();
    }
}
