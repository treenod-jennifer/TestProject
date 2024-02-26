using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCapsuleGachaEvent : MonoBehaviour, IEventBase
{
    public static ManagerCapsuleGachaEvent Instance { get; private set; }
    public const string OutgameTutorialKey = "CapsuleGachaTutorial_Outgame";

    public static bool IsTutorialClear = false;

    public enum GachaType
    {
        NORMAL,
        RARE,
        TENGACHA,
    }
    void Awake()
    {
        Instance = this;
        IsTutorialClear = !OnTutorialCheck();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return false;
        }

        if (ServerContents.CapsuleGacha == null || ServerContents.CapsuleGacha.eventIndex == 0)
        {
            return false;
        }

        return Global.LeftTime(ServerContents.CapsuleGacha.endTs) > 0;
    }

    public static void Init()
    {
        if (Instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerCapsuleGachaEvent>();

        if (Instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(Instance);
    }

    public GameEventType GetEventType()
    {
        return GameEventType.CAPSULE_GACHA;
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {

    }

    public void OnIconPhase()
    {
        if (CheckStartable())
        {
            SideIcon.Maker.MakeIcon<SideIcon.IconCaosuleGachaEvent>(
            scrollBar: ManagerUI._instance.ScrollbarRight,
            init: (icon) => icon.Init(ServerContents.CapsuleGacha));
        }
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        GameObject obj = null;

        string path = $"CapsuleGacha/capsule_1/capsule1";
        GameObject prefabObject = Resources.Load(path) as GameObject;

        if (prefabObject != null)
        {
            obj = ManagerLobby.NewObject(prefabObject);

            AreaBase areaBase = prefabObject.GetComponent<AreaBase>();
            if (areaBase)
            {
                ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
            }
        }

        yield return ManagerCharacter._instance.LoadCharacters();

        if (obj != null)
        {
            CapsuleGachaAreaBase e = obj.GetComponent<CapsuleGachaAreaBase>();
            ManagerArea._instance.RegisterEventLobbyObject(e);
        }
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public bool OnLobbySceneCheck()
    {
        if (ManagerArea._instance.GetEventLobbyObject(GameEventType.CAPSULE_GACHA) != null && IsPlayCutScene())
        {
            return true;
        }

        SetPlayCutScene();

        return false;
    }

    public static bool IsPlayCutScene()
    {
        if (!CheckStartable())
            return false;

        //플레이 기록이 있으면 컷씬 재생하지 않음.
        if (ServerRepos.UserCapsuleGacha?.playCount > 0)
        {
            SetPlayCutScene();
        }

        if (PlayerPrefs.HasKey("CapsuleGachaEventIndex"))
        {
            bool played = PlayerPrefs.GetInt($"CapsuleGachaEventIndex") != ServerContents.CapsuleGacha.eventIndex;

            return played;
        }
        else
        {
            return true;
        }
    }

    public static void SetPlayCutScene()
    {
        if (PlayerPrefs.HasKey("CapsuleGachaEventIndex"))
        {
            bool played = PlayerPrefs.GetInt($"CapsuleGachaEventIndex") == ServerContents.CapsuleGacha.eventIndex;

            if (played)
                return;
        }

        PlayerPrefs.SetInt("CapsuleGachaEventIndex", ServerContents.CapsuleGacha.eventIndex);
    }

    public IEnumerator OnLobbyScenePhase()
    {
        if (CheckStartable())
        {
            SetPlayCutScene();

            yield return ManagerLobby._instance.WaitForSceneEnd(ManagerArea._instance.GetEventLobbyObject(GameEventType.CAPSULE_GACHA).GetAreaBase(), 1); ;
        }
    }

    public void OnLobbyStart()
    {
    }

    public IEnumerator OnPostLobbyEnter()
    {
        yield break;
    }

    public void OnReboot()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
    }

    public void OnEventIconClick(object data = null)
    { 
        if(CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopupCapsuleGacha>();
        }
        else
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
        }
    }

    public IEnumerator OnRewardPhase()
    {
        yield break;
    }

    public bool OnTutorialCheck()
    {
        if (ManagerCapsuleGachaEvent.CheckStartable() &&
            PlayerPrefs.GetInt(ManagerCapsuleGachaEvent.OutgameTutorialKey) != ServerContents.CapsuleGacha.eventIndex)
        {
            if (ServerRepos.UserCapsuleGacha.playCount > 0)
            {
                PlayerPrefs.SetInt(ManagerCapsuleGachaEvent.OutgameTutorialKey, ServerContents.CapsuleGacha.eventIndex);
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerator OnTutorialPhase()
    {
        ManagerEvent.instance.isTutorialPlay = true;

        PlayerPrefs.SetInt(ManagerCapsuleGachaEvent.OutgameTutorialKey, ServerContents.CapsuleGacha.eventIndex);
        ManagerTutorial.PlayTutorial(TutorialType.TutorialCapsuleGacha);
        if (ManagerTutorial._instance != null)
            yield return new WaitUntil(() => UIPopupCapsuleGacha._instance == null && ManagerTutorial._instance._playing == false);
    }

    public static bool CheckPlayStartCutScene()
    {
        bool found = false;

        if (PlayerPrefs.HasKey($"CapsuleGachaEventIndex"))
        {
            int capsuleEventIndex = PlayerPrefs.GetInt($"CapsuleGachaEventIndex");
            found = capsuleEventIndex == ServerContents.CapsuleGacha.eventIndex;
        }

        if (PlayerPrefs.HasKey($"CapsuleGachaEventIndex") && found)
        {
            return true;
        }
        
        return false;
    }

    public static bool CanBuyLimitedCapsuleItem(int goodsId)
    {
        if (ServerRepos.UserCapsuleGacha != null)
        {
            if (ServerRepos.UserCapsuleGacha.gainGachaJson[goodsId - 1] == 1)
            {
                return true;
            }
        }

        return false;
    }

    public static int GetGainLimitedItem()
    {
        int gainCount = 0;

        for (int i = 0; i < ServerRepos.UserCapsuleGacha.gainGachaJson.Length; i++)
        {
            if (ServerRepos.UserCapsuleGacha.gainGachaJson[i] == 1)
            {
                gainCount++;
            }
        }

        return gainCount;
    }

    public static GachaType GetGachaType(List<GradeReward> capsuleItemList)
    {
        if (capsuleItemList.Count > 9) return GachaType.TENGACHA;

        for (int i = 0; i < capsuleItemList.Count; i++)
        {
            if (capsuleItemList[i].grade != 1)
            {
                return GachaType.RARE;
            }
        }
        return GachaType.NORMAL;
    }

    public static bool IsFreeGacha()
    {
        var CapsuleGachaData = ServerRepos.UserCapsuleGacha;

        return CapsuleGachaData?.playCount == 0;
    }

    public static int GetCapsuleEventIndex()
    {
        return ServerContents.CapsuleGacha.eventIndex;
    }

    public static int GetResourceIndex()
    {
        return ServerContents.CapsuleGacha.resourceIndex;
    }

    public static bool IsNoEnoughCapsuleToyToken(int GachaCount)
    {
        int price = ServerContents.CapsuleGacha.price;
        int userToken = 0;

        if (ServerRepos.UserTokenAssets.TryGetValue(2, out var value)) userToken = value.amount;

        if (price * GachaCount > userToken)
            return true;
        else
            return false;
    }

    public static List<LimitReward> GetLimitItemRatio()
    {
        return ServerContents.CapsuleGacha.limitItemRatio;
    }
}