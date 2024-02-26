using System;
using System.Collections;
using System.Collections.Generic;
using PokoAddressable;
using UnityEngine;

public abstract class UIItemRankingBase : MonoBehaviour
{
    [Header("Ranking Base Object")]
    [SerializeField] protected GameObject profileBG_Default;
    [SerializeField] protected UIUrlTexture profileBG_WorldRank;

    [SerializeField] protected GameObject profileFrame_Default;
    [SerializeField] protected UISprite profileFrame_WorldRank;

    [SerializeField] protected UIUrlTexture pokogoro;
    [SerializeField] protected UISprite spriteMedal;
    [SerializeField] protected UILabel labelRank;
    [SerializeField] protected Transform objectRank;
    [SerializeField] protected UILabel labelUserName;
    [SerializeField] protected UILabel rankingPoint;
    [SerializeField] protected UILabel[] userGrade;

    [SerializeField] protected UIItemProfile profileItem;

    private int classIndex = 0;

    /// <summary>
    /// 이전 시즌의 데이터로 자동 세팅
    /// </summary>
    protected void SetProfileBG(bool isMe, string userKey)
    {
        long lastRank = 0;
        int resourceId = 1;

        if (ManagerWorldRanking.contentsData != null)
        {
            lastRank = ManagerWorldRanking.contentsData.GetLastSeasonRanking(userKey);
            resourceId = ManagerWorldRanking.contentsData.GetLastSeasonResourceIndex();
        }

        SetProfileBG(isMe, resourceId, lastRank);
    }

    /// <summary>
    /// 수동 세팅
    /// </summary>
    protected void SetProfileBG(bool isMe, int resourceId, long rank)
    {
        profileBG_Default.SetActive(isMe);

        if (rank == 0 || rank > 100)
        {
            profileBG_WorldRank.gameObject.SetActive(false);
        }
        else
        {
            profileBG_WorldRank.gameObject.SetActive(true);

            profileBG_WorldRank.SettingTextureScale(profileBG_WorldRank.width, profileBG_WorldRank.height);

            profileBG_WorldRank.LoadCDN(
                "CachedResource",
                "CachedResource",
                $"{GetBGFileName(resourceId, rank)}.png");
        }
    }

    /// <summary>
    /// 이전 시즌 랭킹으로 프레임 세팅
    /// </summary>
    /// <param name="userKey"></param>
    protected void SetProfileFrame(string userKey)
    {
        if (ManagerWorldRanking.contentsData == null) return;

        long lastRank = ManagerWorldRanking.contentsData.GetLastSeasonRanking(userKey);
        SetProfileFrame(lastRank);
    }

    /// <summary>
    /// 지정된 랭킹으로 프레임 세팅
    /// </summary>
    /// <param name="rank"></param>
    protected void SetProfileFrame(long rank)
    {
        if (rank == 0 || rank > 10)
        {
            profileFrame_Default.SetActive(true);
            profileFrame_WorldRank.gameObject.SetActive(false);
        }
        else
        {
            profileFrame_Default.SetActive(false);
            profileFrame_WorldRank.gameObject.SetActive(true);
            profileFrame_WorldRank.spriteName = GetFrameFileName(rank);
        }
    }

    private string GetFrameFileName(long rank)
    {
        if(rank >= 1 && rank <= 3)
        {
            return $"worldranking_profile_frame_{rank}";
        }
        else if(rank >= 4 && rank <= 10)
        {
            return "worldranking_profile_frame_4";
        }
        else
        {
            return string.Empty;
        }
    }

    private string GetBGFileName(int resourceId, long rank)
    {
        if (rank <= 3)
        {
            return $"profile_bg_{resourceId}_{rank}";
        }
        else if (rank <= 10)
        {
            return $"profile_bg_{resourceId}_4";
        }
        else if (rank <= 100)
        {
            return $"profile_bg_{resourceId}_5";
        }
        else
        {
            return string.Empty;
        }
    }

    private readonly Color[] RANK_COLOR = new Color[]
    {
        new Color(141f / 255f, 91f / 255f, 8f / 255f),
        new Color(58f / 255f, 94f / 255f, 145f / 255f),
        new Color(106f / 255f, 70f / 255f, 24f / 255f),
        new Color(95f / 255f, 110f / 255f, 119f / 255f)
    };

    private const int minNumLength = 2;
    private const int sizeupValue = 14;
    private const int sprMedalWidth = 54;
    protected void SetRank(int rank)
    {
        labelRank.text = rank.ToString();

        int resourceIndex = Mathf.Min(rank, RANK_COLOR.Length);

        labelRank.color = RANK_COLOR[resourceIndex - 1];
        spriteMedal.spriteName = $"ranking_grade_{resourceIndex:D2}";

        spriteMedal.MakePixelPerfect();
        //메달 길이를 가변적으로 처리
        if (minNumLength < rank.ToString().Length)
        {
            spriteMedal.width = sprMedalWidth + sizeupValue * (rank.ToString().Length - minNumLength);
        }
        //메달 위치를 가변적으로 처리
        if (rank.ToString().Length > 4)
        {
            Vector3 pos = objectRank.localPosition;
            objectRank.localPosition = new Vector3(-47f + ((rank.ToString().Length - 4) * 7f), pos.y, pos.z);
        }
    }

    protected void SettingPokoyuraTexture(string userKey, int score)
    {
        bool bSelect = true;
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            userKey = "null";
        }

        // 프로필 데이터가 있을 때 포코고로가 설정돼있다면.
        if (SDKGameProfileManager._instance.TryGetPIONProfile(userKey, out Profile_PION profileInfo))
        {
            Profile_PIONCustom profileData = profileInfo.profile;
            if (profileData.toy != 0)
            {
                classIndex = 0;
                
                pokogoro.SettingTextureScale(pokogoro.width, pokogoro.height);
                pokogoro.LoadCDN(
                    Global.gameImageDirectory,
                    "Pokoyura",
                    $"y_i_{profileData.toy}.png",
                    LoadCdnSettingCondition);

                bSelect = false;
            }
        }

        // 선택이 안되어있을 경우.
        if (bSelect)
        {
            int index = score / 10 + 1;
            if (classIndex != index)
            {
                classIndex = index;
                pokogoro.mainTexture = null;
                gameObject.AddressableAssetLoadClass<Texture>(index, texture =>
                {
                    if (classIndex == index)
                    {
                        pokogoro.mainTexture = texture;
                    }
                });
            }
        }
    }

    private bool LoadCdnSettingCondition()
    {
        return classIndex == 0;
    }

    protected void SetUserGrade(int rankEventPoint)
    {
        string gradeText = "";
        RankingPointGradeData data = null;

        if (rankEventPoint > 0)
        {
            for (int i = 0; i < Global._instance._strRankingPointGradeData.Count; i++)
            {
                data = Global._instance._strRankingPointGradeData[i];
                if ((rankEventPoint >= data.pointMin) && (rankEventPoint <= data.pointMax))
                {
                    gradeText = Global._instance.GetString(data.strKey);
                    break;
                }
            }
        }

        if (data != null)
        {
            userGrade[0].gradientTop = data.topColor;
            userGrade[0].gradientBottom = data.bottomColor;
            userGrade[0].effectColor = data.effectColor;
            userGrade[0].color = data.colorTint;
        }
        for (int i = 0; i < userGrade.Length; i++)
        {
            userGrade[i].text = gradeText;
        }
    }
}