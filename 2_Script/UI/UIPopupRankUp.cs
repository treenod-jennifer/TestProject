using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Newtonsoft.Json;
using PokoAddressable;

public class UIPopupRankUp : UIPopupBase
{
    public static UIPopupRankUp _instance = null;
    public GameObject   anchorUp;

    public GameObject   rankUpButton;
    public GameObject   lightObject;
    public GameObject   rewardObject;
    public UITexture    light1;
    public UITexture    light2;
    public UITexture    pokogoro1;
    public UITexture    pokogoro2;
    public UILabel      rankupTitle;
    public UILabel      rankupLastTitle;
    public UILabel      rankupMessage;
    //추가 메세지 입력할 때 사용(ex. 포코유라 획득 시).
    public UILabel      textPlus;
    public UITexture pokoyura;

    public GameObject _objParticlePaper;

    [SerializeField] private GameObject objNomalRoot;
    [SerializeField] private GameObject objLastRoot;
    [SerializeField] private TweenAlpha alpha;

    GameObject particle;

    private const int LastRankUpNum = 252;
    private bool bClose;
    int _pokoyura = 0;
    int _classNum;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if(box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    void Awake()
    {
        _instance = this;
    }
    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        this.bCanTouch = false;
    }

    private enum RankUpState
    {
        NomalRankUp,
        LastRankUp,
        PostLastRankUp
    }

    private RankUpState GetRankUpState()
    {
        if (_classNum < LastRankUpNum)
            return RankUpState.NomalRankUp;
        else if ((int)ServerRepos.User.flower > (LastRankUpNum - 1) * 10)
            return RankUpState.PostLastRankUp;
        else
            return RankUpState.LastRankUp;
    }

    private UILabel GetCurrentUILabel()
    {
        if (state == RankUpState.NomalRankUp)
            return rankupTitle;
        else
            return rankupLastTitle;
    }

    private RankUpState state;
    public void InitRankUpPopup(int classNum, int pokoyura)
    {
        bClose = false;
        _pokoyura = pokoyura;
        _classNum = classNum;

        state = GetRankUpState();

        switch (state)
        {
            case RankUpState.NomalRankUp:
                {
                    NomalAction();
                    rankupMessage.text = Global._instance.GetString("p_rk_u_2").Replace("[n]", Global.flower.ToString());
                    break;
                }
            case RankUpState.LastRankUp:
                {
                    NomalAction();
                    rankupMessage.text = Global._instance.GetString("p_rk_u_6");
                    break;
                }    
            case RankUpState.PostLastRankUp:
                {
                    SetParticle();

                    GetCurrentUILabel().gameObject.SetActive(true);

                    Global.flower = (int)GameData.User.flower;

                    rankupMessage.text = Global._instance.GetString("p_rk_u_2").Replace("[n]", Global.flower.ToString());

                    objLastRoot.SetActive(true);

                    alpha.enabled = true;
                    ManagerSound.AudioPlay(AudioLobby.Mission_Finish);
                    break;
                }
        }

        //그로씨
        var growthyMoney = new ServiceSDK.GrowthyCustomLog_Money
                (
                ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.FC,
                ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_LEVEL_UP_REWARD,
                0,
                2,
                (int)(ServerRepos.User.jewel),
                (int)(ServerRepos.User.fjewel),
                _classNum.ToString()
                );
        var docMoney = JsonConvert.SerializeObject(growthyMoney);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", docMoney);

        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
          (
             ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.DECO,
              "POKOYURA_" + _pokoyura,
              "POKOYURA_" + _pokoyura,
              1,
              ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_LEVEL_UP_REWARD
          );
        var doc = JsonConvert.SerializeObject(useReadyItem);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);

