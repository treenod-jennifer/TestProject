using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using DG.Tweening;
using Newtonsoft.Json;

public class UIItemDiaryCostume : MonoBehaviour
{
    public UIUrlTexture itemTexture;
    public GameObject newIcon;
    public UISprite backImage;
    public UILabel costumeName;

    //코스튬 구매했을 때 활성화되는 오브젝트.
    public GameObject   purchaseRoot;
    public UISprite     checkObj;
    public UITexture    selectBack;

    //코스튬 구매 안 했을 때 활성화되는 오브젝트.
    public GameObject defaultRoot;
    public UISprite priceIcon;
    public UILabel priceText;

    //코스튬 데이터.
    private CdnCostume costumeData;

    //코스튬 구매한 상태인지.
    private bool bPurchase = false;

    //아이템 선택 한 뒤 실행될 콜백(카테고리에서 이전에 선택돼있던 아이템 선택해제 해줌).
    public System.Action<int> selectObjectHandler;

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

    public void InitItemDiaryCostume(bool bSelect, bool bNew, CdnCostume data, System.Action<int> selectObject)
    {
        costumeData = data;
        selectObjectHandler = selectObject;
        SettingNewIcon(bNew);
        CheckCostumePurchase();
        SettingCostumeImage();
        SettingCostumeInfo();
        SettingPurchaseState();
        SettingSelectCostume(bSelect);
    }

    public bool CheckSelectIndex(int index)
    {
        //해당 아이템이 현재 선택된 아이템이라면, 체크 해제하고 true 반환.
        if (costumeData.costume_id == index)
        {
            SettingCheck(false);
            return true;
        }
        else
            return false;
    }

    private void SettingNewIcon(bool bNew)
    {
        newIcon.SetActive(bNew);
    }

    private void SettingCostumeImage()
    {
        var costumeId = costumeData.costume_id;
        // NOTE: 2019 만우절
        if ( ServerRepos.LoginCdn.aprilFool != 0)
        {
            costumeId = ServerRepos.LoginCdn.aprilFool;
        }

        string fileName = string.Format("{0}_{1}", costumeData.char_id, costumeId);
        itemTexture.LoadCDN(Global.gameImageDirectory, "Costume/", fileName);
        selectBack.mainTexture = Box.LoadResource<Texture2D>("UI/costume_select_back");
    }

    private void SettingCostumeInfo()
    {
        string cName = string.Format("cos_{0}_{1}", costumeData.char_id, costumeData.costume_id);

        // NOTE: 2019 만우절
        if (ServerRepos.LoginCdn.aprilFool != 0)
        {
            cName = string.Format("cos_{0}_{1}", costumeData.char_id, ServerRepos.LoginCdn.aprilFool);
        }
        costumeName.text = Global._instance.GetString(cName);

        //구매 가능한 상태이면 가격 설정.
        if (costumeData.costume_id != 0 && costumeData.get_type == 1 && bPurchase == false)
        {
            priceText.text = costumeData.price_coin.ToString();
            SettingPricePosition();
        }
    }

    private void SettingPricePosition()
    {
        int lineCount = (int)(costumeName.printedSize.y / costumeName.fontSize);
        if (lineCount <= 1)
        {
            defaultRoot.transform.localPosition = new Vector3(0f, -131f, 0f);
        }
        else
        {
            defaultRoot.transform.localPosition = new Vector3(0f, -142f, 0f);
        }
    }


    private void SettingCheck(bool bSelect)
    {
        if (bSelect == true)
        {
            checkObj.spriteName = "ready_button_02_on";
        }
        else
        {
            checkObj.spriteName = "ready_button_02_off";
        }
        selectBack.gameObject.SetActive(bSelect);
    }

    private void SettingPurchaseState()
    {
        // purchaseRoot : 구매했을 때 보이는 선택창 / defaultRoot : 구매하지 않았을 때 보이는 가격 등의 창.
        //구매 타입의 코스튬이 아닌 경우나 이미 구매한 상태의 아이템은 purchaseRoot 보이게 / defaultRoot 안보이게 설정.
        if (costumeData.get_type != 1 || costumeData.costume_id == 0 || bPurchase == true)
        {
            defaultRoot.SetActive(false);
            purchaseRoot.SetActive(true);
        }
        else
        {   //유저가 이미 보유한 아이템일 경우 purchaseRoot 안 보이게 / defaultRoot 보이게 설정.
            defaultRoot.SetActive(true);
            purchaseRoot.SetActive(false);
        }
    }

    private void SettingSelectCostume(bool bSelect)
    {   //유저가 현재 코스튬을 착용했는지 여부에 따라 체크설정.
        SettingCheck(bSelect);
    }

    private void OnClickBtnCostume()
    {
        if (UIPopupDiary._instance.bCanTouch == false)
            return;
        //구매 가능한 타입의 코스튬이고, 기본형이 아니면 아직 구매하지 않은 상태일 경우.
        if (costumeData.get_type == 1 && costumeData.costume_id > 0 && bPurchase == false)
        {
            ShowPurchasePopUp();
        }
        //이미 가지고 있는 타입의 코스튬일 경우 적용팝업.
        else
        {   
            ServerUserCostume costumeInfo = ServerRepos.UserCostumes.Find(x => x.is_equip == 1);
            if (costumeInfo != null)
            {
                if (costumeInfo.char_id == costumeData.char_id && costumeInfo.costume_id == costumeData.costume_id)
                    return;
            }
            else
            {
                if (costumeData.costume_id == 0)
                    return;
            }
            ShowApplyPopUp();
        }
        //터치막음.
        UIPopupDiary._instance.bCanTouch = false;
    }

