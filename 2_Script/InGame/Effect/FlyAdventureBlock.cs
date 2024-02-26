using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAdventureBlock : MonoBehaviour {

    static public int flyCount = 0;

    BlockColorType blockType = BlockColorType.NONE;
    InGameAnimal targetAnimal = null;
    
    public UISprite mainSprite;

    public void initBlock(BlockColorType blockType, InGameAnimal tempAnimal, Vector3 startPos)
    {
        mainSprite.spriteName = "block" + ManagerBlock.instance.GetColorTypeString(blockType);
        mainSprite.depth = (int)GimmickDepth.FX_FLYEFFECT;
        mainSprite.MakePixelPerfect();

        transform.localPosition = startPos;
        
        targetAnimal = tempAnimal;
        flyCount++;
    }

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetAnimal.gameObject.transform.localPosition, Global.deltaTimePuzzle * 900f*1.5f);

        if (Vector3.Distance(targetAnimal.gameObject.transform.localPosition, transform.localPosition) < 2)
        {
            targetAnimal.AddAttackPoint();
            Destroy(gameObject);
            FlyAdventureBlock.flyCount--;
        }
    }
}
