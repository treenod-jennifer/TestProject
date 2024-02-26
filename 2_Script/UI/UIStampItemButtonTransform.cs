using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStampItemButtonTransform : UIStampItemButton
{
    public struct LabelScaleData
    {
        public int width;
        public int height;
        public int fontSize;
    }

    public List<ObjectActionMouse> eventAction = new List<ObjectActionMouse>();
    public GameObject actionObj;
    public GameObject actionRotObj;

    //---------------------------------------------------------------------------
    protected override void InitEventData ()
    {
        this.selectTexture.gameObject.SetActive( false );
    }

    public override void StartButtonActionEvent ()
    {
        int length = eventAction.Count;
        for ( int i = 0; i < length; i++ )
        {
            this.eventAction[i].StartActionEvent( null, actionObj, UIStampItemButtonContainer.instance.deselectCollider );
        }
    }

    protected override void DestroyButtonActionEvent ()
    {
        int length = eventAction.Count;
        for ( int i = 0; i < length; i++ )
        {
            this.eventAction[i].StopActionEvent( false );
        }

    }

    //---------------------------------------------------------------------------
    public void InitializeData (GameObject actionObj, GameObject actionRotateObj, Stamp data)
    {
        this.actionObj = actionObj;
        this.actionRotObj = actionRotateObj;
        this.actionObj.transform.localEulerAngles = data.textLocalRotation;
        this.actionObj.GetComponent<UIWidget>().width = data.textWidgetWidth;
        this.actionObj.GetComponent<UIWidget>().height = data.textWidgetHeight;
        this.actionObj.GetComponent<UILabel>().fontSize = data.textSize;
        this.actionObj.transform.localPosition = data.textLocalPosition;
    }

    public Transform GetTransformData ()
    {
        return this.actionObj.transform;
    }

    public LabelScaleData GetLabelScaleData ()
    {
        LabelScaleData data = new LabelScaleData();
        data.width = this.actionObj.GetComponent<UIWidget>().width;
        data.height = this.actionObj.GetComponent<UIWidget>().height;
        data.fontSize = this.actionObj.GetComponent<UILabel>().fontSize;

        return data;
    }
}
