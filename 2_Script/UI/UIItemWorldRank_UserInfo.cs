using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemWorldRank_UserInfo : UIItemRankingBase
{
    [Header("World Ranking Object")]
    [SerializeField] protected UILabel[] labelScoreCount;
    [SerializeField] private UIUrlTexture texScoreIcon;

    private ManagerWorldRanking.WorldRankData item;

    private bool UpdateData_Base(ManagerWorldRanking.WorldRankData CellData)
    {
        if (CellData == null)
        {
            gameObject.SetActive(false);
            return false;
        }

        item = CellData;
        if (gameObject.activeInHierarchy == false) return false;

        SetRank((int)item.rank);

        //프로필 아이템 추가
        profileItem.SetProfile(item, item.photoUseAgreed);

        labelUserName.text = string.Format("{0}", Global.ClipString(item.ingameName, 12));
        // 미션 labelMissionNum.text = string.Format("{0}", item.UserProfile.mission);
        labelScoreCount[0].text = string.Format("{0}", item.scoreValue);
        labelScoreCount[1].text = string.Format("{0}", item.scoreValue);
        rankingPoint.text = item.rankEventPoint.ToString();
        //포코유라 이미지 세팅.
        SettingPokoyuraTexture(item.userKey, item.flower);
        //유저 칭호 설정.
        SetUserGrade(item.rankEventPoint);

        return true;
    }

    public void UpdateData(ManagerWorldRanking.WorldRankData CellData)
    {
        if (UpdateData_Base(CellData)) 
        {
            UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

            bool isMe = item.userKey == myProfile._userKey;
            SetProfileBG(isMe, item.userKey);
            SetProfileFrame(item.userKey);

            texScoreIcon.LoadCDN(Global.gameImageDirectory, "IconEvent", $"e_worldrank_{ManagerWorldRanking.contentsData.ResourceIndex}_collectibles.png");
        }
    }

    public void UpdateData(ManagerWorldRanking.WorldRankData CellData, int resourceId, long rank)
    {
        if (UpdateData_Base(CellData))
        {
            UserBase myProfile = SDKGameProfileManager._instance.GetMyProfile();

            bool isMe = item.userKey == myProfile._userKey;
            SetProfileBG(isMe, resourceId, rank);
            SetProfileFrame(rank);

            texScoreIcon.LoadCDN(Global.gameImageDirectory, "IconEvent", $"e_worldrank_{resourceId}_collectibles.png");
        }
    }
    public bool CheckItem(string userKey)
    {
        return item.userKey == userKey;
    }
}