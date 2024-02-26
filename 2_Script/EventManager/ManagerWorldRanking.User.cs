using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerWorldRanking : MonoBehaviour
{
    public class UserData
    {
        public int CurrentStage { get; private set;}
        public int PlayCount { get; private set; }
        private int UserRewardCount { get; set; }
        private int Stage { get; set; }
        public int Score {  get; private set;}
        public int CoopGroupId { get; private set;}
        
        public int SaleContinueCount { get{ return ServerRepos.UserWorldRank?.saleContinueCount ?? 0;} }  // 세일컨티뉴 사용가능횟수

        public string myUserKey { get { return SDKGameProfileManager._instance.GetMyProfile()._userKey; } }

        public void SyncFromServerUserData()
        {
            if(ServerRepos.UserWorldRank == null)
                return;
            CurrentStage = ServerRepos.UserWorldRank.stage;
            Score = ServerRepos.UserWorldRank.score;
            PlayCount = ServerRepos.UserWorldRank.play;
            UserRewardCount = ServerRepos.UserWorldRank.rewardCount;
            Stage = ServerRepos.UserWorldRank.stage;
        }

        public int StageRewardGain() { return StageClearCount / 10 - UserRewardCount; }

        public void SyncForGameClearLog()
        {
            PlayCount = ServerRepos.UserWorldRank.play;
        }

        public int StageClearCount { get { return Stage - 1; } }

        public List<string> GetGroupUserKeys() { return new List<string>() { }; }

        /// 랭킹 토큰샵 관련 함수
        /// 
        public List< CdnWorldRankShop > GetRankPointShopGoods()
        {
            return ServerContents.WorldRankShop;
        }

        public enum BuyError
        {
            NO_ERROR = 0,
            NOT_ENOUGH_TOKEN,
            BUY_LIMIT,
            INVALID_GOODS,
        }

        public int GetRankToken()
        {
            ServerUserTokenAsset userTokenAsset = null;
            if (ServerRepos.UserTokenAssets.TryGetValue(1, out userTokenAsset))
            {
                return userTokenAsset.amount;
            }

            return 0;
        }

        public BuyError CanBuy(int goodsId )
        {
            var goodsInfo = ServerContents.WorldRankShop.Find(x => x.goodsId == goodsId);
            if (goodsInfo == null)
            {
                return BuyError.INVALID_GOODS;
            }

            Dictionary<int, ServerUserTokenUsedHistory> goodsMap;
            if (goodsInfo.buyLimit == 1) // 한정 상품일 경우만 검사.
            {
                if (ServerRepos.UserTokenUsedHistory.TryGetValue(1, out goodsMap))
                {
                    ServerUserTokenUsedHistory buyHistory;
                    if (goodsMap.TryGetValue(goodsId, out buyHistory))
                    {
                        if (buyHistory.count >= goodsInfo.buyLimit)
                            return BuyError.BUY_LIMIT;
                    }
                }
            }

            if (GetRankToken() < goodsInfo.price)
            {
                return BuyError.NOT_ENOUGH_TOKEN;
            }


            return BuyError.NO_ERROR;
        }

        public int UserBuyLimitItemCount(CdnWorldRankShop goodsInfo)
        {
            Dictionary<int, ServerUserTokenUsedHistory> goodsMap;
            if(goodsInfo.buyLimit == 1) // 한정상품 일 때
            {
                if(ServerRepos.UserTokenUsedHistory.TryGetValue(1, out goodsMap))
                {
                    ServerUserTokenUsedHistory buyHistory;
                    if (goodsMap.TryGetValue(goodsInfo.goodsId, out buyHistory))
                    {
                        return goodsInfo.buyLimit - buyHistory.count;
                    }
                }
            }
            return goodsInfo.buyLimit;
        }

        /// <summary>
        /// 현재 시즌에 참여한 이력이 있다면 true
        /// </summary>
        /// <returns></returns>
        internal bool IsParticipated()
        {
            if( ServerRepos.UserWorldRank == null )
                return false;

            if ( ServerRepos.UserWorldRank.stage > 1 )
                return true;

            return false;
        }
    }
}
