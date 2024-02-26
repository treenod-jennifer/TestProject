using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_InstallObject : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected GameObject root;
    [SerializeField] protected GameObject effect;
    [SerializeField] protected GameObject bomb;

    [SerializeField] protected UIAtlas blockAtlas;
    [SerializeField] protected UIAtlas decoAtlas;
    [SerializeField] private UISprite icon;
    [SerializeField] protected GameObject bombIcon;

    protected float iconHeight = 0.0f;
    private bool isComplete = false;
    private event System.Action endEvent;

    public virtual void Init(Transform caster, ENEMY_SKILL_TYPE skillType)
    {
        transform.SetParent(GameUIManager.instance.Advance_Root.transform);
        transform.localScale = Vector3.one;
        transform.position = caster.position;
        transform.localPosition += Vector3.up * 50;

        bool isDeco = InGameSkillUtility.GetSkillType(skillType) == InGameSkillUtility.SKILL_TYPE.INSTALL_DECO;

        icon.atlas = isDeco ? decoAtlas : blockAtlas;
        icon.spriteName = InGameSkillUtility.GetSkillIconName(skillType);
        icon.gameObject.SetActive(false);
        iconHeight = InGameSkillUtility.GetSkillIconHeight(skillType);
    }

    public virtual void Init(Transform caster, ANIMAL_SKILL_TYPE skillType)
    {
        transform.SetParent(GameUIManager.instance.Advance_Root.transform);
        transform.localScale = Vector3.one;
        transform.position = caster.position;
        transform.localPosition += Vector3.up * 50;

        icon.atlas = blockAtlas;
        icon.spriteName = InGameSkillUtility.GetSkillIconName(skillType);
        icon.gameObject.SetActive(false);
    }

    public IEnumerator Play(Vector2 endPosition, System.Action callback = null)
    {
        isComplete = false;
        endEvent = callback;

        animator.SetFloat("PosX", endPosition.x);
        animator.SetFloat("PosY", endPosition.y);

        animator.Play("downTest");
        ManagerSound.AudioPlay(AudioInGame.ENEMESKILL_THROW);

        while (!isComplete)
            yield return new WaitForSeconds(0.1f);

        yield return Bomb_Ani();

        Instantiate(effect, root.transform);
        Sound_Explosion();

        yield return Bomb_Ani2();

        yield return Icon_Ani();

        yield break;
    }

    protected virtual void Sound_Explosion()
    {
        ManagerSound.AudioPlay(AudioInGame.ENEMESKILL_EXPLOSION_1);
    }

    [SerializeField] protected AnimationCurve bombSize_Curve;
    protected virtual IEnumerator Bomb_Ani()
    {
        if (bombSize_Curve.keys == null || bombSize_Curve.keys.Length == 0)
            yield break;

        float totalTime = 0.0f;
        float endTime = bombSize_Curve.keys[bombSize_Curve.keys.Length - 1].time;

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimePuzzle;

            bomb.transform.localScale = Vector3.one * bombSize_Curve.Evaluate(totalTime);

            yield return null;
        }

        bombIcon.SetActive(false);
    }

    protected virtual IEnumerator Bomb_Ani2()
    {
        float totalTime = 0.0f;
        const float endTime = 0.3f;
        float bombStartSize = bomb.transform.localScale.x;
        float iconStartSize = icon.transform.localScale.x;

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimePuzzle;

            bomb.transform.localScale = Vector3.one * Mathf.Lerp(bombStartSize, 1.0f, totalTime / endTime);
            icon.transform.localScale = Vector3.one * Mathf.Lerp(iconStartSize, 1.0f, totalTime / endTime);

            yield return null;
        }
    }

    [SerializeField] protected AnimationCurve iconHeight_Curve;
    protected virtual IEnumerator Icon_Ani()
    {
        if (iconHeight_Curve.keys == null || iconHeight_Curve.keys.Length == 0)
            yield break;

        float totalTime = 0.0f;
        float endTime = iconHeight_Curve.keys[iconHeight_Curve.keys.Length - 1].time;
        float endHeight = iconHeight;

        icon.gameObject.SetActive(true);

        while (totalTime < endTime)
        {
            totalTime += Global.deltaTimePuzzle;
            icon.transform.localPosition = 
                Vector3.up * 
                (iconHeight_Curve.Evaluate(totalTime) + 
                Mathf.Lerp(0.0f, endHeight, totalTime / endTime));

            yield return null;
        }

        Invoke("EffectOff", 0.1f);
    }

    private void EffectOff()
    {
        Destroy(gameObject);
    }

    private void End()
    {
        if (isComplete)
            return;

        isComplete = true;

        if (endEvent != null)
        {
            endEvent();
            endEvent = null;
        }
    }
}
