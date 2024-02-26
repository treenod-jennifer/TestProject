using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSkill_InstallObject : InGameSkill
{
    protected int blockMakeCount;
    protected List<BlockBase> normalBlockList;

    public InGameSkill_InstallObject(InGameSkillCaster skillCaster, List<BlockBase> normalBlockList) : base(skillCaster)
    {
        this.blockMakeCount = skillCaster.SkillGrade;
        this.normalBlockList = normalBlockList;
    }

    public override IEnumerator UesSkill()
    {
        BlockType blockType = (BlockType)skillCategory;

        for (int i = 0; i < blockMakeCount - 1; i++)
        {
            InGameSkillUtility.Instance.StartCoroutine(InstallObjectAni(blockType));
            yield return new WaitForSeconds(0.2f);
        }

        yield return InstallObjectAni(blockType);

        EndSkill();
    }

    private IEnumerator InstallObjectAni(BlockType type)
    {
        int randomIndex = GameManager.instance.GetIngameRandom(0, normalBlockList.Count);
        BlockBase normalBlock = normalBlockList[randomIndex];
        normalBlockList.RemoveAt(randomIndex);

        Effect_InstallObject effect = InGameSkillUtility.Instance.MakeEffect_InstallObject();
        effect.Init(skillCaster.CasterTransform, (ENEMY_SKILL_TYPE)skillCaster.SkillIndex);
        yield return effect.Play(normalBlock.transform.localPosition - effect.transform.localPosition);

        normalBlock.PangDestroyBoardData();
        BlockBase baseBlock = BlockMaker.instance.MakeBlockBase
        (
            indexX: normalBlock.indexX,
            indexY: normalBlock.indexY,
            blockType: type,
            blockColorType: BlockColorType.NONE,
            count: InGameSkillUtility.GetSkillInstallGrade((ENEMY_SKILL_TYPE)skillCaster.SkillIndex),
            subType: skillSubCategory
        );
        ManagerBlock.boards[normalBlock.indexX, normalBlock.indexY].Block = baseBlock;

        yield return new WaitForSeconds(0.1f);

        yield break;
    }
}
