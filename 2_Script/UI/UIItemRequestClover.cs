using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

public class UIItemRequestClover : MonoBehaviour
{
    public UISprite checkSprite;
    public UILabel friendName;
    public UILabel requestTime;
    public GameObject checkButtonObj;

    [SerializeField] private UIItemProfile profileItem;

    UIPopupRequestClover.ReqData item;

    public void UpdateData(UIPopupRequestClover.ReqData CellData)
    {
        item = CellData as UIPopupRequestClover.ReqData;

        if (item == null || gameObject.activeInHierarchy == false)
            return;

        //프로필 아이템 추가
        profileItem.SetProfile(item.userData);

        SettingRequestButton();
        friendName.text = string.Format("{0}", Global.ClipString(item.userData.DefaultName, 10));
    }

    void OnClickBtnRequest()
    {   
        //터치 가능 조건 검사.
        if (UIPopupRequestClover._instance.bCanTouch == false || !checkButtonObj.activeSelf)
            return;
        
        if( this.item.check == false && UIPopupRequestClover._instance.CheckCount() >= 10 )
        {
            return;
        }
        //터치막음.
        UIPopupRequestClover._instance.bCanTouch = false;


        ToggleCheckState();

        UIPopupRequestClover._instance.OnCheckChanged();

        UIPopupRequestClover._instance.bCanTouch = true;
    }

    public void SettingRequestButton()
    {
        if (item == null)
            return;

        var friendData = item.userData as UserFriend;
        if( friendData == null )
            return;

        if (friendData.cloverRequestCoolTime != 0)
        {
            SetRequestButtonState(false);
            if(gameObject.activeInHierarchy)
                StartCoroutine(CoRequestTime());
        }
        else
        {
            SetRequestButtonState(true);
        }
    }

    void SetRequestButtonState(bool state)
    {
        if (state == false)
        {   // 누를 수 없는 상태, 시간 돌아감, 어두움
            requestTime.gameObject.SetActive(true);
            checkButtonObj.SetActive(false);
        }
        else
        {
            requestTime.gameObject.SetActive(false);
            checkButtonObj.SetActive(true);

            checkSprite.gameObject.SetActive(this.item.check);
        }
    }

    void SetCheckState(bool state)
    {
        this.item.check = state;
        checkSprite.gameObject.SetActive(this.item.check);
    }

    void ToggleCheckState()
    {
        SetCheckState(this.item.check == false);
    }

    public IEnumerator CoRequestTime()
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false || item.userData.cloverRequestCoolTime <= 0)
                break;
            requestTime.text = Global.GetTimeText_HHMMSS(item.userData.cloverRequestCoolTime);
            long leftTime = Global.LeftTime(item.userData.cloverRequestCoolTime);
            if (leftTime <= 0)
            {
                SetRequestButtonState(true);
                break;
            }
            yield return null;
        }
    }
}
