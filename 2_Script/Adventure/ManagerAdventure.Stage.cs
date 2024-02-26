using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAdventure : MonoBehaviour {

    public partial class ManagerStageInfo
    {
        bool loaded = false;
        SortedDictionary<int, ChapterInfo> chapterDic = new SortedDictionary<int, ChapterInfo>();

        public class StageOrderData
        {
            public int chapIdx;
            public int stageIdx;
            public int stageType;
        }

        SortedList<int, StageOrderData> normalStageOrder = new SortedList<int, StageOrderData>();
        SortedList<int, StageOrderData> allStageOrder = new SortedList<int, StageOrderData>();

        public SortedDictionary<int, ChapterInfo>.KeyCollection GetChapterIdxList()
        {
            return chapterDic.Keys;
        }
        public ChapterInfo GetChapter(int chapterIdx)
        {
            ChapterInfo c = null;
            chapterDic.TryGetValue(chapterIdx, out c);
            return c;
        }        

        public StageInfo GetStage(int chapterIdx, int stageIdx)
        {
            var chap = GetChapter(chapterIdx);
            if (chap == null)
                return null;
            return chap.GetStage(stageIdx);
        }

        public void OnReboot()
        {
            if (chapterDic != null)
                chapterDic.Clear();
        }

        public void SetTestData()
        {
            for(int cid = 1; cid < 5; cid++)
            {
                ChapterInfo newChap = new ChapterInfo();
                newChap.bossName = "Bossname" + cid.ToString();
                newChap.chapterBossResId = cid;
                chapterDic.Add(cid, newChap);
                
                int stageCount = 6;
                for (int i = 1; i < stageCount; ++i)
                {
                    StageInfo newStage = new StageInfo();
                    newStage.idx = i;
                    newStage.rewards = new StageReward() { exp = 100, reward = new Reward() { type = (int)RewardType.clover, value = 1 } };
                    if (i == 5)
                        newStage.stageType = 1;

                    newChap.stages.Add(newStage);
                }
            }
        }

        public bool CheckNowOpenedChapter()
        {
            foreach (var serverChap in ServerContents.AdvChpaters)
            {
                if (Global.LeftTime(serverChap.Value.start_ts) > 0)
                {
                    continue;
                }

                if (chapterDic.ContainsKey(serverChap.Key) == false)
                    return true;
            }

            return false;
        }

        public bool LoadFromServerContents()
        {
            chapterDic.Clear();
            allStageOrder.Clear();
            normalStageOrder.Clear();
            foreach (var serverChap in ServerContents.AdvChpaters)
            {
                if( Global.LeftTime( serverChap.Value.start_ts ) > 0 )
                {
                    continue;
                }

                ChapterInfo newChap = new ChapterInfo();
                newChap.chapterBossResId = serverChap.Value.bossId;
                chapterDic.Add(serverChap.Key, newChap);

                newChap.chapterClearReward = serverChap.Value.rewards;
                newChap.missionClearReward = serverChap.Value.mission_reward;

                if ( ServerContents.AdvStages.ContainsKey(serverChap.Key) )
                {
                    var serverStageDic = ServerContents.AdvStages[serverChap.Key];

                    SortedDictionary<int, StageInfo> stages = new SortedDictionary<int, StageInfo>();


                    foreach (var serverStage in serverStageDic)
                    {
                        StageInfo newStage = new StageInfo();
                        newStage.idx = serverStage.Key;
                        newStage.mission = serverStage.Value.mission;
                        newStage.firstDropBoxRatio = serverStage.Value.first_drop_box_ratio;
                        newStage.normalDropBoxRatio = serverStage.Value.normal_drop_box_ratio;
                        newStage.dropBoxRatio = serverStage.Value.drop_boxes;
                        newStage.stageType = serverStage.Value.stage_type;
                        newStage.rewards = new StageReward()
                        {
                            exp = serverStage.Value.normal_exp,
                            reward = null,
                        };

                        if( serverStage.Value.rewards.Count > 0)
                        {
                            newStage.rewards.reward = serverStage.Value.rewards[0];
                        }

                        stages.Add(newStage.idx, newStage);

                        StageOrderData orderData = new StageOrderData() { chapIdx = serverChap.Key, stageIdx = newStage.idx, stageType = newStage.stageType };
                        int orderKey = orderData.chapIdx * 100000 + orderData.stageIdx;
                        if (orderData.stageType == 0)
                            normalStageOrder.Add(orderKey, orderData);

                        allStageOrder.Add(orderKey, orderData);

                    }
                    newChap.stages.AddRange(stages.Values);
                }
            }
            return true;
        }

        public StageOrderData GetPrevStage(int chapIdx, int stageIdx, bool onlyNormalStage)
        {
            int orderKey = chapIdx * 100000 + stageIdx;

            if (onlyNormalStage)
            {
                if (normalStageOrder.ContainsKey(orderKey) == false)
                    return null;
                var idx = normalStageOrder.IndexOfKey(orderKey);
                if (idx < 1)
                    return null;

                return normalStageOrder.Values[idx-1];
            }
            else
            {
                if (allStageOrder.ContainsKey(orderKey) == false)
                    return null;

                var idx = allStageOrder.IndexOfKey(orderKey);
                if (idx < 1)
                    return null;

                return allStageOrder.Values[idx-1];
            }
        }

        public bool IsLastStageInChapter(int chapIdx, int stageIdx)
        {
            int orderKey = chapIdx * 100000 + stageIdx;

            if (allStageOrder.ContainsKey(orderKey) == false)
                return true;

            var idx = allStageOrder.IndexOfKey(orderKey);
            if (idx == -1)
                return true;
            if (idx + 1 >= allStageOrder.Count)
                return true;
            var prevStage = allStageOrder.Values[idx];

            var nextStage = allStageOrder.Values[idx + 1];

            if (prevStage.stageType != nextStage.stageType)
                return true;

            if (prevStage.chapIdx != nextStage.chapIdx)
                return true;

            return false;
        }

        public SortedList<int, StageOrderData> GetStageOrderList(bool allTypeStageList)
        {
            return allTypeStageList == false ? this.normalStageOrder : this.allStageOrder;
        }



        //public void AddStageData(int index, ChapterInfo chapterData)
        //{
        //    chapterDic.Add(index, chapterData);
        //}
    }

    

    enum Difficulty
    {
        NORMAL,
        HARD,
    }

    public class ChapterInfo
    {
        internal List<StageInfo> stages = new List<StageInfo>();
        internal List<Reward> chapterClearReward = new List<Reward>();
        internal List<Reward> missionClearReward = new List<Reward>();
        internal string bossName;
        internal int chapterBossResId;

        public string GetBossName() { return bossName; }

        //public int GetStageCount() { return stages.Count; }
        
        public List<StageInfo> GetStageList() { return stages; }    // foreach 개념으로 쓸 때만 참조해야함. 안에 들어있는 스테이지들이 idx가 꽉찬 순서대로 들어있다는 보장이 없음

        public StageInfo GetStage(int stageIdx)
        {
            return stages.Find(x => x.idx == stageIdx);
        }

        public int GetLastStageIdx(bool includeAllType = false)
        {
            int stageIdx = -1;
            for(int i = 0; i < stages.Count; ++i)
            {
                if( includeAllType )
                {
                    if (stageIdx < stages[i].idx)
                        stageIdx = stages[i].idx;
                }
                else
                {
                    if (stageIdx < stages[i].idx && stages[i].stageType == 0)
                        stageIdx = stages[i].idx;
                }
                
            }
            return stageIdx;
        }
    }

	public class StageInfo
    {
        internal int idx;
        internal int stageType;
        internal int mission;
        internal int firstDropBoxRatio;
        internal int normalDropBoxRatio;

        internal Dictionary<int, List<int>> dropBoxRatio = new Dictionary<int, List<int>>();
        internal StageReward rewards = new StageReward();
    }

    public class StageReward
    {
        public int exp;
        public Reward reward;
    }
}
