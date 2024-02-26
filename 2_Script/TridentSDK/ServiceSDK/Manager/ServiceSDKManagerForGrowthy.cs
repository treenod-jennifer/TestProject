using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Trident;
using JsonFx.Json;


namespace ServiceSDK
{
    /// <summary>
    /// 라인 로그인후에 SetupGrowthyService() 호출
    /// 게임 로그인후 유저 정보 받아오면 LoadGrowthyUserInfo()호출
    /// </summary>
    public partial class ServiceSDKManager
    {
        private AnalyticsService _growthyService;

        private GrowthyUserInfo _growthyUserInfo; // UserInfo 로그 데이터
        private int             _growthyEventCountInQueue = 0;
        private string          _timeString               = string.Empty;

        private ArrayList                  _growthyItemDiffDataList           = new ArrayList();
        private List<string>               _growthySequentialEventBeforeSetup = new List<string>();
        private Dictionary<string, object> _growthyUserDiffData               = new Dictionary<string, object>();
        private Dictionary<string, string> _growthyCustomEventBeforeSetup     = new Dictionary<string, string>(); // key : 로그 데이터. value : 이벤트 이름(ex. MONEY, ITEM..)
        
        private bool   IsEnableGrowthy => _growthyService != null;
        public  string GetGrowthyVer() => AnalyticsService.ANALYTICS_MODULE_VERSION;
        public  string GetTimeString() => DateTime.Now.ToString("yyyyMMdd HHmmss");

        /// <summary>
        /// GrowthyService(AnalyticsService) 셋업
        /// </summary>
        public void SetupGrowthyService()
        {
            this.SetTimeString();

#if UNITY_EDITOR || UNUSED_LINE_SDK || UNUSED_GROWTH
            return;
#endif

            if (this._growthyService == null)
            {
                this._growthyService = ServiceManager.getInstance().getService<AnalyticsService>();
            }

            Extension.PokoLog.Log("==========growthy SetupGrowthyService enabled : " + this.IsEnableGrowthy);

            this.FlushGrowthyEventBeforeSetup();
        }
        
        /// <summary>
        /// 현재 시간으로 설정
        /// </summary>
        private void SetTimeString()
        {
            _timeString = GetTimeString();
        }

        /// <summary>
        /// UserInfo 데이터가 달라진것이 있으면 전송 + Growthy SDK 큐에 이벤트가 있으면 전송
        /// </summary>
        public void SendGrowthyInfo()
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK || UNUSED_GROWTH
            return;
#endif

            if (this.IsEnableGrowthy == false)
            {
                return;
            }

            SendDifferentGrowthyInfo();
            FlushGrowthyEvent();
        }
        
        /// <summary>
        /// 그로씨 sdk 큐에 이벤트가 있으면 전송
        /// </summary>
        private void FlushGrowthyEvent()
        {
            Extension.PokoLog.Log("==========growthy FlushGrowthyEvent count : " + _growthyEventCountInQueue);

            if (_growthyEventCountInQueue == 0)
            {
                return;
            }

            _growthyEventCountInQueue = 0;
            _growthyService.flushAllEvents();
        }
        
#region UsetInfoLog

