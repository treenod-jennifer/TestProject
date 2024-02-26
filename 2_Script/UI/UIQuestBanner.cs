using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class UIQuestBanner : MonoBehaviour
{
    public Transform bannerRoot;
    public GameObject itemRoot;

    private List<UIItemQuestAlarm> listItemObj = new List<UIItemQuestAlarm>();

    public Coroutine closeRoutine = null;
    public Method.FunctionVoid callbackEnd = null;

    public bool bCanTouch = false;

    public void OpenBanner(Dictionary<int, QuestAlarmData> dicQuestAlarmData)
    {
        bannerRoot.DOScale(Vector3.one, 0.3f).SetEase(ManagerUI._instance.popupScaleAnimation);
        //터치 관련 막음.
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        transform.position = ManagerUI._instance._btnPlay.transform.position;
        InitBanner(dicQuestAlarmData);

        StartCoroutine(CoAction(0.3f, () =>
        {
            bCanTouch = true;
        }));
    }

    public void CloseBanner()
    {
        StartCoroutine(CoCloseBanner());
    }

    public IEnumerator CoCloseBanner()
    {
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);

        if (listItemObj.Count > 0)
        {
            if (listItemObj[0].IsEventQuestData())
                listItemObj[0].CloseAction_2();
            else
                listItemObj[0].CloseAction_1(ManagerUI._instance.buttonDiary.transform.position);
            yield return new WaitForSeconds(0.1f);
        }

        if (listItemObj.Count > 1)
        {
            for (int i = 1; i < listItemObj.Count; i++)
            {
                //현재 위치 기준 -1칸 아래로 이동.
                listItemObj[i].CloseAction_2();
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (callbackEnd != null)        
            callbackEnd();
        Destroy(gameObject);
    }

    public void InitBanner(Dictionary<int, QuestAlarmData> dicQuestAlarmData)
    {
        float yOffset = 5f;
        var enumerator_Q = dicQuestAlarmData.GetEnumerator();
        while (enumerator_Q.MoveNext())
        {
            UIItemQuestAlarm itemQuestAlarm = ManagerUI._instance.InstantiateUIObject("UIPrefab/UIItemQuestAlarm", itemRoot).GetComponent<UIItemQuestAlarm>();
            Vector3 targetPos = itemQuestAlarm.transform.localPosition + new Vector3(0f, yOffset, 0f);
            itemQuestAlarm.InitQuestAlarm(enumerator_Q.Current.Key, enumerator_Q.Current.Value, targetPos);
            listItemObj.Add(itemQuestAlarm);
            yOffset += 132f;
        }
        closeRoutine = StartCoroutine(CoBannerTimer());
    }

    private IEnumerator CoBannerTimer()
    {
        yield return new WaitForSeconds(2f);
        closeRoutine = null;
        CloseBanner();
    }

    protected IEnumerator CoAction(float _startDelay = 0.0f, UnityAction _action = null)
    {
        yield return new WaitForSeconds(_startDelay);
        _action();
    }

    private void OnDisable()
    {
        //closeRoutine은 팝업이 정상적으로 닫힐 때 null로 만들어줌.
        //비정상적으로 오브젝트가 비활성화 된 경우, 오브젝트 삭제해줌.
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            Destroy(gameObject);
        }
    }
}
