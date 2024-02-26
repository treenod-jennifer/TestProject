using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraEffect : MonoBehaviour
{
    protected GameObject EffectRoot { get; private set; }
    protected UITexture TargetTexture { get; private set; }

    protected const float INTERVAL = 15.0f;

    //protected virtual void Start()
    //{
    //    TargetTexture = gameObject.GetComponent<UITexture>();
    
    //    EffectRoot = MakeEffect();
    //}

    public virtual void Make(GameObject effect)
    {
        TargetTexture = gameObject.GetComponent<UITexture>();

        EffectRoot = MakeEffect(effect);
    }

    protected virtual void OnDestroy()
    {
        if (EffectRoot != null) Destroy(EffectRoot);
    }

    private GameObject MakeEffect(GameObject effect)
    {
        if (TargetTexture == null) return null;

        Texture2D texture2D = TargetTexture.mainTexture as Texture2D;
        if (texture2D == null) return null;

        Sprite sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), GetPivot(TargetTexture.pivot), 1);

        var effectObj = new GameObject();
        effectObj.transform.parent = transform;
        effectObj.transform.localPosition = Vector3.zero;
        effectObj.transform.localEulerAngles = Vector3.zero;
        effectObj.transform.localScale = new Vector3((float)TargetTexture.width / texture2D.width, (float)TargetTexture.height / texture2D.height, 1.0f);

        SpriteRenderer spriteRenderer = effectObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.enabled = false;

        PolygonCollider2D polygonCollider = effectObj.AddComponent<PolygonCollider2D>();

        int pathCount = polygonCollider.pathCount;

        for (int i = 0; i < pathCount; i++)
        {
            PolygonLine line = new PolygonLine();
            line.AddPoint(polygonCollider.GetPath(i));
            line.MakeCurve();

            float distance = line.GetDistance();

            for (float j = 0.0f; j <= distance; j += INTERVAL)
            {
                GameObject obj = Instantiate(effect, effectObj.transform);
                obj.hideFlags = HideFlags.HideInHierarchy;
                obj.transform.localPosition = line.Lerp(j / distance);
            }
        }

        polygonCollider.enabled = false;

        return effectObj;
    }

    protected Vector2 GetPivot(UIWidget.Pivot pivot)
    {
        switch (pivot)
        {
            case UIWidget.Pivot.TopLeft:    return new Vector2(0.0f, 1.0f);
            case UIWidget.Pivot.Top:        return new Vector2(0.5f, 1.0f);
            case UIWidget.Pivot.TopRight:   return new Vector2(1.0f, 1.0f);
            case UIWidget.Pivot.Left:       return new Vector2(0.0f, 0.5f);
            case UIWidget.Pivot.Center:     return new Vector2(0.5f, 0.5f);
            case UIWidget.Pivot.Right:      return new Vector2(1.0f, 0.5f);
            case UIWidget.Pivot.BottomLeft: return new Vector2(0.0f, 0.0f);
            case UIWidget.Pivot.Bottom:     return new Vector2(0.5f, 0.0f);
            case UIWidget.Pivot.BottomRight:return new Vector2(1.0f, 0.0f);
            default:                        return new Vector2(0.5f, 0.5f);
        }
    }

    private class PolygonLine
    {
        private struct PolygonPoint
        {
            public Vector2 point;
            public float distance;

            public PolygonPoint(Vector2 point)
            {
                this.point = point;
                distance = 0.0f;
            }

            public PolygonPoint(Vector2 point, PolygonPoint previousPoint)
            {
                this.point = point;
                distance = Vector2.Distance(point, previousPoint.point) + previousPoint.distance;
            }
        }

        private List<PolygonPoint> points = new List<PolygonPoint>();

        public void AddPoint(Vector2 point)
        {
            PolygonPoint pPoint;

            if (points.Count == 0)
            {
                pPoint = new PolygonPoint(point);
            }
            else
            {
                pPoint = new PolygonPoint(point, points[points.Count - 1]);
            }

            points.Add(pPoint);
        }

        public void AddPoint(Vector2[] points)
        {
            foreach(var point in points)
            {
                AddPoint(point);
            }

            AddPoint(points[0]);//시작점을 넣어서 원이 되도록 세팅
        }

        public float GetDistance()
        {
            if (points.Count == 0) return 0.0f;

            return points[points.Count - 1].distance;
        }

        private AnimationCurve positionX_Curve = new AnimationCurve();
        private AnimationCurve positionY_Curve = new AnimationCurve();

        public void MakeCurve()
        {
            foreach(var point in points)
            {
                positionX_Curve.AddKey(new Keyframe(point.distance, point.point.x));
                positionY_Curve.AddKey(new Keyframe(point.distance, point.point.y));
            }
        }

        public Vector2 Lerp(float time)
        {
            float distance = time * GetDistance();
            return new Vector2(positionX_Curve.Evaluate(distance), positionY_Curve.Evaluate(distance));
        }
    }
}
