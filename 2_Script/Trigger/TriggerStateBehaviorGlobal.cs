using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TypeBehaviorGlobal
{
    Normal,             // 평소 돌리는거(전화, 신문등등 그 자리에서 행동하고 말풍선 나오는)


    Hello_7Below,       // 앱기동시 7시간 이내일때
    Hello_24Below,      // 24시이내
    Hello_48Below,      // 48이내
    Hello_7DayBelow,    // 7일 이내
    Hello_7DayMore,     // 7일 이상
    


    QuestClear,         // 다이어리에서 퀘스트 완료해서 보상 획득후 "열시미 일한 후 보상 아 딸콤해"
    HousingSet,         // 재료로 데코 획득후 "점점더 멋진 집이 되어가고 있어. 영감이 놀라겠지?"
    ClassUp,            // 좋아 계급이 올랐어.
    StampUse,           // 스템프를 사용하고 나서 "메세지를 적어 친한 친구들에게 보내면 기뻐했겠죠?"
    

    GoodStageClear,     // 2~4판 연속 실패후 클리어
    NiceStageClear,     // 5판이상 연속 실패후 클리어


    GroundTouch,    // 그냥 땅을 터치해서 보니를 이동시킬때 "바쁘다 바빠." "이쪽으로 말이지?"
}

public class TriggerStateBehaviorGlobal : TriggerState
{
#if UNITY_EDITOR
    [Header("Comment", order = 1000)]
    [MultiLineString(100.0f, order = 1000)]
    public string comment = "";
#endif

    public int startMissionIndex = 0;
    public int endMissionIndex = 0;
    public TypeBehaviorGlobal _mode = TypeBehaviorGlobal.Normal;

    void Awake()
    {
        _type = TypeTriggerState.BehaviorGlobal;
        for (int i = 0; i < _conditions.Count; i++)
        {
            _conditions[i]._stateType = _type;
            _conditions[i].gameObject.SetActive(false);
        }
        if (LobbyBehavior._instance != null)
            LobbyBehavior._instance._behaviorGlobal.Add(this);
        gameObject.SetActive(false);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
