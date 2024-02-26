using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    CHARGE,
    REVIVE,
}

public class InGameAnimal : MonoBehaviour, InGameSkillCaster
{
    BlockColorType colorType = BlockColorType.NONE;
    public BlockColorType GetColor()
    {
        return colorType;
    }

    [SerializeField]
    ANIMAL_STATE state = ANIMAL_STATE.STOP;
    public ANIMAL_STATE GetState()
    {
        return state;
    }

    //공격모션 전에 죽을것인지 체트
    public bool CheckBeforeDead = false;

    //동물 데이타
    public ManagerAdventure.AnimalInstance data;
    public int pos = 0;
    int tempPos = 0;

    public GameObject AnimalRoot;
    public GameObject TextureRoot;
    public UIUrlTexture MainUrlTexture;    
    public UILabel[] AttLabel;
    public UISprite attTypeSprite;  //색상정보
    public Animation Animalanim;

    //생명게이지
    public UIItemAdventureLifeBar lifeSlider;
    [SerializeField]private UIItemAdventureDamageMeter damageMeter;

    public GameObject HeartRoot;
    public UILabel heartCountLabel;

    //스킬
    public InGameSkillItem_Animal skillItem;
    public GameObject skillItemObj;

    //공격력 표시
    public GameObject NormalScore;
    public GameObject MaxScore;

    public UISprite adventureItemOutLine;

    public GameObject EffectPosObj;

    //속성 추가 공격력
    public float totalAttPoint = 1f;

    //info
    [SerializeField]UIItemAdventureInfo InfoItem;
    [SerializeField]GameObject Info_Root;
    [SerializeField]UILabel Info_att_label;
    [SerializeField]UILabel Info_att_label_atr;
    //[SerializeField]UISprite Info_att_BG;

    public static int isChargingCount = 0;

    //원거리 공격용 투사체
    [SerializeField] private InGameBullet bullet;
    private const string defaultBulletImage = "Adventure/bullet_normal";

    const int MAX_ATT_POINT = 10;
    int addMaxAttPoint = 0;

    [SerializeField]
    int attackPoint = 0;
    int life = 0;

    public Transform CasterTransform { get { return MainUrlTexture.transform; } }

    public int GetLife()
    {
        return life; 
    }
    
    int heartCount = 10;
    int SetEnemyPos = -1;
    int comboCount = 0;

    const float SCORE_SCALE = 0.05f;
    const float ANIMAL_COMBO_SCALE = 0.05f;

    private ResourceBox box;

    private Coroutine _healItemGuideCoroutine = null;

    private void Awake()
    {
        box = ResourceBox.Make(gameObject);
    }

    public void init(ManagerAdventure.AnimalInstance tempData, int tempPos = 0)
    {
        //기본초기화
        attackPoint = 0;

        life = tempData.hp;
        data = tempData;

        //test skill
        //data.skill = (int)ANIMAL_SKILL_TYPE.ADD_LINE_BOMB;
        //data.skillGrade = 3;
        //

        pos = tempPos;

        colorType = (BlockColorType)(tempPos + 1);

        int attackWeight = AttributeCalculator.Calculate(ManagerBlock.instance.stageInfo.bossInfo.attribute, data.attr, ManagerBlock.instance.stageInfo.bossInfo.attrSize);
        if(Global.GameType == GameType.ADVENTURE_EVENT)
        {
            attackWeight += ManagerAdventure.EventData.GetAdvEventBonus(data.idx);
        }

        if(attackWeight > 0)
        {
            totalAttPoint = 1.0f + Mathf.Clamp(attackWeight * 0.01f, 0.0f, Mathf.Infinity);
            Info_att_label_atr.text = "+" + attackWeight + "%";
            Info_att_label_atr.color = new Color(99f / 255f, 1f, 63 / 255);
            Info_att_label_atr.effectColor = new Color(25 / 255f, 103 / 255f, 0f);
        }
        else if(attackWeight < 0)
        {
            totalAttPoint = Mathf.Clamp(1.0f + attackWeight * 0.01f, 0.0f, Mathf.Infinity);
            Info_att_label_atr.text = attackWeight + "%";
            Info_att_label_atr.color = new Color(1f, 99 / 255, 99 / 255);
            Info_att_label_atr.effectColor = new Color(156 / 255f, 0f, 0f);
        }
        else
        {
            totalAttPoint = 1f;
            Info_att_label_atr.text = "";
        }

        attTypeSprite.spriteName = "icon_block_" + ManagerUI.GetColorTypeString(GetColor());
        attTypeSprite.MakePixelPerfect();
        attTypeSprite.cachedTransform.localScale = Vector3.one * 1.2f;

        transform.localPosition = AdventureManager.instance.animalPos(tempPos);

        MainUrlTexture.depth = GetMainTextureDepth(pos);

        MainUrlTexture.SettingTextureScale(AdventureManager.ANIMAL_SIZE, AdventureManager.ANIMAL_SIZE);
        MainUrlTexture.LoadCDN(Global.adventureDirectory, "Animal/", ManagerAdventure.GetAnimalTextureFilename(tempData.idx, tempData.lookId));
        //MainUrlTexture.MakePixelPerfect();
        
        lifeSlider.Init((float)life / (float)data.hp, life);
        lifeSlider.gameObject.transform.localPosition = new Vector3(0, AdventureManager.LIFE_POS_Y);
        StartCoroutine(DamageMeterInit());

        SetAnimation("Animal_Out_wait");

        EffectPosObj.transform.localPosition = new Vector3(0, -70 + ((float)data.animalSize) * 0.5f, 0f);

        Info_Root.SetActive(false);
        Info_Root.transform.localPosition = new Vector3(0, -150 + data.animalSize * 0.86f, 0);// 
        Info_att_label.text = data.atk.ToString();
        //rhd

        SetRangedAttackInfo(tempData);
    }

