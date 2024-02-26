using UnityEngine;

public class GroupRankingAreaBase : AreaBase, IEventLobbyObject
{
    public static GroupRankingAreaBase instance = null;

    public GameObject cameraPositionObject;

    public override bool IsEventArea() => true;

    public GameEventType GetEventType() => GameEventType.GROUP_RANKING;

    private void Awake()
    {
        instance = this;

        InitSceneDatas();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    #region 트리거 체크
    public override void TriggerStart()
    {
        ManagerSound._instance.SetTimeBGM(sceneStartBgmOffset);

        if (sceneStartBgmOff)
        {
            ManagerSound._instance?.PauseBGM();
        }

        TriggerStart_Internal();
    }

    public void TriggerSetting() => _listSceneDatas[0].state = ManagerGroupRanking.CheckPlayStartCutScene() ? TypeSceneState.Wait : TypeSceneState.Active;
    #endregion

    #region 구조상 필요하나 사용하지 않는 코드
    public void OnActionUIActive(bool inActive)
    {
    }

    public void Invalidate()
    {
    }
    #endregion
}