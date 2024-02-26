using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum TutorialConditionType
{
    WAIT_TIME,
    BLOCKSTATE,
    POPUPSTATE,
    CLICK_BUTTON,//특정 버튼 함수에 콜백을 넣을건지..?
}

public enum TutorialActionType
{
    BIRD,
    BLIND_SETDATA,
    BLIND_MAKE,
    FINGER,
    IMAGE,
    OBJECT,
}

[System.Serializable]
public class TutorialData
{
    public List<TutorialData_Wave> listTutorialWaveData = new List<TutorialData_Wave>();
}

public class TutorialBase : MonoBehaviour
{
    [System.NonSerialized]
    public Transform _transform = null;
    public TutorialType _tutorialType;

    public TutorialData tutorialData = new TutorialData();

    public BlindTutorial blind = null;
    public TextboxTutorial textBox = null;

    //튜토리얼 내에서 쓸 데이터들 중 값이 변할 수 있는 데이터.
    public Dictionary<int, List<GameObject>> dicGameObject = new Dictionary<int, List<GameObject>>();
    public Dictionary<int, List<Sequence>> dicTweenSequence = new Dictionary<int, List<Sequence>>();
    public Dictionary<int, List<CustomBlindData>> dicCustomBlindData = new Dictionary<int, List<CustomBlindData>>();
    public Dictionary<int, UITexture> dicFinger = new Dictionary<int, UITexture>();

    public List<GameObject> listWaveObj;

    void Awake()
    {
        _transform = transform;
    }

    protected virtual void OnDestroy()
    {
        if (ManagerTutorial._instance != null)
        {
            string growthyName = "TUTORIAL" + ((int)_tutorialType).ToString() + "_E";
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthySequentialEventTutorial(growthyName);
            ManagerTutorial._instance._current = null;
            ManagerTutorial._instance._playing = false;
        }
    }

    public void InitTutorialData()
    {
        tutorialData.listTutorialWaveData.Clear();

        for (int i = 0; i < listWaveObj.Count; i++)
        {
            TutorialData_Wave waveData = listWaveObj[i].GetComponent<TutorialData_Wave>();
            waveData.listCondition.Clear();
            waveData.listAction.Clear();
            waveData.SetConditionList();
            waveData.SetActionList();

            tutorialData.listTutorialWaveData.Add(waveData);
        }
    }

    public void PlayTutorial()
    {
        if (tutorialData.listTutorialWaveData.Count == 0)
            return;
        StartCoroutine(CoPlayTutorial());
    }

    public IEnumerator CoPlayTutorial()
    {
        InitTutorial();
        //튜토리얼 재생.
        for (int i = 0; i < tutorialData.listTutorialWaveData.Count; i++)
        {
            TutorialData_Wave waveData = tutorialData.listTutorialWaveData[i];
            yield return CoStartCondition(waveData);
            yield return CoStartAction(waveData);
        }
    }

    //튜토리얼 초기화
    private void InitTutorial()
    {
        blind = ManagerTutorial._instance.MakeBlindPanel(ManagerTutorial._instance.transform);
        blind.transform.position = Vector3.zero;
        blind.transform.localPosition += Vector3.zero;
        blind._textureCenter.border = Vector4.one * 50;
        blind._panel.depth = 10;
        blind.SetSizeCollider(0, 0);

        UIRoot tutorialRoot = ManagerTutorial._instance.gameObject.GetComponent<UIRoot>();
        if (tutorialRoot != null)
            tutorialRoot.manualWidth = 750;
    }

    private IEnumerator CoStartCondition(TutorialData_Wave waveData)
    {
        int conditionCount = waveData.listCondition.Count;
        int endCount = 0;

        for (int i = 0; i < conditionCount; i++)
        {
            StartCoroutine(waveData.listCondition[i].StartCondition(() => endCount++));
        }
        yield return new WaitUntil(() => conditionCount <= endCount);
    }

    private IEnumerator CoStartAction(TutorialData_Wave waveData)
    {
        int actionCount = waveData.listAction.Count;
        int endCount = 0;

        for (int i = 0; i < actionCount; i++)
        {
            waveData.listAction[i].StartAction(() => endCount++);
        }
        yield return new WaitUntil(() => actionCount <= endCount);
    }

    #region 튜토리얼 테스트 코드
    public GameObject Get_DiaryIconObject()
    {
        return ManagerUI._instance.buttonDiary;
    }

    public List<GameObject> Get_ListMissionIconObject()
    {
        List<GameObject> listObj = new List<GameObject>();
        listObj.Add(UIDiaryMission._instance.GetMissionButton());
        return listObj;
    }

    public GameObject Get_MissionIconObject()
    {
        return UIDiaryMission._instance.GetMissionButton();
    }

    public bool IsCheckTutorialCondition()
    {
        return UIDiaryMission._instance.bClearButton;
    }

    public void Touch_Diary()
    {
        Touch_UIPokoButton(ManagerUI._instance.buttonDiary);
    }
    #endregion

    #region 인게임 UI 위치 가져오기
    public List<CustomBlindData> GetCustomBlindData_IngameUI_Turn()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        Vector3 targetPos = GameUIManager.instance.turnUi.transform.localPosition + new Vector3(0f, 90f, 0f);
        listCustomBlindData.Add(new CustomBlindData(targetPos, 330, 240));
        return listCustomBlindData;
    }

    public List<CustomBlindData> GetCustomBlindData_IngameUI_Target()
    {
        List<CustomBlindData> listCustomBlindData = new List<CustomBlindData>();
        Vector3 targetPos = GameUIManager.instance.Target_Root.transform.localPosition + new Vector3(0f, 25f, 0f);
        listCustomBlindData.Add(new CustomBlindData(targetPos, 280, 170));
        return listCustomBlindData;
    }
    #endregion

    #region 터치액션
    public void Touch_UIPokoButton(GameObject obj)
    {
        UIPokoButton poko = obj.GetComponent<UIPokoButton>();
        poko.OnPressAtTutorial();
    }

    public void Touch_InGameBlock()
    {
        Pick.instance.OnPressAtTutorial();
    }
    #endregion
}
