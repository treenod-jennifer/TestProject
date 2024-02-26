using UnityEngine;

public class UIItemAntiqueStoreItemArray : MonoBehaviour
{
    [SerializeField] private UIItemAntiqueStoreItem[] _arrayAntiqueItem;
    
    public void UpdateData(HousingList[] listHousing)
    {
        for (int i = 0; i < _arrayAntiqueItem.Length; i++)
        {
            if (gameObject.activeInHierarchy == false)
                return;
            
            _arrayAntiqueItem[i].UpdateData(listHousing[i]);
        }
    }
}
