using System.Collections;
using UnityEngine;

public class FlyTarget : MonoBehaviour {
    [System.NonSerialized]
    public Transform _transform;
    public UISprite _UISprite;
    public UISprite _UISpriteShadow;

    [System.NonSerialized]
    public Vector3 startPos;

    [System.NonSerialized]
    public Vector3 endPos;

    public ParticleSystem _ringEffect;
    public ParticleSystem _particleEffect;

    float _timer = 0f;

    public TARGET_TYPE targetType;
    public BlockColorType targetColor = BlockColorType.NONE;

    public static int flyTargetCount = 0;

    public AnimationCurve CurveScale;
    public AnimationCurve CurvePos;

    public float waitTime = 0f;

    void Awake()
    {
        //flyTargetCount++;
        _transform = transform;
       // _UISprite = GetComponent<UISprite>();
    }

    public void SetSprite(BlockColorType imageColor)
    {
        string spriteName = GetTargetSpriteName(targetType);
        switch (imageColor)
        {
            case BlockColorType.A:
                spriteName = string.Format(spriteName + "{0}", "Rabbit");
                break;
            case BlockColorType.B:
                spriteName = string.Format(spriteName + "{0}", "Bear");
                break;
            case BlockColorType.C:
                spriteName = string.Format(spriteName + "{0}", "Kangaroo");
                break;
            case BlockColorType.D:
                spriteName = string.Format(spriteName + "{0}", "Tiger");
                break;
            case BlockColorType.E:
                spriteName = string.Format(spriteName + "{0}", "Wolf");
                break;
        }
        _UISprite.spriteName = spriteName;
        _UISprite.MakePixelPerfect();
        _UISprite.color = new Color(1, 1, 1, 0.9f);

        _UISpriteShadow.spriteName = _UISprite.spriteName;
        _UISpriteShadow.color = new Color(0, 0, 0, 0.1f);
        _UISpriteShadow.MakePixelPerfect();
        _UISprite.cachedTransform.localScale = Vector3.one * 1.5f;

        //ManagerUIAtlas.CheckAndApplyEventAtlas(_UISprite);
    }

    private string GetTargetSpriteName(TARGET_TYPE targetType)
    {
        switch (targetType)
        {
            case TARGET_TYPE.KEY:
                return "blockKey";
            case TARGET_TYPE.STATUE:
                return "FlyStatue";
            case TARGET_TYPE.SHOVEL:
                return "blockBox";
            case TARGET_TYPE.JEWEL:
                return "blockGround_Jewel";
            case TARGET_TYPE.FLOWER_POT:
                return "Fly_FLOWERPOT";
            case TARGET_TYPE.PEA:
                return "Fly_PEA";
            case TARGET_TYPE.COLORBLOCK:
                return "block";
            case TARGET_TYPE.FLOWER_INK:
                return "Fly_FLOWER_INK";
            case TARGET_TYPE.SPACESHIP:
                return "Fly_SPACESHIP";
            case TARGET_TYPE.HEART:
                return "Fly_HEART";
            case TARGET_TYPE.BREAD_1:
                return "Fly_BREAD_1";
            case TARGET_TYPE.BREAD_2:
                return "Fly_BREAD_2";
            case TARGET_TYPE.BREAD_3:
                return "Fly_BREAD_3";
        }
        return "";
    }

    //
    float speedRatio = -0.8f;
    float velocity = 0.08f;
    float timeRatio = 1.5f;

    public void InitFlyTarget(Vector3 startPos, Vector3 endPos, TARGET_TYPE targetType, params BlockColorType[] colorTypes)
    {
        //날아가는 이미지의 컬러타입과 실제 모이는 컬러타입이 다른 경우(ex.작은 화단)가 있어 2개 타입을 구분해줌.
        BlockColorType targetColor = (colorTypes.Length > 0) ? colorTypes[0] : BlockColorType.NONE;
        BlockColorType ImageColor = (colorTypes.Length > 1) ? colorTypes[1] : targetColor;

        this.endPos = endPos;
        this.startPos = startPos;
        this.targetType = targetType;
        this.targetColor = targetColor;
        SetSprite(ImageColor);
        StartCoroutine(CoInitFlyTarget());
    }

    IEnumerator CoInitFlyTarget()
    {
        flyTargetCount++;
        _transform.position = startPos;
        if (GameManager.gameMode == GameMode.COIN)
        {
            timeRatio = 6f;
            velocity = 0.13f;
        }
        yield return null;
        
        while (waitTime > 0f)
        {
            waitTime -= Global.deltaTimePuzzle;
            yield return null;
        }        

        while (true)
        {
            _timer += Global.deltaTimePuzzle * timeRatio;

            if (Vector3.Distance(_transform.position , endPos) < 0.001f)
                break;

            speedRatio += velocity;
            _transform.position = Vector3.MoveTowards(_transform.position, endPos, speedRatio * Global.deltaTimePuzzle);

            if(speedRatio < 0f)
                _transform.position += Vector3.right * 0.5f* speedRatio * Global.deltaTimePuzzle;

            if (speedRatio > 1 && _particleEffect.gameObject.activeSelf == false)
                _particleEffect.gameObject.SetActive(true);
            yield return null;
        }
        ManagerSound.AudioPlayMany(AudioInGame.GET_TARGET);

        GameUIManager.instance.RefreshTarget(targetType, targetColor);
        ManagerBlock.instance.CheckFeverBlock(targetColor);
        flyTargetCount--;       

        _particleEffect.Stop();
        _ringEffect.gameObject.SetActive(true);
        _UISprite.enabled = false;
        _UISpriteShadow.enabled = false;

        while (true)
        {
          if (!_particleEffect.IsAlive() && !_ringEffect.IsAlive())
                break;
            yield return null;
        }
        InGameEffectMaker.instance.RemoveEffect(gameObject);
    }
}
