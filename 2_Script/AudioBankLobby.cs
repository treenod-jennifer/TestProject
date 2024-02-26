using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AudioLobby
{
    Button_01, //0
    PopUp,
    UseClover,
    CreateMission,
    Chat_Boni,
    Chat_Other, //5
    Mission_Before,
    Mission_Finish,
    Mission_ButtonClick,
    HEART_SHORTAGE,
    Button_02,  //10
    BoniStep,
    LetterBox,
    Mission_BGM_End2,
    liftup_shovel,
    m_hammering,    //15
    m_hammering_ready,
    m_hand_ready,
    m_handing,
    m_painting,
    m_painting_ready,   //20
    m_shovel_ready,
    m_shoveling,
    Mission_BGM_start,
    m_coco_axing,
    m_fix_glass_fixglass,       //25
    m_fix_glass_ready,
    m_sprinkling_water_complete,
    m_sprinkling_water_end_flower,
    m_sprinkling_water_ready,
    m_sprinkling_water_sprinkle,    //30
    m_stone_road_end_stones,
    m_stone_road_making,
    m_boni_haa,
    m_boni_hoa,
    m_boni_tubi,        //35
    m_boni_wow,
    m_bird_happy,
    m_bird_aham,
    m_bird_hehe,
    Deco_Get,   //40
    Housing_Select,
    m_Aroo_speak,
    m_BGM_getpie,
    m_Pie_box,
    m_see_tree_all,
    m_hammering2,
    m_jelly_voive1,
    m_putdown_chair,
    m_make_medicine_melodic,
    m_medicine_bottle,    //50
    m_cut_tree,
    m_drop_table,
    m_make_flowerbed,
    Chat_coco,
    Chat_peng,  //55
    Chat_jeff,
    Chat_jelly,
    Chat_aroo,
    m_bird_oho,
    m_bird_aaa, //60
    m_boni_ahho,
    m_boni_ohho,
    m_boni_yaho,
    m_boni_ye,
    m_bird_hansum,  //65
	Npc_mission_BGM,
	Npc_mission_BGM_end,
    Call_Start,
    Call_End,
    m_alphonse_hello,   //70
    m_alphonse_good,
    Chat_alphonse,
    Count,

    NO_SOUND = -1,
    DEFAULT_AUDIO = -2,
}

public class AudioBankLobby : MonoBehaviour {


    public List<AudioClip> _audioList = new List<AudioClip>();

}
