using System.Collections;
using UnityEngine;
using DG.Tweening;

public class InGameSkillItem_Animal : InGameSkillItem
{
    private BlockColorType colorType = BlockColorType.NONE;
    [SerializeField] private GameObject MSG_Root;

    [SerializeField] protected UISprite EffectRing;
    [SerializeField] protected UISprite linkerSprite;
    [SerializeField] private UISlider skillSlider;
    [SerializeField] private UILabel skillLabelOK;
    [SerializeField] private UISprite gaigeSprite;
    [SerializeField] private UILabel skillInfoLabel;
    [SerializeField] private UISprite skillItemOutLine;

    protected override int MaxSkillPoint { get { return 20; } }
    
    private Coroutine _skillItemGuideCoroutine = null;

    public void InitSkill(InGameSkillCaster skillCaster, BlockColorType tempColor)
    {
        this.skillCaster = skillCaster;

        EffectRing.gameObject.SetActive(false);

        MainSprite.spriteName = "skill_icon_" + (skillCaster.SkillIndex % 100)+ "_1";
        MainSprite.MakePixelPerfect();

        skillLabel.text = ((float)skillPoint / (float)MaxSkillPoint) * 100 + "%";
        skillSlider.value = (float)skillPoint / (float)MaxSkillPoint;
        skillLabelOK.enabled = false;

        gaigeSprite.fillAmount = (float)skillPoint / (float)MaxSkillPoint;

        colorType = tempColor;

        BGSprite.spriteName = "skill_bg_" + tempColor;
        BGSprite.MakePixelPerfect();

        linkerSprite.spriteName = "skill_line_" + tempColor;
        linkerSprite.MakePixelPerfect();
        linkerSprite.cachedTransform.localScale = new Vector3(1, 0.7f, 1);

        SetSkillInfo();

        //위치잡기
        transform.localPosition = new Vector3(-AdventureManager.POS_X_INTERVAL * (int)tempColor, AdventureManager.SKILL_ITEM_POS_Y, 0); //220

        //스킬정보

        //스킬 최대치
    }

    public BlockColorType GetColor()
    {
        return colorType;
    }

    public void AddSkillPointUsingPercent(float percent)
    {
        int tempPoint = (int)(MaxSkillPoint * percent);
        AddSkillPoint(tempPoint);
    }

    public void UseSkill()
    {
        //게임상태 체크
        if (ManagerBlock.instance.state != BlockManagrState.WAIT)
            return;

        if (GameItemManager.instance != null)
        {
            if (GameItemManager.instance.type != GameItemType.SKILL_HAMMER || isFull)
                return;

            GameItemManager.instance.UseAnimalItem(AdventureManager.instance.AnimalLIst.Find(x => x.GetColor() == colorType));
            return;
        }

        if (skillPoint < MaxSkillPoint)
            return;

        if (InGameAnimal.NormalBlockList != null) return;

        if (skillCaster.UseSkill())
        {
            ResetSkill();
            InGameEffectMaker.instance.MakeAdventureEffect(transform.position / transform.parent.lossyScale.x, "animalSkill_Button");
        }
        else
        {
            if (MSG_Root.activeSelf == false)
            {
                StartCoroutine(ShowStunMSG());
            }
        }
    }

    public override InGameSkill GetSkill()
    {
        return null;
    }

    public override void AddSkillPoint(int tempPoint = 1)
    {
        if (skillCaster.GetState() == ANIMAL_STATE.DEAD)
        {
            return;
        }

        if (skillPoint == MaxSkillPoint)
        {
            return;
        }

        skillPoint += tempPoint;
        if (skillPoint >= MaxSkillPoint)
        {
            skillPoint = MaxSkillPoint;
            skillLabel.enabled = false;
            skillLabelOK.enabled = true;
        }
        else
        {
            skillLabel.text = ((float)skillPoint / (float)MaxSkillPoint) * 100 + "%";
        }

        StartCoroutine(CoSkillCharge((float)skillPoint / (float)MaxSkillPoint));
    }
    public override void ResetSkill()
    {
        EffectRing.gameObject.SetActive(false);

        skillLabelOK.enabled = false;
        skillLabel.enabled = true;
        MainSprite.color = Color.white;

        skillPoint = 0;
        skillLabel.text = "0%";
        skillSlider.value = 0f;
        gaigeSprite.fillAmount = 0f;
    }

