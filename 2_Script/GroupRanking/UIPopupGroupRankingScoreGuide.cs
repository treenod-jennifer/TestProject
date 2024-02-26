using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 그룹랭킹 점수 획득 방법 팝업
/// </summary>
public class UIPopupGroupRankingScoreGuide : UIPopupBase
{
    private static UIPopupGroupRankingScoreGuide instance = null;

    [SerializeField] private UILabel _labelNormalMedalCount;
    [SerializeField] private UILabel _labelEndContentsMedalCount;
    [SerializeField] private List<UISprite> _spritesMakePixelPerfect;

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

        _labelNormalMedalCount.text      = ServerContents.GroupRanking.score[0].ToString();
        _labelEndContentsMedalCount.text = ServerContents.GroupRanking.score[1].ToString();
    }

    private void SetAssetBundleResources()
    {
        ManagerAssetLoader._instance.BundleLoad<NGUIAtlas>(ManagerGroupRanking.ATLAS_ASSET_BUNDLE_NAME, ManagerGroupRanking.ATLAS_ASSET_NAME, OnComplete);

        void OnComplete(NGUIAtlas atlas)
        {
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
        foreach (var sprite in _spritesMakePixelPerfect)
        {
            sprite.atlas = null;
        }
    }

#if UNITY_EDITOR
    private void SetEditorAssetBundleResources()
    {
        var atlas = ManagerGroupRanking.instance.LoadEditorAtlasAssetBundle();

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