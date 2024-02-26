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

        transform.position = startPos;
        
        targetAnimal = tempAnimal;
        flyCount++;
    }

    //가속도 넣기
    float speedRatio = -1f;
    float velocity = 0.12f;

    void Update()
    {
        speedRatio += velocity;

        transform.position = Vector3.MoveTowards(transform.position, targetAnimal.gameObject.transform.position, speedRatio * Global.deltaTimePuzzle);

        if (speedRatio < 0f)
            transform.position += Vector3.right* 0.5f * speedRatio * Global.deltaTimePuzzle;

        if (Vector3.Distance(targetAnimal.gameObject.transform.position, transform.position) < 0.05f)
        {

            FlyAdventureBlock.flyCount--;
            targetAnimal.AddAttackPoint();
            Destroy(gameObject);
        }
        /*
        transform.position = Vector3.MoveTowards(transform.position, targetAnimal.gameObject.transform.position, Global.deltaTimePuzzle * 2.5f);

        if (Vector3.Distance(targetAnimal.gameObject.transform.position, transform.position) < 0.05f)
        {
            targetAnimal.AddAttackPoint();
            Destroy(gameObject);
            FlyAdventureBlock.flyCount--;
        }
         */
    }

    /*
    //const float MAX_SPEED = 3.0f;
    IEnumerator Start()
    {
        _transform.position = startPos;

        //거리구하기 
        yield return null;
        
        while (waitTime > 0f)
        {
            waitTime -= Global.deltaTimePuzzle;
            yield return null;
        }        

        while (true)
        {
            _timer += Global.deltaTimePuzzle * 1.5f;


            if (Vector3.Distance(_transform.position , endPos) < 0.001f)
            {
                break;
            }

            speedRatio += velocity;
            //speedRatio = speedRatio > MAX_SPEED ? MAX_SPEED : speedRatio;

            _transform.position = Vector3.MoveTowards(_transform.position, endPos, speedRatio * Global.deltaTimePuzzle);

            if(speedRatio < 0f)
            _transform.position += Vector3.right * 0.5f* speedRatio * Global.deltaTimePuzzle;

            if (speedRatio > 1 && _particleEffect.gameObject.activeSelf == false)
            {
                _particleEffect.gameObject.SetActive(true);
            }      
    */
}
