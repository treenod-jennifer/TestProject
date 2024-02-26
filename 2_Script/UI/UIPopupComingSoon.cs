using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupComingSoon : UIPopupBase
{
    public UILabel commingSoonMsg;
    public UILabel warningMsg;
    public UILabel flowerMsg;

    public UILabel[]    flowerCnt_Cur;
    public UILabel[]    flowerCnt_All;
    public UISprite     flowerImg;
    public UISprite     rewardBox;
    public UITexture    rewardImg;
    public UITexture    bgTexture;
    public UITexture    titleTexture;

    int flowerCount = 0;
    Vector3 flowerCnt_Cur_Position;
    [System.NonSerialized]
    int _step = 0; // 0이면 꽃, 1이면 파란꽃

    public void InitPopUp()
    {

        flowerCount = 0;
        _step = 1;
        for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
        {
            if (ManagerData._instance._stageData[i]._flowerLevel < 3)
            {
                _step = 0;
                break;
            }
        }
        
        for (int i = 0; i < ManagerData._instance._stageData.Count; i++)
        {
            if (_step == 0)
            {
                if (ManagerData._instance._stageData[i]._flowerLevel >= 3)
                    flowerCount++;
            }
            else if (_step == 1)
            {
                if (ManagerData._instance._stageData[i]._flowerLevel >= 4)
                    flowerCount++;
            }
        }

        bgTexture.mainTexture = Resources.Load("UI/comingsoon_boni_" + (_step + 1)) as Texture;
        warningMsg.text = Global._instance.GetString("p_com_3");

        for (int i = 0; i < flowerCnt_Cur.Length; i++)
            flowerCnt_Cur[i].text = flowerCount.ToString();
        for (int i = 0; i < flowerCnt_All.Length; i++)
            flowerCnt_All[i].text = "/" + ManagerData._instance.maxStageCount;

        flowerCnt_Cur_Position = flowerCnt_Cur[0].cachedTransform.localPosition;
        //flowerImg.cachedTransform.localPosition = flowerCnt_Cur[0].cachedTransform.localPosition + (flowerCnt_Cur[0].width + 2) * Vector3.left;
        
        rewardBox.width = flowerCnt_All[0].width + 210;
        float xPos = rewardBox.width - 370f;
        flowerImg.cachedTransform.localPosition = flowerCnt_Cur[0].cachedTransform.localPosition + ((rewardBox.width / 3) - 12) * Vector3.left;

        if (_step == 0)
        {
            flowerMsg.text = Global._instance.GetString("p_com_4");
            bgTexture.width = 467;
            bgTexture.height = 399;
            commingSoonMsg.text = Global._instance.GetString("p_com_1");
            titleTexture.mainTexture = Resources.Load("UI/comingsoon_text_1_Jp") as Texture;
            titleTexture.width = 719;
            titleTexture.height = 160;
            flowerImg.spriteName = "icon_flower_stroke_gray";
            flowerImg.MakePixelPerfect();
            rewardImg.mainTexture = Resources.Load("Message/giftbox1") as Texture;

        }
        else if (_step == 1)
        {
            flowerMsg.text = Global._instance.GetString("p_com_5");
            bgTexture.width = 501;
            bgTexture.height = 417;
            bgTexture.transform.localPosition = new Vector3(bgTexture.transform.localPosition.x, -141f, 0f);

            commingSoonMsg.text = Global._instance.GetString("p_com_2");
            titleTexture.mainTexture = Resources.Load("UI/comingsoon_text_2_Jp") as Texture;
            titleTexture.width = 695;
            titleTexture.height = 138;
            flowerImg.spriteName = "icon_blueflower_stroke_gray";
            flowerImg.MakePixelPerfect();
            rewardImg.mainTexture = Resources.Load("Message/giftbox2") as Texture;
        }

    }

    void Update()
    {
        flowerCnt_Cur[0].cachedTransform.localPosition = flowerCnt_Cur_Position + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 8f) * 2f);
        bgTexture.cachedTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 2f) * 3f);
    }

    private void OnClickBtnPlay()
    {
        ManagerUI._instance.OpenPopupStage();
    }
}
