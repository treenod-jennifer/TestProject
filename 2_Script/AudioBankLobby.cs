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
    n_water_fountain,
    n_break_ladder,
    n_clock_tower_broken, //75
    n_knocking,
    kiri,
    mai,
    m_bird_aham2,
    m_boni_laugh,//80
    m_boni_surprise,
    move_down,
    LetterBox2,
    phonebox_appear,
    phonebox_result,    //85
    phonebox_whirl_2,
    phonebox_result_3star,
    phonebox_result_4star,
    phonebox_result_5star,
    phonebox_starsound2,    //90
    phonebox_starsound3,
    phonebox_starsound4,
    phonebox_starsound5,
    adventure_clear,
    animal_levelup, // 95
    animal_levelup_gage,
    event_mole_startup,
    event_mole__Hit_clear_normal,
    event_mole__Hit_clear_hard,
    event_mole_stun,    //100
    event_mole_up,
    event_mole_down,
    event_mole_lightning,
    event_mole_stage_clear,
    event_mole_hit_fail,    //105
    event_mole_laugh,
    event_mole_fanfare,
    event_mole_end,
    event_mole_turnup,
    event_mole_star,    //110
    event_mole_star2,
    event_mole_wave,
    m_hammering_new,
    Mission_BGM_start_new,
    Npc_mission_BGM_new,    //115
    midori_fall,
    midori_fall_2,
    midori_dark_fall,
    m_effect_1,     
    m_misha_appear, //120
    event_capsule_coin,
    event_capsule_open_n,
    event_capsule_open_r,
    event_capsule_out_n,
    event_capsule_out_r, //125
    event_capsule_shake_n,
    event_capsule_shake_r,
    Chat_noi,
    event_criminal_stamp,
    event_criminal_paper, //130
    event_criminal_prison,
    lucky_roulette_spin,
    group_ranking_reward_box_appear,
    group_ranking_reward_box_shake,
    group_ranking_reward_box_open,
    group_ranking_reward_box_item,
    group_ranking_reward_box_end,
    
    Count,

    NO_SOUND = -1,
    DEFAULT_AUDIO = -2,
}

public class AudioBankLobby : MonoBehaviour {

    [EnumNamedArray(typeof(AudioLobby))]
    public List<AudioClip> _audioList = new List<AudioClip>();

}
