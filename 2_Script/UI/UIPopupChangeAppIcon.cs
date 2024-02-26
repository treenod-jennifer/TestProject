using System.Collections.Generic;
using UnityEngine;

public class AppIconData
{
    public int index;
    public string iconName;
    public Texture2D texture;
    public bool isSelected;
}

public class UIPopupChangeAppIcon : UIPopupBase
{
    public static UIPopupChangeAppIcon instance = null;

    [SerializeField] private AlternateIcons _alternateIconAsset;

    [SerializeField] private UIGrid     _gridAppIcon;
    [SerializeField] private GameObject _objItem;
    [SerializeField] private Texture2D  _defaultTex;

    private List<AppIconData>         _appIconList          = new List<AppIconData>();
    private List<UIItemChangeAppIcon> _uiItemChangeAppIcons = new List<UIItemChangeAppIcon>();

    private void Awake() => instance = this;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        instance = null;
    }

    private void Start()
    {
        InitData();

        for (var i = 0; i < _appIconList.Count; i++)
        {
            _uiItemChangeAppIcons.Add(_gridAppIcon.gameObject.AddChild(_objItem).GetComponent<UIItemChangeAppIcon>());
            _uiItemChangeAppIcons[i].InitData(_appIconList[i]);
        }

        _gridAppIcon.Reposition();
    }

    /// <summary>
    /// 데이터 초기화 및 할당
    /// </summary>
    private void InitData()
    {
        _appIconList.Clear();
        _appIconList.Add(new AppIconData()
        {
            index      = 0,
            texture    = _defaultTex,
            isSelected = string.IsNullOrEmpty(AppIconChanger.iOS.AlternateIconName)
        });

        if (ServerContents.AppIconVer > 0)
        {
            for (var i = 0; i < _alternateIconAsset._icons.Length; i++)
            {
                // ver 체크
                if (ServerContents.AppIconVer != _alternateIconAsset._icons[i]._version)
                {
                    continue;
                }

                var alternateIcon = _alternateIconAsset._icons[i];
                for (var index = 0; index < alternateIcon._iconDatas.Length; index++)
                {
                    var icon = alternateIcon._iconDatas[index];
                    _appIconList.Add(new AppIconData()
                    {
                        index    = index + 1,
                        iconName = icon._name,
                        texture  = icon._texture,

                        // 아이콘 선택으로 설정이 되어 있지 않았을 때 디폴트 아이콘으로 설정
                        isSelected = AppIconChanger.iOS.AlternateIconName == icon._name
                    });
                }
            }
        }
    }

    /// <summary>
    /// 아이콘 클릭 시 데이터 및 UI 갱신
    /// </summary>
    public void SetAppIconBtn(int index)
    {
        for (var i = 0; i < _appIconList.Count; i++)
        {
            _appIconList[i].isSelected = index == _appIconList[i].index;
            _uiItemChangeAppIcons[i].InitData(_appIconList[i]);
        }
    }
}
