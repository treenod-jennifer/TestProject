using UnityEngine;

public class UIPopupGuideEvent : UIPopupBase
{
    [SerializeField] private UIUrlTexture mainTexture;

    public void InitPopup()
    {
        mainTexture.SettingTextureScale(mainTexture.width, mainTexture.height);
        mainTexture.LoadCDN(Global.gameImageDirectory, "GuideEvent/", $"n_guide_{ServerRepos.LoginCdn.tutorialCollaboResource}.png");
    }
}
