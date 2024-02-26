using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameEffectMaker : MonoSingletonOnlyScene<InGameEffectMaker>
{
    public GameObject flyTargetObj;
    public GameObject CrackParticleObj;
    //public GameObject NetParticleObj;
    public GameObject grassParticleObj;

    //폭탄 영역 표시 이펙트
    public GameObject bombBlockPangFieldObj;

    public GameObject BlockPangObj;
    public GameObject bombMakePangObj;
    public GameObject bombMakeEffectObj;

    public GameObject flyAppleObj;
    public GameObject flyIceAppleObj;
    public GameObject flyCoinObj;

    public GameObject flyWorldRankObj;
    public GameObject flyEndContentsObj;

    public GameObject BombNObj;

    public GameObject RainbowObj;
    public GameObject RainbowLightObj;

    public GameObject MakeItemEffectObj;
    public GameObject PlantParticleObj;
    
    public GameObject CircleBombObj;
    public GameObject CircleBombObj2;

    public GameObject LargetCircleBombObj;
    
    public GameObject RainbowEffect_Line;
    public GameObject RainbowEffect_circle_Lighting;
    public GameObject RainbowEffect_particle;
    public GameObject RainbowEffect_circle;
    public GameObject RainbowLineObj;

    public GameObject[] CircleBombXBomb;

    public GameObject RainbowHitObj;
    public GameObject flyBlockObj;

    public GameObject LineEffectObj;
    public GameObject LineEffectRightObj;
    public GameObject LineEffectLeftObj;
    public GameObject LargeLineEffectRightObj;
    public GameObject LargeLineEffectLeftObj;
    
    public GameObject CannonEffectRightObj;
    public GameObject CannonEffectLeftObj;
    public GameObject CannonEffectObj;

    public GameObject fiyStatueObj;
    public GameObject flyStatueShadowObj;

    public GameObject LastBombEffectObj;
    public GameObject LastBombPangEffectObj;

    public GameObject ropeEffectObj;
    public GameObject caslteEffectObj;

    public GameObject RainbowObj2;
    public GameObject RainbowCircleObj;

    public GameObject[] PotEffectObj;       //블럭이펙트
    public GameObject GroundEffectObj;  //흙제거
    public GameObject IceEffectObj; //얼음이펙트

    public GameObject IceShineEffect1;
    public GameObject IceShineEffect2;

    public GameObject ScoreObj; //점수
    public GameObject FeverCountObj; //피버 블럭 수
    public GameObject RockEffectObj;
    public GameObject DuckEffectObj;

    public GameObject AppleEffectObj;

    public GameObject KeyShineEffectObj;
    public GameObject JewelShineEffectObj;

    public GameObject KeyGetEffecObj;
    public GameObject JewelGetEffectObj;
    public GameObject BlackEffectObj;

    public GameObject waterPangEffectObj;

    public GameObject UrlEffectObj;

    public GameObject peaBombEffectObj;
    public GameObject RandomBoxDustEffectObj;
    public GameObject RandomBoxOpenEffectObj;

    //다이나마이트 이펙트
    public GameObject DynamiteLineEffectObj;
    public GameObject[] DynamiteColorPangEffectObj;
    public GameObject DynamiteZeroPangEffectObj;

    //폭탄생성
    public GameObject FlyMakeBombObj;

    //모험모드
    public GameObject FlyAdventureBlockObj;
    public GameObject AdventureHitObj;
    public GameObject AdventureComboObj;

    //모험모드 동물 이펙트
    public GameObject EnemyLightingSkillHitEffectObj;
    public GameObject EnemyStunSkillHitEffectObj;
    public GameObject EnemyHitEffectObj;
    public GameObject animalSkill_Attack_Cast01Obj;
    public GameObject animalSkill_Attack_Cast02Obj;
    public GameObject animalSkill_Heal_Cast01Obj;
    public GameObject animalSkill_Heal_Cast02Obj;
    public GameObject animalSkill_Stun_Cast01Obj;
    public GameObject animalSkill_Stun_Cast02Obj;
    
    //노이 부스트 이벤트
    public GameObject boostNoyObj;
    public GameObject boostStardustObj;
    
    //카펫 이펙트
    [SerializeField]
    GameObject carpetEffectObj;

    //벌집 이펙트
    [SerializeField]
    GameObject fireWorkObj;

    //십자 폭탄 이펙트
    [SerializeField]
    GameObject flyCrossBombObj;

    //화단이펙트
    [SerializeField]
    GameObject flowerBedPangObj;
    [SerializeField]
    GameObject flowerBedFlowerObj;
    [SerializeField]
    GameObject flowerBedPangSingleObj;

    //소다젤리 이펙트
    [SerializeField]
    GameObject[] sodaJellyShineObj;
    [SerializeField]
    GameObject[] sodaJellyHitObj;

    //단계석판 이펙트
    [SerializeField]
    GameObject[] countCrackPangObj;

    //월드랭킹 아이템 이펙트
    [SerializeField]
    GameObject worldRankItemPangObj;
    [SerializeField]
    GameObject worldRankItemGyroObj;

    //하트 기믹 이펙트
    [SerializeField]
    GameObject heartShineObj;

    //건전지 기믹 이펙트
    [SerializeField]
    GameObject batteryCastObj;
    [SerializeField]
    GameObject batterySparkObj;
    [SerializeField]
    GameObject batteryGetObj;

    //페인트 이펙트
    [SerializeField]
    GameObject paintPangObj;
    [SerializeField]
    GameObject flyColorChangeObj;

    //빵 이펙트
    [SerializeField]
    GameObject breadEffectObj;

    //물폭탄 이펙트
    [SerializeField]
    GameObject waterBombEffectObj;
    
    //장막 이펙트
    [SerializeField] 
    GameObject cloverEffectObj;
    
    //대포 이펙트
    [SerializeField] 
    GameObject cannonEffectObj;

    #region 폭탄 영역 관련
    //폭탄 영역 이펙트가 출력되고 있는 인덱스를 저장할 리스트
    private List<Vector2Int> listPangFieldIndex = new List<Vector2Int>();
    #endregion

    public GameObject MakeFlowerBedPang(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flowerBedPangObj);
        obj.transform.position = startPos;
        return obj;
    }
    public GameObject MakeFlowerBedFlower(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flowerBedFlowerObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeLittleFlowerBedPang(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flowerBedPangSingleObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeCarpetEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, carpetEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeAdventureEffect(Vector3 targetPos, string EffectName)
    {
        GameObject obj = null;

        if (EffectName == "EnemyLightingSkillHitEffect")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, EnemyLightingSkillHitEffectObj);
        else if (EffectName == "EnemyStunSkillHitEffect")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, EnemyStunSkillHitEffectObj);
        else if (EffectName == "EnemyHitEffect")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, EnemyHitEffectObj);

        else if (EffectName == "animalSkill_Attack_Cast01")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, animalSkill_Attack_Cast01Obj);
        else if (EffectName == "animalSkill_Attack_Cast02")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, animalSkill_Attack_Cast02Obj);
        else if (EffectName == "animalSkill_Heal_Cast01")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, animalSkill_Heal_Cast01Obj);
        else if (EffectName == "animalSkill_Heal_Cast02")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, animalSkill_Heal_Cast02Obj);
        else if (EffectName == "animalSkill_Stun_Cast01")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, animalSkill_Stun_Cast01Obj);
        else if (EffectName == "animalSkill_Stun_Cast02")
            obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, animalSkill_Stun_Cast02Obj);
        else
            return null;

        obj.transform.localPosition = targetPos;
        return obj;
    }

    public FlyAdventureBlock MakeFlyAdventureBlock(BlockColorType blockType, InGameAnimal tempAnimal, Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, FlyAdventureBlockObj);
        FlyAdventureBlock flyBlock = obj.GetComponent<FlyAdventureBlock>();
        flyBlock.initBlock(blockType, tempAnimal, startPos);

        return flyBlock;
    }

    public FlyMakeBomb MakeFlyMakeBomb(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, FlyMakeBombObj);
        obj.transform.position = startPos;
        FlyMakeBomb flyMakeBomb = obj.GetComponent<FlyMakeBomb>();
        return flyMakeBomb;
    }


    //다이나마이트
    public GameObject MakeDynamiteZeroPangEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, DynamiteZeroPangEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeDynamiteLineEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, DynamiteLineEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeDynamitePangColorEffect(BlockColorType tempColor, Vector3 startPos)
    {
        if (tempColor > BlockColorType.NONE && tempColor < BlockColorType.F)
        {
            int colorType = (int)tempColor - 1;
            GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, DynamiteColorPangEffectObj[colorType]);
            obj.transform.position = startPos;
            return obj;
        }
        else
        {
            return null;
        }
    }

    public GameObject MakeWaterPangEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, waterPangEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeUrlEffect(Vector3 startPos, Vector3 endPos, string imageName, Action endAction)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, UrlEffectObj);
        obj.transform.position = startPos;

        FlyUrlEffect urlEffect = obj.GetComponent<FlyUrlEffect>();
        urlEffect.initEffect(startPos, endPos, imageName, endAction);

        return obj;
    }

    public GameObject MakeIceShineEffect1(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, IceShineEffect1);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeIceShineEffect2(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, IceShineEffect2);
        obj.transform.position = startPos;
        return obj;
    }


    public GameObject MakeGetJewelEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, JewelGetEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeGetKeyEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, KeyGetEffecObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeKeyShineEffect(Transform startPos)
    {
        GameObject obj = NGUITools.AddChild(startPos.gameObject, KeyShineEffectObj);
        //obj.transform.position = startPos.position;
        return obj;
    }

    public GameObject MakeJewelShineEffect(Transform startPos)
    {
        GameObject obj = NGUITools.AddChild(startPos.gameObject, JewelShineEffectObj);
        //obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeBombXBombEffect(Vector3 startPos, bool isSecendBomb)
    {
        GameObject obj;
        if (isSecendBomb)
            obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CircleBombXBomb[1]);
        else
            obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CircleBombXBomb[0]);

        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeAppleEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, AppleEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeDuckEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, DuckEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeRockEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RockEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeBlackEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, BlackEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeScore(Vector3 startPos, int score, float delay = 0)
    {
        if (GameManager.gameMode == GameMode.ADVENTURE || GameManager.gameMode == GameMode.COIN)
            return null;

        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, ScoreObj);
        obj.transform.position = startPos;

        BlockScore tempScore = obj.GetComponent<BlockScore>();
        tempScore.initScore(startPos, score.ToString(), delay);

        return obj;
    }

    public GameObject MakeCombo(Vector3 startPos, string score)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, ScoreObj);
        obj.transform.position = startPos;

        BlockScore tempScore = obj.GetComponent<BlockScore>();
        tempScore.initScore(startPos, score, 0, false);
        tempScore._textScore.color = Color.yellow;
        
        return obj;
    }

    public GameObject MakeAdventureCombo(Vector3 startPos, string score)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Advance_Root, AdventureComboObj);
        obj.transform.position = startPos;

        BlockScore tempScore = obj.GetComponent<BlockScore>();
        tempScore.initScore(startPos, score, 0, true);
        tempScore._textScore.color = Color.yellow;

        AdventureManager.instance.ComboObjList.Add(tempScore);
        tempScore._textScore.fontSize = 30;        

        return obj;
    }

    public GameObject MakeAttPoint(Vector3 startPos, string score, bool maxAtt = false)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, ScoreObj);
        obj.transform.position = startPos;

        BlockScore tempScore = obj.GetComponent<BlockScore>();
        tempScore.initScore(startPos, score);
        //tempScore._textScore.fontSize = 30;

        if (maxAtt)
            tempScore._textScore.color = Color.yellow;

        return obj;
    }

    public GameObject MakeCoinStageCombo(Vector3 startPos, int score, float delay = 0)
    {
        if (GameManager.gameMode != GameMode.COIN)
            return null;

        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, FeverCountObj);
        obj.transform.position = startPos;
        BlockFeverCount tempScore = obj.GetComponent<BlockFeverCount>();
        tempScore.initFeverCount(startPos, score.ToString(), delay);
        return obj;
    }

    public GameObject MakeICeEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, IceEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeGroundEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, GroundEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakePotEffect(Vector3 startPos, int count)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, PotEffectObj[count]);
        obj.transform.position = startPos;
        return obj;
    }


    public GameObject MakeRainbowCircleEffect2(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowCircleObj);
        obj.transform.position = startPos;

        return obj;
    }


    public Effect_FireWork MakeFireWork(Vector3 startPos, Vector3 targetPos, BlockBase tempBlock, Board tempBoard, int tempPangIndex, bool isCarpet = false)  
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, fireWorkObj);
        Effect_FireWork rainbowline = obj.GetComponent<Effect_FireWork>();

        obj.transform.localPosition = startPos + GameUIManager.instance.groundAnchor.transform.localPosition;
        rainbowline.targetBlock = tempBlock;
        rainbowline.targetBoard = tempBoard;
        rainbowline.targetPos = targetPos + GameUIManager.instance.groundAnchor.transform.localPosition;
        rainbowline.HasCarpet = isCarpet;

        BlockBomb._liveCount++;
        return rainbowline;
    }

    public Effect_FlyCrossBomb MakeFlyCrossBomb(Vector3 startPos, Board targetBoard)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyCrossBombObj);
        Effect_FlyCrossBomb flyCrossBomb = obj.GetComponent<Effect_FlyCrossBomb>();
        flyCrossBomb.Init(startPos, targetBoard);
        return flyCrossBomb;
    }
    public GameObject MakeRainbowLine3(Vector3 endPos, BlockBase tempBlock, int tempPangIndex, BlockBombType bombType = BlockBombType.NONE, bool isAdventrueRainbowXRainbow = false, bool hasCarpet = false)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowObj2);
        Effect_RainbowLine rainbowline = obj.GetComponent<Effect_RainbowLine>();

        rainbowline.tempBlock = tempBlock;
        rainbowline.bombType = bombType;
        rainbowline.pangIndex = tempPangIndex;
        rainbowline.HasCarpet = hasCarpet;

        obj.transform.position = endPos;
        return obj;
    }

    public GameObject MakeRainbowLine3(Vector3 startPos, Vector3 endPos, BlockBase tempBlock, BlockBase tempBlockB, int tempPangIndex, BlockBombType bombType = BlockBombType.NONE, float waitTimer = 0f, bool isCarpet = false, bool isIngameItem = false)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowObj2);
        Effect_RainbowLine rainbowline = obj.GetComponent<Effect_RainbowLine>();

        rainbowline.tempBlock = tempBlock;
        rainbowline.bombType = bombType;
        rainbowline.pangIndex = tempPangIndex;
        rainbowline.waitTimer = waitTimer;
        rainbowline.HasCarpet = isCarpet;

        if (isIngameItem == true)
        {
            //인게임 아이템4(레인보우스틱)의 경우 폭탄 모으기 목표에 영향 없도록
            rainbowline.pangByIngameItem_RainbowBombHammer = true;
        }

        Vector3 dir = (startPos - endPos).normalized;
        obj.transform.position = (startPos + endPos) * 0.5f;
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
        rainbowline.trailObj.transform.position = startPos;

        float distanceRainbow = (4 * 78) / Vector3.Distance(tempBlock.transform.localPosition, tempBlockB.transform.localPosition);

        rainbowline.speed = rainbowline.speed * distanceRainbow;

        // if (tempBlockB.indexX < 6) rainbowline.curveDir = 1;
        // if (tempBlockB.indexX <= tempBlock.indexX && tempBlockB.indexY != tempBlock.indexY) rainbowline._transform.localScale = new Vector3(1, -1, 1);
        if (tempBlockB.indexX <= tempBlock.indexX && tempBlockB.indexY == tempBlock.indexY) rainbowline._transform.localScale = new Vector3(1, -1, 1);
        return obj;
    }    

    public GameObject MakeRainbowLine2(Vector3 startPos, Vector3 endPos, BlockBase tempBlock, int tempPangIndex, BlockBombType bombType = BlockBombType.NONE, bool isAdventrueRainbowXRainbow = false, bool hasCarpet = false)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowObj2);
        Effect_RainbowLine rainbowline = obj.GetComponent<Effect_RainbowLine>();

        rainbowline.tempBlock = tempBlock;
        rainbowline.bombType = bombType;
        rainbowline.pangIndex = tempPangIndex;
        rainbowline.HasCarpet = hasCarpet;

        Vector3 dir = (startPos - endPos).normalized;
        obj.transform.position = (startPos + endPos) * 0.5f;
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
        rainbowline.trailObj.transform.position = startPos;

        if (startPos.x > endPos.x && startPos.y != endPos.y) rainbowline._transform.localScale = new Vector3(1, -1, 1);

        return obj;
    }

    public GameObject MakeRainbowLine3(Vector3 startPos, Vector3 endPos, BlockBase tempBlock, BlockBase tempBlockB, int tempPangIndex, BlockBombType bombType = BlockBombType.NONE, float waitTimer = 0f, bool isCarpet = false)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowObj2);
        Effect_RainbowLine rainbowline = obj.GetComponent<Effect_RainbowLine>();

        rainbowline.tempBlock = tempBlock;
        rainbowline.bombType = bombType;
        rainbowline.pangIndex = tempPangIndex;
        rainbowline.waitTimer = waitTimer;
        rainbowline.HasCarpet = isCarpet;

        Vector3 dir = (startPos - endPos).normalized;
        obj.transform.position = (startPos + endPos) * 0.5f;
        obj.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
        rainbowline.trailObj.transform.position = startPos;

        float distanceRainbow = (4 * 78)/Vector3.Distance(tempBlock.transform.localPosition, tempBlockB.transform.localPosition) ;

        rainbowline.speed = rainbowline.speed * distanceRainbow;

       // if (tempBlockB.indexX < 6) rainbowline.curveDir = 1;
        // if (tempBlockB.indexX <= tempBlock.indexX && tempBlockB.indexY != tempBlock.indexY) rainbowline._transform.localScale = new Vector3(1, -1, 1);
        if (tempBlockB.indexX <= tempBlock.indexX && tempBlockB.indexY == tempBlock.indexY) rainbowline._transform.localScale = new Vector3(1, -1, 1);

        return obj;
    }


    public GameObject MakeCastleEffect(Vector3 targetPos)       //안쓰는듯
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, caslteEffectObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeRope(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, ropeEffectObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeGrass(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, grassParticleObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeLastBomb(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, LastBombEffectObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeLineTargetEffect(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, LineEffectObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }
    
    public GameObject MakeCannonTargetEffect(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CannonEffectObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeRainbowTargetEffect(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowHitObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeFlyBlock(BlockType blockType , BlockBase targetBlock, Vector3 startPo, bool tempDestroyOnly = false, string name = null, bool rotateSprite = false)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyBlockObj);
        FlyBlock block = obj.GetComponent<FlyBlock>();
        block.initBlock(blockType, targetBlock, startPo, tempDestroyOnly, name, rotateSprite);

        return obj;
    }

    public GameObject MakeEffectCircleBomb(Transform targetBlock, float waitTimer = 0.5f)
    {
        if (targetBlock == null) return null;

        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CircleBombObj);       //targetBlock.gameObject
                                                                                                      //  Effect_BombAniamtion effect = obj.GetComponent<Effect_BombAniamtion>();
                                                                                                      // effect.waitTimer = waitTimer;
        obj.transform.position = targetBlock.position;
        return obj;
    }

    public GameObject MakeEffectCircleBomb2(Transform targetBlock, float waitTimer = 0.5f)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CircleBombObj2); //Effect_Root
                                                                                                 // GameObject obj = NGUITools.AddChild(targetBlock.gameObject, CircleBombObj2); //Effect_Root
                                                                                                 //  Effect_BombAniamtion effect = obj.GetComponent<Effect_BombAniamtion>();
                                                                                                 // effect.waitTimer = waitTimer;
        obj.transform.position = targetBlock.position;
        return obj;
    }
    
    public GameObject MakeEffectCircleBombByPosition(Vector3 localPos, float waitTimer = 0.5f)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CircleBombObj2); //Effect_Root
        obj.transform.localPosition = localPos;
        return obj;
    }

    public GameObject MakeEffectLargeCircleBomb(Transform targetBlock, float waitTimer = 0.5f, float scaleRatio = 1f)       //이것도 안쓰는듯
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, LargetCircleBombObj);
        obj.transform.position = targetBlock.position;
        obj.transform.localScale = Vector3.one * scaleRatio;
        return obj;
    }

    public GameObject MakelastBombPang(Transform targetBlock, float waitTimer = 0.5f)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, LastBombPangEffectObj);
        obj.transform.position = targetBlock.position;
        return obj;
    }


    public GameObject MakeEffectPlant(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, PlantParticleObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectPeaBomb(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, peaBombEffectObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        return obj;
    }

    public GameObject MakeEffectRandomBoxDust(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RandomBoxDustEffectObj);
        obj.transform.position = targetPos;
        return obj;
    }

    public GameObject MakeEffectRandomBoxOpen(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RandomBoxOpenEffectObj);
        obj.transform.position = targetPos;
        return obj;
    }

    public GameObject MakeMakeItemEffect(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, MakeItemEffectObj);// Instantiate(MakeItemEffectObj, targetPos, Quaternion.identity);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectSodaJellyShineEffect(Vector3 targetPos, int index)
    {
        if (index == 0 || (index - 1) >= sodaJellyShineObj.Length)
            return null;
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, sodaJellyShineObj[index - 1]);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectSodaJellyHitEffect(Vector3 targetPos, int index)
    {
        if (index == 0 || (index - 1) >= sodaJellyHitObj.Length)
            return null;
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, sodaJellyHitObj[index - 1]);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectBatteryCastEffect(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, batteryCastObj);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectBatterySparkEffect(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, batterySparkObj);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectBatteryGetEffect(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, batteryGetObj);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectPaintPangEffect(Vector3 targetPos, BlockColorType colorType)
    {
        GameObject effectPaintPangObj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, paintPangObj);
        effectPaintPangObj.transform.position = targetPos;
        effectPaintPangObj.transform.rotation = Quaternion.identity;
        return effectPaintPangObj.gameObject;
    }

    public GameObject MakeEffectFlyColorChangeEffect(Vector3 sPos, Vector3 tPos, BlockColorType colorType)
    {
        FlyColorChangeEffect colorChangeObj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyColorChangeObj).GetComponent<FlyColorChangeEffect>();
        colorChangeObj.transform.localPosition = sPos + GameUIManager.instance.groundAnchor.transform.localPosition;
        colorChangeObj.transform.rotation = Quaternion.identity;

        Vector3 targetPos = tPos + GameUIManager.instance.groundAnchor.transform.localPosition;
        colorChangeObj.InitEffect(targetPos, colorType);
        return colorChangeObj.gameObject;
    }

    public GameObject MakeEffectCountCrackPang(Vector3 targetPos, int index)
    {
        if (index == 0 || (index - 1) >= countCrackPangObj.Length)
            return null;
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, countCrackPangObj[index - 1]);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectWorldRankItemPang(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, worldRankItemPangObj);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeEffectBreadEffect(Vector3 targetPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, breadEffectObj);
        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.identity;
        return obj;
    }

    public GameObject MakeWaterBombEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, waterBombEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    public GameObject MakeCloverEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, cloverEffectObj);
        obj.transform.position = startPos;
        return obj;
    }
    
    public GameObject MakeCannonHitEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, cannonEffectObj);
        obj.transform.position = startPos;
        return obj;
    }

    //레인보우 이펙트
    /*
    public GameObject MakeRainbowLine(Vector3 startPos, Vector3 endPos)
    {
        GameObject _obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowEffect_Line);
        
        Vector3 dir = (startPos - endPos).normalized;
        _obj.transform.position = (startPos + endPos) * 0.5f;
        _obj.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
        float distanceRatio = Vector3.Distance(startPos, endPos)*2.4f ;// / 256;
        _obj.transform.localScale = new Vector3(distanceRatio, 1, 1);

        return _obj;
    }
    */
    //라인

    public Effect_Rainbow_Line_move MakeRainbowLineMove(Vector3 startPos, Vector3 endPos)       //이것도 안씀
    {
        GameObject _obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowLineObj);

        Effect_Rainbow_Line_move rainbow_line = _obj.GetComponent<Effect_Rainbow_Line_move>();

        rainbow_line._startPos = startPos;
        rainbow_line._endPos = endPos;

        int randomType = GameManager.instance.GetIngameRandom(0,1);
        rainbow_line.type = randomType;

        Vector3 dir = (startPos - endPos).normalized;
        _obj.transform.position = (startPos + endPos) * 0.5f;
        _obj.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
        
        return rainbow_line;
    }
    


    public GameObject MakeRainbowCirclLine(Vector3 targetPos)       //이것도 안씀
    {
        GameObject _obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowEffect_circle_Lighting);
        _obj.transform.position = targetPos;
        _obj.transform.rotation = Quaternion.identity;
        return _obj;
    }

    public GameObject MakeRainbowCircleEffect(Vector3 targetPos)
    {
        GameObject _obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowEffect_circle);
        _obj.transform.position = targetPos;
        _obj.transform.rotation = Quaternion.identity;
        return _obj;
    }

    
    public Effect_Rainbow MakeRainbowLine(Vector3 startPos, Vector3 endPos)
    {
        Effect_Rainbow effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, RainbowObj).GetComponent<Effect_Rainbow>();
        effectA._startPos = startPos+ Vector3.up * (GameManager.MOVE_Y * 78);
        effectA._endPos = endPos + Vector3.up * (GameManager.MOVE_Y * 78);

        return effectA;
    }

    public Effect_Rainbow_Light MakeRainbowLight(GameObject tempParentBlock, string tempSpriteName, bool isRainbow = false)
    {
        Effect_Rainbow_Light effectA = NGUITools.AddChild(tempParentBlock, RainbowLightObj).GetComponent<Effect_Rainbow_Light>();
        effectA._rainbow = isRainbow;
        if (tempSpriteName == "")
        {
            effectA._sprite.width = 2;
            effectA._sprite.height = 2;
        }  
        else
        {
            effectA._sprite.spriteName = tempSpriteName;
        }

        MakePixelPerfect(effectA._sprite);
        if (!isRainbow)
            effectA._sprite.depth = ManagerBlock.EFFECT_RAINBOW_LIGHT;

        return effectA;
    }


    public GameObject MakeBlockPangEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, BlockPangObj);
        obj.transform.position = startPos;

        ManagerBlock.instance.listObject.Add(obj);
        return obj;
    }

    public ActiveHelperEffect MakeBombMakePangFieldEffect(Vector2Int indexData, float? dTime = null)
    {
        //폭탄 영역 이펙트 생성 및 제거 콜백/시간 등록
        ActiveHelperEffect effectObj = InGameObjectPoolManager.instance.ObjectPoolSpawn(EFFECT_TYPE.PANGFIELD);
        effectObj.SetCallback(() => InGameEffectMaker.instance.RemoveListPangFieldEffect(indexData));
        effectObj.SetActiveEffect(dTime);

        //위치 설정
        Vector3 startPos = PosHelper.GetPosByIndex(indexData.x, indexData.y);
        effectObj.transform.localPosition = startPos + GameUIManager.instance.groundAnchor.transform.localPosition;

        //해당 이펙트가 생성된 인덱스 데이터 등록
        InGameEffectMaker.instance.RegisterListPangFieldEffect(indexData);
        return effectObj;
    }

    public GameObject MakeBombMakePangEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, bombMakePangObj);
        obj.transform.position = startPos;

        ManagerBlock.instance.listObject.Add(obj);
        return obj;
    }

    public GameObject MakeBombMakeEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, bombMakePangObj);
        obj.transform.position = startPos;

        GameObject obj2 = NGUITools.AddChild(GameUIManager.instance.Effect_Root, bombMakeEffectObj);
        obj2.transform.position = startPos;

        ManagerBlock.instance.listObject.Add(obj);
        ManagerBlock.instance.listObject.Add(obj2);
        return obj;
    }

    public GameObject MakeCrackParticle(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CrackParticleObj);
        obj.transform.position = startPos;

        ManagerBlock.instance.listObject.Add(obj);
        return obj;
    }

    /// <summary>
    /// 타겟으로 날아가는 이펙트 생성
    /// <returns></returns>
    public FlyTarget MakeFlyTarget(Vector3 startPos, TARGET_TYPE type, params BlockColorType[] colorTypes)
    {
        BlockColorType targetColor = (colorTypes.Length > 0) ? colorTypes[0] : BlockColorType.NONE;

        //달성해야 할 목표가 남아있지 않다면 날아가는 이펙트 생성하지 않음
        if (ManagerBlock.instance.HasAchievedCollectTarget(type, targetColor) == false)
            return null;

        ManagerBlock.instance.UpdateCollectTarget_PangCount(type, targetColor);
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyTargetObj);
        obj.transform.position = startPos;

        Vector3 endPos = Vector3.zero;
        bool HasTarget = false;
        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            GameMissionTarget targetUIData = GameUIManager.instance.listGameTarget[i];
            if (targetUIData.targetType == type && targetUIData.targetColor == targetColor)
            {
                endPos = GameUIManager.instance.listGameTarget[i].transform.position;
                HasTarget = true;
                break;
            }
        }

        if(!HasTarget)
        {
            Destroy(obj);
            return null;
        }
        FlyTarget flyTarget = obj.GetComponent<FlyTarget>();
        flyTarget.InitFlyTarget(startPos, endPos, type, colorTypes);

        ManagerBlock.instance.listObject.Add(obj);
        return flyTarget;
    }

    float flyTargetMakeTime = 0;

    int startMakeFly = 0;
    IEnumerator CoMakeFlyTarget(Vector3 startPos, BlockColorType tempColor)
    {
        float waitTimer = 0;

        while (startMakeFly > 0)
        {
            waitTimer = 0;
            while (waitTimer < 0.004f)
            {
                waitTimer += Global.deltaTimePuzzle;
                yield return null;
            }
            yield return null;
        }
        startMakeFly++;
        yield return null;

        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyTargetObj);
        obj.transform.position = startPos;

        Vector3 endPos = Vector3.zero;
        bool HasTarget = false;
        for (int i = 0; i < GameUIManager.instance.listGameTarget.Count; i++)
        {
            GameMissionTarget targetUIData = GameUIManager.instance.listGameTarget[i];
            if (targetUIData.targetType == TARGET_TYPE.COLORBLOCK && targetUIData.targetColor == tempColor)
            {
                endPos = GameUIManager.instance.listGameTarget[i].transform.position;
                HasTarget = true;
                break;
            }
        }

        if (!HasTarget)
        {
            Destroy(obj);
        }
        else
        {
            FlyTarget flyTarget = obj.GetComponent<FlyTarget>();
            flyTarget.InitFlyTarget(startPos, endPos, TARGET_TYPE.COLORBLOCK, tempColor);
            ManagerBlock.instance.listObject.Add(obj);
        }

        startMakeFly--;
        yield return null;
    }

    public FlyTarget MakeFlyFeverBlock(Vector3 startPos, BlockColorType tempColor)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyTargetObj);
        obj.transform.position = startPos;

        FlyTarget flyTarget = obj.GetComponent<FlyTarget>();
        flyTarget.InitFlyTarget(startPos, GameUIManager.instance.feverBlockColor.transform.position, TARGET_TYPE.COLORBLOCK, tempColor);
        return flyTarget;
    }
    
    public FlyTarget MakeFlyTargetColor(Vector3 startPos, BlockColorType tempColor)
    {
        ManagerBlock.instance.UpdateCollectTarget_PangCount(TARGET_TYPE.COLORBLOCK, tempColor);
        StartCoroutine(CoMakeFlyTarget(startPos, tempColor));
        return null;
    }

    public FlyIceApple MakeFlyIceApple(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyIceAppleObj);
        obj.transform.position = startPos;

        FlyIceApple flyTarget = obj.GetComponent<FlyIceApple>();
        flyTarget.startPos = startPos;
        flyTarget.endPos = GameUIManager.instance.turnSpirte.transform.position;


        ManagerBlock.instance.listObject.Add(obj);

        return flyTarget;
    }

    public FlyApple MakeFlyApple(Vector3 startPos, int count, float delay)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyAppleObj);
        obj.transform.position = startPos;

        FlyApple flyTarget = obj.GetComponent<FlyApple>();
        flyTarget.startPos = startPos;
        flyTarget.endPos = GameUIManager.instance.moveCountLabel.transform.position;
        flyTarget._delay = delay;

        ManagerBlock.instance.listObject.Add(obj);

        return flyTarget;
    }

    public FlyCoin MakeFlyCoinAdventure(Vector3 startPos, int count, float delay = 0f)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.TopUIRoot, flyCoinObj);
        obj.transform.position = startPos;

        FlyCoin flyTarget = obj.GetComponent<FlyCoin>();
        flyTarget.startPos = startPos;

        flyTarget.endPos = GameUIManager.instance.adventureCoinLabel.transform.position;

        flyTarget._delay = delay;
        ManagerBlock.instance.listObject.Add(obj);
        return flyTarget;
    }

    public FlyCoin MakeFlyCoin(Vector3 startPos, int count, float delay = 0f, bool isAddCoin = true)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyCoinObj);
        obj.transform.position = startPos;

        FlyCoin flyTarget = obj.GetComponent<FlyCoin>();
        flyTarget.startPos = startPos;

        flyTarget.endPos = GameUIManager.instance.coinSprite.transform.position;    

        flyTarget._delay = delay;

        flyTarget.count = count;

        flyTarget.isAddCoin = isAddCoin;
        ManagerBlock.instance.listObject.Add(obj);
        return flyTarget;
    }
    
    public Effect_Cannon MakeEffectCannonLine(Vector3 startPos, EEffectCannonLine tempType, float opacity = 1f)
    {
        Effect_Cannon effectA = null;

        if (tempType == EEffectCannonLine.eHLeft || tempType == EEffectCannonLine.eVDown)
        {
            effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CannonEffectLeftObj).GetComponent<Effect_Cannon>();
        }
        else
        {
            effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, CannonEffectRightObj).GetComponent<Effect_Cannon>();
        }

        effectA._transform.position = startPos;
        effectA._type = tempType;

        if(opacity != 1f) effectA.SetOpacity(opacity);

        return effectA;
    }

    public Effect_Line MakeEffectLine(Vector3 startPos, EEffectBombLine tempType, bool largeBomb = false, bool crossBomb = false, int arrowDepth = 0, float opacity = 1f)
    {
        Effect_Line effectA = null;

        if (!largeBomb)
        {
            if (tempType == EEffectBombLine.eHLeft || tempType == EEffectBombLine.eVDown)
            {
                effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, LineEffectLeftObj).GetComponent<Effect_Line>();
                effectA.arrowDepth = arrowDepth;
                effectA.crossLine = crossBomb;
            }
            else
            {
                effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, LineEffectRightObj).GetComponent<Effect_Line>();
                effectA.arrowDepth = arrowDepth;
                effectA.crossLine = crossBomb;
            }
        }
        else
        {
            if (tempType == EEffectBombLine.eHLeft || tempType == EEffectBombLine.eVDown)
            {
                effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, LargeLineEffectLeftObj).GetComponent<Effect_Line>();
                effectA.arrowDepth = arrowDepth;
                effectA.crossLine = crossBomb;
            }
            else
            {
                effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, LargeLineEffectRightObj).GetComponent<Effect_Line>();
                effectA.arrowDepth = arrowDepth;
                effectA.crossLine = crossBomb;
            }
        }

        effectA._transform.position = startPos;
        effectA._type = tempType;

        if(opacity != 1f) effectA.SetOpacity(opacity);

        /*
        effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, bombLineObj).GetComponent<Effect_Line>();
        effectA._transform.position = startPos;
        effectA._type = tempType;
        //effectA._scale = new Vector3(1f, (largeBomb ? 2.5f:1f), 1f);     
        if (tempType == EEffectBombLine.eHLeft || tempType == EEffectBombLine.eVDown)
            effectA._sprite.spriteName = "Line_Bomb_Left"; //"blockWolfL";
        else
            effectA._sprite.spriteName = "Line_Bomb_Right";//"blockWolfR";
            */

        return effectA;
    }

    public Effect_BombN MakeEffectBombL(Vector3 startPos, float tempAddScale =  0.1f)
    {
        Effect_BombN effectA = NGUITools.AddChild(GameUIManager.instance.Effect_Root, BombNObj).GetComponent<Effect_BombN>();
        MakePixelPerfect(effectA._sprite1);
        MakePixelPerfect(effectA._sprite2);
        effectA.transform.position = startPos;
        effectA._addScale = tempAddScale;

        return effectA;
    }

    public FlyStatue MakeFlyStatue(Vector3 startPos, float startRotatePos, Vector3 statueScale)
    {
        FlyStatue flyStatue = null;

        if (GameUIManager.instance.Liststatue.Count == 0)
        {
            flyStatue = NGUITools.AddChild(GameUIManager.instance.Effect_Root, fiyStatueObj).GetComponent<FlyStatue>();
        }
        else
        {
            while (true)
            {
                if (GameUIManager.instance.Liststatue[0] != null)
                {
                    break;
                }
                else
                {
                    GameUIManager.instance.Liststatue.RemoveAt(0);
                }
            }

            GameUIManager.instance.Liststatue[0].SetActive(true);
            flyStatue = GameUIManager.instance.Liststatue[0].GetComponent<FlyStatue>();
            GameUIManager.instance.Liststatue[0].transform.parent = GameUIManager.instance.Effect_Root.transform;
            GameUIManager.instance.Liststatue.RemoveAt(0);
        }

        flyStatue.startPos = startPos;
        flyStatue.startRotatePos = startRotatePos;
        flyStatue.startScale = statueScale;

        FlyStatue.flyStatueList.Add(flyStatue);
        return flyStatue;
    }

    public GameObject MakeFlyWorldRankItem(Vector3 startPos, Vector3 endPos, Action endAction)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyWorldRankObj);
        obj.transform.position = startPos;

        FlyWorldRankItem worldRankEffect = obj.GetComponent<FlyWorldRankItem>();
        worldRankEffect.InitEffect(startPos, endPos, endAction);

        return obj;
    }
    
    public GameObject MakeFlyEndContentsItem(Vector3 startPos, Vector3 endPos, Action endAction)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, flyEndContentsObj);
        obj.transform.position = startPos;

        FlyEndContentsItem endContentsEffect = obj.GetComponent<FlyEndContentsItem>();
        endContentsEffect.InitEffect(startPos, endPos, endAction);

        return obj;
    }

    public IngameGyroObj MakeFlyWorldRankGyroItem()
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.gyroSpawn_Root, worldRankItemGyroObj);

        float randPos_X = UnityEngine.Random.Range(-300f, 300f);
        obj.transform.localPosition = new Vector3(randPos_X, 0f, 0f);

        return obj.GetComponent<IngameGyroObj>();
    }

    public void RemoveEffect(GameObject obj)
    {
        ManagerBlock.instance.listObject.Remove(obj);
        Destroy(obj);
    }

    private void MakePixelPerfect(UISprite sprite, float offset = 1.25f)
    {
        sprite.MakePixelPerfect();
        sprite.width = Mathf.RoundToInt(sprite.width * offset);
        sprite.height = Mathf.RoundToInt(sprite.height * offset);
    }
    
    public GameObject MakeBoostNoyEffect()
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, boostNoyObj);
        obj.transform.position = Vector3.zero;
        obj.transform.localScale = Vector3.one * 100f;
        return obj;
    }
    
    public GameObject MakeBoostStardustEffect(Vector3 startPos)
    {
        GameObject obj = NGUITools.AddChild(GameUIManager.instance.Effect_Root, boostStardustObj);
        obj.transform.localPosition = startPos;
        obj.transform.localScale = Vector3.one * 80f;
        return obj;
    }

    #region 폭탄 영역 관련 함수
    /// <summary>
    /// 폭탄 이펙트가 출력되고 있는 인덱스를 리스트에 등록
    /// </summary>
    public void RegisterListPangFieldEffect(Vector2Int indexData)
    {
        listPangFieldIndex.Add(indexData);
    }

    /// <summary>
    /// 폭탄 이펙트가 출력된 인덱스를 찾아 리스트에서 제거
    /// </summary>
    public void RemoveListPangFieldEffect(Vector2Int indexData)
    {
        int fIdx = GetIndexPangFieldEffect(indexData);
        if (fIdx != -1)
            listPangFieldIndex.RemoveAt(fIdx);
    }

    /// <summary>
    /// 폭탄 이펙트가 등록된 리스트의 인덱스를 찾아 반환
    /// </summary>
    public int GetIndexPangFieldEffect(Vector2Int indexData)
    {
        int fIdx = listPangFieldIndex.FindIndex(x => x == indexData);
        return fIdx;
    }

    public void MakePangFieldEffectToDirection(int indexX, int indexY, BlockBombType type, int offX = 0, int offY = 0, List<BombAreaInfo> _infoList = null, bool bNoneDirection = false, int unique = 0, float? dTime = null)
    {
        switch (type)
        {
            case BlockBombType.LINE:
                MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1, _unique: unique);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, offX, offY, 10, _unique: unique);
                break;
            case BlockBombType.LINE_X_LINE:
                MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1, _unique: unique);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, 1, 0, 10, _unique: unique);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, 0, 1, 10, _unique: unique);
                break;
            case BlockBombType.BOMB: //circle
                MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1, _unique: unique);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, 1, 0, 1, _infoList, bNoneDirection, unique);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, 0, 1, 1, _infoList, bNoneDirection, unique);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, 1, 1, 1, _infoList, bNoneDirection, unique);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, 1, -1, 1, _infoList, bNoneDirection, unique);
                break;
            case BlockBombType.BLACK_BOMB:
            case BlockBombType.CLEAR_BOMB:
                MakePangFieldEffectToDirection_SingleLine(indexX, indexY, 0, 0, 1, _infoList, _unique: unique, dTime : dTime);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, 1, 0, 1, _infoList, _unique: unique, dTime: dTime);
                MakePangFieldEffectToDirection_BothLine(indexX, indexY, 0, 1, 1, _infoList, _unique: unique, dTime: dTime);
                break;
        }
        return;
    }

    public void MakePangFieldEffectDiagonalBlock(int x, int y, int addX, int addY, int count, List<BombAreaInfo> infoList = null, bool bNoneDirection = false, int unique = 0, int indexX = -1, int indexY = -1)
    {
        Board back = PosHelper.GetBoardSreeen(x, y, addX, addY);
        BlockDirection bombDirection = ManagerBlock.instance.GetBombDirection(addX, addY);

        if (back != null && count > 0)
        {
            //폭탄 영역 표시 출력할지 검사
            bool showFieldEffect = ManagerBlock.instance.IsCanShowPangFieldByBomb(back, BlockDirection.NONE, unique);
            if (showFieldEffect == true)
                MakeBombPangFieldEffect(back);

            if (ManagerBlock.instance.IsCanPangExtendByBomb(back, bombDirection, unique))
            {
                bool bombExtend = true;
                for (int j = 0; j < back.BoardOnDisturbs.Count; j++)
                {
                    IDisturb disturb = back.BoardOnDisturbs[j];
                    bombExtend = ManagerBlock.instance.IsCanExtensionBombDirection(back, disturb, back.indexX, back.indexY, bombDirection);
                    if (bombExtend == false)
                        break;
                }

                if (bombExtend == true)
                {
                    MakePangFieldEffectToDirection_SingleLine(x + addX, y + addY, addX, 0, count - 1, infoList, bNoneDirection, unique, x, y);
                    MakePangFieldEffectToDirection_SingleLine(x + addX, y + addY, 0, addY, count - 1, infoList, bNoneDirection, unique, x, y);
                }
                if (ManagerBlock.instance.IsDiagonalDisturbPang(x + addX, y + addY, addX, addY) == false)
                    MakePangFieldEffectDiagonalBlock(x + addX, y + addY, addX, addY, count - 1, infoList, bNoneDirection, unique, x, y);
            }
            MakePangFieldEffectToDirectionDeco(back, BlockDirection.NONE, infoList, bNoneDirection, (indexX == -1 ? x : indexX), (indexY == -1 ? y : indexY), unique);
        }
    }

    //한 방향으로만 이펙트 발생
    public void MakePangFieldEffectToDirection_SingleLine(int inX, int inY, int offX, int offY, int count = 10, List<BombAreaInfo> infoList = null, bool bNoneDirection = false, int _unique = 0, int indexX = -1, int indexY = -1, bool isAlwaysEffectOn = false, float? dTime = null)
    {
        int offsetX = offX;
        int offsetY = offY;

        bool bombExtend = true;
        BlockDirection bombDirection = ManagerBlock.instance.GetBombDirection(offsetX, offsetY);

        for (int i = 0; i < count; i++)
        {
            Board back = PosHelper.GetBoardSreeen(inX, inY, offsetX, offsetY);
            if (back != null && bombExtend)
            {
                //폭탄 영역 표시 출력할지 검사
                bool showFieldEffect = ManagerBlock.instance.IsCanShowPangFieldByBomb(back, bombDirection, _unique, isAlwaysEffectOn );
                if (showFieldEffect == true)
                    MakeBombPangFieldEffect(back, dTime);

                //해당 방향으로 폭탄 계속 퍼져나갈지 검사
                bombExtend = ManagerBlock.instance.IsCanPangExtendByBomb(back, bombDirection, _unique);
                if (bombExtend == true)
                {
                    for (int j = 0; j < back.BoardOnDisturbs.Count; j++)
                    {
                        IDisturb disturb = back.BoardOnDisturbs[j];
                        bombExtend = ManagerBlock.instance.IsCanExtensionBombDirection(back, disturb, back.indexX, back.indexY, bombDirection);
                        if (bombExtend == false)
                            break;
                    }
                }
                MakePangFieldEffectToDirectionDeco(back, bombDirection, infoList, bNoneDirection, (indexX == -1 ? inX : indexX), (indexY == -1 ? inY : indexY), _unique);
            }

            offsetX += offX;
            offsetY += offY;
        }
    }
    
    //한 방향으로만 이펙트 발생 : 관통 효과 (돌 울타리, 바위 무시)
    public void MakePangFieldEffectToDirection_SingleLine_NotDisturb(int inX, int inY, int offX, int offY, int count = 10, List<BombAreaInfo> infoList = null, bool bNoneDirection = false, int _unique = 0, int indexX = -1, int indexY = -1, bool isAlwaysEffectOn = false, float? dTime = null)
    {
        int offsetX = offX;
        int offsetY = offY;

        BlockDirection bombDirection = ManagerBlock.instance.GetBombDirection(offsetX, offsetY);

        for (int i = 0; i < count; i++)
        {
            Board back = PosHelper.GetBoardSreeen(inX, inY, offsetX, offsetY);
            if (back != null)
            {
                //폭탄 영역 표시 출력할지 검사
                bool showFieldEffect = ManagerBlock.instance.IsCanShowPangFieldByBomb_NotDisturb(back, bombDirection, _unique, isAlwaysEffectOn );
                if (showFieldEffect == true)
                    MakeBombPangFieldEffect(back, dTime);
                MakePangFieldEffectToDirectionDeco(back, bombDirection, infoList, bNoneDirection, (indexX == -1 ? inX : indexX), (indexY == -1 ? inY : indexY), _unique);
            }

            offsetX += offX;
            offsetY += offY;
        }
    }

    //양쪽 방향 둘 다 이펙트 발생
    public void MakePangFieldEffectToDirection_BothLine(int inX, int inY, int offX, int offY, int count = 10, List<BombAreaInfo> infoList = null, bool bNoneDirection = false, int _unique = 0, int indexX = -1, int indexY = -1, bool isAlwaysEffectOn = false, float? dTime = null)
    {
        int offsetX = offX;
        int offsetY = offY;

        bool upBombExtend = true;
        bool downBombExtend = true;

        BlockDirection upDirection = ManagerBlock.instance.GetBombDirection(offsetX, offsetY);
        BlockDirection downDirection = BlockDirection.NONE;

        if (upDirection == BlockDirection.RIGHT)
            downDirection = BlockDirection.LEFT;
        else if (upDirection == BlockDirection.LEFT)
            downDirection = BlockDirection.RIGHT;
        else if (upDirection == BlockDirection.DOWN)
            downDirection = BlockDirection.UP;
        else if (upDirection == BlockDirection.UP)
            downDirection = BlockDirection.DOWN;

        //대각선 방향의 블럭을 검사하는 경우, 주위에 블럭 팡을 방해하는 데코가 없는지 검사함.
        if (offsetX != 0 && offsetY != 0)
        {
            if (ManagerBlock.instance.IsDiagonalDisturbPang(inX, inY, offsetX, offsetY) == true)
            {
                upBombExtend = false;
            }
            if (ManagerBlock.instance.IsDiagonalDisturbPang(inX, inY, -offsetX, -offsetY) == true)
            {
                downBombExtend = false;
            }
        }
        else
        {   // 현재 폭탄이 향하는 방향과 반대 방향에 방해블럭이 있으면, 해당 방향으로는 폭탄 영역 확장안되도록 함.
            Board checkBoard = PosHelper.GetBoardSreeen(inX, inY, 0, 0);

            if (checkBoard != null)
            {
                for (int i = 0; i < checkBoard.BoardOnDisturbs.Count; i++)
                {
                    IDisturb disturb = checkBoard.BoardOnDisturbs[i];
                    if (upBombExtend == true && ManagerBlock.instance.IsCanExtensionBombDirection(checkBoard, disturb, inX, inY, upDirection) == false)
                        upBombExtend = false;
                    if (downBombExtend == true && ManagerBlock.instance.IsCanExtensionBombDirection(checkBoard, disturb, inX, inY, downDirection) == false)
                        downBombExtend = false;
                    MakePangFieldEffectToDirectionDeco(checkBoard, upDirection, infoList, bNoneDirection, (indexX == -1 ? inX : indexX), (indexY == -1 ? inY : indexY), _unique);
                    MakePangFieldEffectToDirectionDeco(checkBoard, downDirection, infoList, bNoneDirection, (indexX == -1 ? inX : indexX), (indexY == -1 ? inY : indexY), _unique);
                }
            }
        }

        for (int i = 0; i < count; i++)
        {
            Board Upback = PosHelper.GetBoardSreeen(inX, inY, offsetX, offsetY);
            Board Downback = PosHelper.GetBoardSreeen(inX, inY, -offsetX, -offsetY);

            if (Upback != null && upBombExtend)
            {
                //폭탄 영역 표시 출력할지 검사
                bool showFieldEffect = ManagerBlock.instance.IsCanShowPangFieldByBomb(Upback, upDirection, _unique, isAlwaysEffectOn);
                if (showFieldEffect == true)
                    MakeBombPangFieldEffect(Upback, dTime);

                //해당 방향으로 폭탄 계속 퍼져나갈지 검사
                upBombExtend = ManagerBlock.instance.IsCanPangExtendByBomb(Upback, upDirection, _unique);
                if (upBombExtend == true)
                {
                    //해당방향으로 방해블럭이 있지는 않은지 검사.
                    for (int j = 0; j < Upback.BoardOnDisturbs.Count; j++)
                    {
                        IDisturb disturb = Upback.BoardOnDisturbs[j];
                        if (upBombExtend == true && ManagerBlock.instance.IsCanExtensionBombDirection(Upback, disturb, Upback.indexX, Upback.indexY, upDirection) == false)
                            upBombExtend = false;
                    }
                }
                MakePangFieldEffectToDirectionDeco(Upback, upDirection, infoList, bNoneDirection, (indexX == -1 ? inX : indexX), (indexY == -1 ? inY : indexY), _unique);
            }

            if (Downback != null && downBombExtend)
            {
                //폭탄 영역 표시 출력할지 검사
                bool showFieldEffect = ManagerBlock.instance.IsCanShowPangFieldByBomb(Downback, downDirection, _unique, isAlwaysEffectOn);
                if (showFieldEffect == true)
                    MakeBombPangFieldEffect(Downback, dTime);

                //해당 방향으로 폭탄 계속 퍼져나갈지 검사
                downBombExtend = ManagerBlock.instance.IsCanPangExtendByBomb(Downback, downDirection, _unique);
                if (downBombExtend == true)
                {
                    for (int j = 0; j < Downback.BoardOnDisturbs.Count; j++)
                    {
                        IDisturb disturb = Downback.BoardOnDisturbs[j];
                        if (downBombExtend == true && ManagerBlock.instance.IsCanExtensionBombDirection(Downback, disturb, Downback.indexX, Downback.indexY, downDirection) == false)
                            downBombExtend = false;
                    }
                }
                MakePangFieldEffectToDirectionDeco(Downback, downDirection, infoList, bNoneDirection, (indexX == -1 ? inX : indexX), (indexY == -1 ? inY : indexY), _unique);
            }
            offsetX += offX;
            offsetY += offY;
        }
    }

    public void MakePangFieldEffect_PowerBomb(int minX, int minY, int maxX, int maxY, List<BombAreaInfo> infoList, int _unique, BlockColorType colorType, float? dTime = null)
    {
        //파워해머 폭탄 영역 표시
        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                Board checkBoard = PosHelper.GetBoardSreeen(i, j);
                if (checkBoard == null)
                    continue;
                
                //폭발 영역 내에 있는 데코에 폭발 연출 적용
                MakePangFieldEffectToDirectionDeco(checkBoard, BlockDirection.UP, infoList, true, i, j, 0);
                MakePangFieldEffectToDirectionDeco(checkBoard, BlockDirection.DOWN, infoList, true, i, j, 0);
                MakePangFieldEffectToDirectionDeco(checkBoard, BlockDirection.RIGHT, infoList, true, i, j, 0);
                MakePangFieldEffectToDirectionDeco(checkBoard, BlockDirection.LEFT, infoList, true, i, j, 0);

                //폭발 영역 내에 폭발 이펙트 표시
                bool isAlwaysEffectOn = (checkBoard.Block == null) ? false : checkBoard.Block.IsCanPangByPowerBomb();
                bool showFieldEffect = ManagerBlock.instance.IsCanShowPangFieldByBomb(checkBoard, BlockDirection.NONE, _unique, isAlwaysEffectOn);
                if (showFieldEffect == true)
                    MakeBombPangFieldEffect(checkBoard, dTime);
            }
        }
    }

    //방향으로 터질수 있는 데코 검사.
    private void MakePangFieldEffectToDirectionDeco(Board checkBoard, BlockDirection direction, List<BombAreaInfo> infoList, bool bNoneDirection, int indexX, int indexY, int _uniqueIndex)
    {
        //현재 보드에 있는 데코 중, 방향 확인해서 터지는 데코들에 대한 처리.
        foreach (DecoBase boardDeco in checkBoard.DecoOnBoard)
        {
            if (boardDeco.IsDisturbDeco_ByBomb(checkBoard, indexX, indexY, _uniqueIndex, direction, infoList, bNoneDirection))
            {
                boardDeco.MakeFieldEffect();
            }
        }
    }

    /// <summary>
    /// 특정 블럭 아래 폭탄 영역 이펙트 생성하는 함수
    /// destroyTime : 제거되기까지 대기 시간
    /// </summary>
    public void MakeBombPangFieldEffect(Board board, float? dTime = null)
    {
        Vector2Int indexData = new Vector2Int(board.indexX, board.indexY);
        MakeBombMakePangFieldEffect(indexData, dTime);
    }
    #endregion
}
