using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalEvents : MonoBehaviour {

    [SerializeField]
    InGameAnimal animal;
	[SerializeField]InGameEnemy enemy;

    public void HideScreenEffect()
    {
        GameUIManager.instance.AdventureEffectBG_Off();
    }

    public void AttackEnemy()
    {
        animal.AttackEnemy();
    }

    //public void SkillAttackEnemy()
    //{
    //    animal.SkillAttackEnemy();
    //}

    public void AttackAnimal()
    {
        enemy.DemageAnimal();
    }

    public void RangedAttackEnemy()
    {
        enemy.ShotBullet();
    }

    public void RangedAttackAnimal()
    {
        animal.ShotBullet();
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

    public void EnemyHeartSkillHitEffect()   //0 - 1
    {
        enemy.EnemyHeartSkillHitEffect();
        //몬스터 하트공격 스킬 피격 이펙트
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

	public void animalSkill_Attack_Cast02() //4-1
	{
        animal.animalSkill_Attack_Cast02();
	}

    public void animalSkill_Attack_Cast03() //4-2
    {
        animal.animalSkill_Attack_Cast03();
    }

    public void animalSkill_Heal_Cast01()   //5
	{
        animal.animalSkill_Heal_Cast01();
	}

	public void animalSkill_Heal_Cast02()   //6
	{
        animal.animalSkill_Heal_Cast02();
	}

    public void animalSkill_Heal_Target()   //9
    {
        animal.animalSkill_Heal_Target();
    }

    public void animalHitEffect()   //10
    {
        animal.animalHitEffect();
    }

    public void animalSkill_Attack_Target01()   //11
    {
        animal.animalSkill_Attack_Target01(200);
		//단일
    }

    public void animalSkill_Attack_Target01_All()   //12
    {
        animal.animalSkill_Attack_Target01_All(200);
		//전체
    }

    public void animalSkill_Frozen_Cast01() //13
    {
        animal.animalSkill_Frozen_Cast01();
    }

    public void animalSkill_Frozen_Cast02() //14
    {
        animal.animalSkill_Frozen_Cast02();
    }

    public void EnemySkill_Frozen_Hit01()   //15
    {
        enemy.EnemySkill_Frozen_Hit01();
    }

    public void AnimalSkill_InstallObject()
    {
        animal.animalSkill_InstallObject();
    }

    public void animalWait()    //16
    {
        Animation aniController = GetComponent<Animation>();
        aniController.Stop();
        aniController.Play("Animal_Wait");
    }

    public void EnemyLand() //17
    {
        ManagerSound.AudioPlay(AudioInGame.ENEMY_LAND);
    }

    public void EnemySkill() //18
    {
        StartCoroutine(enemy.CoEnemySkill());
    }

    #region SoundEvent
    public void animalSound_Attack()
    {
        animal.animalSound_Attack();
    }

    public void animalSound_Hit()
    {
        animal.animalSound_Hit();
    }

    public void animalSound_Skill_Thunderbolt()
    {
        ManagerSound.AudioPlay(AudioInGame.SKILL_THUNDERBOLT);
    }

    public void animalSound_Skill_Heartrain()
    {
        ManagerSound.AudioPlay(AudioInGame.LAST_PANG);
    }

    public void animalSound_Skill_Freeze()
    {
        ManagerSound.AudioPlay(AudioInGame.SKILL_FREEZE);
    }

    public void animalSound_Skill_Heal()
    {
        ManagerSound.AudioPlay(AudioInGame.SKILL_HEAL);
    }

    public void animalSound_Skill_InstallObject()
    {
        ManagerSound.AudioPlay(AudioInGame.SKILL_INSTALL);
    }

    public void enemySound_Attack()
    {
        enemy.enemySound_Attack();
    }

    public void enemySound_Skill_Hit_Thunderbolt()
    {
        ManagerSound.AudioPlay(AudioInGame.SKILL_HIT_THUNDERBOLT);
    }

    public void enemySound_Skill_Hit_Heartrain()
    {
        ManagerSound.AudioPlay(AudioInGame.SKILL_HIT_HEARTRAIN);
    }

    public void enemySound_Skill_Hit_Freeze()
    {
        ManagerSound.AudioPlay(AudioInGame.SKILL_HIT_FREEZE);
    }
    #endregion


    public void CameraShake()
	{
        GameUIManager.instance.ShakingCamera();
    }
    
    public void CameraShake_normal()
    {
        ShakeCameraEffect._instance.ShakeCamera(0);
    }

	public void CameraShake_skill()
	{
		ShakeCameraEffect._instance.ShakeCamera(1);
	}

    public void CameraFlicker()
    {
        ShakeCameraEffect._instance.FlickerCamera();
    }
}
