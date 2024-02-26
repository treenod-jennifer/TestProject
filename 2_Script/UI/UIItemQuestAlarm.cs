using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SideIcon;

public class UIItemQuestAlarm : MonoBehaviour
{
    public GameObject questIconBox;
    public UISprite mainSprite;

    public UIUrlTexture questIcon;
    public UILabel questLevel;
    public UILabel questInfo;
    public UILabel questTime;
    public UILabel progressStep;
    public UIProgressBar progressBar;

    //이벤트.
    public GameObject eventObject;
    public UIUrlTexture eventIcon;
    public UILabel eventInfo;

    //이펙트
    public GameObject effectRoot;

    private QuestGameData questData = null;

    public void InitQuestAlarm(int qIndex, QuestAlarmData alarmData, Vector3 targetPos)
    {
        questData = null;
        ManagerData._instance._questGameData.TryGetValue(qIndex, out questData);
        if (questData == null)
            return;

        effectRoot.SetActive(false);

        //퀘스트 시간 표시(제한 시간이 있다면 이벤트 퀘스트, 없으면 일반 퀘스트).
        //퀘스트 duration 이나 valueTime 이 있을 경우, 타이머 시작.
        if (questData.valueTime1 > 1)
        {
            SettingEventQuest();
            StartCoroutine(CoQuestTimer(questData.valueTime1));
        }
        else
        {
            SettingDefaultQuest();
            if (questData.duration > 0)
            {
                questInfo.transform.localPosition = new Vector3(questInfo.transform.localPosition.x, -36.5f, 0f);
                StartCoroutine(CoQuestTimer(questData.duration));
            }
            else
            {
                questTime.gameObject.SetActive(false);
            }
        }

        //프로그레스 바 설정.
        float progressOffset = 100f / questData.targetCount;
        progressBar.value = (alarmData.preProg1 * progressOffset) * 0.01f;

        //퀘스트 진행 카운트 설정(진행 수는 타겟 수를 넘지 않게 표시됨).
        int curCount = alarmData.preProg1;
        if (curCount > questData.targetCount)
        {
            curCount = questData.targetCount;
        }
        progressStep.text = string.Format("{0}/{1}", curCount, questData.targetCount);

        mainSprite.color = new Color(1f, 1f, 1f, 0f);
        //생성되는 연출.
        StartCoroutine(CoOpenAction(targetPos));
    }

    IEnumerator CoOpenAction(Vector3 targetPos)
    {
        yield return new WaitForSeconds(0.5f);

        transform.DOLocalMove(targetPos, 0.2f);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 1f, 0.2f);

        yield return new WaitForSeconds(0.2f);
        float progressOffset = 100f / questData.targetCount;
        float targetValue = (questData.prog1 * progressOffset) * 0.01f;
        DOTween.To(() => progressBar.value, x => progressBar.value = x, targetValue, 0.3f).SetEase(Ease.Linear);
        
        //보상 받을 수 있는 상태에서 이펙트 출력
        if (questData.state > 0)
            effectRoot.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        //진행 카운트 표시 변경.
        int curCount = questData.prog1;
        if (curCount > questData.targetCount)
        {   
            curCount = questData.targetCount;
        }
        progressStep.text = string.Format("{0}/{1}", curCount, questData.targetCount);
        progressStep.transform.DOShakeScale(0.5f, 0.7f);
    }

    public void CloseAction_1(Vector3 targetPos)
    {
        transform.DOScaleX(0f, 0.2f);
        transform.DOMove(targetPos, 0.2f);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, 0.2f);
    }

    public void CloseAction_2()
    {   
        transform.DOLocalMoveY(transform.localPosition.y - mainSprite.height, 0.2f);
        DOTween.ToAlpha(() => mainSprite.color, x => mainSprite.color = x, 0f, 0.2f);
    }

    private void SettingDefaultQuest()
    {
        eventObject.SetActive(false);
        questIconBox.SetActive(true);
        questInfo.gameObject.SetActive(true);

        //이미지 로드.
        string fileName = string.Format("q_{0}", (int)questData.type);
        questIcon.LoadCDN(Global.gameImageDirectory, "IconQuest/", fileName);

        //텍스트 세팅.
        questInfo.text = string.Format("{0}", Global.ClipString(Global._instance.GetString(questData.info), 12));
        
        if ((int)questData.type >= 1000)
        {
            questLevel.text = string.Format("ep.{0}", questData.level);
        }
        else
        {
            questLevel.text = string.Format("Lv.{0}", questData.level);
        }
    }

    private void SettingEventQuest()
    {
        eventObject.SetActive(true);
        questIconBox.SetActive(false);
        questInfo.gameObject.SetActive(false);

        //이미지 로드.
        string fileName = string.Format("e_{0}", questData.type);
        eventIcon.LoadCDN(Global.gameImageDirectory, "IconQuest/", fileName);
        
        //텍스트 세팅.
        string eventInfoText;
        if ((int)questData.type == 162)
        {
            int lastEpisode = ManagerData.GetFinalChapter();
            eventInfoText = Global._instance.GetString(questData.info).Replace("[n]", lastEpisode.ToString());
        }
        else
        {
            eventInfoText = Global._instance.GetString(questData.info);
        }
        eventInfo.text = Global.ClipString(eventInfoText, 12);
    }

    private IEnumerator CoQuestTimer(long timer)
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;
            questTime.text = Global.GetTimeText_DDHHMM(timer, true);

            if (Global.LeftTime(timer) <= 0)
                break;
            yield return null;
        }
    }
    
    private void OnClickBtnBanner()
    {
        UIQuestBanner uiQuestBanner = GetComponentInParent<UIQuestBanner>();
        
        if (uiQuestBanner == null) 
            return;
        
        if (uiQuestBanner.bCanTouch == false)
            return;

        if (uiQuestBanner.closeRoutine != null)
        {
            uiQuestBanner.StopCoroutine(uiQuestBanner.closeRoutine);
            uiQuestBanner.closeRoutine = null;
        }
        if (questData.valueTime1 >= Global.GetTime())
            uiQuestBanner.callbackEnd += OpenPopupEventQuest;
        else
            uiQuestBanner.callbackEnd += OpenPopupDiary;
        uiQuestBanner.CloseBanner();
    }

    private void OpenPopupDiary()
    {
        ManagerUI._instance.OpenPopupDiary(TypePopupDiary.eMission);
    }
    
    private void OpenPopupEventQuest()
    {
        IconEventQuest uiIcon = null;
        foreach (var icon in ManagerUI._instance.ScrollbarRight.icons)
            if (icon is IconEventQuest)
                uiIcon = (icon as IconEventQuest);
        uiIcon?.OnIconClick_QuestAlarm();
    }

    public bool IsEventQuestData()
    {
        return questData.valueTime1 != 1;
    }
}
