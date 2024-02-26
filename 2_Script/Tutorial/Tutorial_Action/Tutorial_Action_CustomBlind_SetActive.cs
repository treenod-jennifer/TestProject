
/// <summary>
/// 커스텀 블라인드 활성화 여부
/// </summary>
public class Tutorial_Action_CustomBlind_SetActive : Tutorial_Action
{
    public bool _isActive = true;

    public Tutorial_Action_CustomBlind_SetActive(bool active) => _isActive = active;

    public override void StartAction(System.Action endAction = null)
    {
        ManagerTutorial._instance._current.blind.SetActiveCustomBlind(_isActive);
        endAction.Invoke();
    }
}