        /// <summary>
        /// UserInfo 로그 정보 셋팅 및 전송
        /// </summary>
        public void LoadGrowthyUserInfo()
        {
            // Offline Mode에서는 호출 되어서는 안됨
            _growthyUserInfo = new GrowthyUserInfo
            {
                L_MID   = GetUserKey(),
                L_PCASH = GameData.User.jewel,
                L_FCASH = GameData.User.fjewel,
                L_PGOLD = GameData.User.coin,
                L_FGOLD = GameData.User.fcoin,
                L_APNT  = GameData.User.AllClover,
                L_LFRN  = SDKGameProfileManager._instance.GetPlayingLineFriendsCount(),
                L_CDT   = _timeString,
                L_ULV   = (int)GameData.User.stage - 1,
                L_NUM1  = GameData.User.star,
            };

            // NUM2~NUM5, NUM7 : 꽃 단계별 데이터
            int star1 = 0; int star2 = 0; int star3 = 0; int star4 = 0; int star5 = 0;
            foreach(var tempStage in ServerRepos.UserStages)
            {
                if (tempStage.flowerLevel == 5) star5++;
                else if (tempStage.flowerLevel == 4) star4++;                
                else if (tempStage.flowerLevel == 3) star3++;
                else if (tempStage.flowerLevel == 2) star2++;
                else if (tempStage.flowerLevel == 1) star1++;
            }

            // NUM10 : 월드 랭킹 토큰 데이터
            int rankTokenCount = 0;
            ServerUserTokenAsset rankTokenAsset = null;
            if (ServerRepos.UserTokenAssets != null && ServerRepos.UserTokenAssets.TryGetValue(1, out rankTokenAsset))
            {
                rankTokenCount = rankTokenAsset.amount;
            }

            _growthyUserInfo.L_NUM2 = star1;
            _growthyUserInfo.L_NUM3 = star2;
            _growthyUserInfo.L_NUM4 = star3;
            _growthyUserInfo.L_NUM5 = star4;
            _growthyUserInfo.L_NUM6 = GameData.UserInfo.rankPoint;
            _growthyUserInfo.L_NUM7 = star5;
            _growthyUserInfo.L_NUM8 = GameData.User.AllWing;
            _growthyUserInfo.L_NUM9 = (long)GameData.User.expBall;
            _growthyUserInfo.L_NUM10 = rankTokenCount;
            
            // STR1 : 현재 활성화 되어있는 미션 정보
            foreach(var temp in ServerRepos.OpenMission)
            {
                if(temp.state == (int)TypeMissionState.Active)
                {
                    _growthyUserInfo.L_STR1 = "m_" + temp.idx;
                }
            }
            
            // STR2 : 탐험 모드 정보 (유저가 클리어한 최종 챕터 & 스테이지)
            int tempChapterCnt = 0; int tempStageCnt = 0;
            for (var i = 0; i < ServerRepos.UserAdventureStages.Count; ++i)
            {
                if (ServerRepos.UserAdventureStages[i].chapter > tempChapterCnt && ServerRepos.UserAdventureStages[i].flag > 0)
                {
                    tempChapterCnt = ServerRepos.UserAdventureStages[i].chapter;
                    tempStageCnt = ServerRepos.UserAdventureStages[i].stage;
                }
                else if (tempChapterCnt > 0
                         && ServerRepos.UserAdventureStages[i].chapter == tempChapterCnt
                         && ServerRepos.UserAdventureStages[i].stage   > tempStageCnt
                         && ServerRepos.UserAdventureStages[i].flag    > 0)
                {
                    tempStageCnt = ServerRepos.UserAdventureStages[i].stage;
                }
            }
            if (tempChapterCnt != 0 && tempStageCnt != 0)
            {
                _growthyUserInfo.L_STR2 = tempChapterCnt + "_" + tempStageCnt;
            }
            else
            {
                if(PlayerPrefs.HasKey("ADV_STR_NUM"))
                {
                    _growthyUserInfo.L_STR2 = PlayerPrefs.GetString("ADV_STR_NUM");
                }
            }

            // STR3 : 유저 세그먼트 정보(NRU, CBU, STU...)
            _growthyUserInfo.L_STR3 = makeUserSegmentString();
            
            // STR4 : 언어 선택 환경
            _growthyUserInfo.L_STR4 = LanguageUtility.SystemCountryCode;

            // STR5 : 튜토리얼 활성화 여부
            var tut            = Global._optionTutorialOn;
            var tutorialString = tut ? "TUT1" : "TUT0";
            _growthyUserInfo.L_STR5 = tutorialString;
            
            // STR6, STR7 : 이벤트 정보, 이벤트 재화
            string endContentsInfo = "NONE";
            if (ServerRepos.UserEndContentsEvent != null)
            {
                endContentsInfo = ServerRepos.UserEndContentsEvent.status switch
                {
                    0 => "NONE",
                    1 => "VIEW",
                    2 => "ABLE",
                    _ => endContentsInfo
                };
            }
            _growthyUserInfo.L_STR6.Add(this._growthyUserInfo.GetEventDictionary("END_CONTENTS", endContentsInfo));
            _growthyUserInfo.L_STR7.Add(this._growthyUserInfo.GetEventDictionary("END_CONTENTS", ManagerEndContentsEvent.GetPokoCoin()));

            // L_ITEM : 하우징
            int count = ServerRepos.UserHousingItems != null ? ServerRepos.UserHousingItems.Count : 0;
            for (int i = 0; i < count; i++)
            {
                GrowthyItemInfo.Code_L_STAT state = GrowthyItemInfo.Code_L_STAT.OWN;
                foreach (var temp in ServerRepos.UserHousingSelected)
                {
                    if (temp.index       == ServerRepos.UserHousingItems[i].index &&
                        temp.selectModel == ServerRepos.UserHousingItems[i].modelIndex)
                    {
                        state = GrowthyItemInfo.Code_L_STAT.EQUIP;
                    }
                }

                var itemInfo = new GrowthyItemInfo
                (
                    GetItemId(GrowthyItemInfo.Code_L_CAT.DECO, ServerRepos.UserHousingItems[i].index, ServerRepos.UserHousingItems[i].modelIndex),
                    GetItemId(GrowthyItemInfo.Code_L_CAT.DECO, ServerRepos.UserHousingItems[i].index, ServerRepos.UserHousingItems[i].modelIndex),
                    GrowthyItemInfo.Code_L_CAT.DECO,
                    state,
                    1
                );

                if (!string.IsNullOrEmpty(itemInfo.L_IUID))
                {
                    _growthyUserInfo.GetItemDic().Add(itemInfo);
                }
                //Debug.Log(JsonWriter.Serialize(itemInfo));
            }

            // L_ITEM : 재료
            count = ServerRepos.UserMaterials != null ? ServerRepos.UserMaterials.Count : 0;
            for (int i = 0; i < count; i++)
            {
                if (ServerRepos.UserMaterials[i].count == 0) continue;

                var itemInfo = new GrowthyItemInfo
                (
                 GetItemId(GrowthyItemInfo.Code_L_CAT.MATERIAL, ServerRepos.UserMaterials[i].index, 0),
                  "MATERIAL_" + ServerRepos.UserMaterials[i].index.ToString(),
                  GrowthyItemInfo.Code_L_CAT.MATERIAL,
                  GrowthyItemInfo.Code_L_STAT.OWN,
                  ServerRepos.UserMaterials[i].count
                );

                if (!string.IsNullOrEmpty(itemInfo.L_IUID))
                {
                    _growthyUserInfo.GetItemDic().Add(itemInfo);
                }
                //Debug.Log(JsonWriter.Serialize(itemInfo));
            }

            // L_ITEM : 인게임, 레디 아이템
            count = ServerRepos.UserItem != null && ServerRepos.UserItem.items != null ? ServerRepos.UserItem.items.Count : 0;
            for (int i = 0; i < count; i++)
            {
                if (ServerRepos.UserItem.items[i] == 0) continue;

                if (i > 5)
                {
                    GrowthyItemInfo itemInfo = new GrowthyItemInfo
                    (
                     this.GetItemId(GrowthyItemInfo.Code_L_CAT.CONSUMPTION_INGAME, (i-6), 0),
                     "InGameItem" + (i - 6).ToString(), 
                      GrowthyItemInfo.Code_L_CAT.CONSUMPTION_INGAME,
                      GrowthyItemInfo.Code_L_STAT.OWN,
                      ServerRepos.UserItem.items[i]
                    );

                    if (!string.IsNullOrEmpty(itemInfo.L_IUID))
                    {
                        _growthyUserInfo.GetItemDic().Add(itemInfo);
                    }
                    //Debug.Log(JsonWriter.Serialize(itemInfo));
                }
                else
                {
                    GrowthyItemInfo itemInfo = new GrowthyItemInfo
                    (
                     this.GetItemId(GrowthyItemInfo.Code_L_CAT.CONSUMPTION_LOBBY, i, 0),
                     "ReadyItem" + i.ToString(),
                      GrowthyItemInfo.Code_L_CAT.CONSUMPTION_LOBBY,
                      GrowthyItemInfo.Code_L_STAT.OWN,
                      ServerRepos.UserItem.items[i]
                    );

                    if (!string.IsNullOrEmpty(itemInfo.L_IUID))
                    {
                        _growthyUserInfo.GetItemDic().Add(itemInfo);
                    }
                    //Debug.Log(JsonWriter.Serialize(itemInfo));
                }
            }


#if UNITY_EDITOR || UNUSED_LINE_SDK || UNUSED_GROWTH
            return;
#endif
            _growthyUserInfo.L_CHAR = _growthyUserInfo.GetCharacterDic().Values.ToList();
            _growthyUserInfo.L_ITEM = _growthyUserInfo.GetItemDic().ToList();
            
            // UserInfo 로그 전송
            _growthyService.sendProfile(JsonWriter.Serialize(_growthyUserInfo));
        }

