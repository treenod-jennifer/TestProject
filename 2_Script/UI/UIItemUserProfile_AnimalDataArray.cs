using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemUserProfile_AnimalDataArray : MonoBehaviour
{
    [SerializeField] private UIItemUserProfile_AnimalData[] _arrayAnimalData;

    public void UpdataData(UserProfileData[] arrayProfileData)
    {
        for (int i = 0; i < _arrayAnimalData.Length; i++)
        {
            if (gameObject.activeInHierarchy == false)
                return;

            _arrayAnimalData[i].UpdataData(arrayProfileData[i]);
        }
    }
}
