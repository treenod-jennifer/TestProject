using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIPopUpEventGuidance : UIPopupBase
{
    [SerializeField] UITexture backTexture;

    public void InitPopUp()
    {
        backTexture.mainTexture = ResourceBox.Make(gameObject).LoadResource<Texture2D>("UI/event_expose_notice_01");
    }
}
