using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupCoinShop : UIPopupBase
{
    public static UIPopupCoinShop _instance = null;

    public UILabel coinshopText;
 //   public UIItemCoinShop[] itemCoinShop;

    public UITexture boniImage;
    public UITexture[] coinIcon;

    private int[] coinPrice = new int[5] { 10, 50, 120, 250, 450 };

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public override void OpenPopUp(int _depth)
    {/*
        base.OpenPopUp(_depth);
        popupType = PopupType.coinShop;
        boniImage.mainTexture = Resources.Load("UI/shop_boni_coin") as Texture2D;
        for (int i = 0; i < 5; i++)
        {
            itemCoinShop[i].InitItemCoin(i, coinPrice[i]);
            string path = string.Format("UI/shop_icon_coin_0{0}", (i + 1));
            coinIcon[i].mainTexture = Resources.Load(path) as Texture2D;
        }*/
    }
}