        /// <summary>
        /// UserInfo 데이터가 달라진것이 있으면 전송
        /// </summary>
        private void SendDifferentGrowthyInfo()
        {
            if (_growthyUserInfo == null)
            {
                return;
            }

            // 현재 시간 설정
            SetTimeString();

            bool diff = MakeDifferentGrowthyUserInfo();
            Extension.PokoLog.Log("==========growthy SendDifferntGrowthyInfo : " + diff);

            if (diff == true)
            {
                _growthyService.sendProfile(JsonWriter.Serialize(_growthyUserDiffData));
            }
        }

        /// <summary>
        /// UserInfo 데이터 변경점이 있는지 여부 반환 및 _growthyUserDiffData 업데이트
        /// </summary>
        private bool MakeDifferentGrowthyUserInfo()
        {
            bool diff = false;
            _growthyUserDiffData.Clear();

            ServerUser userData = GameData.User;

            //스테이지 업데이트
            if (this._growthyUserInfo.L_ULV.Equals(((int)GameData.User.stage - 1)) == false && GameData.User.stage != 0)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_ULV", (int)GameData.User.stage - 1);
                this._growthyUserInfo.L_ULV = (int)GameData.User.stage - 1;
            }

            //유상 1차 코인 체크
            if (this._growthyUserInfo.L_PCASH.Equals(userData.jewel) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_PCASH", userData.jewel);
                this._growthyUserInfo.L_PCASH = userData.jewel;
            }

            //무상 1차 코인 체크
            if (this._growthyUserInfo.L_FCASH.Equals(userData.fjewel) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_FCASH", userData.fjewel);
                this._growthyUserInfo.L_FCASH = userData.fjewel;
            }

            //유상 2차 코인 체크
            if (this._growthyUserInfo.L_PGOLD.Equals(userData.coin) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_PGOLD", userData.coin);
                this._growthyUserInfo.L_PGOLD = userData.coin;
            }

            //무상 2차 코인 체크
            if (this._growthyUserInfo.L_FGOLD.Equals(userData.fcoin) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_FGOLD", userData.fcoin);
                this._growthyUserInfo.L_FGOLD = userData.fcoin;
            }

            //액션포인트(클로버) 체크
            if (this._growthyUserInfo.L_APNT.Equals(userData.AllClover) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_APNT", userData.AllClover);
                this._growthyUserInfo.L_APNT = userData.AllClover;
            }


