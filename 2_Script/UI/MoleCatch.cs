using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

[System.Serializable]
public class MoleCatchHole
{
    public UITexture hole;
    public Texture _holeNormal;
    public Texture _holedHard;
}

public class MoleCatch : MonoBehaviour
{
    //sorting Layer 사용해야하는 오브젝트 리스트(리스트에 들어간 순서대로 뎁스올려줌)
    public List<GameObject> listSortLayerObject;
    public GameObject effectRoot;

    public GameObject midRewardRoot;
    public GameObject midRewardBase;
    public GameObject midRewardLineRoot;

    public UIUrlTexture finalRewardImage;

    #region 스파인 오브젝트(화면 뒷판, 두더지, 뿅망치)
    public List<UIItemMoleCatch> listMole;
    public SkeletonAnimation spineHammer;
    public SkeletonAnimation spineMachine;
    #endregion

    #region 텍스쳐(보드, 텍스트)
    public List<UITexture> listBoardTexture;
    public List<UITexture> listStageText;
    public List<UITexture> listBonusText;

    public UIMoleBoardCover boardCover;

    public Texture2D[] waveAlphaSource;
    public Texture2D[] boardAlphaSource;
    public Texture2D waveBoardClearAlphaSource;
    #endregion

    #region 두더지 홀
    public List<MoleCatchHole> listMoleCatchHole;
    #endregion

    #region 텍스트(웨이브)
    public UILabel waveText;
    #endregion

    #region 컬러(보드, 텍스트)
    public Color boardColor_1;
    public Color textColor_1;
    public Color bonusTextColor_1;

    public Color boardColor_2;
    public Color textColor_2;
    public Color bonusTextColor_2;
    #endregion    

    #region 이펙트
    public List<GameObject> listEffect_Light;
    public GameObject _objEffectHit;
    public GameObject _objEffectBonus;
    public GameObject lastRewardEffect;
    public Transform bonusEffectTr;
    #endregion

    #region 사운드
    //AudioClip 형태로 받는 사운드부터 검사, 
    //AudioClip 이 null이면 AudioLobby 에 등록된 사운드 출력.
    public AudioLobby hitVoice = AudioLobby.event_mole_stun;
    public AudioLobby missVoice = AudioLobby.event_mole_laugh;
    public AudioClip hitVoiceClip = null;
    public AudioClip missVoiceClip = null;
    public AudioClip bgmClip = null;
    #endregion

    #region 레디창 두더지, 이모티콘
    public Texture readyMole_normal_1;
    public Texture readyMole_normal_2;    

    public Texture readyMole_hard_1;
    public Texture readyMole_hard_2;

    public Texture readyEmoticonTexture;
    public Vector3 readyEmoticonPosition_1 = new Vector3(30f, 95f, 0f);
    public Vector3 readyEmoticonPosition_2 = new Vector3(30f, 75f, 0f);
    #endregion

    public GameObject hammerObj;
    public Vector3 hitEfOffset = Vector3.zero;

    #region 튜토리얼 위치
    public Vector3 holeTr = new Vector3(0f, -250f, 0f);
    public Vector3 boardTr = new Vector3(0f, 135f, 0f);
    #endregion
}
