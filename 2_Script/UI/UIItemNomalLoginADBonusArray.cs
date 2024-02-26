using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemNomalLoginADBonusArray : MonoBehaviour
{
    [SerializeField] private List<UIItemNomalLoginADBonus> nLoginADBonus;
    public void InitData(List<LoginADBonusData> loginADBonusDatas)
    {
        for (int i = 0; i < loginADBonusDatas.Count; i++)
        {
            nLoginADBonus[i].InitData(loginADBonusDatas[i]);
        }
    }
}
