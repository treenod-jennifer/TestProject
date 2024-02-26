using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class UIPopupAtelierReward : UIPopupBase
{
    #region UI
    [SerializeField] private UILabel[]           _labelTitle;
    [SerializeField] private UILabel             _labelContents;
    [SerializeField] private List<GenericReward> _rewards;
    [SerializeField] private UIGrid              _grid;
    [SerializeField] private UISprite            _spritePuzzle;

    public UIPokoButton _btnClose;
    public UIPokoButton _btnYes;
    #endregion

    protected override void OnDestroy()
    {
        _spritePuzzle.atlas = null;
        base.OnDestroy();
    }

    public void Init(List<Reward> rewards, Method.FunctionVoid closeCallback = null)
    {
        _spritePuzzle.atlas = ManagerAtelier.instance._atelierPack.AtlasUI;

        //리워드 설정
        for (var i = 0; i < _rewards.Count; i++)
        {
            if (rewards.Count > i)
            {
                _rewards[i].SetReward(rewards[i]);
            }
            else
            {
                _rewards[i].gameObject.SetActive(false);
            }
        }

        _grid.Reposition();

        //타이틀, 내용 설정
        if (rewards == null || rewards.Count <= 0)
        {
            _labelTitle[0].text = Global._instance.GetString("atelier_bonus_1");
            _labelTitle[1].text = Global._instance.GetString("atelier_bonus_1");
            _labelContents.text = Global._instance.GetString("atelier_bonus_2");
        }
        else
        {
            _labelTitle[0].text = Global._instance.GetString("atelier_bonus_3");
            _labelTitle[1].text = Global._instance.GetString("atelier_bonus_3");
            _labelContents.text = Global._instance.GetString("atelier_bonus_4");
        }

        //닫기, 확인 버튼
        _btnClose.OnClickAsObservable()
            .Where(_ => bCanTouch)
            .ThrottleFirst(TimeSpan.FromSeconds(0.3f))
            .Subscribe(_ => OnClickBtnClose())
            .AddTo(this);
        _btnYes.OnClickAsObservable()
            .Where(_ => bCanTouch)
            .ThrottleFirst(TimeSpan.FromSeconds(0.3f))
            .Subscribe(_ => OnClickBtnClose())
            .AddTo(this);

        _callbackEnd = closeCallback;
    }
}