    int GetMainTextureDepth(int tempPos)
    {
        return 10 + tempPos;

        if (tempPos == 0)
            return 9;
        else if (tempPos == 1)
            return 8;
        else //if (tempPos == 2)
            return 10;
    }

    private void SetRangedAttackInfo(ManagerAdventure.AnimalInstance animalData)
    {
        if (animalData.atkType == 1)
        {
            if(animalData.bulletImageName == null || animalData.bulletImageName == "0")
                bullet.SetLocalTexture(defaultBulletImage);
            else
                bullet.SetCDNTexture(animalData.bulletImageName);
        }

        if (animalData.protectedFromMelee)
        {
            LoadAudioClip(animalData.animalHitSoundName, (AudioClip audio) => animalHitSound = audio);
            LoadAudioClip(animalData.animalDamageSoundName, (AudioClip audio) => animalDamageSound = audio);
            LoadAudioClip(animalData.enemyHitSoundName, (AudioClip audio) => enemyHitSound = audio);
            LoadAudioClip(animalData.enemyDamageSoundName, (AudioClip audio) => enemyDamageSound = audio);

            if(animalData.damageEffectName_1 != null && animalData.damageEffectName_1 != "0")
            {
                box.LoadCDN(
                    "Effect", 
                    animalData.damageEffectName_1, 
                    (Texture2D texture) => damageEffectTexture_1 = texture
                );
            }

            if (animalData.damageEffectName_2 != null && animalData.damageEffectName_2 != "0")
            {
                box.LoadCDN(
                    "Effect",
                    animalData.damageEffectName_2,
                    (Texture2D texture) => damageEffectTexture_2 = texture
                );
            }

            if (animalData.hitEffectName_1 != null && animalData.hitEffectName_1 != "0")
            {
                box.LoadCDN(
                    "Effect",
                    animalData.hitEffectName_1,
                    (Texture2D texture) => hitEffectTexture_1 = texture
                );
            }

            if (animalData.hitEffectName_2 != null && animalData.hitEffectName_2 != "0")
            {
                box.LoadCDN(
                    "Effect",
                    animalData.hitEffectName_2,
                    (Texture2D texture) => hitEffectTexture_2 = texture
                );
            }
        }
    }

    private void LoadAudioClip(string soundName, System.Action<AudioClip> loadedEvent)
    {
        if (soundName == null || soundName == "0")
            loadedEvent(null);
        else if (soundName == "NOSOUND")
            loadedEvent(AudioClip.Create("NOSOUND", 1, 1, 44100, false));
        else
        {
            box.LoadCDN(
                "Sound",
                soundName,
                (AudioClip audio) => loadedEvent(audio)
            );
        }
    }

    private AudioClip animalHitSound;
    private AudioClip animalDamageSound;
    public AudioClip enemyHitSound { private set; get; }
    public AudioClip enemyDamageSound{ private set; get; }

    private Texture damageEffectTexture_1;
    private Texture damageEffectTexture_2;
    public Texture hitEffectTexture_1 { private set; get; }
    public Texture hitEffectTexture_2 { private set; get; }

    public bool IsAnimal
    {
        get
        {
            return true;
        }
    }

    public int SkillIndex
    {
        get
        {
            return data.skill;
        }
    }

    public int SkillGrade
    {
        get
        {
            return data.skillGrade;
        }
    }

    void SetAnimation(string animName)
    {
        Animalanim.Stop();
        Animalanim.Play(animName);
        //Debug.Log("동물공격 모션 " + animName);
    }


    private IEnumerator DamageMeterInit()
    {
        while (MainUrlTexture.mainTexture == null)
            yield return new WaitForSeconds(0.1f);

        damageMeter.Init(MainUrlTexture.mainTexture.GetHeight());
    }

    public void StartAnimal()
    {
        SetAnimation("Animal_Start");
        state = ANIMAL_STATE.WAIT;
    }

    //int BonusComboCount = 0;
    float COMBO_BONUS_RATIO = 0.05f;
    public void AddComboBonus()
    {
        if (GetState() == ANIMAL_STATE.DEAD)
            return;

        ManagerSound.AudioPlayMany(AudioInGame.CHARGING);        
        
        if (attackPoint > 0)
        {


            int addCount = Mathf.RoundToInt(data.atk * attackPoint * COMBO_BONUS_RATIO + 0.005f);

            if (addCount < 1) 
                addCount = 1;

            if (comboCount < 20)
            {
                //BonusComboCount += addCount;// (int)(((float)data.atk * attackPoint) * COMBO_BONUS_RATIO);//*ManagerBlock.instance.comboCount);            
                comboCount++;
            }
//            if (data.atk * attackPoint + BonusComboCount > (MAX_ATT_POINT + addMaxAttPoint) * data.atk)
//                BonusComboCount = (MAX_ATT_POINT + addMaxAttPoint) * data.atk - data.atk * attackPoint;

            //InGameEffectMaker.instance.MakeComboBonus(AttLabel[0].transform.position, "+" + (ManagerBlock.instance.comboCount * 10) + "%");
            //InGameEffectMaker.instance.MakeComboBonus(AttLabel[0].transform.position, "+" + addCount);

            MainUrlTexture.transform.localScale = Vector3.one * (1 + attackPoint * SCORE_SCALE + comboCount * ANIMAL_COMBO_SCALE);
            
            foreach (var temp in AttLabel)
                temp.color = new Color(1, 2f / 255f, 115f / 255f, 1f);

            AttLabel[1].applyGradient = false;

            StartCoroutine(DoShowAddCombo(comboCount));

            /*
            if (shadowObj == null)
            {
                shadowObj = NGUITools.AddChild(MainUrlTexture[0].gameObject, MainUrlTexture[0].gameObject).GetComponent<UIUrlTexture>();
                shadowObj.gameObject.transform.localScale = Vector3.one * 1.1f;
                shadowObj.depth--;
                shadowObj.color = Color.red;
                shadowObj.alpha = 0.7f;
            }
            */
        }
    }

