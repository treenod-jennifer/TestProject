using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupNotice : UIPopupBase, IImageRequestable
{
    public static UIPopupNotice _instance = null;
    public UITexture noticeImg;
    public GameObject check;
    public UILabel textLoad;
    public UILabel textInfo;

    public GameObject _objUrlTexture;

    //임시.
   // Notice _data = null;
    bool bCheck = false;

    private void Awake()
    {
        _instance = this;
    }
    new void OnDestroy()
    {
        base.OnDestroy();
        _instance = null;
    }
    /*
    public void InitPopUp(Notice in_data)
    {
        _data = in_data;
        check.SetActive(bCheck);
        textInfo.text = Global._instance.GetString("p_nt_1");
        UIImageLoader.Instance.Load(Global.noticeDirectory, "Notice/", "n_" + _data.noticeIndex, this);

        //추가 이미지 세팅.
        SettingNoticeActionSprite();
    }
    public void InitPopUpInfo(Notice in_data)
    {
        _data = in_data;
        check.SetActive(bCheck);
        UIImageLoader.Instance.Load(Global.noticeDirectory, "Notice/", "info_" + _data.noticeIndex, this);

        check.transform.parent.gameObject.SetActive(false);
        textInfo.gameObject.SetActive(false);

        ManagerSound.AudioPlay(AudioLobby.Chat_Boni);
    }
    void OnClickBtnCheck()
    {
        if (bCheck == false)
            bCheck = true;
        else
            bCheck = false;
        check.SetActive(bCheck);

        if (bCheck)
            PlayerPrefs.SetString("notice_" + _data.noticeIndex, (Global.GetTime() + (60 * 60 * 24 * 3)).ToString());
        else
            PlayerPrefs.DeleteKey("notice_" + _data.noticeIndex);

    }
    void OnClickBtnUrl()
    {
        if (_data.url != null)
        {
            if (_data.url.Length > 0)
                Application.OpenURL(_data.url);
        }

    }*/
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
    
    private void SettingNoticeActionSprite()
    {/*
        int spriteCount = _data.noticeSprite.Count;
        if (spriteCount == 0)
            return;
        
        for (int i = 0; i < spriteCount; i++)
        {
            UIItemActionUrlTexture texture = NGUITools.AddChild(mainSprite.gameObject, _objUrlTexture).GetComponent<UIItemActionUrlTexture>();
            int depth = noticeImg.depth + 5;
            List<int> size = new List<int>();

            size.Add(_data.noticeSprite[i].GetWeight());
            size.Add(_data.noticeSprite[i].GetHeight());

            texture.InitTexture(_data.noticeSprite[i].intervals, _data.noticeSprite[i].GetPosition(), size, depth);
            
            texture.urlTexture.SettingCallBack(texture.SettingTexture);
            texture.urlTexture.Load(Global.noticeDirectory, "Notice/", _data.noticeSprite[i].filename);
        }*/
    }

    void Update()
    {
        textLoad.spacingX = (int)Mathf.PingPong(Time.time * 10f, 3);
    }
}
