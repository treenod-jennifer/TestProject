using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSkillItem_Enemy : InGameSkillItem
{
    [SerializeField] private UISprite decoIcon;
    [SerializeField] private UILabel skillInfoLabel;
    [SerializeField] private Color skillLabelColor;
    [SerializeField] private GameObject skillChangeEffect;

    public enum SKILL_ROTATION_TYPE
    {
        SEQUNTIAL = 1,
        RANDOM_SEQUNTIAL = 2,
        RANDOM = 3
    }
    private SKILL_ROTATION_TYPE ROTATION_TYPE;

    private int currentSkillIndex = 0;
    private List<EnemySkillInfo> skillList = null;
    public EnemySkillInfo CurrentSkill
    {
        get
        {
            if (skillList == null ||
                skillList.Count <= currentSkillIndex)
            {
                return null;
            }

            return skillList[currentSkillIndex];
        }
    }

    protected override int MaxSkillPoint
    {
        get
        {
            if(CurrentSkill != null)
            {
                return CurrentSkill.skillMaxCount;
            }

            return 0;
        }
    }

    public void InitSkill(InGameSkillCaster skillCaster, List<EnemySkillInfo> skillList, SKILL_ROTATION_TYPE ROTATION_TYPE)
    {
        if (skillList.Count == 0)
            return;

        Init(skillCaster);
        this.skillList = skillList;
        this.ROTATION_TYPE = ROTATION_TYPE;

        if (this.ROTATION_TYPE == SKILL_ROTATION_TYPE.RANDOM_SEQUNTIAL)
        {
            SkillListShuffle();
        }
        if (this.ROTATION_TYPE == SKILL_ROTATION_TYPE.RANDOM)
        {
            currentSkillIndex = GameManager.instance.GetIngameRandom(0, skillList.Count);
        }

        SetIcon((ENEMY_SKILL_TYPE)CurrentSkill.skill);

        //skillGrade = CurrentSkill.skillGrade;
        skillLabel.text = CurrentSkill.skillMaxCount.ToString();

        SetSkillInfo();

        if (CurrentSkill.skillMaxCount == 1)
            StartCoroutine(DoTweenIcon());
    }

    public override InGameSkill GetSkill()
    {
        InGameSkillUtility.SKILL_TYPE skillType = InGameSkillUtility.GetSkillType((ENEMY_SKILL_TYPE)skillCaster.SkillIndex);
        int skillGrade = skillCaster.SkillGrade;
        int skillCategory = InGameSkillUtility.GetSkillCategory((ENEMY_SKILL_TYPE)skillCaster.SkillIndex);
        int skillSubCategory = InGameSkillUtility.GetSkillSubCategory((ENEMY_SKILL_TYPE)skillCaster.SkillIndex);

        InGameSkill skill = null;

        switch (skillType)
        {
            case InGameSkillUtility.SKILL_TYPE.ACTIVE:
                break;
            case InGameSkillUtility.SKILL_TYPE.INSTALL_BLOCK:
            case InGameSkillUtility.SKILL_TYPE.INSTALL_DECO:
                var normalBlockList = PosHelper.GetRandomBlockList(false);
                if (normalBlockList.Count < skillGrade + 3)
                {
                    break;
                }

                if (skillType == InGameSkillUtility.SKILL_TYPE.INSTALL_BLOCK)
                {
                    skill = new InGameSkill_InstallObject(skillCaster, normalBlockList);
                }
                else
                {
                    skill = new InGameSkill_InstallDecoObject(skillCaster, normalBlockList);
                }
                break;
            default:
                break;
        }

        return skill;
    }

    public override void AddSkillPoint(int tempPoint = 1)
    {
        if (CurrentSkill == null ||
            skillCaster.GetState() == ANIMAL_STATE.DEAD ||
            skillPoint == MaxSkillPoint)
        {
            return;
        }

        skillPoint += tempPoint;
        if (skillPoint >= MaxSkillPoint)
        {
            skillPoint = MaxSkillPoint;
            skillLabel.enabled = false;
        }
        else
        {
            skillLabel.text = (MaxSkillPoint - skillPoint).ToString();
        }

        if (MaxSkillPoint - skillPoint == 1)
        {
            InGameEffectMaker.instance.MakeAdventureSkillIconEffect(gameObject);
            StartCoroutine(DoTweenIcon());
        }
    }

    public override void ResetSkill()
    {
        skillPoint = 0;

        skillLabel.enabled = true;
        skillLabel.text = MaxSkillPoint.ToString();
    }
    
    private IEnumerator DoTweenIcon()
    {
        while (skillPoint + 1 >= MaxSkillPoint)
        {
            if(GameManager.instance.state == GameState.PLAY)
            {
                float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 20);
                transform.localScale = Vector3.one * (0.97f + ratioScale * 0.06f);

                float colorValue = Mathf.Sin(Mathf.PI * ManagerBlock.instance.BlockTime * 8);
                skillLabel.color = Color.Lerp(Color.white, skillLabel.effectColor, colorValue);
            }

            yield return null;
        }
        
        transform.localScale = Vector3.one;
        skillLabel.color = skillLabelColor;
        yield return null;
    }

    private void SetSkillInfo()
    {
        ENEMY_SKILL_TYPE type = (ENEMY_SKILL_TYPE)skillCaster.SkillIndex;
        InGameSkillUtility.SKILL_INFO_TYPE infoType = InGameSkillUtility.GetSkillInfoType(type);

        switch (infoType)
        {
            case InGameSkillUtility.SKILL_INFO_TYPE.NORMAL:
                skillInfoLabel.text = "";
                break;
            case InGameSkillUtility.SKILL_INFO_TYPE.COUNT:
				skillInfoLabel.text = "x" + skillCaster.SkillGrade;
                break;
            default:
                skillInfoLabel.text = "";
                break;
        }
    }

    private void SetIcon(ENEMY_SKILL_TYPE type)
    {
        string spriteName = InGameSkillUtility.GetSkillIconName(type);
        bool isDeco = InGameSkillUtility.GetSkillType(type) == InGameSkillUtility.SKILL_TYPE.INSTALL_DECO;

        UISprite icon;

        if (isDeco)
        {
            MainSprite.gameObject.SetActive(false);
            decoIcon.gameObject.SetActive(true);

            icon = decoIcon;
        }
        else
        {
            MainSprite.gameObject.SetActive(true);
            decoIcon.gameObject.SetActive(false);

            icon = MainSprite;
        }

        icon.spriteName = spriteName;
        icon.MakePixelPerfect();
    }


    public void SkillChange()
    {
        switch (ROTATION_TYPE)
        {
            case SKILL_ROTATION_TYPE.SEQUNTIAL:
            case SKILL_ROTATION_TYPE.RANDOM_SEQUNTIAL:
                currentSkillIndex++;
                if (currentSkillIndex >= skillList.Count)
                    currentSkillIndex = 0;
                break;
            case SKILL_ROTATION_TYPE.RANDOM:
                currentSkillIndex = GameManager.instance.GetIngameRandom(0, skillList.Count);
                break;
            default:
                break;
        }

        SetIcon((ENEMY_SKILL_TYPE)CurrentSkill.skill);
        SetSkillInfo();

        var effect = Instantiate(skillChangeEffect, transform);
        Destroy(effect, 2.0f);
    }

    private void SkillListShuffle()
    {
        List<EnemySkillInfo> tempList = new List<EnemySkillInfo>();

        while (skillList.Count > 0)
        {
            int randomIndex = GameManager.instance.GetIngameRandom(0, skillList.Count);

            tempList.Add(skillList[randomIndex]);
            skillList.RemoveAt(randomIndex);
        }

        skillList = tempList;
    }
}
