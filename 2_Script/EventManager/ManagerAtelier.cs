using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SideIcon;

public class ManagerAtelier : MonoBehaviour, IEventBase
{
    public enum PuzzleGameType
    {
        COLLECT,
        SCORE,
    }

    public static ManagerAtelier instance = null;

    //번들
    public AtelierPack       _atelierPack       = null;
    public AtelierInGamePack _atelierInGamePack = null;

    #region ContentsData
    public int            _vsn;
    public PuzzleGameType _mode;
    public int            _requireRewardCount;
    public List<int>      _puzzleList; //0 : 보너스, 1 : 스테이지 
    public List<Reward>   normalRewardList;
    public List<Reward>   completeRewardList;
    public int            _live2dIndex;
    public int            _paintIndex;
    #endregion


    #region UserData
    public int       _lastSelectIndex;
    public int       _currentPuzzleIndex;
    public int       _normalRewardState;   //0:보상 못받음 1:보상받을 수 있음 2:보상받았음
    public int       _completeRewardState; //0:보상 못받음 1:보상받을 수 있음 2:보상받았음
    public List<int> _puzzleState;
    #endregion

    private IconAtelier _icon;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnDestroy()
    {
        _atelierPack       = null;
        _atelierInGamePack = null;

        if (instance == this)
        {
            instance = null;
        }
    }

    #region SyncData
    private void SyncContentData()
    {
        _vsn                = ServerContents.Atelier.vsn;
        _mode               = (PuzzleGameType)ServerContents.Atelier.mode;
        _requireRewardCount = ServerContents.Atelier.requireRewardCount;
        _puzzleList         = ServerContents.Atelier.puzzleList;
        normalRewardList    = ServerContents.Atelier.normalRewardList;
        completeRewardList  = ServerContents.Atelier.completeRewardList;
        _live2dIndex        = ServerContents.Atelier.live2dIndex;
        _paintIndex         = ServerContents.Atelier.paintIndex;
    }

    public void SyncUserData()
    {
        _lastSelectIndex     = ServerRepos.UserAtelier.lastSelectIndex;
        _currentPuzzleIndex  = ServerRepos.UserAtelier.currentStageIndex;
        _normalRewardState   = ServerRepos.UserAtelier.normalRewardState;
        _completeRewardState = ServerRepos.UserAtelier.completeRewardState;
        _puzzleState         = ServerRepos.UserAtelier.puzzleState;
    }
    #endregion

    public static void Init()
    {
        if (CheckStartable() == false)
        {
            return;
        }

        if (instance != null)
        {
            return;
        }

        Global._instance.gameObject.AddMissingComponent<ManagerAtelier>();
        if (instance == null)
        {
            return;
        }

        ManagerEvent.instance.RegisterEvent(instance);
    }

    public static bool CheckStartable()
    {
        if (GameData.User.missionCnt < ManagerLobby._missionThreshold_firstDayOver)
        {
            return false;
        }

        if (ServerContents.Atelier == null || ServerRepos.UserAtelier == null)
        {
            return false;
        }

        if (Global.LeftTime(ServerContents.Atelier.endTs) < 0)
        {
            return false;
        }

        return true;
    }

