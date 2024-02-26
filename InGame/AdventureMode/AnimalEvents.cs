using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalEvents : MonoBehaviour {

    [SerializeField]
    InGameAnimal animal;
    [SerializeField]
    InGameEnemy enemy;

    public void ShowScreenEffect()
    {
        animal.ShowScreenEffect();
    }

    public void HideScreenEffect()
    {
        animal.HideScreenEffect();
    }

    public void AttackEnemy()
    {
        animal.AttackEnemy();
    }

    public void AttackAnimal()
    {
        enemy.DemageAnimal();
    }

    public void SkillUse()
    {
        animal.SkillAttack();
    }

    public void EnemyLightingSkillHitEffect()   //0
    {
        enemy.EnemyLightingSkillHitEffect();
        //몬스터 번개공격 스킬 피격 이펙트
    }

    public void EnemyStunSkillHitEffect()   //1
    {
        //몬스터기절 스킬 피격 이펙트
        enemy.EnemyStunSkillHitEffect();
    }

    public void EnemyHitEffect()    //2
    {
        // 몬스터 기본 피격 이펙 트
        enemy.EnemyHitEffect();
    }

    public void animalSkill_Attack_Cast01() //3
    {
        animal.animalSkill_Attack_Cast01();
    }

    public void animalSkill_Attack_Cast02() //4
    {
        animal.animalSkill_Attack_Cast02();
    }

    public void animalSkill_Heal_Cast01()   //5
    {
        animal.animalSkill_Heal_Cast01();
    }

    public void animalSkill_Heal_Cast02()   //6
    {
        animal.animalSkill_Heal_Cast02();
    }

    public void animalSkill_Stun_Cast01()   //7
    {
        animal.animalSkill_Stun_Cast01();
    }

    public void animalSkill_Stun_Cast02()   //8
    {
        animal.animalSkill_Stun_Cast02();
    }

    public void CameraShake()
    {
        GameUIManager.instance.ShakingCamera();
    }   
}
