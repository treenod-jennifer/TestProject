using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Protocol;
using UnityEngine;

/// <summary>
/// 그룹랭킹 메인 팝업
/// </summary>
public class UIPopupGroupRanking : UIPopupBase
{
    public static UIPopupGroupRanking instance = null;

    private const string TITLE_TEXTURE_NAME  = "main_title";
    private const string REWARD_TEXTURE_NAME = "reward_item";
    private const string LAN_PAGE_URL        = "LGPKV_group_ranking";
    private const string LAN_PAGE_BUTTON_KEY = "p_grk_8";

    private readonly Color _grayColor = new Color(0.2862745f, 0.2862745f, 0.2862745f);

#region Inspector
    [SerializeField] private UITexture _textureMainTitle;

    // end time
    [SerializeField] private UILabel _labelEndTime;

    // start button
    [SerializeField] private UILabel[]    _labelStartButton;
    [SerializeField] private UISprite     _spriteStartButton;
    [SerializeField] private GameObject   _startButtonPlayIconObj;
    [SerializeField] private BoxCollider  _colliderStartButton;

    // lan
    [SerializeField] private UIItemLanpageButton _lanpageButton;

    // ranking reward
    [SerializeField] private Transform     _housingRewardRoot;
    [SerializeField] private GenericReward _rewardDeco;
    [SerializeField] private UITexture     _rewardItems;

    // score reward
    [SerializeField] private GenericReward _rewardScoreReward;
    [SerializeField] private UILabel       _labelScore;
    [SerializeField] private UIProgressBar _progressScore;
    [SerializeField] private UISprite      _spriteScoreRewardButton;
    [SerializeField] private BoxCollider   _colliderScoreRewardButton;
    [SerializeField] private GameObject    _checkObj;

    // my ranking
    [SerializeField] private UIItemGroupRanking _myRankItem;

    // group ranking
    [SerializeField] private UIReuseGridGroupRanking _reuseGrid;
    [SerializeField] private UIScrollView            _scrollView;

    [SerializeField] private List<UISprite> _sprites;
    [SerializeField] private List<UISprite> _spritesMakePixelPerfect;
#endregion

#region private
    private long                    _score;
    private long                    _scoreRewardMax;
    private Reward                  _scoreReward;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    
    private List<ManagerGroupRanking.GroupRankingUserData> _rankingData = new List<ManagerGroupRanking.GroupRankingUserData>();
#endregion

    private string LanPageButtonText  => Global._instance.GetString(LAN_PAGE_BUTTON_KEY);
    private string ScoreProgressText  => $"{Mathf.Clamp(_score, 0, _scoreRewardMax)}/{_scoreRewardMax}";
    private float  ScoreProgressValue => (float)_score / _scoreRewardMax;

    public  int  GetGroupRankingDataCount() => _rankingData.Count;
    private void SetRankingDecoRewards()    => _rewardDeco.SetReward(ServerContents.GroupRanking.GetPokoyuraReward());

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        instance = null;

        _cts.Cancel();
        _cts.Dispose();
        
