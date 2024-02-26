using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_PaintPang : MonoBehaviour
{
    [SerializeField] private List<GameObject> listColorSplashEffectObj = new List<GameObject>();
    [SerializeField] private GameObject objEffectRoot = null;

    public void InitEffect(BlockColorType colorType)
    {
        switch(colorType)
        {
            case BlockColorType.A:
                listColorSplashEffectObj[0].SetActive(true);
                break;
            case BlockColorType.B:
                listColorSplashEffectObj[1].SetActive(true);
                break;
            case BlockColorType.C:
                listColorSplashEffectObj[2].SetActive(true);
                break;
            case BlockColorType.D:
                listColorSplashEffectObj[3].SetActive(true);
                break;
            case BlockColorType.E:
                listColorSplashEffectObj[4].SetActive(true);
                break;
        }
        objEffectRoot.SetActive(true);
    }
}
