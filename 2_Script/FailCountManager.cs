using System.Collections.Generic;
using UnityEngine;

public partial class FailCountManager : MonoBehaviour
{
    // NPU 컨티뉴 광고 Fail 키
    private const string NPU_AD_FAIL_DATA_KEY = "FailData_NPU_AD";
    private const string NPU_AD_FAIL_DATA_PACKAGE_KEY = "FailData_PackgeIdx_NPU_AD";
    
    // NPU 스팟 패키지 Fail 키
    private const string NPU_SPOT_FAIL_DATA_KEY = "FailData_NPU_Spot";
    private const string NPU_SPOT_FAIL_DATA_PACKAGE_KEY = "FailData_PackgeIdx_NPU_Spot";
    private const string NPU_SPOT_CHECK_ISOPEN_KEY = "FailData_Version_NPU_Spot";

    public enum FailCountType
    {
        NONE,
        NPU_AD_CONTINUE = 2,          // NPU 컨티뉴 광고지면
        SPOT_NPU_PACKAGE = 3,         // 스팟 NonePU 패키지
    }

    private static FailCountManager instance;
    public static FailCountManager _instance
    {
        get
        {
            if(instance == null)
            {
                var managerObject = new GameObject();
                managerObject.name = "FailCountManager";
                instance = managerObject.AddComponent<FailCountManager>();
                instance.DataLoad_All();
            }

            return instance;
        }
    }
    