    UIUrlTexture shadowObj = null;

    IEnumerator DoShowAddCombo(int tempComboCount)
    {
        int totalCount = Mathf.RoundToInt(attackPoint * data.atk * (1 + (tempComboCount * COMBO_BONUS_RATIO)) * totalAttPoint + 0.005f); //Mathf.FloorToInt((data.atk * attackPoint + BonusComboCount) * totalAttPoint);
        int startCount = Mathf.RoundToInt(data.atk * attackPoint * totalAttPoint + 0.005f);

        yield return null;

        float showTimer = 0;
        while (showTimer < 1f)
        {
            float posY = AdventureManager.instance.animCurveTextPosY.Evaluate(showTimer);
            float scaleRatio = AdventureManager.instance.animCurveTextScale.Evaluate(showTimer);

            NormalScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize + posY * 10 + comboCount * 3, 0);
            MaxScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize + posY * 10 + comboCount * 3, 0);

            NormalScore.transform.localScale = Vector3.one * (1 + comboCount * SCORE_SCALE) * scaleRatio;
            MaxScore.transform.localScale = Vector3.one * (1 + comboCount * SCORE_SCALE) * scaleRatio;

            int displayCount = Mathf.RoundToInt(Mathf.Lerp((float)startCount, (float)totalCount, showTimer));
            foreach (var temp in AttLabel)
                temp.text = displayCount.ToString();            

