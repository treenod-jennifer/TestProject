using PokoAddressable;
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
    [SerializeField] private GameObject title_1;
    [SerializeField] private GameObject title_2;

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

        this.gameObject.AddressableAssetLoad<Texture>("local_ui/comingsoon_boni_" + (_step + 1),(x) => bgTexture.mainTexture = x);
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
            title_1.SetActive(true);
            flowerImg.spriteName = "icon_flower_stroke_gray";
            flowerImg.MakePixelPerfect();
            gameObject.AddressableAssetLoad<Texture2D>("local_message/giftbox1", (texture) => rewardImg.mainTexture = texture);

        }
        else if (_step == 1)
        {
            flowerMsg.text = Global._instance.GetString("p_com_5");
            bgTexture.width = 501;
            bgTexture.height = 417;
            bgTexture.transform.localPosition = new Vector3(bgTexture.transform.localPosition.x, -141f, 0f);

            commingSoonMsg.text = Global._instance.GetString("p_com_2");
            title_2.SetActive(true);
            flowerImg.spriteName = "icon_blueflower_stroke_gray";
            flowerImg.MakePixelPerfect();
            gameObject.AddressableAssetLoad<Texture2D>("local_message/giftbox2", (texture) => rewardImg.mainTexture = texture);
        }

    }

    void Update()
    {
        flowerCnt_Cur[0].cachedTransform.localPosition = flowerCnt_Cur_Position + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 8f) * 2f);
        bgTexture.cachedTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 2f) * 3f);
    }

    private void OnClickBtnPlay()
    {
        if(_step == 0)
            ManagerUI._instance.OpenPopupStage(UIReuseGrid_Stage.ScrollMode.FlowerFind);
        else if(_step == 1)
            ManagerUI._instance.OpenPopupStage(UIReuseGrid_Stage.ScrollMode.BlueFlowerFind);
        else
            ManagerUI._instance.OpenPopupStage();
    }
}
