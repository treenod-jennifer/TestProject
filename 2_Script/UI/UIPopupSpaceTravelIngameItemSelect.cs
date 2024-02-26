using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine.Unity;

public class UIPopupSpaceTravelIngameItemSelect : UIPopupBase
{
    //캐릭터 스파인
    private SkeletonAnimation _spineItemSelect = null;

    //스파인 위에 올라갈 패널
    [SerializeField] private UIPanel _panelUpperSpine;

    //보너스 아이템
    [SerializeField] private List<UIItemSpaceTravelSelectItem> _selectItems;

    //아틀라스 설정용 스프라이트 리스트
    [SerializeField] private List<UISprite> _spriteList;

    //아이템 선택했을 때 실행될 콜백함수
    private System.Action<ManagerSpaceTravel.BonusItemType> _selectAction = null;
    
    private void OnDestroy()
    {
        ManagerSpaceTravel.instance.UnLoadSpaceTravelResource(ManagerSpaceTravel.PrefabType.UI);
        foreach (var spr in _spriteList)
            spr.atlas = null;
        Destroy(_spineItemSelect);
        _spineItemSelect = null;
        _cts.Cancel();
        _cts.Dispose();

        base.OnDestroy();
    }
    
    public override void SettingSortOrder(int layer)
    {
        _panelUpperSpine.depth = uiPanel.depth + 1;
        
        if (layer < 10)
        {
            return;
        }

        uiPanel.useSortingOrder = true;
        uiPanel.sortingOrder = layer;
        
        _spineItemSelect.GetComponent<MeshRenderer>().sortingOrder = layer + 1;
        
        _panelUpperSpine.useSortingOrder = true;
        _panelUpperSpine.sortingOrder = layer + 2;

        ManagerUI._instance.TopUIPanelSortOrder(this);
    }

    public override void OpenPopUp(int depth)
    {
        base.OpenPopUp(depth);
        _callbackOpen += () => AsyncActionOpenPopup().Forget();
        InitSpineObject();
        foreach (var item in _spriteList)
        {
            item.atlas = ManagerSpaceTravel.instance._spaceTravelPackIngame.Atlas;
        }
    }

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private async UniTask AsyncActionOpenPopup()
    {
        //오비스 사운드
        ManagerSound.AudioPlay(AudioLobby.Npc_mission_BGM_end);

        //사운드 나오고 연출 나오는 동안 잠시 대기
        await UniTask.WaitForSeconds(0.5f, cancellationToken:_cts.Token);

        //인게임 아이템들 터치 활성화
        foreach (var item in _selectItems)
        {
            item.SetIsCanTouchItem(true);
        }
    }

    private void InitSpineObject()
    {
        if (_spineItemSelect == null)
        {
            _spineItemSelect = Instantiate(ManagerSpaceTravel.instance._spaceTravelPackIngame.SelectSpineObj, mainSprite.transform).GetComponent<SkeletonAnimation>();
        }

        _spineItemSelect.AnimationName           = "appear";
        _spineItemSelect.transform.localScale    = Vector3.one * 100f;
        _spineItemSelect.transform.localPosition = new Vector2(15, -240f);
        _spineItemSelect.AnimationState.Complete += delegate
        {
            _spineItemSelect.loop          = true;
            _spineItemSelect.AnimationName = "idle";
        };
    }
    
    public void InitPopup(System.Action<ManagerSpaceTravel.BonusItemType> action)
    {
        foreach (var item in _selectItems)
        {
            item.InitItem(SelectBonusItem);
        }

        _selectAction = action;
    }

    private void SelectBonusItem(ManagerSpaceTravel.BonusItemType type)
    {
        ManagerSpaceTravel.instance.AddSelectItemDictionary(type);
        
        // 아이템 선택 관련 그로시 로그 전송
        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
        (
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.EVENT_MODE,
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.SPACE_TRAVEL_SELECT,
            string.Format(type.ToString()),
            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS, 
            ManagerSpaceTravel.instance.EventIndex, 
            Global.stageIndex.ToString()
        );
        var d = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", d);
        
        //인게임 아이템들 터치 비활성화
        foreach (var item in _selectItems)
        {
            item.SetIsCanTouchItem(false);
        }

        //아이템 선택 콜백 실행
        _selectAction?.Invoke(type);
        OnClickBtnClose();
    }

    /// <summary>
    /// 뒤로 가기 버튼으로 창 닫히지 않도록 설정
    /// </summary>
    public override void OnClickBtnBack()
    {
        return;
    }
}
