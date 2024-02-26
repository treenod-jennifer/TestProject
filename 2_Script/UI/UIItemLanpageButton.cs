using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLanpageButton : MonoBehaviour
{
    [SerializeField] private GameObject button;
    [SerializeField] private UILabel text;
    private string lanpageURL;
    private string lanpageTitle;
    private bool isActive = false;

    public void On(string lanpageURL, string title)
    {
        button.SetActive(true);

        text.text = title;
        this.lanpageURL = lanpageURL;
        this.lanpageTitle = title;

        isActive = true;
    }

    public void On(string lanpageURL, bool isPremium = false)
    {
        string title = Global._instance.GetString(isPremium ? "p_frd_l_21" : "p_frd_l_20");
        
        On(lanpageURL, title);
    }

    public void Off()
    {
        button.SetActive(false);

        isActive = false;
    }

    public void OnClickLanpage()
    {
        if (!isActive) return;

        ServiceSDK.ServiceSDKManager.instance.ShowBoard
        (
            Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms,
            lanpageURL,
            lanpageTitle
        );
    }
}
