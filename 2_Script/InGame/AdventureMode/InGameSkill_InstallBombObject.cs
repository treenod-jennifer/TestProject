using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSkill_InstallBombObject : InGameSkill
{
    protected int blockMakeCount;
    protected List<BlockBase> normalBlockList;
    private BlockColorType colorType;

    public InGameSkill_InstallBombObject(InGameSkillCaster skillCaster, BlockColorType colorType, List<BlockBase> normalBlockList) : base(skillCaster)
    {
        this.blockMakeCount = skillCaster.SkillGrade;
        this.normalBlockList = normalBlockList;
        this.colorType = colorType;
    }

    public override IEnumerator UesSkill()
    {
        BlockBombType blockType = (BlockBombType)skillCategory;

        for (int i = 0; i < blockMakeCount - 1; i++)
        {
            if(blockType == BlockBombType.LINE_H || blockType == BlockBombType.LINE_V)
            {
                blockType = GameManager.instance.GetIngameRandom(0, 2) == 1 ? BlockBombType.LINE_H : BlockBombType.LINE_V;
            }
            InGameSkillUtility.Instance.StartCoroutine(InstallBombObjectAni(blockType, blockType != BlockBombType.RAINBOW ? BlockColorType.NONE : colorType));
            yield return new WaitForSeconds(0.2f);
        }

        if (blockType == BlockBombType.LINE_H || blockType == BlockBombType.LINE_V)
        {
            blockType = GameManager.instance.GetIngameRandom(0, 2) == 1 ? BlockBombType.LINE_H : BlockBombType.LINE_V;
        }
        yield return InstallBombObjectAni(blockType, blockType != BlockBombType.RAINBOW ? BlockColorType.NONE : colorType);

        EndSkill();
    }

    private IEnumerator InstallBombObjectAni(BlockBombType type, BlockColorType colorType = BlockColorType.NONE)
    {
        int randomIndex = GameManager.instance.GetIngameRandom(0, normalBlockList.Count);
        BlockBase normalBlock = normalBlockList[randomIndex];
        normalBlockList.RemoveAt(randomIndex);

        Effect_InstallObject effect = null;

        switch (type)
        {
            case BlockBombType.RAINBOW:
                effect = InGameSkillUtility.Instance.MakeEffect_InstallRanibowObject();
                break;
            case BlockBombType.LINE_H:
                effect = InGameSkillUtility.Instance.MakeEffect_InstallLineBombObject();
                break;
            case BlockBombType.LINE_V:
                effect = InGameSkillUtility.Instance.MakeEffect_InstallLineBombObject();
                break;
            case BlockBombType.BOMB:
                effect = InGameSkillUtility.Instance.MakeEffect_InstallBombObject();
                break;
        }

        if (effect == null)
        {
            yield break;
        }

        effect.Init(skillCaster.CasterTransform, (ANIMAL_SKILL_TYPE)skillCaster.SkillIndex);
        yield return effect.Play(normalBlock.transform.localPosition - effect.transform.localPosition);

        
        normalBlock.bombType = type;
        if (colorType != BlockColorType.NONE) { normalBlock.colorType = colorType; }
        normalBlock.Destroylinker();

        yield return new WaitForSeconds(0.1f);

        yield break;
    }
}
