using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerStickerEvent : MonoBehaviour, IEventBase
{
    public static ManagerStickerEvent instance = null;
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

        Global._instance.gameObject.AddMissingComponent<ManagerStickerEvent>();
        if (instance == null)
            return;
        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        if (ServerRepos.UserEventStickers.Count > 0)
        {
            var stickerEventIndex = ServerRepos.UserEventStickers[0].eventIndex;
            if (ServerContents.EventStickers.ContainsKey(stickerEventIndex))
            {
                var t = Global.LeftTime(ServerContents.EventStickers[stickerEventIndex].endTs);
                if (t < 0)
                    t = 0;

                if (t > 0)
                    return true;
            }
        }
        return false;

    }
    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.STICKER;
    }

    void IEventBase.OnIconPhase()
    {
        if( CheckStartable() )
        {
            int eventIndex = ServerRepos.UserEventStickers[0].eventIndex;
            int endTs = ServerContents.EventStickers[eventIndex].endTs;

            var eventData = new SideIcon.NormalEvent(eventIndex, endTs);
            SideIcon.Maker.MakeIcon<SideIcon.IconStickerEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) => icon.Init(eventData));
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

    bool IEventBase.OnLobbySceneCheck()
    {
        return false;
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
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
        yield break;
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
            Destroy(instance);
    }

    public void OnEventIconClick(object data = null)
    {
        int eventIndex = data is int ? (int)data : 0;
        ManagerUI._instance.OpenPopupMissionStampEvent(eventIndex);
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield break;
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

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }
}
