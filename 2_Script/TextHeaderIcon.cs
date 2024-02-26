using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextHeaderIcon : MonoBehaviour {

    [SerializeField] UILabel targetLabel;
    [SerializeField] UISprite spr;

    public float OffsetValue = 0;

    // Use this for initialization
    void Start () {
        StartCoroutine(SetPos());
	}
    public IEnumerator SetPos()
    {
        yield return null;

        UIWidget.Pivot pivot = targetLabel.pivot;
        float xOffset = 0f;
        switch (pivot)
        {
            case UIWidget.Pivot.Center:
                xOffset = -1 * (targetLabel.printedSize.x / 2f + spr.width / 2f);

                break;
            case UIWidget.Pivot.Left:
            case UIWidget.Pivot.TopLeft:
            case UIWidget.Pivot.BottomLeft:
                xOffset = -1 * (spr.width / 2f);
                break;
            case UIWidget.Pivot.Right:
            case UIWidget.Pivot.TopRight:
            case UIWidget.Pivot.BottomRight:
                xOffset = 1 * (targetLabel.printedSize.x / 2f + spr.width / 2f);
                break;

        }
        spr.transform.localPosition = Vector3.right * (xOffset + OffsetValue);
        yield break;
    }
	
	// Update is called once per frame
	void Update () {

        
		
	}


}
