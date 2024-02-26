using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using SideIcon;
using UnityEngine;

public class ManagerMoleCatch : MonoBehaviour, IEventBase {
    static public ManagerMoleCatch instance = null;

    public enum EventState
    {
        NOT_STARTED,
        WAIT_FOR_START,
        RUNNING,
        ENDED,
    }

    EventState eventState = EventState.NOT_STARTED;

    int eventIndex;
    int resourceIndex;
    long endTs;

    private IconMoleCatchEvent icon;

    public struct RewardData
    {
        public int rewardIndex;
        public bool isWaveReward;
        public Reward reward;
        public List<int> requiredStages;
    }
    

    public class WaveData
    {
        public int stageCount = 9;
        public Dictionary<int, int> stageMap = new Dictionary<int, int>();
        public HashSet<int> hardMap = new HashSet<int>();
        public Dictionary<int, int> stageVer = new Dictionary<int, int>();
        public SortedDictionary<int, RewardData> rewards = new SortedDictionary<int, RewardData>();
    }

    public class UserState
    {
        public int currentWave = 1;
        public List<int> upMoles = new List<int>();

        public HashSet<int> clearedStages = new HashSet<int>();
        public List<int> leftRewards = new List<int>();
    }

    public class ReadyResource
    {
        public Texture readyMole_normal_1;
        public Texture readyMole_normal_2;

        public Texture readyMole_hard_1;
        public Texture readyMole_hard_2;

        public Texture readyEmoticonTexture;
        public Vector3 readyEmoticonPosition_1 = new Vector3(30f, 95f, 0f);
        public Vector3 readyEmoticonPosition_2 = new Vector3(30f, 75f, 0f);
    }

    SortedDictionary<int, WaveData> waveData = new SortedDictionary<int, WaveData>();

    UserState userState = new UserState();

    public ReadyResource readyResource = null;

    static public int lastPlayedStage = 0;


    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnReboot()
    {
        if( instance != null )
        {
            Destroy(instance);
            lastPlayedStage = 0;
        }
    }
    
    public void OnEventIconClick(object obj = null)
    {
        icon?.SetNewIcon(IsPlayCurrentVersion() == false);
        ManagerUI._instance.OpenPopupMoleCatch();
    }

    public int GetEventIndex()
    {
        return eventIndex;
    }

    public static int GetResourceIndex()
    {
        if (instance == null)
            return 1;

        return instance.resourceIndex;
    }

    public static long GetEndTime()
    {
        if (instance == null)
            return 0;

        return instance.endTs;
    }

    public int GetWaveIndex()
    {
        return this.userState.currentWave;
    }

    public int GetWaveCount()
    {
        return ServerContents.MoleCatchChapter.Count;
    }

    public static bool CheckStartable()
    {
        if(ServerRepos.UserMoleCatchChapters != null && ServerContents.MoleCatchChapter != null && GetEventState() == EventState.ENDED)
        {
            return true;
        }

        return GetEventState() == EventState.RUNNING;
    }

    public const string OpenSceneKey = "molescene";

    public static bool CheckPlayStartCutScene()
    {
        if (instance == null)
            return false;

        int resIndex = instance.eventIndex;
        bool showScene = true;
        HashSet<int> showedScenes = new HashSet<int>();

        if (PlayerPrefs.HasKey(OpenSceneKey))
        {
            var tmpList = JsonFx.Json.JsonReader.Deserialize<int[]>(PlayerPrefs.GetString(OpenSceneKey));
            for (int i = 0; i < tmpList.Length; ++i)
                showedScenes.Add(tmpList[i]);
        }

        if( showedScenes.Contains(resIndex) )
        {
            showScene = false;
        }

        if( instance != null )
        {
            // 플레이 횟수가 있는 경우에는 씬 보여줄 필요가 없다
            if (instance.CheckNoPlay() == false)
                showScene = false;
        }

        return showScene;
    }

