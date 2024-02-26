using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ANIMAL_ATTRIBUTE
{
    NONE,
    ROCK,
    PAPER,
    SCISSORS,
}


public enum ANIMAL_STATE
{
    STOP,
    WAIT,
    ATTACK,
    PANG,
    DEAD,
    CHANGE_POS,
    SKILL,
}

public class InGameAnimal : MonoBehaviour 
{
    BlockColorType colorType = BlockColorType.NONE;
    public BlockColorType GetColor()
    {
        return colorType;
    }

    ANIMAL_STATE state = ANIMAL_STATE.STOP;
    public ANIMAL_STATE GetState()
    {
        return state;
    }

    //동물 데이타
    public ManagerAdventure.AnimalInstance data;
    public int pos = 0;
    int tempPos = 0;

    public GameObject AnimalRoot;
    public GameObject TextureRoot;
    public UIUrlTexture[] MainUrlTexture;    
    public UILabel[] AttLabel;
    public UISprite attTypeSprite;  //색상정보
    public Animation Animalanim;

    //생명게이지
    public GameObject lifeRoot;
    public UISlider lifeSlider;
    public UILabel lifeLabel;

    public GameObject HeartRoot;
    public UILabel heartCountLabel;

    //스킬
    public InGameSkillItem skillItem;
    public GameObject skillItemObj;

    //공격력 표시
    public GameObject NormalScore;
    public GameObject MaxScore;

    public GameObject EffectPosObj;

    const int MAX_ATT_POINT = 10;
    int addMaxAttPoint = 0;

    int attackPoint = 0;
    int life = 0;
    int heartCount = 10;
    int SetEnemyPos = -1;

    public void init(ManagerAdventure.AnimalInstance tempData, int tempPos = 0)
    {
        //기본초기화
        attackPoint = 0;

        life = tempData.hp;
        data = tempData;
        pos = tempPos;

        colorType = (BlockColorType)(tempPos + 1);

        attTypeSprite.spriteName = "icon_block_" + ManagerUI.GetColorTypeString(GetColor());
        attTypeSprite.MakePixelPerfect();
        attTypeSprite.cachedTransform.localScale = Vector3.one * 1.2f;

        transform.localPosition = AdventureManager.instance.animalPos(tempPos);

        MainUrlTexture[0].depth = 10 - pos;
        MainUrlTexture[0].Load(Global.adventureDirectory, "Animal/", string.Format("at_{0:D4}", tempData.idx));
        MainUrlTexture[0].MakePixelPerfect();
        MainUrlTexture[0].SettingTextureScale(AdventureManager.ANIMAL_SIZE, AdventureManager.ANIMAL_SIZE);

        MainUrlTexture[1].depth = 10 - pos;
        MainUrlTexture[1].Load(Global.adventureDirectory, "Animal/", string.Format("at_{0:D4}", tempData.idx) + "_a");
        MainUrlTexture[1].MakePixelPerfect();
        MainUrlTexture[1].SettingTextureScale(AdventureManager.ANIMAL_SIZE, AdventureManager.ANIMAL_SIZE);

        MainUrlTexture[2].depth = 10 - pos;
        MainUrlTexture[2].Load(Global.adventureDirectory, "Animal/", string.Format("at_{0:D4}", tempData.idx) + "_b");
        MainUrlTexture[2].MakePixelPerfect();
        MainUrlTexture[2].SettingTextureScale(AdventureManager.ANIMAL_SIZE, AdventureManager.ANIMAL_SIZE);

        lifeLabel.text = life.ToString();
        lifeSlider.value = (float)life / (float)data.hp;
        lifeSlider.gameObject.transform.localPosition = new Vector3(0, AdventureManager.LIFE_POS_Y);

        Animalanim.Stop();
        Animalanim.Play("Animal_Out_wait");

        EffectPosObj.transform.localPosition = new Vector3(0, -70 + ((float)data.animalSize)*0.5f, 0f);
    }

    public void StartAnimal()
    {
        Animalanim.Stop();
        Animalanim.Play("Animal_Start");

        state = ANIMAL_STATE.WAIT;
    }

