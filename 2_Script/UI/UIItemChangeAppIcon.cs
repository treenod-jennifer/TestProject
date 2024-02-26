using UnityEngine;

public class UIItemChangeAppIcon : MonoBehaviour
{
    [SerializeField] private UITexture texAppIcon;
    [SerializeField] private GameObject objSelectedAppIcon;
    
    private AppIconData _appIconData;
    
    public void InitData(AppIconData appIconData)
    {
        _appIconData = appIconData;
        
        texAppIcon.mainTexture = _appIconData.texture;
        objSelectedAppIcon.SetActive(_appIconData.isSelected);
    }
    
    private void OnClickChangeIcon()
    {
        if (_appIconData.isSelected)
            return;
        
        AppIconChanger.iOS.SetAlternateIconName(_appIconData.iconName);
        UIPopupChangeAppIcon.instance.SetAppIconBtn(_appIconData.index);
    }
}
