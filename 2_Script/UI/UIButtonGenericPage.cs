using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonGenericPage : UIButtonEventBase
{
    public string page;
    public long endTs;
    public bool needLogin;
    [SerializeField] GameObject timeObject;
    [SerializeField] UILabel timeText;

    public override void SetButtonEvent(int idx)
    {
        base.SetButtonEvent(idx);
        string fileName = string.Format("n_{0}", idx);

        buttonTexture.SuccessEvent += OnLoadComplete;
        buttonTexture.LoadCDN(Global.noticeDirectory, "Notice/", fileName);
    }

    protected override void OnClickBtnEvent()
    {
        if( needLogin && ServiceSDK.ServiceSDKManager.instance.IsGuestLogin() )
        {
            ManagerUI._instance.GuestLoginSignInCheck();
        }
        else
        {
            ManagerUI._instance.OpenPopupGenericPage("Notice/" + page);
        }
    }

    private void Start()
    {
        if(endTs == 0)
            timeObject.SetActive(false);
        else
        {
            timeObject.SetActive(true);
            StartCoroutine(CoTimeLeftUpdate());
        }
        
    }

    IEnumerator CoTimeLeftUpdate()
    {
        yield return null;
        if(endTs == 0 )
            yield break;

        long leftTime = 0;

        while (gameObject.activeInHierarchy == true)
        {
            leftTime = Global.LeftTime(this.endTs);
            if (leftTime <= 0)
            {
                leftTime = 0;
                timeText.text = "00:00:00";
                yield break;
            }
            timeText.text = Global.GetLeftTimeText(this.endTs);
            yield return null;
        }
        yield return null;
    }

}