    public void AddComboBonus()
    {
        if (GetState() == ANIMAL_STATE.DEAD)
            return;

        //if (Animalanim.IsPlaying("Animal_Charge") == false)
        {
            Animalanim.Stop();
            Animalanim.Play("Animal_Charge");
        }
    }

    public void AddAttackPoint(int tempPoint = 1)
    {
        if (GetState() == ANIMAL_STATE.DEAD)
            return;
        
        if (Animalanim.IsPlaying("Animal_Charge") == false)
        {
            Animalanim.Stop();
            Animalanim.Play("Animal_Charge");
        }

        attackPoint++;

        if (attackPoint > MAX_ATT_POINT + addMaxAttPoint)
        {
            attackPoint = MAX_ATT_POINT + addMaxAttPoint;
            //코인으로 추가
            InGameEffectMaker.instance.MakeFlyCoinAdventure(transform.position, 1);
            return;
        }

        foreach (var temp in MainUrlTexture)
            temp.transform.localScale = Vector3.one * (1 + attackPoint * 0.05f);

        foreach (var temp in AttLabel)
        {
            temp.text = (data.atk * attackPoint).ToString();
            temp.depth = 11 - tempPos;
        }

        if (attackPoint >= MAX_ATT_POINT + addMaxAttPoint)
        {
            MaxScore.SetActive(true);
            NormalScore.SetActive(false);
            MaxScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize, 0);

            StartCoroutine(DoShowAttLabel((data.atk * attackPoint).ToString()));
        }
        else
        {
            MaxScore.SetActive(false);
            NormalScore.SetActive(true);
            NormalScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize, 0);

