using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 그룹랭킹 순위 별 보상 목록 팝업
/// </summary>
public class UIPopupGroupRankingRewardInfo : UIPopupBase
{
    private static UIPopupGroupRankingRewardInfo instance = null;

    [SerializeField] private List<UIItemGroupRankingRewardInfo> _items;
    [SerializeField] private List<UISprite>                     _sprites;
    [SerializeField] private List<UISprite>                     _spritesMakePixelPerfect;

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

        UnloadAssetBundleResources();
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

        SetReward();
    }

    private void SetReward()
    {
        for (var index = 0; index < _items.Count; index++)
        {
            if(ServerContents.GroupRanking.rankingReward.Count <= index ||
               ServerContents.GroupRanking.rankingRewardRange.Count <= index)
            {
                continue;
            }

            var rewardList = ServerContents.GroupRanking.rankingReward[index];
            _items[index].Init(ServerContents.GroupRanking.rankingRewardRange[index], rewardList);
        }
    }

    private void SetAssetBundleResources()
    {
        ManagerAssetLoader._instance.BundleLoad<NGUIAtlas>(ManagerGroupRanking.ATLAS_ASSET_BUNDLE_NAME, ManagerGroupRanking.ATLAS_ASSET_NAME, OnComplete);

        void OnComplete(NGUIAtlas atlas)
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
        }
    }

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
    }
#endif
}