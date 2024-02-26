using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ENEMY_SKILL_TYPE
{
    NONE = 0,

    INSTALL_PLANT_1 = 1,
    INSTALL_PLANT_2 = 2,
    INSTALL_PLANT_3 = 3,

    INSTALL_ROCK_1 = 4,
    INSTALL_ROCK_2 = 5,

    INSTALL_NET_1 = 6,
    INSTALL_NET_2 = 7,

    INSTALL_ICE_1 = 8,
    INSTALL_ICE_2 = 9,
    INSTALL_ICE_3 = 10,

    INSTALL_HP_PLANT_1 = 11,
    INSTALL_HP_PLANT_2 = 12,
    INSTALL_HP_PLANT_3 = 13,

    INSTALL_SP_PLANT_1 = 14,
    INSTALL_SP_PLANT_2 = 15,
    INSTALL_SP_PLANT_3 = 16,

    INSTALL_HP_ROCK_1 = 17,
    INSTALL_HP_ROCK_2 = 18,

    INSTALL_SP_ROCK_1 = 19,
    INSTALL_SP_ROCK_2 = 20,
}

public enum ANIMAL_SKILL_TYPE
{
    NONE = 0,

    ATTACK = 1,
    ATTACK_ALL = 2,

    ADD_1_MAX_SKILL_POINT = 3,
    ADD_ALL_MAX_SKILL_POINT = 4,

    HEAL_HP = 5,
    HEAL_ALL_HP = 6,

    STUN = 7,
    STUN_ALL = 8,

    ADD_LINE_BOMB = 9,
    ADD_BOMB = 10,
    ADD_RAINBOW_BOMB = 11,

    CHANGE_BLOCK_COLOR = 12,
    REMOVE_GIMIK_1STEP = 13,
    GET_ALL_ITEM_BLOCK = 14,
    MAKE_ITEM = 15,
}

public abstract class InGameSkill
{
    protected InGameSkillCaster skillCaster;

    protected InGameSkillUtility.SKILL_TYPE skillType;
    protected int skillCategory;
    protected int skillSubCategory;

    public event System.Action EndSkillEvent;

    public InGameSkill(InGameSkillCaster skillCaster)
    {
        this.skillCaster = skillCaster;

        if (skillCaster.IsAnimal)
        {
            skillType = InGameSkillUtility.GetSkillType((ANIMAL_SKILL_TYPE)skillCaster.SkillIndex);
            skillCategory = InGameSkillUtility.GetSkillCategory((ANIMAL_SKILL_TYPE)skillCaster.SkillIndex);
            skillSubCategory = InGameSkillUtility.GetSkillSubCategory((ANIMAL_SKILL_TYPE)skillCaster.SkillIndex);
        }
        else
        {
            skillType = InGameSkillUtility.GetSkillType((ENEMY_SKILL_TYPE)skillCaster.SkillIndex);
            skillCategory = InGameSkillUtility.GetSkillCategory((ENEMY_SKILL_TYPE)skillCaster.SkillIndex);
            skillSubCategory = InGameSkillUtility.GetSkillSubCategory((ENEMY_SKILL_TYPE)skillCaster.SkillIndex);
        }
    }

    public abstract IEnumerator UesSkill();

    protected virtual void EndSkill()
    {
        EndSkillEvent?.Invoke();
    }
}

public interface InGameSkillCaster
{
    bool IsAnimal { get; }

    int SkillIndex { get; }

    int SkillGrade { get; }

    Transform CasterTransform { get; }

    ANIMAL_STATE GetState();

    bool UseSkill();
}

public class InGameSkillUtility : MonoBehaviour
{
    private static InGameSkillUtility _instance;