        if( state == RankUpState.LastRankUp)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                        (
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.STAGE,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.LAST_FLOWER_RANK,
                           "LAST_FLOWER_RANK",
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                        );
            var docLastRank = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", docLastRank);
        }


    }

    void NomalAction()
    {
        if (GetCurrentUILabel() == null) return;

        _callbackEnd += SetClose;

        objNomalRoot.SetActive(true);
        rankUpButton.SetActive(false);
        lightObject.SetActive(false);
        rewardObject.SetActive(false);
        textPlus.gameObject.SetActive(false);
        GetCurrentUILabel().transform.localScale = Vector3.one * 1.5f;

        pokogoro1.color = new Color(1f, 1f, 1f, 0f);

        pokogoro2.transform.localPosition = new Vector3(0f, 0f, 0f);
        pokogoro2.transform.localScale = Vector3.one * 0.7f;
        pokogoro2.color = new Color(1f, 1f, 1f, 0f);

        light1.color = new Color(1f, 1f, 1f, 0f);
        light2.color = new Color(1f, 1f, 1f, 0f); ;

        GetCurrentUILabel().color = new Color(1f, 1f, 1f, 0f);
        rankupMessage.color = new Color(1f, 1f, 1f, 0f);

        if (_pokoyura > 0)
        {
            Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Pokoyura", $"y_i_{pokoyura}.png", OnLoadComplete);
        }

        Global.flower = (int)GameData.User.flower;
        
        gameObject.AddressableAssetLoadClass<Texture>(_classNum < 2 ? _classNum : _classNum - 1, texture =>
        {
            pokogoro1.mainTexture = texture;
        });

        gameObject.AddressableAssetLoadClass<Texture>(_classNum, texture =>
        {
            pokogoro2.mainTexture = texture;
        });
        
        StartCoroutine(CoRankUpAction());
    }

    public void OnLoadComplete(Texture2D r)
    {
        pokoyura.mainTexture = r;
    }

    IEnumerator CoRankUpAction()
    {
        yield return new WaitForSeconds(0.3f);
        DOTween.ToAlpha(() => pokogoro1.color, x => pokogoro1.color = x, 1f, 0.5f);

        ManagerSound.AudioPlay(AudioLobby.Mission_Finish);

        yield return new WaitForSeconds(0.5f);
        pokogoro2.transform.DOLocalMoveY(55f, 0.5f).SetEase(Ease.InQuart);
        pokogoro2.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetEase(Ease.InQuart);
        DOTween.ToAlpha(() => pokogoro2.color, x => pokogoro2.color = x, 1f, 0.3f);
        yield return new WaitForSeconds(0.5f);

        pokogoro1.transform.DOLocalMove(new Vector3(-400f, 400f, 0f), 0.7f).SetEase(Ease.OutQuart);
        pokogoro1.transform.DOScale(Vector3.one * 0.5f, 0.7f).SetEase(Ease.OutSine);
        DOTween.ToAlpha(() => pokogoro1.color, x => pokogoro1.color = x, 0f, 0.7f).SetEase(Ease.OutSine);

        GetCurrentUILabel().gameObject.SetActive(true);
        GetCurrentUILabel().transform.DOScale(1f, 0.5f).SetEase(Ease.InQuint);
        DOTween.ToAlpha(() => GetCurrentUILabel().color, x => GetCurrentUILabel().color = x, 1f, 0.3f).SetEase(Ease.InQuint);
        DOTween.ToAlpha(() => rankupMessage.color, x => rankupMessage.color = x, 1f, 0.3f).SetEase(Ease.InQuint);

        pokogoro2.transform.DOShakePosition(1.0f, 5f, 20, 90f, false, false);
        lightObject.SetActive(true);
        lightObject.transform.DOScale(1.5f, 0.5f);
        DOTween.ToAlpha(() => light1.color, x => light1.color = x, 1f, 0.5f);
        DOTween.ToAlpha(() => light2.color, x => light2.color = x, 1f, 0.5f);

        //최종랭크일 때만 꽃가루 연출
        if (state == RankUpState.LastRankUp)
        {
            SetParticle();
        }

        yield return new WaitForSeconds(0.5f);
        GetCurrentUILabel().transform.DOShakePosition(0.3f, 10f, 20, 90f, false, true);
        StartCoroutine(CoRotate());

        yield return new WaitForSeconds(0.3f);
        rankUpButton.SetActive(true);
        rewardObject.SetActive(true);

        if (_pokoyura > 0 && state == RankUpState.NomalRankUp)
        {
            pokoyura.gameObject.SetActive(true);

            ManagerSound.AudioPlay(AudioInGame.CONTINUE);

            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("p_rk_u_4"), false);
            popupSystem.SetImage((Texture2D)pokoyura.mainTexture);
            popupSystem.SortOrderSetting(true,4);
        }
        
        if (state == RankUpState.LastRankUp)
        {
            UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
            popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_s_52"), false);
            popupSystem.SetResourceImage("Message/coin");
            popupSystem.SortOrderSetting(true, 4);
        }

        this.bCanTouch = true;
    }

    IEnumerator CoRotate()
    {
        Vector3 pos = pokogoro2.transform.localPosition;
        
        while (bClose == false)
        {
            light1.transform.Rotate(Vector3.right, 2.0f * Global.deltaTime, Space.World);
            light2.transform.Rotate(Vector3.right, 5.0f * Global.deltaTime, Space.World);
            pokogoro2.transform.localPosition = new Vector3(pos.x, pos.y + (Mathf.Sin(Time.time * 5f) * 8f), pos.z);
            yield return null;
        }
        yield return null;
    }

    void SetParticle()
    {
        particle = NGUITools.AddChild(anchorUp, _objParticlePaper);

        particle.transform.localPosition
            = new Vector3(particle.transform.localPosition.x, particle.transform.localPosition.y + 100f, particle.transform.localPosition.z);
    }

    void SetClose()
    {
        _instance = null;
        bClose = true;
        if(particle !=null)
        {
            particle.GetComponentInChildren<ParticleSystem>().Stop();
        }
    }

    public override void ClosePopUp(float _mainTime = 0.3F, Method.FunctionVoid callback = null)
    {
        ManagerUI._instance.bTouchTopUI = false;
        ManagerSound.AudioPlay(AudioLobby.PopUp);
        _callbackEnd += callback;
     
        DOTween.To(() => uiPanel.alpha, x => uiPanel.alpha = x, 0f, _mainTime).SetEase(ManagerUI._instance.popupAlphaAnimation);

        //연출 끝난 후 해당 팝업 삭제.
        StartCoroutine(CoAction(_mainTime + 0.15f, () =>
        {
            Destroy(gameObject);
        }));
    }
}
