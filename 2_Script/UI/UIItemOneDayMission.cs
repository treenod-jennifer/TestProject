using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIItemOneDayMission : MonoBehaviour
{
    public static UIItemOneDayMission _instance = null;

    [SerializeField] private GameObject oneDayMissionObjRoot;
    [SerializeField] private UIProgressBar progressBar;
    [SerializeField] private UILabel progressPercentCurrent;
    [SerializeField] private UILabel progressPercentMax;

    [SerializeField] private Vector3 itemOnedayMissionPos;

    [SerializeField] private float speed = 1.0f;


    private int clearMissionCount = 0;
    private int dayMissionCount = 0;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        InitData();
    }


    private void InitData()
    {
        //현재 미션 카운트와 첫째날의 미션 갯수
        dayMissionCount = ManagerData._instance.chapterData[0]._stageCount;
        clearMissionCount = ServerRepos.User.missionCnt;
    }

    //미션창 위치 조정
    public void SetOneDayMissionPosition()
    {
        transform.localPosition = itemOnedayMissionPos;
    }

    private void OnEnable()
    {
        if (ServerRepos.User.missionCnt < dayMissionCount && ManagerLobby._instance != null)
        {
            oneDayMissionObjRoot.SetActive(true);
            IsRunCoroutine();
        }
        else
        {
            oneDayMissionObjRoot.SetActive(false);
        }
    }
    //프로그래스바 연출
    private Coroutine runCoroutine;

    public bool IsRun { get { return runCoroutine != null; } }

    public void IsRunCoroutine()
    {
        if (IsRun) return;
        runCoroutine = StartCoroutine(CoMoveProgressBar());
    }

    private IEnumerator CoMoveProgressBar()
    {
        float totalTime = 0.0f;
        //프로그래스바 값 설정
        clearMissionCount = ServerRepos.User.missionCnt;
        float targetValue = (float)clearMissionCount / (float)ManagerData._instance.chapterData[0]._stageCount;
        progressPercentCurrent.text = string.Format("{0}", clearMissionCount);
        progressPercentMax.text = string.Format("/ {0}", dayMissionCount);

        //보니 위치 설정
        Vector3 targetVector = new Vector3(progressBar.GetComponent<UISprite>().width
            * ((float)clearMissionCount / (float)dayMissionCount), 0.0f, 0.0f) + progressBar.transform.localPosition;

        //움직이는 애니메이션
        while (progressBar.value < targetValue)
        {
            totalTime += Time.unscaledDeltaTime * speed;

            progressBar.value = Mathf.Lerp(progressBar.value, targetValue, totalTime);

            yield return null;
        }

        runCoroutine = null;
    }

    public void OneDayMissionClear()
    {
        //보상처리 - 하루 미션
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.SortOrderSetting();
        string text = Global._instance.GetString("n_s_49");        
        popupSystem._callbackEnd += objDestroy;
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);
        popupSystem.SetResourceImage("Message/coin");
    }

    public void objDestroy()
    {
        Destroy(this.gameObject);
    }
}
