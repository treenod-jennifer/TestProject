using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_InstallBomb : Effect_InstallObject
{
    public override void Init(Transform caster, ENEMY_SKILL_TYPE skillType)
    {
        transform.SetParent(GameUIManager.instance.Advance_Root.transform);
        transform.localScale = Vector3.one;
        transform.position = caster.position;
        transform.localPosition += Vector3.up * 50;

        iconHeight = InGameSkillUtility.GetSkillIconHeight(skillType);
    }

    public override void Init(Transform caster, ANIMAL_SKILL_TYPE skillType)
    {
        transform.SetParent(GameUIManager.instance.Advance_Root.transform);
        transform.localScale = Vector3.one;
        transform.position = caster.position;
        transform.localPosition += Vector3.up * 50;
    }

    protected override IEnumerator Bomb_Ani()
    {
        yield break;
    }

    protected override IEnumerator Bomb_Ani2()
    {
        yield break;
    }

    protected override IEnumerator Icon_Ani()
    {
        bomb.SetActive(false);
        Invoke("EffectOff", 1.5f);
        yield break;
    }

    protected override void Sound_Explosion()
    {
        ManagerSound.AudioPlay(AudioInGame.ENEMESKILL_EXPLOSION_2);
    }
}