            //게임 친구 체크
            int friendCount = SDKGameProfileManager._instance.GetPlayingLineFriendsCount();
            if (this._growthyUserInfo.L_LFRN.Equals(friendCount) == false && friendCount > 0)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_LFRN", friendCount);
                this._growthyUserInfo.L_LFRN = friendCount;

                //Debug.Log("친구수B" + this._growthyUserInfo.L_LFRN);
            }

            //게임+라인 친구 체크
            int totalFriendCount = SDKGameProfileManager._instance.GetPlayingFriendsCount();
            if (this._growthyUserInfo.L_FRN.Equals(totalFriendCount) == false && totalFriendCount > 0)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_FRN", totalFriendCount);
                this._growthyUserInfo.L_FRN = totalFriendCount;

                //Debug.Log("친구수B" + this._growthyUserInfo.L_LFRN);
            }


            //미션용 별 체크
            if (this._growthyUserInfo.L_NUM1.Equals(userData.star) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM1", userData.star);
                this._growthyUserInfo.L_NUM1 = userData.star;
            }

            //꽃 단계별 데이터 체크
            int star1 = 0; int star2 = 0; var star3 = 0; int star4 = 0; int star5 = 0;
            foreach (var tempStage in ServerRepos.UserStages)
            {
                if (tempStage.flowerLevel == 5) star5++;
                else if (tempStage.flowerLevel == 4) star4++;
                else if (tempStage.flowerLevel == 3) star3++;
                else if (tempStage.flowerLevel == 2) star2++;
                else if (tempStage.flowerLevel == 1) star1++;
            }
            
            if (this._growthyUserInfo.L_NUM2.Equals(star1) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM2", star1);
                this._growthyUserInfo.L_NUM2 = star1;
            }
            if (this._growthyUserInfo.L_NUM3.Equals(star2) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM3", star2);
                this._growthyUserInfo.L_NUM3 = star2;
            }
            if (this._growthyUserInfo.L_NUM4.Equals(star3) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM4", star3);
                this._growthyUserInfo.L_NUM4 = star3;
            }
            if (this._growthyUserInfo.L_NUM5.Equals(star4) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM5", star4);
                this._growthyUserInfo.L_NUM5 = star4;
            }

            if (this._growthyUserInfo.L_NUM6.Equals(GameData.UserInfo.rankPoint) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM6", GameData.UserInfo.rankPoint);
                this._growthyUserInfo.L_NUM6 = GameData.UserInfo.rankPoint;
            }

            if (this._growthyUserInfo.L_NUM7.Equals(star5) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM7", star5);
                this._growthyUserInfo.L_NUM7 = star5;
            }

            //날개
            if (this._growthyUserInfo.L_NUM8.Equals(GameData.User.AllWing) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM8", GameData.User.AllWing);
                this._growthyUserInfo.L_NUM8 = GameData.User.AllWing;
            }

            //성장구슬
            if (this._growthyUserInfo.L_NUM9.Equals(GameData.User.expBall) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM9", GameData.User.expBall);
                this._growthyUserInfo.L_NUM9 = GameData.User.expBall;
            }
            
            int rankToken      = 0;
            ServerUserTokenAsset userTokenAsset = null;
            if (ServerRepos.UserTokenAssets != null && ServerRepos.UserTokenAssets.TryGetValue(1, out userTokenAsset))
            {
                rankToken = userTokenAsset.amount;
            }

            if (this._growthyUserInfo.L_NUM10.Equals(rankToken) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_NUM10", rankToken);
                this._growthyUserInfo.L_NUM10 = rankToken;
            }
            
            string m_STR1 = String.Empty;
            foreach (var temp in ServerRepos.OpenMission)
            {
                if (temp.state == (int)TypeMissionState.Active)
                {
                    m_STR1 = "m_" + temp.idx;
                }
            }
            
            if (this._growthyUserInfo.L_STR1.Equals(m_STR1) == false)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_STR1", m_STR1);
                this._growthyUserInfo.L_STR1 = m_STR1;
            }

            //탐험모드
            //탐험모드정보
            {
                int tempChapterCnt = 0;
                int tempStageCnt = 0;

                for (int i = 0; i < ServerRepos.UserAdventureStages.Count; ++i)
                {
                    if (ServerRepos.UserAdventureStages[i].chapter > tempChapterCnt
                        && ServerRepos.UserAdventureStages[i].flag > 0
                        )
                    {
                        tempChapterCnt = ServerRepos.UserAdventureStages[i].chapter;
                        tempStageCnt = ServerRepos.UserAdventureStages[i].stage;
                    }
                    else if (tempChapterCnt > 0
                        && ServerRepos.UserAdventureStages[i].chapter == tempChapterCnt
                        && ServerRepos.UserAdventureStages[i].stage > tempStageCnt
                         && ServerRepos.UserAdventureStages[i].flag > 0
                        )
                    {
                        tempStageCnt = ServerRepos.UserAdventureStages[i].stage;
                    }
                }

                if (tempChapterCnt != 0 && tempStageCnt != 0)
                {
                    string tempL_STR2 = tempChapterCnt + "_" + tempStageCnt;

                    if (this._growthyUserInfo.L_STR2.Equals(tempL_STR2) == false)
                    {
                        this.InsertObjectIntoDic(this._growthyUserDiffData, "L_STR2", tempL_STR2);
                        this._growthyUserInfo.L_STR2 = tempL_STR2;

                        PlayerPrefs.SetString("ADV_STR_NUM", tempL_STR2);
                    }
                }
            }
            
            var uss = makeUserSegmentString();

            if( !this._growthyUserInfo.L_STR3.Equals(uss) )
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_STR3", uss);
                this._growthyUserInfo.L_STR3 = uss;
            }

            var scc = LanguageUtility.SystemCountryCode;
            if (!this._growthyUserInfo.L_STR4.Equals(scc))
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_STR4", scc);
                this._growthyUserInfo.L_STR4 = scc;
            }

            var tut            = Global._optionTutorialOn;
            var tutorialString = tut ? "TUT1" : "TUT0";
            if (!this._growthyUserInfo.L_STR5.Equals(tutorialString))
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_STR5", tutorialString);
                this._growthyUserInfo.L_STR5 = tutorialString;
            }

            // 이벤트 정보, 이벤트 재화
            string endContentsInfo = "NONE";
            if (ServerRepos.UserEndContentsEvent != null)
            {
                if (ServerRepos.UserEndContentsEvent.status == 0)
                    endContentsInfo = "NONE";
                else if (ServerRepos.UserEndContentsEvent.status == 1)
                    endContentsInfo = "VIEW";
                else if (ServerRepos.UserEndContentsEvent.status == 2)
                    endContentsInfo = "ABLE";
            }

            int pokoCoin = ManagerEndContentsEvent.GetPokoCoin();
            
            for (int i = 0; i < _growthyUserInfo.L_STR6.Count; i++)
            {
                if (this._growthyUserInfo.L_STR6[i].Values.Contains("END_CONTENTS"))
                {
                    if (this._growthyUserInfo.L_STR6[i]["L_VALUE"] != endContentsInfo)
                    {
                        this._growthyUserInfo.L_STR6[i]["L_VALUE"] = endContentsInfo;
                        this.InsertObjectIntoDic(this._growthyUserDiffData, "L_STR6", this._growthyUserInfo.L_STR6);
                    }
                    else
                        this._growthyUserInfo.L_STR6[i]["L_VALUE"] = endContentsInfo;
                }

            }
            
            for (int i = 0; i < _growthyUserInfo.L_STR7.Count; i++)
            {
                if (this._growthyUserInfo.L_STR7[i].Values.Contains("END_CONTENTS"))
                {
                    if (this._growthyUserInfo.L_STR7[i]["L_VALUE"].ToString() != pokoCoin.ToString())
                    {
                        this._growthyUserInfo.L_STR7[i]["L_VALUE"] = pokoCoin;
                        this.InsertObjectIntoDic(this._growthyUserDiffData, "L_STR7", this._growthyUserInfo.L_STR7);
                    }
                    else
                        this._growthyUserInfo.L_STR7[i]["L_VALUE"] = pokoCoin;
                }

                    
            }

            //아이템 체크
            MakeDifferentGrowthyItemInfoList();
            if (this._growthyItemDiffDataList.Count > 0)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_ITEM", this._growthyItemDiffDataList);
            }

            diff = this._growthyUserDiffData.Count > 0;

            if (diff == true)
            {
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_SVR", string.Empty);
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_MID", this.GetUserKey());
                this.InsertObjectIntoDic(this._growthyUserDiffData, "L_CDT", this._timeString);
            }

            return diff;
        }

        /// <summary>
        /// 딕셔너리가 생성되지 않았다면 생성후 데이터변경하고 반환
        /// </summary>
        private Dictionary<string, object> InsertObjectIntoDic(Dictionary<string, object> dic, string key, object obj)
        {
            if (dic == null)
            {
                dic = new Dictionary<string, object>();
            }

            if (dic.ContainsKey(key))
            {
                dic[key] = obj;
            }
            else
            {
                dic.Add(key, obj);
            }

            return dic;
        }

