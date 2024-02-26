using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine.Networking;

public class ManagerSingleRoundEvent : MonoBehaviour, IEventBase
{
    public static ManagerSingleRoundEvent instance = null;

    public StageMapData lastStageData = null;
    private static bool _isSingleRoundStage = false; //로비 진입 시 현재 스테이지가 수문장 크루그 스테이지인지 여부 저장
    private static bool _isPlaying = false; //로비 진입 시 이벤트 진행 여부 저장

    public static bool IsSingleRoundStage
    {
        get { return _isSingleRoundStage; }
    }

    public static bool IsPlaying
    {
        get { return _isPlaying; }
    }
    
    public GameEventType GetEventType()
    {
        return GameEventType.SINGLE_ROUND_EVENT;
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Init()
    {
        if (CheckStartable() == false)
            return;
        
        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerSingleRoundEvent>();
        if (instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(instance);
    }

    public IEnumerator SetIsPlaying()
    {
        if (CheckStartable() == false)
        {
            _isSingleRoundStage = false;
            _isPlaying = false;
            yield break;
        }
        else
        {
            _isPlaying = true;
        }
        
        yield return StartCoroutine(CoCheckLastStageData());

        if(instance == null ||
           lastStageData == null || 
           lastStageData.isHardStage != 1 ||
           CanPlaySingleRoundEvent() == false)
            _isSingleRoundStage = false;
        else
            _isSingleRoundStage = true;
        
        yield return null;
    }

    public static bool CheckStartable()
    {
        if (ServerContents.SingleRound == null)
            return false;
            
        if (Global.LeftTime(ServerContents.SingleRound.endTs) < 0 ||
            Global.LeftTime(ServerContents.SingleRound.startTs) > 0)
            return false;

        return true;
    }
    
    /// <summary>
    /// 수문장 크루그(단판 승부) 플레이 가능 여부 체크. 스테이지 데이터 생성(레디 팝업 오픈) 시점에만 호출. 시간 체크 하지 않음.
    /// </summary>
    /// <returns></returns>
    public static bool CanPlaySingleRoundEvent()
    {
        if (ServerContents.SingleRound == null)
            return false;
        
        if (ManagerUI._instance.IsCanPlayLastState() == false) // 마지막 스테이지 플레이 가능 여부 체크
            return false;
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        if (myProfile.stage < 50) // 유저 현재 스테이지 체크
            return false;
        
        if (ServerRepos.UserStages == null || ServerRepos.UserStages.Count == 0)
            return false;

        if (ServerRepos.UserStages.Count > 0 && ServerRepos.UserStages.Last().stage + 1 != myProfile.stage) // 플레이 이력이 있는지 체크. 로비일 때는 아직 스테이지 리스트에 추가되지 않은 상태
            return false;
        
        return true;
    }

    public Reward GetReward()
    {
        if(ServerContents.SingleRound.reward != null)
        {
            return ServerContents.SingleRound.reward;
        }
        else
        {
            return null;
        }
    }
    
    public void OnReboot()
    {
        _isSingleRoundStage = false;
        _isPlaying = false;
        
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    private IEnumerator CoCheckLastStageData()
    {
        lastStageData = null;
        
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        string stageName = Global.GetHashfromText("pp" + myProfile.stage + ".xml") + ".xml";;
        yield return ManagerUI._instance.CoCheckStageData(stageName, true);

        using (var www = UnityWebRequest.Get(Global.FileUri + Global.StageDirectory + stageName))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");

            yield return www.SendWebRequest();

            if (!www.IsError() && www.downloadHandler != null)
            {
                StringReader reader = new StringReader(www.downloadHandler.text);
                var serializer = new XmlSerializer(typeof(StageMapData));
                StageMapData tempData = serializer.Deserialize(reader) as StageMapData;

                yield return lastStageData = tempData;
            }
        }
    }

    #region 사용하지 않는 함수

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }
    
    public void OnLobbyStart()
    {
    }
    
    public bool OnTutorialCheck()
    {
        return false;
    }
    
    public bool OnLobbySceneCheck()
    {
        return false;
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public IEnumerator OnTutorialPhase()
    {
        yield break;
    }

    public IEnumerator OnLobbyScenePhase()
    {
        yield break;
    }

    public IEnumerator OnRewardPhase()
    {
        yield break;
    }

    public void OnIconPhase()
    {
        
    }

    #endregion
}
