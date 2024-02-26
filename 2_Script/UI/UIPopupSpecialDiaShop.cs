using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupSpecialDiaShop : UIPopupBase
{
    public static UIPopupSpecialDiaShop _instance = null;

    //오브젝트 링크
    [SerializeField] private UILabel labelSpecialDiaEventTs;

    //다이아 상품
    [SerializeField] private List<UIItemSpecialDiaShop> listSpecialDiaItem;

    //거래법 링크 관련
    [SerializeField] private GameObject objTermsOfUse;
    [SerializeField] private UILabel labelTermsOfUse;
    [SerializeField] private GameObject objPrecautions;
    [SerializeField] private UILabel labelPrecautions;
    
    //UI 조절
    [SerializeField] private GameObject prefabMessage;
    [SerializeField] private UIGrid     gridMessage;
    [SerializeField] private GameObject topRoot;
    [SerializeField] private GameObject bottomRoot;
    [SerializeField] private UISprite   spriteBGBlue;
    

    private string TermsOfUse
    {
        get
        {
            string value = Global._instance.GetString("p_dia_4");

            if (string.IsNullOrEmpty(value))
                objTermsOfUse.SetActive(false);
            else
                objTermsOfUse.SetActive(true);

            return value;
        }
    }

    private string Precautions
    {
        get
        {
            string value = Global._instance.GetString("p_dia_3");

            if (string.IsNullOrEmpty(value))
                objPrecautions.SetActive(false);
            else
                objPrecautions.SetActive(true);

            return value;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    void OnDestroy()
    {
        _instance = null;
        base.OnDestroy();
    }

    public void InitData(List<CdnMailShopGoods> mailShopGoods)
    {
        //메일 샵 아이템 개수로 UI 조절
        int defaultCount   = 3;
        int extensionCount = mailShopGoods.Count - defaultCount;
        
        if (extensionCount > 0)
        {
            //UI 크기, 위치 설정
            extensionCount = extensionCount > 2 ? 2 : extensionCount;
            int itemHeight = 147;

            int totalHeight = itemHeight * extensionCount;
            mainSprite.height   += totalHeight;
            spriteBGBlue.height += totalHeight;

            topRoot.transform.localPosition     = new Vector2(topRoot.transform.localPosition.x,topRoot.transform.localPosition.y         + totalHeight * 0.5f);
            bottomRoot.transform.localPosition  = new Vector2(bottomRoot.transform.localPosition.x,bottomRoot.transform.localPosition.y   - totalHeight * 0.5f);
            gridMessage.transform.localPosition = new Vector2(gridMessage.transform.localPosition.x,gridMessage.transform.localPosition.y + totalHeight * 0.5f);
            mainSprite.transform.localPosition  = new Vector2(mainSprite.transform.localPosition.x,mainSprite.transform.localPosition.y   - totalHeight * 0.125f);
            
            //메일 아이템 추가
            for (int i = 0; i < extensionCount; i++)
            {
                GameObject uiItem = Instantiate(prefabMessage, gridMessage.transform);
                uiItem.transform.localScale = Vector3.one;
                listSpecialDiaItem.Add(uiItem.GetComponent<UIItemSpecialDiaShop>());
            }
            gridMessage.Reposition();
        }
        
        //국가별 주의사항 및 사용조건에 키값이 없을 경우 해당 버튼 꺼짐.
        labelTermsOfUse.text = TermsOfUse;
        labelPrecautions.text = Precautions;

        for (int i = 0; i < listSpecialDiaItem.Count; i++)
        {
            listSpecialDiaItem[i].InitData(mailShopGoods[i]);
        }
        listSpecialDiaItem[listSpecialDiaItem.Count -1].CoverLastLine();

        StartCoroutine(CoSpecialDiaEventEndTs());
    }

    //이벤트 시간
    IEnumerator CoSpecialDiaEventEndTs()
    {
        while(true)
        {
            labelSpecialDiaEventTs.text = Global.GetTimeText_HHMMSS(ServerRepos.UserMailShop.endTs);

            if (Global.LeftTime(ServerRepos.UserMailShop.endTs) < 0) break;

            yield return new WaitForSeconds(1.0f);
        }

        labelSpecialDiaEventTs.text = "00: 00: 00";
    }

    /// <summary>
    /// 특정 상거래법 링크 클릭시 호출
    /// </summary>
    private void OnClickTermsOfUse()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGAPP_ebiz_rules", Global._instance.GetString("p_dia_4"));
    }

    /// <summary>
    /// 자금결제법 링크 클릭시 호출
    /// </summary>
    private void OnClickPrecautions()
    {
        ServiceSDK.ServiceSDKManager.instance.ShowBoard(Trident.LCNoticeServiceBoardCategory.LCNoticeBoardTerms, "LGPKV_PaymentAct", Global._instance.GetString("p_dia_3"));
    }
}