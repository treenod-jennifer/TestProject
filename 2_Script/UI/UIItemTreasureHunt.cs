using UnityEngine;

public class UIItemTreasureHunt : MonoBehaviour
{
    [SerializeField] private int stageIndex;
    [SerializeField] private GenericReward stageRewardItem;
    
    //Sprite
    [SerializeField] private UISprite spriteMap;
    [SerializeField] private UISprite spriteStep;
    [SerializeField] private UISprite spriteCheck;
    
    public Transform pointPos;
    private ManagerTreasureHunt.StageStatusType stageStatus = 0;
    private Reward stageReward = new Reward();
    
    public void InitStage()
    {
        stageReward = ManagerTreasureHunt.instance.StageRewardList[stageIndex];
        stageStatus = ManagerTreasureHunt.instance.stageStatusList[stageIndex];
        if (stageReward == null || stageReward.value == 0 )    // 1. 보상 값이 비어있거나, 2. 보상 개수가 0개일 경우 보상 이미지 비활성화
            stageRewardItem.gameObject.SetActive(false);
        else
            stageRewardItem.SetReward(stageReward);
        SetSprite();
    }

    private void SetSprite()
    {
        INGUIAtlas uiAtlas = ManagerTreasureHunt.instance.treasureHuntPack.UIAtlas;
        
        spriteStep.atlas = uiAtlas;
        spriteMap.atlas = uiAtlas;
        spriteCheck.atlas = uiAtlas;
        spriteCheck.spriteName = "check_icon";
        spriteStep.spriteName = stageIndex <= ManagerTreasureHunt.instance.GetStageProgress() ? "step" : "step_off";

        if (stageStatus == ManagerTreasureHunt.StageStatusType.NotClear)
        {
            spriteMap.spriteName = "icon_off";
            spriteCheck.gameObject.SetActive(false);
        }
        else if (stageStatus == ManagerTreasureHunt.StageStatusType.Clear)
        {
            spriteMap.spriteName = "icon_off";
            spriteCheck.gameObject.SetActive(true);
        }
        else if (stageStatus == ManagerTreasureHunt.StageStatusType.Complete)
        {
            spriteMap.spriteName = "icon";
            spriteCheck.gameObject.SetActive(true);
        }
    }

    private void OnClick()
    {
        if (stageIndex > ManagerTreasureHunt.instance.GetStageProgress())
            return;
        
        UIPopupTreasureHunt._instance.MovePoint(stageIndex);
        // 메인 팝업에서는 0~11로 스테이지 인덱스를 관리하나, API 호출 및 Global.stageIndex에서는 1~12로 관리
        ManagerUI._instance.OpenPopupReadyTreasure(stageIndex + 1);
    }

    public int GetStageIndex()
    {
        return stageIndex;
    }
}