        UnloadAssetBundleResources();
    }

    public void InitData()
    {
        _score          = ManagerGroupRanking.instance.serverData.myRankingUserData.score;
        _scoreRewardMax = ServerContents.GroupRanking.targetScore;
        _scoreReward    = ServerContents.GroupRanking.scoreReward;
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (Global.LoadFromInternal)
        {
            SetEditorAssetBundleResources();
        }
        else
        {
            SetAssetBundleResources();
        }
#else
        SetAssetBundleResources();
#endif

        _labelEndTime.text = Global.GetTimeText_MMDDHHMM_Plus1(ManagerGroupRanking.instance.GetEndTs());;

        // Ranking Rewards
        SetRankingDecoRewards();

        // Score Progress Bar
        _labelScore.text     = ScoreProgressText;
        _progressScore.value = ScoreProgressValue;

        // Score Reward
        _rewardScoreReward.SetReward(_scoreReward);

        // Score Reward Get Button
        SetActiveScoreRewardButton(_score >= _scoreRewardMax && !ServerRepos.UserGroupRanking.isScoreRewardReceived);

        SetMyRankItem();
        SetRankItems();

        // Start Button
        SetActiveStartButton(ManagerGroupRanking.IsDeadlineOver == false);

        _lanpageButton.On(LAN_PAGE_URL, LanPageButtonText);

        // Get Reward Popup Open
        GetRewardPopupOpen();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
        {
            return;
        }

        _scrollView.panel.useSortingOrder = true;
        _scrollView.panel.sortingOrder    = layer + 1;

        base.SettingSortOrder(layer);
    }

    public ManagerGroupRanking.GroupRankingUserData GetGroupRankingData(int index)
    {
        if (_rankingData == null)
        {
            return null;
        }

        if (_rankingData.Count <= index || index < 0)
        {
            return null;
        }

        return _rankingData[index];
    }

    /// <summary>
    /// 스크롤 뷰 상단 유저 랭킹 UI 설정
    /// </summary>
    private void SetMyRankItem()
    {
        var user = new ManagerGroupRanking.GroupRankingUserData(ManagerGroupRanking.instance.serverData.myRankingUserData);
        _myRankItem.UpdateData(user);
    }

    /// <summary>
    /// 스크롤 뷰 - 그룹 랭킹 유저 UI 리스트 설정
    /// </summary>
    private void SetRankItems()
    {
        _rankingData.Clear();

        var tempList        = new List<ManagerGroupRanking.GroupRankingUserData>();
        var isGetRankReward = ServerRepos.UserGroupRanking.isGetRankRewardReceived;
        
        foreach (var data in ManagerGroupRanking.instance.serverData.rankingUserDatas)
        {
            if (data.my)
            {
                var user = new ManagerGroupRanking.GroupRankingUserData(data, true, isGetRankReward);
                tempList.Add(user);
            }
            else
            {
                var user = new ManagerGroupRanking.GroupRankingUserData(data);
                tempList.Add(user);
            }
        }

        _rankingData = tempList.OrderBy(x => x.rank).ThenBy(x => x.name).ToList();

        if (ManagerGroupRanking.instance.serverData.CanGetRankingReward() || isGetRankReward)
        {
            var myRankIndex = _rankingData.FindIndex(0, x => x.isMyRank);

            _reuseGrid.gameObject.SetActive(true);
            _reuseGrid.InitReuseGrid(myRankIndex);
        }
        else
        {
            _reuseGrid.gameObject.SetActive(true);
            _reuseGrid.InitReuseGrid();
        }
    }

    /// <summary>
    /// 상태에 따라 점수 보상 획득 버튼 설정
    /// </summary>
    private void SetActiveScoreRewardButton(bool enable)
    {
        if (enable)
        {
            _spriteScoreRewardButton.spriteName = "diary_button_green";
            _colliderScoreRewardButton.enabled  = true;
        }
        else
        {
            _spriteScoreRewardButton.spriteName = "diary_button_gray";
            _colliderScoreRewardButton.enabled  = false;
        }
        
        _checkObj.SetActive(ServerRepos.UserGroupRanking.isScoreRewardReceived);
    }

    /// <summary>
    /// 상태에 따라 스타트 버튼 설정
    /// </summary>
    private void SetActiveStartButton(bool enable)
    {
        if (enable)
        {
            _labelStartButton.SetText(Global._instance.GetString("btn_3"));
            _spriteStartButton.spriteName = "button_play";
            _startButtonPlayIconObj.SetActive(true);
            _colliderStartButton.enabled = true;
        }
        else
        {
            _labelStartButton[0].transform.localPosition = Vector3.zero;
            _labelStartButton.SetText(Global._instance.GetString("btn_18"));
            _labelStartButton[1].color = _grayColor;
            _labelStartButton.SetEffectColor(_grayColor);

            _spriteStartButton.spriteName = "button_play03";
            _startButtonPlayIconObj.SetActive(false);
            _colliderStartButton.enabled = false;
        }
    }

    /// <summary>
    /// 보상 획득 팝업 오픈 (참가 보상, 랭킹 보상)
    /// </summary>
    private void GetRewardPopupOpen()
    {
        if (ManagerGroupRanking.instance.serverData.CanGetParticipationReward())
        {
            ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, ManagerGroupRanking.instance.serverData.participationReward);
            ManagerGroupRanking.instance.serverData.ApplyToRepos();
        }
        else if (ManagerGroupRanking.instance.serverData.CanGetRankingReward())
        {
            OpenGroupRankingGetRewardPopup(ManagerGroupRanking.instance.serverData.rankingReward);
            ManagerGroupRanking.instance.serverData.ApplyToRepos();
        }
        
        void OpenGroupRankingGetRewardPopup(AppliedRewardSet rewardSet)
        {
            ManagerUI._instance.OpenPopup<UIPopupGroupRankingGetReward>(OnComplete);

            void OnComplete(UIPopupGroupRankingGetReward popup)
            {
                var myData = ManagerGroupRanking.instance.serverData.myRankingUserData;
                popup.Init(myData.rank, rewardSet);
            }
        }
    }

    /// <summary>
    /// Start 버튼 클릭 후 레디 팝업 오픈 (통상 레디 or 비밀의 섬 레디)
    /// </summary>
    private async UniTask OpenReadyAndEndContentsEventPopup()
    {
        await UniTask.WaitForSeconds(0.2f, cancellationToken: _cts.Token);
        ManagerUI._instance.OpenReadyAndEndContentsEventPopup();
    }

    #region Button Event
    
    /// <summary>
    /// 점수 보상 획득 버튼 클릭
    /// </summary>
    public void OnClickScoreRewardGetButton()
    {
        if (bCanTouch == false)
        {
            return;
        }
        bCanTouch = false;

        ServerAPI.RequestGetGroupRankingScoreReward(ResponseGetScoreReward);
        
        void ResponseGetScoreReward(GroupRankingGetScoreRewardResp resp)
        {
            if (resp.IsSuccess)
            {
                ServerRepos.UserGroupRanking.isScoreRewardReceived = _textureMainTitle;
                SetActiveScoreRewardButton(false);
                
                ManagerUI._instance.OpenPopupGetRewardAlarm(Global._instance.GetString("n_s_46"), null, resp.reward);
                resp.ApplyToRepos();
                
                // growthy
                ManagerGroupRanking.instance.SendRewardLog(resp.reward, "ScoreReward");
            }

            bCanTouch = true;
        }
    }

    /// <summary>
    /// 점수 획득 방법 버튼 클릭
    /// </summary>
    public void OnClickScoreGuideButton()
    {
        if (bCanTouch == false)
        {
            return;
        }

        ManagerUI._instance.OpenPopup<UIPopupGroupRankingScoreGuide>();
    }

    /// <summary>
    /// 순위 별 보상 목록 버튼 클릭
    /// </summary>
    public void OnClickRankingRewardInfoButton()
    {
        if (bCanTouch == false)
        {
            return;
        }

        ManagerUI._instance.OpenPopup<UIPopupGroupRankingRewardInfo>();
    }

    /// <summary>
    /// Start 버튼 클릭
    /// </summary>
    public void OnClickStageStartButton()
    {
        if (bCanTouch == false)
        {
            return;
        }

        if (ManagerGroupRanking.IsDeadlineOver)
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false, null);
            });
            return;
        }

        bCanTouch = false;
        ManagerUI._instance.ClosePopUpUI(instance);
        
        OpenReadyAndEndContentsEventPopup().Forget();
    }
    
    /// <summary>
    /// 유저 프로필 수정 버튼 클릭
    /// </summary>
    public void OnClickEditProfileButton()
    {
        if (bCanTouch == false)
        {
            return;
        }

        ManagerGroupRanking.instance.OpenPionProfilePopup(false, OnUpdateMyProfile);
    }

    private void OnUpdateMyProfile()
    {
        var myProfile = SDKGameProfileManager._instance.GetMyProfile();
        
        ManagerGroupRanking.instance.serverData.myRankingUserData.name = myProfile.GameName;
        ManagerGroupRanking.instance.serverData.myRankingUserData.alterPicture = myProfile._alterPicture;

        foreach (var data in _rankingData)
        {
            if (data.isMyRank == true)
            {
                data.name = myProfile.GameName;
                data.alterPicture = myProfile._alterPicture;
                break;
            }
        }
        
        _reuseGrid.SetContent();
        SetMyRankItem();
    }

    protected override void OnClickBtnClose()
    {
        base.OnClickBtnClose();
        ManagerGroupRanking.instance.isTutorialEnd = true;
    }

    #endregion

    /// <summary>
    /// 에셋번들 리소스 설정
    /// </summary>
    private void SetAssetBundleResources()
    {
        ManagerAssetLoader._instance.BundleLoad<NGUIAtlas>(ManagerGroupRanking.ATLAS_ASSET_BUNDLE_NAME, ManagerGroupRanking.ATLAS_ASSET_NAME, atlas =>
        {
            foreach (var sprite in _sprites)
            {
                sprite.atlas = atlas;
            }

            foreach (var sprite in _spritesMakePixelPerfect)
            {
                var scale = sprite.transform.localScale;
                sprite.atlas = atlas;
                sprite.MakePixelPerfect();
                sprite.transform.localScale = scale;
            }
        });

        ManagerAssetLoader._instance.BundleLoad<Texture>(ManagerGroupRanking.GetTextureAssetBundleName(), TITLE_TEXTURE_NAME, texture =>
        {
            _textureMainTitle.mainTexture = texture;
            _textureMainTitle.MakePixelPerfect();
        });

        ManagerAssetLoader._instance.BundleLoad<Texture>(ManagerGroupRanking.GetTextureAssetBundleName(), REWARD_TEXTURE_NAME, texture =>
        {
            _rewardItems.mainTexture = texture;
            _rewardItems.MakePixelPerfect();
        });

        ManagerAssetLoader._instance.BundleLoad<GameObject>(ManagerGroupRanking.GetHousingAssetBundleName(), ManagerGroupRanking.GetHousingAssetName, obj =>
        {
            if (obj == null || obj.transform.Find("rootModel") == null)
            {
                Debug.LogError("Asset Not found: " + ManagerGroupRanking.GetHousingAssetBundleName() + "(not in assetDataList)");
                return;
            }

            var housing = _housingRewardRoot.AddChild(obj.transform.Find("rootModel").gameObject);
            housing.transform.localScale = new Vector3(7, 7, 1);
            housing.transform.rotation   = Quaternion.identity;
            housing.layer                = LayerMask.NameToLayer("UI");

            housing.GetComponent<MeshRenderer>().sortingOrder = _scrollView.panel.sortingOrder + 1;
        });
    }

    /// <summary>
    /// 에셋번들 리소스 해제
    /// </summary>
    private void UnloadAssetBundleResources()
    {
        foreach (var sprite in _sprites)
        {
            sprite.atlas = null;
        }

        foreach (var sprite in _spritesMakePixelPerfect)
        {
            sprite.atlas = null;
        }

        _textureMainTitle.mainTexture = null;
        _rewardItems.mainTexture      = null;
    }

#if UNITY_EDITOR
    private void SetEditorAssetBundleResources()
    {
        var atlas = ManagerGroupRanking.instance.LoadEditorAtlasAssetBundle();

        foreach (var sprite in _sprites)
        {
            sprite.atlas = atlas;
        }

        foreach (var sprite in _spritesMakePixelPerfect)
        {
            var scale = sprite.transform.localScale;
            sprite.atlas = atlas;
            sprite.MakePixelPerfect();
            sprite.transform.localScale = scale;
        }

        _textureMainTitle.mainTexture = ManagerGroupRanking.instance.LoadEditorTextureAssetBundle(TITLE_TEXTURE_NAME);
        _textureMainTitle.MakePixelPerfect();

        _rewardItems.mainTexture = ManagerGroupRanking.instance.LoadEditorTextureAssetBundle(REWARD_TEXTURE_NAME);
        _rewardItems.MakePixelPerfect();
    }
#endif
}