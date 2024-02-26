using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PokoAddressable;

public class UIPopupIntegratedEvent : UIPopupBase
{
    public static UIPopupIntegratedEvent _instance = null;

    [SerializeField] private GameObject objBannerItem;
    [SerializeField] private UIPanel    scrollView;
    [SerializeField] private UITable    table;
    [SerializeField] private UITexture  titleTexture;

    private bool isCompleteSetBannerInfos = false;

    private List<IntegratedEventInfo>     bannerInfos;
    private List<UIIntegratedEventBanner> bannerItems = new List<UIIntegratedEventBanner>();

    public void InitData(List<IntegratedEventInfo> data)
    {
        bannerInfos   = data;
        _callbackOpen = OnCallbackOpen;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        titleTexture = null;
        _instance    = null;
    }

    private void Start()
    {
        gameObject.AddressableAssetLoad<Texture2D>("local_ui/event_banner_title", (texture) =>
        {
            titleTexture.mainTexture = texture;
        });
        
        SetScrollView();
        SettingDepth();
    }

    private void OnCallbackOpen()
    {
        StartCoroutine(CoStartNewTween());
    }

    private void SetScrollView()
    {
        bannerItems.Clear();

        foreach (var info in bannerInfos)
        {
            var bannerItem = table.transform.AddChild(objBannerItem).GetComponent<UIIntegratedEventBanner>();
            bannerItem.Init(info);

            bannerItems.Add(bannerItem);
        }

        scrollView.GetComponentInChildren<UITable>().Reposition();
        scrollView.GetComponent<UIScrollView>().ResetPosition();

        isCompleteSetBannerInfos = true;
    }

    private void SettingDepth()
    {
        int depth = uiPanel.depth;
        int layer = uiPanel.sortingOrder;

        scrollView.depth = depth + 1;

        if (layer < 10) return;

        scrollView.useSortingOrder = true;
        scrollView.sortingOrder    = layer + 1;
    }

    private IEnumerator CoStartNewTween()
    {
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => isCompleteSetBannerInfos);
        
        foreach (var item in bannerItems)
        {
            item.StartNewTween();
        }
    }
}