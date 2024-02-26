using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameEnemy : MonoBehaviour, InGameSkillCaster {

    public event System.Action<int> initEvent;
    public event System.Action<int> damageEvent;

    [SerializeField]
    ANIMAL_STATE state = ANIMAL_STATE.STOP;
    public ANIMAL_STATE GetState()
    {
        return state;
    }

    //동물 데이타
    public EnemyInfo data;
    public int pos = 0;
    int tempPos = 0;

    //public UITexture MainTexture;
    public UIUrlTexture MainUrlTexture;
    public UILabel CountLable;
    public UISprite attTypeSprite;
    public GameObject shadowObj;
    public Animation Animalanim;
    public GameObject CenterObj;
    public InGameTreasure treasure;

    //생명게이지
    public UIItemAdventureLifeBar lifeSlider;
    [SerializeField] private UIItemAdventureDamageMeter damageMeter; 

    int AttackTurn = 3;
    int waitCount = 0;
    public int stunCount = 0;

    //info
    [SerializeField]    UIItemAdventureInfo InfoItem;
    [SerializeField]    GameObject Info_Root;
    [SerializeField]    UILabel Info_att_label;

    //원거리 공격용 투사체
    [SerializeField] private InGameBullet bullet;
    private const string defaultBulletImage = "Adventure/bullet_normal";
    private const string defaultBulletImage_Boss = "Adventure/bullet_boss";

    int waitTurn
    {
        set
        {
            int transValue = value - (waitCount + stunCount);

            if(transValue >= 0)
            {
                waitCount += transValue;
            }
            else
            {
                if (stunCount == transValue * -1)
                {
                    StunEnd();
                }

                stunCount += transValue;
                if (stunCount < 0)
                {
                    waitCount += stunCount;
                    stunCount = 0;
                }
            }
        }
        get
        {
            return waitCount + stunCount;
        }
    }

    [SerializeField]
    int life = 0;
    int SetanimalPos = -1;

    bool isDropTreasure = false;

    public bool IsDropTreasure
    {
        get
        {
            return isDropTreasure;
        }
        set
        {
            isDropTreasure = value;
        }
    }

    private InGameSkillItem_Enemy skillItemObj = null;

    public Transform CasterTransform { get { return MainUrlTexture.transform; } }

    public bool IsAnimal
    {
        get
        {
            return false;
        }
    }

    public int SkillIndex {
        get
        {
            if(skillItemObj != null && skillItemObj.CurrentSkill != null)
            {
                return skillItemObj.CurrentSkill.skill;
            }

            return 0;
        }
    }

    public int SkillGrade
    {
        get
        {
            if (skillItemObj != null && skillItemObj.CurrentSkill != null)
            {
                return skillItemObj.CurrentSkill.skillGrade;
            }

            return 0;
        }
    }

    public void init(EnemyInfo tempData, int tempPos = 0)
    {
        //기본초기화
        AttackTurn = tempData.TurnCount;
        waitTurn = tempData.TurnCount;

        CountLable.text = waitCount.ToString();
        if (waitCount == 1) ShowWarning = true;

        data = tempData;
        pos = tempPos;

        transform.localPosition = AdventureManager.instance.EnemyPos(tempPos);

        attTypeSprite.spriteName = string.Format("icon_{0:D3}", tempData.attribute);

        if (tempData.isBoss)
            SetBossEnemy();
        else
            SetNormalEnemy();

        MainUrlTexture.depth = 10 - pos;
        MainUrlTexture.SettingTextureScale(AdventureManager.ANIMAL_SIZE, AdventureManager.ANIMAL_SIZE);
        MainUrlTexture.SuccessEvent += textureLoaded;
        MainUrlTexture.LoadCDN(Global.adventureDirectory, "Animal/", string.Format("m_{0:D4}", tempData.idx));

        life = data.life;
        lifeSlider.Init(1.0f, data.life);
        lifeSlider.gameObject.transform.localPosition = new Vector3(0, AdventureManager.LIFE_POS_Y);
        lifeSlider.isEnemy = true;

        StartCoroutine(DamageMeterInit());

        Info_Root.SetActive(false);
        Info_Root.transform.localPosition = new Vector3(0, -150 + data.enemyHeight * 0.86f, 0);
        Info_att_label.text = data.attPoint.ToString();

        Animalanim.Stop();
        Animalanim.Play("Enemy_Start");
        state = ANIMAL_STATE.WAIT;

        SetSkill();
    }

    private System.Action textureLoaded;

    private void SetBossEnemy()
    {
        bullet.SetLocalTexture(defaultBulletImage_Boss);

        if (Global.GameType == GameType.ADVENTURE_EVENT && ManagerAdventure.EventData.GetAdvEventStageCount() == Global.stageIndex)
        {
            textureLoaded = () =>
            {
                var effect = MainUrlTexture.gameObject.AddComponent<AuraEffect_RenderTexture>();
                effect.Make
                (
                    ObjectMaker.Instance.GetPrefab("AuraEffect_01"), 
                    new Color(219.0f / 255.0f, 0.0f / 255.0f, 126.0f / 255.0f, 150.0f / 255.0f)
                );
            };
        }
    }

    private void SetNormalEnemy()
    {
        bullet.SetLocalTexture(defaultBulletImage);
    }

    private List<EnemySkillInfo> GetTestSkillData()
    {
        List<EnemySkillInfo> tempList = new List<EnemySkillInfo>();

        var skill_1 = new EnemySkillInfo();
        skill_1.skill = (int)ENEMY_SKILL_TYPE.INSTALL_HP_PLANT_3;
        skill_1.skillGrade = 5;
        skill_1.skillMaxCount = 2;
        tempList.Add(skill_1);

        var skill_2 = new EnemySkillInfo();
        skill_2.skill = (int)ENEMY_SKILL_TYPE.INSTALL_SP_PLANT_3;
        skill_2.skillGrade = 5;
        skill_2.skillMaxCount = 2;
        tempList.Add(skill_2);

        var skill_3 = new EnemySkillInfo();
        skill_3.skill = (int)ENEMY_SKILL_TYPE.INSTALL_ICE_1;
        skill_3.skillGrade = 5;
        skill_3.skillMaxCount = 2;
        tempList.Add(skill_3);

        var skill_4 = new EnemySkillInfo();
        skill_4.skill = (int)ENEMY_SKILL_TYPE.INSTALL_NET_1;
        skill_4.skillGrade = 5;
        skill_4.skillMaxCount = 2;
        tempList.Add(skill_4);

        var skill_5 = new EnemySkillInfo();
        skill_5.skill = (int)ENEMY_SKILL_TYPE.INSTALL_ROCK_1;
        skill_5.skillGrade = 5;
        skill_5.skillMaxCount = 2;
        tempList.Add(skill_5);

        //data.skillRotation = (int)SKILL_ROTATION_TYPE.RANDOM_SEQUNTIAL;

        return tempList;
    }

    private IEnumerator DamageMeterInit()
    {
        while (MainUrlTexture.mainTexture == null)
            yield return new WaitForSeconds(0.1f);

        damageMeter.Init(MainUrlTexture.mainTexture.GetHeight());
    }

    bool ShowWarning = false;
    public void AddAttackPoint()
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        if (skillItemObj != null && stunCount == 0)
            skillItemObj.AddSkillPoint();

        if (waitTurn <= 0)
        {
            waitCount = AttackTurn;
        }
        else
        {
            waitTurn--;

            if (waitTurn <= 1)            
                ShowWarning = true;            
        }

        //턴변화는 애니메이션
        CountLable.text = waitCount.ToString();
    }

    public bool IsAttackTurn()
    {
        if (state == ANIMAL_STATE.DEAD)
            return false;

        return waitTurn <= 0;
    }

    public void DemageAnimal()
    {
        if (SetanimalPos == -1)
            return;

        AdventureManager.instance.DemageAnimal(SetanimalPos, data.attPoint);
    }

    private InGameAnimal attackTarget;
    public void Attack(int enemyPos, bool isRangedAttack = false)
    {
        if (enemyPos == -1)
            return;

        //동물데미지 미리 계산하기
        foreach (var temp in AdventureManager.instance.AnimalLIst)
        {
            if (temp.pos == enemyPos)
            {
                attackTarget = temp;

                if (temp.GetLife() <= data.attPoint)
                    temp.CheckBeforeDead = true;

                break;
            }
        }

        ShowWarning = false;
        CountLable.color = Color.white;

        SetanimalPos = enemyPos;
        state = ANIMAL_STATE.ATTACK;
        StartCoroutine(DoAttack(isRangedAttack));
    }

    IEnumerator DoAttack(bool isRangedAttack)
    {
        string animName;

        if (!isRangedAttack)
        {
            int distance;
            if (ManagerBlock.instance.stageInfo.battleWaveList[AdventureManager.instance.waveCount].enemyIndexList.Count == 1)
                distance = SetanimalPos + 2;
            else
                distance = SetanimalPos + pos + 1;

            animName = "Enemy_Att_" + distance;
        }
        else
        {
            animName = "Enemy_Att_Ranged";
        }

        Animalanim.Stop();
        Animalanim.Play(animName);
        //ManagerSound.AudioPlay(Random.value >= 0.5f ? AudioInGame.ENEMY_ATTACK_1 : AudioInGame.ENEMY_ATTACK_2);

        float timer = 0f;     
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        while (Animalanim.IsPlaying(animName))
            yield return null;

        waitCount = AttackTurn;
        CountLable.text = waitCount.ToString();

        if (waitTurn <= 1)
            ShowWarning = true;   

        state = ANIMAL_STATE.WAIT;
        yield return null;
    }

    /// <summary>
    /// 공격해야 될 턴에 공격을 하지 못할 경우 공격 초기화
    /// </summary>
    public void InitAttack()
    {
        ShowWarning = false;
        CountLable.color = Color.white;

        waitCount = AttackTurn;
        CountLable.text = waitCount.ToString();

        if (waitTurn <= 1)
            ShowWarning = true;

        state = ANIMAL_STATE.WAIT;
    }

    private InGameAnimal attacker;
    public void Pang(InGameAnimal attacker, int pangPoint, bool isSkill = false, int SkillType = 0)
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        this.attacker = attacker;
        life -= pangPoint;
        lifeSlider.Value = (float)life / (float)data.life;
        damageMeter.Damage(data.life, pangPoint);
        
        if (life > 0)
        {
            state = ANIMAL_STATE.PANG;
            StartCoroutine(DoPangAni(isSkill, SkillType));
            enemySound_Hit();
        }
        else
        {
            state = ANIMAL_STATE.DEAD;
            StartCoroutine(DoDeadAni(isSkill, SkillType));
            enemySound_Dead();
        }
    }

    public void PangEnemyByTool()
    {
        life = 0;
        state = ANIMAL_STATE.DEAD;
        MainUrlTexture.gameObject.SetActive(false);
        shadowObj.SetActive(false);
    }

    IEnumerator DoPangAni(bool isSkill, int SkillType)
    {
        string animName = "Enemy_Demage";

        if (isSkill)
        {
            switch (SkillType)
            {
                case 0: // 번개
                    animName = "Enemy_Skill_Lighting_Demage";
                    break;
                case 1: // 하트
                    animName = "Enemy_Skill_Heart_Demage";
                    break;
                default:
                    break;
            }
        }

        Animalanim.Stop();
        Animalanim.Play(animName);
        //ManagerSound.AudioPlay(AudioInGame.HIT_ENEMY);

        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        while (Animalanim.IsPlaying(animName))
            yield return null;

        if(state != ANIMAL_STATE.DEAD)
            state = ANIMAL_STATE.WAIT;

        yield return null;
    }

    IEnumerator DoDeadAni(bool isSkill, int SkillType)
    {
        if (stunCount > 0)
            StunEnd();

        string animName = "Enemy_" + (data.isBoss ? "Boss" : "") + "Dead";

        if (isSkill)
        {
            switch (SkillType)
            {
                case 0: // 번개
                    animName = "Enemy_Skill_Lighting_" + (data.isBoss ? "Boss" : "") + "Dead";
                    break;
                case 1: // 하트
                    animName = "Enemy_Skill_Heart_" + (data.isBoss ? "Boss" : "") + "Dead";
                    break;
                default:
                    break;
            }
        }
        
        Animalanim.Stop();
        Animalanim.Play(animName);
        
        if (data.isBoss)
        {
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < 10; i++)
            {
                InGameEffectMaker.instance.MakeFlyCoinAdventureBoss(CenterObj.transform.position, 1);
                yield return new WaitForSeconds(0.03f);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }        

        while (Animalanim.IsPlaying(animName))
            yield return null;

        MainUrlTexture.gameObject.SetActive(false);
        shadowObj.SetActive(false);

        if (isDropTreasure == true)
        {
            treasure.gameObject.SetActive(true);
            treasure.SetTreasureImage((TreasureType)AdventureManager.instance.TreasureType);
            StartCoroutine(treasure.CoDropAction());
        }

        yield return null;

        DestroySkillItem();
    }


    void Update()
    {
        switch (state)
        {
            case ANIMAL_STATE.WAIT:
                if (Animalanim.isPlaying == false && stunCount == 0)  
                    Animalanim.Play("Enemy_Wait");                
                break;
        }

        if (ShowWarning)
            CountLable.color = Color.Lerp(Color.white, Color.red, Mathf.Sin(Mathf.PI * ManagerBlock.instance.BlockTime*8));
        
    }
    
    //보물획득.
    public void GetTreasure()
    {
        if (isDropTreasure)
        {
            AdventureManager.instance.TreasureCnt++;
            treasure.GetAction();
        }
    }

    public void GaigeStun()
    {
        stunCount++;
        EnemySkill_Frozen_Hit01();
    }

    public void Stun(int count = 1)
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        if (stunCount == 0)
        {
            Animalanim.Stop();
            Animalanim.Play("Enemy_Skill_Frozen_Demage");
        }
        
        stunCount = count;
    }

    private void StunEnd()
    {
        Destroy(frozenObject);
        frozenObject = null;
    }

    public void ActiveAtBlind(bool active)
    {
        int activeValue = active ? 60 : -60;

        if(frozenObject != null)
        {
            var effect = frozenObject.GetComponentsInChildren<UIWidget>();

            for(int i=0; i<effect.Length; i++)
                effect[i].depth += activeValue;
        }

        MainUrlTexture.depth += activeValue;

        var lifebars = lifeSlider.GetComponentsInChildren<UIWidget>();
        foreach (var bar in lifebars)
            bar.depth += activeValue;
    }

    public void EnemyLightingSkillHitEffect()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture.transform.position / CenterObj.transform.lossyScale.x, "EnemyLightingSkillHitEffect");
    }

    public void EnemyHeartSkillHitEffect()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture.transform.position / CenterObj.transform.lossyScale.x, "EnemyHeartSkillHitEffect");
    }

    public void EnemyStunSkillHitEffect()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture.transform.position / CenterObj.transform.lossyScale.x, "EnemyStunSkillHitEffect");
    }

    public void EnemyHitEffect()
    {
        var effect = InGameEffectMaker.instance.MakeAdventureEffect(CenterObj.transform.position / CenterObj.transform.lossyScale.x, "EnemyHitEffect");

        if (attacker != null && attacker.hitEffectTexture_1 != null)
        {
            ParticleSystemRenderer render = effect.transform.Find("particle_Star01").GetComponent<ParticleSystemRenderer>();
            var material = new Material(render.material);
            material.SetTexture("_MainTex", attacker.hitEffectTexture_1);
            render.material = material;
        }

        if (attacker != null && attacker.hitEffectTexture_2 != null)
        {
            ParticleSystemRenderer render = effect.transform.Find("Particle_Hit01").GetComponent<ParticleSystemRenderer>();
            var material = new Material(render.material);
            material.SetTexture("_MainTex", attacker.hitEffectTexture_2);
            render.material = material;
        }
    }

    private GameObject frozenObject;
    public void EnemySkill_Frozen_Hit01()
    {
        frozenObject = InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture.transform.position / CenterObj.transform.lossyScale.x, "EnemySkill_Frozen_Hit01", Mathf.Infinity);
    }

    public void enemySound_Attack()
    {
        if (attackTarget.enemyHitSound != null)
            ManagerSound.AudioPlay(attackTarget.enemyHitSound);
        else
            ManagerSound.AudioPlay(Random.value >= 0.5f ? AudioInGame.ENEMY_ATTACK_1 : AudioInGame.ENEMY_ATTACK_2);
    }

    public void enemySound_Hit()
    {
        if (attacker != null && attacker.enemyDamageSound != null)
            ManagerSound.AudioPlay(attacker.enemyDamageSound);
        else
            ManagerSound.AudioPlay(AudioInGame.HIT_ENEMY);
    }

    private void enemySound_Dead()
    {
        if(attacker != null && attacker.enemyDamageSound != null)
            ManagerSound.AudioPlay(attacker.enemyDamageSound);
        else
            ManagerSound.AudioPlay(AudioInGame.HIT_ENEMY);

        if (data.isBoss)
            ManagerSound.AudioPlay(AudioInGame.BOSS_DISAPPEAR);
        else
            ManagerSound.AudioPlay(AudioInGame.ENEMY_DISAPPEAR_1);
    }


    public void ShowEnemyInfo()
    {
        if (GetState() == ANIMAL_STATE.DEAD)
            return;
        
        InfoItem.ShowInfo();

        if(skillItemObj != null)
            skillItemObj.ShowInfo();
    }

    public void ShotBullet()
    {
        foreach(var animal in AdventureManager.instance.AnimalLIst)
        {
            if(animal.pos == SetanimalPos)
            {
                bullet.Shot(animal.HeartRoot.transform, DemageAnimal);
                break;
            }
        }
    }

    private void SetSkill()
    {
        //test//
        //data.skillList = GetTestSkillData();
        //data.skillRotation = (int)InGameSkillItem_Enemy.SKILL_ROTATION_TYPE.RANDOM;
        //////

        if (data.skillList.Count == 0)
        {
            return;
        }

        skillItemObj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, AdventureManager.instance.skillItem_Enemy_Obj).GetComponent<InGameSkillItem_Enemy>();
        skillItemObj.InitSkill(this, data.skillList, (InGameSkillItem_Enemy.SKILL_ROTATION_TYPE)data.skillRotation);
        skillItemObj.skillCaster = this;

        Vector3 position = skillItemObj.transform.localPosition;
        position.x = transform.localPosition.x;
        position.y = AdventureManager.SKILL_ITEM_POS_Y;
        skillItemObj.transform.localPosition = position;
    }

    private void DestroySkillItem()
    {
        if(skillItemObj != null)
        {
            Destroy(skillItemObj.gameObject);
            skillItemObj = null;
        }
    }

    public bool UseSkill()
    {
        if (skillItemObj == null ||
            !skillItemObj.isFull)
        {
            return false;
        }

        var skill = skillItemObj.GetSkill();

        if(skill == null)
        {
            return false;
        }

        IsSkillEnd = false;

        skill.EndSkillEvent += () => 
        {
            skillItemObj.SkillChange();
            skillItemObj.ResetSkill();
            IsSkillEnd = true;
        };

        Animalanim.Stop();
        Animalanim.Play("Enemy_Skill");

        CoEnemySkill = skill.UesSkill;

        return true;
    }

    public System.Func<IEnumerator> CoEnemySkill;
    public bool IsSkillEnd { get; private set; } = true;

    //private void EndSkill()
    //{
    //    CoEnemySkill = null;

    //    SkillChange();
    //    skillItemObj.ResetSkill();
    //}
}
