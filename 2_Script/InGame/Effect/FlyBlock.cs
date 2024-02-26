using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyBlock : MonoBehaviour
{
    public BlockType changeBlockType = BlockType.NONE;
    public BlockBase targetBlock = null;

    public UISprite mainSprite;

    bool destroyOnly = false;
    bool rotateSprite = false;

    public void initBlock(BlockType type, BlockBase tempTargetBlock,Vector3 startPos, bool tempDestroyOnly = false, string tempName = null, bool tempRotate = false)
    {
        if(tempName == null)mainSprite.spriteName = type.ToString();
        else mainSprite.spriteName = tempName;
        mainSprite.depth = (int)GimmickDepth.FX_FLYEFFECT;
        mainSprite.MakePixelPerfect();

        changeBlockType = type;
        targetBlock = tempTargetBlock;
        transform.localPosition = startPos;
        mainSprite.MakePixelPerfect();
        destroyOnly = tempDestroyOnly;
        rotateSprite = tempRotate;
    }


	void Update ()
    {
		if(targetBlock != null)
        {
            if (rotateSprite)
            {
                Vector3 dir = (targetBlock.transform.localPosition - transform.localPosition).normalized;
                mainSprite.cachedTransform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
            }
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetBlock.transform.localPosition, Global.deltaTimePuzzle * 900f);
            
            if(Vector3.Distance(targetBlock.transform.localPosition, transform.localPosition) < 2)
            {
                if (BlockMatchManager.instance.checkBlockToyBlastMatch(targetBlock) && destroyOnly == false)
                {
                    BlockMatchManager.instance.CheckMatchPangBlock(targetBlock, false);
                }
                else
                {
                    BlockBase.uniqueIndexCount++;
                    targetBlock.BlockPang(BlockBase.uniqueIndexCount, BlockColorType.NONE, true);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
	}
}
