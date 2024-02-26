using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameEnemy : MonoBehaviour {

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

    //생명게이지
    public UISlider lifeSlider;
    public UILabel lifeLabel;

    int AttackTurn = 3;
    int waitTurn = 0;
    [SerializeField]
    int life = 0;
    int SetanimalPos = -1;

    public void init(EnemyInfo tempData, int tempPos = 0)
    {
        //기본초기화
        AttackTurn = tempData.TurnCount;
        waitTurn = tempData.TurnCount;

        CountLable.text = waitTurn.ToString();

        data = tempData;
        pos = tempPos;

        transform.localPosition = AdventureManager.instance.EnemyPos(tempPos);

        attTypeSprite.spriteName = string.Format("icon_{0:D3}", tempData.attribute);

        MainUrlTexture.depth = 10 - pos;
        MainUrlTexture.Load(Global.adventureDirectory, "Animal/", string.Format("m_{0:D4}", tempData.idx));
        MainUrlTexture.MakePixelPerfect();
        MainUrlTexture.SettingTextureScale(AdventureManager.ANIMAL_SIZE, AdventureManager.ANIMAL_SIZE);

        //CountLable.depth = 11 - pos;

        life = data.life;
        lifeLabel.text = data.life.ToString();
        lifeSlider.value = (float)data.life / (float)data.life;
        lifeSlider.gameObject.transform.localPosition = new Vector3(0, AdventureManager.LIFE_POS_Y);

                
        Animalanim.Stop();
        Animalanim.Play("Enemy_Start");
        state = ANIMAL_STATE.WAIT;
    }


    public void AddAttackPoint()
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        if (waitTurn <= 0)
        {
            waitTurn = AttackTurn;
        }
        else
        {
            waitTurn--;
        }

        //턴변화는 애니메이션
        CountLable.text = waitTurn.ToString();
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

    public void Attack(int enemyPos)
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        if (enemyPos == -1)
            return;

        SetanimalPos = enemyPos;
        state = ANIMAL_STATE.ATTACK;
        StartCoroutine(DoAttack(enemyPos));
    }

    IEnumerator DoAttack(int enemyPos)
    {
        string animName = "Enemy_Att_" + (enemyPos + pos + 1); 
        if (ManagerBlock.instance.stageInfo.battleWaveList[AdventureManager.instance.waveCount].enemyIndexList.Count == 1)
            animName = "Enemy_Att_" + (pos + 2);  

        Animalanim.Stop();
        Animalanim.Play(animName);

        float timer = 0f;     
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        while (Animalanim.IsPlaying(animName))
            yield return null;

        state = ANIMAL_STATE.WAIT;
        yield return null;
    }

    public void Pang(int pangPoint, bool isSkill =  false)
    {
        if (state == ANIMAL_STATE.DEAD)
            return;

        life -= pangPoint;
        lifeSlider.value = (float)life / (float)data.life;
        lifeLabel.text = life.ToString();

        if (life > 0)
        {
            state = ANIMAL_STATE.PANG;
            StartCoroutine(DoPangAni(isSkill));
        }
        else
        {
            state = ANIMAL_STATE.DEAD;
            StartCoroutine(DoDeadAni(isSkill));
        }
    }

    IEnumerator DoPangAni(bool isSkill)
    {
        string animName = "Enemy_Demage";                
        if (isSkill)
            animName = "Enemy_Skill_Demage";

        Animalanim.Stop();
        Animalanim.Play(animName);

        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        while (Animalanim.IsPlaying(animName))
            yield return null;

        if (life > 0)
        { 
            state = ANIMAL_STATE.WAIT; 
        }
        else
        {
            state = ANIMAL_STATE.DEAD;
            MainUrlTexture.gameObject.SetActive(false);
            shadowObj.SetActive(false);
        }

        yield return null;
    }

    IEnumerator DoDeadAni(bool isSkill)
    {
        lifeSlider.gameObject.SetActive(false);

        string animName = "Enemy_Demage";
        if (isSkill)
            animName = "Enemy_Skill_Demage";

        Animalanim.Stop();
        Animalanim.Play(animName);

        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Global.deltaTimePuzzle;
            yield return null;
        }

        while (Animalanim.IsPlaying(animName))
            yield return null;

        MainUrlTexture.gameObject.SetActive(false);
        shadowObj.SetActive(false);

        yield return null;
    }


    void Update()
    {
        switch (state)
        {
            case ANIMAL_STATE.WAIT:
                if (Animalanim.isPlaying == false)  
                    Animalanim.Play("Enemy_Wait");                
                break;
        }
    }

    public void Stun(int count = 1)
    {
        waitTurn += count;
    }

    public void EnemyLightingSkillHitEffect()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(CenterObj.transform.position, "EnemyLightingSkillHitEffect");
    }

    public void EnemyStunSkillHitEffect()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(CenterObj.transform.position, "EnemyStunSkillHitEffect");
    }

    public void EnemyHitEffect()
    {
        InGameEffectMaker.instance.MakeAdventureEffect(CenterObj.transform.position, "EnemyHitEffect");
    }
}
