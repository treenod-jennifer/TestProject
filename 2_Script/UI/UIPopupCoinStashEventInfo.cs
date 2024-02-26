using PokoAddressable;
using UnityEngine;

public class UIPopupCoinStashEventInfo : UIPopupBase
{
    [SerializeField] UIUrlTexture textureImage0;
    [SerializeField] UIUrlTexture textureImage1;

    [SerializeField] UILabel labelEventInfo;

    private void Start()
    {
        InitTexture();
        InitText();
    }

    private void InitTexture()
    {
        gameObject.AddressableAssetLoad<Texture2D>("local_ui/coinstash_bg3", (texture) =>
        {
            textureImage0.mainTexture = texture;
            textureImage0.MakePixelPerfect();
        });
        
        gameObject.AddressableAssetLoad<Texture2D>("local_ui/coinstash_bg2", (texture) =>
        {
            textureImage1.mainTexture = texture;
            textureImage1.height = 289;
            textureImage1.transform.localScale = new Vector2(0.35f, 0.35f);
        });
    }

    private void InitText()
    {
        labelEventInfo.text = Global._instance.GetString("gp_met_4")
            .Replace("[n]", ServerContents.CoinStashEvent.coinMin.ToString())
            .Replace("[m]", ServerContents.CoinStashEvent.coinMax.ToString());
    }
}