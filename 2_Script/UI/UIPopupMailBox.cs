using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public class UIPopupMailBox : UIPopupBase
{
    public static UIPopupMailBox _instance = null;

    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;
    
    public UIPanel      scrollPanel;
    public GameObject   mailboxEmpty;
    public GameObject   mailboxScroll;
    public UILabel[]    title;
    public UILabel      mailBoxText;
    public UILabel      mailBoxWarning;
    public UILabel      emptyText;
    public UITexture    emptyTexture;

    private List<MessageData> listMessageDatas = new List<MessageData>();

    private bool bCheckFriendMessage = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();


        bool skip = false;
#if UNITY_IOS
        if(ServerRepos.LoginCdn.EnableInvite == 0)
            skip = true;
#endif

        if (!skip)
        {
            int day = System.DateTime.Now.DayOfYear;
            int openCount = PlayerPrefs.GetInt("OpenPopupInviteSmallCount", 0);
            if (openCount < 10 && PlayerPrefs.GetInt("OpenPopupInviteSmall", 0) != day)
            {
                openCount++;
                PlayerPrefs.SetInt("OpenPopupInviteSmall", day);
                PlayerPrefs.SetInt("OpenPopupInviteSmallCount", openCount);
                ManagerUI._instance.OpenPopupInviteSmall();
            }
        }
    }
    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        scrollPanel.depth = uiPanel.depth + 1;
        InitMailBox();
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        scrollPanel.useSortingOrder = true;
        scrollPanel.sortingOrder = layer + 1;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void SetMailBox()
    {
        MessageData.SetUserData();
        listMessageDatas = ManagerData._instance._messageData;
        if (ManagerUI._instance != null)
        {
            ManagerUI._instance.UpdateUI();
        }

        if (listMessageDatas.Count == 0)
        {
            mailboxEmpty.SetActive(true);
            mailboxScroll.SetActive(false);
            emptyText.text = Global._instance.GetString("p_e_11");
            emptyTexture.mainTexture = Resources.Load("UI/mailbox_empty") as Texture2D;
        }
        else
        {
            mailboxEmpty.SetActive(false);
            mailboxScroll.SetActive(true);
        }

        if (callbackDataComplete != null && listMessageDatas.Count > 0)
        {
            callbackDataComplete();
        }
    }

    public MessageData GetMessageData(int index)
    {
        if (listMessageDatas.Count <= index || listMessageDatas[index] == null)
            return null;
        return listMessageDatas[index];
    }

    void InitMailBox()
    {
        string titleText = Global._instance.GetString("p_msg_1");
        title[0].text = titleText;
        title[1].text = titleText;
        mailBoxText.text = Global._instance.GetString("p_msg_2");
        mailBoxWarning.text = Global._instance.GetString("p_msg_3");
    }

    void OnClickReceiveAllMessage()
    {
        //터치 가능 조건 검사.
        if (UIPopupMailBox._instance.bCanTouch == false
            || ManagerData._instance._messageData.Count == 0)
            return;

        bCheckFriendMessage = CheckFriendsMessage();
        string text = "";
        if (bCheckFriendMessage == true)
            text = Global._instance.GetString("n_m_3");
        else
            text = Global._instance.GetString("n_m_6");

        //메세지를 모두 받겠습니까? 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "ConfirmReceiveAllMessage", gameObject, true);
        popupSystem.SetButtonText(0, Global._instance.GetString("btn_11"));
        popupSystem.SetButtonText(1, Global._instance.GetString("btn_2"));
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), text, false);

        listTempMessageDatas = new List<MessageData>();
        foreach (var item in listMessageDatas)
        {
            MessageData tempItem = new MessageData();
            tempItem.type = item.type;
            tempItem.value = item.value;
            tempItem.textKey = item.textKey;
            listTempMessageDatas.Add(tempItem);
        }
    }

    private List<MessageData> listTempMessageDatas = new List<MessageData>();

    void ConfirmReceiveAllMessage()
    {
        //터치 가능 조건 검사.
        if (UIPopupMailBox._instance.bCanTouch == false
            || ManagerData._instance._messageData.Count == 0)
            return;

        //터치막음.
        UIPopupMailBox._instance.bCanTouch = false;

    }
    

    void OnTouch()
    {
        //터치 가능.
        UIPopupMailBox._instance.bCanTouch = true;
    }

    void SendReplyToAllFriend()
    {
        //Trident.StringList providerKey = new Trident.StringList();
        List<string> userkeyList = new List<string>();
        for (int i = 0; i < listMessageDatas.Count; i++)
        {
            MessageData mData = listMessageDatas[i];
            //시스템 메세지인 경우 반환.
            if (mData.userKey == "system" || mData.userKey == "SYSTEM")
                continue;
            UserData uData = null;
            //친구가 보낸 메세지일 경우, 클로버 답장.
            if (ManagerData._instance._friendsData.TryGetValue(mData.userKey, out uData))
            {
                bool bRequest = false;
                if(mData.type == RewardType.none && mData.value == 0)
                {
                    bRequest = true;
                }
                //친구가 클로버를 보낸 타입인데, 요청 타입이 아니고 쿨타임 다 안됐으면 라인메세지 안보냄.
                if (bRequest == false && mData.type == RewardType.clover && mData.value > 0 && uData.cloverCoolTime != 0)
                {
                    continue;
                }
                uData.cloverCoolTime = Global.GetTime() + (60 * 60 * 24);
                //userkeyList.Add(ManagerData._instance._friendsData[mData.userKey]._profile.userKey);
            }
        }

        #region 예전 라인 메세지 전송코드
        /*
        // 게임서버와 통신 성공후 친구에게 라인 메세지 전달
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ServiceSDK.ServiceSDKManager lineMessageManager = ServiceSDK.ServiceSDKManager.instance;
            string message = string.Format("[LINE ポコパンタウン] {0}さんからクローバーが届いたよ！", ManagerData._instance.userData._profile.name);
            lineMessageManager.SendMessage(Trident.GraphEventType.GraphEventPresent, providerKey, lineMessageManager.GetMessageContent("lgpkv_send_clover_jp_1", ManagerData._instance.userData._profile.name, message), (bool isSucess) =>
            {
                Debug.Log("=============== Message Successed ===============");
                Debug.Log(providerKey.Count);
            });
            ServiceSDK.MATManager.instance.OnSendClover(1);
        }
        */
        #endregion

        //string message = string.Format("[LINE ポコパンタウン] {0}さんからクローバーが届いたよ！", ManagerData._instance.userData._profile.name);
        //ManagerData.SendLineMessage(userkeyList, "lgpkv_send_clover_jp_1", message);
    }

    bool CheckFriendsMessage()
    {
        for (int i = 0; i < listMessageDatas.Count; i++)
        {
            MessageData mData = listMessageDatas[i];

            //시스템에서 받은 메세지가 아니고 친구에게서 온 메세지가 하나라도 있는 경우, true 반환.
            if (mData.userKey != "system" && mData.userKey != "SYSTEM" &&
               ((mData.type == RewardType.none && mData.value == 0) || mData.type == RewardType.clover))
            {
                return true;
                
            }
        }
        //리스트 다 검사했을 때 친구에게 온 메세지가 없으면 false 반환.
        return false;
    }
}
