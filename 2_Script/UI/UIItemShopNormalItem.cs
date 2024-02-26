using UnityEngine;

public class UIItemShopNormalItem : MonoBehaviour
{
    [SerializeField] private GameObject obj_NormalItem;
    [SerializeField] private GameObject obj_SpecialItem;
    [SerializeField] private GameObject obj_AdvertiseItem;

    private int _index = 0;
    
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

    public void Init(int index = 0)
    {
        _index = index;
        obj_NormalItem.SetActive(false);
        obj_SpecialItem.SetActive(false);
        obj_AdvertiseItem.SetActive(false);
        if (index == 0)
            obj_NormalItem.SetActive(true);
        else if (index == 1)
            obj_SpecialItem.SetActive(true);
        else
            obj_AdvertiseItem.SetActive(true);
            
    }

    private void OnClickPurchaseButton()
    {
        if (_index == 2)
        {
            // 광고 시청 프로세스
        }
        else
        {
            // 스페셜 상품 및 일반 상품 구매 프로세스
        }
    }
}