#region L_ITEM : 아이템

        /// <summary>
        /// UserInfo.L_ITEM : 아이템의 변경점이 있으면 멤버 변수(_growthyItemDiffDataList)에 추가
        /// </summary>
        private void MakeDifferentGrowthyItemInfoList()
        {
            _growthyItemDiffDataList.Clear();

            // 하우징
            int count = ServerRepos.UserHousingItems != null ? ServerRepos.UserHousingItems.Count : 0;
            for (int i = 0; i < count; i++)
            {
                GrowthyItemInfo.Code_L_STAT state = GrowthyItemInfo.Code_L_STAT.OWN;

                foreach (var temp in ServerRepos.UserHousingSelected)
                {
                    if (temp.index       == ServerRepos.UserHousingItems[i].index &&
                        temp.selectModel == ServerRepos.UserHousingItems[i].modelIndex)
                    {
                        state = GrowthyItemInfo.Code_L_STAT.EQUIP;
                    }
                }

                var itemInfo = new GrowthyItemInfo
                (
                    GetItemId(GrowthyItemInfo.Code_L_CAT.DECO, ServerRepos.UserHousingItems[i].index,
                        ServerRepos.UserHousingItems[i].modelIndex),
                    string.Format("DECO-{0}-{1}", ServerRepos.UserHousingItems[i].index,
                        ServerRepos.UserHousingItems[i].modelIndex),
                    GrowthyItemInfo.Code_L_CAT.DECO,
                    state,
                    1
                );

                if (!string.IsNullOrEmpty(itemInfo.L_IUID))
                {
                    string itemId = GetItemId(GrowthyItemInfo.Code_L_CAT.DECO, ServerRepos.UserHousingItems[i].index,
                        ServerRepos.UserHousingItems[i].modelIndex);
                    bool hasItem = false;
                    foreach (var temp in _growthyUserInfo.GetItemDic())
                    {
                        if (temp.L_IUID != itemId) continue;

                        if (temp.L_STAT == state.ToString())
                        {
                            hasItem = true;
                            break;
                        }
                        else
                        {
                            _growthyItemDiffDataList.Add(itemInfo);
                            temp.L_STAT = state.ToString();
                            hasItem     = true;
                            break;
                        }
                    }

                    if (hasItem == false)
                    {
                        _growthyItemDiffDataList.Add(itemInfo);
                        _growthyUserInfo.GetItemDic().Add(itemInfo);
                        continue;
                    }
                }
            }

            // 재료
            count = ServerRepos.UserMaterials != null ? ServerRepos.UserMaterials.Count : 0;
            for (int i = 0; i < count; i++)
            {
                var itemInfo = new GrowthyItemInfo
                (
                    GetItemId(GrowthyItemInfo.Code_L_CAT.MATERIAL, ServerRepos.UserMaterials[i].index, 0),
                    "MATERIAL_" + ServerRepos.UserMaterials[i].index.ToString(),
                    GrowthyItemInfo.Code_L_CAT.MATERIAL,
                    GrowthyItemInfo.Code_L_STAT.OWN,
                    ServerRepos.UserMaterials[i].count
                );

                if (!string.IsNullOrEmpty(itemInfo.L_IUID))
                {
                    string itemId = GetItemId(GrowthyItemInfo.Code_L_CAT.MATERIAL, ServerRepos.UserMaterials[i].index,
                        0);
                    bool hasItem = false;
                    foreach (var temp in this._growthyUserInfo.GetItemDic())
                    {
                        if (temp.L_IUID != itemId) continue;

                        if (temp.L_CNT == itemInfo.L_CNT)
                        {
                            hasItem = true;
                            break;
                        }
                        else
                        {
                            _growthyItemDiffDataList.Add(itemInfo);
                            temp.L_CNT = itemInfo.L_CNT;
                            hasItem    = true;
                            break;
                        }
                    }

                    if (hasItem == false)
                    {
                        _growthyItemDiffDataList.Add(itemInfo);
                        _growthyUserInfo.GetItemDic().Add(itemInfo);
                        continue;
                    }
                }
            }

            // 게임 아이템
            count = ServerRepos.UserItem != null && ServerRepos.UserItem.items != null
                    ? ServerRepos.UserItem.items.Count
                    : 0;
            for (int i = 0; i < count; i++)
            {
                if (i > 5)
                {
                    var itemInfo = new GrowthyItemInfo
                    (
                        GetItemId(GrowthyItemInfo.Code_L_CAT.CONSUMPTION_INGAME, (i - 6), 0),
                        "InGameItem" + (i - 6).ToString(),
                        GrowthyItemInfo.Code_L_CAT.CONSUMPTION_INGAME,
                        GrowthyItemInfo.Code_L_STAT.OWN,
                        ServerRepos.UserItem.items[i]
                    );

                    if (string.IsNullOrEmpty(itemInfo.L_IUID)) continue;

                    string itemId  = GetItemId(GrowthyItemInfo.Code_L_CAT.CONSUMPTION_INGAME, (i - 6), 0);
                    bool   hasItem = false;
                    foreach (var temp in this._growthyUserInfo.GetItemDic())
                    {
                        if (temp.L_IUID != itemId) continue;

                        if (temp.L_CNT == itemInfo.L_CNT)
                        {
                            hasItem = true;
                            break;
                        }
                        else
                        {
                            _growthyItemDiffDataList.Add(itemInfo);
                            temp.L_CNT = itemInfo.L_CNT;
                            hasItem    = true;
                            break;
                        }
                    }

                    if (hasItem == false)
                    {
                        _growthyItemDiffDataList.Add(itemInfo);
                        _growthyUserInfo.GetItemDic().Add(itemInfo);
                        continue;
                    }
                }
                else
                {
                    var itemInfo = new GrowthyItemInfo
                    (
                        GetItemId(GrowthyItemInfo.Code_L_CAT.CONSUMPTION_LOBBY, i, 0),
                        "ReadyItem" + i.ToString(),
                        GrowthyItemInfo.Code_L_CAT.CONSUMPTION_LOBBY,
                        GrowthyItemInfo.Code_L_STAT.OWN,
                        ServerRepos.UserItem.items[i]
                    );

                    if (string.IsNullOrEmpty(itemInfo.L_IUID)) continue;

                    string itemId  = GetItemId(GrowthyItemInfo.Code_L_CAT.CONSUMPTION_LOBBY, i, 0);
                    bool   hasItem = false;
                    foreach (var temp in this._growthyUserInfo.GetItemDic())
                    {
                        if (temp.L_IUID != itemId) continue;

                        if (temp.L_CNT == itemInfo.L_CNT)
                        {
                            hasItem = true;
                            break;
                        }
                        else
                        {
                            _growthyItemDiffDataList.Add(itemInfo);
                            temp.L_CNT = itemInfo.L_CNT;
                            hasItem    = true;
                            break;
                        }
                    }

                    if (hasItem == false)
                    {
                        _growthyItemDiffDataList.Add(itemInfo);
                        _growthyUserInfo.GetItemDic().Add(itemInfo);
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 아이템 인덱스값으로 id 구성
        /// </summary>
        private string GetItemId(GrowthyItemInfo.Code_L_CAT type, int nIndex, int sIndex)
        {
            if (sIndex == 0)
            {
                return string.Format("{0}-{1}", type.ToString(), nIndex);
            }
            else
            {
                return string.Format("{0}-{1}-{2}", type.ToString(), nIndex, sIndex);
            }
        }

#endregion

#region L_STR3 : 세그먼트

        /// <summary>
        /// 유저 세그먼트 정보 반환
        /// </summary>
        private static string makeUserSegmentString()
        {
            string attendSegment  = "";
            var    createdTs      = ConvertToUnixTimestamp(ServerRepos.User.createdAt);
            var    nowTs          = ServerRepos.GetServerTime();
            var    nowDate        = ConvertFromUnixTimestamp(nowTs);
            long   accountElapsed = nowTs - createdTs;

            if (accountElapsed < 3600 * 24 * 30)
            {
                if (ServerRepos.User.createdAt.Year      == nowDate.Year &&
                    ServerRepos.User.createdAt.DayOfYear == nowDate.DayOfYear)
                {
                    attendSegment = "NRU1";
                }
                else
                {
                    attendSegment = "NRU2";
                }
            }
            else if (ServerRepos.UserComeback != null && ServerRepos.UserComeback.comebackTs + 3600 * 24 * 30 > nowTs)
            {
                var comeBackDate = ConvertFromUnixTimestamp(ServerRepos.UserComeback.comebackTs);

                if (nowDate.Year == comeBackDate.Year && nowDate.DayOfYear == comeBackDate.DayOfYear)
                {
                    attendSegment = "CBU1";
                }
                else
                {
                    attendSegment = "CBU2";
                }
            }
            else
            {
                long  attendedDay = ServerRepos.UserInfo.loginCnt * 3600 * 24;
                float attendRate  = (float)attendedDay            / (float)accountElapsed;
                if (attendRate < 0.25f)
                    attendSegment = "STU1";
                else if (attendRate < 0.50f)
                    attendSegment = "STU2";
                else if (attendRate < 0.75f)
                    attendSegment = "STU3";
                else if (attendRate < 0.90f)
                    attendSegment = "STU4";
                else
                    attendSegment = "STU5";
            }

            string puSegment    = "PUX";
            var    lastBuyDate  = ConvertFromUnixTimestamp(ServerRepos.LastBuyTime);
            long   buyAfterTime = nowTs - ServerRepos.LastBuyTime;
            if (ServerRepos.LastBuyTime == 0)
            {
                puSegment = "PUX";
            }
            else if (nowDate.Year == lastBuyDate.Year && nowDate.DayOfYear == lastBuyDate.DayOfYear)
            {
                puSegment = "PU0";
            }
            else if (buyAfterTime > 365 * 24 * 3600)
            {
                puSegment = "PU1";
            }
            else if (buyAfterTime > 182 * 24 * 3600)
            {
                puSegment = "PU2";
            }
            else if (buyAfterTime > 91 * 24 * 3600)
            {
                puSegment = "PU3";
            }
            else if (buyAfterTime > 30 * 24 * 3600)
            {
                puSegment = "PU4";
            }
            else
            {
                puSegment = "PU5";
            }

            int flowerCount = 0;
            int clearCount  = 0;
            for (int i = 0; i < ManagerData._instance._stageData.Count; ++i)
            {
                if (ManagerData._instance._stageData[i]._flowerLevel < 3)
                {
                    flowerCount += ManagerData._instance._stageData[i]._flowerLevel < 3
                            ? 0
                            : ManagerData._instance._stageData[i]._flowerLevel - 3;
                }

                if (ManagerData._instance._stageData[i]._flowerLevel != 0)
                    clearCount++;
            }

            float  clearRate = (float)clearCount / (float)ManagerData._instance._stageData.Count;
            string stageSegment;
            if (clearCount >= ManagerData._instance._stageData.Count)
                stageSegment = "SP7";
            else if (GameManager.NowFinalChapter())
                stageSegment = "SP6";
            else if (clearCount <= 50)
                stageSegment = "SP0";
            else if (clearRate < 0.25f)
                stageSegment = "SP1";
            else if (clearRate < 0.50f)
                stageSegment = "SP2";
            else if (clearRate < 0.75f)
                stageSegment = "SP3";
            else if (clearRate < 0.90f)
                stageSegment = "SP4";
            else
                stageSegment = "SP5";

            string flowerSegment = flowerCount == ManagerData._instance._stageData.Count
                    ? "FLA"
                    : $"FL{(int)((flowerCount * 10) / ManagerData._instance._stageData.Count)}";

            string seg = $"{attendSegment}_{puSegment}_{stageSegment}_{flowerSegment}";
            Debug.Log($"Segment String: {attendSegment} / {puSegment} / {stageSegment} / {flowerSegment} = {seg}");
            return seg;
        }

        private static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff   = date - origin;
            return (long)Math.Floor(diff.TotalSeconds);
        }

        private static System.DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            origin = origin.AddHours(9);
            return origin.AddSeconds(timestamp);
        }

#endregion
        
#endregion

#region CustomLog

        /// <summary>
        /// 게임 시작부터 로비 진입까지의 로그 (튜토리얼 시)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void InsertGrowthySequentialEventTutorial(string value)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK || UNUSED_GROWTH
            return;
#endif
            //그로씨 서비스 비활성화시 임시큐로 저장
            if (this.IsEnableGrowthy == false)
            {
                Extension.PokoLog.Log("==========growthy InsertGrowthySequentialEvent(disable): " + value);
                this.InsertGrowthySequentialEventBeforeSetup(value);
                return;
            }

            Extension.PokoLog.Log("==========growthy InsertGrowthySequentialEvent(enable) : " + value);
            this._growthyEventCountInQueue++;

            this._growthyService.trackSequentialEvent(value);
            this._growthyService.flushAllEvents();
        }

        /// <summary>
        /// 게임 시작부터 로비 진입까지의 로그
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void InsertGrowthySequentialEvent(GROWTHY_INFLOW_VALUE value)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK || UNUSED_GROWTH
            return;
#endif

            //그로씨 서비스 비활성화시 임시큐로 저장
            if (this.IsEnableGrowthy == false)
            {
                Extension.PokoLog.Log("==========growthy InsertGrowthySequentialEvent(disable): " + value.ToString());
                this.InsertGrowthySequentialEventBeforeSetup(Enum.GetName(typeof(GROWTHY_INFLOW_VALUE), value));
                return;
            }

            Extension.PokoLog.Log("==========growthy InsertGrowthySequentialEvent(enable) : " + value.ToString());
            this._growthyEventCountInQueue++;

            this._growthyService.trackSequentialEvent(Enum.GetName(typeof(GROWTHY_INFLOW_VALUE), value));
            this._growthyService.flushAllEvents();
        }

        /// <summary>
        /// 그로씨가 활성화 되지 않았을때 임시 큐로 저장
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private void InsertGrowthyEventBeforeSetup(string key, string val)
        {
            if (this._growthyCustomEventBeforeSetup.ContainsKey(key))
            {
                this._growthyCustomEventBeforeSetup[key] = val;
            }
            else
            {
                this._growthyCustomEventBeforeSetup.Add(key, val);
            }
        }
        
        /// <summary>
        /// 그로씨가 활성화 되지 않았을때 임시 큐로 저장 (Sequential)
        /// </summary>
        /// <param name="val"></param>
        private void InsertGrowthySequentialEventBeforeSetup(string val)
        {
            if (!this._growthySequentialEventBeforeSetup.Contains(val))
            {
                this._growthySequentialEventBeforeSetup.Add(val);
            }
        }

        /// <summary>
        /// 그로씨가 활성화 되지 않아 임시 큐로 저장된것을 전송
        /// </summary>
        private void FlushGrowthyEventBeforeSetup()
        {
            // Sequential 이벤트
            if (this._growthySequentialEventBeforeSetup.Count > 0)
            {
                foreach (var data in this._growthySequentialEventBeforeSetup)
                {
                    this._growthyService.trackSequentialEvent(data);
                }

                this._growthySequentialEventBeforeSetup.Clear();
            }

            // 커스텀 이벤트
            if (this._growthyCustomEventBeforeSetup.Count > 0)
            {
                foreach (KeyValuePair<string, string> data in this._growthyCustomEventBeforeSetup)
                {
                    _growthyService.trackCustomizedEvent(data.Value, data.Key);
                }

                this._growthyCustomEventBeforeSetup.Clear();
            }

            // 모두 전송
            this._growthyService.flushAllEvents();
        }
        
        /// <summary>
        /// 커스텀 로그 남기기
        /// </summary>
        public void InsertGrowthyCustomLog(string key, string data)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK || UNUSED_GROWTH
            Debug.Log(key + ", " + data);
            return;
#endif

            if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
            {
                Debug.Log("Growthy =>" +  key + ", " + data);
            }  

            //그로씨 서비스 비활성화시 임시큐로 저장
            if (this.IsEnableGrowthy == false)
            {
                this.InsertGrowthyEventBeforeSetup(data, key);
                return;
            }

            _growthyEventCountInQueue++;
            _growthyService.trackCustomizedEvent(key, data);
            _growthyService.flushAllEvents();

        }

        /// <summary>
        /// MONEY 로그
        /// </summary>
        public void InsertGrowthyCustomLog(GrowthyCustomLog_Money data)
        {
#if UNITY_EDITOR || UNUSED_LINE_SDK || UNUSED_GROWTH
            return;
#endif
            string key = "MONEY";
            string val = JsonWriter.Serialize(data);

            this.InsertGrowthyCustomLog(key, val);
        }

#endregion
    }
}