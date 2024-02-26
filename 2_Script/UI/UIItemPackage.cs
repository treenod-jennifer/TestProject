using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UIItemPackage : MonoBehaviour {
    
    public string imgFilename;
    public UITexture texture;
    [SerializeField] private UISprite blind;
    [SerializeField] private Transform blind_Stamp;

    public UIPokoButton button;
    public UIPokoButton buttonOff;
    bool alreadyBought = false;

    [SerializeField] private GameObject[] diaObjects;

    PackageBuyRoutine buyRoutine = null;

    public bool textureLoadCompleted = false;
    CdnShopPackage data;

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

    public void Init(CdnShopPackage data, bool autoOpen = false)
    {
        this.data = data;
        Box.LoadCDN<Texture2D>(Global.gameImageDirectory, "Shop/", data.image, OnLoadComplete, true);
        buyRoutine = gameObject.AddComponent<PackageBuyRoutine>();
        buyRoutine.init(UIPopupCompletePackage.instance, data, autoOpen : autoOpen);

        for (int i = 0; i < diaObjects.Length; ++i)
        {
            diaObjects[i].SetActive(data.payType != 0);
        }

        if (data.payType == 0)
        {
            string baseDisplayPrice = LanguageUtility.GetPrices(data.prices);
            button.SetLabel(baseDisplayPrice);
            buttonOff.SetLabel(baseDisplayPrice);

#if !UNITY_EDITOR
            string displayPrice = string.Empty;

            if (ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic.ContainsKey(this.buyRoutine.productCode))
            {
                displayPrice = ServiceSDK.ServiceSDKManager.instance.billingProductInfoDic[this.buyRoutine.productCode].displayPrice;
                button.SetLabel(displayPrice);
                buttonOff.SetLabel(displayPrice);
            }
#endif
        }
        else
        {            
            string baseDisplayPrice = data.prices[0];
            button.SetLabel(baseDisplayPrice);
            button.ApplyLabelActionToFirstLabel(lbl => lbl.transform.localPosition = new Vector3(18, 3, 0));
            buttonOff.SetLabel( baseDisplayPrice);
            buttonOff.ApplyLabelActionToFirstLabel(lbl => lbl.transform.localPosition = new Vector3(18, 3, 0));
        }


        CheckValidationBuyButton();
    }



    public void OnClick()
    {
        buyRoutine.OnClickButton();

        //그로씨
        if(PackageBuyRoutine.packageSuggestedAtLogin)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
            (
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.PACKAGE,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.PACKAGE_SUGGEST,
                null,
                ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
            );
            var doc = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
        }
    }

    public void OnLoadComplete(Texture2D result)
    {
        textureLoadCompleted = true;
        texture.mainTexture = result;
        texture.width = result.width;
        texture.height= result.height;

        blind.width = texture.width;
        blind.height = texture.height;
        blind_Stamp.localPosition = Vector3.up * Mathf.RoundToInt(blind.height * 0.5f);
    }
    public void OnLoadFailed()
    {
        textureLoadCompleted = true;
    }

    public void CheckValidationBuyButton()
    {
        //이미 구매했는지 체크
        if (ServerRepos.UserShopPackages.Exists(x => x.idx == data.idx))
        {
            alreadyBought = true;
        }

        button.gameObject.SetActive(alreadyBought == false);
        buttonOff.gameObject.SetActive(alreadyBought == true);
        blind.gameObject.SetActive(alreadyBought == true);
        blind_Stamp.gameObject.SetActive(alreadyBought == true);
    }

    public bool isBuy()
    {
        return alreadyBought;
    }
}
