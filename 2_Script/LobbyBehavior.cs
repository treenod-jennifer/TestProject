using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Pathfinding;


public class LobbyBehavior : MonoBehaviour {

    public static LobbyBehavior _instance = null;
    public bool _on = true;
    public float _spawnMinTime = 3f;
    public float _spawnMaxTime = 5f;

    public SkeletonAnimation _commandUI;

    public delegate void OnStarPathDelegate();

    public int _countBehavior = 0;
    public bool _playing = false;
    public float _timerBehavior = 0f;
    [SerializeField]
    private TriggerState _selectTrigger = null;

    public List<TriggerStateBehaviorScene> _behaviorScene = new List<TriggerStateBehaviorScene>();
    public List<TriggerStateBehaviorArea> _behaviorArea = new List<TriggerStateBehaviorArea>();
    public List<TriggerStateBehaviorGlobal> _behaviorGlobal = new List<TriggerStateBehaviorGlobal>();

    // 랜덤하게 순서를 바꾸고 각 종류에 따라 나열한 리스트
  /*  public List<TriggerStateBehaviorGlobal> _behaviorGlobal_Hello_7Below = new List<TriggerStateBehaviorGlobal>();
    public List<TriggerStateBehaviorGlobal> _behaviorGlobal_Hello_24Below = new List<TriggerStateBehaviorGlobal>();
    public List<TriggerStateBehaviorGlobal> _behaviorGlobal_Hello_48Below = new List<TriggerStateBehaviorGlobal>();
    public List<TriggerStateBehaviorGlobal> _behaviorGlobal_Hello_7DayBelow = new List<TriggerStateBehaviorGlobal>();
    public List<TriggerStateBehaviorGlobal> _behaviorGlobal_Hello_7DayMore = new List<TriggerStateBehaviorGlobal>();*/


    public TriggerState TempBehavior = null;


    
    void Awake()
    {
        _instance = this;
    }
	// Use this for initialization
	void Start () {
		
	}
    


    bool onBehavior = false;
    float timerBehaviorStart = 2f;
    public void EndBehavior()
    {
        _playing = false;
    }
    public void CancleBehavior()
    {
        if (_playing)
        {
            if (_selectTrigger != null)
            {
                for (int i = 0; i < _selectTrigger._conditions.Count; i++)
                {
                    _selectTrigger._conditions[i].Reset();
                    _selectTrigger._conditions[i].gameObject.SetActive(false);
                }

                AIChangeCommand command = new AIChangeCommand();
                command._state = AIStateID.eIdle;
                CharacterBoni._boni._ai.ChangeState(command);

                _selectTrigger = null;
            }
//            timerBehaviorStart = Random.Range(_spawnMinTime, _spawnMaxTime);
            _playing = false;
        }
        timerBehaviorStart = Random.Range(_spawnMinTime, _spawnMaxTime);
    }
	// Update is called once per frame
    void Update()
    {

      //  if (Input.GetKeyDown(KeyCode.C))
        //    CancleBehavior();

        if (!_on)
            return;

        bool skip = false;

        if (ManagerUI._instance != null)
        {
            if (ManagerUI._instance.GetPopupCount() > 0)
            {
                skip = true;

                if( _selectTrigger != null )
                {
                    CancleBehavior();
                }
            }
        }
            
        if (ManagerLobby._instance._state != TypeLobbyState.Wait)
            skip = true;

        if (ManagerTutorial._instance != null)
            if (ManagerTutorial._instance._playing)
                skip = true;

        if (_playing)
            skip = true;
        if(skip)
        {
            timerBehaviorStart = Random.Range(_spawnMinTime, _spawnMaxTime);
            return;
        }
        /*     timerBehaviorStart -= Global.deltaTimeLobby;
             if (timerBehaviorStart <= 0f && !onBehavior)
             {
                 StartBehavior();
              //   StartCoroutine(CoBehavior);
             }*/

        return;
#if false
        if(CharacterBoni._boni._ai.GetStateID() == AIStateID.eIdle)
        {
            timerBehaviorStart -= Global.deltaTimeLobby;
            if (timerBehaviorStart < 0f || Input.GetKeyDown(KeyCode.A))
            {
                timerBehaviorStart = Random.Range(_spawnMinTime, _spawnMaxTime);
                _selectTrigger = SelectBehavior();

                if (_selectTrigger != null)
                    PlayBehavior(_selectTrigger);
            }
        }
#else
        if (CharacterBoni._boni._ai.GetStateID() != AIStateID.eIdle)
            timerBehaviorStart = Random.Range(_spawnMinTime, _spawnMaxTime);
            //timerBehaviorStart = 0f;
        else

            timerBehaviorStart -= Global.deltaTimeLobby;

        if(CharacterBoni._boni._ai.GetStateID() == AIStateID.eIdle)
        {
            if (timerBehaviorStart < 0f)// || Input.GetKeyDown(KeyCode.A))
            {
                timerBehaviorStart = Random.Range(_spawnMinTime, _spawnMaxTime);
                _selectTrigger = SelectBehavior();

                if (_selectTrigger != null)
                    PlayBehavior(_selectTrigger);
            }
        }
#endif
        //   if (Input.GetKeyDown(KeyCode.A))
        //       SelectBehavior();
    }
    //List<TriggerState> tempWaitList = new List<TriggerState>();
    //List<TriggerState> tempAcitveList = new List<TriggerState>();
    //List<TriggerState> tempFinishList = new List<TriggerState>();

