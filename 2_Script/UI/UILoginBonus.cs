using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class UILoginBonus : MonoBehaviour
{
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _jpStringData = null;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] public TextAsset _exStringData = null;

    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _jpFileName;
    [Tooltip("4.4부터 사용하지 않습니다. textFileName 필드를 사용해 주세요.")] [System.Obsolete()] [SerializeField] private string _exFileName;

    public string textFileName;
    
    public Texture stepOnTexture;
    public Texture stepOffTexture;
    public Texture stepLineOnTexture;
    public Texture stepLineOffTexture;

    public SkeletonAnimation spineCharacter;
    public string appearAniname = "1_appear";
    public string idleAniname = "1_idle ";
    public string questionSelectAniname = "4_appear";
    public string resultAppearAniname = "2_appear";
    public string resultIdleAniname = "2_idle";
    public string rewardIdleAniname = "3_idle";

    public float questionSelectDelay = 5.0f;

    public Color stepOffColor = new Color(76f, 35f, 129f, 255f);
    public Color stepOnColor = new Color(197f, 91f, 36f, 255f);

    public int stepOnFontSize = 40;

    public Vector2 stepOffSize = new Vector2(86, 83);
    public Vector2 stepOnSize = new Vector2(96, 93);
    
    public UITexture title;
    public UITexture mainStar;
    public UITexture blind;
    public UITexture[] titleAlphaObj;

    //question
    public GameObject[] questionRoot;

    //question
    public GameObject[] answerRoot;
    public UITexture answerBox;

    //bonus
    public GameObject[] bonusRoot;
    public Vector3 offsetReward = Vector3.zero;
    public UITexture actionStep;
    public List<UITexture> step = new List<UITexture>();
    public List<UITexture> stepLine = new List<UITexture>();
    public List<GameObject> loginRewardBubble = new List<GameObject>();
    public List<GameObject> loginRewardRoot = new List<GameObject>();

    //파티클.
    public float magicEffectSize = 700f;
    public Transform magicEffectRoot;
    public GameObject _objParticleMagicCrystal;

    //loginData
    public int[] textListCount = new int[2];
    public List<LoginDialogData> dialogData;

    public UIPanel topPanel;
}