    //코스튬 구매한 상태인지 검사하는 함수.
    private void CheckCostumePurchase()
    {
        //1번 타입이 아니면 검사할 필요없음.
        if (costumeData.get_type != 1)
            return;

        //유저 데이터에 현재 코스튬이 있으면 구매한 상태.
        for (int i = 0; i < ServerRepos.UserCostumes.Count; i++)
        {
            if (costumeData.IsBuy(ServerRepos.UserCostumes[i].char_id, ServerRepos.UserCostumes[i].costume_id))
            {
                bPurchase = true;
                return;
            }
        }
        bPurchase = false;
    }

    private void ShowPurchasePopUp()
    {
        //이 코스튬을 구매하겠습니까? 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "CheckCoin", gameObject, true);
        popupSystem.FunctionSetting(2, "DairybCanTouchOn", gameObject, true);
        popupSystem.FunctionSetting(3, "DairybCanTouchOn", gameObject, true);
        popupSystem.SetButtonText(1, Global._instance.GetString("btn_1"));
        string message = string.Format(Global._instance.GetString("n_b_7"));
        message = message.Replace("[n]", costumeData.price_coin.ToString());
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, true);
        popupSystem.SetImage((Texture2D)itemTexture.mainTexture);
    }

    private void ShowApplyPopUp()
    {
        //이 코스튬을 적용하겠습니까? 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        popupSystem.FunctionSetting(1, "OnCostumeSet", gameObject, true);
        popupSystem.FunctionSetting(2, "DairybCanTouchOn", gameObject, true);
        popupSystem.FunctionSetting(3, "DairybCanTouchOn", gameObject, true);
        popupSystem.SetButtonText(1, Global._instance.GetString("btn_1"));
        popupSystem.SetButtonText(2, Global._instance.GetString("btn_2"));
        string message = string.Format(Global._instance.GetString("n_s_17"));
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, true);
        popupSystem.SetImage((Texture2D)itemTexture.mainTexture);
    }

    private void CheckCoin()
    {
        if (Global.coin >= costumeData.price_coin)
        {
            ServerAPI.CostumeBuy(costumeData.idx, recvCostumeBuy);
        }
        else
        {
            ManagerUI._instance.LackCoinsPopUp();
            DairybCanTouchOn();
        }
    }

    void recvCostumeBuy(CostumeBuyResp resp)
    {
        if (resp.IsSuccess)
        {
            ShowPurchaseSucessPopUp();
            Global.coin = (int)(GameData.User.AllCoin);
            ManagerSound.AudioPlay(AudioLobby.UseClover);
            ManagerUI._instance.UpdateUI();
            StartCoroutine(CoChangeButton());

            bPurchase = true;
        }
    }

    private void ShowPurchaseSucessPopUp()
    {
        //구매성공! 팝업.
        UIPopupSystem popupSystem = ManagerUI._instance.OpenPopupSystem(true).GetComponent<UIPopupSystem>();
        popupSystem.SetButtonText(1, Global._instance.GetString("btn_1"));
        string message = string.Format(Global._instance.GetString("n_b_11"));
        popupSystem.InitSystemPopUp(Global._instance.GetString("p_t_4"), message, false);
        popupSystem.SetResourceImage("Message/ok");
        //터치 가능하도록 변경.
        DairybCanTouchOn();
    }

    public IEnumerator CoChangeButton(bool bClose = false, System.Action action = null)
    {
        checkObj.transform.localScale = Vector3.zero;

        defaultRoot.transform.DOScale(Vector3.zero, 0.1f);
        yield return new WaitForSeconds(0.1f);

        purchaseRoot.SetActive(true);
        defaultRoot.SetActive(false);
        checkObj.transform.DOScale(Vector3.one, 0.1f);
        yield return new WaitForSeconds(0.1f);

        if (action != null)
        {
            action();
        }
    }

    public void OnCostumeSet()
    {
        ServerAPI.CostumeSet(costumeData.char_id, costumeData.costume_id, recvCostumeSet);
    }

    void recvCostumeSet(CostumeSetResp resp)
    {
        if (resp.IsSuccess)
        {
            var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                (
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.COSTUME,
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.CHANGE_COSTUME,
                    string.Format("COSTUME_{0}", costumeData.idx),
                    ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                );
            var d = JsonConvert.SerializeObject(achieve);
            ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);

            SelectCostume();
            ManagerLobby._instance.SetCostume(costumeData.idx);
        }
    }

    private void SelectCostume()
    {
        if(purchaseRoot.activeInHierarchy == false)
            purchaseRoot.SetActive(true);
        if(defaultRoot.activeInHierarchy == true)
            defaultRoot.SetActive(false);

        SettingCheck(true);

        if (selectObjectHandler != null)
        {
            selectObjectHandler(costumeData.costume_id);
            ManagerSound.AudioPlay(AudioLobby.m_boni_yaho);
            ManagerUI._instance.ClosePopUpUI();

            var ch = ManagerLobby._instance.GetCharacter((TypeCharacterType)costumeData.char_id);
            if (ch != null)
            {
                CameraController._instance.MoveToPosition(ch.transform.position, 1f);
                LobbyEntryFocus.ResetFocusCandidates();
                StartCoroutine(UIDiaryController._instance.CoDiaryActionComplete(1f));
            }
        }
    }

    private void DairybCanTouchOn()
    {
        UIPopupDiary._instance.bCanTouch = true;
    }
}
