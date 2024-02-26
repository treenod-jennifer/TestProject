using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SKILL_TYPE
{
    NONE,
    ATTACK = 1,
    ATTACK_ALL =2,
    ADD_1_MAX_SKILL_POINT =3,
    ADD_ALL_MAX_SKILL_POINT=4,
    HEAL_HP =5 ,
    HEAL_ALL_HP = 6,
    STUN = 7,
    STUN_ALL = 8,
    ADD_LINE_BOMB = 9,
    ADD_DOUBLE_BOMB = 10,
    ADD_RAINBOW_BOMB = 11,   
    CHANGE_BLOCK_COLOR = 12,
    REMOVE_GIMIK_1STEP = 13,
    GET_ALL_ITEM_BLOCK = 14,
    MAKE_ITEM = 15,
}


public class InGameSkillItem : MonoBehaviour {

    public BlockColorType colorType = BlockColorType.NONE;

    public UISprite BGSprite;
    public UISprite MainSprite;
    public UISprite EffectRing;
    public UISprite gaigeSprite;
    public UISprite linkerSprite;

    public UISlider skillSlider;    
    public UILabel skillLabel;
    public UILabel skillLabelOK;

    public InGameAnimal animal;

    public int SkillType = 0;

    int skillPoint = 0;
    int MaxSkillPoint = 20;

    public void InitSkill(BlockColorType tempColor, int tempSkill)
    {
        colorType = tempColor;
        SkillType = tempSkill;

        BGSprite.spriteName = "skill_bg_" + tempColor;
        BGSprite.MakePixelPerfect();

        linkerSprite.spriteName = "skill_line_" + tempColor;
        linkerSprite.MakePixelPerfect();

        MainSprite.spriteName = "skill_icon_" + tempSkill + "_1";
        MainSprite.MakePixelPerfect();

        EffectRing.gameObject.SetActive(false);

        skillLabel.text = ((float)skillPoint / (float)MaxSkillPoint) * 100 + "%";
        skillSlider.value = (float)skillPoint / (float)MaxSkillPoint;
        gaigeSprite.fillAmount = (float)skillPoint / (float)MaxSkillPoint;
        skillLabelOK.enabled = false;

        //위치잡기
        transform.localPosition = new Vector3(-AdventureManager.POS_X_INTERVAL * (int)tempColor, AdventureManager.SKILL_ITEM_POS_Y, 0); //220

        //스킬정보

        //스킬 최대치
    }

    void UseSkill()
    {        
        //게임상태 체크
        if (ManagerBlock.instance.state != BlockManagrState.WAIT)
            return;

        if (GameItemManager.instance != null)// && GameItemManager.instance.used == true)
            return;



        if (skillPoint < MaxSkillPoint)
            return;

        skillLabelOK.enabled = false;
        skillLabel.enabled = true;

        skillPoint = 0;
        skillLabel.text = ((float)skillPoint / (float)MaxSkillPoint) * 100 + "%";
        skillSlider.value = (float)skillPoint / (float)MaxSkillPoint;
        gaigeSprite.fillAmount = (float)skillPoint / (float)MaxSkillPoint;
                     
        animal.UseSkill();
    }
    
    public void AddSkillPoint(int tempPoint = 1)
    {
        if (animal.GetState() == ANIMAL_STATE.DEAD)
            return;

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


        skillSlider.value = (float)skillPoint / (float)MaxSkillPoint;
        gaigeSprite.fillAmount = (float)skillPoint / (float)MaxSkillPoint;
                
        //포인트 최대치일때 반짝반짝효과 추가
        StartCoroutine(DoTweenIcon());
    }

    IEnumerator DoTweenIcon() 
    {
        //MainSprite.spriteName = "skill_icon_" + SkillType + "_2";
        //MainSprite.MakePixelPerfect();
        EffectRing.gameObject.SetActive(true);

        
        while (skillPoint == MaxSkillPoint && GameManager.instance.state == GameState.PLAY)
        {            
            float ratioScale = Mathf.Sin(ManagerBlock.instance.BlockTime * 20);
            MainSprite.color = new Color(0.7f + ratioScale * 0.3f, 0.7f + ratioScale * 0.3f, 0.7f + ratioScale * 0.3f, 1); //mainSprite.color = Color.white * (0.8f + Mathf.Sin(Time.time * 5f) * 0.2f);            
            transform.localScale = Vector3.one * (0.97f + ratioScale * 0.06f);

            yield return null;
        }

        //MainSprite.spriteName = "skill_icon_" + SkillType + "_1";
        //MainSprite.MakePixelPerfect();
        EffectRing.gameObject.SetActive(false);

        MainSprite.color = Color.white;
        transform.localScale = Vector3.one;
        yield return null;
    }

    public BlockColorType GetColor()
    {
        return colorType;
    }

    public void ResetSkill()
    {
        //MainSprite.spriteName = "skill_icon_" + SkillType + "_1";
        //MainSprite.MakePixelPerfect();

        EffectRing.gameObject.SetActive(false);
        
        skillLabelOK.enabled = false;
        skillLabel.enabled = false;

        skillPoint = 0;
        skillLabel.text =  "0%";
        skillSlider.value = 0f;
        gaigeSprite.fillAmount = 0f;
    }
}