    private void OnDestroy()
    {
        Save(FailCountType.NPU_AD_CONTINUE);
        Save(FailCountType.SPOT_NPU_PACKAGE);

        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// 해당 메니저의 동작 여부
    /// 조건에 따라 스테이지 실패 횟수를 카운팅 하지 않는 경우가 있다.
    /// </summary>
    private bool IsActive (GameType gameType, FailCountType failCountType)
    {
        if (failCountType == FailCountType.NPU_AD_CONTINUE)
        {
            if (ServerContents.NpuAdContinueInfo == null)
            {
                return false;
            }

            if (GetFailPackageVer(failCountType) == 0)
            {
                return false;
            }

            if (!AdManager.ADCheck(AdManager.AdType.AD_21))
            {
                return false;
            }

            return gameType == GameType.NORMAL;
        }

        if (failCountType == FailCountType.SPOT_NPU_PACKAGE)
        {
            if (ServerContents.NpuSpotPackage == null)
            {
                return false;
            }

            if (GetFailPackageVer(failCountType) == 0)
            {
                return false;
            }

            return gameType == GameType.NORMAL;
        }

        return false;
    }

    /// <summary>
    /// 스테이지의 고유 키값을 만들어 낸다.
    /// 해당 키값이 몇개 저장 되어 있는지 여부로 같은 스테이지의 실패 횟수를 카운팅 한다.
    /// </summary>
    private string GetStageKey(GameType gameType, int eventIndex, int chapter, int stage) => $"{gameType}_{eventIndex}_{chapter}_{stage}";

    /// <summary>
    /// 스테이지의 실패 카운트를 1 더한다.
    /// </summary>
    public void SetFail(GameType gameType, int eventIndex, int chapter, int stage)
    {
        var failCountType = new List<FailCountType>();

        if (IsActive(gameType, FailCountType.NPU_AD_CONTINUE))
        {
            failCountType.Add(FailCountType.NPU_AD_CONTINUE);
        }

        if (IsActive(gameType, FailCountType.SPOT_NPU_PACKAGE))
        {
            failCountType.Add(FailCountType.SPOT_NPU_PACKAGE);
        }

        foreach (var countType in failCountType)
        {
            if (!PlayerPrefs.HasKey(GetPackageDataKey(countType)))
            {
                DataReset(countType);
                PlayerPrefs.SetInt(GetPackageDataKey(countType), GetFailPackageVer(countType));
            }
            else
            {
                if (PlayerPrefs.GetInt(GetPackageDataKey(countType)) != GetFailPackageVer(countType))
                {
                    DataReset(countType);
                    PlayerPrefs.SetInt(GetPackageDataKey(countType), GetFailPackageVer(countType));
                }
            }

            if (countType == FailCountType.NPU_AD_CONTINUE)
            {
                failData_NPUAD.Add(GetStageKey(gameType, eventIndex, chapter, stage));
            }

            if (countType == FailCountType.SPOT_NPU_PACKAGE)
            {
                failData_NPUSpot.Add(GetStageKey(gameType, eventIndex, chapter, stage));
            }
        }

    }

    /// <summary>
    /// 스테이지의 over실패 여부
    /// </summary>
    /// <returns>실패 카운트가 기준(FAIL_OVER) 이상이면 true 아니면 false</returns>
    public bool FailCheck(GameType gameType, int eventIndex, int chapter, int stage, FailCountType failCountType)
    {
        if (!IsActive(gameType, failCountType))
        {
            return false;
        }
       
        var stageKey = GetStageKey(gameType, eventIndex, chapter, stage);
        if (failCountType == FailCountType.NPU_AD_CONTINUE)
        {
            return failData_NPUAD.ContainsStage(stageKey) + 1 >= ServerContents.NpuAdContinueInfo.fail_count;
        }

        if (failCountType == FailCountType.SPOT_NPU_PACKAGE)
        {
            return failData_NPUSpot.ContainsStage(stageKey) + 1 >= ServerContents.NpuSpotPackage.fail_count;
        }

        return false;
    }

    /// <summary>
    /// PlayerPrefs 키를 체크하여 패키지/광고 버전이 변경되었는지 리턴
    /// </summary>
    private bool CheckPackgeChanged(FailCountType failCountType, int packageIdx = -1)
    {
        if (packageIdx == -1)
        {
            packageIdx = GetFailPackageVer(failCountType);
        }

        var failDataKey = GetPackageDataKey(failCountType);
        
        if (!PlayerPrefs.HasKey(failDataKey))
        {
            PlayerPrefs.SetInt(failDataKey, packageIdx);
            return false;
        }

        if (packageIdx != PlayerPrefs.GetInt(failDataKey))
        {
            PlayerPrefs.SetInt(failDataKey, packageIdx);
            DataReset(failCountType);
            return true;
        }

        return false;
    }

    #region FailData Prefs Key 관리 Func

    private void Save(FailCountType failCountType)
    {
        if (failCountType == FailCountType.NPU_AD_CONTINUE)
        {
            PlayerPrefs.SetString(GetDataKey(failCountType), JsonUtility.ToJson(failData_NPUAD));
        }

        if (failCountType == FailCountType.SPOT_NPU_PACKAGE)
        {
            PlayerPrefs.SetString(GetDataKey(failCountType), JsonUtility.ToJson(failData_NPUSpot));
        }

        PlayerPrefs.Save();
    }
    private void Load(FailCountType failCountType)
    {
        _instance.CheckPackgeChanged(failCountType);

        if (PlayerPrefs.HasKey(GetDataKey(failCountType)))
        {
            var jsonData = PlayerPrefs.GetString(GetDataKey(failCountType));
            if (failCountType == FailCountType.NPU_AD_CONTINUE)
            {
                failData_NPUAD = JsonUtility.FromJson<FailCountData>(jsonData);
            }
            if (failCountType == FailCountType.SPOT_NPU_PACKAGE)
            {
                failData_NPUSpot = JsonUtility.FromJson<FailCountData>(jsonData);
            }
        }
        else
        {
            if (failCountType == FailCountType.NPU_AD_CONTINUE)
            {
                failData_NPUAD = new FailCountData();
            }

            if (failCountType == FailCountType.SPOT_NPU_PACKAGE)
            {
                failData_NPUSpot = new FailCountData();
            }
        }
    }
    private void DataReset(FailCountType failCountType)
    {
        if (PlayerPrefs.HasKey(GetDataKey(failCountType)))
        {
            PlayerPrefs.DeleteKey(GetDataKey(failCountType));
        }

        Load(failCountType);

        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 스팟다이아, NPU 광고 컨티뉴, NPU 스팟 패키지 중 하나라도 오픈 되면 모든 FailData 초기화
    /// </summary>
    public void ClearData()
    {
        failData_NPUAD.Clear();
        failData_NPUSpot.Clear();
        
        Save(FailCountType.NPU_AD_CONTINUE);
        Save(FailCountType.SPOT_NPU_PACKAGE);
        PlayerPrefs.Save();
    }

    private void DataLoad_All()
    {
        Load(FailCountType.NPU_AD_CONTINUE);
        Load(FailCountType.SPOT_NPU_PACKAGE);
    }
    
    #endregion

    #region Key, Version 리턴 Func

    private string GetDataKey(FailCountType failCountType)
    {
        if (failCountType == FailCountType.NPU_AD_CONTINUE)
        {
            return NPU_AD_FAIL_DATA_KEY;
        }
        if (failCountType == FailCountType.SPOT_NPU_PACKAGE)
        {
            return NPU_SPOT_FAIL_DATA_KEY;
        }

        return string.Empty;
    }

    private string GetPackageDataKey(FailCountType failCountType)
    {
        if (failCountType == FailCountType.NPU_AD_CONTINUE)
        {
            return NPU_AD_FAIL_DATA_PACKAGE_KEY;
        }

        if (failCountType == FailCountType.SPOT_NPU_PACKAGE)
        {
            return NPU_SPOT_FAIL_DATA_PACKAGE_KEY;
        }

        return string.Empty;
    }

    private int GetFailPackageVer(FailCountType failCountType)
    {
        if (failCountType == FailCountType.NPU_AD_CONTINUE && ServerContents.NpuAdContinueInfo != null)
        {
            return ServerContents.NpuAdContinueInfo.ver;
        }

        if (failCountType == FailCountType.SPOT_NPU_PACKAGE && ServerContents.NpuSpotPackage != null)
        {
            return ServerContents.NpuSpotPackage.ver;
        }

        return 0;
    }

    #endregion
    
    public static bool CheckNpuAdContinue()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return false;
        }

        if (ServerContents.NpuAdContinueInfo.ver != 0)
        {
            _instance.CheckPackgeChanged(FailCountType.NPU_AD_CONTINUE);
        }

        if (Global.GameType != GameType.NORMAL)
        {
            return false;
        }

        return AdManager.ADCheck(AdManager.AdType.AD_21);
    }
    
