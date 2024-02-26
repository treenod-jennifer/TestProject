using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPopupRequestClover : UIPopupBase
{
    public static UIPopupRequestClover _instance = null;

    //데이터 전송 다 받았을 때의 콜백.
    public Method.FunctionVoid callbackDataComplete = null;

    public UIPanel scrollPanel;
    public UILabel emptyText;

    List<UserData> friendList = new List<UserData>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        scrollPanel.depth = uiPanel.depth + 1;
        if (UIPopupReady._instance != null)
        {
            uiPanel.useSortingOrder = true;
            scrollPanel.useSortingOrder = true;
        }
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 안올림.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            scrollPanel.useSortingOrder = true;
            scrollPanel.sortingOrder = layer + 1;
        }
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void InitPopUpRequestClover(int requestCloverCnt)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            MakeFriends();
        }
        else
        {
            friendList = ManagerData._instance._friendsData.Values.ToList();
        }

        if (friendList.Count > 0)
        {
            emptyText.gameObject.SetActive(false);
        }
        else
        {
            emptyText.gameObject.SetActive(true);
            emptyText.text = Global._instance.GetString("p_e_2");
        }

        if (callbackDataComplete != null && friendList.Count > 0)
        {
            callbackDataComplete();
        }
    }

    void MakeFriends()
    {
        friendList.Clear();
        //{
        //    int noCount = 1;
        //    //친구정보 생성
        //    for (int i = 1; i < 60; i++)
        //    {
        //        if (i % 2 == 0)
        //            continue;
        //        for (int d = 0; d < 3; d++)
        //        {
        //            UserData fp = new UserData();
        //            fp._profile = new ServiceSDK.UserProfileData();
        //            fp._profile.name = "OREO" + i;
        //            fp._profile.pictureUrl = "http://dl-hsp.profile.line.naver.jp/ch/p/u5511fda78f60dba3796f18bccdcd1768?r=0m07eda01a725&size=small";
        //            friendList.Add(fp);
        //        }
        //    }
        //}
        /*
        // 친구
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "제아(Jea)";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            friendList.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "Evie ㅇㅇㅈ";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            friendList.Add(fp);
        }
        {
            UserData fp = new UserData();
            fp._profile = new ServiceSDK.UserProfileData();
            fp._profile.name = "안준호 (Jude)";
            fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
            friendList.Add(fp);
        }*/
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "Ford";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    friendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "빅 vic";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    friendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "신소정(포코퍼즐 Meriel)";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    friendList.Add(fp);
        //}
        //{
        //    UserData fp = new UserData();
        //    fp._profile = new ServiceSDK.UserProfileData();
        //    fp._profile.name = "에피 (ㄴㄱㅅㅇ)";
        //    fp._profile.pictureUrl = "http://dl.profile.line.naver.jp/0m05debf31725153c696826f7c029877dbe76ef62aa116/small";
        //    friendList.Add(fp);
        //}
    }
    
    public UserData GetFriendData(int index)
    {
        if (friendList.Count <= index || friendList[index] == null)
            return null;

        return friendList[index];
    }

    public int GetUserFriendsCount()
    {
        return (friendList.Count - 1);
    }
}
