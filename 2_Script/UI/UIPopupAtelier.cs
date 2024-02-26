using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Protocol;
using Spine.Unity;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;

public class UIPopupAtelier : UIPopupBase
{
    public static UIPopupAtelier instance = null;

    #region UI
    [SerializeField] private UILabel _labelEndTs;
    [SerializeField] private UILabel _labelMode;

    [SerializeField] private UIUrlTexture              _texturePicture;
    [SerializeField] private List<UIItemAtelierPuzzle> _listUIItemAtelierPuzzles;

    [SerializeField] private UILabel                   _labelRewardCount;
    [SerializeField] private List<UIItemAtelierReward> _listUIItemAtelierNormalRewards;
    [SerializeField] private List<UIItemAtelierReward> _listUIItemAtelierCompleteRewards;
    [SerializeField] private GameObject                _paintingCheck;
    [SerializeField] private GameObject[]              _objNormalRewardBtn;
    [SerializeField] private GameObject[]              _objCompleteRewardBtn;
    [SerializeField] private UISprite                  _spritePictureReward;
    [SerializeField] private UISprite                  _spritePictureLight;
    [SerializeField] private GameObject                _objSpine;

    [SerializeField] private UIItemLanpageButton _lanPageBtn;
    [SerializeField] private UILabel             _labelContents;

    [SerializeField] public UIPokoButton _btnNormalReward;
    [SerializeField] public UIPokoButton _btnCompleteReward;
    [SerializeField] public UIPokoButton _btnCollection;

    [SerializeField] private List<GameObject> _objCompletePos;
    [SerializeField] private GameObject       _completeEffect;
    [SerializeField] private GameObject       _objCompleteLight;

    //번들 처리 sprite
    [SerializeField] private List<UISprite> _sprAtelierList;

    private CancellationTokenSource _initRewardTokenSource;
    private SkeletonAnimation       _spineBrush;
    #endregion


