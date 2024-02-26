using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupNoticeHelp : UIPopupBase
{
    public static UIPopupNoticeHelp _instance = null;

    public UITexture noticeImg;
    public UILabel textLoad;

    //임시.
    Notice _data = null;

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
        _instance = this;
    }

    new void OnDestroy()
    {
        
        base.OnDestroy();
        _instance = null;


        // ManagerUI._instance.OpenPopupRequestReview();
    }

    public void InitPopUp(Notice in_data)
    {
        _data = in_data;
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "CachedResource/", $"info_{_data.noticeIndex}.png", OnLoadComplete);

        ManagerSound.AudioPlay(AudioLobby.m_bird_oho);
    }

    void OnClickBtnUrl()
    {
        if (_data.url != null)
        {
            if (_data.url.Length > 0)
                Application.OpenURL(_data.url);
        }

    }

    public void OnLoadComplete(Texture2D r)
    {
        noticeImg.mainTexture = r;
        textLoad.gameObject.SetActive(false);
    }

    void Update()
    {
        textLoad.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
    }
}
