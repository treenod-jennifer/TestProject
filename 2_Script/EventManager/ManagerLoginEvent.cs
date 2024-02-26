using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerLoginEvent : MonoBehaviour, IEventBase
{
    public static ManagerLoginEvent instance = null;
    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerLoginEvent>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        //로그인 이벤트.
        if (ServerRepos.LoginCdn.loginEventVer != 0 && ServerContents.LoginEvent != null &&
                Global.LeftTime(ServerContents.LoginEvent.endTs) > 0)   // 이벤트가 존재하는지 먼저 체크
        {
            return true;
        }

        return false;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.LOGIN_EVENT;
    }

    void IEventBase.OnLobbyStart()
    {
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return false;
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield break;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        if (ManagerLobby._firstLogin && Global._onlineMode)
        {
            if (ServerRepos.LoginCdn.loginEventVer != 0 && ServerContents.LoginEvent != null &&
                Global.LeftTime(ServerContents.LoginEvent.endTs) > 0)   // 이벤트가 존재하는지 먼저 체크
            {
                NetworkLoading.MakeNetworkLoading(0.5f);
                bool loginWaitFlag = true;
                bool loginDataExist = false;
                ServerAPI.LoginEvent((x) => {
                    loginWaitFlag = false;
                    loginDataExist = x.LoginEvent != null;
                });
                while (loginWaitFlag)
                {
                    yield return null;
                }

                NetworkLoading.EndNetworkLoading();

                // 강제 로그인 이벤트 띄워서 테스트하기 위한 코드 (밑에 있는 if (loginDataExist) 라인 주석치고 이 줄을 살려주면 됨)
                //ServerRepos.SaveLoginEvent(new ServerUserLoginEvent() { loginEventCnt = 1, loginEventReward = new Reward() { type = 1, value = 1 } });

                if (loginDataExist) // ServerAPI.LoginEvent 에서 LoginEvent를 리턴한 경우에만 오늘 로그인이벤트 처리를 하는거 (null로 오는 경우에는 이미 한번 본 거)
                {
                    yield return ManagerUI._instance.OpenPopupLoginBonus(ServerContents.LoginEvent.conceptIndex);
                    yield return new WaitUntil(() => UIPopupLoginBonus._instance == null);
                }
            }
        }
    }

    void IEventBase.OnIconPhase()
    {
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }
}
