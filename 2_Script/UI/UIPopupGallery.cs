using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class UIPopupGallery : UIPopupBase
{
    public static UIPopupGallery instance = null;

    [SerializeField] private UIUrlTexture       _textureBg;
    [SerializeField] private UIUrlTexture       _texture;
    [SerializeField] private GameObject         _scrollRoot;
    [SerializeField] private UIPanel            _scrollView;
    [SerializeField] private UIPanel            _panelCenterLight;
    [SerializeField] private UIReuseGridGallery _grid;
    [SerializeField] private GameObject         _objText;

    [SerializeField] private UIPokoButton    _btnNext;
    [SerializeField] private UIPokoButton    _btnPrev;
    private                  UIItemGallery[] _itemGalleries;

    private List<int> _data = new List<int>(); //데이터

    private void Awake()
    {
        _itemGalleries = _grid.GetComponentsInChildren<UIItemGallery>();
        instance       = this;
    }

    protected override void OnDestroy()
    {
        _texture   = null;
        _textureBg = null;
        instance   = null;

        base.OnDestroy();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
        {
            return;
        }

        uiPanel.useSortingOrder           = true;
        uiPanel.sortingOrder              = layer;
        uiPanel.depth                     = layer;
        _scrollView.useSortingOrder       = true;
        _scrollView.sortingOrder          = layer + 1;
        _scrollView.depth                 = layer + 1;
        _panelCenterLight.useSortingOrder = true;
        _panelCenterLight.sortingOrder    = layer + 2;
        _panelCenterLight.depth           = layer + 2;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void Init()
    {
        _textureBg.LoadCDN(Global.gameImageDirectory, "Gallery/", $"gallery_bg.png");

        var atelierClearList = ServerRepos.AtelierClearList;
        if (atelierClearList == null || atelierClearList.Count == 0)
        {
            _scrollRoot.SetActive(false);
            SetCenterImage(null);
            return;
        }

        _data = atelierClearList.Reverse<int>().ToList();

        //그리드 초기화
        _grid.Init(_data, SetCenterImage);

        _btnNext.OnClickAsObservable()
            .Skip(TimeSpan.FromSeconds(0.3f))
            .Subscribe(_ => _grid.MoveRelative(1))
            .AddTo(gameObject);

        _btnPrev.OnClickAsObservable()
            .Skip(TimeSpan.FromSeconds(0.3f))
            .Subscribe(_ => _grid.MoveRelative(-1))
            .AddTo(gameObject);
    }

    //메인 이미지 설정
    private void SetCenterImage(GameObject centerObj)
    {
        _texture.SettingTextureScale(_texture.width, _texture.height);
        //개수 0일 때 기본 이미지 설정
        if (centerObj == null || _data == null || _data.Count == 0)
        {
            _objText.SetActive(true);
            _texture.LoadCDN(Global.gameImageDirectory, "Gallery/", $"painting_empty.png");
        }
        //센터 오브젝트의 텍스쳐를 가져옴
        else
        {
            _objText.SetActive(false);
            var item          = centerObj.GetComponent<UIItemGallery>();
            var centerTexture = item.GetTexture();
            if (centerTexture == null)
            {
                _texture.LoadCDN(Global.gameImageDirectory, "Gallery/", $"painting_{_data[0]}.png");
            }
            else
            {
                _texture.mainTexture = item.GetTexture();
            }
        }
    }

    public void SetPaintAlpha(int idx, float alpha) => _itemGalleries[idx].SetBlack(alpha);
}