using System.Collections;
using UnityEngine;

public class UIButtonExpansionBox : MonoBehaviour {
    private enum ExpansionState
    {
        Expansion,
        Reduction,
        Transforming
    }

    [Header("Animation Control")]
    public AnimationCurve aniControl;
    public float speed = 1.0f;

    [Header("Linked Object")]
    public UISprite frame;
    public UIPanel panel;
    public BoxCollider boxCollider;
    public Transform contentsField;
    public Transform triangle;
    private ExpansionState state = ExpansionState.Reduction;

    public void ButtonCall()
    {
        if (state == ExpansionState.Reduction)
            Expansion();
        else if (state == ExpansionState.Expansion)
            Reduction();
    }
    private void Expansion()
    {
        state = ExpansionState.Transforming;
        StartCoroutine(CoTransforming(110));
    }

    private void Reduction()
    {
        state = ExpansionState.Transforming;
        StartCoroutine(CoTransforming(-110));
    }

    private IEnumerator CoTransforming(int transforHeight)
    {
        float aniTime = 0.0f;
        float panelStartHeight = transform.localPosition.y;
        float contentsStartHeight = contentsField.localPosition.y;
        int defHeight = frame.height;

        while (aniTime < 1.0f)
        {
            aniTime += Time.unscaledDeltaTime * speed;

            int height = Mathf.RoundToInt(Mathf.Lerp(0.0f, transforHeight, aniControl.Evaluate(aniTime)));
            frame.height = defHeight + height;
            boxCollider.size = new Vector3(boxCollider.size.x, defHeight + height, boxCollider.size.z);

            panel.baseClipRegion = new Vector4(panel.baseClipRegion.x, panel.baseClipRegion.y, panel.baseClipRegion.z, defHeight + height);

            transform.localPosition = new Vector3(transform.localPosition.x, panelStartHeight + (height * 0.5f), transform.localPosition.z);

            contentsField.localPosition = new Vector3(contentsField.localPosition.x, contentsStartHeight + (height * 0.5f), contentsField.localPosition.z);

            if(transforHeight > 0)
                triangle.localEulerAngles = new Vector3(triangle.localEulerAngles.x, triangle.localEulerAngles.y, Mathf.Lerp(0.0f, 180.0f, aniTime));
            else
                triangle.localEulerAngles = new Vector3(triangle.localEulerAngles.x, triangle.localEulerAngles.y, Mathf.Lerp(180.0f, 0.0f, aniTime));

            yield return null;
        }

        if(transforHeight > 0)
            state = ExpansionState.Expansion;
        else
            state = ExpansionState.Reduction;
    }
}
