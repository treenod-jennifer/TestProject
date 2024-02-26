using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTitleBtnContainer : MonoBehaviour 
{
    public enum BtnContainerState
    {
        None,               //버튼없음
        Start,              //스타트 버튼
        LineAndGuest        //라인 로그인과 게스트 로그인 버튼 2개
    }

    //----------------------------------------------------------------------------
    public GameObject loginBtnContainer;
    public GameObject startBtnContainer;

    public UITexture _btnStart;
    public UITexture _btnGuest;
    public UITexture _imgIconLine;

    public System.Action<bool> callbackHandler;

    public Texture imgAOSBtnLogin, imgAOSBtnGuestLogin;
    public Texture imgIOSBtnLogin, imgIOSBtnGuestLogin;
    public Texture imgAOSIconLine, imgIOSIconLine;

    [SerializeField]
    private GameObject _lineLoginButton;
    [SerializeField]
    private GameObject _guestLoginButton;

    //----------------------------------------------------------------------------
    private BtnContainerState _state = BtnContainerState.None;


    //----------------------------------------------------------------------------
    public void InitData ( BtnContainerState state, System.Action<bool> callbackHandler )
    {
        this.callbackHandler = callbackHandler;
        
        this._state = state;

        if ( this._state == BtnContainerState.Start )
        {
            this.startBtnContainer.SetActive( true );
            this.loginBtnContainer.SetActive( false );
        }
        else if ( this._state == BtnContainerState.None )
        {
            this.startBtnContainer.SetActive( false );
            this.loginBtnContainer.SetActive( false );
        }
        else
        {
            UIWidget loginWidget = this._btnStart.GetComponent<UIWidget>();
            UIWidget guestWidget = this._btnGuest.GetComponent<UIWidget>();

#if UNITY_ANDROID && !UNITY_EDITOR
            this._btnStart.mainTexture = imgAOSBtnLogin;
            loginWidget.width = 480;
            loginWidget.height = 88;
            this._btnStart.border =  new Vector4(89.86f, 18f, 17.75f, 18f);

            this._btnGuest.mainTexture = imgAOSBtnGuestLogin;
            guestWidget.width = 320;
            guestWidget.height = 76;
            this._btnGuest.border = new Vector4( 15f, 15f, 15f, 15f );

            this._imgIconLine.mainTexture = imgAOSIconLine;
            this._imgIconLine.MakePixelPerfect();
            this._imgIconLine.transform.localPosition = new Vector3( -196f, 0f, 0f );

#elif UNITY_IOS && !UNITY_EDITOR
            this._btnStart.mainTexture = this.imgIOSBtnLogin;
            loginWidget.width = 480;
            loginWidget.height = 88;
            this._btnStart.border = new Vector4( 91f, 21f, 14f, 21f );

            this._btnGuest.mainTexture = this.imgIOSBtnGuestLogin;
            guestWidget.width = 320;
            guestWidget.height = 76;
            this._btnGuest.border = new Vector4( 18f, 17f, 18f, 17f );

            this._imgIconLine.mainTexture = this.imgIOSIconLine;
            this._imgIconLine.MakePixelPerfect();
            this._imgIconLine.transform.localPosition = new Vector3( -196f, 0f, 0f );
#endif
            this.loginBtnContainer.SetActive(true);
            this.startBtnContainer.SetActive(false);
            this._guestLoginButton.SetActive( true );
        }

    }

    private void OnClickLogin ()
    {
        Debug.Log( "===================== ButtonClickLogin =====================" );

       // DelegateHelper.SafeCall<bool>( this.callbackHandler, false );
    }

    private void OnClickGuestLogin ()
    {
        Debug.Log( "===================== ButtonClickGuestLogin =====================" );

       // DelegateHelper.SafeCall<bool>( this.callbackHandler, true );
    }

    private void OnClickStart ()
    {
        Debug.Log( "===================== ButtonClickStart =====================" );

       // DelegateHelper.SafeCall<bool>(this.callbackHandler, ServiceSDK.ServiceSDKManager.instance.IsGuestLogin());
    }

    //----------------------------------------------------------------------------

}
