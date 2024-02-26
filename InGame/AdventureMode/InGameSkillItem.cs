using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InGameSkillItem : MonoBehaviour
{
    [SerializeField] protected UIItemAdventureInfo InfoItem;
    [SerializeField] protected UISprite BGSprite;
    [SerializeField] protected UISprite MainSprite;
    [SerializeField] protected UILabel skillLabel;

    public InGameSkillCaster skillCaster;

    protected int skillPoint = 0;
    protected abstract int MaxSkillPoint { get; }

    public bool isFull
    {
        get
        {
            return (skillPoint == MaxSkillPoint);
        }
    }

    public virtual void Init(InGameSkillCaster caster)
    {
        skillCaster = caster;
    }

    public abstract InGameSkill GetSkill();
    public abstract void AddSkillPoint(int tempPoint = 1);
    public abstract void ResetSkill();

    public void ShowInfo()
    {
        InfoItem.ShowInfo();
    }
}