    public static InGameSkillUtility Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Instantiate(Resources.Load("Adventure/InGameSkillUtility") as GameObject).GetComponent<InGameSkillUtility>();
            }

            return _instance;
        }

        private set { }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public enum SKILL_TYPE
    {
        ACTIVE,
        INSTALL_BLOCK,
        INSTALL_DECO,
        INSTALL_BOMB
    }

    public enum SKILL_INFO_TYPE
    {
        NONE,

        NORMAL,
        COUNT,
        PERCENT
    }

    private struct SkillData
    {
        public SKILL_TYPE type;
        public int category;
        public int subCategory;
        public int install_grade;
        public string iconName;
        public int iconHeight;
        public SKILL_INFO_TYPE infoType;

		/// <summary>
		/// 스킬을 구성하는데 필요한 데이터 모음
		/// </summary>
		/// <param name="type">스킬 타입 (1 - 액티브형, 2 - 설치형(블럭), 3 - 설치형(데코))</param>
		/// <param name="category">스킬 종류</param>
		/// <param name="install_grade">설치물 단계</param>
		/// <param name="iconName">스킬 아이콘 이름</param>
		/// <param name="iconHeight">스킬 아이콘 기본 높이</param>
		/// <param name="infoType">스킬 정보 보여주는 형식</param>
		public SkillData(SKILL_TYPE type, int category = 0, int subCategory = 0, int install_grade = 0, string iconName = null, int iconHeight = 0, SKILL_INFO_TYPE infoType = SKILL_INFO_TYPE.NORMAL)
        {
            this.type = type;
            this.category = category;
            this.subCategory = subCategory;
            this.install_grade = install_grade;
            this.iconName = iconName;
            this.iconHeight = iconHeight;
            this.infoType = infoType;
        }
    }

    /// <summary>
    /// 스킬타입으로 스킬구성에 필요한 각종 데이터를 가져오는 기능
    /// </summary>
    private static SkillData GetSkillData(ENEMY_SKILL_TYPE type)
    {
        switch (type)
        {
            case ENEMY_SKILL_TYPE.NONE: return new SkillData();

            case ENEMY_SKILL_TYPE.INSTALL_PLANT_1: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, install_grade: 1, iconName: "plant1", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_PLANT_2: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, install_grade: 2, iconName: "plant2", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_PLANT_3: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, install_grade: 3, iconName: "plant3", iconHeight: 14, infoType: SKILL_INFO_TYPE.COUNT);

            case ENEMY_SKILL_TYPE.INSTALL_ROCK_1: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 11, install_grade: 1, iconName: "blockStone1", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_ROCK_2: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 11, install_grade: 2, iconName: "blockStone2", infoType: SKILL_INFO_TYPE.COUNT);

            case ENEMY_SKILL_TYPE.INSTALL_NET_1: return new SkillData(type: SKILL_TYPE.INSTALL_DECO, category: 1, install_grade: 1, iconName: "netPlants1", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_NET_2: return new SkillData(type: SKILL_TYPE.INSTALL_DECO, category: 1, install_grade: 2, iconName: "netPlants2", infoType: SKILL_INFO_TYPE.COUNT);

            case ENEMY_SKILL_TYPE.INSTALL_ICE_1: return new SkillData(type: SKILL_TYPE.INSTALL_DECO, category: 13, install_grade: 1, iconName: "DecoIce1", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_ICE_2: return new SkillData(type: SKILL_TYPE.INSTALL_DECO, category: 13, install_grade: 2, iconName: "DecoIce2", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_ICE_3: return new SkillData(type: SKILL_TYPE.INSTALL_DECO, category: 13, install_grade: 3, iconName: "DecoIce3", infoType: SKILL_INFO_TYPE.COUNT);

            case ENEMY_SKILL_TYPE.INSTALL_HP_PLANT_1: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, subCategory: 13, install_grade: 1, iconName: "plantPotionHeal1", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_HP_PLANT_2: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, subCategory: 13, install_grade: 2, iconName: "plantPotionHeal2", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_HP_PLANT_3: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, subCategory: 13, install_grade: 3, iconName: "plantPotionHeal3", iconHeight: 14, infoType: SKILL_INFO_TYPE.COUNT);

            case ENEMY_SKILL_TYPE.INSTALL_SP_PLANT_1: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, subCategory: 14, install_grade: 1, iconName: "plantPotionSkill1", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_SP_PLANT_2: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, subCategory: 14, install_grade: 2, iconName: "plantPotionSkill2", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_SP_PLANT_3: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 2, subCategory: 14, install_grade: 3, iconName: "plantPotionSkill3", iconHeight: 14, infoType: SKILL_INFO_TYPE.COUNT);

            case ENEMY_SKILL_TYPE.INSTALL_HP_ROCK_1: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 11, subCategory: 9, install_grade: 1, iconName: "blockStone1_HealPotion", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_HP_ROCK_2: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 11, subCategory: 9, install_grade: 2, iconName: "blockStone2_HealPotion", infoType: SKILL_INFO_TYPE.COUNT);

            case ENEMY_SKILL_TYPE.INSTALL_SP_ROCK_1: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 11, subCategory: 10, install_grade: 1, iconName: "blockStone1_SkillPotion", infoType: SKILL_INFO_TYPE.COUNT);
            case ENEMY_SKILL_TYPE.INSTALL_SP_ROCK_2: return new SkillData(type: SKILL_TYPE.INSTALL_BLOCK, category: 11, subCategory: 10, install_grade: 2, iconName: "blockStone2_SkillPotion", infoType: SKILL_INFO_TYPE.COUNT);

            default: return new SkillData();
        }
    }

    private static SkillData GetSkillData(ANIMAL_SKILL_TYPE type)
    {
        switch (type)
        {
            case ANIMAL_SKILL_TYPE.NONE: return new SkillData();

            case ANIMAL_SKILL_TYPE.ATTACK:     return new SkillData(type: SKILL_TYPE.ACTIVE, infoType: SKILL_INFO_TYPE.PERCENT);
            case ANIMAL_SKILL_TYPE.ATTACK_ALL: return new SkillData(type: SKILL_TYPE.ACTIVE, infoType: SKILL_INFO_TYPE.PERCENT);

            case ANIMAL_SKILL_TYPE.HEAL_HP:     return new SkillData(type: SKILL_TYPE.ACTIVE, infoType: SKILL_INFO_TYPE.PERCENT);
            case ANIMAL_SKILL_TYPE.HEAL_ALL_HP: return new SkillData(type: SKILL_TYPE.ACTIVE, infoType: SKILL_INFO_TYPE.PERCENT);

            case ANIMAL_SKILL_TYPE.STUN:     return new SkillData(type: SKILL_TYPE.ACTIVE, infoType: SKILL_INFO_TYPE.NORMAL);
            case ANIMAL_SKILL_TYPE.STUN_ALL: return new SkillData(type: SKILL_TYPE.ACTIVE, infoType: SKILL_INFO_TYPE.NORMAL);

            case ANIMAL_SKILL_TYPE.ADD_LINE_BOMB:    return new SkillData(type: SKILL_TYPE.INSTALL_BOMB, category: 4, infoType: SKILL_INFO_TYPE.NORMAL);
            case ANIMAL_SKILL_TYPE.ADD_BOMB:         return new SkillData(type: SKILL_TYPE.INSTALL_BOMB, category: 6, infoType: SKILL_INFO_TYPE.NORMAL);
            case ANIMAL_SKILL_TYPE.ADD_RAINBOW_BOMB: return new SkillData(type: SKILL_TYPE.INSTALL_BOMB, category: 2, infoType: SKILL_INFO_TYPE.NORMAL);

            default: return new SkillData();
        }
    }

    public static SKILL_TYPE GetSkillType(ENEMY_SKILL_TYPE type)
    {
        return GetSkillData(type).type;
    }

    public static int GetSkillCategory(ENEMY_SKILL_TYPE type)
    {
        return GetSkillData(type).category;
    }

    public static int GetSkillSubCategory(ENEMY_SKILL_TYPE type)
    {
        return GetSkillData(type).subCategory;
    }

    public static int GetSkillInstallGrade(ENEMY_SKILL_TYPE type)
    {
        return GetSkillData(type).install_grade;
    }

    public static string GetSkillIconName(ENEMY_SKILL_TYPE type)
    {
        return GetSkillData(type).iconName;
    }

    public static int GetSkillIconHeight(ENEMY_SKILL_TYPE type)
    {
        return GetSkillData(type).iconHeight;
    }

    public static SKILL_INFO_TYPE GetSkillInfoType(ENEMY_SKILL_TYPE type)
    {
        return GetSkillData(type).infoType;
    }


    public static SKILL_INFO_TYPE GetSkillInfoType(ANIMAL_SKILL_TYPE type)
    {
        return GetSkillData(type).infoType;
    }

    public static string GetSkillIconName(ANIMAL_SKILL_TYPE type)
    {
        return GetSkillData(type).iconName;
    }

    public static SKILL_TYPE GetSkillType(ANIMAL_SKILL_TYPE type)
    {
        return GetSkillData(type).type;
    }

    public static int GetSkillCategory(ANIMAL_SKILL_TYPE type)
    {
        return GetSkillData(type).category;
    }

    public static int GetSkillSubCategory(ANIMAL_SKILL_TYPE type)
    {
        return GetSkillData(type).subCategory;
    }


    [Header("EffectObject")]
    [SerializeField] private GameObject installObject;
    [SerializeField] private GameObject installLineBombObject;
    [SerializeField] private GameObject installBombObject;
    [SerializeField] private GameObject installRainbowObject;

    public Effect_InstallObject MakeEffect_InstallObject()
    {
        GameObject effect = Instantiate(installObject);
        return effect.GetComponent<Effect_InstallObject>();
    }

    public Effect_InstallBomb MakeEffect_InstallLineBombObject()
    {
        GameObject effect = Instantiate(installLineBombObject);
        return effect.GetComponent<Effect_InstallBomb>();
    }

    public Effect_InstallBomb MakeEffect_InstallBombObject()
    {
        GameObject effect = Instantiate(installBombObject);
        return effect.GetComponent<Effect_InstallBomb>();
    }

    public Effect_InstallBomb MakeEffect_InstallRanibowObject()
    {
        GameObject effect = Instantiate(installRainbowObject);
        return effect.GetComponent<Effect_InstallBomb>();
    }
}
