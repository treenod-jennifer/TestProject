using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemInstallableGuestList : MonoBehaviour
{
    public UIItemInstallableGuest[] itemGuests;

    public void UpdateGuestList(List<InstallabelGuestData> listAnimalIndex, bool isCanChangeCharacter)
    {
        for (int i = 0; i < itemGuests.Length; i++)
        {
            if (i < listAnimalIndex.Count)
            {
                itemGuests[i].UpdateItem(listAnimalIndex[i], isCanChangeCharacter);
                itemGuests[i].gameObject.SetActive(true);
            }
            else
            {
                itemGuests[i].gameObject.SetActive(false);
            }
        }
    }
}
