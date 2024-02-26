using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAdventure : MonoBehaviour {

    public interface IUserAnimalListener
    {
        void OnAnimalChanged(int animalIdx);
    }

    public interface IUserDeckListener
    {
        void OnDeckChanged();
    }

    enum ModeState
    {
        CLOSED,
        LOAD_START,
        WEBDATA_LOADED,
        SERVERDATA_LOADED,
    }


    ModeState state = ModeState.CLOSED;
    static public ManagerAdventure instance;

    static public ManagerAnimalInfo Animal;
    static public ManagerStageInfo Stage;

    static public UserData User;

    public bool ExpEvent { get; private set;} = false;

    public Dictionary<string, string> _stringData = new Dictionary<string, string>();
    
    private void Awake()
    {
        instance = this;
        User = gameObject.AddComponent<UserData>();
        NewMarkUtility.newAnimalDataLoad();
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            if (User != null)
            {
                Destroy(User);
                User = null;
            }
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    static public bool CheckStartable()
    {
        if (ServerRepos.LoginCdn.adventureVer == 0)
            return false;

        if ( ServerRepos.User.missionCnt < 26 )  // day 2 완료이후 오픈
            return false;

        return true;
    }

    static public void OnInit(System.Action<bool> initCompleteCallback = null)
    {
        if( instance.state >= ModeState.SERVERDATA_LOADED)
        {
            instance.StartCoroutine(CheckChapterOpen(initCompleteCallback));
            return;
        }

        if(instance.state > ModeState.CLOSED)
        {
            return;
        }

        instance.state = ModeState.LOAD_START;

        Stage = new ManagerStageInfo();
        Animal = new ManagerAnimalInfo();

        if( User == null)
            User = instance.gameObject.AddComponent<UserData>();

        instance.StartCoroutine(InitAdventureMode(initCompleteCallback));

        //Stage.SetTestData();
        //Animal.SetTestData();
        //User.SetTestData();
    }

    static public void OnLobbyEnter()
    {
        if( instance != null )
        {
            instance.ExpEvent = ServerRepos.LoginCdn.advExpEventRatio > 0 && Global.LeftTime(ServerRepos.LoginCdn.advExpEventEndTs) > 0;
        }
    }

    static public bool Active
    {
        get
        {
            return instance.state == ModeState.SERVERDATA_LOADED;
        }
    }

    static public void OnReboot()
    {
        Debug.Log("ManagerAdv.OnReboot");
        UIPopupStageAdventure.ResetStaticVariables();
        if ( User != null )
            User.OnReboot();
        if( Animal != null )
            Animal.OnReboot();
        if( Stage != null )
            Stage.OnReboot();

        if (User != null)
        {
            Destroy(User);
            User = null;
            Debug.Log("ManagerAdv.OnReboot.UserDestroy");
        }
        Animal = null;
        Stage = null;
        instance.state = ModeState.CLOSED;
    }

    static IEnumerator InitAdventureMode(System.Action<bool> initCompleteCallback = null)
    {
        instance.state = ModeState.LOAD_START;
        yield return Load();

        instance.state = ModeState.WEBDATA_LOADED;

        bool initRet = false;
        ServerAPI.AdventureInit((Protocol.AdventureInitResp resp)
            => {
                initRet = true;
                if (resp.IsSuccess)
                {
                }
            });

        yield return new WaitUntil(() => initRet == true);

        initRet = false;
        ServerAPI.AdventureLoadContents((resp)
            => {
                initRet = true;
            });

        yield return new WaitUntil(() => initRet == true);
        

        Stage.LoadFromServerContents();

        instance.state = ModeState.SERVERDATA_LOADED;

        User.SyncFromServerData();

        initCompleteCallback(true);
        yield break;
    }

    static IEnumerator CheckChapterOpen(System.Action<bool> initCompleteCallback = null)
    {
        bool initRet = false;

        if( ManagerAdventure.Stage.CheckNowOpenedChapter() )
        {
            ServerAPI.AdventureContentsReFlash((resp) => { initRet = true; });
            while (initRet == false)
                yield return new WaitForSeconds(0.1f);

            Stage.LoadFromServerContents();
        }

        initCompleteCallback(false);
        yield break;
    }

    public string GetString(string in_key)
    {
        if (_stringData.ContainsKey(in_key))
            return _stringData[in_key];
        else
            return in_key + ": string empty";
    }

    public const string OpenSceneKey = "AdventureStartSceneViewed";

    public static int GetActiveCollabo()
    {
        for (int i = 0; i < ServerRepos.LoginCdn.GachaProducts.Count; ++i)
        {
            int gachaPId = ServerRepos.LoginCdn.GachaProducts[i];
            if (ServerContents.AdvGachaProducts.ContainsKey(gachaPId) == false)
                continue;
            var gachaProduct = ServerContents.AdvGachaProducts[gachaPId];

            if (gachaProduct.price == 0 || (gachaProduct.expired_at != 0 && Global.LeftTime(gachaProduct.expired_at) < 0))
                continue;

            if (gachaProduct.collabo != 0)
            {
                return gachaProduct.collabo;
            }
        }
        return 0;
    }

    public static bool GetActiveGachaSale()
    {
        for (var i = 0; i < ServerRepos.LoginCdn.GachaProducts.Count; i++)
        {
            var gachaPId = ServerRepos.LoginCdn.GachaProducts[i];
            if (ServerContents.AdvGachaProducts.ContainsKey(gachaPId) == false)
            {
                continue;
            }

            var gachaProduct = ServerContents.AdvGachaProducts[gachaPId];

            if (gachaProduct.price == 0 ||
                (gachaProduct.expired_at != 0 && Global.LeftTime(gachaProduct.expired_at) < 0))
            {
                continue;
            }

            if (ServerContents.AdvGachaProducts[gachaPId].sale == 1)
            {
                return true;
            }
        }

        return false;
    }
    
    //경험치 배율로 이벤트 열린지 확인하여 해당함수 사용하지 않음.

    public static int GetActiveEvent_ExpUpMagnification()
    {
        //서버에서 경험치 배율 받아오기
        if(ManagerAdventure.instance.ExpEvent)
        {
            return ServerRepos.LoginCdn.advExpEventRatio;
        }
        else
        {
            return 0;
        }
    }

    public static bool GetActiveEvent_MileageGachaEvent()
    {
        bool mileageGachaEvent = false;

        foreach (int gachaPId in ServerRepos.LoginCdn.GachaProducts)
        {
            CdnAdventureGachaProduct gachaProduct;
            if (!ServerContents.AdvGachaProducts.TryGetValue(gachaPId, out gachaProduct))
                continue;

            if (gachaProduct.expired_at != 0 && Global.LeftTime(gachaProduct.expired_at) < 0)
                continue;

            if (gachaProduct.mileage_flag != 0)
            {
                mileageGachaEvent = true;
                break;
            }
        }

        //Debug.Log("mileageGachaEvent : " + mileageGachaEvent);

        return mileageGachaEvent;
    }

    public static bool GetActiveEvent_GachaRateUp()
    {
        bool rateUpEvent = false;

        foreach (int gachaPId in ServerRepos.LoginCdn.GachaProducts)
        {
            CdnAdventureGachaProduct gachaProduct;
            if (!ServerContents.AdvGachaProducts.TryGetValue(gachaPId, out gachaProduct))
                continue;

            if (gachaProduct.expired_at != 0 && Global.LeftTime(gachaProduct.expired_at) < 0)
                continue;

            if (gachaProduct.rate_up != 0)
            {
                rateUpEvent = true;
                break;
            }
        }

        //Debug.Log("rateUpEvent : " + rateUpEvent);

        return rateUpEvent;
    }

    public static string GetGachaLanpageKey(CdnAdventureGachaProduct gacha)
    {
        if(gacha.info_page_key == null || gacha.info_page_key == string.Empty)
        {
            return GetGachaLanpageKey(gacha.gacha_id);
        }
        else
        {
            return gacha.info_page_key;
        }
    }

    public static string GetGachaLanpageKey(int gacha_id)
    {
        return $"LGPKV_gacha_{gacha_id}";
    }
}

