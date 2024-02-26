using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Protocol;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

/// <summary>
/// 그룹랭킹 보상 획득 팝업
/// </summary>
public class UIPopupGroupRankingGetReward : UIPopupBase
{
    private enum RewardSpineState
    {
        NONE,
        APPEAR,
        END,
        ITEM,
        OPEN,
        SHAKE,
    }

    [SerializeField] private TextMeshGlobalString _title;
    [SerializeField] private UISprite             _spriteTouch;
    [SerializeField] private BoxCollider          _collider;
    [SerializeField] private GameObject           _rootBox;
    [SerializeField] private GameObject           _beforeObj;
    [SerializeField] private GameObject           _afterObj;
    [SerializeField] private GameObject           _rewardItem;
    [SerializeField] private UIPokoGrid           _grid;

    private long                    _rank;
    private int                     _skinIndex;
    private int                     _showRewardIndex;
    private bool                    _isOpen;
    private AppliedRewardSet        _rewardSet;
    private SkeletonAnimation       _skeletonAnimation;
    private List<GenericReward>     _genericRewards = new List<GenericReward>();
    private CancellationTokenSource _cts            = new CancellationTokenSource();

    private void OnOpenPopup() => AsyncOpenPopup(_cts.Token).Forget();

    public void Init(long rank, AppliedRewardSet rewardSet)
    {
        _rank      = rank;
        _rewardSet = rewardSet;

        _skinIndex = rank switch
        {
            1 => 1,
            2 => 2,
            3 => 3,
            _ => 4
        };

        _callbackOpen += OnOpenPopup;
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

        var titleText = Global._instance.GetString("grk_rw_1").Replace("[n]", _rank.ToString());
        _title.SetText(titleText);

        SetReward();
    }

    protected override void OnDestroy()
    {
        _spriteTouch.atlas = null;

        _skeletonAnimation.state.Event -= AnimationEventListener;
        Destroy(_skeletonAnimation.gameObject);

        _cts.Cancel();
        _cts.Dispose();

        ManagerSound.StopEffectSound();

        base.OnDestroy();
    }

    public void OnClickBox()
    {
        if (_skeletonAnimation == null || _isOpen)
        {
            return;
        }

        _isOpen           = true;
        _collider.enabled = false;
        _beforeObj.SetActive(false);
        _afterObj.SetActive(true);

        foreach (var reward in _genericRewards)
        {
            reward.gameObject.SetActive(false);
        }

        StartOpenAnimation();
    }

    private async UniTask AsyncOpenPopup(CancellationToken ct)
    {
        await UniTask.WaitForSeconds(0.2f, cancellationToken: ct); // 상자 등장 연출이 눈에 잘 보일 수 있도록 딜레이.
        await UniTask.WaitUntil(() => _skeletonAnimation != null, cancellationToken: ct);

        StartAppearAnimation();
    }

    private void SetReward()
    {
        //즉시지급 보상 UI 설정
        if (_rewardSet.directApplied != null)
        {
            foreach (var item in _rewardSet.directApplied)
            {
                var genericReward = _grid.transform.AddChild(_rewardItem).GetComponent<GenericReward>();
                if (genericReward != null)
                {
                    var reward = new Reward { type = (int)item.Key, value = (int)item.Value.valueDelta };
                    genericReward.SetReward(reward);
                    _genericRewards.Add(genericReward);
                }
            }
        }

        //메일로 들어가는 보상 UI 설정
        if (_rewardSet.mailReceived != null)
        {
            for (var i = 0; i < _rewardSet.mailReceived.Length; i++)
            {
                var genericReward = _grid.transform.AddChild(_rewardItem).GetComponent<GenericReward>();
                if (genericReward != null)
                {
                    genericReward.SetReward(_rewardSet.mailReceived[i]);
                    _genericRewards.Add(genericReward);
                }
            }
        }
    }

    private void StartAppearAnimation()
    {
        if (_skeletonAnimation == null)
        {
            return;
        }

        // skin 설정.
        _skeletonAnimation.initialSkinName = _skinIndex.ToString();
        _skeletonAnimation.Initialize(true);

        _skeletonAnimation.state.Start    += AnimationStartListener;
        _skeletonAnimation.state.Complete += AnimationCompleteListener;
        _skeletonAnimation.state.Event    += AnimationEventListener;

        _skeletonAnimation.state.SetAnimation(0, GetAnimName(RewardSpineState.APPEAR), false);
        _skeletonAnimation.state.AddAnimation(0, GetAnimName(RewardSpineState.SHAKE), true, 0f);
    }

