using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManagerLuckyRoulette : MonoBehaviour, IEventBase
{
    public static ManagerLuckyRoulette instance = null;
    
    private const string OBJ_PLAYER_PREFAB = "ShowedLuckyRoulette";
    
    //번들
    public LuckyRoulettePack luckyRoulettePack = null;
    
    
    #region ContentsData

    //뽑기 가격
    public int price;
    //일반 리워드
    public List<LuckyRouletteReward> reward;
    //스페셜 리워드
    public Reward completeReward;

    #endregion
    
    
    #region UserData
    
    //일반 리워드 상태
    public List<int> rewardState;
    //스페셜 리워드 획득 상태
    public int completeRewardState;
    
    #endregion
    
    
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

    #region SyncData

    private void SyncContentData()
    {
        price = ServerContents.LuckyRoulette.price;
        reward = ServerContents.LuckyRoulette.reward.OrderBy(x=> x.order_index).ToList();
        completeReward = ServerContents.LuckyRoulette.completeReward;
    }

    public void SyncUserData()
    {
        rewardState = ServerRepos.UserLuckyRoulette.rewardState;
        completeRewardState = ServerRepos.UserLuckyRoulette.completeRewardState;
    }
    
    #endregion

    public static void Init()
    {
        if (CheckStartable() == false)
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerLuckyRoulette>();
        if (instance == null)
            return;

        ManagerEvent.instance.RegisterEvent(instance);
    }
    
    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
            return false;

        if (ServerContents.LuckyRoulette == null || ServerRepos.UserLuckyRoulette == null)
            return false;

        if (Global.LeftTime(ServerContents.LuckyRoulette.endTs) < 0)
            return false;
        
        return true;
    }
    
    void IEventBase.OnReboot()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    public void OnEventIconClick(object obj = null)
    {
    }

    GameEventType IEventBase.GetEventType()
    {
        return GameEventType.LUCKY_ROULETTE;
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
        if (CheckStartable())
        {
            string assetName = "shop_0";
            ManagerAssetLoader.BundleRequest bundleReq = new ManagerAssetLoader.BundleRequest()
            {
                uri = assetName,
                successCallback = null,
                failCallback = (r) =>
                {
                    Debug.LogWarning("Download Asset Bundle(Chapter) Error");
                    failCallback(r, $"{assetName}\n");
                }
            };
            loadList.Add(bundleReq);
        }
    }

    void IEventBase.OnLobbyStart()
    {
        if (instance != null && ServerContents.LuckyRoulette != null && ServerRepos.UserLuckyRoulette != null)
        {
            instance.SyncContentData();
            instance.SyncUserData();
        }
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        if (ManagerArea._instance.GetEventLobbyObject(GameEventType.LUCKY_ROULETTE) != null &&
            ServerContents.LuckyRoulette != null &&
            CheckStartable())
        {
            return !IsLobbyObjActive(ServerContents.LuckyRoulette.vsn);
        }
        return false;
    }
    
    public bool IsLobbyObjActive(int eventId)
    {
        HashSet<int> showedLuckyRoulette = new HashSet<int>();

        if (PlayerPrefs.HasKey(OBJ_PLAYER_PREFAB))
        {
            var tmpList = JsonFx.Json.JsonReader.Deserialize<int[]>(PlayerPrefs.GetString(OBJ_PLAYER_PREFAB));

            for (int i = 0; i < tmpList.Length; ++i)
            {
                showedLuckyRoulette.Add(tmpList[i]);
            }
        }

        return showedLuckyRoulette.Contains(eventId);
    }
    
    private void SetLobbyObjActive(int eventId)
    {
        HashSet<int> showedLuckyRoulette = new HashSet<int>();

        if (PlayerPrefs.HasKey(OBJ_PLAYER_PREFAB))
        {
            var tmpList = JsonFx.Json.JsonReader.Deserialize<int[]>(PlayerPrefs.GetString(OBJ_PLAYER_PREFAB));

            for (int i = 0; i < tmpList.Length; ++i)
            {
                showedLuckyRoulette.Add(tmpList[i]);
            }
        }

        showedLuckyRoulette.Add(eventId);

        string showData = JsonFx.Json.JsonWriter.Serialize(showedLuckyRoulette);

        PlayerPrefs.SetString(OBJ_PLAYER_PREFAB, showData);
    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield return CoLoadAssetBundle();
    }
    
    public IEnumerator CoLoadAssetBundle()
    {
        string assetName = $"lucky_roulette_{ServerContents.LuckyRoulette.resourceIndex}";

        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            string path =
                $"Assets/5_OutResource/luckyRoulette/{assetName}/LuckyRoulettePack.prefab";
            GameObject bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (bundleObj.GetComponent<LuckyRoulettePack>() != null)
                luckyRoulettePack = bundleObj.GetComponent<LuckyRoulettePack>();
#endif
        }
        else
        {
            NetworkLoading.MakeNetworkLoading(0.5f);
                
            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(assetName);
            while (e.MoveNext())
            {
                yield return e.Current;
            }
                
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    GameObject bundleObj = assetBundle.LoadAsset<GameObject>($"LuckyRoulettePack");
                        
                    if (bundleObj.GetComponent<LuckyRoulettePack>() != null)
                        luckyRoulettePack = bundleObj.GetComponent<LuckyRoulettePack>();
                }
            }
                
            NetworkLoading.EndNetworkLoading();
        }
        yield return null;
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        if (CheckStartable())
        {
            string eventObjAssetName = "shop_0";
            GameObject obj = null;
            if (Global.LoadFromInternal)
            {
#if UNITY_EDITOR
                string path = "Assets/5_OutResource/shop/" + eventObjAssetName + "/" + eventObjAssetName + ".prefab";
                GameObject bundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                obj = ManagerLobby.NewObject(bundleObject);

                AreaBase areaBase = bundleObject.GetComponent<AreaBase>();
                if (areaBase)
                {
                    ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                }
#endif
            }
            else
            {
                IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(eventObjAssetName);
                while (e.MoveNext())
                    yield return e.Current;
                if (e.Current != null)
                {
                    AssetBundle assetBundle = e.Current as AssetBundle;
                    if (assetBundle != null)
                    {
                        GameObject objn = assetBundle.LoadAsset<GameObject>(eventObjAssetName);
                        obj = ManagerLobby.NewObject(objn);

                        AreaBase areaBase = objn.GetComponent<AreaBase>();
                        if (areaBase)
                        {
                            ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                        }
                    }
                }
            }

            yield return ManagerCharacter._instance.LoadCharacters();

            if (obj != null)
            {
                LuckyRouletteObj eventObj = obj.GetComponent<LuckyRouletteObj>();
                ManagerArea._instance.RegisterEventLobbyObject(eventObj);
            }
            yield return null;
        }
    }

    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield return null;
    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        var lobbyObject = ManagerArea._instance.GetEventLobbyObject(GameEventType.LUCKY_ROULETTE);
        if( lobbyObject == null)
            yield break;

        if (ServerContents.LuckyRoulette != null && ServerRepos.UserLuckyRoulette != null && !IsLobbyObjActive(ServerContents.LuckyRoulette.vsn))
        {
            SetLobbyObjActive(ServerContents.LuckyRoulette.vsn);

            yield return ManagerLobby._instance.WaitForSceneEnd(lobbyObject.GetAreaBase(), 1);
        }
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield return null;
    }

    void IEventBase.OnIconPhase()
    {
        if (CheckStartable())
        {
            ManagerUI._instance.SidebarLeft.AddBannerMaker((int)PackageType.LuckyRoulette, () =>
            {
                SideBanner.Maker.MakeBanner<SideBanner.BannerLuckyRoulette>(
                    sidebar: ManagerUI._instance.SidebarLeft,
                    init: (banner) => banner.Init(ServerContents.LuckyRoulette));
            });
        }
    }
}
