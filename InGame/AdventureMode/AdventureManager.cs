using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ADVENTURE_STATE
{
    WAIT,
    GET_COMBO,
    ANIMAL_ATT,
    ENEMY_ATT,
    MOVE_WAVE,
    MAKE_BOMB,
}

public class AdventureManager : MonoSingletonOnlyScene<AdventureManager>
{
    [SerializeField]

    ADVENTURE_STATE tempState = ADVENTURE_STATE.WAIT;
    ADVENTURE_STATE state
    {
        get { return tempState; }
        set
        {
            if (tempState != value)
            {
                tempState = value;

                if (NetworkSettings.Instance.buildPhase == NetworkSettings.eBuildPhases.SANDBOX)
                {
                    //GameUIManager.instance.advantureWaveLabel.text = value.ToString();
                }  
            }
        }
    }
    

    public const int ANIMAL_SIZE = 220;

    public const float ANIMAL_POS_Y = 470;
    public const float ADD_POS_Y = 0;//-15f;
    public const float POS_X_INTERVAL = 105f;

    public const float SKILL_ITEM_POS_Y = 250f;
    public const float LIFE_POS_Y = -150;//80f;

    float addPosY2 = 20;

    //게임상태
    public int waveCount = 0;

    public List<InGameAnimal> AnimalLIst = new List<InGameAnimal>();
    public List<InGameEnemy> EnemyLIst = new List<InGameEnemy>();
    public List<GameObject> ObjList = new List<GameObject>();

    public List<InGameSkillItem_Animal> skillItemList = new List<InGameSkillItem_Animal>();

    public GameObject AnimalObj;
    public GameObject EnemyObj;
    public GameObject BossLabelObj;
    public GameObject StartLabelObj;

    //애니메이션커브
    public AnimationCurve animCurveTextPosY;
    public AnimationCurve animCurveTextScale;

    //폭탄게이지    
    //int AdventureGaigeCount = 0;
    //BlockBombType advantureBombType = BlockBombType.BOMB;
    
    //힐연출관련
    public int HealCount = 0;

    //스킬아이콘 셋팅
    public GameObject skillItem_Animal_Obj;
    public GameObject skillItem_Enemy_Obj;

    //보물.
    int treasureCnt = 0;
    int treasureWave = 0;
    int treasureType = 0;
    bool isDropTreasure = false;

    //배속 버튼
    [SerializeField] private UIButtonAdventureSpeedUp speedUpButton;

    public bool isDoAction = false;
    public ADVENTURE_STATE AdventureState
    {
        get { return state; }
    }

    public int TreasureCnt
    {
        get { return treasureCnt; }
        set { treasureCnt = value; }
    }

    public int TreasureType
    {
        get { return treasureType; }
        set { treasureType = value; }
    }

    //초기화
    bool isFinishedInit = false;
    public bool IsFinishedInitAction()
    {
        return isFinishedInit;
    }

    bool enemyAttackPause = false;
    public void PauseEnemyAttactEvent(bool enable)
    {
        enemyAttackPause = enable;
    }

    //콤보
    public List<FlyComboCount> ComboObjList = new List<FlyComboCount>();

    public void InitStage()
    {
        HealCount = 0;
        waveCount = 0;
        FlyAdventureBlock.flyCount = 0;
        FlyComboCount.ComboCount = 0;
        InGameAnimal.isChargingCount = 0;
        
        foreach (var temp in ObjList) Destroy(temp);
        ObjList.Clear();

        //보물정보.
        if (EditManager.instance == null)
            SetStageTreasureInfo();

        //동물데이타 넣기
        AnimalLIst.Clear();
        skillItemList.Clear();
        for (int i = 0; i < 3; i++)
        {
            var animalData = new ManagerAdventure.AnimalInstance();

            if (EditManager.instance == null)
                animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
            else
            {
                if (EditManager.instance.animalList.Count > 0 && EditManager.instance.animalList.Count > i)
                {
                    animalData.idx = EditManager.instance.animalList[i].idx;
                    animalData.level = EditManager.instance.animalList[i].level;
                    animalData.overlap = EditManager.instance.animalList[i].overlap;
                }
                else
                {
                    animalData.idx = i + 1001;
                }
                animalData.attr = 1;
                animalData.hp = 100;
                animalData.atk = 4;
                animalData.skill = i;
            }

            InGameAnimal tempAnimal = NGUITools.AddChild(GameUIManager.instance.Advance_Root, AnimalObj).GetComponent<InGameAnimal>();
            tempAnimal.init(animalData, i);

            AnimalLIst.Add(tempAnimal);
            ObjList.Add(tempAnimal.gameObject);

            //스킬셋팅
            if (animalData.skill != 0)
            {
                InGameSkillItem_Animal tempSkillItem = NGUITools.AddChild(GameUIManager.instance.Advance_Root, skillItem_Animal_Obj).GetComponent<InGameSkillItem_Animal>();
                tempSkillItem.InitSkill(tempAnimal, (BlockColorType)(i + 1));
                skillItemList.Add(tempSkillItem);
                ObjList.Add(tempSkillItem.gameObject);

                tempAnimal.skillItemObj = tempSkillItem.gameObject;
                tempAnimal.skillItem = tempSkillItem;
                tempSkillItem.skillCaster = tempAnimal as InGameSkillCaster;
            }
        }

        if (EditManager.instance != null)
        {
            EnemyLIst.Clear();
            for (int i = 0; i < ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count; i++)
            {
                EnemyInfo enemyInfo = ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[i];

                InGameEnemy tempEnemy = NGUITools.AddChild(GameUIManager.instance.Advance_Root, EnemyObj).GetComponent<InGameEnemy>();
                tempEnemy.init(enemyInfo, i);

                EnemyLIst.Add(tempEnemy);
                ObjList.Add(tempEnemy.gameObject);   
            }
            SetTreasureEnemy();
        }

        GameUIManager.instance.SetAdvantureWave(waveCount + 1, ManagerBlock.instance.stageInfo.battleWaveList.Count);

        SetSpeedUpButton();
    }

