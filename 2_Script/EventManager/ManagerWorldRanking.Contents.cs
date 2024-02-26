using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerWorldRanking : MonoBehaviour
{
    public class ContentsData
    {
        public List<CdnWorldRankHeader> AllEventRankingFactors { get; private set; }

        private CdnWorldRankHeader previousEventRankingFactor;
        public CdnWorldRankHeader PreviousEventRankingFactor 
        {
            get
            {
                if (ServerContents.WorldRank == null) return null;

                previousEventRankingFactor = GetPreviousEventRankingFactor(ServerContents.WorldRankHeaders, ServerContents.WorldRank.eventIndex);

                return previousEventRankingFactor;
            }
        }
        
        public int EventIndex { get; private set; } = 0;

        public int ResourceIndex { get; private set; } = 0;
        
        public int IngameBGIndex { get; private set; } = 0;

        public int StageCount { get; private set; }

        public int ReqStage { get; private set; } = 0;

        public int stageRandSeed;

        public string TableIdRank { get; private set;}
        
        public string TableIdEntry { get; private set; }

        public List<int> easyStageOrder = new List<int>();
        public List<int> stageOrder = new List<int>();

        public int coopGroupSize;
        
        public CoopReward[] coopReward;

        public List<WorldRankReward> rewards;

        private class PreviousEvent
        {
            public int previousEventIndex = 0;
            public int previousResourceIndex = 1;
            public Dictionary<string, WorldRankData> previousEventRanking = new Dictionary<string, WorldRankData>();
        }
        private PreviousEvent previousEvent = new PreviousEvent();

        public void SyncFromServerContentsData()
        {
            this.stageOrder.Clear();
            this.easyStageOrder.Clear();

            var Rng = new System.Random(ServerContents.WorldRank.stageRandSeed);
            for(int i = 1; i < ServerContents.WorldRank.stageCount + 1; ++i)
            {
                this.stageOrder.Add(i);
            }

            var easyRng = new System.Random(ServerContents.WorldRank.easyStageRandSeed);
            for (int i = 1; i < ServerContents.WorldRank.easyStageCount + 1; ++i)
            {
                this.easyStageOrder.Add(i);
            }

            if (ServerContents.WorldRank.stageRandSeed != 0)
                GenericHelper.Shuffle(stageOrder, Rng);

            if (ServerContents.WorldRank.easyStageRandSeed != 0)
                GenericHelper.Shuffle(easyStageOrder, easyRng);

            EventIndex = ServerContents.WorldRank.eventIndex;
            ResourceIndex = ServerContents.WorldRank.resourceId;
            AllEventRankingFactors = ServerContents.WorldRankHeaders;
            IngameBGIndex = ServerContents.WorldRank.themeId;
            StageCount = ServerContents.WorldRank.stageCount;
            ReqStage = ServerContents.WorldRank.reqStage;
            TableIdRank = ServerContents.WorldRank.tableIdRank;
            TableIdEntry = ServerContents.WorldRank.tableIdEntry;
            coopReward = ServerContents.WorldRank.coopReward;
            coopGroupSize = ServerContents.WorldRank.coopGroupSize;
            rewards = ServerContents.WorldRank.rewards;

            instance.StartCoroutine(LoadPreviousRanking());
        }

        public bool IsEasyStage(int stageIndex)
        {
            return stageIndex <= ServerContents.WorldRank.easySectionSize;
        }

        // 스테이지 파일명 구하기 (현재 진행중인 스테이지 순서 넣으면, 스테이지 파일명에 쓸 번호 나옴)
        public int GetStageFileIndex(int stageIndex)
        {
            int stageIndexModified = stageIndex;
            int sectionSize = ServerContents.WorldRank.easyStageCount;

            if ( !IsEasyStage(stageIndex) )
            {
                stageIndexModified = stageIndex - ServerContents.WorldRank.easySectionSize;
                sectionSize = ServerContents.WorldRank.stageCount;

                var s = ((stageIndexModified - 1) % sectionSize);
                return stageOrder[s];
            }
            else
            {
                stageIndexModified = stageIndex;
                sectionSize = ServerContents.WorldRank.easyStageCount;

                var s = ((stageIndex - 1) % sectionSize);
                return easyStageOrder[s];
            }
        }

        // 현재 몇바퀴째 돌렸는지
        public int GetRound(int stageIndex)
        {
            int stageIndexModified = stageIndex;
            if (!IsEasyStage(stageIndex))
            {
                stageIndexModified = stageIndex - ServerContents.WorldRank.easySectionSize;
                // input stageindex = 1 ~ (stageCount+1)
                var s = (stageIndexModified - 1) / this.StageCount;
                return s;

            }
            else return 0;
        }

        public int GetCoopRewardLevel(int totalScore)
        {
            for(int i = coopReward.Length - 1; i >= 0;i-- )
            {
                if (coopReward[i].targetCount <= totalScore)
                    return i;
            }
            return -1;
        }


        public CoopReward GetCurrentCoopReward(int step)
        {
            if (step >= coopReward.Length)
                step = coopReward.Length - 1;

            return coopReward[step];
        }

        private CdnWorldRankHeader GetPreviousEventRankingFactor(List<CdnWorldRankHeader> allEventRankingFactors, int currentEventIndex)
        {
            if (allEventRankingFactors == null) return null;

            if (allEventRankingFactors.Count < 2) return null;

            if (ServerContents.WorldRank != null && Global.LeftTime(ServerContents.WorldRank.rewardTs) <= 0)
            {
                return allEventRankingFactors.Find((rank) => rank.eventIndex == currentEventIndex);
            }
            else
            {
                for (int i = 1; i < allEventRankingFactors.Count; i++)
                {
                    if (allEventRankingFactors[i].eventIndex == currentEventIndex)
                    {
                        return allEventRankingFactors[i - 1];
                    }
                }
            }

            return null;
        }

        public long GetLastSeasonRanking(string userKey)
        {
#if UNITY_EDITOR
            if (userKey == null || userKey == string.Empty) return 1;
            return Mathf.Abs(userKey.GetHashCode()) % 100 + 1;
#endif

            if(previousEvent.previousEventRanking.TryGetValue(userKey, out WorldRankData rankData))
            {
                return rankData.rank;
            }

            return 0;
        }

        public int GetLastSeasonResourceIndex()
        {
            return previousEvent?.previousResourceIndex ?? 1;
        }

        private IEnumerator LoadPreviousRanking()
        {
            CdnWorldRankHeader previousRankingFactor = PreviousEventRankingFactor;

            if (previousRankingFactor == null) yield break;

            if (previousRankingFactor.eventIndex == previousEvent.previousEventIndex) yield break;

            previousEvent.previousEventIndex = previousRankingFactor.eventIndex;
            previousEvent.previousResourceIndex = previousRankingFactor.resourceId;
            List<WorldRankData> previousRanking = new List<WorldRankData>();
            yield return QueryHallOfFame(previousRankingFactor.tableIdRank, previousRanking);

            previousEvent.previousEventRanking.Clear();
            foreach(var rank in previousRanking)
            {
                previousEvent.previousEventRanking.Add(rank.userKey, rank);
            }
        }
    }   
}