    private void StartOpenAnimation()
    {
        if (_skeletonAnimation == null)
        {
            return;
        }

        bCanTouch = false;
        ManagerSound.StopEffectSound();

        // 모든 track 제거.
        _skeletonAnimation.state.ClearTracks();

        _skeletonAnimation.state.SetAnimation(0, GetAnimName(RewardSpineState.OPEN), false);
        for (var i = 0; i < _genericRewards.Count; i++)
        {
            _skeletonAnimation.state.AddAnimation(0, GetAnimName(RewardSpineState.ITEM), false, 0f);
        }

        _skeletonAnimation.state.AddAnimation(0, GetAnimName(RewardSpineState.END), true, 0f);
    }

    private void AnimationStartListener(TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == GetAnimName(RewardSpineState.END))
        {
            bCanTouch = true;

            _skeletonAnimation.state.Start -= AnimationStartListener;
        }
    }

    private void AnimationCompleteListener(TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == GetAnimName(RewardSpineState.APPEAR))
        {
            _spriteTouch.gameObject.SetActive(true);
            _collider.enabled = true;

            _skeletonAnimation.state.Complete -= AnimationCompleteListener;
        }
    }

    private void AnimationEventListener(TrackEntry trackEntry, Event e)
    {
        if (e.Data.Name != "sound")
        {
            return;
        }

        if (trackEntry.Animation.Name == GetAnimName(RewardSpineState.ITEM))
        {
            if (_showRewardIndex < _genericRewards.Count)
            {
                _genericRewards[_showRewardIndex].gameObject.SetActive(true);
                _showRewardIndex++;
            }
        }

        AudioPlay(trackEntry.Animation.Name);
    }

    private void AudioPlay(string animName)
    {
        switch (animName)
        {
            case "box_appear":
                ManagerSound.AudioPlay(AudioLobby.group_ranking_reward_box_appear);
                break;
            case "box_shake":
                ManagerSound.AudioPlay(AudioLobby.group_ranking_reward_box_shake);
                break;
            case "box_open":
                ManagerSound.AudioPlay(AudioLobby.group_ranking_reward_box_open);
                break;
            case "box_item":
                ManagerSound.AudioPlay(AudioLobby.group_ranking_reward_box_item);
                break;
            case "box_end":
                ManagerSound.AudioPlay(AudioLobby.group_ranking_reward_box_end, 0.8f);
                break;
        }
    }

    private string GetAnimName(RewardSpineState state) => state switch
    {
        RewardSpineState.SHAKE  => "box_shake",
        RewardSpineState.APPEAR => "box_appear",
        RewardSpineState.OPEN   => "box_open",
        RewardSpineState.ITEM   => "box_item",
        RewardSpineState.END    => "box_end",
        _                       => "box_end",
    };

    private void SetAssetBundleResources()
    {
        ManagerAssetLoader._instance.BundleLoad<NGUIAtlas>(ManagerGroupRanking.ATLAS_ASSET_BUNDLE_NAME, ManagerGroupRanking.ATLAS_ASSET_NAME, atlas => { _spriteTouch.atlas = atlas; });

        ManagerAssetLoader._instance.BundleLoad<GameObject>(ManagerGroupRanking.SPINE_ASSET_BUNDLE_NAME, ManagerGroupRanking.SPINE_ASSET_NAME, obj =>
        {
            if (obj == null)
            {
                Debug.LogError("Asset Not found: " + ManagerGroupRanking.SPINE_ASSET_BUNDLE_NAME + "(not in assetDataList)");
                return;
            }

            var spineObj = _rootBox.AddChild(obj);
            spineObj.layer = LayerMask.NameToLayer("UI");
            spineObj.GetComponentInChildren<MeshRenderer>(true).sortingOrder = uiPanel.sortingOrder + 1;

            _skeletonAnimation = spineObj.GetComponentInChildren<SkeletonAnimation>(true);
        });
    }

#if UNITY_EDITOR
    private void SetEditorAssetBundleResources()
    {
        var atlas = ManagerGroupRanking.instance.LoadEditorAtlasAssetBundle();
        if (atlas != null)
        {
            _spriteTouch.atlas = atlas;
            _spriteTouch.MakePixelPerfect();
        }
        
        var obj = ManagerGroupRanking.instance.LoadEditorSpineAssetBundle();
        if (obj != null)
        {
            var spineObj = _rootBox.AddChild(obj);
            spineObj.layer = LayerMask.NameToLayer("UI");
            spineObj.GetComponentInChildren<MeshRenderer>(true).sortingOrder = uiPanel.sortingOrder + 1;

            _skeletonAnimation = spineObj.GetComponentInChildren<SkeletonAnimation>(true);
        }
    }
#endif
}