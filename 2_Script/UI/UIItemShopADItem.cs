using UnityEngine;

public class UIItemShopADItem : MonoBehaviour
{
    [SerializeField] private UISprite sprite_ADIcon;
    [SerializeField] private UIItemADButton btn_AD;
    [SerializeField] private GameObject rootDefaultButton;
    [SerializeField] private GameObject rootIconButton;

    [SerializeField] private UISprite sprite_purchaseItem;
    [SerializeField] private UIUrlTexture texture_purchaseItem;
    [SerializeField] private UILabel[] label_purchaseCount;
    [SerializeField] private UILabel label_singleItemText;
    [SerializeField] private UILabel label_singleItemShadow;
        
    public enum ADItemType
    {
        None = 0,
        Clover = 1,
        Wing = 2,
        Diamond = 3,
    }

    public void Init(ADItemType type)
    {
        label_singleItemText.text = Global._instance.GetString("p_ads_7");
        label_singleItemShadow.text = label_singleItemText.text;
        if (type == (int) ADItemType.None)
        {
            sprite_ADIcon.gameObject.SetActive(false);
        }
        else
        {
            sprite_ADIcon.gameObject.SetActive(true);
            switch (type)
            {
                case ADItemType.Clover:
                    btn_AD.SetADType(AdManager.AdType.AD_6);
                    sprite_ADIcon.spriteName = $"icon_ad_clover";
                    SetAdButton(AdManager.AdType.AD_6);
                    string reward = Global._instance.GetString("item_1");
                    label_singleItemText.text = Global._instance.GetString("p_ads_11").Replace("[1]", reward);
                    label_singleItemShadow.text = label_singleItemText.text;
                    break;
                case ADItemType.Wing:
                    btn_AD.SetADType(AdManager.AdType.AD_7);
                    sprite_ADIcon.spriteName = $"icon_ad_wings";
                    break;
                case ADItemType.Diamond:
                    btn_AD.SetADType(AdManager.AdType.AD_15);
                    sprite_ADIcon.spriteName = $"icon_ad_gacha";
                    break;
            }
        }
    }

    //ad button의 아이템을 설정
    private void SetAdButton(AdManager.AdType adType)
    {
        if (ServerContents.AdInfos[(int)adType].rewards.Length <= 0)
            return;
        if (ServerContents.AdInfos[(int)adType].rewards.Length == 1)
        {
            Reward reward = new Reward()
            {
                type = ServerContents.AdInfos[(int)adType].rewards[0].type,
                value = ServerContents.AdInfos[(int)adType].rewards[0].value
            };
            RewardHelper.SetRewardImage(reward, sprite_purchaseItem,
                texture_purchaseItem, label_purchaseCount, 1);

            rootIconButton.SetActive(true);
            rootDefaultButton.SetActive(false);
        }
    }
}
