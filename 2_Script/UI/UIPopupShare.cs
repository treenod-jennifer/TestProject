using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Newtonsoft.Json;

public class UIPopupShare : UIPopupBase
{
    public static UIPopupShare _instance = null;

    public GameObject firstShareBox;
    public GameObject diaBubble;

    public UILabel[] title;
    public UILabel[] button1;
    public UILabel[] button2;
    public UILabel[] getMessage;
    public UILabel mainMessage;
    public UILabel warningMessage;

  //  private UIItemStamp itemStamp;
    private bool isSetupData = false;
    public static int _index = 0;
  //  public static UIItemStampEdit _itemStamp = null;

    void Awake()
    {
        _instance = this;
    }
    new void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
    }
    /*
    public void InitPopUp ( UIItemStamp itemStamp, bool isSetupData )
    {
        this.itemStamp = itemStamp;
        this.isSetupData = isSetupData;

        // 처음 공유할 경우, 첫 공유 이미지 띄움.
        {
            firstShareBox.SetActive(true);
            diaBubble.SetActive(true);
            string warningText = Global._instance.GetString("p_sh_3");
            string getText = Global._instance.GetString("btn_15");
            warningMessage.text = warningText;
            getMessage[0].text = getText;
            getMessage[1].text = getText;
        }
        //아닐 경우, 이미지 안보이게.
        //{
        //    diaBubble.SetActive(false);
        //    firstShareBox.setActive(false);
        //}

        string titleText = Global._instance.GetString("p_sh_1");
        string btnText1 = Global._instance.GetString("btn_20");
        string btnText2 = Global._instance.GetString("btn_21");
        string mainText = Global._instance.GetString("p_sh_2");

        title[0].text = titleText;
        title[1].text = titleText;
        button1[0].text = btnText1;
        button1[1].text = btnText1;
        button2[0].text = btnText2;
        button2[1].text = btnText2;
        mainMessage.text = mainText;


    }
    */
    private void OnClickBtnGallery ()
    {
        if ( working )
            return;

        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
        popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_st_3" ), false);
        popup.FunctionSetting(1, "SaveImage", this.gameObject, true);
    }
    bool working = false;

    private void OnClickBtnTimeLine ()
    {
        if (working)
            return;

        if (UIPopupSystem._instance != null) return;
            

        /*if ( ServiceSDK.ServiceSDKManager.instance.IsGuestLogin() )
        {
            ManagerUI._instance.GuestLoginSignInCheck();
        }
        else*/
        {
            UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem( false ).GetComponent<UIPopupSystem>();
            popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_st_4" ), false);
            popup.FunctionSetting( 1, "UploadTimeLine", this.gameObject, true );
        }
    }

    private void OnClickBtnClose ()
    {
        if(working)
           return;

        ManagerUI._instance.ClosePopUpUI();
    }

    private void SaveImage ()
    {/*
        // 스크린 캡처 시작
        if ( ManagerScreenCapture._instance == null )
        {
            GameObject obj = Instantiate( UIDiaryStamp._instance._objScreenCaptrue );
            obj.name = "ManagerScreenCapture";
            obj.transform.position = new Vector3( 2f, -3f, 0f );
        }

        // 스크린 캡처 저장
        ManagerScreenCapture._instance.SaveImageCapture( this.itemStamp, true, this.isSetupData, this.GetStampScreenShotHandler );*/
    }

    public void GetStampScreenShotHandler ( byte[] byteTexture )
    {
        /*// 수정 확인 팝업
        UIPopupSystem popup = ManagerUI._instance.OpenPopupSystem(false).GetComponent<UIPopupSystem>();
        popup.InitSystemPopUp( Global._instance.GetString( "p_t_4" ), Global._instance.GetString( "n_st_5" ), false );

        GameObject go = new GameObject( typeof( ManagerGallery ).Name );
        ManagerGallery galleryManager = go.AddComponent<ManagerGallery>();

#if !UNITY_EDITOR
        string fileName = "MyStamp" +  System.DateTime.Now.ToString( "yyyy-MM-dd_HH-mm-ss" );
        galleryManager.SaveImage( byteTexture, fileName );
#endif
        var useReadyItem = new ServiceSDK.GrowthyCustomLog_ITEM
              (
                 ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.EDIT,
                  ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.STAMP,
                          "Stamp" + ServerRepos.UserStamps[_index].index,
                          "Stamp",
                         1,
                  ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EDIT_STAMP
              );
        var doc = JsonConvert.SerializeObject(useReadyItem);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", doc);
        */
    }

    public void UploadTimeLine ()
    {
        working = true;
     /*
        // 스크린 캡처 시작
        if ( ManagerScreenCapture._instance == null )
        {
            GameObject obj = Instantiate( UIDiaryStamp._instance._objScreenCaptrue );
            obj.name = "ManagerScreenCapture";
            obj.transform.position = new Vector3( 2f, -3f, 0f );
        }

        ManagerScreenCapture._instance.SaveImageCapture( this.itemStamp, false, this.isSetupData, this.UpdateTimeLine );*/
    }

    public void UpdateTimeLine ( byte[] byteTexture )
    {   /*
#if !UNITY_EDITOR
        string fileName = "MyStamp" + System.DateTime.Now.ToString( "yyyy-MM-dd_HH-mm-ss" );

        GameObject objTimeLine = new GameObject();
        SDKTimeLineManager timelineManager = objTimeLine.AddComponent<SDKTimeLineManager>();
        timelineManager.GetTImeLineAccess(byteTexture, fileName, this.UpdateTimeLineFinish);
#endif
*/
    }

   
}
