using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class UIPopupAccountDeleteComplete : UIPopupAccountDeleteBase
{
    #region Readonly
    private static readonly Color32 COLOR_ACTIVE = new Color32(34, 100, 14, 255);
    #endregion

    #region Private
    [SerializeField] private BoxCollider boxColliderCompleteButton;
    [SerializeField] private UISprite    sprCompleteButton;
    [SerializeField] private UILabel[]   labelCompleteButton;
    #endregion
    
    private void OnClickAccountDelete() => OnClickAccountDelete(AccountDelete);

    private void Start() =>
        this.UpdateAsObservable()
            .Delay(TimeSpan.FromSeconds(1f))
            .Subscribe(_ => SetButtonActive())
            .AddTo(this);
    
    private void AccountDelete()
    {
#if UNITY_EDITOR
        Global.ReBoot();
#else
        NetworkLoading.MakeNetworkLoading();

        ServerAPI.AccountDelete((resp) =>
        {
            NetworkLoading.EndNetworkLoading();

            if (resp.IsSuccess)
            {
                // LINE 로그아웃 및 타이틀 화면에서 로그인 페이지 출력
                ServiceSDK.ServiceSDKManager.instance.SignOut();
                ServiceSDK.ServiceSDKManager.isRebootSignIn = true;

                Global.isChangeUserData = true;
                
                var popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("a_d_8"), Global._instance.GetString("a_d_9"), false);
                popup.FunctionSetting(1, "ClosePopUpUI", popup.gameObject, true);
                popup.SetButtonText(1, Global._instance.GetString("btn_93"));
                popup.SortOrderSetting();
                popup._callbackClose += Global.ReBoot;
            }
            else
            {
                var popup = ManagerUI._instance.OpenPopupSystem().GetComponent<UIPopupSystem>();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("a_d_10"), false);
                popup.FunctionSetting(1, "ClosePopUpUI", popup.gameObject, true);
                popup.SetButtonText(1, Global._instance.GetString("btn_1"));
                popup.SortOrderSetting();
                popup._callbackClose += Global.ReBoot;
            }
        });
#endif
    }

    private void SetButtonActive()
    {
        sprCompleteButton.spriteName       = "button_play";
        labelCompleteButton[0].effectColor = COLOR_ACTIVE;
        labelCompleteButton[1].color       = COLOR_ACTIVE;
        labelCompleteButton[1].effectColor = COLOR_ACTIVE;
        boxColliderCompleteButton.enabled  = true;
    }
}