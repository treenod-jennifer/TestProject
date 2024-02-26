using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Obsolete("이거 안씁니다")]
public class ObjectRewardIcon : ObjectIcon
{
    public AnimationCurve _curveShow;
    public AnimationCurve _curveTouch;

    public Transform _transformSprite;

    public GameObject defaultRoot;

    public GameObject bg;
    public GameObject disappearingRoot;
    public RawImage _uiDisapperingIcon;

    public Text[] _uiCount;
    public RawImage _uiIcon;

    bool _canClick = false;

    int animalIdx;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    public static List<ObjectRewardIcon> _iconList = new List<ObjectRewardIcon>();
    void Awake()
    {
        base.Awake();
        _iconList.Add(this);
    }

    void OnDestroy()
    {
        _iconList.Remove(this);
    }

    public static void Clear()
    {
        for(int i = 0; i < _iconList.Count; ++i)
        {
            Destroy(_iconList[i].gameObject);
        }
        _iconList.Clear();

    }

    static public void RemoveRewardIcon(int animalIdx)
    {
        if (animalIdx == -1)
        {
            for (int i = 0; i < _iconList.Count; i++)
                _iconList[i].CloseIcon();
            return;
        }

        for (int i = 0; i < _iconList.Count; i++)
        {
            if (_iconList[i].animalIdx == animalIdx)
            {
                _iconList[i].CloseIcon();
                break;
            }
        }
    }

    override public void OnTap()
    {
        if (_canClick == false)
            return;
        _canClick = false;

        ManagerSound.AudioPlay(AudioLobby.Mission_ButtonClick);
        StartCoroutine(DoTouchObjectAnimation());
    }
	// Update is called once per frame
	void Update () {

        if (defaultRoot.activeInHierarchy)
            _transformSprite.rotation = Quaternion.Euler(50f, -45f, Mathf.Sin(Time.time * 5f) * 8f);
    }

    public void InitLobbyMission(int hostAnimalIdx, Reward reward, Vector3 in_position)
    {
        this.animalIdx = hostAnimalIdx;

        _uiIcon.gameObject.SetActive(true);
        var box = ResourceBox.Make(_uiIcon.gameObject);
        _uiIcon.texture = box.LoadResource<Texture2D>(RewardHelper.GetRewardTextureResourcePath((RewardType)reward.type));
        for (int i = 0; i < _uiCount.Length; ++i)
            _uiCount[i].text = "+" + reward.value.ToString();

        _uiDisapperingIcon.texture = _uiIcon.texture;

        _transform.position = in_position;
        
        StartCoroutine(CoOpenIcon());
    }

    public void CloseIcon()
    {
        StartCoroutine(CoCloseIcon());
    }

    private IEnumerator CoOpenIcon()
    {
        float animationTimer = 0f;
        float ratio;
        while (animationTimer < 1f)
        {
            ratio = _curveShow.Evaluate(animationTimer);
            _transformSprite.localScale = Vector3.one * ratio;
            animationTimer += Time.deltaTime * 2.5f;
            yield return null;
        }
        _transformSprite.localScale = Vector3.one;
        _canClick = true;
    }
    private IEnumerator CoCloseIcon()
    {
        _canClick = false;
        _transformSprite.rotation = Quaternion.Euler(50f, -45f, 7f);

        this.defaultRoot.SetActive(false);
        this.disappearingRoot.SetActive(true);

        float animationTimer = 1f;
        float ratio;
        while (animationTimer > 0f)
        {
            //ratio = _curveShow.Evaluate(animationTimer);
            //_transformSprite.localScale = Vector3.one * ratio;
            animationTimer -= Time.deltaTime * 1.5f;
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator DoTouchObjectAnimation(bool bEndOpenPopup = true)
    {
        float animationTimer = 0f;
        float ratio;
        bool sendMessage = false;

        while (animationTimer < 1f)
        {
            ratio = _curveTouch.Evaluate(animationTimer);
            _transformSprite.localScale = Vector3.one * ratio;
            animationTimer += Time.deltaTime * 2.5f;

            yield return null;
        }

        bool retReceived = false;
        ServerAPI.AdventureGetLobbyAnimalReward(animalIdx,
                       (resp) =>
                       {
                           retReceived = true;
                           if (resp.IsSuccess)
                           {
                               ManagerAIAnimal.instance.OnReceivedLobbyAnimalReward(animalIdx);

                               Global.clover = (int)GameData.Asset.AllClover;
                               Global.coin = (int)GameData.Asset.AllCoin;
                               Global.jewel = (int)GameData.Asset.AllJewel;
                               Global.wing = (int)GameData.Asset.AllWing;
                               Global.exp = (int)GameData.User.expBall;
                               ManagerUI._instance.UpdateUI();

                               if (resp.rewards != null)
                               {
                                   for (int i = 0; i < resp.rewards.Count; ++i)
                                   {
                                       ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog(
                                           rewardType: resp.rewards[i].type,
                                           rewardCount: resp.rewards[i].value,
                                           moneyMRSN: ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LOBBY_ANIMAL_GIFT,
                                           itemRSN: ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LOBBY_ANIMAL_GIFT,
                                           QuestName: $"Animal_{animalIdx}"
                                           );
                                   }
                               }
                           }
                       });

        while(retReceived == false)
        {
            yield return null;
        }

        CloseIcon();

        _transformSprite.localScale = Vector3.one;
        //_canClick = true;
        yield return null;
    }

    void IconImageLoad(string path, string fileName)
    {
        if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(fileName))
        {
            Box.LoadCDN<Texture2D>(Global.gameImageDirectory, path, fileName, OnLoadComplete);
        }
        else
        {
            OnLoadComplete(null);
        }
    }

    public void OnLoadComplete(Texture2D r)
    {
        _uiIcon.gameObject.SetActive(true);
        r.wrapMode = TextureWrapMode.Clamp;
        _uiIcon.texture = r;
        //_meshRenderer.material.mainTexture = texture;
    }
}