    #region Initialize
    private void Awake()
    {
        instance = this;

        //일반 리워드 버튼 클릭 시 획득
        _btnNormalReward.OnClickAsObservable()
            .Where(_ => ManagerAtelier.instance._normalRewardState == 1 && bCanTouch)
            .Subscribe(_ => OnClickNormalReward())
            .AddTo(gameObject);

        //최종 리워드 버튼 클릭 시 획득
        _btnCompleteReward.OnClickAsObservable()
            .Where(_ => ManagerAtelier.instance._completeRewardState == 1 && bCanTouch)
            .Subscribe(_ => OnClickCompleteReward())
            .AddTo(gameObject);

        //콜렉션 버튼 클릭 시 콜렉션 팝업 오픈
        _btnCollection.OnClickAsObservable()
            .Where(_ => bCanTouch)
            .Subscribe(_ => OpenPopupAtelierCollection())
            .AddTo(gameObject);

        //퍼즐 클릭
        foreach (var itemAtelierPuzzle in _listUIItemAtelierPuzzles)
        {
            itemAtelierPuzzle._btnPuzzle
                .OnClickAsObservable()
                .Where(_ => bCanTouch)
                .Subscribe(_ => OnClickPuzzle(itemAtelierPuzzle.PuzzleIndex))
                .AddTo(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            foreach (var spr in _sprAtelierList)
                spr.atlas = null;

            Destroy(_spineBrush);
            _spineBrush = null;

            instance = null;
            if (_initRewardTokenSource != null)
            {
                _initRewardTokenSource.Cancel();
                _initRewardTokenSource.Dispose();
            }
        }

        base.OnDestroy();
    }

    /// <summary>
    /// 스파인 초기화
    /// </summary>
    private void SetSpineObject()
    {
        var spineObj = ManagerAtelier.instance._atelierPack.ObjSpine ? ManagerAtelier.instance._atelierPack.ObjSpine : null;

        if (spineObj == null)
        {
            return;
        }

        _objSpine.gameObject.SetActive(false);
        _spineBrush = Instantiate(spineObj, _objSpine.transform).GetComponent<SkeletonAnimation>();
    }

    public override void SettingSortOrder(int layer)
    {
        SetSpineObject();

        if (layer < 10)
        {
            return;
        }

        uiPanel.useSortingOrder                               = true;
        uiPanel.sortingOrder                                  = layer;
        _spineBrush.GetComponent<MeshRenderer>().sortingOrder = layer + 1;

        base.SettingSortOrder(layer);
    }

    private void Start()
    {
        foreach (var sprite in _sprAtelierList)
        {
            sprite.atlas = ManagerAtelier.instance._atelierPack.AtlasUI;
        }

        foreach (var reward in _listUIItemAtelierNormalRewards)
        {
            reward.Init();
        }

        foreach (var reward in _listUIItemAtelierCompleteRewards)
        {
            reward.Init();
        }

        for (var i = 0; i < _listUIItemAtelierPuzzles.Count; i++)
        {
            _listUIItemAtelierPuzzles[i].Init(i);
        }
    }

    public override void ClosePopUp(float _mainTime = openTime, Method.FunctionVoid callback = null)
    {
        DOTween.ToAlpha(() => _texturePicture.color, x => _texturePicture.color = x, 0, 0.2f);
        base.ClosePopUp(_mainTime, callback);
    }
    #endregion


    #region UI Setting
    /// <summary>
    /// 고정된 Contents UI 초기화
    /// </summary>
    public void InitContentUI(bool stageClear = false)
    {
        //모드별 라벨
        if (ManagerAtelier.instance._mode == ManagerAtelier.PuzzleGameType.SCORE)
        {
            _labelMode.text     = Global._instance.GetString("p_atelier_8");
            _labelContents.text = Global._instance.GetString("p_atelier_4");
        }
        else
        {
            _labelMode.text     = Global._instance.GetString("p_atelier_9");
            _labelContents.text = Global._instance.GetString("p_atelier_5");
        }

        //텍스쳐
        _texturePicture.LoadCDN(Global.gameImageDirectory, "Gallery/", $"painting_{ManagerAtelier.instance._paintIndex}.png");
        _callbackOpen += ()=> DOTween.ToAlpha(() => _texturePicture.color, x => _texturePicture.color = x, 1f, 0.3f);

        //Lan 페이지 설정 랜페이지명 확인 필요
        _lanPageBtn.On($"LGPKV_event_PuzzlePainting", Global._instance.GetString("p_atelier_7"));

        //EndTs 설정
        _labelEndTs.text = Global.GetTimeText_MMDDHHMM_Plus1(ServerContents.Atelier.endTs);

        //리워드 초기화
        for (var i = 0; i < _listUIItemAtelierNormalRewards.Count; i++)
            _listUIItemAtelierNormalRewards[i].SetReward(ManagerAtelier.instance.normalRewardList[i]);

        for (var i = 0; i < _listUIItemAtelierCompleteRewards.Count; i++)
            _listUIItemAtelierCompleteRewards[i].SetReward(ManagerAtelier.instance.completeRewardList[i]);

        //UI 갱신
        InitReward(stageClear).Forget();
    }

    /// <summary>
    /// *팝업 시 리워드 갱신*
    /// 1.오픈, 플레이 종료, 일반/최종 보상 : 데이터 싱크 -&gt; 퍼즐 UI 갱신 / 리워드 UI 갱신                          null                   
    /// 2.클리어 : 데이터 싱크 전 -> 이전 퍼즐 UI 갱신 / 리워드 UI 갱신 -> 획득 연출 -> 데이터 싱크 -> 리워드 UI 갱신    stageClear
    /// 3.보너스 : 데이터 싱크 전 -> 이전 퍼즐 UI 상태 : 획득 연출 -> 데이터 싱크 -> 리워드 UI 갱신                    rewardResp != null .reward == null
    /// 4.스페셜 보너스 : 데이터 싱크 전 -> 이전 퍼즐 UI 상태 : 획득 연출 -> 데이터 싱크 -> 리워드 UI 갱신              reward.Resp.reward != null          
    /// </summary>
    private async UniTask InitReward(bool stageClear = false, AppliedRewardSet rewardResp = null)
    {
        bCanTouch = false;

        //1.일반 오픈, 일반/최종 보상 획득
        if (stageClear == false && rewardResp == null)
        {
            ManagerAtelier.instance.SyncUserData();
            InitRewardUI();
            InitPuzzleUI();
        }
        //2.스테이지 클리어
        else if (stageClear)
        {
            InitRewardUI();
            InitPuzzleUI();
            ManagerAtelier.instance.SyncUserData();
            await CoReceiveEffect(true);
            InitRewardUI();
        }
        //3.4.퍼즐 획득
        else
        {
            ManagerAtelier.instance.SyncUserData();
            await CoReceiveEffect(false, rewardResp);
            InitRewardUI();
        }
        
        ManagerUI._instance.UpdateUI();
        ManagerUI._instance.SyncTopUIAssets();

        bCanTouch = true;

        //리워드 갱신
        void InitRewardUI()
        {
            #region 퍼즐 획득 개수 표시
            var openedRewardCount = ManagerAtelier.instance.GetClearCount(ManagerAtelier.instance._puzzleState);
            var openedRewardValue = openedRewardCount < ManagerAtelier.instance._requireRewardCount ? openedRewardCount : ManagerAtelier.instance._requireRewardCount;

            _labelRewardCount.text = Global._instance.GetString("p_atelier_2").Replace("[0]", openedRewardValue.ToString());
            _labelRewardCount.text = _labelRewardCount.text.Replace("[n]", ManagerAtelier.instance._requireRewardCount.ToString());
            #endregion


            #region 일반 및 최종 보상 상태 UI 반영
            var receivedNormalReward   = ManagerAtelier.instance._normalRewardState   == 2;
            var receivedCompleteReward = ManagerAtelier.instance._completeRewardState == 2;

            foreach (var reward in _listUIItemAtelierNormalRewards)
            {
                reward.SetReceive(receivedNormalReward);
            }

            foreach (var reward in _listUIItemAtelierCompleteRewards)
            {
                reward.SetReceive(receivedCompleteReward);
            }

            if (receivedCompleteReward)
            {
                var color = new Color(0.6f, 0.6f, 0.6f);
                _spritePictureReward.color = color;

                var colorAlpha = new Color(1, 1, 1, 0.5f);
                _spritePictureLight.color = colorAlpha;
            }

            _paintingCheck.SetActive(receivedCompleteReward);
            SetButtonState(0, ManagerAtelier.instance._normalRewardState);
            SetButtonState(1, ManagerAtelier.instance._completeRewardState);
            #endregion
        }

        void InitPuzzleUI()
        {
            for (var i = 0; i < ManagerAtelier.instance._puzzleState.Count; i++)
            {
                var puzzleState = UIItemAtelierPuzzle.PuzzleState.COVERED;

                if (ManagerAtelier.instance.IsPuzzleClear(i, ManagerAtelier.instance._puzzleState))
                {
                    puzzleState = UIItemAtelierPuzzle.PuzzleState.CLEAR;
                }
                else if (ManagerAtelier.instance._currentPuzzleIndex == i)
                {
                    puzzleState = UIItemAtelierPuzzle.PuzzleState.SELECTED;
                }

                RefreshPuzzleUI(i, puzzleState, ServerRepos.UserAtelier.puzzleState[i]);
            }
        }

        //버튼 상태 UI 반영
        //rewardType - 0 : 일반 보상, 1 : 최종 보상
        //received - 획득 불가 : 0, 획득 가능 : 1, 획득 완료 : 2
        void SetButtonState(int rewardType = 0, int received = 0)
        {
            var objButtons = rewardType == 0 ? _objNormalRewardBtn : _objCompleteRewardBtn;

            objButtons[0].SetActive(received == 1);
            objButtons[1].SetActive(received == 0 || received == 2);
        }
    }

    /// <summary>
    /// 퍼즐 UI 갱신
    /// </summary>
    private void RefreshPuzzleUI(int puzzleIndex, UIItemAtelierPuzzle.PuzzleState puzzleState, int score, bool effect = false) =>
        _listUIItemAtelierPuzzles[puzzleIndex].SetIcon(ManagerAtelier.instance._mode, puzzleState, score, effect);
    #endregion


    #region Effect
    /// <summary>
    /// 퍼즐 조각 획득 연출
    /// </summary>
    private async UniTask CoReceiveEffect(bool isClear, AppliedRewardSet rewardResp = null)
    {
        _initRewardTokenSource = new CancellationTokenSource();
        var lastSelect = ManagerAtelier.instance._lastSelectIndex;
        var isAllClear = ManagerAtelier.instance.GetClearCount(ManagerAtelier.instance._puzzleState) == ManagerAtelier.instance._puzzleList.Count;

        #region 보상 팝업
        if (isClear == false)
        {
            var rewardPopup = OpenPopupAtelierReward(rewardResp);
            await UniTask.WaitWhile(() => rewardPopup != null, cancellationToken: _initRewardTokenSource.Token);
        }
        #endregion


        #region 브러시 스파인 연출
        _objSpine.transform.parent        = _listUIItemAtelierPuzzles[lastSelect].transform;
        _objSpine.transform.localPosition = Vector3.zero;
        _objSpine.gameObject.SetActive(true);
        _spineBrush.AnimationState.SetAnimation(0, "appear", false);
        #endregion


        #region 퍼즐 오픈 연출
        await UniTask.Delay(300, cancellationToken: _initRewardTokenSource.Token);

        var cover = ManagerAtelier.instance.IsPuzzleClear(lastSelect, ManagerAtelier.instance._puzzleState)
            ? UIItemAtelierPuzzle.PuzzleState.CLEAR
            : UIItemAtelierPuzzle.PuzzleState.COVERED;
        RefreshPuzzleUI(lastSelect, cover, ManagerAtelier.instance._puzzleState[lastSelect], true);

        await UniTask.Delay(1000, cancellationToken: _initRewardTokenSource.Token);
        #endregion


        #region 퍼즐 올 클리어 연출 / 최종 보상 안내 팝업
        if (isAllClear)
        {
            await CoAllClearEffect();

            var popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_at_1"), false);
            popup.SortOrderSetting();

            await UniTask.WaitWhile(() => popup != null, cancellationToken: _initRewardTokenSource.Token);

            await UniTask.Delay(1000, cancellationToken: _initRewardTokenSource.Token);
        }
        #endregion


        //폭죽 및 라이트 on
        async UniTask CoAllClearEffect()
        {
            foreach (var completePos in _objCompletePos)
            {
                var obj = mainSprite.gameObject.AddChild(_completeEffect);
                obj.transform.localPosition = completePos.transform.localPosition;
                obj.transform.localScale    = new Vector3(0.1f, 0.1f, 0.1f);

                var children = obj.GetComponentsInChildren<ParticleSystemRenderer>();
                foreach (var child in children)
                {
                    child.sortingLayerName = gameObject.layer.ToString();
                    child.sortingOrder     = uiPanel.sortingOrder + 2;
                }

                await UniTask.Delay(300, cancellationToken: _initRewardTokenSource.Token);
            }

            _objCompleteLight.gameObject.SetActive(true);

            await UniTask.Delay(1000, cancellationToken: _initRewardTokenSource.Token);
        }
    }
    #endregion


    #region Popup
    /// <summary>
    /// 퍼즐 획득 시 퍼즐 명화 전용 팝업
    /// </summary>
    private UIPopupBase OpenPopupAtelierReward(AppliedRewardSet rewardSet)
    {
        var popup = ManagerUI._instance.OpenPopup<UIPopupAtelierReward>();

        var rewards = new List<Reward>();
        if (rewardSet != null)
        {
            if (rewardSet.directApplied != null)
            {
                rewards.AddRange(rewardSet.directApplied.Values.Select(reward => new Reward { type = reward.type, value = reward.valueDelta }));
            }

            if (rewardSet.mailReceived != null)
            {
                rewards.AddRange(rewardSet.mailReceived);
            }
        }

        popup.Init(rewards);

        return popup;
    }

    /// <summary>
    /// 유저 버튼 클릭 시 보상 팝업 : 일반, 최종 보상
    /// </summary>
    private void OpenRewardPopup(AppliedRewardSet reward)
    {
        if (reward == null)
        {
            bCanTouch                       = true;
            ManagerUI._instance.bTouchTopUI = true;
            return;
        }

        //리워드 팝업
        ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), () =>
        {
            InitReward().Forget();
            bCanTouch                       = true;
            ManagerUI._instance.bTouchTopUI = true;
        }, reward);
    }

    /// <summary>
    /// 갤러리 팝업
    /// </summary>
    private void OpenPopupAtelierCollection()
    {
        bCanTouch = false;
        ManagerUI._instance.OpenPopupGallery(_callbackOpen = () => bCanTouch = true);
    }
    #endregion


    #region RecvFunction
    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// 퍼즐 클릭
    /// </summary>
    private void OnClickPuzzle(int puzzleIndex)
    {
        bCanTouch = false;

        if (ManagerAtelier.CheckStartable() == false)
        {
            ManagerUI._instance.OpenPopupEventOver();
            bCanTouch = true;
            return;
        }

        //이미 클릭된 퍼즐이 있을 경우 안내 팝업
        if (_listUIItemAtelierPuzzles[puzzleIndex].IsCanSelect() == false)
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_at_2"), false, null);
                bCanTouch = true;
            });
            return;
        }

        //이미 선택된 스테이지라면 서버 통신 없이 레디창 오픈
        if (ManagerAtelier.instance._currentPuzzleIndex == puzzleIndex)
        {
            ManagerUI._instance.OpenPopupReadyAtelier(ManagerAtelier.instance.GetStageIndexFromPuzzle(puzzleIndex)
                , () => bCanTouch = true);
            return;
        }

        //선택
        ServerAPI.AtelierPuzzleSelect(puzzleIndex,
            (resp) =>
            {
                if (resp.IsSuccess)
                {
                    //레디창 오픈
                    if (ManagerAtelier.instance._puzzleList[puzzleIndex] == 1)
                    {
                        //선택 UI로 갱신
                        RefreshPuzzleUI(puzzleIndex, UIItemAtelierPuzzle.PuzzleState.SELECTED, ManagerAtelier.instance._puzzleState[puzzleIndex]);

                        ManagerAtelier.instance.SyncUserData();
                        ManagerUI._instance.OpenPopupReadyAtelier(ManagerAtelier.instance.GetStageIndexFromPuzzle(puzzleIndex)
                            , () => bCanTouch = true);
                    }
                    //보너스 퍼즐
                    else
                    {
                        //선택 UI로 갱신
                        RefreshPuzzleUI(puzzleIndex, UIItemAtelierPuzzle.PuzzleState.SELECTED, 3);

                        InitReward(rewardResp: resp.reward).Forget();
                        RewardGrowthy(resp.reward, 0);
                    }
                }
            });
    }

    /// <summary>
    /// 일반 보상 획득
    /// </summary>
    private void OnClickNormalReward()
    {
        bCanTouch = false;
        //선택
        ServerAPI.AtelierNormalReward((resp) =>
        {
            if (resp.IsSuccess)
            {
                ManagerAtelier.instance.RefreshForceDisplayReward();
                OpenRewardPopup(resp.reward);
                InitReward().Forget();
                RewardGrowthy(resp.reward, 1);
            }
        });
    }

    /// <summary>
    /// 최종 보상 획득 클릭
    /// </summary>
    private void OnClickCompleteReward()
    {
        bCanTouch = false;
        //선택
        ServerAPI.AtelierCompleteReward((resp) =>
            {
                if (resp.IsSuccess)
                {
                    ManagerAtelier.instance.RefreshForceDisplayReward();
                    OpenRewardPopup(resp.reward);
                    InitReward().Forget();
                    RewardGrowthy(resp.reward, 2);
                }
            }
        );
    }

    /// <summary>
    /// 보상 획득 그로시
    /// </summary>
    /// <param name="rewardType"> 0: 퍼즐 획득, 1: 일반 보상, 2: 최종 보상</param>
    public static void RewardGrowthy(AppliedRewardSet resp, int rewardType, bool stageClear = false)
    {
        #region Achievement 로그
        //퍼즐 획득 시 - 스테이지, 일반 보너스, 스페셜 보너스 구분
        if (rewardType == 0)
        {
            string dtl = null;
            if (stageClear)
            {
                dtl = $"ATELIER_{ManagerAtelier.instance._vsn}_STAGE_{ServerRepos.UserAtelier.lastSelectIndex}";
            }
            else if (resp == null || ((resp.directApplied   == null || resp.directApplied.Count == 0)
                                      && (resp.mailReceived == null || resp.mailReceived.Length == 0)))
            {
                dtl = $"ATELIER_{ManagerAtelier.instance._vsn}_BONUS_{ServerRepos.UserAtelier.lastSelectIndex}";
            }
            else if (resp != null && (resp.mailReceived != null || resp.directApplied != null))
            {
                dtl = $"ATELIER_{ManagerAtelier.instance._vsn}_SPECIAL_BONUS_{ServerRepos.UserAtelier.lastSelectIndex}";
            }

            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.ATELIER_PUZZLE_REWARD,
                dtl,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
        #endregion


        #region 아이템/재화 로그
        var rewardDtl = rewardType switch
        {
            0 => $"ATELIER_{ManagerAtelier.instance._vsn}_PUZZLE_{ServerRepos.UserAtelier.lastSelectIndex}",
            1 => $"ATELIER_{ManagerAtelier.instance._vsn}_NORMAL",
            2 => $"ATELIER_{ManagerAtelier.instance._vsn}_COMPLETE",
            _ => null
        };

        if (resp != null)
        {
            if (resp.directApplied != null)
            {
                foreach (var reward in resp.directApplied.Values)
                {
                    // 구매한 아이템 기록
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        reward.type,
                        reward.valueDelta,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ATELIER_REWARD,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ATELIER_REWARD,
                        rewardDtl
                    );
                }
            }

            if (resp.mailReceived != null)
            {
                foreach (var reward in resp.mailReceived)
                {
                    // 구매한 아이템 기록
                    ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                        reward.type,
                        reward.value,
                        ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_ATELIER_REWARD,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_ATELIER_REWARD,
                        rewardDtl
                    );
                }
            }
        }
        #endregion
    }
    #endregion
}