    public static void MarkCutScenePlayed()
    {
        if (instance == null)
            return;

        int resIndex = instance.eventIndex;
        HashSet<int> showedScenes = new HashSet<int>();

        if (PlayerPrefs.HasKey(OpenSceneKey))
        {
            var tmpList = JsonFx.Json.JsonReader.Deserialize<int[]>(PlayerPrefs.GetString(OpenSceneKey));
            for (int i = 0; i < tmpList.Length; ++i)
                showedScenes.Add(tmpList[i]);
        }
        showedScenes.Add(resIndex);

        PlayerPrefs.SetString(OpenSceneKey, JsonFx.Json.JsonWriter.Serialize(showedScenes));

        return;
    }

    public static void Init()
    {
        if (!CheckStartable())
            return;

        if (instance != null)
            return;

        Global._instance.gameObject.AddMissingComponent<ManagerMoleCatch>();
        if (instance == null)
            return; // PANIC: 윗줄에서 awake 불려야되는데?!
        ManagerEvent.instance.RegisterEvent(instance);

        instance.eventState = GetEventState();
        instance.SyncFromServer();
    }

    public static void UpdateEventState()
    {
        if( instance != null )
            instance.eventState = GetEventState();
    }

    static EventState GetEventState()
    {
        if (ServerContents.MoleCatchChapter == null)
            return EventState.NOT_STARTED;

        if (ServerContents.MoleCatchChapter.ContainsKey(1) == false)
        {   // PANIC: 뭔 상황인지 모르겠는데 여튼 이런상황은 있으면 안됨
            return EventState.NOT_STARTED;
        }

        if(GameData.User.missionCnt <= ManagerLobby._missionThreshold_eventstageOpen_noticeOpen_packageshopOpen)
            return EventState.NOT_STARTED;

        if ( Global.LeftTime(ServerContents.MoleCatchChapter[1].start_ts) > 0 )
        {
            return EventState.WAIT_FOR_START;
        }

        if ( Global.LeftTime(ServerContents.MoleCatchChapter[1].end_ts) < 0 )
        {
            return EventState.ENDED;
        }

        return  EventState.RUNNING;
    }

    public bool CheckNoPlay()
    {
        for(int i = 0; i < ServerRepos.UserMoleCatchStages.Count; ++i)
        {
            if (ServerRepos.UserMoleCatchStages[i].play > 0)
                return false;
        }
        return true;

    }

    public static bool IsActiveEvent()
    {
        if( instance == null )
        {
            return false;
        }

        return instance.eventState == EventState.RUNNING;
    }

    void LoadContentsData()
    {
        this.eventIndex = ServerContents.MoleCatchChapter[1].event_index;
        this.resourceIndex = ServerContents.MoleCatchChapter[1].resource_index;


        this.waveData.Clear();
        foreach (var contentsChapter in ServerContents.MoleCatchChapter)
        {
            if (contentsChapter.Value.end_ts > endTs)
                endTs = contentsChapter.Value.end_ts;

            var waveInstance = new WaveData();

            for (int i = 0; i < contentsChapter.Value.maps.Count; ++i)
            {
                waveInstance.stageMap.Add(i + 1, contentsChapter.Value.maps[i]);
            }

            for(int i = 0; i < contentsChapter.Value.hard_stages.Count; ++i)
            {
                waveInstance.hardMap.Add(contentsChapter.Value.hard_stages[i]);
            }

            waveInstance.stageCount = contentsChapter.Value.stage_count;
            for(int i = 0; i < contentsChapter.Value.stage_count; ++i)
            {
                waveInstance.stageVer.Add(i + 1, contentsChapter.Value.stageVersions[i]);
            }

            /*if( ServerContents.MoleCatchReward.ContainsKey(eventIndex) )
            {*/
                var rewards_byEventIdx = ServerContents.MoleCatchReward;
                var waveReward = rewards_byEventIdx[contentsChapter.Value.chapter];
                foreach (var rew in waveReward)
                {
                    RewardData rewData = new RewardData();
                    rewData.requiredStages = rew.Value.condition;
                    rewData.reward = rew.Value.rewards[0];
                    rewData.rewardIndex = rew.Value.reward_index;
                    rewData.isWaveReward = rewData.requiredStages.Count == waveInstance.stageMap.Count;

                    waveInstance.rewards.Add(rewData.rewardIndex, rewData);
                }
            /*}*/
            

            waveData.Add(contentsChapter.Value.chapter, waveInstance);
        }

    }

