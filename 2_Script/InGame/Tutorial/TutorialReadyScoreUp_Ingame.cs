using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialReadyScoreUp_Ingame : TutorialBase
{
    public GameObject GetObjScoreGauge()
    {
        return GameUIManager.instance.gaugeRoot;
    }
    public GameObject GetObjScoreUpItem()
    {
        return GameUIManager.instance.scoreUpRoot;
    }
}
