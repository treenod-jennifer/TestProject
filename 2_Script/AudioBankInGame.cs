using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum AudioInGame
{
    PANG1,  //0
    PANG2,
    APPLE,
    KEY,
    PLANT_PANG1,
    PLANT_PANG2, //5
    PLANT_PANG3,
    STAGE_CLEAR,
    NET_PANG,
    PANG_RAINBOW,
    PANG_LINE_BOMB, //10
    PANG_LINE,
    PANG_BOMB,
    CRACK_PANG,
    PRAISE0, 
    PRAISE1, //15
    PRAISE2,
    PRAISE3,
    PRAISE4,
    PANG_CIRCLE_BOMB,
    LAST_PANG, //20
    STATUE_GET, 
    GET_TARGET,
    STAGE_FAIL,
    CONTINUE,
    RESULT_STAR1, //25
    RESULT_STAR2,
    RESULT_STAR3,
    RESULT_STAR4,
    STAGE_CLEAR_BUTTON,
    GET_HEART,  //30
    CREAT_BOMB,
    LAND_BLOCK,
    TURN5_LEFT,
    BREAK_1_JAR,
    BREAK_2_JAR,    //35
    BREAK_3_JAR,
    BREAK_WALK_BLOCK_1,
    BREAK_WALK_BLOCK_2,
    GET_STONE_STATUE,
    COMBO1,         //40
    COMBO2,
    COMBO3,
    COMBO4,
    COMBO5,
    COMBO6,         //45
    COMBO7,
    COMBO8,
    COMBO9,
    COMBO10,

    LAVA_LOG,       //50
    GET_CANDY,
    GET_DUCK,
    Get_Jewelry,
    Release_Jewelry1,

    Release_Jewelry2,   //55
    BREAK_SOIL_POT1,
    BREAK_SOIL_POT2,
    LAVA_LOG2,
    STAGE_FAIL2,

    INGAME_ITEM_CLICK,    //60
    CLICK_BLOCK,
    MOVE_GROUND,
    GET_STATUE,
    BLOCKBLOCK_PANG,

    BLACK_PANG,         //65
    WATER_MAKE,
    WATER_PANG,
    DYNAMITE_COUNT_DOWN, 
    DYNAMITE_MATCH,
    DYNAMITE_REMOVE,    //70

    DYNAMITE_FAIL,      //71
    RANK_CONTINUE,
    RANK_NORMAL_CLEAR,  //
    SPECIAL_EVENT_ROUTTE,  
    
    MOVE_WAVE_1,        //75
    FRIEND_NORMAL_ATTACK_SHORT,
    FRIEND_NORMAL_ATTACK_LONG,
    CHARGING,
    HIT_FRIEND_1,
    ENEMY_LAND,         //80
    ENEMY_ATTACK_1,
    ENEMY_ATTACK_2,
    HIT_ENEMY,
    ENEMY_DISAPPEAR_1,
    ENEMY_DISAPPEAR_2,  //85
    BOSS_DISAPPEAR,
    BOSS_STAGE,

    SKILL_THUNDERBOLT,
    SKILL_FREEZE,
    SKILL_HEAL,         //90
    SKILL_HIT_THUNDERBOLT,
    SKILL_HIT_FREEZE,
    SKILL_HIT_HEAL,
    SKILL_COMMON,
    SKILL_INSTALL,

    BLOCK_CHARGING,     //96
    WOOL_MAKE,          //97

    FLOWERPOT_1PANG,    //98
    FLOWERPOT_DESTROY,  

    ENEMESKILL_THROW,   //100
    ENEMESKILL_EXPLOSION_1,
    ENEMESKILL_EXPLOSION_2,

    FEVER_START,    // 103

    BEE_GROWUP,     // 104
    BEE_HIT,
    TIME_ALERT,
    TIMEOVER,   //107

    YOI,
    START,      //109

    TIME_CLEAR,      //110

    SODAJELLY_1PANG,    //111
    SODAJELLY_DESTROY,  //112

    PEA_OPEN,   //113

    WORLDRANKITEM_PANG,   //114

    FLOWERINK_PANG, // 115

    SPACESHIP_PANG, // 116
    SPACESHIP_UP, // 117

    TURNRELAY_WAVECLEAR, //118

    SKILL_HIT_HEARTRAIN, //119

    COINSTASH_COIN,          //120
    INGAME_ITEM_COLOR_BRUSH, //121
    
    Count,  //현재 enum의 마지막 카운트를 알아오기 위해 사용, 항상 마지막에 위치해야함.
}


public class AudioBankInGame : MonoBehaviour
{
    public List<AudioClip> _audioList = new List<AudioClip>();
}
