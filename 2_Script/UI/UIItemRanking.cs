using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Protocol;
using Newtonsoft.Json;

// 셀 데이터를 받아서 스크롤뷰셀의 데이터를 갱신한다.
public class UIItemRanking : UIItemRankingBase
{
    [Header("Normal Ranking Object")]
    [SerializeField] private UILabel labelMissionNum;
    [SerializeField] private UILabel[] labelFlowerCount;
    [SerializeField] private UIItemSendClover sendClover;

    private RankingUIData item;

    public void UpdateData (RankingUIData CellData)
    {
        item = CellData as RankingUIData;
        if ( item == null || gameObject.activeInHierarchy == false )
            return;

        var myProfile = SDKGameProfileManager._instance.GetMyProfile();

        bool isMe = item.UserProfile._userKey == myProfile._userKey;
        SetProfileBG(isMe, item.UserProfile._userKey);
        SetProfileFrame(item.UserProfile._userKey);

        SetRank(item.nRank);

        //프로필 아이템 추가
        profileItem.SetProfile(item.UserProfile);

        labelUserName.text = string.Format( "{0}", Global.ClipString( item.UserProfile.DefaultName, 12 ) );
        labelMissionNum.text = string.Format( "{0}", item.UserProfile.mission );
        labelFlowerCount[0].text = string.Format( "{0}", item.UserProfile.Flower );
        labelFlowerCount[1].text = string.Format( "{0}", item.UserProfile.Flower );
        rankingPoint.text = item.UserProfile.rankEventPoint.ToString();
        //포코유라 이미지 세팅.
        SettingPokoyuraTexture(item.UserProfile._userKey, (int)item.UserProfile.Flower);
        //유저 칭호 설정.
        SetUserGrade(item.UserProfile.rankEventPoint);
        //클로버 전송 이미지.
        sendClover.Init(item.UserProfile as UserFriend);
        sendClover.SetActive(!isMe);
    }
    
    public int GetWidth ()
    {
        return 0;
    }
    public int GetHeight ()
    {
        return 0;
    }

    void OnClickUserProfile ()
    {
        if ( Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer )
        {
            ServerAPI.ProfileLookup( item.UserProfile._userKey, recvProfileLookup );
        }
        else
        {
            ServerAPI.ProfileLookup( "3570255808", recvProfileLookup );
        }
    }

    void recvProfileLookup (ProfileLookupResp resp)
    {
        if ( resp.IsSuccess )
        {
            Method.FunctionVoid func = null;
            //Debug.Log( "** Profile Lookup day : " + resp.profileLookup.day );

            UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

            //현재 유저 데이터라면.
            if (item.UserProfile._userKey == myProfile._userKey)
            {
                func = () => { SettingPokoyuraTexture(item.UserProfile._userKey, (int)item.UserProfile.Flower); };
            }
            ManagerUI._instance.OpenPopupUserProfile(resp, item.UserProfile, func);
        }
    }
}
