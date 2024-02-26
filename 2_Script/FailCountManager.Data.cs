using System.Collections.Generic;
using UnityEngine;

public partial class FailCountManager : MonoBehaviour
{
    private FailCountData failData_NPUAD;
    private FailCountData failData_NPUSpot;
    
    /// <summary>
    /// 스팟 다이아 패키지 Fail 데이터
    /// </summary>
    [System.Serializable]
    private class FailCountData
    {
        [SerializeField]
        private List<string> failData;
        private const int MAX_DATA_SIZE = 300;

        public FailCountData() => failData = new List<string>(MAX_DATA_SIZE);

        public void Add(string stage)
        {
            failData.Add(stage);
            
            if (failData.Count == MAX_DATA_SIZE)
            {
                failData.RemoveAt(0);
            }
            else
            {
                if (failData.Count > MAX_DATA_SIZE)
                {
                    Debug.LogWarning("Overflow");

                    var overCount = failData.Count - MAX_DATA_SIZE;
                    failData.RemoveRange(0, overCount);
                }
            }
        }
        
        public int ContainsStage(string stage)
        {
            var count = 0;
            for (var i = 0; i < failData.Count; i++)
            {
                if (failData[i] == stage)
                {
                    count++;
                }
            }
            return count;
        }
        
        public void Clear() => failData.Clear();
    }
}
