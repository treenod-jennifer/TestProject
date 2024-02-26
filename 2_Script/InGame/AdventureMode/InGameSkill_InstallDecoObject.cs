using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSkill_InstallDecoObject : InGameSkill
{
    protected int blockMakeCount;
    protected List<BlockBase> normalBlockList;

    public InGameSkill_InstallDecoObject(InGameSkillCaster skillCaster, List<BlockBase> normalBlockList) : base(skillCaster)
    {
        this.blockMakeCount = skillCaster.SkillGrade;
        this.normalBlockList = normalBlockList;
}

    public override IEnumerator UesSkill()
    {
        BoardDecoType decoType = (BoardDecoType)skillCategory;

        for (int i = 0; i < blockMakeCount - 1; i++)
        {
            InGameSkillUtility.Instance.StartCoroutine(InstallDecoObjectAni(decoType));
            yield return new WaitForSeconds(0.2f);
        }

        yield return InstallDecoObjectAni(decoType);

        EndSkill();
    }

    private IEnumerator InstallDecoObjectAni(BoardDecoType type)
    {
        int randomIndex = GameManager.instance.GetIngameRandom(0, normalBlockList.Count);
        BlockBase normalBlock = normalBlockList[randomIndex];
        normalBlockList.RemoveAt(randomIndex);

        Effect_InstallObject effect = InGameSkillUtility.Instance.MakeEffect_InstallObject();
        effect.Init(skillCaster.CasterTransform, (ENEMY_SKILL_TYPE)skillCaster.SkillIndex);
        yield return effect.Play(normalBlock.transform.localPosition - effect.transform.localPosition);

        DecoInfo info = new DecoInfo();
        info.BoardType = (int)type;
        info.count = InGameSkillUtility.GetSkillInstallGrade((ENEMY_SKILL_TYPE)skillCaster.SkillIndex);

        if (type == BoardDecoType.ICE)
        {
            BoardDecoMaker.instance.MakeBlockDeco(normalBlock, normalBlock.indexX, normalBlock.indexY, info);

            normalBlock.RemoveLinkerNoReset();
            normalBlock.expectType = BlockBombType.NONE;
            BlockMatchManager.instance.SetBlockLink();
        }
        else
            BoardDecoMaker.instance.MakeBoardDeco(ManagerBlock.boards[normalBlock.indexX, normalBlock.indexY], normalBlock.indexX, normalBlock.indexY, info);

        yield return new WaitForSeconds(0.1f);

        yield break;
    }
}
