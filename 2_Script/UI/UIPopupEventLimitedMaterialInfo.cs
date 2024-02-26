using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class UIPopupEventLimitedMaterialInfo : UIPopupBase {

    static public UIPopupEventLimitedMaterialInfo _instance = null;
    
    [SerializeField] UIUrlTexture materialTex;

    [SerializeField] UILabel[] titleText;
    [SerializeField] UILabel materialInfoText;
    [SerializeField] UILabel descText;
    [SerializeField] UILabel endDateText;
    [SerializeField] UILabel leftTimeText;

    long expiredAt;
    int materialNo;

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

    private void OnDestroy()
    {
        base.OnDestroy();
        if (_instance == this)
            _instance = null;
    }

    public void SetData(long expTime, int materialNo )
    {
        this.expiredAt = expTime;
        this.materialNo = materialNo;

        foreach (var matMeta in ServerContents.MaterialMeta)
        {
            if (matMeta.Value.mat_id == materialNo)
            {
                var dateString = Global._instance.GetString("p_mt_tm_3");
                dateString = dateString.Replace("[n]", Global.GetTimeText_Abs(matMeta.Value.expireTs, true));
                this.endDateText.text = dateString;
                break;
            }
        }
    }

   
    // Use this for initialization
    void Start () {
        for(int i = 0; i < titleText.Length; ++i)
            this.titleText[i].text = Global._instance.GetString("p_mt_tm_1");
        this.descText.text = Global._instance.GetString("p_mt_tm_4");
        
        this.materialInfoText.text = Global._instance.GetString(string.Format("mt_" + materialNo));

        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "IconMaterial", $"mt_{materialNo}", OnLoadComplete);

        StartCoroutine(CoLeftTime());
    }

    public void OnLoadComplete(Texture2D result)
    {
        this.materialTex.mainTexture = result;
        this.materialTex.MakePixelPerfect();
    }
    public void OnLoadFailed() { }

    IEnumerator CoLeftTime()
    {
        while(true)
        {
            var leftTime = Global.LeftTime(this.expiredAt);
            if (leftTime <= 0)
            {
                leftTime = 0;
                leftTimeText.text = "00:00:00";
                yield break;
            }
            leftTimeText.text = Global.GetTimeText_HHMMSS(this.expiredAt);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
