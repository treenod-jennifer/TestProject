using UnityEngine;

[System.Serializable]
public class BezierCurve2DController
{
    [SerializeField] private BezierCurve topLeft;
    [SerializeField] private BezierCurve topRight;
    [SerializeField] private BezierCurve bottomLeft;
    [SerializeField] private BezierCurve bottomRight;

    private Vector2? point = null;

    private BezierCurve.BakeCurve bakeCurve;

    public float Distance { get { return bakeCurve.Distance ?? 0.0f; } }

    public void SetPoint(Vector2 point)
    {
        if (this.point == point) return;

        point.x = Mathf.Clamp(point.x, -1.0f, 1.0f);
        point.y = Mathf.Clamp(point.y, -1.0f, 1.0f);

        this.point = point;

        var tempCurve = new BezierCurve.BakeCurve(new BezierCurve.BakeCurve.BakeInfo(UncertainEvaluate));

        if (BezierCurve.BakeCurve.ResamplingCheck(tempCurve.Distance.Value))
        {
            bakeCurve = new BezierCurve.BakeCurve(new BezierCurve.BakeCurve.BakeInfo(tempCurve.Distance.Value, UncertainEvaluate));
        }
        else
        {
            bakeCurve = tempCurve;
        }
    }

    private Vector3 UncertainEvaluate(float time)
    {
        float x = Mathf.Lerp(0.0f, 1.0f, (point.Value.x + 1.0f) / 2.0f);
        float y = Mathf.Lerp(0.0f, 1.0f, (point.Value.y + 1.0f) / 2.0f);

        Vector3 topLeft = this.topLeft.Evaluate(time);
        Vector3 topRight = this.topRight.Evaluate(time);

        Vector3 topX = Vector3.Lerp(topLeft, topRight, x);


        Vector3 bottomLeft = this.bottomLeft.Evaluate(time);
        Vector3 bottomRight = this.bottomRight.Evaluate(time);

        Vector3 bottomX = Vector3.Lerp(bottomLeft, bottomRight, x);


        return Vector3.Lerp(bottomX, topX, y);
    }

    public Vector3 Evaluate(float time)
    {
        return bakeCurve.Evaluate(time);
    }

    public Vector3 Evaluate(Vector2 point, float time)
    {
        SetPoint(point);
        return Evaluate(time);
    }
}
