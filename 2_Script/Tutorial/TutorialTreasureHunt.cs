using System.Collections.Generic;
using UnityEngine;

public class TutorialTreasureHunt : TutorialBase
{
    public void MoveCamera()
    {
        var focusPos = ManagerHousing.GetHousingFocusPosition(56);
        CameraController._instance.MoveToPosition(focusPos, 0.25f);
        CameraController._instance.SetFieldOfView(30f);
    }

    public List<GameObject> GetFingerObject()
    {
        return new List<GameObject>(){Global._instance.gameObject};
    }
}
