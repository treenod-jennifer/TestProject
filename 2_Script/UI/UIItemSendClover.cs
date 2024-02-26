using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Trident;
using UnityEngine;
using UnityEngine.Networking;

public class UIItemSendClover : MonoBehaviour
{
    [SerializeField] private UILabel labelCloverTime;
    [SerializeField] private UISprite spriteBtnSendClover;
    [SerializeField] private BoxCollider collider;

    private UserFriend friend;
    private bool bCanSendClover = false;
    private Action enableAction;

    private void OnEnable()
    {
        if (enableAction != null)
        {
            enableAction();
            enableAction = null;
        }
    }

    public void Init(UserFriend friend)
    {
        enableAction = null;
        this.friend = friend;
        bCanSendClover = !(friend == null || (friend.CloverCoolTime != 0 && Global.LeftTime(friend.CloverCoolTime) > 0));

        SetCloverButton();
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    private void SetCloverButton()
    {
        if (bCanSendClover)
        {
            labelCloverTime.gameObject.SetActive(false);
            spriteBtnSendClover.color = new Color(1f, 1f, 1f, 1f);
            
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
        else
        {
            labelCloverTime.gameObject.SetActive(true);
            spriteBtnSendClover.color = new Color(152f / 255f, 185f / 255f, 208f / 255f, 1f);
            StartTimer();
            
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

    private Coroutine cloverTimer = null;

    private void StartTimer()
    {
        if (friend == null)
        {
            return;
        }

        //오브젝트가 비활성 상태라면, 활성화 상태가 되었을때 타이머를 시작 하도록 처리
        if (!gameObject.activeInHierarchy)
        {
            enableAction += StartTimer;
            return;
        }

        if (cloverTimer != null)
        {
            StopCoroutine(cloverTimer);
        }

        cloverTimer = StartCoroutine(CoCloverTimer());
    }

    private IEnumerator CoCloverTimer()
    {
        while (true)
        {
            long leftTime = Global.LeftTime(friend.CloverCoolTime);
            if (leftTime >= 60)
            {
                labelCloverTime.text = Global.GetTimeText_HHMM(friend.CloverCoolTime);
            }
            else
            {
                labelCloverTime.text = Global.GetTimeText_SS(friend.CloverCoolTime);
            }

            if (leftTime <= 0)
            {
                bCanSendClover = true;
                SetCloverButton();
                break;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    public void OnClickBtnSendClover()
    {
        if (bCanSendClover == false) return;

        // 클로버를 보낼까? 팝업
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "SendClover", gameObject);
        string message = Global._instance.GetString("n_s_14");
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
        popupSystem.SetResourceImage("Message/clover");
    }

    void SendClover()
    {
        // 라인 로그인을 했다면 친구
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ServerAPI.SendClover(friend._userKey, RecvSendClover);
        }
        else //유니티에서는 테스트용 자기 pid로 보내기 > 메세지함에서 확인
        {
            ServerAPI.SendClover("3570255808", RecvSendClover);
        }
    }

    void RecvSendClover(Protocol.SendCloverResp resp)
    {
        if (resp.IsSuccess)
        {
            QuestGameData.SetUserData();

            //Debug.Log( "** Send Clover fUserKey :" + resp.cloverCoolTime.fUserKey );
            string userKey = friend._userKey;
            if (friend != null)
            {
                friend.CloverCoolTime = resp.cloverCoolTime.sendCoolTime;
            }

            if (SDKGameProfileManager._instance.TryGetPlayingFriend(userKey, out UserFriend user))
            {
                if (user.GetTridentProfile() != null)
                {
                    var lineTemplateId = GetCloverSendLineTemplateId();
                    ManagerData.SendLineMessage(userKey, lineTemplateId);
                }
            }

            // 버튼 UI 상태 바꾸기. 시간표시
            bCanSendClover = false;
            SetCloverButton();

            //다이어리 쪽 퀘스트 데이터 갱신.
            UIDiaryController._instance.UpdateQuestData(true);

            //그로씨
            {
                var myProfile = SDKGameProfileManager._instance.GetMyProfile();
                var inviteFriend = new ServiceSDK.GrowthyCustomLog_Social
                (
                    myProfile.stage.ToString(),
                    ServiceSDK.GrowthyCustomLog_Social.Code_L_RSN.SEND_CLOVAR,
                    friend._userKey,
                    user.GetTridentProfile() != null ? "" : "GAMEFRIEND"
                );
                var doc = JsonConvert.SerializeObject(inviteFriend);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("SOCIAL", doc);
            }
        }
        else if (resp.code == (int)ServerError.NotFoundFUserKey)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_gf_16"), false);
        }
    }

    internal static string GetCloverSendLineTemplateId()
    {
        var lineTemplateId = "lgpkv_send_clover_jp_2";
        switch (LanguageUtility.SystemCountryCode)
        {
            case "tw":
                {
                    lineTemplateId = "lgpkv_send_clover_tw_2";
                    break;
                }
            case "jp":
                {
                    lineTemplateId = "lgpkv_send_clover_jp_2";
                    break;
                }
            default:
                {
                    lineTemplateId = "lgpkv_send_clover_jp_2";
                    break;
                }
        }
        return lineTemplateId;
    }

    

   
}
