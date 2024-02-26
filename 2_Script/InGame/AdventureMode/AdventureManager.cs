using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureManager : MonoSingletonOnlyScene<AdventureManager>
{
    public const int ANIMAL_SIZE = 220;

    public const float ANIMAL_POS_Y = 432;
    public const float ADD_POS_Y = 0;//-15f;
    public const float POS_X_INTERVAL = 105f;

    public const float SKILL_ITEM_POS_Y = 220f;
    public const float LIFE_POS_Y = -150;//80f;

    float addPosY2 = 20;

    //게임상태
    public int waveCount = 0;

    public List<InGameAnimal> AnimalLIst = new List<InGameAnimal>();
    public List<InGameEnemy> EnemyLIst = new List<InGameEnemy>();
    public List<GameObject> ObjList = new List<GameObject>();

    public List<InGameSkillItem> skillItemList = new List<InGameSkillItem>();

    public GameObject AnimalObj;
    public GameObject EnemyObj;
    public GameObject BossLabelObj;
    public GameObject StartLabelObj;

    //폭탄게이지
    int AdventureGaigeCount = 0;
    BlockBombType advantureBombType = BlockBombType.BOMB;

    //스킬아이콘 셋팅
    public GameObject skillObj;

    //초기화
    bool isFinishedInit = false;
    public bool IsFinishedInitAction()
    {
        return isFinishedInit;
    }

    //콤보
    public List<BlockScore> ComboObjList = new List<BlockScore>();

    public void InitStage()
    {
        waveCount = 0;
        FlyAdventureBlock.flyCount = 0;

        
        foreach (var temp in ObjList) Destroy(temp);
        ObjList.Clear();
        
        //동물데이타 넣기
        AnimalLIst.Clear();
        skillItemList.Clear();
        for (int i = 0; i < 3; i++)
        {
            var animalData = new ManagerAdventure.AnimalInstance();

            //if (EditManager.instance == null)
            //    animalData = ManagerAdventure.User.GetAnimalFromDeck(1, i);
            //else
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
            InGameSkillItem tempSkillItem = NGUITools.AddChild(GameUIManager.instance.Advance_Root, skillObj).GetComponent<InGameSkillItem>();
            tempSkillItem.InitSkill((BlockColorType)(i + 1), animalData.attr);
            skillItemList.Add(tempSkillItem);
            ObjList.Add(tempSkillItem.gameObject);

            tempAnimal.skillItemObj = tempSkillItem.gameObject;
            tempAnimal.skillItem = tempSkillItem;
            tempSkillItem.animal = tempAnimal;
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
        }

        GameUIManager.instance.SetAdvantureWave(waveCount + 1, ManagerBlock.instance.stageInfo.battleWaveList.Count);        
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

        isFinishedInit = true;
        yield return null;
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

        /////콤보모으기
        if (ComboObjList.Count > 0)
        {
            foreach (var temp in ComboObjList)
            {
                temp.MoveCombo = true;
            }

            waitTimer = 0f;
            while (waitTimer < ManagerBlock.instance.comboCount * 0.1f + 0.5f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
            GameUIManager.instance.adventureComboLabel.gameObject.SetActive(false);
        }

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
                    temp.Attack(GetEnemyPos());
                    LockBlock(true);

                    waitTimer = 0f;
                    while (waitTimer < 0.4f)
                    {
                        waitTimer += Global.deltaTimePuzzle;
                        yield return null;
                    }
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
            {
                foreach (var temp in AnimalLIst)
                    temp.ResetAnimal();
                break;
            }
            yield return null;
        }

        if (HasAnimalAttPoint)
        {
            waitTimer = 0f;
            while (waitTimer < 0.7f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }        
        }

        ////적공격////////////////////////////////////
        int enemyIndex = 0;
        int animalIndex = 0; //동물순서

        while (true)
        {
            foreach (var enemy in EnemyLIst)
            {
                if (enemy.pos == enemyIndex && enemy.IsAttackTurn())
                {
                    enemy.Attack(GetAnimalPos(animalIndex));
                    LockBlock(true);
                    HasAnimalAttPoint = true;

                    waitTimer = 0f;
                    while (waitTimer < 0.5f)
                    {
                        waitTimer += Global.deltaTimePuzzle;
                        yield return null;
                    }
                    animalIndex++;
                    yield return null;
                }
            }
            enemyIndex++;

            if (enemyIndex >= EnemyLIst.Count)
                break;
            yield return null;
        }

        //게임오버 판정하기////////////////////////////////
        bool IsGameOver = true;
        foreach (var temp in AnimalLIst)
            if(temp.GetState() != ANIMAL_STATE.DEAD)
            {
                IsGameOver = false;
                break;
            }

        if (IsGameOver)
        {
            GameManager.instance.StageFail();
            yield break;
        }


        //적카운트내리기//동물 카운트 내리기//////////////////////////////////
        foreach (var enemy in EnemyLIst)        
            enemy.AddAttackPoint();        

        foreach (var animal in AnimalLIst)        
            animal.CountHeart();        

        waitTimer = 0f;
        while (waitTimer < 0.5f)
        {
            waitTimer += Global.deltaTimePuzzle;
            yield return null;
        }

        ManagerBlock.instance.blockMove = true;
        List<BlockBase> makeBlockList = ManagerBlock.instance.MakeAdventureBlock();
        yield return null; 


        //웨이브 이동///////////////////////////////////////               
        bool WaveFinished = true;
        foreach (var temp in EnemyLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD)            
                WaveFinished = false;            
        }
        
        if (WaveFinished)
        {
            waveCount++;

            if (waveCount >= ManagerBlock.instance.stageInfo.battleWaveList.Count)
            {
                GameManager.instance.StageClear();

                waitTimer = 0f;
                while (waitTimer < 0.4f)
                {
                    waitTimer += Global.deltaTimePuzzle;
                    yield return null;
                }

                foreach (var temp in AnimalLIst) temp.ClearStage();

                yield break;
            }
            else
            {
                foreach (var temp in AnimalLIst) temp.MoveWave();
                GameUIManager.instance.SetAdvantureWave(waveCount + 1, ManagerBlock.instance.stageInfo.battleWaveList.Count);

                if (ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave > 0)
                {
                    GameUIManager.instance.SetAdvantureBG("Boss_wave", true);
                    /*
                    //보드내리기
                    ManagerBlock.instance.SetDigCountMove(9);
                    yield return null;

                    ManagerSound.AudioPlay(AudioInGame.MOVE_GROUND);    //
                    while (true)
                    {
                        if (ManagerBlock.instance.MoveGround(3f))
                        {
                            break;
                        }
                        yield return null;
                    }

                    ManagerBlock.instance.movePanelPause = false;
                    yield return null;
                    */
                    
                    waitTimer = 0f;
                    while (waitTimer < 1.533f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }
                    
                    GameUIManager.instance.SetAdvantureBG("Boss_idle", true);
                    GameUIManager.instance.SetAdventureEffectBG(true);

                    GameObject bossLabel = NGUITools.AddChild(GameUIManager.instance.Advance_Root, BossLabelObj);
                    //bossLabel.transform.localPosition = new Vector3(0, 440,0);


                    waitTimer = 0f;
                    while (waitTimer < 2f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }
                    Destroy(bossLabel);
                    GameUIManager.instance.SetAdventureEffectBG(false);
                }
                else
                {
                    GameUIManager.instance.SetAdvantureBG("wave", true);
                    /*
                    //보드내리기
                    ManagerBlock.instance.SetDigCountMove(9);
                    yield return null;

                    ManagerSound.AudioPlay(AudioInGame.MOVE_GROUND);    //
                    while (true)
                    {
                        if (ManagerBlock.instance.MoveGround(3f))
                        {
                            break;
                        }
                        yield return null;
                    }

                    ManagerBlock.instance.movePanelPause = false;
                    yield return null;
                    */

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
                //TODO 보스모드인때 갯수에 따라서 정렬하기

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
            }
            //캐릭터불러오기 등장
        }

        //블럭풀기//////////////////////////////////////////
        LockBlock(false);

        //폭탄만들기///////////////////////////////////////
        /*
        if (AdventureGaigeCount >= 100) //게임이 안끝났을때만
        {
            BlockBase tempBlock = PosHelper.GetRandomBlock();
            Vector3 startPos = GameUIManager.instance.advantureGaigeBomb.cachedTransform.position;

            FlyMakeBomb flyMakeBomb = InGameEffectMaker.instance.MakeFlyMakeBomb(startPos);
            flyMakeBomb.initBlock(advantureBombType, tempBlock, startPos);

            AdventureGaigeCount = 0;
            GameUIManager.instance.SetAdventureGaige(0f);

            //날아가는동안 기다리기
            waitTimer = 0f;
            while (waitTimer < 1f)
            {
                waitTimer += Global.deltaTimePuzzle*1.5f;
                yield return null;
            }
        }
        */

        isFinishedEvent = true;        
        yield return null;
    }


    //아이템이벤트
    public bool IsFinishedItemAction()
    {
        return IsPlayItemAction;
    }

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
        float waitTimer = 0;

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
                    temp.Attack(GetEnemyPos());
                    LockBlock(true);

                    waitTimer = 0f;
                    while (waitTimer < 0.4f)
                    {
                        waitTimer += Global.deltaTimePuzzle;
                        yield return null;
                    }
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
            {
                foreach (var temp in AnimalLIst)
                    temp.ResetAnimal();
                break;
            }
            yield return null;
        }

        if (HasAnimalAttPoint)
        {
            waitTimer = 0f;
            while (waitTimer < 0.8f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
        }

        //웨이브 이동///////////////////////////////////////               
        bool WaveFinished = true;
        foreach (var temp in EnemyLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD)
                WaveFinished = false;
        }

        if (WaveFinished)
        {
            waveCount++;

            if (waveCount >= ManagerBlock.instance.stageInfo.battleWaveList.Count)
            {
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

                //웨이브카운트표시         
                GameUIManager.instance.SetAdvantureWave(waveCount + 1, ManagerBlock.instance.stageInfo.battleWaveList.Count);

                if (ManagerBlock.instance.stageInfo.battleWaveList[waveCount].bossWave > 0)
                {
                    GameUIManager.instance.SetAdvantureBG("Boss_wave", false);

                    /*
                    //보드내리기
                    ManagerBlock.instance.SetDigCountMove(9);
                    yield return null;

                    ManagerSound.AudioPlay(AudioInGame.MOVE_GROUND);    //
                    while (true)
                    {
                        if (ManagerBlock.instance.MoveGround(3f))
                        {
                            break;
                        }
                        yield return null;
                    }

                    ManagerBlock.instance.movePanelPause = false;
                    yield return null;
                    */

                    waitTimer = 0f;
                    while (waitTimer < 1.533f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }

                    GameUIManager.instance.SetAdvantureBG("Boss_idle", true);

                    GameObject bossLabel = NGUITools.AddChild(GameUIManager.instance.Advance_Root, BossLabelObj);
                    bossLabel.transform.localPosition = new Vector3(0, 440, 0);

                    waitTimer = 0f;
                    while (waitTimer < 1f)
                    {
                        waitTimer += Global.deltaTimePuzzle * 1f;
                        yield return null;
                    }
                    Destroy(bossLabel);
                }
                else
                {
                    GameUIManager.instance.SetAdvantureBG("wave", false);

                    /*
                    //보드내리기
                    ManagerBlock.instance.SetDigCountMove(9);
                    yield return null;

                    ManagerSound.AudioPlay(AudioInGame.MOVE_GROUND);    //
                    while (true)
                    {
                        if (ManagerBlock.instance.MoveGround(3f))
                        {
                            break;
                        }
                        yield return null;
                    }

                    ManagerBlock.instance.movePanelPause = false;
                    yield return null;
                    */

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

                //적생성
                //TODO 보스모드인때 갯수에 따라서 정렬하기

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
            }
            //캐릭터불러오기 등장
        }

        //블럭풀기//////////////////////////////////////////
        //LockBlock(false);

        ManagerBlock.instance.state = BlockManagrState.WAIT;
        IsPlayItemAction = false ;
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
        float addY = pos != 1 ? 0 : addPosY2;
        
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


    public int GetAnimalPos(int posIndex)  //3
    {
        List<int> poslist = new List<int>();    //2

        int posCount = 0;

        while (true)
        {
            foreach (var ani in AnimalLIst)
            {
                if (ani.pos == posCount && ani.GetState() != ANIMAL_STATE.DEAD)
                {
                    poslist.Add(ani.pos);
                }
            }

            posCount++;
            if (posCount >= AnimalLIst.Count)
                break;
        }

        if (poslist.Count == 0)
            return -1;

        posIndex = posIndex % poslist.Count;

        return poslist[posIndex];
    }

    public Vector3 animalPos(int pos)
    {
        float addY = pos != 1 ? 0 : addPosY2;
        return new Vector3(-AdventureManager.POS_X_INTERVAL * (1 + pos), AdventureManager.ANIMAL_POS_Y + AdventureManager.ADD_POS_Y * pos + addY, 0);
    }

    ///////////////////
    //블럭제거시 
    ///////////////////    
    public void AddAnimalAttack(BlockColorType tempColor, Vector3 blockPos, int tempPoint = 1)
    {
        foreach (var temp in AnimalLIst)
        {
            if (temp.GetState() != ANIMAL_STATE.DEAD && temp.GetColor() == tempColor)
            {
                //이펙트날리기
                InGameEffectMaker.instance.MakeFlyAdventureBlock(tempColor, temp, blockPos);
                //temp.AddAttackPoint(tempPoint);

                foreach (var tempS in skillItemList)
                {
                    if (tempS.GetColor() == tempColor)
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
        
    }

    //동물이 적공격
    public void DemageEnemy(int enemyPos, int demagePoint, bool isSkill = false)
    {
        foreach (var enemy in EnemyLIst)
        {
            if (enemy.pos == enemyPos)
            {
                enemy.Pang(demagePoint, isSkill);
            }
        }
    }

    public void DemageEnemyAll(int demagePoint)
    {
        foreach (var enemy in EnemyLIst)
            enemy.Pang(demagePoint, true);
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

    public void GetAdventureGaige(int tempCount)
    {
        if (AdventureGaigeCount >= 100)        
            return;

        AdventureGaigeCount += tempCount;
        GameUIManager.instance.SetAdventureGaige(((float)AdventureGaigeCount) / 100f);
    }

    public void ContinueGame(int count)
    {
        foreach (var animal in AnimalLIst)
            animal.RecoverAnimal(50);        
    }

    //게이지 아이템
    public void UseAdventureGaige()
    {
        if (AdventureGaigeCount >= 100) //게임이 안끝났을때만
        {
            //모든블럭을 찾아내기, 컬러가 D,E 인것은 색상바꾸기
            /*
            List<BlockBase> tempList = new List<BlockBase>();

            for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                {
                    BlockBase tempBlock = PosHelper.GetBlockScreen(j, i);
                    if (tempBlock != null && tempBlock.IsNormalBlock())
                    {
                        //확률체크
                        if (ManagerBlock.instance.stageInfo.probability[4] == 0 && tempBlock.colorType == BlockColorType.D)
                        {
                            tempList.Add(tempBlock);
                        }
                        else if (ManagerBlock.instance.stageInfo.probability[4] > 0 && tempBlock.colorType == BlockColorType.E)
                        {
                            tempList.Add(tempBlock);
                        }
                    }
                }

            BlockBase.uniqueIndexCount++;
            foreach (var temp in tempList)
            {
                temp.state = BlockState.PANG;
                temp.pangIndex = BlockBase.uniqueIndexCount;
                temp.Pang();
            }
            */
            float inX = (GameManager.MinScreenX + GameManager.MaxScreenX) * 0.5f;
            float inY = (GameManager.MinScreenY + GameManager.MaxScreenY) * 0.5f;

            /*
            float inCount = 0;
            for (int i = GameManager.MinScreenY; i < GameManager.MaxScreenY; i++)
                for (int j = GameManager.MinScreenX; j < GameManager.MaxScreenX; j++)
                {
                    BlockBase tempBlock = PosHelper.GetBlockScreen(j, i);
                    if (tempBlock != null && tempBlock.IsNormalBlock())
                    {
                        inX += i;
                        inY += j;

                        inCount++;
                    }
                }
             */
 
            BlockMaker.instance.MakeBombBlock(null, (int)inX, (int)inY + 2, BlockBombType.ADVENTURE_BOMB, BlockColorType.NONE, false, false, false);
            
            /*
            BlockBase tempBlock = PosHelper.GetRandomBlock();
            Vector3 startPos = GameUIManager.instance.advantureGaigeBomb.cachedTransform.position;

            FlyMakeBomb flyMakeBomb = InGameEffectMaker.instance.MakeFlyMakeBomb(startPos);
            flyMakeBomb.initBlock(advantureBombType, tempBlock, startPos);
            */
            AdventureGaigeCount = 0;
            GameUIManager.instance.SetAdventureGaige(0f);
        }
    }

    //아이템기능
    public void ChargeAllSkillPoint()
    {
        foreach (var temp in skillItemList)
        {
            temp.AddSkillPoint(100);
            //연출추가
        }
    }

    //헬프기능/////////////////////////////////////////////////////////////////////
    public void ClearWave()
    {
        foreach (var enemy in EnemyLIst)       
            enemy.Pang(100000);
        
        EventAction();
    }

    public void PangAnimal()
    {
        foreach (var animal in AnimalLIst)
        {
            if (animal.GetState() != ANIMAL_STATE.DEAD)
            {
                animal.Pang(1000000);
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
                enemy.Pang(1000000);
                break;
            }
        }
    }
}