    void SyncFromServer()
    {
        LoadContentsData();

        userState.currentWave = 1;
        for(int i = 0; i < ServerRepos.UserMoleCatchChapters.Count; ++i)
        {
            var userChap = ServerRepos.UserMoleCatchChapters[i];
            if (userChap.state == 2 && i + 1 != ServerRepos.UserMoleCatchChapters.Count)    // 올클리어 상태일 때는 어쨌든 마지막 웨이브로 처리해줄수밖에 없음
                continue;

            userState.currentWave = userChap.chapter;

            SyncUpMoles();

            foreach( var stageUserData in ServerRepos.UserMoleCatchStages )
            {
                if (stageUserData.chapter == userState.currentWave)
                {
                    if( stageUserData.score > 0 || waveData[userState.currentWave].stageMap[stageUserData.stage] == 0 )
                    {
                        userState.clearedStages.Add(stageUserData.stage);
                    }
                }
                    
            }

            break;
        }

        SyncRewardList();

        SyncClearedStageList();

    }

    public List<int> GetLeftStages()
    {
        List<int> ret = new List<int>();
        foreach(var stage in this.waveData[userState.currentWave].stageMap )
        {
            if( this.userState.clearedStages.Contains(stage.Key ) == false)
            {
                ret.Add(stage.Key);
            }
        }

        return ret;
    }

    public List<int> GetAllStages()
    {
        List<int> ret = new List<int>();
        foreach (var stage in this.waveData[userState.currentWave].stageMap)
        {
            ret.Add(stage.Key);
        }

        return ret;
    }

    public List<int> GetClearedStages()
    {
        List<int> ret = new List<int>();
        foreach (var stage in userState.clearedStages)
        {
            ret.Add(stage);
        }

        return ret;
    }

    public List<int> GetOpenedStages()
    {
        return userState.upMoles;
    }

    public void SyncUpMoles()
    {
        var userChap = ServerRepos.UserMoleCatchChapters.Find( x => x.chapter == userState.currentWave);
        userState.upMoles.Clear();
        for (int i = 0; i < userChap.open_stages.Count; ++i)
        {
            userState.upMoles.Add(userChap.open_stages[i]);
        }
    }


    // 이번에 클리어된 스테이지
    public int GetNowClearedStage()
    {
        var userWaveData = ServerRepos.UserMoleCatchChapters.Find(x => x.chapter == userState.currentWave);

        foreach (var stageUserData in ServerRepos.UserMoleCatchStages)
        {
            if (stageUserData.chapter == userState.currentWave && stageUserData.score > 0 && userState.clearedStages.Contains(stageUserData.stage) == false)
                return stageUserData.stage;
        }
        return 0;
    }

    public void SyncClearedStageList()
    {
        this.userState.clearedStages.Clear();
        for(int i = 0; i < ServerRepos.UserMoleCatchStages.Count; ++i)
        {
            var moleStage = ServerRepos.UserMoleCatchStages[i];
            if( userState.currentWave == moleStage.chapter )
            {
                if (moleStage.score > 0 || waveData[userState.currentWave].stageMap[moleStage.stage] == 0)
                {
                    this.userState.clearedStages.Add(moleStage.stage);
                }
            }
            
        }
    }

    public int GetMapIndex(int stageIndex)
    {
        if (this.waveData[userState.currentWave].stageMap.ContainsKey(stageIndex))
        {
            return this.waveData[userState.currentWave].stageMap[stageIndex];
        }
        else
            return 0;
    }

    public bool IsClearedStage(int stageIndex)
    {
        return this.userState.clearedStages.Contains(stageIndex);
    }

    public bool IsFreeStage(int stageIndex)
    {
        if (this.waveData[userState.currentWave].stageMap.ContainsKey(stageIndex))
        {
            return this.waveData[userState.currentWave].stageMap[stageIndex] == 0;
        }
        else return false;
    }

    public bool IsHardStage(int stageIndex)
    {
        return waveData[userState.currentWave].hardMap.Contains(stageIndex);
    }


    public List<int> GetIncompletedRewards()
    {
        return userState.leftRewards;
    }

