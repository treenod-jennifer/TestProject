using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopUpLandMove_Small : UIPopupLandMove
{
    [SerializeField] private Transform landItemRoot;

    public void SetLandItemPosition(int landCount)
    {
        Vector3 posLandRoot = landCount > 1 ? new Vector3(0, -90, 0) : Vector3.zero;

        landItemRoot.localPosition = posLandRoot;
    }
}