    List<TriggerState> selectList = new List<TriggerState>();


    // 처음 행동은 무조껀 글로발(로그인(자주,오랜만에),게임 성공시,실패시)
    // 두번째는 무조껀 해야할 미션
    // 세번째는 랜덤(area행동 + 클리어 미션에 대한 행동 + 해야할 미션에 대한 행동)
    static bool firstLogin = false;

    public void ResetSelectBehavior()
    {
        selectList.Clear();
    }
    public void PlayHelloBehavior()
    {
        if (_behaviorGlobal != null)
        {
            if (_behaviorGlobal.Count > 0)
            {/*
                templist.Clear();
                long deltaTime = Global.GetTime() - ServerRepos.User.loginTs;
                TypeBehaviorGlobal serchMode = TypeBehaviorGlobal.Hello_24Below;
                if (deltaTime < 60 * 60 * 7)
                {
                    serchMode = TypeBehaviorGlobal.Hello_7Below;
                }
                else if (deltaTime < 60 * 60 * 24)
                {
                    serchMode = TypeBehaviorGlobal.Hello_24Below;
                }
                else if (deltaTime < 60 * 60 * 24 * 2)
                {
                    serchMode = TypeBehaviorGlobal.Hello_48Below;
                }
                else if (deltaTime < 60 * 60 * 24 * 7)
                {
                    serchMode = TypeBehaviorGlobal.Hello_7DayBelow;
                }
                else
                {
                    serchMode = TypeBehaviorGlobal.Hello_7DayMore;
                }

                for (int i = 0; i < _behaviorGlobal.Count; i++)
                {
                    if (_behaviorGlobal[i]._mode == serchMode)
                        templist.Add(_behaviorGlobal[i]);
                }
                _selectTrigger = templist[Random.Range(0, templist.Count)];*/
            }
        }
        if (_selectTrigger != null)
            PlayBehavior(_selectTrigger);
    }
    public void PlayUpdateStageBehavior(int in_tryCount)
    {
        templist.Clear();
        if (in_tryCount >= 2 && in_tryCount <= 4)
        {
            for (int i = 0; i < _behaviorGlobal.Count; i++)
            {
                if (_behaviorGlobal[i]._mode == TypeBehaviorGlobal.GoodStageClear)
                    templist.Add(_behaviorGlobal[i]);
            }
        }
        else if (in_tryCount >= 5)
        {
            for (int i = 0; i < _behaviorGlobal.Count; i++)
            {
                if (_behaviorGlobal[i]._mode == TypeBehaviorGlobal.NiceStageClear)
                    templist.Add(_behaviorGlobal[i]);
            }
        }
        if (templist.Count > 0)
        {
            _selectTrigger = templist[Random.Range(0, templist.Count)];
            if (_selectTrigger != null)
                PlayBehavior(_selectTrigger);
        }

    }
    public void PlayUpdateAppBehavior()
    {

    }
    List<TriggerStateBehaviorGlobal> templist = new List<TriggerStateBehaviorGlobal>();
    TriggerState SelectBehavior()
    {
        TriggerState resultBehavior = null;

        if (selectList.Count == 0)
        {
            for (int i = 0; i < _behaviorScene.Count; i++)
            {
                TriggerStateBehaviorScene behavior = _behaviorScene[i];

                if (behavior._missionIndex == 0 || i >= ManagerData._instance._missionData.Count)
                    continue;




                TypeMissionState sceneState = ManagerData._instance._missionData[behavior._missionIndex].state;

                if (sceneState == TypeMissionState.Inactive && behavior._onWait)
                {
                    if (behavior.activeSize == 0f)
                        selectList.Add(behavior);
                    else if ((behavior.transform.position - CharacterBoni._boni._transform.position).magnitude < behavior.activeSize)
                        selectList.Add(behavior);
                }
                else if (sceneState == TypeMissionState.Active && behavior._onActive)
                {
                    if (behavior.activeSize == 0f)
                        selectList.Add(behavior);
                    else if ((behavior.transform.position - CharacterBoni._boni._transform.position).magnitude < behavior.activeSize)
                        selectList.Add(behavior);
                }
                else if (sceneState == TypeMissionState.Clear && behavior._onFinish)
                {
                    if (behavior.activeSize == 0f)
                        selectList.Add(behavior);
                    else if ((behavior.transform.position - CharacterBoni._boni._transform.position).magnitude < behavior.activeSize)
                        selectList.Add(behavior);
                }
            }

            for (int i = 0; i < _behaviorArea.Count; i++)
            {
                TriggerStateBehaviorArea behavior = _behaviorArea[i];

                if (behavior.activeSize == 0f)
                    selectList.Add(behavior);
                else if ((behavior.transform.position - CharacterBoni._boni._transform.position).magnitude < behavior.activeSize)
                    selectList.Add(behavior);
            }
        }

        if (selectList.Count > 0)
        {
            resultBehavior = selectList[Random.Range(0, selectList.Count - 1)];
            selectList.Remove(resultBehavior);

            return resultBehavior;
        }
        return null;

   /*     tempWaitList.Clear();
        tempAcitveList.Clear();
        tempFinishList.Clear();

        for (int i = 0; i < _behaviorScene.Count; i++)
        {
        //    Debug.Log(_behaviorScene[i]._missionIndex);
         //   Debug.Log(_behaviorScene[i].transform.parent.name +  _behaviorScene[i]._missionIndex);
            if (_behaviorScene[i]._missionIndex == 0 || i >= ManagerData._instance.missionData.Count)
                continue;

            TypeMissionState sceneState = ManagerData._instance.missionData[_behaviorScene[i]._missionIndex - 1].state;

            if (sceneState == TypeMissionState.Inactive && _behaviorScene[i]._onWait)
            {
                tempWaitList.Add(_behaviorScene[i]);
            }

            if (sceneState == TypeMissionState.Active && _behaviorScene[i]._onActive)
            {
                tempAcitveList.Add(_behaviorScene[i]);
            }

            if (sceneState == TypeMissionState.Clear && _behaviorScene[i]._onFinish)
            {
                tempFinishList.Add(_behaviorScene[i]);
            }
        }

        //  두번째는 무조껀 해야할 미션
        if (_countBehavior == 0)
        {
            if (tempAcitveList.Count > 0)
            {
                resultBehavior = tempAcitveList[Random.Range(0, tempAcitveList.Count - 1)];
                return resultBehavior;
            }
        }

        // 나머질들 랜덤
        if (_countBehavior >= 1 || resultBehavior == null)
        {
            float value = Random.value;

            if (value < 0.33f)
            {
                if (tempWaitList.Count > 0)
                {
                    resultBehavior = tempWaitList[Random.Range(0, tempWaitList.Count - 1)];
                    return resultBehavior;
                }
            }
            else if (value < 0.66f)
            {
                if (tempFinishList.Count > 0)
                {
                    resultBehavior = tempFinishList[Random.Range(0, tempFinishList.Count - 1)];
                    return resultBehavior;
                }
            }
            else
            {
                if (_behaviorArea.Count > 0)
                {
                    resultBehavior = _behaviorArea[Random.Range(0, _behaviorArea.Count - 1)];
                    return resultBehavior;
                }
            }
            // 그래도 비어있으면 앞으로 할일 표시
            if (resultBehavior == null)
            {
                if (tempWaitList.Count > 0)
                {
                    resultBehavior = tempWaitList[Random.Range(0, tempWaitList.Count - 1)];
                    return resultBehavior;
                }
            }
        }*/
        return null;
    }
    public void PlayBehavior(TriggerState in_behavior)
    {

        if (in_behavior == null)
            return;

        _countBehavior++;
        _playing = true;
        in_behavior.Reset();
        in_behavior.gameObject.SetActive(true);
        in_behavior.StartCondition();
    }

