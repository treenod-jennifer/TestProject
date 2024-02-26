using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemStageAlarmBubble : MonoBehaviour
{
    public static UIItemStageAlarmBubble _instance;

    [SerializeField] private UISprite sprMissionIcon;
    [SerializeField] private List<GameObject> alarmViewList;
    [SerializeField] private UISprite objBubble;
    [SerializeField] private float delayTime;

    private SortedDictionary<int, bool> alarmStateDic = new SortedDictionary<int, bool>();

    private Coroutine coroutine = null;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }

    private void OnEnable()
    {
        alarmStateDic.Clear();

        for (int i = 0; i < alarmViewList.Count; i++)
        {
            alarmViewList[i].SetActive(false);
            alarmStateDic.Add(i, false);
        }
            
        coroutine = StartCoroutine(CoInitEpMission());
    }

    private void OnDisable()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    IEnumerator CoInitEpMission()
    {
        yield return new WaitWhile(() => ManagerData._instance._questGameData.Count == 0);

        objBubble.gameObject.SetActive(false);


        var enumerator = ManagerData._instance._questGameData.GetEnumerator();
        while(enumerator.MoveNext())
        {
            QuestGameData qData = enumerator.Current.Value;
            if((int)qData.type >= 1000 && (int)qData.type < 2000)
            {
                if(qData.state != QuestState.Finished)
                {
                    AddAlarmStateData(qData.type == QuestType.chapter_Candy ? 0 : 1);
                }
            }
        }

        yield return CoLastEpisodeCheck();

        if (alarmStateDic.Count > 0)
        {
            yield return CoAction();
        }
    }

    IEnumerator CoLastEpisodeCheck()
    {
        while (ManagerData._instance._state != DataLoadState.eComplete)
        {
            yield return null;
        }

        if (!PlayerPrefs.HasKey("LastEpisodeIndex"))
        {
            NewObjectAdd(false);
            yield return null;
        }

        if (PlayerPrefs.GetInt("LastEpisodeIndex") < ManagerData._instance.chapterData.Count)
        {
            NewObjectAdd(true);
        }
            
    }

    void NewObjectAdd(bool objAdd)
    {
        if(objAdd == true)
        {
            if (!alarmStateDic.ContainsKey(2))
                alarmStateDic.Add(2, true);
            else
                alarmStateDic[2] = true;
        }
        else
        {
            if (!alarmStateDic.ContainsKey(2))
                alarmStateDic.Add(2, false);
            else
                alarmStateDic[2] = false;

            alarmViewList[2].SetActive(false);
        }
    }

    private void AddAlarmStateData(int addIndex)
    {
        if (!alarmStateDic.ContainsKey(addIndex))
        {
            alarmStateDic.Add(addIndex, true);
        }
        else
        {
            alarmStateDic[addIndex] = true;
        }
    }

    private IEnumerator CoAction()
    {
        int index = 0;
        int count = 0;

        while (GetAlarmTrueCount() > 0)
        {
            index = ShowViewObject(index);
            count++;
            //말풍선 생성 연출
            objBubble.gameObject.SetActive(true);
            DOTween.ToAlpha(() => objBubble.color, color => objBubble.color = color, 1f, 0.2f);
            objBubble.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.3f);
            
            yield return new WaitForSeconds(delayTime);

            while(count < GetAlarmTrueCount())
            {
                count++;
                alarmViewList[index - 1].SetActive(false);
                index = ShowViewObject(index);

                yield return new WaitForSeconds(delayTime);
            }

            //말풍선 들어가는 연출
            objBubble.transform.DOScale(new Vector3(0f, 1f, 1f), 0.3f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.1f);
            DOTween.ToAlpha(() => objBubble.color, color => objBubble.color = color, 0f, 0.2f);
            yield return new WaitForSeconds(0.2f);
            objBubble.gameObject.SetActive(false);
            alarmViewList[index - 1].gameObject.SetActive(false);
            yield return new WaitForSeconds(delayTime);
            index = 0;
            count = 0;
        }
    }

    private int GetAlarmTrueCount()
    {
        int value = 0;
        foreach(var alarmState in alarmStateDic)
        {
            if(alarmState.Value == true)
            {
                value++;
            }
        }
        return value;
    }

    private int ShowViewObject(int _index)
    {
        for(int index = _index; index< alarmStateDic.Count; index++)
        {
            if (alarmStateDic[index] == true)
            {
                alarmViewList[index].SetActive(true);
                return index + 1;
            }

            _index++;
        }
        return _index;
    }

    public void LastEpisodeCheck(int stageIndex = -1)
    {
        if (stageIndex == -1)
        {
            PlayerPrefs.SetInt("LastEpisodeIndex", ManagerData._instance.chapterData.Count);
            NewObjectAdd(false);
        }
        else
        {
            if (PlayerPrefs.HasKey("LastEpisodeIndex") && getChapterIndex(stageIndex) > PlayerPrefs.GetInt("LastEpisodeIndex"))
            {
                PlayerPrefs.SetInt("LastEpisodeIndex", ManagerData._instance.chapterData.Count);
                NewObjectAdd(false);
            }
        }
    }

    private int getChapterIndex(int stageIndex)
    {
        for (int i = 0; i < ManagerData._instance.chapterData.Count; i++)
        {
            if (ManagerData._instance.chapterData[i]._stageIndex <= stageIndex && ManagerData._instance.chapterData[i]._stageIndex + ManagerData._instance.chapterData[i]._stageCount > stageIndex)
                return i + 1;
        }
        return -1;
    }
}
