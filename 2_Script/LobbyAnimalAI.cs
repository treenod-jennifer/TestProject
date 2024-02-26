using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyAnimalAI : MonoBehaviour {
    int lobbyCharIdx;
    int costumeIdx;
    public LobbyAIHints aiHint = null;

    public AIGoalType currentGoal;
    public CharacterNPC agent;

    public class LobbyAnimalReward
    {
        public int animalIdx = -1;

        public UILobbyRewardChat rewardChat = null;

        public long getRewardTs;
        public List<Reward> rewards;
    }

    LobbyAnimalReward rewardModule = null;
    NPCHost npcHost = null;

    public Method.FunctionTemplate<LobbyAnimalAI> tapCallback;

    Coroutine currentRoutine;
    public bool passive = false;

    float monologCountdown = 3f;

    [Serializable]
    public class Circle
    {
        public Vector3 center;
        public float radius;
    }

    public enum AIGoalType
    {
        Idle,       // 생각이 없다
        OnScene,
        Perform,
        Roaming,    // 방황한다
        Talking,
        WaitForHarvest,
    }

    public enum AIActionType
    {
        Wait,
        Move,
    }
    
    public int LobbyCharIdx { get { return lobbyCharIdx; } }
    public int CostumeIdx { get { return costumeIdx; } }
    

    internal void OnTap()
    {
        agent.TouchBounce();

        tapCallback(this);
        monologCountdown = aiHint.monologInterval.GetInnerValue();
    }

    void OnTap_RewardAnimal(LobbyAnimalAI ai)
    {
        if( rewardModule == null )
            return;

        ManagerUI._instance.MakeLobbyGuestIcon();

        if (rewardModule.rewardChat != null)
        {
            rewardModule.rewardChat.OnTap();
            return;
        }
        else
        {
            if( passive )
            {
                agent.TouchAction();
                return;
            }
            var charImportData = ManagerCharacter._instance.GetCharacter(this.lobbyCharIdx, costumeIdx);
            string speechTxt = "";
            if( charImportData.aiHint != null )
            {
                if(  charImportData.aiHint.touchScripts != null )
                {
                    var txtKey = charImportData.aiHint.touchScripts[UnityEngine.Random.Range(0, charImportData.aiHint.touchScripts.Count)];
                    speechTxt = ManagerAdventure.instance.GetString(txtKey);
                }
            }
            else
            {
                string[] str = new string[3] { "a{0}_l_1", "a{0}_l_2", "a{0}_l_3" };
                speechTxt = ManagerAdventure.instance.GetString(string.Format(str[UnityEngine.Random.Range(0, str.Length)], this.rewardModule.animalIdx));
            }
            
            speechTxt += "\n[c][1F6898FF](" + Global.GetTimeText_HHMMSS(this.rewardModule.getRewardTs) + ")[-][/c]";
            //var speech = ManagerSound._instance._lobbySpeechBank.GetSpeech(agent._type);
            agent.MakeSpeech(speechTxt, AudioLobby.DEFAULT_AUDIO, true);
        }
    }

    private void OnDestroy()
    {
        if (rewardModule != null)
        {
            if (rewardModule.rewardChat != null)
            {
                Destroy(rewardModule.rewardChat.gameObject);
                rewardModule.rewardChat = null;
            }
        }

        if ( npcHost != null )
        {
            npcHost.OnNPCDestroyed(this);
        }

        if (ManagerAIAnimal.instance != null)
        {
            ManagerAIAnimal.instance.Unregister(lobbyCharIdx);
        }
    }

    bool readyForAction = true;

    // Use this for initialization
    void Start () {

        currentRoutine = StartCoroutine(CoWait(3.0f));

    }
	
	// Update is called once per frame
	void Update ()
    {
        if(readyForAction)
        {
            if ( this.rewardModule != null && 
                (this.rewardModule.rewardChat == null && Global.LeftTime(rewardModule.getRewardTs) < 0))
            {
                readyForAction = false;
                rewardModule.getRewardTs += int.MaxValue;

                var genPos = agent.gameObject.transform.position;
                genPos.y = agent._heightOffset + 7.0f;
                rewardModule.rewardChat = UILobbyRewardChat.MakeLobbyRewardIcon(this.transform, this.rewardModule.animalIdx, this.rewardModule.rewards[0], 1.5f);
                rewardModule.rewardChat.heightOffset = agent.GetBubbleHeightOffset();
            }
            else if( passive == false)
            {
                int behaviorChance = UnityEngine.Random.Range(0, 100);

                // 룰렛코드 짜기 귀찮아서 일단 이렇게 처리했는데, 많아지면 룰렛코드 제대로 짜야됨
                if(behaviorChance < aiHint.behaviourRatio.wait )
                {
                    currentRoutine = StartCoroutine(CoWait(UnityEngine.Random.Range(aiHint.waitTimeRange.min, aiHint.waitTimeRange.max)));
                }
                else if (behaviorChance < aiHint.behaviourRatio.wait + aiHint.behaviourRatio.walk)
                {
                    currentRoutine = StartCoroutine(CoRoaming(false, UnityEngine.Random.Range(aiHint.walkRange.min, aiHint.walkRange.max)));
                }
                else if (behaviorChance < aiHint.behaviourRatio.wait + aiHint.behaviourRatio.walk + aiHint.behaviourRatio.run)
                {
                    currentRoutine = StartCoroutine(CoRoaming(true, UnityEngine.Random.Range(aiHint.runRange.min, aiHint.runRange.max)));
                }
            }
        }
        else
        {
            if ( this.rewardModule != null && 
                (this.rewardModule.rewardChat == null && Global.LeftTime(this.rewardModule.getRewardTs) < 0 && stopRoutine == false) )
            {
                stopRoutine = true;
            }
        }

        if(aiHint != null && (rewardModule == null || rewardModule.rewardChat == null ) )
        {
            monologCountdown -= Time.deltaTime;
            if (monologCountdown <= 0f)
            {
                monologCountdown = this.aiHint.monologInterval.GetInnerValue();

                if (aiHint.monologs != null && aiHint.monologs.Count > 0)
                {
                    string speechTxt;
                    var txtKey = aiHint.monologs[UnityEngine.Random.Range(0, aiHint.monologs.Count)];
                    speechTxt = Global._instance.GetString(txtKey);
                    
                    agent.MakeSpeech(speechTxt, AudioLobby.DEFAULT_AUDIO, true);
                }
            }
        }

    }

    bool stopRoutine = false;
    IEnumerator CoRoaming(bool run, float runRange)
    {
        stopRoutine = false;
        readyForAction = false;

        var destination = Vector3.one;
        bool pathGet = false;
        Path path = null;
        Vector3 startPos = agent.gameObject.transform.position;

        while (pathGet == false)
        {
            Circle roamingCircle = aiHint.roamingArea[UnityEngine.Random.Range(0, aiHint.roamingArea.Count)];

            if (rewardModule != null)
            {
                var animalPosData = ManagerAIAnimal.instance.GetAnimalPositionData(rewardModule.animalIdx, aiHint);
                roamingCircle = animalPosData.roamingArea[UnityEngine.Random.Range(0, animalPosData.roamingArea.Count)];
            }

            Vector2 rRange = UnityEngine.Random.insideUnitCircle * roamingCircle.radius;
            destination = roamingCircle.center + new Vector3(rRange.x, 0, rRange.y);

            var posDiff = (destination - agent.gameObject.transform.position);
            if(run == false)
            {
                agent._ai.canRun = false;
            }

            bool returned = false;
            agent.StartPath(destination, 
                (p) => 
                {
                    returned = true;
                    if( p != null && p.vectorPath.Count > 0 )
                    {
                        path = p;
                        pathGet = true;
                    }
                        
                });

            while (returned == false)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if(stopRoutine)
            {
                agent.StopPath();
                agent._ai.canRun = true;
                readyForAction = true;
                yield break;
            }
        }
        
        while(true && stopRoutine == false)
        {
            Vector3 currPos = agent.gameObject.transform.position;
            if( (currPos - startPos).sqrMagnitude > runRange * runRange )
            {
                agent.StopPath();
                break;
            }

            if ( agent._path == null)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }

        if (stopRoutine)
        {
            agent.StopPath();
            agent._ai.canRun = true;
            readyForAction = true;
            yield break;
        }

        agent._ai.canRun = true;
        readyForAction = true;
    }

    IEnumerator CoWait(float len)
    {
        readyForAction = false;
        yield return new WaitForSeconds(len);
        readyForAction = true;
    }

    internal void OnReceivedLobbyAnimalReward()
    {
        if( rewardModule == null )
            return;

        readyForAction = true;

        var animalData = ServerContents.AdvAnimals[rewardModule.animalIdx];
        var rewards = animalData.lobby_rewards;
        var userAnimal = ServerRepos.UserAdventureLobbyAnimals.Find(x => x.animalId == rewardModule.animalIdx);
        if( userAnimal != null )
        {
            rewardModule.getRewardTs = userAnimal.get_reward_ts;
            SetNotification();
        }
    }

    internal void Init(CharacterNPC agent, int lobbyCharIdx, int _costumeIdx)
    {   
        this.agent = agent;
        agent.attachedLobbyAI = this;
        this.lobbyCharIdx = lobbyCharIdx;
        costumeIdx = _costumeIdx;

        var charImportData = ManagerCharacter._instance.GetCharacter(this.lobbyCharIdx, costumeIdx);
        if (charImportData.aiHint == null)
        {
            InitDefaultAIHint();
        }
        else
            aiHint = charImportData.aiHint;

        if (AreaEvent.instance != null)
            passive = AreaEvent.instance._characters.Contains((TypeCharacterType)lobbyCharIdx);
        else
            passive = false;
    }

    public void SetNpcHost(NPCHost h)
    {
        npcHost = h;
        tapCallback += h.OnNPCTap;

        h.OnNPCCreated(this);
    }

    public void SetRewardModule(int animalIdx)
    {
        var animalData = ServerContents.AdvAnimals[animalIdx];
        if (animalData.lobby_rewards.Count > 0)
        {
            rewardModule = new LobbyAnimalReward() { rewards = animalData.lobby_rewards };
            rewardModule.animalIdx = animalIdx;
            tapCallback += OnTap_RewardAnimal;
        }

        var userAnimal = ServerRepos.UserAdventureLobbyAnimals.Find(x => x.animalId == animalIdx);
        if (userAnimal != null)
        {
            if (rewardModule != null)
            {
                rewardModule.getRewardTs = userAnimal.get_reward_ts;
                SetNotification();
            }
        }
    }

    void InitDefaultAIHint()
    {
        aiHint = this.gameObject.AddMissingComponent<LobbyAIHints>();

        if (rewardModule != null)
        {
            string[] str = new string[3] { "a{0}_d_1", "a{0}_d_2", "a{0}_c_1" };
            for (int i = 0; i < 3; ++i)
            {
                aiHint.touchScripts.Add(string.Format(str[UnityEngine.Random.Range(0, str.Length)], this.rewardModule.animalIdx));
            }
        }

        var animalPosData = ManagerAIAnimal.instance.GetAnimalPositionData(rewardModule.animalIdx, aiHint);
        animalPosData.startPos = new Vector3(1.5f, 0, 1f);
        animalPosData.roamingArea.Add(new Circle() { center = new Vector3(0, 0, 0), radius = 30f });
    }

    internal LobbyAnimalReward GetRewardModule()
    {
        return this.rewardModule;
    }

    private void SetNotification()
    {
        if (rewardModule != null)
        {
            int remaining = (int)Global.LeftTime(rewardModule.getRewardTs);

            if (remaining > 0.0f)
            {
                LocalNotification.FriendGiftNotification(rewardModule.animalIdx, remaining);
            }
        }
    }
}