    void IEventBase.OnReboot()
    {
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    GameEventType IEventBase.GetEventType() => GameEventType.ATELIER;

    /// <summary>
    ///개별 퍼즐 상태의 퍼즐 획득 여부 반환
    /// </summary>
    public bool IsPuzzleClear(int puzzleIdx, List<int> stages)
    {
        var clearValue = _puzzleList[puzzleIdx] == 0 ? 1 : _mode == PuzzleGameType.SCORE ? 3 : 1;
        return stages[puzzleIdx] >= clearValue;
    }

    /// <summary>
    /// 클리어 개수 반환
    /// </summary>
    public int GetClearCount(List<int> stages)
    {
        var clearCount = 0;
        for (var i = 0; i < _puzzleList.Count; i++)
        {
            if (IsPuzzleClear(i, stages))
            {
                clearCount++;
            }
        }

        return clearCount;
    }

    /// <summary>
    /// 퍼즐 인덱스로부터 스테이지 인덱스 구하기.
    /// 스테이지 시작 인덱스는 1
    /// puzzleList에서 스테이지인 퍼즐만 모두 더해서 선택한 퍼즐의 스테이지 번호 반환
    /// </summary>
    public int GetStageIndexFromPuzzle(int puzzleIndex)
    {
        var stageIndex = 0;

        if (_puzzleList[puzzleIndex] != 1)
        {
            return -1;
        }

        //선택한 인덱스보다 앞선 스테이지 개수 모두 더해서 실제 스테이지 인덱스 구함
        for (var i = 0; i <= puzzleIndex; i++)
        {
            if (_puzzleList[i] == 1)
            {
                stageIndex++;
            }
        }

        return stageIndex;
    }

    void IEventBase.OnLobbyStart()
    {
        if (instance != null && ServerContents.Atelier != null && ServerRepos.UserAtelier != null)
        {
            instance.SyncContentData();
        }
        else
        {
            _atelierPack = null;
        }

        _atelierInGamePack = null;
    }

    private CancellationTokenSource _cts;

    public async UniTask CoLoadAssetBundle<T>(bool isAllocate = true)
    {
        var inGame    = typeof(T) == typeof(AtelierInGamePack);
        var packName  = inGame ? "AtelierInGamePack" : "AtelierPack";
        var assetName = inGame ? $"atelier_ingame_{ServerContents.Atelier.resourceIndex}" : $"atelier_popup_{ServerContents.Atelier.resourceIndex}";

        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            var path      = $"Assets/5_OutResource/AtelierEvent/{assetName}/{packName}.prefab";
            var bundleObj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (isAllocate)
            {
                Allocate<T>(bundleObj);
            }
#endif
        }
        else
        {
            NetworkLoading.MakeNetworkLoading(0.5f);

            _cts = new CancellationTokenSource();
            var bundle    = await ManagerAssetLoader._instance.AsyncAssetBundleLoader(assetName, _cts.Token);
            var bundleObj = bundle.LoadAsset<GameObject>(packName);

            if (isAllocate)
            {
                Allocate<T>(bundleObj);
            }

            _cts.Cancel();
            _cts.Dispose();
            NetworkLoading.EndNetworkLoading();
        }

        void Allocate<T>(GameObject bundleObj)
        {
            if (bundleObj.GetComponent<T>() != null)
            {
                if (inGame)
                {
                    _atelierInGamePack = bundleObj.GetComponent<AtelierInGamePack>();
                }
                else
                {
                    _atelierPack = bundleObj.GetComponent<AtelierPack>();
                }
            }
        }
    }

    void IEventBase.OnIconPhase()
    {
        if (CheckStartable())
        {
            Maker.MakeIcon<IconAtelier>(
                ManagerUI._instance.ScrollbarRight,
                init: (icon) =>
                {
                    icon.Init(ServerContents.Atelier);
                    this._icon = icon;
                });
        }
    }

    public void OnEventIconClick(object obj = null)
    {
        if (CheckStartable())
        {
            _icon?.SetNewIcon(IsPlayCurrentVersion() == false);
            ManagerUI._instance.OpenPopupAtelier();
        }
        else
        {
            _icon?.SetNewIcon(false);
            ManagerUI._instance.OpenPopupEventOver();
        }
    }

    /// <summary>
    /// 스테이지형 이벤트 팝업 강제 노출
    /// </summary>
    public IEnumerator CoOpenForceDisplayEventPopup()
    {
        if (CheckStartable() == false)
        {
            yield break;
        }

        yield return ManagerUI._instance.CoOpenPopupAtelier();
    }

    /// <summary>
    /// 강제 노출 이벤트 리워드 갱신
    /// </summary>
    public void RefreshForceDisplayReward()
    {
        if (ManagerForceDisplayEvent.instance == null)
        {
            return;
        }

        if (ServerRepos.UserAtelier.normalRewardState == 2 && ServerRepos.UserAtelier.completeRewardState == 2)
        {
            ManagerForceDisplayEvent.instance.UpdateReward(ManagerForceDisplayEvent.ForceDisplayEventType.ATELIER);
        }
    }

    public bool IsPlayCurrentVersion()
    {
        if ((ServerRepos.UserAtelierStage                                                                == null ||
             ServerRepos.UserAtelierStage.FindAll(x => x.eventIndex == ServerContents.Atelier.vsn).Count == 0) &&
            ServerRepos.UserAtelier.puzzleState.FindAll(x => x == 0).Count == ServerRepos.UserAtelier.puzzleState.Count)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
        if (_atelierInGamePack == null)
        {
            CoLoadAssetBundle<AtelierInGamePack>(false).Forget();
        }
    }

    #region 미사용 코드
    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield break;
    }

    bool IEventBase.OnTutorialCheck()   => false;
    public bool     OnLobbySceneCheck() => false;

    IEnumerator IEventBase.OnLobbyObjectLoadPhase()
    {
        yield return null;
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
        yield return null;
    }

    IEnumerator IEventBase.OnRewardPhase()
    {
        yield return null;
    }
    #endregion
}