    public static (bool, CdnShopPackage) CheckNpuSpotPackage()
    {
        if (GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
        {
            return (false, null);
        }

        if (ServerContents.NpuSpotPackage.ver != 0)
        {
            _instance.CheckPackgeChanged(FailCountType.SPOT_NPU_PACKAGE);
        }

        if (Global.GameType != GameType.NORMAL)
        {
            return (false, null);
        }

        foreach (var item in ServerContents.Packages.Values)
        {
            if (item == null)
            {
                continue;
            }

            if (item.type != 10)
            {
                continue;
            }

            var alreadyBuy = ServerRepos.UserShopPackages.Exists(x => { return x.idx == item.idx; });
            var alreadyShow = PlayerPrefs.GetInt(NPU_SPOT_CHECK_ISOPEN_KEY) == ServerContents.NpuSpotPackage.ver;

            // 이번 버전에서 한번이라도 출력되거나 구매된 적 있으면 인게임에서 다시 출력 X
            if (!alreadyBuy && !alreadyShow)
            {
                ManagerUI._instance.OpenPopup<UIPopupNewPackage>((popup) => popup.InitPopup(item));
                ServerAPI.SaveNPUSpotPackageShowFlag();
                PlayerPrefs.SetInt(NPU_SPOT_CHECK_ISOPEN_KEY, ServerContents.NpuSpotPackage.ver);
                
                var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.NPU_SPOT_PACKAGE,
                    "NPU_SPOT_PACKAGE_OPEN",
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS,
                    ServerContents.NpuSpotPackage.ver,
                    Global.stageIndex.ToString()
                );
                var doc = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
                _instance.ClearData();

                return (true, item);
            }
        }

        return (false, null);
    }
}
