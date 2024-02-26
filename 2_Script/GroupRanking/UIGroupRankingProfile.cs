using System.Collections.Generic;
using UnityEngine;

public class UIGroupRankingProfile : MonoBehaviour
{
    [SerializeField] private List<UISprite> _sprites;

    private void Start()
    {
        if (ManagerGroupRanking.isEventOn == false)
        {
            return;
        }

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
    }

    private void OnDestroy() => UnloadAssetBundleResources();

    private void SetAssetBundleResources()
    {
        ManagerAssetLoader._instance.BundleLoad<NGUIAtlas>(ManagerGroupRanking.ATLAS_ASSET_BUNDLE_NAME, ManagerGroupRanking.ATLAS_ASSET_NAME, OnComplete);

        void OnComplete(NGUIAtlas atlas)
        {
            foreach (var sprite in _sprites)
            {
                sprite.atlas = atlas;
            }
        }
    }

    private void UnloadAssetBundleResources()
    {
        foreach (var sprite in _sprites)
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
    }
#endif
}