using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemLoginADBonusArray : MonoBehaviour
{
    [SerializeField] private UIItemNomalLoginADBonusArray nLoginADBonus;
    [SerializeField] private UIItemLastLoginADBonus lLoginADBonus;

    public void InitData(List<LoginADBonusData> loginADBonusDatas)
    {
        if(loginADBonusDatas.Count > 1)
        {
            nLoginADBonus.gameObject.SetActive(true);
            lLoginADBonus.gameObject.SetActive(false);

            nLoginADBonus.InitData(loginADBonusDatas);
        }
        else
        {
            nLoginADBonus.gameObject.SetActive(false);
            lLoginADBonus.gameObject.SetActive(true);

            lLoginADBonus.InitData(loginADBonusDatas[0]);
        }
    }
}