            StartCoroutine(DoShowAttLabel((data.atk * attackPoint).ToString()));
        }        
    }

    
    IEnumerator DoShowAttLabel(string text, bool maxScore = false)
    {
        float waitTimer = 0;
        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle*5;

            float ratioA = ManagerBlock.instance._curveBlockPopUp.Evaluate(waitTimer);
            MaxScore.transform.localScale = Vector3.one * ratioA;
            NormalScore.transform.localScale = Vector3.one * ratioA;
            yield return null;
        }

        MaxScore.transform.localScale = Vector3.one;
        NormalScore.transform.localScale = Vector3.one;
        waitTimer = 0f;
        yield return null;
    }

    public void ResetAnimal()
    {
        if (attackPoint > 0)
            StartCoroutine(DoRecoverScale());

        attackPoint = 0;

        MaxScore.SetActive(false);
        NormalScore.SetActive(false);
    }

    IEnumerator DoRecoverScale()
    {
        float scaleX = MainUrlTexture[0].transform.localScale.x;

        float waitTimer = 0;
        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * 5;
            float scaleRatio = Mathf.Lerp(scaleX, 1f, waitTimer);

            foreach (var temp in MainUrlTexture)
                temp.transform.localScale = Vector3.one * scaleRatio;

            yield return null;
        }

        foreach (var temp in MainUrlTexture)
            temp.transform.localScale = Vector3.one * 1f;

        yield return null;
    }

    public bool HasAttackPoint()
    {
        return attackPoint > 0;
    }

    public void Attack(int enemyPos)
    {
        if (enemyPos == -1)
        { 
            ResetAnimal();
            return;
        }

        state = ANIMAL_STATE.ATTACK;
        StartCoroutine(DoAttack(enemyPos));
    }

    IEnumerator DoAttack(int enemyPos)
    {
        SetEnemyPos = enemyPos;

        string animName = "Animal_Att_" + (enemyPos + pos + 1);
        if (ManagerBlock.instance.stageInfo.battleWaveList[AdventureManager.instance.waveCount].enemyIndexList.Count == 1)
            animName = "Animal_Att_" + (pos + 2);         
        
        Animalanim.Stop();
        Animalanim.Play(animName);

        MaxScore.SetActive(false);
        NormalScore.SetActive(false);

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 2f;

            float ratio = Mathf.Sin(timer * Mathf.PI);

            foreach (var temp in MainUrlTexture)
                temp.transform.localScale = Vector3.Lerp(Vector3.one * (1f + attackPoint * 0.05f), Vector3.one * 1f, ratio);

            yield return null;
        }

        while (Animalanim.IsPlaying(animName))
            yield return null;

        state = ANIMAL_STATE.WAIT;

        foreach (var temp in MainUrlTexture)
            temp.transform.localScale = Vector3.one;// *(1f + attackPoint * 0.05f);

        MaxScore.SetActive(false);
        NormalScore.SetActive(false);
        yield return null;
    }

    public void AttackEnemy()
    {
        AdventureManager.instance.DemageEnemy(SetEnemyPos, attackPoint * data.atk);
        attackPoint = 0;
        addMaxAttPoint = 0;        
    }

    public void Pang(int pangPoint)
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        life -= pangPoint;
        lifeSlider.value = (float)life / (float)data.hp;
        lifeLabel.text = life.ToString();

        if (life > 0)
        {
            state = ANIMAL_STATE.PANG;
            StartCoroutine(DoPangAni());
        }
        else
        {
            state = ANIMAL_STATE.DEAD;
            skillItem.ResetSkill();
            StartCoroutine(DoDeadAni());
        }
    }

    IEnumerator DoPangAni()
    {
        float timer = 0f;

        Animalanim.Stop();
        Animalanim.Play("Animal_Demage");

        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 4f;
            yield return null;
        }

        while (Animalanim.IsPlaying("Animal_Demage"))
            yield return null;

        state = ANIMAL_STATE.WAIT;
        yield return null;
    }

    IEnumerator DoDeadAni()
    {
        Animalanim.Stop();
        Animalanim.Play("Animal_Demage");

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 8f;
            yield return null;
        }

        lifeRoot.SetActive(false);
        HeartRoot.SetActive(true);

        heartCount = 10;
        heartCountLabel.text = heartCount.ToString();

        while (Animalanim.IsPlaying("Animal_Demage"))
            yield return null;

        TextureRoot.SetActive(false);

        yield return null;
    }

    public void ChangPos(int temp)
    {
        if (temp > 2)
        {
            temp -= 3;
        }
        else if (temp < 0)
        {
            temp += 3;
        }

        state = ANIMAL_STATE.CHANGE_POS;
        tempPos = temp;
        StartCoroutine(DoChangePos());
    }

    IEnumerator DoChangePos()
    {
        Vector3 targetPos = AdventureManager.instance.animalPos(tempPos);
        Vector3 startPos = AdventureManager.instance.animalPos(pos); 

        float timer = 0f;
        while (true)
        {
            timer += Global.deltaTimePuzzle * 4f;
            if (timer > 1f)            
                timer = 1f;            

            float ratio = Mathf.Sin(timer * Mathf.PI * 0.5f);
            Vector3 cross = Vector3.Cross((startPos - targetPos).normalized, Vector3.forward);

            int dir = tempPos < pos ? 1 : -1;
            
            
            if ( Mathf.Abs(pos - tempPos) > 1)            
                transform.localPosition = Vector3.Lerp(startPos, targetPos, ratio) + cross * Mathf.Sin(timer * Mathf.PI) * -50f * dir;  
            else
                transform.localPosition = Vector3.Lerp(startPos, targetPos, ratio) + cross * Mathf.Sin(timer * Mathf.PI) * 50f * dir;

            skillItemObj.transform.localPosition = Vector3.Lerp(new Vector3(startPos.x, AdventureManager.SKILL_ITEM_POS_Y), new Vector3(targetPos.x, AdventureManager.SKILL_ITEM_POS_Y), ratio);

            if (timer >= 1f)            
                break;
            
            if (timer > 0.5f)
                foreach (var temp in MainUrlTexture)
                    temp.depth = 10 - tempPos;            

            yield return null;
        }

        pos = tempPos;
        foreach (var temp in MainUrlTexture)
            temp.depth = 10 - pos;   

        if (life > 0)
            state = ANIMAL_STATE.WAIT;
        else
            state = ANIMAL_STATE.DEAD;

        yield return null;

    }

    void Update()
    {
        switch (state)
        {
            case ANIMAL_STATE.WAIT:
                if (Animalanim.isPlaying == false)                
                    Animalanim.Play("Animal_Wait");   
                break;
        }
    }

    public void UseSkill()
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        state = ANIMAL_STATE.SKILL;

        switch ((SKILL_TYPE)data.skill)
        {
            case SKILL_TYPE.ATTACK:
            case SKILL_TYPE.ATTACK_ALL:
                Animalanim.Stop();
                Animalanim.Play("Animal_Skill_1");
                break;

            case SKILL_TYPE.ADD_1_MAX_SKILL_POINT:
                break;
            case SKILL_TYPE.ADD_ALL_MAX_SKILL_POINT:
                break;

            case SKILL_TYPE.HEAL_HP:
            case SKILL_TYPE.HEAL_ALL_HP:
                Animalanim.Stop();
                Animalanim.Play("Animal_Skill_2");
                break;

            case SKILL_TYPE.STUN:
                break;
            case SKILL_TYPE.STUN_ALL:
                break;
            case SKILL_TYPE.ADD_LINE_BOMB:
                break;
            case SKILL_TYPE.ADD_DOUBLE_BOMB:
                break;
            case SKILL_TYPE.ADD_RAINBOW_BOMB:
                break;
            case SKILL_TYPE.CHANGE_BLOCK_COLOR:
                break;
            case SKILL_TYPE.REMOVE_GIMIK_1STEP:
                break;
            case SKILL_TYPE.GET_ALL_ITEM_BLOCK:
                break;
            case SKILL_TYPE.MAKE_ITEM:
                break;
        }


        StartCoroutine(CoUSeSkill());
    }


    public void SkillAttack()
    {
        switch ((SKILL_TYPE)data.skill)
        {
            case SKILL_TYPE.ATTACK:
                AdventureManager.instance.DemageEnemy(AdventureManager.instance.GetEnemyPos(), data.skillGrade, true);
                break;

            case SKILL_TYPE.ATTACK_ALL:
                AdventureManager.instance.DemageEnemyAll(data.skillGrade);
                break;

            case SKILL_TYPE.ADD_1_MAX_SKILL_POINT:
                addMaxAttPoint += data.skillGrade;
                break;
            case SKILL_TYPE.ADD_ALL_MAX_SKILL_POINT:
                foreach (var temp in AdventureManager.instance.AnimalLIst)
                    temp.SetAddMaxAttPoint(data.skillGrade);
                break;
            case SKILL_TYPE.HEAL_HP:
                HealHP(data.skillGrade);
                break;

            case SKILL_TYPE.HEAL_ALL_HP:
                foreach (var temp in AdventureManager.instance.AnimalLIst)
                    temp.HealHP(data.skillGrade);
                break;
                
            case SKILL_TYPE.STUN:
                foreach (var temp in AdventureManager.instance.EnemyLIst)
                {
                    if (temp.pos == AdventureManager.instance.GetEnemyPos())
                    {
                        temp.Stun(data.skillGrade);
                        break;
                    }
                }
                break;
            case SKILL_TYPE.STUN_ALL:
                foreach (var temp in AdventureManager.instance.EnemyLIst)
                {
                    temp.Stun(data.skillGrade);
                }
                break;
            case SKILL_TYPE.ADD_LINE_BOMB:
                for (int i = 0; i < data.skillGrade; i++)
                {
                    BlockBase tempBlock = PosHelper.GetRandomBlock();
                    Vector3 startPos = gameObject.transform.position;
                    FlyMakeBomb flyMakeBomb = InGameEffectMaker.instance.MakeFlyMakeBomb(startPos);
                    int randomLine = Random.Range(0, 100);
                    BlockBombType lineType = randomLine > 50 ? BlockBombType.LINE_H : BlockBombType.LINE_V;
                    flyMakeBomb.initBlock(lineType, tempBlock, startPos);
                }
                break;
            case SKILL_TYPE.ADD_DOUBLE_BOMB:
                for (int i = 0; i < data.skillGrade; i++)
                {
                    BlockBase tempBlockA = PosHelper.GetRandomBlock();
                    Vector3 startPosA = gameObject.transform.position;
                    FlyMakeBomb flyMakeBombA = InGameEffectMaker.instance.MakeFlyMakeBomb(startPosA);
                    flyMakeBombA.initBlock(BlockBombType.BOMB, tempBlockA, startPosA);
                }
                break;
            case SKILL_TYPE.ADD_RAINBOW_BOMB:
                for (int i = 0; i < data.skillGrade; i++)
                {
                    BlockBase tempBlockR = PosHelper.GetRandomBlock();
                    Vector3 startPosR = gameObject.transform.position;
                    FlyMakeBomb flyMakeBombR = InGameEffectMaker.instance.MakeFlyMakeBomb(startPosR);
                    flyMakeBombR.initBlock(BlockBombType.RAINBOW, tempBlockR, startPosR);
                }
                break;
            case SKILL_TYPE.CHANGE_BLOCK_COLOR:
                for (int i = 0; i < data.skillGrade; i++)
                {
                    while (true)
                    {
                        BlockBase tempBlockR = PosHelper.GetRandomBlock();
                        if (tempBlockR.colorType != GetColor())
                        {
                            tempBlockR.colorType = GetColor();
                            break;
                        }
                    }
                }
                break;
            case SKILL_TYPE.REMOVE_GIMIK_1STEP:
                break;
            case SKILL_TYPE.GET_ALL_ITEM_BLOCK:
                break;
            case SKILL_TYPE.MAKE_ITEM:
                break;
        }
    }


    IEnumerator CoUSeSkill()
    {
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        while (Animalanim.isPlaying)
            yield return null;

        GameManager.instance.IsCanTouch = true;
        ManagerBlock.instance.state = BlockManagrState.BEFORE_WAIT;
        yield return null;
    }


    public void SetAddMaxAttPoint(int addPoint)
    {
        addMaxAttPoint += addPoint;

        //최대값추가 이펙트
    }

    public void HealHP(float percent)
    {
        life += (int)(((float)life) * percent);

        if (life > data.hp)
            life = data.hp;

        //힐효과
    }

    public void MoveWave()
    {
        Animalanim.Stop();
        Animalanim.Play("Animal_Wave");
    }

    public void WaitAction()
    {
        Animalanim.Stop();
        Animalanim.Play("Animal_Wait");
    }

    public void RecoverAnimal(int hppoint)
    {
        TextureRoot.SetActive(true);
        lifeRoot.SetActive(true);
        HeartRoot.SetActive(false);

        state = ANIMAL_STATE.WAIT;

        Animalanim.Stop();
        Animalanim.Play("Animal_Wait");

        float recoverHp = ((float)(data.hp * hppoint)) * 0.01f;
        life = (int)recoverHp;
        lifeSlider.value = (float)life / (float)data.hp;
        lifeLabel.text = life.ToString();

        attackPoint = 0;
        skillItem.ResetSkill();
    }

    public void CountHeart()
    {
        if (state != ANIMAL_STATE.DEAD)
            return;

        heartCount--;

        if (heartCount <= 0)        
            RecoverAnimal(30);
    }

    public void SelectHealAnimalByItem()
    {
        if (GameItemManager.instance == null || 
            GameItemManager.instance.used == true ||
            GameItemManager.instance.type != GameItemType.HEAL_ONE_ANIMAL)
            return;

        if (GetState() != ANIMAL_STATE.DEAD)
            return;                

        GameItemManager.instance.UseAnimalItem(this);
    }

    public void ClearStage()
    {
        Animalanim.Stop();
        Animalanim.Play("Animal_Clear");
    }

    /////////이펙트 사용
    public void ShowScreenEffect()
    {
        GameUIManager.instance.SetAdventureEffectBG(true);

        foreach(var temp in MainUrlTexture)        
            temp.depth = 60;   
    }

    public void HideScreenEffect()
    {
        GameUIManager.instance.SetAdventureEffectBG(false);

        foreach (var temp in MainUrlTexture)
            temp.depth = 10 - pos;
    }

    public void animalSkill_Attack_Cast01()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position, "animalSkill_Attack_Cast01");
    }

    public void animalSkill_Attack_Cast02()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture[0].transform.position, "animalSkill_Attack_Cast02");
    }

    public void animalSkill_Heal_Cast01()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position, "animalSkill_Heal_Cast01");
    }

    public void animalSkill_Heal_Cast02()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture[0].transform.position, "animalSkill_Heal_Cast02");
    }

    public void animalSkill_Stun_Cast01()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position, "animalSkill_Stun_Cast01");
    }

    public void animalSkill_Stun_Cast02()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture[0].transform.position, "animalSkill_Stun_Cast02");
    }
    
}
