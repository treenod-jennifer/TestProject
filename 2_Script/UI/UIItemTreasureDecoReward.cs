using UnityEngine;

public class UIItemTreasureDecoReward : MonoBehaviour
{
    [SerializeField] private int decoIndex;
    [SerializeField] private UILabel labelSign;
    [SerializeField] private GameObject getButton;
    [SerializeField] private GameObject checkObj;
    
    //Sprite
    [SerializeField] private UISprite spriteButton;
    [SerializeField] private UISprite spriteBubble;
    [SerializeField] private UISprite spriteBubbleTail;
    [SerializeField] private UISprite spriteSign;
    [SerializeField] private UISprite spriteMap;
    [SerializeField] private UISprite spriteCheck;

    public void InitItem()
    {
        var item = ManagerTreasureHunt.instance.DecoRewardList[decoIndex];
        var status = ManagerTreasureHunt.instance.decoStatusList[decoIndex];
        
        // 하우징 데이터가 비어있으면 팝업에서 보이지 않도록 제거
        if (item.reward == null)
            gameObject.SetActive(false);
        
        if (status == ManagerTreasureHunt.DecoStatusType.None)    // 보상 수령 불가능
        {
            getButton.SetActive(false);
            checkObj.SetActive(false);
        }
        else if (status == ManagerTreasureHunt.DecoStatusType.Get)   // 보상 수령 가능
        {
            getButton.SetActive(true);
            checkObj.SetActive(false);
        }
        else if (status == ManagerTreasureHunt.DecoStatusType.Complete)   // 보상 수령 완료
        {
            getButton.SetActive(false);
            checkObj.SetActive(true);
        }

        labelSign.text = Global._instance.GetString("p_ce_2").Replace("[n]", item.index.ToString());;
        SetSprite();
    }

    private void SetSprite()
    {
        INGUIAtlas uiAtlas = ManagerTreasureHunt.instance.treasureHuntPack.UIAtlas;
        
        spriteSign.atlas = uiAtlas;
        spriteMap.atlas = uiAtlas;
        spriteCheck.atlas = uiAtlas;
        spriteButton.atlas = uiAtlas;
        spriteBubble.atlas = uiAtlas;
        spriteBubbleTail.atlas = uiAtlas;
    }

    private void OnClickGetBtn()
    {
        if (UIPopupTreasureHunt._instance != null)
            UIPopupTreasureHunt._instance.GetDecoReward(decoIndex);
    }
}
