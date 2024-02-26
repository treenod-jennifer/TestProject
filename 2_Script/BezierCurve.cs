using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierCurve : PropertyAttribute
{
    [SerializeField] private Vector3[] positions = new Vector3[0];
    [SerializeField] private BezierCurveCore[] curves = new BezierCurveCore[0];
    [SerializeField] private BakeCurve bakeCurve;

    public int PointLength { get { return positions.Length; } }
    public int CurveLength { get { return curves.Length; } }
    public float Distance { get { return bakeCurve.Distance ?? 0.0f; } }

    private void Bake()
    {
        if (curves.Length == 0) return;

        List<BakeCurve.BakeInfo> curvesInfo = new List<BakeCurve.BakeInfo>();

        for (int i=0; i<curves.Length; i++)
        {
            var curveInfo = new BakeCurve.BakeInfo(curves[i].Distance, curves[i].Evaluate);
            curvesInfo.Add(curveInfo);
        }

        bakeCurve = new BakeCurve(curvesInfo.ToArray());
    }

    public void AddPoint(Vector3 point, Vector3 startControl, Vector3 endControl)
    {
        Vector3[] vector3s = new Vector3[positions.Length + 1];

        if(vector3s.Length > 1)
        {
            var bezierCurveCore = new BezierCurveCore();
            bezierCurveCore.Start = positions[positions.Length - 1];
            bezierCurveCore.End = point;
            bezierCurveCore.LocalStartControl = startControl;
            bezierCurveCore.LocalEndControl = endControl;

            AddCurve(bezierCurveCore);
        }

        if(positions.Length > 0)
        {
            System.Array.Copy(positions, vector3s, positions.Length);
        }

        vector3s[vector3s.Length - 1] = point;

        positions = vector3s;

        Bake();
    }

    private void AddCurve(BezierCurveCore curve)
    {
        BezierCurveCore[] bezierCurveCores = bezierCurveCores = new BezierCurveCore[curves.Length + 1];

        if(curves.Length > 0)
        {
            System.Array.Copy(curves, bezierCurveCores, curves.Length);
        }

        bezierCurveCores[bezierCurveCores.Length - 1] = curve;

        curves = bezierCurveCores;
    }

    public void RemovePoint()
    {
        if (positions.Length == 0) return;

        Vector3[] vector3s = new Vector3[positions.Length - 1];

        if(positions.Length > 1)
        {
            System.Array.Copy(positions, vector3s, positions.Length - 1);
        }

        positions = vector3s;


        if (curves.Length == 0) return;

        BezierCurveCore[] bezierCurveCores = new BezierCurveCore[curves.Length - 1];

        if (curves.Length > 1)
        {
            System.Array.Copy(curves, bezierCurveCores, curves.Length - 1);
        }

        curves = bezierCurveCores;
    }

    public void MovePoint(int index, Vector3 point)
    {
        bool dirty = false;

        if (index < positions.Length)
        {
            positions[index] = point;
            dirty = true;
        }

        if (index - 1 >= 0)
        {
            curves[index - 1].End = point;
            dirty = true;
        }

        if (index < curves.Length)
        {
            curves[index].Start = point;
            dirty = true;
        }

        if (dirty)
        {
            Bake();
        }
    }

    public void MoveStartControl(int index, Vector3 startControl)
    {
        if (curves.Length <= index) return;

        curves[index].StartControl = startControl;
        Bake();
    }

    public void MoveEndControl(int index, Vector3 endControl)
    {
        if (curves.Length <= index) return;

        curves[index].EndControl = endControl;
        Bake();
    }

    public void MoveLocalStartControl(int index, Vector3 startControl)
    {
        if (curves.Length <= index) return;

        curves[index].LocalStartControl = startControl;
        Bake();
    }

    public void MoveLocalEndControl(int index, Vector3 endControl)
    {
        if (curves.Length <= index) return;

        curves[index].LocalEndControl = endControl;
        Bake();
    }

    public Vector3? GetPoint(int index)
    {
        if (positions.Length <= index) return null;

        return positions[index];
    }

    public Vector3? GetStartControl(int index)
    {
        if (curves.Length <= index) return null;

        return curves[index].StartControl;
    }

    public Vector3? GetEndControl(int index)
    {
        if (curves.Length <= index) return null;

        return curves[index].EndControl;
    }

    public Vector3? GetLocalStartControl(int index)
    {
        if (curves.Length <= index) return null;

        return curves[index].LocalStartControl;
    }

    public Vector3? GetLocalEndControl(int index)
    {
        if (curves.Length <= index) return null;

        return curves[index].LocalEndControl;
    }

    public Vector3 Evaluate(float time)
    {
        return bakeCurve.Evaluate(time);
    }
    

    [System.Serializable]
    private class BezierCurveCore
    {
        [SerializeField] private Vector3 start;
        [SerializeField] private Vector3 startControl;
        [SerializeField] private Vector3 end;
        [SerializeField] private Vector3 endControl;
        private float distance;
        private bool dirty = false;

        public Vector3 Start
        {
            get
            {
                return start;
            }
            set
            {
                Vector3 controlPos = LocalStartControl;
                start = value;
                LocalStartControl = controlPos;

                dirty = true;
            }
        }
        public Vector3 StartControl
        {
            get
            {
                return startControl;
            }
            set
            {
                startControl = value;
                dirty = true;
            }
        }
        public Vector3 LocalStartControl
        {
            get
            {
                return startControl - Start;
            }
            set
            {
                startControl = value + Start;
                dirty = true;
            }
        }
        public Vector3 End
        {
            get
            {
                return end;
            }
            set
            {
                Vector3 controlPos = LocalEndControl;
                end = value;
                LocalEndControl = controlPos;

                dirty = true;
            }
        }
        public Vector3 EndControl
        {
            get
            {
                return endControl;
            }
            set {
                endControl = value;
                dirty = true;
            }
        }
        public Vector3 LocalEndControl
        {
            get
            {
                return endControl - End;
            }
            set
            {
                endControl = value + End;
                dirty = true;
            }
        }
        public float Distance
        {
            get
            {
                if (dirty)
                {
                    float distance = 0.0f;
                    Vector3 pos = Evaluate(0.0f);

                    for (int i = 0; i < SAMPLING_LEVEL; i++)
                    {
                        Vector3 nextPos = Evaluate((float)(i + 1) / SAMPLING_LEVEL);

                        distance += Vector3.Distance(pos, nextPos);

                        pos = nextPos;
                    }

                    this.distance = distance;

                    dirty = false;
                }

                return this.distance;
            }
        }

        /// <summary>
        /// 하나의 곡선을 50등분해 거리를 계산한다.
        /// </summary>
        private const int SAMPLING_LEVEL = 50;

        public Vector3 Evaluate(float time)
        {
            return Calculate(Start, StartControl, End, EndControl, time);
        }

        private Vector3 Calculate(Vector3 start, Vector3 start_control, Vector3 end, Vector3 end_control, float time)
        {
            float t = Mathf.Clamp(time, 0.0f, 1.0f);
            float tt = t * t;
            float ttt = tt * t;

            float u = 1.0f - t;
            float uu = u * u;
            float uuu = uu * u;

            Vector3 p = uuu * start; //first term  
            p += 3 * uu * t * start_control; //second term  
            p += 3 * u * tt * end_control; //third term  
            p += ttt * end; //fourth term  

            return p;
        }
    }


    [System.Serializable]
    private struct BakePosition
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private float distance;

        public Vector3 Position { get { return position; } }
        public float Distance { get { return distance; } }

        public BakePosition(Vector3 position)
        {
            this.position = position;
            distance = 0.0f;
        }

        public BakePosition(Vector3 position, BakePosition previousPosition)
        {
            this.position = position;
            distance = Vector3.Distance(position, previousPosition.Position) + previousPosition.Distance;
        }
    }

    [System.Serializable]
    public class BakeCurve
    {
        public struct BakeInfo
        {
            public float? distance;
            public System.Func<float, Vector3> func;

            public BakeInfo(System.Func<float, Vector3> func)
            {
                this.distance = null;
                this.func = func;
            }

            public BakeInfo(float distance, System.Func<float, Vector3> func)
            {
                this.distance = distance;
                this.func = func;
            }
        }

        /// <summary>
        /// 샘플링 거리마다 bakePosition을 만들어 저장한다. (예 - 0.1인 경우 길이 1의 곡선을 표현하면 11개의 bakePosition을 생성한다)
        /// </summary>
        private const float SAMPLING_DISTANCE = 0.025f;

        /// <summary>
        /// 거리를 모를때 사용하는 값. 해당 값 만큼 샘플링을 한다.
        /// </summary>
        private const int DEFAULT_SAMPLING = 128;

        [SerializeField] private BakePosition[] bakePositions = new BakePosition[0];

        public float? Distance
        {
            get
            {
                if (bakePositions == null || bakePositions.Length == 0) return null;

                return bakePositions[bakePositions.Length - 1].Distance;
            }
        }

        public BakeCurve(params BakeInfo[] curves)
        {
            if (curves.Length == 0) return;

            List<BakePosition> vector3s = new List<BakePosition>();

            vector3s.Add(new BakePosition(curves[0].func(0.0f)));

            for (int i = 0; i < curves.Length; i++)
            {
                int samplingCount = 
                    (curves[i].distance == null) ? 
                    DEFAULT_SAMPLING : 
                    Mathf.FloorToInt(curves[i].distance.Value / SAMPLING_DISTANCE);

                for (int j = 0; j < samplingCount; j++)
                {
                    vector3s.Add
                    (
                        new BakePosition
                        (
                            curves[i].func((float)(j + 1) / samplingCount),
                            vector3s[vector3s.Count - 1]
                        )
                    );
                }
            }

            bakePositions = vector3s.ToArray();
        }

        public Vector3 Evaluate(float time)
        {
            float totalDistance = bakePositions[bakePositions.Length - 1].Distance;
            float currentDistance = Mathf.Lerp(0.0f, totalDistance, time);

            int selectIndex = 1;
            float localTime = 0.0f;

            for (int i = 1; i < bakePositions.Length; i++)
            {
                if (currentDistance <= bakePositions[i].Distance)
                {
                    selectIndex = i;

                    float localDistance = currentDistance - bakePositions[selectIndex - 1].Distance;

                    localTime = localDistance / (bakePositions[selectIndex].Distance - bakePositions[selectIndex - 1].Distance);

                    break;
                }
            }

            return Vector3.Lerp(bakePositions[selectIndex - 1].Position, bakePositions[selectIndex].Position, localTime);
        }

        public static bool ResamplingCheck(float distance)
        {
            int samplingLevel = Mathf.FloorToInt(distance / SAMPLING_DISTANCE);

            return samplingLevel > DEFAULT_SAMPLING;
        }
    }
}
