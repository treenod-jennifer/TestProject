using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupNoticeHelp : UIPopupBase, IImageRequestable
{
    public static UIPopupNoticeHelp _instance = null;

    public UITexture noticeImg;
    public UILabel textLoad;

    //임시.
    //Notice _data = null;

    private void Awake()
    {
        _instance = this;
    }

    new void OnDestroy()
    {
        
        base.OnDestroy();
        _instance = null;


        ManagerUI._instance.OpenPopupRequestReview();
    }
    /*
    public void InitPopUp(Notice in_data)
    {
        _data = in_data;
        UIImageLoader.Instance.Load(Global.gameImageDirectory, "Notice/", "info_" + _data.noticeIndex, this);

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
    */
    public void OnLoadComplete(ImageRequestableResult r)
    {
        noticeImg.mainTexture = r.texture;
        textLoad.gameObject.SetActive(false);
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

    void Update()
    {
        textLoad.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
    }
}
