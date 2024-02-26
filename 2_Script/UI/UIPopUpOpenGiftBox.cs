using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using Newtonsoft.Json;

public class UIPopUpOpenGiftBox : UIPopupBase, IImageRequestable
{
    public static UIPopUpOpenGiftBox _instance = null;

    public UIPokoButton _button;
    public UILabel _btnLabel;
    public UILabel _btnLabelShadow;
    public SkeletonAnimation spineFlower;
    //public UIItemGiftBoxReward _itemReward;
    //public ServerUserGiftBox _data = null;

    int textureKey = 0;
    Dictionary<int, Texture> matrialTexture = new Dictionary<int, Texture>();

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
       // _itemReward.gameObject.SetActive(false);
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

    public void OnLoadComplete(ImageRequestableResult r)
    {
        matrialTexture.Add(textureKey, r.texture);
        textureKey = -1;
    }

    public void OnLoadFailed() { }
    public int GetWidth()
    {
        return 0;
    }
    public int GetHeight()
    {
        return 0;
    }
    IEnumerator CoGiftAction()
    {
        ManagerSound._instance.SetTimeBGM(96f);

        yield return null;/*
        for (int i = 0; i < _data.rewardList.Count; i++)
        {
            if ((int)_data.rewardList[i].type > (int)RewardType.material)
            {
                textureKey = i;

                UIImageLoader.Instance.Load(Global.gameImageDirectory, "IconMaterial/", "mt_" + ((int)_data.rewardList[i].type - (int)RewardType.material), this);
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

        AudioClip audioBgm = Resources.Load("Sound/giftbox-BGM") as AudioClip;
        AudioClip audioItem = Resources.Load("Sound/giftbox-item") as AudioClip;
        AudioClip audioOpenEnd = Resources.Load("Sound/giftbox-openEnd") as AudioClip;
        ManagerSound.AudioPlay(audioBgm);

        yield return null;

        yield return new WaitForSeconds(1.05f);


        for (int i = 0; i < _data.rewardList.Count; i++)
        {
            if( i != 0) // 처음 아이템 뱉는 부분은 시작 애니메이션을 사용
                SetPullOutAni();

            yield return new WaitForSeconds(0.15f);

            UIItemGiftBoxReward obj = Instantiate(_itemReward.gameObject).GetComponent<UIItemGiftBoxReward>();
            obj.gameObject.SetActive(true);
            obj._transform.parent = transform;
            obj._transform.localScale = Vector3.one;
            obj._transform.localPosition = new Vector3(0f, 0f, 0f);

            //if ((RewardType)_data.rewardList[i].type == RewardType.clover)
            if ((int)_data.rewardList[i].type > (int)RewardType.material)
            {
                obj._reward.mainTexture = matrialTexture[i];
            }else
                obj._reward.mainTexture = Resources.Load("Message/" + (RewardType)_data.rewardList[i].type) as Texture;
            obj._laberCount.text = _data.rewardList[i].value.ToString();

            if (_data.rewardList.Count == 3)
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
            yield return new WaitForSeconds(0.15f);
        }

        ManagerSound.AudioPlay(audioOpenEnd);

        Global.star = (int)GameData.User.Star;
        Global.clover = (int)(GameData.User.AllClover);
        Global.coin = (int)(GameData.User.AllCoin);
        Global.jewel = (int)(GameData.User.AllJewel);

        if (ManagerUI._instance != null)
            ManagerUI._instance.UpdateUI();

        _button.gameObject.SetActive(true);

        bCanTouch = true;


        for (int i = 0; i < _data.rewardList.Count; i++)
        {
            if ((int)_data.rewardList[i].type > (int)RewardType.material)
            {
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                      "MATERIAL_" + (_data.rewardList[i].type - (int)RewardType.material).ToString(),
                      "material",
                      _data.rewardList[i].value,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PRESENT_BOX
                  );
                var doc = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
            }
            else if ((int)_data.rewardList[i].type == (int)RewardType.stamp)
            {
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.STAMP,
                      "Stamp" + _data.rewardList[i].value,
                      "Stamp" + _data.rewardList[i].value,
                      1,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PRESENT_BOX
                  );
                var doc = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
            }
            else if ((int)_data.rewardList[i].type == (int)RewardType.clover)
            {
                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_PRESENT_BOX,
                0,
                _data.rewardList[i].value,
                0,//(int)(ServerRepos.User.clover),
                (int)(ServerRepos.User.AllClover)//(int)(ServerRepos.User.fclover)//,
                //"q_" + _data.index.ToString()
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if ((int)_data.rewardList[i].type == (int)RewardType.coin)
            {
                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.SC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_PRESENT_BOX,
                0,
                _data.rewardList[i].value,
                (int)(ServerRepos.User.coin),
                (int)(ServerRepos.User.fcoin)//,
                //"q_" + _data.index.ToString()
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if ((int)_data.rewardList[i].type == (int)RewardType.star)
            {
                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_STAR,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_PRESENT_BOX,
                0,
                _data.rewardList[i].value,
                0,
                (int)(ServerRepos.User.fcoin)//,
                //"q_" + _data.index.ToString()
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if ((int)_data.rewardList[i].type == (int)RewardType.jewel)
            {
                var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_PRESENT_BOX,
                0,
                _data.rewardList[i].value,
                (int)(ServerRepos.User.jewel),
                (int)(ServerRepos.User.fjewel)//,
                //"q_" + _data.index.ToString()
                );
                var docMoney = JsonConvert.SerializeObject(growthyMoney);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);
            }
            else if ((int)_data.rewardList[i].type >= (int)RewardType.ingameItem1 && (int)_data.rewardList[i].type <= (int)RewardType.ingameItem5)
            {
                GameItemType tempItemType = (GameItemType)((int)_data.rewardList[i].type - (int)RewardType.ingameItem1  + 1);
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_INGAME,
                        "InGameItem" + ((int)tempItemType).ToString(),
                        "InGameItem" + tempItemType.ToString(),
                        _data.rewardList[i].value,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PRESENT_BOX
                    );
                var doc = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
            }
            else if ((int)_data.rewardList[i].type >= (int)RewardType.readyItem1 && (int)_data.rewardList[i].type <= (int)RewardType.readyItem6)
            {
                READY_ITEM_TYPE tempItemType = (READY_ITEM_TYPE)((int)_data.rewardList[i].type - (int)RewardType.readyItem1);
                var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
                    (
                       ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.CONSUMPTION_LOBBY,
                        "ReadyItem" + ((int)tempItemType).ToString(),
                        "ReadyItem" + tempItemType.ToString(),
                        _data.rewardList[i].value,
                        ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_PRESENT_BOX
                    );
                var doc = JsonConvert.SerializeObject(useReadyItem);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
            }
        }
        */
    }
    void SetSkeleton()
    {
        spineFlower.gameObject.SetActive(true);
        /*
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
        spineFlower.Update(0f);*/
    }

    void SetPullOutAni()
    {/*
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
        spineFlower.Update(0f);*/
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
}