    private IEnumerator DoTweenIcon()
    {
        EffectRing.gameObject.SetActive(true);

        while (skillPoint == MaxSkillPoint && GameManager.instance.state == GameState.PLAY)
        {
            float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 20);
            MainSprite.color = new Color(0.7f + ratioScale * 0.3f, 0.7f + ratioScale * 0.3f, 0.7f + ratioScale * 0.3f, 1); //mainSprite.color = Color.white * (0.8f + Mathf.Sin(Time.time * 5f) * 0.2f);            
            transform.localScale = Vector3.one * (0.97f + ratioScale * 0.06f);
            yield return null;
        }

        EffectRing.gameObject.SetActive(false);

        MainSprite.color = Color.white;
        transform.localScale = Vector3.one;
        yield return null;
    }

    private IEnumerator CoSkillCharge(float targetValue)
    {
        if (targetValue == 1.0f)
            InGameEffectMaker.instance.MakeAdventureSkillIconEffect(gameObject);

        DOTween.To(() => skillSlider.value, x => skillSlider.value = x, targetValue, 0.2f).SetEase(Ease.Flash);

        yield return new WaitForSeconds(0.2f);
        //포인트 최대치일때 반짝반짝효과 추가\
        
        StartCoroutine(DoTweenIcon());
    }

    private IEnumerator ShowStunMSG()
    {
        MSG_Root.SetActive(true);

        float timer = 0;
        while (timer < 1f)
        {
            timer += Global.deltaTimePuzzle * 1.2f;
            yield return null;
        }

        MSG_Root.SetActive(false);
    }

    private void SetSkillInfo()
    {
        ANIMAL_SKILL_TYPE type = (ANIMAL_SKILL_TYPE)(skillCaster.SkillIndex % 100);
        InGameSkillUtility.SKILL_INFO_TYPE infoType = InGameSkillUtility.GetSkillInfoType(type);

        switch (infoType)
        {
            case InGameSkillUtility.SKILL_INFO_TYPE.NORMAL:
                skillInfoLabel.text = "+" + skillCaster.SkillGrade;
                break;
            case InGameSkillUtility.SKILL_INFO_TYPE.PERCENT:
                skillInfoLabel.text = "+" + skillCaster.SkillGrade + "%";
                break;
            default:
                skillInfoLabel.text = "";
                break;
        }
    }

    public void SkillItemGuideOn(bool bOn)
    {
        if (bOn == true)
        {
            if (isFull || isActiveAndEnabled == false)
            {
                return;
            }

            skillItemOutLine.enabled = true;

            if (_skillItemGuideCoroutine != null)
            {
                StopCoroutine(_skillItemGuideCoroutine);
            }

            _skillItemGuideCoroutine = StartCoroutine(CoActionSkillItemGuide());
        }
        else
        {
            skillItemOutLine.enabled = false;

            if (_skillItemGuideCoroutine != null)
            {
                StopCoroutine(_skillItemGuideCoroutine);
            }
        }
    }

    private IEnumerator CoActionSkillItemGuide()
    {
        var stateTimer = 0f;
        var color      = skillItemOutLine.color;
        while (true)
        {
            if (GameItemManager.instance == null)
            {
                break;
            }

            stateTimer += Global.deltaTime;
            var ratio = (0.4f + Mathf.Cos(Time.time * 10f) * 0.3f);
            skillItemOutLine.color = new Color(color.r, color.g, color.b, ratio);
            yield return null;

            var ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 20);
            skillItemOutLine.transform.localScale = Vector3.one * (0.97f + ratioScale * 0.06f);

            yield return null;
        }

        yield return null;
    }
}
