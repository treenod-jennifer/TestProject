using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ManagerAdventure : MonoBehaviour {

    public partial class ManagerStageInfo
    {
        bool loaded = false;
        Dictionary<int, ChapterInfo> chapterDic = new Dictionary<int, ChapterInfo>();

        public Dictionary<int, ChapterInfo>.KeyCollection GetChapterIdxList()
        {
            return chapterDic.Keys;
        }
        public ChapterInfo GetChapter(int chapterIdx)
        {
            ChapterInfo c = null;
            chapterDic.TryGetValue(chapterIdx, out c);
            return c;
        }

        public int GetStageCount(int chapterIdx)
        {
            var chap = GetChapter(chapterIdx);
            if (chap == null)
                return 0;
            return chap.GetStageCount();
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

        }

        public void SetTestData()
        {
            for(int cid = 0; cid < 5; cid++)
            {
                ChapterInfo newChap = new ChapterInfo();
                newChap.bossName = "Bossname" + cid.ToString();
                chapterDic.Add(cid, newChap);

                int stageCount = 6;
                for (int i = 0; i < stageCount; ++i)
                {
                    StageInfo newStage = new StageInfo();

                    int monsterCount = 3;
                    for (int j = 0; j < monsterCount; ++j)
                    {
                        MonsterInfo newMon = new MonsterInfo();
                        newMon.resID = j;
                        newMon.SetTestData();
                        newStage.monsters.Add(newMon);
                    }

                    for (int j = 0; j < 3; ++j)
                    {
                        WaveInfo newWave = new WaveInfo();
                        newStage.waves.Add(newWave);
                        for (int k = 0; k < 3; ++k)
                        {
                            newWave.monsters.Add(Random.Range(1, 3));
                        }
                    }
                    newChap.stages.Add(newStage);
                }

            }
            
        }
    }

    

    enum Difficulty
    {
        NORMAL,
        HARD,
    }

    public class ChapterInfo
    {
        internal List<StageInfo> stages = new List<StageInfo>();
        //internal List<Reward> chapterClearReward = new List<Reward>();
        internal string bossName;

        public string GetBossName() { return bossName; }

        public int GetStageCount() { return stages.Count; }
        public StageInfo GetStage(int stageIdx)
        {
            if(stageIdx >= 0 && stageIdx < stages.Count)
                return stages[stageIdx];
            return null;
        }
    }

	public class StageInfo
    {
        int idx;
        internal List<WaveInfo> waves = new List<WaveInfo>();
        internal StageReward[] rewards = new StageReward[2];
        internal List<MonsterInfo> monsters = new List<MonsterInfo>();

        int GetWaveCount() { return waves.Count; }
        WaveInfo GetWaveInfo(int idx)
        {
            return waves[idx];
        }

        int GetMonsterCount() { return monsters.Count; }
        MonsterInfo GetMonster(int monIdx) { return monsters[monIdx]; }
    }
    public class StageReward
    {
        int exp;
        //Reward reward;
    }

    public class WaveInfo
    {
        internal List<int> monsters = new List<int>();
        //internal Reward clearReward;
    }

    public class MonsterInfo
    {
        internal int resID;
        internal int hp;
        internal int atk;
        internal int atkInterval;
        internal bool isBoss;

        public void SetTestData()
        {
            resID = 0;
            hp = Random.Range(100, 1000);
            atk = Random.Range(10, 100);
            atkInterval = Random.Range(1, 3);
            isBoss = false;
        }
    }
}