    // 이번에 완료된 보상 리스트
    public List<int> GetNowCompletedRewardList()
    {
        List<int> outList = new List<int>();
        for (int i = 0; i < userState.leftRewards.Count; ++i)
        {
            bool completed = true;
            var rewardInfo = waveData[userState.currentWave].rewards[userState.leftRewards[i]];
            for( int j = 0; j < rewardInfo.requiredStages.Count; ++j )
            {
                if (userState.clearedStages.Contains(rewardInfo.requiredStages[j]))
                    continue;

                completed = false;
                break;
            }

            if (completed)
                outList.Add(rewardInfo.rewardIndex);
        }
        return outList;
    }

    public void SyncRewardList()
    {
        userState.leftRewards.Clear();
        foreach(var reward in waveData[userState.currentWave].rewards)
        {
            var userWaveData = ServerRepos.UserMoleCatchChapters.Find(x => x.chapter == userState.currentWave);
            if (userWaveData.get_rewards.Contains(reward.Value.rewardIndex) == false)
            {
                userState.leftRewards.Add(reward.Value.rewardIndex);
            }
        }
    }

    public RewardData GetRewardInfo(int rewardIndex)
    {
        var rewardInfo = waveData[userState.currentWave].rewards[rewardIndex];
        return rewardInfo;
    }

    //현재 웨이브 내의 모든 스테이지가 클리어되었는지
    public bool IsWaveStageAllCleared()
    {
        int stageCount = this.waveData[this.userState.currentWave].stageCount;

        for (int i = 0; i < stageCount; ++i)
        {
            if (this.userState.clearedStages.Contains(i + 1))
                continue;
            else
                return false;
        }

        return true;
    }

    // 웨이브가 끝장났는지
    // 웨이브가 끝난게 체크돼서 다음 웨이브로 갈 때는 SyncFromServer 불러주면 됨
    public bool IsWaveCompleted()
    {
        var userWaveData = ServerRepos.UserMoleCatchChapters.Find(x => x.chapter == userState.currentWave);
        if (userWaveData.state == 2)
            return true;
        else return false;
    }

    public bool NextWaveExist()
    {
        var nextChapterExist = ServerContents.MoleCatchChapter.ContainsKey(userState.currentWave + 1);
        return nextChapterExist;
    }

    public void SyncWave()
    {
        userState.currentWave = 1;
        for(int i = 0; i < ServerRepos.UserMoleCatchChapters.Count; ++i)
        {
            var userChap = ServerRepos.UserMoleCatchChapters[i];

            if (userChap.state == 2 && i + 1 != ServerRepos.UserMoleCatchChapters.Count)    // 올클리어 상태일 때는 어쨌든 마지막 웨이브로 처리해줄수밖에 없음
                continue;

            userState.currentWave = userChap.chapter;

            SyncUpMoles();
            foreach (var stageUserData in ServerRepos.UserMoleCatchStages)
            {
                if (stageUserData.chapter == userState.currentWave)
                {
                    if (stageUserData.score > 0 || waveData[userState.currentWave].stageMap[stageUserData.stage] == 0)
                    {
                        userState.clearedStages.Add(stageUserData.stage);
                    }
                }

            }

            break;
        }

        SyncRewardList();

        SyncClearedStageList();
    }

    public void OpenReady(int stageIndex, Method.FunctionVoid startCallback)
    {
        Global.SetGameType_MoleCatch(this.eventIndex, this.userState.currentWave, stageIndex);

        ManagerUI._instance.OpenPopupReadyMoleCatch(startCallback);
    }

    public static bool IsRewardPeriod()
    {
        return ServerContents.MoleCatchChapter != null && ServerRepos.UserMoleCatchChapters != null && ServerRepos.UserMoleCatchChapters.Count > 0 && 
            GetEventState() == EventState.ENDED;
    }

