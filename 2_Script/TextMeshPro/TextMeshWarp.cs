using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TextMeshWarp : TextMeshPostProcessing
{
    private TMP_Text textComponent;
    private TMP_Text TextComponent
    {
        get
        {
            if(textComponent == null)
            {
                textComponent = gameObject.GetComponent<TMP_Text>();
            }

            return textComponent;
        }
    }

    [SerializeField] private AnimationCurve VertexCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f, 60.0f, 60.0f), 
        new Keyframe(0.5f, 15.0f), 
        new Keyframe(1.0f, 0.0f, -60.0f, -60.0f)
    );

    private void Awake()
    {
        UpdateText();
    }

    public override void UpdateText()
    {
        WarpText();
    }

    private void WarpText()
    {
        VertexCurve.preWrapMode = WrapMode.Clamp;
        VertexCurve.postWrapMode = WrapMode.Clamp;

        Vector3[] vertices;
        Matrix4x4 matrix;

        TextComponent.havePropertiesChanged = true;

        if (!TextComponent.havePropertiesChanged)
        {
            return;
        }

        TextComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = TextComponent.textInfo;
        int characterCount = textInfo.characterCount;


        if (characterCount == 0) return;

        float boundsMinX = TextComponent.bounds.min.x;
        float boundsMaxX = TextComponent.bounds.max.x;



        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            vertices = textInfo.meshInfo[materialIndex].vertices;

            Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);

            vertices[vertexIndex + 0] += -offsetToMidBaseline;
            vertices[vertexIndex + 1] += -offsetToMidBaseline;
            vertices[vertexIndex + 2] += -offsetToMidBaseline;
            vertices[vertexIndex + 3] += -offsetToMidBaseline;

            float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX);
            float x1 = x0 + 0.0001f;
            float y0 = VertexCurve.Evaluate(x0);
            float y1 = VertexCurve.Evaluate(x1);

            Vector3 horizontal = new Vector3(1, 0, 0);
            Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

            float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
            Vector3 cross = Vector3.Cross(horizontal, tangent);
            float angle = cross.z > 0 ? dot : 360 - dot;

            matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

            vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
            vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
            vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
            vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

            vertices[vertexIndex + 0] += offsetToMidBaseline;
            vertices[vertexIndex + 1] += offsetToMidBaseline;
            vertices[vertexIndex + 2] += offsetToMidBaseline;
            vertices[vertexIndex + 3] += offsetToMidBaseline;
        }


        TextComponent.UpdateVertexData();
    }
}
