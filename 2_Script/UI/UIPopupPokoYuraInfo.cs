using System;
using PokoAddressable;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIPopupPokoYuraInfo : UIPopupBase
{
    [Serializable]
    public class PokoYuraLoadData
    {
        public UITexture texture;
        public AssetReference reference;

        public void Load()
        {
            texture.gameObject.AddressableAssetLoad<Texture>(reference, tex =>
            {
                texture.mainTexture = tex;
            });
        }
    }

    public UILabel[] title;
    public UILabel message;

    [SerializeField] private PokoYuraLoadData[] pokoyuras;

    public override void OpenPopUp(int _depth)
    {
        base.OpenPopUp(_depth);
        
        string titleText = Global._instance.GetString("p_u_p_7");
        title[0].text = titleText;
        title[1].text = titleText;
        message.text = Global._instance.GetString("p_u_p_8");

        foreach (var pokoyura in pokoyuras)
        {
            pokoyura.Load();
            
        }
    }
}