            showTimer += Global.deltaTimePuzzle * 4f;
            yield return null;
        }

        foreach (var temp in AttLabel)        
            temp.text = totalCount.ToString();

        yield return null;
    }


    public void AddAttackPoint(int tempPoint = 1)
    {
        if (GetState() == ANIMAL_STATE.DEAD)
            return;

        isChargingCount++;
        if (Animalanim.IsPlaying("Animal_Charge") == false)
        {
            SetAnimation("Animal_Charge");
        }

        ManagerSound.AudioPlay(AudioInGame.BLOCK_CHARGING);

        if (attackPoint >= MAX_ATT_POINT + addMaxAttPoint)
        {
            attackPoint = MAX_ATT_POINT + addMaxAttPoint;

            if (ManagerBlock.instance.coins < 100)
            {
                InGameEffectMaker.instance.MakeFlyCoinAdventure(transform.position, 1);                
            }

            isChargingCount--;
            return;
        }

        attackPoint++;

        MainUrlTexture.transform.localScale = Vector3.one * (1 + attackPoint * SCORE_SCALE + comboCount * ANIMAL_COMBO_SCALE); 

        foreach (var temp in AttLabel)
        {
            //temp.text = (data.atk * attackPoint).ToString();
            temp.depth = 15 - tempPos;
        }

        if (attackPoint >= MAX_ATT_POINT + addMaxAttPoint)
        {
            MaxScore.SetActive(true);
            NormalScore.SetActive(false);
            MaxScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize, 0);

            if (ShowAttLabel)
            {
                ShowAttTimer = 0f;
                isChargingCount--;
            }
            else
            {
                ShowAttLabel = true;
                StartCoroutine(DoShowAttLabel((Mathf.RoundToInt(data.atk * attackPoint * totalAttPoint + 0.005f)).ToString()));
            }
        }
        else
        {
            MaxScore.SetActive(false);
            NormalScore.SetActive(true);
            NormalScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize, 0);

            if (ShowAttLabel)
            {
                ShowAttTimer = 0f;
                isChargingCount--;
            }
            else
            {
                ShowAttLabel = true;
                StartCoroutine(DoShowAttLabel(((Mathf.RoundToInt(data.atk * attackPoint * totalAttPoint + 0.005f)).ToString())));
            }
        }        
    }

    float ShowAttTimer = 0f;
    bool ShowAttLabel = false;

    IEnumerator DoShowAttLabel(string text, bool maxScore = false)
    {
        while (ShowAttTimer < 1f)
        {
            int displayCount = Mathf.RoundToInt(Mathf.Lerp((float)Mathf.RoundToInt(data.atk * (attackPoint - 1) * totalAttPoint + 0.005f), (float)Mathf.RoundToInt(data.atk * attackPoint * totalAttPoint + 0.005f), ShowAttTimer));

            foreach (var temp in AttLabel)
                temp.text = displayCount.ToString();

            ShowAttTimer += Global.deltaTimePuzzle * 4;

            float ratioA = ManagerBlock.instance._curveBlockPopUp.Evaluate(ShowAttTimer);
            MaxScore.transform.localScale = Vector3.one * (1 + comboCount * SCORE_SCALE) * ratioA;
            NormalScore.transform.localScale = Vector3.one * (1 + comboCount * SCORE_SCALE) * ratioA;

            float posY = AdventureManager.instance.animCurveTextPosY.Evaluate(ShowAttTimer);
            float scaleRatio = AdventureManager.instance.animCurveTextScale.Evaluate(ShowAttTimer);

            NormalScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize + posY * 10 + comboCount * 3, 0);
            MaxScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize + posY * 10 + comboCount * 3, 0);
            yield return null;
        }

        MaxScore.transform.localScale = Vector3.one * (1 + comboCount * SCORE_SCALE);
        NormalScore.transform.localScale = Vector3.one * (1 + comboCount * SCORE_SCALE);

        NormalScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize  + comboCount * 3, 0);
        MaxScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize + comboCount * 3, 0);

        foreach (var temp in AttLabel)
            temp.text = (Mathf.RoundToInt(data.atk * attackPoint * totalAttPoint + 0.005f)).ToString();

        ShowAttLabel = false;
        ShowAttTimer = 0f;

        isChargingCount--;
        yield return null;


    }

    public void ResetAnimal()
    {
        if (attackPoint > 0)
            StartCoroutine(DoRecoverScale());

        attackPoint = 0;
        comboCount = 0;
        
        MaxScore.transform.localScale = Vector3.one;
        NormalScore.transform.localScale = Vector3.one;
        MaxScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize, 0);
        NormalScore.transform.localPosition = new Vector3(-16, 10 + data.animalSize, 0);
        
        MaxScore.SetActive(false);
        NormalScore.SetActive(false);

        foreach (var temp in AttLabel)
            temp.color = Color.white;

        AttLabel[1].applyGradient = true;
    }

    IEnumerator DoRecoverScale()
    {
        float scaleX = MainUrlTexture.transform.localScale.x;

        float waitTimer = 0;
        while (waitTimer < 1f)
        {
            waitTimer += Global.deltaTimePuzzle * 5;
            float scaleRatio = Mathf.Lerp(scaleX, 1f, waitTimer);
          
            MainUrlTexture.transform.localScale = Vector3.one * scaleRatio;

            yield return null;
        }

        MainUrlTexture.transform.localScale = Vector3.one;
        
        yield return null;
    }

    public bool HasAttackPoint()
    {
        return attackPoint > 0;
    }

    public bool isAttacking = false;
    public void Attack(int enemyPos, bool isRangedAttack = false)
    {
        if (enemyPos == -1)
        { 
            ResetAnimal();
            return;
        }

        state = ANIMAL_STATE.ATTACK;
        SetEnemyPos = enemyPos;
        StartCoroutine(DoAttack(isRangedAttack));
        isAttacking = true;

        //Destroy(shadowObj.gameObject);
    }

    IEnumerator DoAttack(bool isRangedAttack)
    {
        string animName;

        if (!isRangedAttack)
        {
            int distance;
            if (ManagerBlock.instance.stageInfo.battleWaveList[AdventureManager.instance.waveCount].enemyIndexList.Count == 1)
                distance = pos + 2;
            else
                distance = SetEnemyPos + pos + 1;

            animName = "Animal_Att_" + distance;
        }
        else
        {
            animName = "Animal_Att_Ranged";
        }

        SetAnimation(animName);
        //ManagerSound.AudioPlay(Random.value >= 0.5f ? AudioInGame.FRIEND_NORMAL_ATTACK_SHORT : AudioInGame.FRIEND_NORMAL_ATTACK_LONG);

        MaxScore.SetActive(false);
        NormalScore.SetActive(false);


        if (isRangedAttack)
        {
            float timer = 0f;
            while (timer < 0.4f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            timer = 0f;
            Vector3 startSize = MainUrlTexture.transform.localScale;
            while (timer < 1f)
            {
                timer += Global.deltaTimePuzzle * 5.0f;

                float ratio = Mathf.Sin(timer * (Mathf.PI * 0.5f));

                MainUrlTexture.transform.localScale = Vector3.Lerp(startSize, Vector3.one, ratio);

                yield return null;
            }
        }
        else
        {
            float timer = 0f;
            while (timer < 0.8f)
            {
                timer += Global.deltaTimePuzzle;
                yield return null;
            }

            MainUrlTexture.transform.localScale = Vector3.one;
        }

        while (Animalanim.IsPlaying(animName))
        {
            yield return null; 
        }

        state = ANIMAL_STATE.WAIT;
        
        MainUrlTexture.transform.localScale = Vector3.one;

        MaxScore.transform.localScale = Vector3.one ;
        NormalScore.transform.localScale = Vector3.one;

        MaxScore.SetActive(false);
        NormalScore.SetActive(false);
        yield return null;

        //isAttacking = false;
    }

    public void AttackEnemy()
    {
        //공격력 계산 
        int totalAttPint = Mathf.RoundToInt(attackPoint * data.atk * (1 + (comboCount * COMBO_BONUS_RATIO)) * totalAttPoint + 0.005f);

        AdventureManager.instance.DemageEnemy(this, SetEnemyPos,totalAttPint);// Mathf.FloorToInt((attackPoint * data.atk + BonusComboCount)*totalAttPoint));
        attackPoint = 0;
        addMaxAttPoint = 0;
        //BonusComboCount = 0;
        comboCount = 0;

        isAttacking = false;
    }

    public void SkillAttackEnemy()
    {
        AdventureManager.instance.DemageEnemy(this, AdventureManager.instance.GetEnemyPos(), data.skillGrade, true);
    }

    public void Pang(int pangPoint)
    {
        if (state == ANIMAL_STATE.DEAD || state == ANIMAL_STATE.PANG)
            return;

        life -= pangPoint;
        lifeSlider.Value = (float)life / (float)data.hp;
        damageMeter.Damage(data.hp, pangPoint);

        if (life > 0)
        {
            state = ANIMAL_STATE.PANG;
            StartCoroutine(DoPangAni());
        }
        else
        {
            state = ANIMAL_STATE.DEAD;
            heartCount = 10;
            if (skillItem != null)
            {
                skillItem.ResetSkill();
                skillItemObj.SetActive(false);
            }
            StartCoroutine(DoDeadAni());
        }
    }

    public void DeadAnimalByTool()
    {
        life = 0;
        state = ANIMAL_STATE.DEAD;

        SetAnimation("Animal_Demage");
        HeartRoot.SetActive(true);

        heartCount = 10;
        heartCountLabel.text = heartCount.ToString();
        TextureRoot.SetActive(false);
    }

    IEnumerator DoPangAni()
    {
        float timer = 0f;
        
        SetAnimation("Animal_Demage");
        //ManagerSound.AudioPlay(AudioInGame.HIT_FRIEND_1);

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
        SetAnimation("Animal_Demage");
        //ManagerSound.AudioPlay(AudioInGame.HIT_FRIEND_1);

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 8f;
            yield return null;
        }

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
        SetAnimation("Animal_PosChange");        
    }

    IEnumerator DoChangePos()
    {
        Vector3 targetPos = AdventureManager.instance.animalPos(tempPos);
        Vector3 startPos = AdventureManager.instance.animalPos(pos);

        MainUrlTexture.depth = GetMainTextureDepth(tempPos);

        float timer = 0f;
        while (true)
        {
            if (tempPos == 1 || pos == 1)
            timer += Global.deltaTimePuzzle * 4f;
            else
                timer += Global.deltaTimePuzzle * 3f;            

            if (timer > 1f)            
                timer = 1f;            

            float ratio = Mathf.Sin(timer * Mathf.PI * 0.5f);
            Vector3 cross = Vector3.Cross((startPos - targetPos).normalized, Vector3.forward);//Vector3.zero;// 

            int frontMoveHeight = 150;
            if (pos == 1) frontMoveHeight = 100;
            
            if (pos < tempPos)            
                transform.localPosition = Vector3.Lerp(startPos, targetPos, ratio) + cross * Mathf.Sin(timer * Mathf.PI) * 50f;  
            else
                transform.localPosition = Vector3.Lerp(startPos, targetPos, ratio) + cross * Mathf.Sin(timer * Mathf.PI) * frontMoveHeight;

            if (skillItem != null)
            skillItemObj.transform.localPosition = Vector3.Lerp(new Vector3(startPos.x, AdventureManager.SKILL_ITEM_POS_Y), new Vector3(targetPos.x, AdventureManager.SKILL_ITEM_POS_Y), ratio);

            if (timer >= 1f)            
                break;       

            yield return null;
        }

        pos = tempPos;
        MainUrlTexture.depth =GetMainTextureDepth(pos);

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

    private InGameSkill skill = null;
    public static List<BlockBase> NormalBlockList { get; private set; } = null;

    public bool UseSkill()
    {
        if (state == ANIMAL_STATE.DEAD)
            return false;

        int skillType = data.skill % 100;

        switch ((ANIMAL_SKILL_TYPE)skillType)
        {
            case ANIMAL_SKILL_TYPE.ATTACK:
                SetAnimation($"Animal_Skill_{data.skill}");
                var attackList = new List<InGameEnemy>();
                foreach (var temp in AdventureManager.instance.EnemyLIst)
                {
                    if (temp.pos == AdventureManager.instance.GetEnemyPos())
                        attackList.Add(temp);
                }
                GameUIManager.instance.AdventureEffectBG_On(new List<InGameAnimal>() { this }, attackList);
                break;

            case ANIMAL_SKILL_TYPE.ATTACK_ALL:
                int skillName = data.skill - 1;
                SetAnimation($"Animal_Skill_{skillName}_all");
                GameUIManager.instance.AdventureEffectBG_On(new List<InGameAnimal>() { this }, AdventureManager.instance.EnemyLIst);
                break;

            case ANIMAL_SKILL_TYPE.HEAL_HP:
                SetAnimation("Animal_Skill_2");
                GameUIManager.instance.AdventureEffectBG_On(new List<InGameAnimal>() { this }, null);
                break;
            case ANIMAL_SKILL_TYPE.HEAL_ALL_HP:
                SetAnimation("Animal_Skill_2_all");
                GameUIManager.instance.AdventureEffectBG_On(AdventureManager.instance.AnimalLIst, null);
                break;

            case ANIMAL_SKILL_TYPE.STUN:
                bool isCanUseStun = false;
                foreach (var temp in AdventureManager.instance.EnemyLIst)
                {
                    if (temp.pos == AdventureManager.instance.GetNonStunEnemyPos())
                    {
                        isCanUseStun = true;
                        break;
                    }
                }

                if (!isCanUseStun) return false;

                SetAnimation("Animal_Skill_3");
                var stunList = new List<InGameEnemy>();
                foreach (var temp in AdventureManager.instance.EnemyLIst)
                {
                    if (temp.pos == AdventureManager.instance.GetNonStunEnemyPos())
                    { 
                        stunList.Add(temp);
                        break;
                    }
                }
                GameUIManager.instance.AdventureEffectBG_On(new List<InGameAnimal>() { this }, stunList);
                break;
            case ANIMAL_SKILL_TYPE.STUN_ALL:
                isCanUseStun = false;
                foreach (var temp in AdventureManager.instance.EnemyLIst)
                {
                    if (temp.pos == AdventureManager.instance.GetNonStunEnemyPos())
                    {
                        isCanUseStun = true;
                        break;
                    }
                }

                if (!isCanUseStun) return false;

                SetAnimation("Animal_Skill_3_all");
                GameUIManager.instance.AdventureEffectBG_On(new List<InGameAnimal>() { this }, AdventureManager.instance.EnemyLIst);
                break;

            case ANIMAL_SKILL_TYPE.ADD_LINE_BOMB:
                NormalBlockList = PosHelper.GetRandomBlockList(false);
                if (NormalBlockList.Count < data.skillGrade)
                {
                    NormalBlockList = null;
                    return false;
                }

                skill = new InGameSkill_InstallBombObject(this, colorType, NormalBlockList);
                AdventureManager.instance.isDoAction = true;
                skill.EndSkillEvent += () => { NormalBlockList = null; SkillEnd(); };

                SetAnimation("Animal_Skill_Install_Bomb");
                GameUIManager.instance.AdventureEffectBG_On(new List<InGameAnimal>() { this }, null);
                break;
            case ANIMAL_SKILL_TYPE.ADD_BOMB:
                NormalBlockList = PosHelper.GetRandomBlockList(false);
                if (NormalBlockList.Count < data.skillGrade)
                {
                    NormalBlockList = null;
                    return false;
                }
                
                skill = new InGameSkill_InstallBombObject(this, colorType, NormalBlockList);
                AdventureManager.instance.isDoAction = true;
                skill.EndSkillEvent += () => { NormalBlockList = null; SkillEnd(); };

                SetAnimation("Animal_Skill_Install_Bomb");
                GameUIManager.instance.AdventureEffectBG_On(new List<InGameAnimal>() { this }, null);
                break;
            case ANIMAL_SKILL_TYPE.ADD_RAINBOW_BOMB:
                NormalBlockList = PosHelper.GetRandomBlockList(false);
                if (NormalBlockList.Count < data.skillGrade)
                {
                    NormalBlockList = null;
                    return false;
                }

                skill = new InGameSkill_InstallBombObject(this, colorType, NormalBlockList);
                AdventureManager.instance.isDoAction = true;
                skill.EndSkillEvent += () => { NormalBlockList = null; SkillEnd(); };

                SetAnimation("Animal_Skill_Install_Bomb");
                GameUIManager.instance.AdventureEffectBG_On(new List<InGameAnimal>() { this }, null);
                break;
        }

        ManagerSound.AudioPlay(AudioInGame.SKILL_COMMON);
        state = ANIMAL_STATE.SKILL;
        ManagerBlock.instance.state = BlockManagrState.BEFORE_WAIT;
        GameUIManager.instance.ShowAdventureDarkBGBlock(true);

        StartCoroutine(CheckAnimalAnimationEnd());

        return true;
    }

    public void SkillAttack()
    {
        int attackDemage = Mathf.RoundToInt(((float)data.atk) * totalAttPoint * ((float)data.skillGrade) * 0.01f + 0.005f);

        int SkillType = data.skill % 100;

        switch ((ANIMAL_SKILL_TYPE)SkillType)
        {
            case ANIMAL_SKILL_TYPE.ATTACK:
                AdventureManager.instance.DemageEnemy(this, AdventureManager.instance.GetEnemyPos(), attackDemage, true, data.skill / 100);
                break;

            case ANIMAL_SKILL_TYPE.ATTACK_ALL:
                AdventureManager.instance.DemageEnemyAll(this, attackDemage, true, data.skill / 100);
                break;
                /*
            case SKILL_TYPE.ADD_1_MAX_SKILL_POINT:
                addMaxAttPoint += data.skillGrade;
                break;
            case SKILL_TYPE.ADD_ALL_MAX_SKILL_POINT:
                foreach (var temp in AdventureManager.instance.AnimalLIst)
                    temp.SetAddMaxAttPoint(data.skillGrade);
                break;
                 */
            case ANIMAL_SKILL_TYPE.HEAL_HP:
                HealHP(data.skillGrade * 0.01f);
                break;

            case ANIMAL_SKILL_TYPE.HEAL_ALL_HP:
                foreach (var temp in AdventureManager.instance.AnimalLIst)
                {
                    if(temp.GetState() != ANIMAL_STATE.DEAD)
                        temp.HealHP(data.skillGrade * 0.01f);
                }
                break;
                
            case ANIMAL_SKILL_TYPE.STUN:
                foreach(var temp in AdventureManager.instance.EnemyLIst)
                {
                    if(temp.pos == AdventureManager.instance.GetNonStunEnemyPos())
                    {
                        temp.Stun(data.skillGrade);
                        break;
                    }
                }
                break;
            case ANIMAL_SKILL_TYPE.STUN_ALL:
                foreach (var temp in AdventureManager.instance.EnemyLIst)
                {
                    temp.Stun(data.skillGrade);
                }
                break;
            
            case ANIMAL_SKILL_TYPE.ADD_LINE_BOMB:
            case ANIMAL_SKILL_TYPE.ADD_BOMB:
            case ANIMAL_SKILL_TYPE.ADD_RAINBOW_BOMB:
                StartCoroutine(skill.UesSkill());
                break;

            case ANIMAL_SKILL_TYPE.CHANGE_BLOCK_COLOR:
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
            case ANIMAL_SKILL_TYPE.REMOVE_GIMIK_1STEP:
                break;
            case ANIMAL_SKILL_TYPE.GET_ALL_ITEM_BLOCK:
                break;
            case ANIMAL_SKILL_TYPE.MAKE_ITEM:
                break;
        }
    }

    /// <summary>
    /// 스킬을 사용하고 난 후 동몰의 애니메이션 을 Wait 상태로 바꿔주는 기능
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckAnimalAnimationEnd()
    {
        yield return new WaitForSeconds(0.5f);

        yield return new WaitWhile(() => Animalanim.isPlaying);

        state = ANIMAL_STATE.WAIT;

        //새로운 스킬 시스템을 사용하지 않는 스킬이라면 동물이 Wait상태가 된경우를 SkillEnd로 간주 한다.
        if (skill == null) SkillEnd();
    }

    private void SkillEnd()
    {
        GameManager.instance.IsCanTouch = true;
        AdventureManager.instance.ItemAction();
    }

    public void SetAddMaxAttPoint(int addPoint)
    {
        addMaxAttPoint += addPoint;

        //최대값추가 이펙트
    }

    public void HealItemGuideOn(bool bOn)
    {
        if (bOn == true)
        {
            if (state != ANIMAL_STATE.DEAD)
                return;
            TextureRoot.SetActive(true);
            MainUrlTexture.color = new Color(1f, 1f, 1f, 0.5f);

            adventureItemOutLine.enabled = true;

            if (_healItemGuideCoroutine != null)
            {
                StopCoroutine(_healItemGuideCoroutine);
            }

            _healItemGuideCoroutine = StartCoroutine(CoActionHealItemGuide());
        }
        else
        {
            if (state == ANIMAL_STATE.DEAD && HeartRoot.activeInHierarchy == true)
                TextureRoot.SetActive(false);

            MainUrlTexture.color         = new Color(1f, 1f, 1f, 1f);
            adventureItemOutLine.enabled = false;

            if (_healItemGuideCoroutine != null)
            {
                StopCoroutine(_healItemGuideCoroutine);
            }
        }
    }


    IEnumerator CoActionHealItemGuide()
    {
        float stateTimer = 0f;
        Color color = adventureItemOutLine.color;
        while (true)
        {
            if (GameItemManager.instance == null)
                break;

            stateTimer += Global.deltaTime;

            float ratio = (0.4f + Mathf.Cos(Time.time * 10f) * 0.3f);
            adventureItemOutLine.color = new Color(color.r, color.g, color.b, ratio);
            yield return null;
        }
        yield return null;
    }


    public void HealHP(float percent)
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        if (life < data.hp)
        {
            AdventureManager.instance.HealCount++;
            lifeSlider.HealEndEvent += EndHealEvent;
        }

        int healPoint = Mathf.RoundToInt((data.hp) * percent + 0.005f);

        life += healPoint;

        if (life > data.hp)
            life = data.hp;
        
        lifeSlider.Value = (float)life / (float)data.hp;        
        damageMeter.Damage(data.hp, healPoint, true);
        animalSkill_Heal_Target();
    }

    void EndHealEvent()
    {
        AdventureManager.instance.HealCount--;
    }

    public void MoveWave()
    {
        SetAnimation("Animal_Wave");
        ManagerSound.AudioPlay(AudioInGame.MOVE_WAVE_1);

        //웨이브 진행시 살아 있는 동물들에게 힐이 들어가도록
        if (state != ANIMAL_STATE.DEAD)
        {
            //animalSkill_Heal_Cast02();
            //HealHP(0.1f);

            int healPoint = Mathf.RoundToInt((data.hp - life) * 0.5f + 0.005f);//(int)(life * 0.5f);
            life += healPoint;

            if (life > data.hp)
                life = data.hp;

            lifeSlider.Value = (float)life / (float)data.hp;
            damageMeter.Damage(data.hp, healPoint, true);
            animalSkill_Heal_Target();
        }
    }

    public void ReviveAnimal(int hppoint)
    {
        state = ANIMAL_STATE.REVIVE;
        StartCoroutine(CoRevieAnimal(hppoint));
    }

    public IEnumerator CoRevieAnimal(int hppoint)
    {
        SetAnimalActive(true);
        SetAnimation("Animal_Revive");
        yield return new WaitUntil(() => IsPlayingAnimation("Animal_Revive") == false);
        RecoverAnimal(hppoint, false);
    }

    private bool IsPlayingAnimation(string clipName)
    {
        if (this.Animalanim.isPlaying)
        {
            foreach (AnimationState item in this.Animalanim)
            {
                if (this.Animalanim.IsPlaying(item.name))
                {
                    return (item.name.Equals(clipName));
                }
            }
        }
        return false;
    }

    public void RecoverAnimal(int hppoint, bool bRootActive = true)
    {
        if (bRootActive == true)
            SetAnimalActive(true);

        if (Animalanim.clip.name != "Animal_Wait")
        {
            SetAnimation("Animal_Wait");
        }

        int recoverHp = Mathf.RoundToInt(((float)(data.hp * hppoint)) * 0.01f + 0.005f);
        life = recoverHp;
        lifeSlider.Value = (float)life / (float)data.hp;
        damageMeter.Damage(data.hp, recoverHp, true);

        attackPoint = 0;
        comboCount = 0;
        CheckBeforeDead = false;

        if (skillItemObj != null)
            skillItemObj.SetActive(true);

        if (skillItem != null)
        skillItem.ResetSkill();

        state = ANIMAL_STATE.WAIT;
    }

    private void SetAnimalActive(bool bAnimalActive)
    {
        TextureRoot.SetActive(bAnimalActive);
        //lifeSlider.gameObject.SetActive(bAnimalActive);
        HeartRoot.SetActive(!bAnimalActive);
    }

    public void CountHeart()
    {
        if (state != ANIMAL_STATE.DEAD || CheckBeforeDead == false)
            return;

        heartCount--;
        heartCountLabel.text = heartCount.ToString();

        if (heartCount <= 0)
        {
            heartCount = 10;
            ReviveAnimal(25); 
        }
    }

    public void SelectHealAnimalByItem()
    {
        if (GameItemManager.instance == null ||
            GameItemManager.instance.used == true ||
            GameItemManager.instance.type != GameItemType.HEAL_ONE_ANIMAL)
        {
            InfoItem.ShowInfo();

            if(skillItem != null)
                skillItem.ShowInfo();

            return;
        }

        if (GetState() != ANIMAL_STATE.DEAD)
            return;                

        GameItemManager.instance.UseAnimalItem(this);
    }

    public void ActiveAtBlind(bool active)
    {
        int activeValue = active ? 60 : -60;

        MainUrlTexture.depth += activeValue;

        var lifebars = lifeSlider.GetComponentsInChildren<UIWidget>();
        foreach (var bar in lifebars)
            bar.depth += activeValue;
    }

    public void animalSkill_Attack_Cast01()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture.transform.position / transform.lossyScale.x, "animalSkill_Attack_Cast01");
    }

    public void animalSkill_Attack_Cast02()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position / transform.lossyScale.x, "animalSkill_Attack_Cast02");
    }

    public void animalSkill_Attack_Cast03()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position / transform.lossyScale.x, "animalSkill_Attack_Cast03");
    }

    public void animalSkill_Heal_Cast01()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture.transform.position / transform.lossyScale.x, "animalSkill_Heal_Cast01");
    }

    public void animalSkill_Heal_Cast02()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position / transform.lossyScale.x, "animalSkill_Heal_Cast02");
    }

    public void animalSkill_Heal_Target()
    {
        ManagerSound.AudioPlay(AudioInGame.SKILL_HIT_HEAL, 0.4f);

        GameObject effect = InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position / transform.lossyScale.x, "animalSkill_Heal_Target");

        if (!GameUIManager.instance.adventureEffectBG.gameObject.activeSelf)
        {
            foreach(var effectParts in effect.GetComponentsInChildren<UIWidget>())
                effectParts.depth -= 60;
        }
    }

    public void animalHitEffect()
    {
        var effect = InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position / transform.lossyScale.x, "EnemyHitEffect");

        if(damageEffectTexture_1 != null)
        {
            ParticleSystemRenderer render = effect.transform.Find("particle_Star01").GetComponent<ParticleSystemRenderer>();
            var material = new Material(render.material);
            material.SetTexture("_MainTex", damageEffectTexture_1);
            render.material = material;
        }

        if (damageEffectTexture_2 != null)
        {
            ParticleSystemRenderer render = effect.transform.Find("Particle_Hit01").GetComponent<ParticleSystemRenderer>();
            var material = new Material(render.material);
            material.SetTexture("_MainTex", damageEffectTexture_2);
            render.material = material;
        }
    }

    public void animalSkill_Attack_Target01(int offsetHeight)
    {
        InGameEffectMaker.instance.MakeAdventureEffect(AdventureManager.instance.EnemyPos(AdventureManager.instance.GetEnemyPos()) + Vector3.up * offsetHeight, "animalSkill_Attack_Target01");
    }

    public void animalSkill_Attack_Target01_All(int offsetHeight)
    {
        for(int i=0; i<AdventureManager.instance.EnemyLIst.Count; i++)
        {
            if(AdventureManager.instance.EnemyLIst[i].GetState() != ANIMAL_STATE.DEAD)
                InGameEffectMaker.instance.MakeAdventureEffect(AdventureManager.instance.EnemyPos(AdventureManager.instance.EnemyLIst[i].pos) + Vector3.up * offsetHeight, "animalSkill_Attack_Target01");
        }
    }

    public void animalSkill_Frozen_Cast01()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(MainUrlTexture.transform.position / transform.lossyScale.x, "animalSkill_Frozen_Cast01");
    }

    public void animalSkill_Frozen_Cast02()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position / transform.lossyScale.x, "animalSkill_Frozen_Cast02");
    }

    public void animalSkill_InstallObject()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(EffectPosObj.transform.position / transform.lossyScale.x, "animalSkill_InstallObject");
    }

    public void animalSound_Attack()
    {
        if(data.atkType == 1 && animalHitSound != null)
            ManagerSound.AudioPlay(animalHitSound);
        else
            ManagerSound.AudioPlay(Random.value >= 0.5f ? AudioInGame.FRIEND_NORMAL_ATTACK_SHORT : AudioInGame.FRIEND_NORMAL_ATTACK_LONG);
    }

    public void animalSound_Hit()
    {
        if (data.protectedFromMelee && animalDamageSound != null)
            ManagerSound.AudioPlay(animalDamageSound);
        else
            ManagerSound.AudioPlay(AudioInGame.HIT_FRIEND_1);
    }

    public void ShotBullet()
    {
        foreach (var enemy in AdventureManager.instance.EnemyLIst)
        {
            if (enemy.pos == SetEnemyPos)
            {
                bullet.Shot(enemy.CenterObj.transform, AttackEnemy);
                break;
            }
        }
    }
}