    public static IEnumerator RequestUnclaimedRewards()
    {
        bool claimed = false;

        if (ServerRepos.UserMoleCatchChapters == null)
            yield break;

        //보상 정보
        Protocol.AppliedRewardSet rewardSet = null;

        List<int> chapterList = new List<int>();
        foreach (var chap in ServerRepos.UserMoleCatchChapters)
        {
            chapterList.Add(chap.chapter);

        }

        for(int i = 0; i < chapterList.Count; ++i)
        {
            var chap = ServerRepos.UserMoleCatchChapters.Find( x => {return chapterList[i] == x.chapter; } );
            if (chap == null) 
                continue;

            bool ret = false;
            bool unclaimedExist = false;

            if (chap.state == 2)
                continue;

            if (ServerContents.MoleCatchReward.ContainsKey(chap.chapter))
            {
                var rewards_byEventIdx = ServerContents.MoleCatchReward;
                var chapContents = ServerContents.MoleCatchChapter[chap.chapter];
                var waveReward = rewards_byEventIdx[chap.chapter];
                
                foreach (var rew in waveReward)
                {
                    if (chap.get_rewards.Contains(rew.Value.reward_index))
                        continue;

                    bool rewGet = true;
                    foreach(var rewCondStage in rew.Value.condition )
                    {
                        var stageInfo = ServerRepos.UserMoleCatchStages.Find(x => x.stage == rewCondStage && x.chapter == chap.chapter);
                        if (stageInfo != null && (stageInfo.score > 0 || (chapContents.maps[stageInfo.stage - 1] == 0 && stageInfo.play > 0)))
                        {

                        }
                        else
                        {
                            rewGet = false;
                            break;
                        }
                    }
                    if( rewGet )
                        unclaimedExist = true;
                }
            }

            if (unclaimedExist)
            {
                chap.state = 2; // 앱 기동 후 두번은 안부르게

                ServerAPI.MoleCatchReward(chap.chapter, chap.event_index, 
                    (resp) =>
                    {
                        ret = true;
                        if( resp.IsSuccess )
                        {
                            //보상 적용
                            rewardSet = resp.clearReward;

                            if ( resp.clearReward.directApplied?.Count > 0 || resp.clearReward.mailReceived?.Length > 0 )
                            {
                                claimed = true;
                                LogReward(resp.rewards, chap.event_index, chap.chapter);
                                OnGetRewardList(resp);
                            }
                        }
                    } );

                while (ret == false)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        if (claimed)
        {
            //수령하지 않은 두더지 이벤트 보상 지급 팝업
            if (rewardSet != null)
            {
                bool isGetReward = false;

                //ManagerUI._instance.OpenPopupGetRewardAlarm
                //    (Global._instance.GetString("p_mc_3"),
                //    () => { isGetReward = true; },
                //    rewardSet);

                ManagerUI._instance.OpenPopup_LobbyPhase<UIPopupGetRewardAlarm>((popup) =>
                {
                    popup.InitPopup(Global._instance.GetString("p_mc_3"), rewardSet);
                }, () => isGetReward = true);

                //보상 팝업 종료될 때까지 대기.
                yield return new WaitUntil(() => isGetReward == true);
            }
        }
    }

    public void LogWaveEnd()
    {
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                        (
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.WHAC_A_MOLE,
                           "MOLE_EVENT_" + eventIndex.ToString() + "_" + userState.currentWave.ToString(),
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                        );
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
    }

    public void LogAllWaveEnd()
    {
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                        (
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.WHAC_A_MOLE,
                           "MOLE_EVENT_" + eventIndex.ToString(),
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.FIRST_CLEAR
                        );

        int playTotal = 0;
        for(int i = 0; i < ServerRepos.UserMoleCatchStages.Count; ++i)
        {
            playTotal += ServerRepos.UserMoleCatchStages[i].play;
        }

        achieve.L_NUM1 = playTotal;
        var doc = JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);

    }

    public static void LogReward(List<Reward> rewards, int eventIndex, int waveIndex)
    {
        if( rewards == null )
            return;

        for (int i = 0; i < rewards.Count; ++i)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                           rewards[i].type, 
                           rewards[i].value,
                           ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_EVENT_REWARD,
                           ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_OTHER,
                           "MOLE_EVENT_" + eventIndex.ToString() + "_" + waveIndex.ToString()
                           );
        }
    }


    public static void OnGetRewardList(Protocol.MoleCatchRewardResp resp)
    {
        if (resp.IsSuccess == false)
            return;

        string systemTitle = Global._instance.GetString("p_t_4");

        bool needUpdateTopAsset = false;
        bool needUpdateMaterial = false;

        for (int i = 0; i < resp.rewards.Count; ++i)
        {
            var item = resp.rewards[i];
            RewardType rewardType = (RewardType)item.type;

            switch( rewardType )
            {
                case RewardType.clover:
                case RewardType.coin:
                case RewardType.jewel:
                case RewardType.star:
                case RewardType.wing:
                case RewardType.expBall:
                    {
                        needUpdateTopAsset = true;
                    }
                    break;
                case RewardType.flower:
                    {
                        Global.flower = (int)GameData.User.flower;
                        ManagerUI._instance.SettingRankingPokoYura();
                    }
                    break;

                // 이 아래에 있는 내용들은 두더지에서는 전부 우편함으로 가기 때문에 업데이트 필요가 없음

                case RewardType.boxSmall:
                case RewardType.boxMiddle:
                case RewardType.boxBig:
                    {
                        ManagerLobby._instance.ReMakeGiftbox();

                        if(resp.giftBoxes != null )
                        {
                            for (var j = 0; j < resp.giftBoxes.Count; j++)
                            {
                                LocalNotification.GiftBoxNotification(resp.giftBoxes[j].index, (int)resp.giftBoxes[j].openTimer - (int)GameData.GetTime());
                            }
                        }
                    }
                    break;



                //case RewardType.housing:
                //    {
                //        PlusHousingModelData.SetUserData();
                //    }
                //    break;
                //case RewardType.toy:
                //    {
                //        PokoyuraData.SetUserData();
                //        ManagerLobby._instance.ReMakePokoyura();
                //    }
                //    break;
                //case RewardType.animal:
                //    {
                //        ManagerAdventure.UserDataAnimal getAnimal = new ManagerAdventure.UserDataAnimal()
                //        {
                //            animalIdx = resp.userAnimal.animalId,
                //            exp = resp.userAnimal.exp,
                //            gettime = 0,
                //            grade = resp.userAnimal.grade,
                //            level = resp.userAnimal.level,
                //            overlap = resp.userAnimal.Overlap
                //        };

                //        ManagerAdventure.OnInit((b) =>
                //        {
                //            var newAnimal = ManagerAdventure.User.GetAnimalInstance(getAnimal);
                //            ManagerAdventure.User.SyncFromServer_Animal();
                //            ManagerAIAnimal.Sync();

                //            UIPopupAdventureSummonAction summonPopup = ManagerUI._instance.OpenPopupStageAdventureSummonAction(null, newAnimal, UIPopupAdventureSummonAction.SummonType.TICKET, new List<Reward>(), null);
                //        });
                //    }
                //    break;
                //case RewardType.costume:
                //    {
                //        ManagerUI._instance.ClosePopupAndOpenPopupCostume();
                //        UIDiaryController._instance.UpdateCostumeData();
                //    }
                //    break;
                default:
                    {
                        if (rewardType >= RewardType.material)
                        {
                            needUpdateMaterial = true;
                            
                            break;
                        }
                    }
                    break;
            }
        }

        if (needUpdateTopAsset && ManagerUI._instance != null)
            ManagerUI._instance.SyncTopUIAssets();

        if (needUpdateMaterial)
        {
            MaterialData.SetUserData();
            UIDiaryController._instance.UpdateProgressHousingData();
        }

    }

    public GameEventType GetEventType()
    {
        return GameEventType.MOLE_CATCH;
    }

    public void OnLobbyStart()
    {
        if (ManagerMoleCatch.IsActiveEvent())
        {
            ManagerMoleCatch.UpdateEventState();
            if (ManagerMoleCatch.IsActiveEvent() == false && Global.GameType == GameType.MOLE_CATCH)
            {
                Global.SetGameType_NormalGame();
            }
        }

    }

    public static IEnumerator OnLobbyLoadDisabled()
    {
        if (ManagerMoleCatch.IsActiveEvent() == false)
        {
            if (PlayerPrefs.HasKey(ManagerMoleCatch.OpenSceneKey))
            {
                ManagerLobby._instance.PlayExtendTriggerFinish("off_mole");
            }
            yield break;
        }
    }

    public IEnumerator OnLobbyObjectLoadPhase()
    {
        if(!ManagerMoleCatch.IsActiveEvent() )
            yield break;

        int moleResIndex = ManagerMoleCatch.GetResourceIndex();
        string prefabName = string.Format("mole_entry", moleResIndex);
        string bundleName = string.Format("mole_v2_{0}", moleResIndex);
        GameObject obj = null;
        if (Global.LoadFromInternal)
        {
#if UNITY_EDITOR
            string path = "Assets/5_OutResource/MoleCatch/" + bundleName + "/" + prefabName + ".prefab";
            GameObject BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (BundleObject != null)
            {
                obj = ManagerLobby.NewObject(BundleObject);

                AreaBase areaBase = BundleObject.GetComponent<AreaBase>();
                if (areaBase)
                {
                    ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                }
            }
#endif
        }
        else
        {
            IEnumerator e = ManagerAssetLoader._instance.AssetBundleLoader(bundleName);
            while (e.MoveNext())
                yield return e.Current;
            if (e.Current != null)
            {
                AssetBundle assetBundle = e.Current as AssetBundle;
                if (assetBundle != null)
                {
                    GameObject objn = assetBundle.LoadAsset<GameObject>(prefabName);
                    obj = ManagerLobby.NewObject(objn);

                    AreaBase areaBase = objn.GetComponent<AreaBase>();
                    if (areaBase)
                    {
                        ManagerCharacter._instance.AddLoadList(areaBase._characters, areaBase._costumeCharacters, areaBase._live2dChars);
                    }
                }
            }
        }

        yield return ManagerCharacter._instance.LoadCharacters();

        if (obj != null)
        {
            MoleCatchEvent e = obj.GetComponent<MoleCatchEvent>();
            ManagerArea._instance.RegisterEventLobbyObject(e);
        }
    }
    IEnumerator IEventBase.OnLobbyObjectLoadPhase_Outland()
    {
        yield break;
    }

    public void OnIconPhase()
    {
        if (IsActiveEvent())
        {
            var eventData = new SideIcon.UseResourceEvent(eventIndex, resourceIndex, endTs);

            SideIcon.Maker.MakeIcon<SideIcon.IconMoleCatchEvent>(
                scrollBar: ManagerUI._instance.ScrollbarRight,
                init: (icon) =>
                {
                    icon.Init(eventData);
                    this.icon = icon;
                });
        }
    }

    public void OnTutorialPhase()
    {

    }

    public IEnumerator OnRewardPhase()
    {
        if (ManagerMoleCatch.IsRewardPeriod())
        {
            yield return ManagerMoleCatch.RequestUnclaimedRewards();
        }

    }

    IEnumerator IEventBase.OnPostLobbyEnter()
    {
        yield break;
    }    

    IEnumerator IEventBase.OnTutorialPhase()
    {
        yield break;

    }

    IEnumerator IEventBase.OnLobbyScenePhase()
    {
        ManagerMoleCatch.MarkCutScenePlayed();
        yield return ManagerLobby._instance.WaitForSceneEnd(ManagerArea._instance.GetEventLobbyObject(GameEventType.MOLE_CATCH).GetAreaBase(), 1);
    }

    bool IEventBase.OnTutorialCheck()
    {
        return false;
    }

    bool IEventBase.OnLobbySceneCheck()
    {
        return ManagerArea._instance.GetEventLobbyObject(GameEventType.MOLE_CATCH) != null && ManagerMoleCatch.CheckPlayStartCutScene();
    }

    void IEventBase.OnBundleLoadPhase(List<ManagerAssetLoader.BundleRequest> loadList, System.Action<ManagerAssetLoader.ResultCode, string> failCallback)
    {
    }

    public bool IsPlayCurrentVersion()
    {
        if (ServerRepos.UserMoleCatchStages != null && ServerRepos.UserMoleCatchStages.FindIndex(x=>x.event_index == eventIndex) == -1)
            return false;
        else 
            return true;
    }
}