    public void PlayExternalBehavior(TriggerState in_behavior)
    {

        if (in_behavior == null)
            return;

        _selectTrigger = in_behavior;

        _countBehavior++;
        _playing = true;
        in_behavior.Reset();
        in_behavior.gameObject.SetActive(true);
        in_behavior.StartCondition();
    }

    IEnumerator CoBehavior(TriggerState in_behavior)
    {
        in_behavior.Reset();

        in_behavior.gameObject.SetActive(true);
        in_behavior.StartCondition();


        yield return null;
    }
    public void CommandObjectEvent(ObjectEvent in_obj)
    {

    }
    public void CommandWalk()
    {
        if( _playing )
        {
            CancleBehavior();
        }

        Vector3 target = CameraController._instance.GetWorldPosFromScreen(Global._touchPos);
        Character._boni.StartPath(target, OnPathComplete,true);
        
    }
    public virtual void OnPathComplete(Path _p)
    {
        // 못가는 곳
        if (_p.vectorPath.Count == 0)
        {
            CantRun();
            Character._boni.StopPath();
            
            return;
        }
        if ((_p.vectorPath[_p.vectorPath.Count - 1] - Character._boni._targetPos).sqrMagnitude > 0.1f)
        {
            Character._boni._targetPos = _p.vectorPath[_p.vectorPath.Count - 1];
        }
        if (!_commandUI.gameObject.active)
            _commandUI.gameObject.SetActive(true);

        _commandUI.transform.position = Character._boni._targetPos;
        _commandUI.skeleton.SetSkin("click_walk");
        _commandUI.skeleton.SetSlotsToSetupPose();

        _commandUI.state.SetAnimation(0, "click_walk_start", false);
        _commandUI.state.AddAnimation(0, "click_walk_loop", true, 0.5f);
    }
    public void CompleteRun()
    {
        if (_commandUI.gameObject.active)
            _commandUI.state.SetAnimation(0, "click_walk_end", false);
    }
    public void CantRun()
    {
        if (_commandUI.gameObject.active)
        {
            _commandUI.transform.position = Character._boni._targetPos;
            _commandUI.skeleton.SetSkin("click_x");
            _commandUI.skeleton.SetSlotsToSetupPose();
            _commandUI.state.SetAnimation(0, "click_x", false);
        }
    }
}
