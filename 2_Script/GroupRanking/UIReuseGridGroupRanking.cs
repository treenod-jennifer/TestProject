using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIReuseGridGroupRanking : UIReuseGridBase
{
    private CancellationTokenSource _cts = new CancellationTokenSource();

    protected override void Awake() => onInitializeItem += OnInitializeItem;

    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
    
    public void InitReuseGrid(int startIndex = 0)
    {
        minIndex = (UIPopupGroupRanking.instance.GetGroupRankingDataCount() - 1) * -1;
        AsyncInitReuseGrid(_cts.Token, UIPopupGroupRanking.instance.GetGroupRankingDataCount(), startIndex).Forget();
    }

    protected override void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
    {
        if (realIndex < minIndex)
        {
            go.SetActive(false);
            return;
        }
        
        base.OnInitializeItem(go, wrapIndex, realIndex);
        var rankingCell = go.gameObject.GetComponent<UIItemGroupRanking>();
        rankingCell.UpdateData(UIPopupGroupRanking.instance.GetGroupRankingData(realIndex * -1));
    }
}
