using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerSpecialEvent : MonoBehaviour, IEventBase
{
    static public ManagerSpecialEvent instance = null;
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

        Global._instance.gameObject.AddMissingComponent<ManagerSpecialEvent>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt > ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            foreach (var item in ServerContents.SpecialEvent)
            {
                long leftTime = Global.LeftTime(item.Value.endTs);

                if (leftTime > 0)
                {
                    Global.specialEventIndex = item.Value.index;
                    return true;
                }
            }
        }
        return false;

    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.SPECIAL_EVENT;
    }

    void IEventBase.OnIconPhase()
    {
        //스페셜이벤트.
        if(CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconSpecialEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(Global.specialEventIndex));
        }
        else
        {
            Global.specialEventIndex = 0;
        }
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield break;
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        yield break;
    }

    void IEventBase.OnLobbyStart()
    {
        
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        //스페셜이벤트.
        if (Global.specialEventIndex > 0)
        {
            if (ServerContents.SpecialEvent.ContainsKey(Global.specialEventIndex))
            {
                long leftTime = Global.LeftTime(ServerContents.SpecialEvent[Global.specialEventIndex].endTs);

                if (leftTime <= 0)
                {
                    Global.specialEventIndex = 0;
                }
            }
            else
                Global.specialEventIndex = 0;
        }

        //남아있는 키 제거.
        if (PlayerPrefs.HasKey("ShowSpeicalEventPopup") == true)
        {
            PlayerPrefs.DeleteKey("ShowSpeicalEventPopup");
        }

        yield break;
    }

    void IEventBase.OnReboot()
    {
        Global.specialEventIndex = 0;

        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object obj = null)
    {
        ManagerUI._instance.OpenPopupSpecialEvent(Global.specialEventIndex, true);
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        ////모으기 이벤트 상태일 때, 목적 달성하면 띄우기.
        //if (Global.specialEventIndex > 0 && PlayerPrefs.HasKey("ShowSpeicalEventPopup"))
        //{
        //    //첫 진입 시 남아있는 키는 제거.
        //    if (ManagerLobby._firstLobby)
        //    {
        //        PlayerPrefs.DeleteKey("ShowSpeicalEventPopup");
        //    }
        //    else
        //    {
        //        ManagerUI._instance.OpenPopupSpecialEvent(Global.specialEventIndex, false);
        //        yield return null;
        //        while (UIPopupSpecialEvent._instance != null)
        //            yield return null;
        //    }
        //}
        yield return null;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return false;
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }
}
