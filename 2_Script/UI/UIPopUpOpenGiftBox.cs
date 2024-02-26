using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using PokoAddressable;
using Newtonsoft.Json;

public class UIPopUpOpenGiftBox : UIPopupBase
{
    public static UIPopUpOpenGiftBox _instance = null;

    public UIPokoButton _button;
    public UILabel _btnLabel;
    public UILabel _btnLabelShadow;
    public SkeletonAnimation spineFlower;
    public UIItemGiftBoxReward _itemReward;
    public ServerUserGiftBox _data = null;

    [SerializeField] private UIItemBonusRewardBox bonusBox;
    private Reward bonusReward = null;

    private Reward rankTokenReward = null;
    public List<int> gradeRewardList = new List<int>();

    int textureKey = 0;
    Dictionary<int, Texture> matrialTexture = new Dictionary<int, Texture>();

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

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }


      /*  _data = new ServerUserGiftBox();
        _data.rewardList = new List<Reward>();

        {
            Reward data = new Reward();
            data.type = 1;
            data.value = 2;
            _data.rewardList.Add(data);
        }
        {
            Reward data = new Reward();
            data.type = 2;
            data.value = 3;
            _data.rewardList.Add(data);
        }
        {
            Reward data = new Reward();
            data.type = 3;
            data.value = 44;
            _data.rewardList.Add(data);
        }

        {
            Reward data = new Reward();
            data.type = 3;
            data.value = 55;
            _data.rewardList.Add(data);
        }

        {
            Reward data = new Reward();
            data.type = 3;
            data.value = 66;
            _data.rewardList.Add(data);
        }*/
        _button.gameObject.SetActive(false);
        _itemReward.gameObject.SetActive(false);
    }

    public override void OpenPopUp(int depth)
    {
        //터치 관련 막음.
        bCanTouch = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        uiPanel.depth = depth;
        spineFlower.gameObject.SetActive(false);
        StartCoroutine(CoGiftAction());
    }

    public override void SettingSortOrder(int layer)
    {
        if (layer < 10)
            return;
        //전에 팝업들이 sortOrder을 사용하지 않는다면 스파인 레이어만 올려줌.
        if (layer != 10)
        {
            uiPanel.useSortingOrder = true;
            uiPanel.sortingOrder = layer;
            layer += 1;
        }
        spineFlower.GetComponent<MeshRenderer>().sortingOrder = layer;
        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public void SetBonus(Reward reward)
    {
        bonusReward = reward;
    }

    public void SetRankToken(Reward reward)
    {
        rankTokenReward = reward;
    }

    public void OnLoadComplete(Texture2D r)
    {
        matrialTexture.Add(textureKey, r);
        textureKey = -1;
    }

    IEnumerator CoGiftAction()
    {
        ManagerSound._instance.SetTimeBGM(96f);

        yield return null;
        for (int i = 0; i < _data.rewardList.Count; i++)
        {
            if ((int)_data.rewardList[i].type > (int)RewardType.material)
            {
                textureKey = i;

                Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconMaterial/", $"mt_{_data.rewardList[i].type - (int)RewardType.material}", OnLoadComplete);
                while (true)
                {
                    if (textureKey == -1)
                        break;
                    yield return null;
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
        SetSkeleton();

        AudioClip audioBgm = Box.LoadResource<AudioClip>("Sound/giftbox-BGM");
        AudioClip audioItem = Box.LoadResource<AudioClip>("Sound/giftbox-item");
        AudioClip audioOpenEnd = Box.LoadResource<AudioClip>("Sound/giftbox-openEnd");
        ManagerSound.AudioPlay(audioBgm);

        yield return null;

        yield return new WaitForSeconds(1.05f);


        for (int i = 0; i < _data.rewardList.Count; i++)
        {
            if ( i != 0) // 처음 아이템 뱉는 부분은 시작 애니메이션을 사용
                SetPullOutAni();

            yield return new WaitForSeconds(0.15f);

            UIItemGiftBoxReward obj = Instantiate(_itemReward.gameObject).GetComponent<UIItemGiftBoxReward>();
            obj.gameObject.SetActive(true);
            obj._transform.parent = transform;
            obj._transform.localScale = Vector3.one;
            obj._transform.localPosition = new Vector3(0f, 0f, 0f);
            obj.SetRewardPanel(uiPanel.depth, uiPanel.sortingOrder);

            //if ((RewardType)_data.rewardList[i].type == RewardType.clover)
            if ((int)_data.rewardList[i].type > (int)RewardType.material)
            {
                obj._reward.mainTexture = matrialTexture[i];
            }else
                gameObject.AddressableAssetLoad<Texture2D>("local_message/" + (RewardType)_data.rewardList[i].type, (texture) => obj._reward.mainTexture = texture);
            obj._laberCount.text = _data.rewardList[i].value.ToString();
            obj._laberLuckyCount.text = _data.rewardList[i].value.ToString();

            if (_data.rewardList.Count == 1)
                obj._transform.DOLocalMove(new Vector3(0.0f, 350f, 0f), 0.3f).SetEase(Ease.OutSine);
            else if (_data.rewardList.Count == 2)
                obj._transform.DOLocalMove(new Vector3(166f * (float)i - 83f, 350f, 0f), 0.3f).SetEase(Ease.OutSine);
            else if (_data.rewardList.Count == 3)
                obj._transform.DOLocalMove(new Vector3(166f * (float)(i - 1), 350f, 0f), 0.3f).SetEase(Ease.OutSine);
            else if (_data.rewardList.Count == 4)
                obj._transform.DOLocalMove(new Vector3(83f + 166f * (float)(i - 2), 350f, 0f), 0.3f).SetEase(Ease.OutSine);
            else if (_data.rewardList.Count == 5)
            {
                if(i<3)
                    obj._transform.DOLocalMove(new Vector3(166f * (float)(i - 1), 350f, 0f), 0.3f).SetEase(Ease.OutSine);
                else
                    obj._transform.DOLocalMove(new Vector3(83f + 166f * (float)(i - 4),250f, 0f), 0.3f).SetEase(Ease.OutSine);
            }

            DOTween.To(() => 0f, x => obj.SetAlpha(x), 1f, 0.3f);
            ManagerSound.AudioPlay(audioItem);

            if (rankTokenReward != null)
                obj.gradeEffect.SetGradeEffect(gradeRewardList[i]);

            yield return new WaitForSeconds(0.15f);
        }

        if(bonusReward != null && bonusBox != null)
        {
            yield return new WaitForSeconds(0.1f);

            SetPullOutAni();

            yield return new WaitForSeconds(0.15f);

            bonusBox.SetReward(bonusReward);
            bonusBox.gameObject.SetActive(true);
            bonusBox.transform.DOLocalMove(new Vector3(280.0f, 260.0f, 0.0f), 0.3f).SetEase(Ease.OutSine);
            ManagerSound.AudioPlay(audioItem);

            yield return new WaitForSeconds(0.2f);

            yield return bonusBox.CoGetReward(BonusOpenEvent);
        }

        if (rankTokenReward != null)
        {
            yield return new WaitForSeconds(0.1f);
            
            SetPullOutAni();

            yield return new WaitForSeconds(0.15f);

            UIItemGiftBoxReward obj = Instantiate(_itemReward.gameObject).GetComponent<UIItemGiftBoxReward>();
            obj.gameObject.SetActive(true);
            obj._transform.parent = transform;
            obj._transform.localScale = Vector3.one;
            obj._transform.localPosition = new Vector3(0f, 0f, 0f);
            obj.SetRewardPanel(uiPanel.depth, uiPanel.sortingOrder);

            obj.gradeEffect.SetGradeEffect(1);
            gameObject.AddressableAssetLoad<Texture2D>("local_message/" + (RewardType)rankTokenReward.type, (texture) => obj._reward.mainTexture = texture);
            obj._laberCount.text = rankTokenReward.value.ToString();

            DOTween.To(() => 0f, x => obj.SetAlpha(x), 1f, 0.3f);

            obj.transform.DOLocalMove(new Vector3(0.0f, 220.0f, 0.0f), 0.3f).SetEase(Ease.OutSine);

            ManagerSound.AudioPlay(audioItem);

            yield return new WaitForSeconds(0.2f);
        }

        ManagerSound.AudioPlay(audioOpenEnd);

        Global.star = (int)GameData.User.Star;
        Global.clover = (int)(GameData.User.AllClover);
        Global.coin = (int)(GameData.User.AllCoin);
        Global.jewel = (int)(GameData.User.AllJewel);

        //다이어리 쪽 하우징 데이터 갱신.
        UIDiaryController._instance.UpdateProgressHousingData();

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        _button.gameObject.SetActive(true);

        bCanTouch = true;

        SendGrowthyLog_Item();
    }

    private IEnumerator BonusOpenEvent()
    {
        AudioClip audioItem = Box.LoadResource<AudioClip>("Sound/giftbox-item");
        ManagerSound.AudioPlay(audioItem);
        yield return null;
    }

    void SetSkeleton()
    {
        spineFlower.gameObject.SetActive(true);

        string boxSkinName = "box1";
        switch( _data.type )
        {
            case 5: boxSkinName = "box6"; break;
            case 4: boxSkinName = "box5"; break;
            case 3: boxSkinName = "box4"; break;
            case 2: boxSkinName = "box3"; break;
            case 1: boxSkinName = "box2"; break;
            default: boxSkinName = "box1"; break;
        }

        // 작업전 예외상황 회피
        if (spineFlower.skeleton.Data.FindSkin(boxSkinName) == null)
        {
            boxSkinName = "box1";
        }

        string boxAniName = boxSkinName + "_loop";

        spineFlower.skeleton.SetSkin(boxSkinName);
        spineFlower.state.SetAnimation(0, "appear", false);
        spineFlower.state.AddAnimation(0, boxAniName, true, 0f);
        spineFlower.Update(0f);
    }

    void SetPullOutAni()
    {
        string boxSkinName = "box1";
        switch (_data.type)
        {
            case 5: boxSkinName = "box6"; break;
            case 4: boxSkinName = "box5"; break;
            case 3: boxSkinName = "box4"; break;
            case 2: boxSkinName = "box3"; break;
            case 1: boxSkinName = "box2"; break;
            default: boxSkinName = "box1"; break;
        }

        string boxAniName = boxSkinName + "_item";

        string boxLoopAniName = boxSkinName + "_loop";

        spineFlower.state.SetAnimation(0, boxAniName, false);
        spineFlower.state.AddAnimation(0, boxLoopAniName, true, 0f);
        spineFlower.Update(0f);
    }

    public override void ClosePopUp(float _mainTime = 0.3f, Method.FunctionVoid callback = null)
    {
        ManagerUI._instance.bTouchTopUI = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        _callbackEnd += callback;

        DOTween.To(() => uiPanel.alpha, x => uiPanel.alpha = x, 0f, _mainTime).SetEase(ManagerUI._instance.popupAlphaAnimation);
        StartCoroutine(CoAction(0.15f, () => { spineFlower.gameObject.SetActive(false); }));
 
        //연출 끝난 후 해당 팝업 삭제.
        StartCoroutine(CoAction(_mainTime + 0.15f, () =>
        {
            Destroy(gameObject);
        }));
    }

    private void SendGrowthyLog_Item()
    {
        foreach(var reward in _data.rewardList)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
            (
                rewardType: reward.type,
                rewardCount: reward.value,
                moneyMRSN: MoneyMRSN,
                itemRSN: ItemRSN,
                QuestName: RewardRsnDtl
            );
        }

        if(bonusReward != null)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
            (
                rewardType: bonusReward.type,
                rewardCount: bonusReward.value,
                moneyMRSN: MoneyMRSN,
                itemRSN: ItemRSN,
                QuestName: RewardRsnDtl
            );
        }

        if (rankTokenReward != null)
        {
            ServiceSDK.GrowthyCusmtomLogHelper.SendGrowthyLog
            (
                rewardType: rankTokenReward.type,
                rewardCount: rankTokenReward.value,
                moneyMRSN: MoneyMRSN,
                itemRSN: ItemRSN,
                QuestName: RewardRsnDtl
            );
        }
    }

    private ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN MoneyMRSN
    {
        get
        {
            switch (_data.type)
            {
                case 5: return ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_WORLDRANK_BONUS;
                default: return ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_PRESENT_BOX;
            }
        }
    }

    private ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN ItemRSN
    {
        get
        {
            switch (_data.type)
            {
                case 3: return ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_BOMB_BOX;
                case 4: return ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_MATERIAL_BOX;
                case 5: return ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_WORLDRANK_BONUS;
                default: return ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PRESENT_BOX;
            }
        }
    }

    private string RewardRsnDtl
    {
        get
        {
            switch (_data.type)
            {
                case 5: return $"WR_{ServerContents.WorldRank.eventIndex}";
                default: return null;
            }
        }
    }
}