    private void SetSpeedUpButton()
    {
        bool isClear = false;

        if(Global.GameType == GameType.ADVENTURE)
        {
            var chapterProgress = ManagerAdventure.User.GetChapterProgress(Global.chapterIndex);
            if (chapterProgress != null)
            {
                if (chapterProgress.stageProgress.TryGetValue(Global.stageIndex, out ManagerAdventure.UserDataStageProgress stageProgress))
                {
                    if (stageProgress.clearLevel != 0)
                    {
                        isClear = true;
                    }
                }
            }
        }
        else if(Global.GameType == GameType.ADVENTURE_EVENT)
        {
            isClear = ManagerAdventure.EventData.IsAdvEventStageCleared(Global.stageIndex);
        }

        speedUpButton.gameObject.SetActive(true);
        speedUpButton.InitButton(!isClear);
    }

    public void StartAdventure()
    {
        isFinishedInit = false;
        StartCoroutine(DoInitAction());
    }

    IEnumerator DoInitAction()
    {
        float waitTimer = 0f;
        while (waitTimer < 0.5f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        //GameObject startLabel = NGUITools.AddChild(GameUIManager.instance.Advance_Root, StartLabelObj);
        //startLabel.transform.localPosition = new Vector3(0, 440, 0);

        GameObject startLabel = NGUITools.AddChild(GameUIManager.instance.Advance_Root, StartLabelObj);
        //bossLabel.transform.localPosition = new Vector3(0, 440,0);

        //동물생성표시
        //적등장
        EnemyLIst.Clear();

        for (int i = 0; i < AnimalLIst.Count; i++)
        {
            AnimalLIst[i].StartAnimal();

            if (i < ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count)
            {
                EnemyInfo enemyInfo = ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[i];

                InGameEnemy tempEnemy = NGUITools.AddChild(GameUIManager.instance.Advance_Root, EnemyObj).GetComponent<InGameEnemy>();
                tempEnemy.init(enemyInfo, i);
                //if (enemyInfo.isBoss)

                EnemyLIst.Add(tempEnemy);
                ObjList.Add(tempEnemy.gameObject);

                waitTimer = 0f;
                while (waitTimer < 0.5f / ( 1+ i))
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }
        }

        GameUIManager.instance.ShowInfo();
        SetTreasureEnemy();
        /*
            for (int i = 0; i < ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count; i++)
            {
                EnemyInfo enemyInfo = ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[i];

                InGameEnemy tempEnemy = NGUITools.AddChild(GameUIManager.instance.Advance_Root, EnemyObj).GetComponent<InGameEnemy>();
                tempEnemy.init(enemyInfo, i);

                EnemyLIst.Add(tempEnemy);
                ObjList.Add(tempEnemy.gameObject);

                waitTimer = 0f;
                while (waitTimer < 0.5f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }
        */
        //waitTimer = 0f;
        while (waitTimer < 0.7f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        Destroy(startLabel);

        if(Global.GameType == GameType.ADVENTURE_EVENT && ManagerAdventure.EventData.GetAdvEventIndex() != 0)
        {
            int count = ManagerAdventure.EventData.GetAdvEventBonusAnimalCount();

            switch (count)
            {
                case 1:
                    MakeBomb(GameManager.instance.GetIngameRandom(0, 2) == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H);
                    break;
                case 2:
                    MakeBomb(GameManager.instance.GetIngameRandom(0, 2) == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H);
                    yield return new WaitForSeconds(0.3f);
                    MakeBomb(BlockBombType.BOMB);
                    break;
                case 3:
                    MakeBomb(GameManager.instance.GetIngameRandom(0, 2) == 0 ? BlockBombType.LINE_V : BlockBombType.LINE_H);
                    yield return new WaitForSeconds(0.3f);
                    MakeBomb(BlockBombType.BOMB);
                    yield return new WaitForSeconds(0.3f);
                    MakeBomb(BlockBombType.RAINBOW);
                    break;
                default:
                    break;
            }
        }

        isFinishedInit = true;
        yield return null;
    }

    private void MakeBomb(BlockBombType bombType)
    {
        BlockBase lineBlock = PosHelper.GetRandomBlock();
        if (lineBlock != null)
        {
            ManagerSound.AudioPlay(AudioInGame.CREAT_BOMB);
            var effectObj = InGameEffectMaker.instance.MakeLastBomb(lineBlock._transform.position);
            Destroy(effectObj, 5.0f);

            lineBlock.bombType = bombType;
            lineBlock.JumpBlock();
            lineBlock.Destroylinker();
        }
    }

    //테스트 폭탄 타입바꾸기
    public int TestBombType = 0;

    public void ChangeBombMode(int temp)
    {
        TestBombType = temp;
    }


    /////////EVENT_ACTION/////////////////
    public bool IsFinishedEventAction()
    {
        return isFinishedEvent;
    }


    bool isFinishedEvent = true;

    public void EventAction()
    {
        isFinishedEvent = false;
        StartCoroutine(DoAttackAnimal());
    }

    IEnumerator DoAttackAnimal()
    {
        float waitTimer = 0;

        while (FlyAdventureBlock.flyCount > 0)
            yield return null;

        while (InGameAnimal.isChargingCount > 0)
        {
            waitTimer = 0f;
            while (waitTimer < 0.5f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        GameUIManager.instance.ShowAdventureDarkBGBlock(true);
        
        state = ADVENTURE_STATE.GET_COMBO;        
        /////콤보모으기
        if (ComboObjList.Count > 0)
        {

            waitTimer = 0f;
            while (waitTimer < 0.3f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
            
            for (int i = 0; i < ComboObjList.Count; i++)
            {
                ComboObjList[i].StartCombo();//.MoveCombo = true;
                yield return null;                

                waitTimer = 0f;
                while (waitTimer < 1f / (4f ))//+ i))// / (i+1))
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }

                AddComboPoint();

                int praiseIndex = i;
                if (praiseIndex >= 4)                
                    praiseIndex = i % 4;

                ManagerSound.AudioPlay((AudioInGame)((int)AudioInGame.PRAISE0 + praiseIndex));
            }
                       
            ComboObjList.Clear();

            while (FlyComboCount.ComboCount > 0)
                yield return null;

            waitTimer = 0f;
            while (waitTimer < 0.6f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }           
        }
        
        while (FlyComboCount.ComboCount > 0)
            yield return null;

        while (HealCount > 0)
            yield return null;

        //혹시 동물이 위치이동중이면 대기후 공격
        foreach (var temp in AnimalLIst)
        {
            while (temp.GetState() == ANIMAL_STATE.CHANGE_POS)
                yield return null;
        }

        state = ADVENTURE_STATE.ANIMAL_ATT;
        ////동물공격////////////////////////////////////
        bool HasAnimalAttPoint = false;
        int posIndex = 0;
        while (true)
        {
            foreach (var temp in AnimalLIst)
            {
                if (temp.pos == posIndex && temp.HasAttackPoint())
                {
                    HasAnimalAttPoint = true;

                    temp.Attack(GetEnemyPos(), temp.data.atkType == 1);
                    //LockBlock(true);

                    while (temp.isAttacking)
                        yield return new WaitForSeconds(0.1f);
                }
            }
            posIndex++;

            if (posIndex >= AnimalLIst.Count)
                break;

            int enemyCount = 0;
            foreach (var enemy in EnemyLIst)
            {
                if (enemy.GetState() != ANIMAL_STATE.DEAD)
                    enemyCount++;                
            }

            if (enemyCount == 0)
                break;
            
            yield return null;
        }

        if (HasAnimalAttPoint)
        {
            waitTimer = 0f;
            while (waitTimer < 0.5f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        foreach (var temp in AnimalLIst)
            temp.ResetAnimal();

        //적카운트내리기//동물 카운트 내리기//////////////////////////////////
        foreach (var enemy in EnemyLIst)
            enemy.AddAttackPoint();

        waitTimer = 0f;
        while (waitTimer < 0.25f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        state = ADVENTURE_STATE.ENEMY_ATT;
        
        while (enemyAttackPause)
            yield return null;

        //적 스킬 사용//
        foreach (var enemy in EnemyLIst)
        {
            if (enemy.UseSkill())
            {
                GameUIManager.instance.ShowAdventureDarkBGBlock(false);
                yield return new WaitUntil(() => enemy.IsSkillEnd);
            }
        }
        GameUIManager.instance.ShowAdventureDarkBGBlock(true);


        ////적공격////////////////////////////////////
        HasAnimalAttPoint = false;

        //공격할 동물리스트만들기 -> 이유는 이번공격으로 인해서 죽는 동물이 있기때문에 따라 리스트만듬
        List<InGameAnimal> tempAnimalList = GetLiveAnimalList();
                
        int animalIndex = 0; //동물순서
        bool deadAnimal = false;

        foreach (var enemy in EnemyLIst)
        {
            if (enemy.IsAttackTurn())
            {
                int totalCount = 0;

                while (true)
                {
                    if (tempAnimalList[animalIndex].GetState() != ANIMAL_STATE.DEAD && tempAnimalList[animalIndex].CheckBeforeDead == false)
                    {
                        enemy.Attack(tempAnimalList[animalIndex].pos, tempAnimalList[animalIndex].data.protectedFromMelee);
                        //LockBlock(true);
                        HasAnimalAttPoint = true;

                        waitTimer = 0f;
                        while (waitTimer < 0.5f)
                        {
                            waitTimer += Global.deltaTimePuzzle;
                            yield return null;
                        }
                        animalIndex++;

                        if (animalIndex >= tempAnimalList.Count)
                            animalIndex = animalIndex % tempAnimalList.Count;

                        break;
                    }

                    animalIndex++;
                    if (animalIndex >= tempAnimalList.Count)
                        animalIndex = animalIndex % tempAnimalList.Count;

                    deadAnimal = true;
                    foreach (var temp in AnimalLIst)
                        if (temp.GetState() != ANIMAL_STATE.DEAD || temp.CheckBeforeDead == false)
                        {
                            deadAnimal = false;
                            break;
                        }

                    if (deadAnimal)
                    {
                        waitTimer = 0f;
                        while (waitTimer < 0.5f)
                        {
                            waitTimer += Global.deltaTimePuzzle;
                            yield return null;
                        }

                        enemy.InitAttack();
                        break;
                    }
                    
                    totalCount++;
                    if (totalCount > 10)
                        break;
                     
                }
            }
        }

        //동물 공격동작 끝났는지 판단
        foreach (var enemy in EnemyLIst)
        {
            while(enemy.GetState() == ANIMAL_STATE.ATTACK)
                    yield return null;
        }

        //게임오버 판정하기////////////////////////////////
        bool IsGameOver = true;
        foreach (var temp in AnimalLIst)
            if (temp.GetState() != ANIMAL_STATE.DEAD) // || temp.CheckBeforeDead == false)
            {
                IsGameOver = false;
                break;
            }

        if (IsGameOver)
        {
            waitTimer = 0f;
            while (waitTimer < 0.5f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            GameManager.instance.StageFail();
            yield break;
        }


        foreach (var animal in AnimalLIst)        
            animal.CountHeart();        


        state = ADVENTURE_STATE.MOVE_WAVE;
        //웨이브 이동///////////////////////////////////////               
        bool WaveFinished = true;
        foreach (var temp in EnemyLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD)            
                WaveFinished = false;            
        }
        
        if (WaveFinished)
        {
            if (waveCount + 1 >= ManagerBlock.instance.stageInfo.battleWaveList.Count)
            {
                waitTimer = 0f;
                while (waitTimer < 2.5f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }

            int treasureCount = 0;
            foreach (var temp in EnemyLIst)
            {
                if (temp.IsDropTreasure)
                    treasureCount++;
                temp.GetTreasure();
            }
            waveCount++;

            if (waveCount >= ManagerBlock.instance.stageInfo.battleWaveList.Count)
            {
                if(treasureCount > 0)
                    yield return new WaitForSeconds(1.0f);

                GameManager.instance.StageClear();

                waitTimer = 0f;
                while (waitTimer < 0.4f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }

                yield break;
            }
            else
            {
                foreach (var temp in AnimalLIst) temp.MoveWave();
                GameUIManager.instance.SetAdvantureWave(waveCount + 1, ManagerBlock.instance.stageInfo.battleWaveList.Count);

                if (ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave > 0)
                {
                    GameUIManager.instance.SetAdvantureBG("Boss_wave", true);

                    waitTimer = 0f;
                    while (waitTimer < 1.533f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }
                    
                    GameUIManager.instance.SetAdvantureBG("Boss_idle", true);
                    GameUIManager.instance.AdventureEffectBG_On(null, null);

                    GameObject bossLabel = NGUITools.AddChild(GameUIManager.instance.Advance_Root, BossLabelObj);
                    ManagerSound.AudioPlay(AudioInGame.BOSS_STAGE);
                    //bossLabel.transform.localPosition = new Vector3(0, 440,0);


                    waitTimer = 0f;
                    while (waitTimer < 2f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }
                    Destroy(bossLabel);
                    GameUIManager.instance.AdventureEffectBG_Off();
                }
                else
                {
                    GameUIManager.instance.SetAdvantureBG("wave", false);

                    waitTimer = 0f;
                    while (waitTimer < 1.533f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }

                    GameUIManager.instance.SetAdvantureBG("idle", true);
                }

                //동물들 기본상태로 바꾸기                
                foreach (var temp in EnemyLIst)Destroy(temp.gameObject);
                EnemyLIst.Clear();

                //웨이브카운트표시               

                //적생성
                for (int i = 0; i < ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count; i++)
                {
                    EnemyInfo enemyInfo = ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[i];
                    InGameEnemy tempEnemy = NGUITools.AddChild(GameUIManager.instance.Advance_Root, EnemyObj).GetComponent<InGameEnemy>();
                    tempEnemy.init(enemyInfo, i);

                    EnemyLIst.Add(tempEnemy);
                    ObjList.Add(tempEnemy.gameObject);

                    Vector3 targetPos = EnemyPos(i);
                    Vector3 startPos = EnemyPos(i) + new Vector3(0, 400f, 0);

                    float timer = 0f;
                    while (true)
                    {
                        timer += Global.deltaTimePuzzle * 4f;
                        if (timer > 1f)
                            timer = 1f;

                        float ratio = Mathf.Sin(timer * Mathf.PI * 0.5f);
                        tempEnemy.gameObject.transform.localPosition = Vector3.Lerp(startPos, targetPos, ratio);

                        if (timer >= 1f)
                            break;

                        yield return null;
                    }
                }

                GameUIManager.instance.ShowInfo();
                SetTreasureEnemy();
            }
            //캐릭터불러오기 등장

            //튜토리얼.
            CheckAndPlayAdventureTutorial();
        }

        //동물, 상태가, wait가 되었는지 확인
        foreach (var temp in AnimalLIst)
        {
            while (temp.GetState() != ANIMAL_STATE.WAIT && temp.GetState() != ANIMAL_STATE.DEAD)
            {
                yield return null;
            }
        }

        GameUIManager.instance.ShowAdventureDarkBGBlock(false);

        isFinishedEvent = true;
        state = ADVENTURE_STATE.WAIT;
        yield return null;
    }

    //죽은 동물 있는 지 확인.
    public bool IsExistDeathAnimal()
    {
        foreach (var animal in AnimalLIst)
        {
            if (animal.GetState() == ANIMAL_STATE.DEAD)
                return true;
        }
        return false;
    }

    //모든 동물이 대기/죽은 상태가 될 때까지 대기하는 코드
    public IEnumerator CoWait_AllAnimalStateWait()
    {
        while (true)
        {
            bool isAllAnimalStateWait = true;
            foreach (var animal in AnimalLIst)
            {
                ANIMAL_STATE aState = animal.GetState();
                if (aState != ANIMAL_STATE.WAIT && aState != ANIMAL_STATE.DEAD)
                {
                    isAllAnimalStateWait = false;
                    break;
                }
            }

            if (isAllAnimalStateWait == false)
                yield return null;
            else
                break;
        }
    }

    //아이템이벤트

    bool IsPlayItemAction = false;
    public void ItemAction()
    {
        if (IsPlayItemAction)
            return;

        IsPlayItemAction = true;
        StartCoroutine(DoAttackAnimalItem());
    }

    IEnumerator DoAttackAnimalItem()
    {
        isDoAction = true;
        float waitTimer = 0;

        while (FlyAdventureBlock.flyCount > 0)
            yield return null;

        while (InGameAnimal.isChargingCount > 0)
        {
            waitTimer = 0f;
            while (waitTimer < 0.3f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        GameUIManager.instance.ShowAdventureDarkBGBlock(true);

        state = ADVENTURE_STATE.GET_COMBO;
        /////콤보모으기
        if (ComboObjList.Count > 0)
        {

            waitTimer = 0f;
            while (waitTimer < 0.3f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            for (int i = 0; i < ComboObjList.Count; i++)
            {
                ComboObjList[i].StartCombo();//.MoveCombo = true;
                yield return null;

                waitTimer = 0f;
                while (waitTimer < 1f / (4f))//+ i))// / (i+1))
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }

                AddComboPoint();

                int praiseIndex = i;
                if (praiseIndex >= 4)
                    praiseIndex = i % 4;

                ManagerSound.AudioPlay((AudioInGame)((int)AudioInGame.PRAISE0 + praiseIndex));
            }

            ComboObjList.Clear();

            while (FlyComboCount.ComboCount > 0)
                yield return null;

            waitTimer = 0f;
            while (waitTimer < 0.6f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        while (FlyComboCount.ComboCount > 0)
            yield return null;

        while (HealCount > 0)
            yield return null;

        //혹시 동물이 위치이동중이면 대기후 공격
        foreach (var temp in AnimalLIst)
        {
            while (temp.GetState() == ANIMAL_STATE.CHANGE_POS)
                yield return null;
        }

        state = ADVENTURE_STATE.ANIMAL_ATT;
        ////동물공격////////////////////////////////////
        bool HasAnimalAttPoint = false;
        int posIndex = 0;
        while (true)
        {
            foreach (var temp in AnimalLIst)
            {
                if (temp.pos == posIndex && temp.HasAttackPoint())
                {
                    HasAnimalAttPoint = true;

                    temp.Attack(GetEnemyPos(), temp.data.atkType == 1);
                    //LockBlock(true);

                    while (temp.isAttacking)
                        yield return new WaitForSeconds(0.1f);
                }
            }
            posIndex++;

            if (posIndex >= AnimalLIst.Count)
                break;

            int enemyCount = 0;
            foreach (var enemy in EnemyLIst)
            {
                if (enemy.GetState() != ANIMAL_STATE.DEAD)
                    enemyCount++;
            }

            if (enemyCount == 0)
                break;

            yield return null;
        }

        if (HasAnimalAttPoint)
        {
            waitTimer = 0f;
            while (waitTimer < 0.5f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        foreach (var temp in AnimalLIst)
            temp.ResetAnimal();


        //게임오버 판정하기////////////////////////////////
        bool IsGameOver = true;
        foreach (var temp in AnimalLIst)
            if (temp.GetState() != ANIMAL_STATE.DEAD) // || temp.CheckBeforeDead == false)
            {
                IsGameOver = false;
                break;
            }

        if (IsGameOver)
        {
            waitTimer = 0f;
            while (waitTimer < 0.5f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }

            GameManager.instance.StageFail();
            yield break;
        }

        /*
        foreach (var animal in AnimalLIst)
            animal.CountHeart();
        */

        state = ADVENTURE_STATE.MOVE_WAVE;
        //웨이브 이동///////////////////////////////////////               
        bool WaveFinished = true;
        foreach (var temp in EnemyLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD)
                WaveFinished = false;
        }

        if (WaveFinished)
        {
            if (waveCount + 1 >= ManagerBlock.instance.stageInfo.battleWaveList.Count)
            {
                waitTimer = 0f;
                while (waitTimer < 2.5f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }
            }

            int treasureCount = 0;
            foreach (var temp in EnemyLIst)
            {
                if (temp.IsDropTreasure)
                    treasureCount++;
                temp.GetTreasure();
            }
            waveCount++;

            if (waveCount >= ManagerBlock.instance.stageInfo.battleWaveList.Count)
            {
                if (treasureCount > 0)
                    yield return new WaitForSeconds(1.0f);

                GameManager.instance.StageClear();

                waitTimer = 0f;
                while (waitTimer < 0.4f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }

                yield break;
            }
            else
            {
                foreach (var temp in AnimalLIst) temp.MoveWave();
                GameUIManager.instance.SetAdvantureWave(waveCount + 1, ManagerBlock.instance.stageInfo.battleWaveList.Count);

                if (ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave > 0)
                {
                    GameUIManager.instance.SetAdvantureBG("Boss_wave", true);

                    waitTimer = 0f;
                    while (waitTimer < 1.533f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }

                    GameUIManager.instance.SetAdvantureBG("Boss_idle", true);
                    GameUIManager.instance.AdventureEffectBG_On(null, null);

                    GameObject bossLabel = NGUITools.AddChild(GameUIManager.instance.Advance_Root, BossLabelObj);
                    ManagerSound.AudioPlay(AudioInGame.BOSS_STAGE);
                    //bossLabel.transform.localPosition = new Vector3(0, 440,0);


                    waitTimer = 0f;
                    while (waitTimer < 2f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }
                    Destroy(bossLabel);
                    GameUIManager.instance.AdventureEffectBG_Off();
                }
                else
                {
                    GameUIManager.instance.SetAdvantureBG("wave", false);

                    waitTimer = 0f;
                    while (waitTimer < 1.533f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }

                    GameUIManager.instance.SetAdvantureBG("idle", true);
                }

                //동물들 기본상태로 바꾸기                
                foreach (var temp in EnemyLIst) Destroy(temp.gameObject);
                EnemyLIst.Clear();

                //웨이브카운트표시               

                //적생성
                for (int i = 0; i < ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count; i++)
                {
                    EnemyInfo enemyInfo = ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList[i];
                    InGameEnemy tempEnemy = NGUITools.AddChild(GameUIManager.instance.Advance_Root, EnemyObj).GetComponent<InGameEnemy>();
                    tempEnemy.init(enemyInfo, i);

                    EnemyLIst.Add(tempEnemy);
                    ObjList.Add(tempEnemy.gameObject);

                    Vector3 targetPos = EnemyPos(i);
                    Vector3 startPos = EnemyPos(i) + new Vector3(0, 400f, 0);

                    float timer = 0f;
                    while (true)
                    {
                        timer += Global.deltaTimePuzzle * 4f;
                        if (timer > 1f)
                            timer = 1f;

                        float ratio = Mathf.Sin(timer * Mathf.PI * 0.5f);
                        tempEnemy.gameObject.transform.localPosition = Vector3.Lerp(startPos, targetPos, ratio);

                        if (timer >= 1f)
                            break;

                        yield return null;
                    }
                }
                SetTreasureEnemy();
            }
        }

        //동물, 상태가, wait가 되었는지 확인
        foreach (var temp in AnimalLIst)
        {
            while (temp.GetState() != ANIMAL_STATE.WAIT && temp.GetState() != ANIMAL_STATE.DEAD)
            {
                yield return null;
            }
        }

        GameUIManager.instance.ShowAdventureDarkBGBlock(false);

        IsPlayItemAction = false;
        state = ADVENTURE_STATE.WAIT;
        ManagerBlock.instance.state = BlockManagrState.WAIT;
        isDoAction = false;
        yield return null;
    }


    IEnumerator DoStageEnemy()
    {

        yield return null;
    }

    public int GetEnemyPos()
    {
        foreach (var temp in EnemyLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD)
            {
                return temp.pos;
            }
        }
        return -1;
    }

    public int GetNonStunEnemyPos()
    {
        foreach (var temp in EnemyLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD && temp.stunCount == 0)
            {
                return temp.pos;
            }
        }
        return -1;
    }

    public Vector3 AnimalLocalPos(int pos)
    {
        foreach (var temp in AnimalLIst)
        {
            if (temp.pos == pos)
            {
                return temp.gameObject.transform.position;
            }
        }
        return Vector3.zero;
    }


    public Vector3 EnemyPos(int pos)
    {        
        //float addY = pos != 1 ? 0 : addPosY2;
        float addY = (2 - pos) * addPosY2 * 0.5f;
        
        if (ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count == 1)        
            return new Vector3(POS_X_INTERVAL * (2 + pos), ANIMAL_POS_Y + ADD_POS_Y * pos, 0);        
        /*
        else if (ManagerBlock.instance.stageInfo.battleWaveList[waveCount].enemyIndexList.Count == 2)
                    return new Vector3(POS_X_INTERVAL * (3 + pos * 2) * 0.5f, ANIMAL_POS_Y + ADD_POS_Y * pos + addY, 0);        
        */
        return new Vector3(POS_X_INTERVAL * (1 + pos), ANIMAL_POS_Y + ADD_POS_Y * pos + addY, 0);
    }

    public Vector3 EnemyLocalPos(int pos)
    {        
        foreach (var temp in EnemyLIst)
        {
            if (temp.pos == pos)
            {
                return temp.gameObject.transform.position;
            }
        }
        return Vector3.zero;        
    }

    public Vector3 animalPos(int pos)
    {
        //float addY = pos != 1 ? 0 : addPosY2;
        float addY = (2 - pos) * addPosY2 * 0.5f; ;

        return new Vector3(-AdventureManager.POS_X_INTERVAL * (1 + pos), AdventureManager.ANIMAL_POS_Y + AdventureManager.ADD_POS_Y * pos + addY, 0);
    }

    List<InGameAnimal> GetLiveAnimalList()
    {
        List<InGameAnimal> tempAnimalList = new List<InGameAnimal>();

        for (int i = 0; i < 3; i++)        
            for (int j = 0; j < AnimalLIst.Count; j++ )            
                if (AnimalLIst[j].pos == i &&
                    AnimalLIst[j].GetState() != ANIMAL_STATE.DEAD)                
                    tempAnimalList.Add(AnimalLIst[j]); 
                
        return tempAnimalList;
    }

    ///////////////////
    //블럭제거시 
    ///////////////////    
    public void AddAnimalAttack(BlockColorType tempColor, Vector3 blockPos, int tempPoint = 1)
    {
        if (tempColor < BlockColorType.A || tempColor > BlockColorType.C)
            return;

        foreach (var temp in AnimalLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD && temp.GetColor() == tempColor)
            {
                //이펙트날리기
                InGameEffectMaker.instance.MakeFlyAdventureBlock(tempColor, temp, blockPos);
                //temp.AddAttackPoint(tempPoint);

                foreach (var tempS in skillItemList)
                {
                    if (temp.data.skill != 0 && tempS.GetColor() == tempColor && tempS.gameObject.activeSelf == true )
                    {
                        tempS.AddSkillPoint(tempPoint);
                        break;
                    }
                }
                break;
            }
        }
    }

    
    public void AddComboPoint()
    {
        foreach (var temp in AnimalLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD)
            {
                temp.AddComboBonus();
            }
        }
    }
    

    public void ChangePos(BlockBase tempBlock)
    {       
        if (tempBlock.colorType < BlockColorType.A || tempBlock.colorType > BlockColorType.C)
            return;


        //선택컬러 번호 찾기
        foreach (var temp in AnimalLIst)
        {
            if (temp.GetColor() == tempBlock.colorType)
            {
                if (temp.pos == 0)
                {
                    return;
                }
                else
                {
                    int tempPos = temp.pos;
                    foreach (var tempFirst in AnimalLIst)
                    {
                        if (tempFirst.pos == 0)
                        {
                            tempFirst.ChangPos(tempPos);
                            temp.ChangPos(0);
                            return;
                        }
                    }
                }
            }
        }


        //선택컬러가 0번이면 가만히 있기

        //아니면 위치 바꾸기


        /*
        //지금퍼스트찾기
        BlockColorType firstColor = BlockColorType.NONE;

        foreach (var temp in AnimalLIst)
        {
            if (temp.pos == 0)
            {
                firstColor = temp.GetColor();
                break;
            }
        }



        int moveCount = (int)tempBlock.colorType - (int)firstColor;

        if (moveCount == -1)
        {
            foreach (var temp in AnimalLIst)
                temp.ChangPos(temp.pos + 1);
        }
        else if (moveCount == 1)
        {
            foreach (var temp in AnimalLIst)
                temp.ChangPos(temp.pos - 1);
        }
        else if (moveCount == 2)
        {
            foreach (var temp in AnimalLIst)
                temp.ChangPos(temp.pos + 1);
        }
        else if (moveCount == -2)
        {
            foreach (var temp in AnimalLIst)
                temp.ChangPos(temp.pos - 1);
        }
        */
    }

    //동물이 적공격
    public void DemageEnemy(InGameAnimal attacker, int enemyPos, int demagePoint, bool isSkill = false, int SkillType = 0)
    {
        foreach (var enemy in EnemyLIst)
        {
            if (enemy.pos == enemyPos)
            {
                enemy.Pang(attacker, demagePoint, isSkill, SkillType);
            }
        }
    }

    public void DemageEnemyAll(InGameAnimal attacker, int demagePoint, bool isSkill = false, int SkillType = 0)
    {
        foreach (var enemy in EnemyLIst)
            enemy.Pang(attacker, demagePoint, isSkill, SkillType);
    }

    public void DemageAnimal(int aniamlPos, int damagePoint)
    {
        foreach (var animal in AnimalLIst)
        {
            if (animal.pos == aniamlPos)
            {
                animal.Pang(damagePoint);
            }
        }
    }

    private bool isClearStage()
    {
        if (Global.GameType == GameType.ADVENTURE)
        {
            var chapter = ManagerAdventure.User.GetChapterProgress(Global.chapterIndex);
            if (chapter == null)
                return false;

            ManagerAdventure.UserDataStageProgress stage = null;
            if (chapter.stageProgress.TryGetValue(Global.stageIndex, out stage) == false)
                return false;

            if (stage.clearLevel == 0)
                return false;

            return true;
        }
        else if(Global.GameType == GameType.ADVENTURE_EVENT)
        {
            return ManagerAdventure.EventData.IsAdvEventStageCleared(Global.stageIndex);
        }
        else
        {
            return false;
        }
    }

    //현재 스테이지 보물 정보들 세팅.
    private void SetStageTreasureInfo()
    {
        int treasureDropRatio = 0;

        ManagerAdventure.StageInfo info = null;

        if (Global.GameType == GameType.ADVENTURE)
        {
            info = ManagerAdventure.Stage.GetStage(Global.chapterIndex, Global.stageIndex);
        }
        else if(Global.GameType == GameType.ADVENTURE_EVENT)
        {
            info = ManagerAdventure.EventData.GetAdvEventStage(Global.stageIndex);
        }

        //플레이 횟수에 따라, 박스 드랍률 설정.
        if (isClearStage() == true)
            treasureDropRatio = info.normalDropBoxRatio;
        else
            treasureDropRatio = info.firstDropBoxRatio;

        if (Random.Range(1, 100) <= treasureDropRatio)
            isDropTreasure = true;

        //드랍안되면 반환.
        if (isDropTreasure == false)
            return;

        //박스 종류 드랍율 설정.
        int treasureTypeRatio = 0;
        foreach (var ratio in info.dropBoxRatio)
            treasureTypeRatio += ratio.Value[0];

        int rand = Random.Range(1, treasureTypeRatio);
        int totlaValue = 0;
        foreach (var ratio in info.dropBoxRatio)
        {
            totlaValue += ratio.Value[0];
            if (rand <= totlaValue)
            {
                TreasureType = ratio.Key;
                break;
            }
        }

        if (GameUIManager.instance != null)
        {
            GameUIManager.instance.treasureSprite.MakePixelPerfect();
            GameUIManager.instance.treasureSprite.transform.localScale = Vector3.one * 0.75f;
        }
        //어떤 웨이브에서 드랍할 건지 결정.
        treasureWave = Random.Range(0, ManagerBlock.instance.stageInfo.battleWaveList.Count);
    }
    
    //보물 드랍할 적 인덱스 랜덤으로 결정.
    private void SetTreasureEnemy()
    {
        if (isDropTreasure == true && waveCount == treasureWave)
        {
            int rand = Random.Range(0, EnemyLIst.Count);
            EnemyLIst[rand].IsDropTreasure = true;
        }
    }

    public void LockSkillItem()
    {

    }

    public void LockBlock(bool isLock = true)
    {
        if (isLock)
        {
            for (int tempX = GameManager.MIN_X; tempX < GameManager.MAX_X; tempX++)
            {
                for (int tempY = GameManager.MIN_Y; tempY < GameManager.MAX_Y; tempY++)
                {
                    Board back = PosHelper.GetBoardSreeen(tempX, tempY);
                    if (back != null && !back.HasDecoCoverBlock() && back.Block != null && (back.Block.IsNormalBlock() || back.Block.IsBombBlock()) && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                    {
                        back.Block.Hide(0.5f);
                    }
                }
            }
        }
        else
        {
            for (int tempX = GameManager.MIN_X; tempX < GameManager.MAX_X; tempX++)
            {
                for (int tempY = GameManager.MIN_Y; tempY < GameManager.MAX_Y; tempY++)
                {
                    Board back = PosHelper.GetBoardSreeen(tempX, tempY);
                    if (back != null && !back.HasDecoCoverBlock() && back.Block != null && (back.Block.IsNormalBlock() || back.Block.IsBombBlock()) && (back.Block.blockDeco == null || !back.Block.blockDeco.IsInterruptBlockSelect()))
                    {
                        back.Block.Show();
                    }
                }
            }
        }
    }

    /*
    public void GetAdventureGaige(int tempCount)
    {
        if (AdventureGaigeCount >= 100)        
            return;

        AdventureGaigeCount += tempCount;
        GameUIManager.instance.SetAdventureGaige(((float)AdventureGaigeCount) / 100f);
    }
    */

    public void ContinueGame()
    {
        foreach (var animal in AnimalLIst)
        {
            animal.ReviveAnimal(50);
        }
        //LockBlock(false);
        //GameUIManager.instance.adventureDarkBGBlock.SetActive(false);
        GameUIManager.instance.ShowAdventureDarkBGBlock(false);
    }

    public void ChargeAllSkillPoint()
    {
        foreach (var temp in skillItemList)
        {
            temp.AddSkillPoint(100);
        }
    }

    public void HealItemGuideOn(bool bOn)
    {
        for (int i = 0; i < AnimalLIst.Count; i++)
        {
            AnimalLIst[i].HealItemGuideOn(bOn);
        }
    }

    public void SkillItemGuideOn(bool bOn)
    {
        for (var i = 0; i < skillItemList.Count; i++)
        {
            skillItemList[i].SkillItemGuideOn(bOn);
        }
    }

    //포션 기능
    public void GetSkillPotion(float skillPercent)
    {
        foreach (var temp in AnimalLIst)
        {
            if (temp.skillItem != null)
                temp.skillItem.AddSkillPointUsingPercent(skillPercent);
        }
    }

    public void GetHealPotion(float healPercent)
    {
        foreach (var temp in AnimalLIst)
        {
            temp.HealHP(healPercent);
        }
    }

    //헬프기능/////////////////////////////////////////////////////////////////////
    public void ClearWave()
    {
        foreach (var enemy in EnemyLIst)
        {
            enemy.PangEnemyByTool();
        }
            //enemy.Pang(null, 100000);
        
        EventAction();
    }

    public void PangAnimal()
    {
        foreach (var animal in AnimalLIst)
        {
            if (animal.GetState() != ANIMAL_STATE.DEAD)
            {
                animal.DeadAnimalByTool();
                break;
            }
        }
    }

    public void HealAnimal()
    {
        foreach (var animal in AnimalLIst)
        {
            if (animal.GetState() == ANIMAL_STATE.DEAD)
            {
                animal.RecoverAnimal(100);
                break;
            }
        }
    }

    public void PangEnemy()
    {
        foreach (var enemy in EnemyLIst)
        {
            if (enemy.GetState() != ANIMAL_STATE.DEAD)
            {
                enemy.Pang(null, 1000000);
                break;
            }
        }
    }

    #region 탐험모드 튜토리얼
    private void CheckAndPlayAdventureTutorial()
    {
        if (GameManager.instance.firstPlay == true)
        {
            if (Global.chapterIndex == 1 && Global.stageIndex == 1
                && ManagerAdventure.User.GetChapterCursor() == 1 && ManagerAdventure.User.GetStageCursor() == 1)
            {
                if (AdventureManager.instance.waveCount == 1) // 스킬 튜토리얼
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialUseSkill_Adventure);
                else if (AdventureManager.instance.waveCount == 2) // 보스 튜토리얼
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGameRule_Adventure);
            }
        }
    }
    #endregion
}
