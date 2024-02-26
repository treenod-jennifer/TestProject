using System.Collections;
using ServiceSDK;
using UnityEngine;

public class UIButtonPromotion : UIButtonEventBase
{
    [SerializeField] private UILabel timeLabel;

    private bool isWaitStop = false;

    private void Start()
    {
        StartCoroutine(CoTimeUpdate());
    }

    public override void SetButtonEvent(int resourceIdx)
    {
        //이벤트 이미지 설정.
        buttonTexture.SuccessEvent += OnLoadComplete;
        buttonTexture.LoadCDN(Global.gameImageDirectory, "IconEvent/", "line_pe_" + resourceIdx);
    }

    protected override void OnClickBtnEvent()
    {
        if (ManagerLobby._instance == null || !ManagerLobby._instance.IsLobbyComplete)
            return;
        
        //이벤트 시간 종료되었을 때, 안내 팝업
        if (Global.LeftTime(ServerContents.PromotionEvent.end_ts) <= 0)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_41"), false, null);
            return;
        }

        //게스트 로그인일 때 라인 로그인 유도 팝업.
        if (ServiceSDK.ServiceSDKManager.instance.IsGuestLogin())
        {
            ManagerUI._instance.GuestLoginSignInCheck();
            return;
        }

        bCanTouch = false;
        if (ManagerData.promotionState == ManagerData.PromotionInitializeState.NONE)
            ManagerData._instance.InitializePromotionSDK();
        StartCoroutine(CoWaitTimer());
        StartCoroutine(CoTriggerChannel());
    }

    private IEnumerator CoWaitTimer()
    {
        yield return new WaitForSeconds(10f);
        isWaitStop = true;
    }

    private IEnumerator CoTriggerChannel()
    {
        NetworkLoading.MakeNetworkLoading(1f);
        yield return new WaitUntil(IsWaitStop);
        NetworkLoading.EndNetworkLoading();

        bCanTouch = true;

        //초기화 완료됐을때만 트리거 채널 호출.
        if (ManagerData.promotionState == ManagerData.PromotionInitializeState.COMPLETE)
        {
            if (ServerContents.PromotionEvent.haveMissions)
            {
                NetworkLoading.MakeNetworkLoading(1f);
                yield return CheckPromotionMission();
                NetworkLoading.EndNetworkLoading();
            }

            ServiceSDKManager.instance.TriggerChannel(ServerContents.PromotionEvent.triggerString, (eventCode, message) =>
            {
                //Debug.Log("PromotionSDK onTriggerMessage with code: " + eventCode + ", message: " + message);
            });
        } 
    }

    private bool IsWaitStop()
    {
        if (isWaitStop == true || ManagerData.promotionState == ManagerData.PromotionInitializeState.COMPLETE)
            return true;
        return false;
    }

    private IEnumerator CheckPromotionMission()
    {
        bool ret = false;
        ServerAPI.CheckPromotionMission((resp) 
            => {
                ret = true;
                }
            );

        yield return new WaitUntil(() => {return ret == true; });
    }
    public IEnumerator CoTimeUpdate()
    {
        yield return new WaitWhile(() => gameObject.activeInHierarchy == false);

        long leftTime = 0;

        while (gameObject.activeInHierarchy == true)
        {
            leftTime = Global.LeftTime(ServerContents.PromotionEvent.end_ts);
            if (leftTime <= 0)
            {
                leftTime = 0;
                timeLabel.text = "00:00:00";
                yield break;
            }

            timeLabel.text = Global.GetLeftTimeText(ServerContents.PromotionEvent.end_ts);
            yield return null;
        }
        yield return null;
    }
}
